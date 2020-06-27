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

namespace m0_COMPOSER.UIWpf.Visualisers.Controls
{
    class TimeSpanAxisDecorator : Canvas, IZoomScrollViewAxisDecorator
    {
        public Size Size { get; set; }
        public List<AxisSegment> Segments { get; set; }

        IVertex baseVertex;

        double zoomFactor;        
        
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

        private void CreateTimeSpanStructure()
        {            
            timeSpanStructure = new List<timeSpanLevel>();

            IVertex r = m0.MinusZero.Instance.root;

            IVertex baseTimeSpanLevelVertex = r.Get(false, @"System\Lib\Music\Data\BaseTimeSpanLevel:");

            GetTimeSpanStructureDeepLevel_Reccurent(baseVertex, baseTimeSpanLevelVertex, 0);

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

            Segments = new List<AxisSegment>();

            //int cnt = 0;

            double segmentSize = zoomFactor / 10;

            double maxWidth = 0;

            //foreach (IEdge e in baseVertex)
            for(int cnt=0; cnt < 100;cnt++)
            {
                AxisSegment segment = new AxisSegment();

                segment.lineStyle = new LineStyle();

                segment.StartPosition = cnt * segmentSize;
                segment.EndPosition = -1;

                if (maxWidth < segment.StartPosition)
                    maxWidth = segment.StartPosition;

                //segment.baseVertex = e.To;

                Segments.Add(segment);
            }

            Size s = new Size();
            s.Width = maxWidth;

            Size = s;
        }

        public void SetBaseVertex(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;

            Update();
        }

        public void SetZoomFactor(double _zoomFactor)
        {
            zoomFactor = _zoomFactor;

            Update();
        }

        public void SetLength(double length)
        {

        }

    }
}
