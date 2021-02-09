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
        double FontSize = 12;

        double SegmentHeight = 20;

        double segmentSize;        
       

        private void Draw()
        {
            Children.Clear();

            Width = 0;

            Height = 20;
            
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
            segment.EndPosition = SegmentHeight;
                        
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

        public void SetLength(double length)
        {

        }

        public OneSegmentAxisDecorator()
        {
            //this.MouseDown += TrackAxisDecorator_MouseDown;
        }

        private void TrackAxisDecorator_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            foreach (AxisSegment s in Segments)
                if (s.StartPosition <= p.Y && p.Y <= s.EndPosition)
                {
                    if (Selection == s)
                        Selection = null;
                    else
                        Selection = s;

                    if(SelectionChanged!=null)
                        SelectionChanged(sender, null);

                    Draw();
                }
        }

        public event EventHandler SelectionChanged;

        public object Selection { get; set; }
    }
}
