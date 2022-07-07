using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using m0;
using System.Windows.Shapes;
using m0.UIWpf;
using System.Windows.Media;
using m0.Graph;
using m0.UIWpf.Visualisers.Controls;

namespace m0_COMPOSER.UIWpf.Visualisers.Control
{
    class OneSegmentAxisDecorator : AxisDecoratorBase, IZoomScrollViewAxisDecorator
    {
        public double ValueSpaceMin { get; set; }

        public double ValueSpaceMax { get; set; }

        public double ScreenToValueSpace(double screenPosition) { return 0; }

        public double ValueSpaceToScreen(double valueSpacePosition) { return 0; }

        public bool isHorizontal { get; set; }

        double FontSize = 12;        

        double segmentSize;        
       

        private void Draw()
        {
            Children.Clear();

            Width = 0;

            Height = segmentSize;
            
            Size newSize = new Size();
            newSize.Width = Width;            

            newSize.Height = Height;

            Size = newSize;                                    
        }

        private void Update()
        {
            Segments = new List<AxisSegment>();
            
            AxisSegment segment = new AxisSegment();

            segment.LineStyle = new LineStyle();

            segment.StartPosition = 0;
            segment.EndPosition = segmentSize;
                        
            Segments.Add(segment);

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

            segmentSize = 3 +  (15 * (zoomFactor / 40) );

            Update();
        }

        public void SetValueSpaceMax(double length) { }

        public OneSegmentAxisDecorator() { }        

        public event EventHandler SelectionChanged;

        public object Selection { get; set; }
    }
}
