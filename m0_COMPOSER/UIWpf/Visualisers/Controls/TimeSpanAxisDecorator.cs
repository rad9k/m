using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using m0.Foundation;
using System.Windows.Controls;
using m0.Util;
using m0.Graph;
using System.Windows.Shapes;
using m0.UIWpf;
using System.Windows.Media;

namespace m0_COMPOSER.UIWpf.Visualisers.Controls
{
    class TimeSpanAxisDecorator : Canvas, IZoomScrollViewAxisDecorator
    {
        public Size Size { get; set; }
        public List<AxisSegment> Segments { get; set; }

        IVertex baseVertex;

        double zoomFactor;

        double FontSize = 12;

        double timeSpanHeight;

        private void Draw()
        {
            timeSpanHeight = FontSize * 2;

            Size s = new Size();
            s.Width = Length * baseUnitSize;
            s.Height = timeSpanHeight;

            Size = s;

            //

            Draw_Recurent(0);
            
        }

        private void Draw_Recurent(int level)
        {
            if (level == timeSpanLevels - 1)
                return;

            timeSpanLevel thisLevel = timeSpanStructure[level];

            for (int cnt = 0; cnt < Length; cnt += thisLevel.baseTimeSpanLevelCountForThisLevel)
            {
                Line l = new Line();

                double horizontalPosition = cnt * baseUnitSize;

                WpfUtil.SetLinePosition(l, horizontalPosition, 0, horizontalPosition, Size.Height);

                l.StrokeThickness = level * 1.5;

                l.Stroke = (Brush)WpfUtil.FindResource("0ForegroundBrush");

                Children.Add(l);

                AxisSegment segment = new AxisSegment();

                segment.lineStyle = new LineStyle();

                segment.StartPosition = cnt * baseUnitSize;
                segment.EndPosition = -1;
                
            }


            //Draw_Recurent(level + 1);
        }
        
        private int GetTimeSpanStructureDeepLevel_Reccurent(IVertex thisVertex, IVertex targetVertex, int deepLevel)
        {
            timeSpanLevel tsl = new timeSpanLevel();

            tsl.timeSpanLevelVertex = thisVertex;
            tsl.length = (int)GraphUtil.GetIntegerValue(thisVertex.Get(false, "Length:"));

            timeSpanStructure.Add(tsl);

            if (thisVertex == targetVertex)
                return deepLevel;

            return GetTimeSpanStructureDeepLevel_Reccurent(thisVertex.Get(false, @"SubLevel:"), targetVertex, deepLevel + 1);
        }

        class timeSpanLevel
        {
            public int baseTimeSpanLevelCountForThisLevel;
            public int length;
            public IVertex timeSpanLevelVertex;
        }

        List<timeSpanLevel> timeSpanStructure;

        int timeSpanLevels;

        double baseUnitSize;

        private void CreateTimeSpanStructure()
        {            
            timeSpanStructure = new List<timeSpanLevel>();

            IVertex r = m0.MinusZero.Instance.root;

            IVertex baseTimeSpanLevelVertex = r.Get(false, @"System\Lib\Music\Data\BaseTimeSpanLevel:");

            GetTimeSpanStructureDeepLevel_Reccurent(baseVertex, baseTimeSpanLevelVertex, 0);

            timeSpanLevels = timeSpanStructure.Count;

            int baseTimeSpanLevelCount = 1;

            for (int x = timeSpanStructure.Count - 1 ; x!=-1 ; x--)
            {
                baseTimeSpanLevelCount = baseTimeSpanLevelCount * timeSpanStructure[x].length;
                timeSpanStructure[x].baseTimeSpanLevelCountForThisLevel = baseTimeSpanLevelCount;
            }
            
        }

        private void Update()
        {
            CreateTimeSpanStructure();

            int baseUnit = timeSpanStructure[timeSpanLevels - 2].baseTimeSpanLevelCountForThisLevel;

            int nextUnitBaseCountMax = timeSpanStructure[timeSpanLevels - 3].length;

            Segments = new List<AxisSegment>();

            double baseUnitSize = 1.0 / 5 * zoomFactor / 50;

            int nextUnitBaseCount = 0;

            for (int cnt = 0; cnt < Length; cnt += baseUnit)
            {
                AxisSegment segment = new AxisSegment();

                segment.lineStyle = new LineStyle();

                segment.StartPosition = cnt * baseUnitSize;
                segment.EndPosition = -1;

                //

                if (nextUnitBaseCount == nextUnitBaseCountMax)
                {
                    segment.lineStyle.StrokeThickness = 3;

                    nextUnitBaseCount = 0;
                }
                
                nextUnitBaseCount++;

                Segments.Add(segment);
            }            

            Draw();
        }

        public void SetBaseVertex(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;

            Update();
        }

        public void SetZoomFactor(double _zoomFactor)
        {
            zoomFactor = _zoomFactor;

            baseUnitSize = 1.0 / 5 * zoomFactor / 50;

            Update();
        }

        double Length;

        public void SetLength(double length)
        {
            Length = length * 10;
        }

    }
}
