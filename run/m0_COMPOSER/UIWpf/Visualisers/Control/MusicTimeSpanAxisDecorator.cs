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

namespace m0_COMPOSER.UIWpf.Visualisers.Control
{
    public class MusicTimeSpanAxisDecorator : AxisDecoratorBase, IZoomScrollViewAxisDecorator
    {
        public double ValueSpaceMin { get; set; }

        public double ValueSpaceMax { get; set; }

        public double ScreenToValueSpace(double screenPosition) { return 0; }

        public double ValueSpaceToScreen(double valueSpacePosition) { return 0; }

        public bool isHorizontal { get; set; }

        public MusicTimeSpanAxisDecorator(ZoomScrollViewBasedVisualiserBase _visualiser)
        {
            visualiser = _visualiser;
        }

        public int BoldLineCount;        

        double barLength;
        public double SegmentLength
        {
            get
            {
                return barLength;
            }
        }

        double FontSize = 10;

        double timeSpanHeight;

        class timeSpanLevel
        {
            public int BaseMusicTimeSpanLevelCountForThisLevel;
            public int length;
            public IVertex timeSpanLevelVertex;
        }

        List<timeSpanLevel> timeSpanStructure;

        int timeSpanLevels;                

        //        

        private void Draw()
        {
            timeSpanHeight = FontSize * 2;

            Size s = new Size();
            s.Width = ValueSpaceMax * baseUnitSize;
            s.Height = timeSpanHeight;

            Size = s;

            Width = Size.Width;
            Height = Size.Height;

            //

            Children.Clear();

            DrawBackground();

            Draw_Recurent(0);            

            CreateAndDrawPositionMark();
        }

        Brush getBrushForLevel(int level)
        {
            if (level == 0)
                return (Brush)WpfUtil.FindResource("0GrayBrush");

            return (Brush)WpfUtil.FindResource("0LightGrayBrush");
        }

        double getThicknessForLevel(int level)        
        {
            if (level == 0)
                return 3;

            return 1;
        }

        private void Draw_Recurent(int level)
        {
            if (level == timeSpanLevels - 1)
                return;

            timeSpanLevel thisLevel = timeSpanStructure[level];

            int textCount = 1;

            for (int cnt = 0; cnt < ValueSpaceMax; cnt += thisLevel.BaseMusicTimeSpanLevelCountForThisLevel)
            {                
                double horizontalPosition = cnt * baseUnitSize;

                double verticalStartPosition = ((double)level / (timeSpanLevels -1)) * Size.Height;

                //

                if (level == 0 || baseUnitSize > 0.15)
                {

                    TextBlock t = new TextBlock();

                    t.PreviewMouseDown += MouseDownHandler;

                    t.Foreground = getBrushForLevel(level);

                    t.Text = textCount.ToString();

                    t.FontSize = FontSize;

                    WpfUtil.SetPosition(t, horizontalPosition + 3, verticalStartPosition - 3);

                    Children.Add(t);

                    textCount++;

                    if (level > 0 && timeSpanStructure[level - 1].length + 1 == textCount)
                        textCount = 1;
                }

                //

                Line l = new Line();

                l.PreviewMouseDown += MouseDownHandler;

                WpfUtil.SetLinePosition(l, horizontalPosition, verticalStartPosition, horizontalPosition, Size.Height);

                l.StrokeThickness = getThicknessForLevel(level);

                l.Stroke = (Brush)WpfUtil.FindResource("0ForegroundBrush");

                Children.Add(l);                                
            }


            Draw_Recurent(level + 1);
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

        private void CreateTimeSpanStructure()
        {            
            timeSpanStructure = new List<timeSpanLevel>();

            IVertex r = m0.MinusZero.Instance.root;

            IVertex BaseMusicTimeSpanLevelVertex = r.Get(false, @"System\Lib\Music\Data\BaseMusicTimeSpanLevel:");

            GetTimeSpanStructureDeepLevel_Reccurent(baseVertex, BaseMusicTimeSpanLevelVertex, 0);

            timeSpanLevels = timeSpanStructure.Count;

            int BaseMusicTimeSpanLevelCount = 1;

            for (int x = timeSpanStructure.Count - 1 ; x!=-1 ; x--)
            {
                BaseMusicTimeSpanLevelCount = BaseMusicTimeSpanLevelCount * timeSpanStructure[x].length;
                timeSpanStructure[x].BaseMusicTimeSpanLevelCountForThisLevel = BaseMusicTimeSpanLevelCount;
            }

            barLength = timeSpanStructure[timeSpanStructure.Count - 2].length;
        }

        private void Update()
        {
            CreateTimeSpanStructure();

            int baseUnit = timeSpanStructure[timeSpanLevels - 2].BaseMusicTimeSpanLevelCountForThisLevel;

            int nextUnitBaseCountMax = timeSpanStructure[timeSpanLevels - 3].length;

            Segments = new List<AxisSegment>();            

            int nextUnitBaseCount = 0;

            for (int cnt = 0; cnt <= ValueSpaceMax; cnt += baseUnit)
            {
                AxisSegment segment = new AxisSegment();

                segment.LineStyle = new LineStyle();

                segment.StartPosition = cnt * baseUnitSize;
                segment.EndPosition = -1;

                //

                if (nextUnitBaseCount == 0)                
                    segment.LineStyle.StrokeThickness = 3;
                else
                {
                    if (BoldLineCount != 0)
                        if (nextUnitBaseCount % BoldLineCount != 0)
                            segment.LineStyle.Stroke = (Brush)WpfUtil.FindResource("0LightForegroundBrush");
                        else
                            segment.LineStyle.StrokeThickness = 2;
                }

                nextUnitBaseCount++;

                if (nextUnitBaseCount == nextUnitBaseCountMax)
                    nextUnitBaseCount = 0;

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


            if(zoomFactor > 50)
                //baseUnitSize = 0.02 + (1.0 / 5 * ((zoomFactor / 5) - 10));
                baseUnitSize = -1.73 + (1.0 / 5 * ((zoomFactor / 5) ));
            else
                baseUnitSize = 0.02 + (1.0 / 5 * zoomFactor / 40);

            Update();
        }

        public event EventHandler SelectionChanged;

        public object Selection { get; set; }

    }
}
