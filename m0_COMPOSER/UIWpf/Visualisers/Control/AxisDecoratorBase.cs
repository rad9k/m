using m0.Foundation;
using m0.UIWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace m0_COMPOSER.UIWpf.Visualisers.Control
{
    public class AxisDecoratorBase : Canvas
    {
        protected ZoomScrollViewBasedVisualiserBase visualiser;

        public Size Size { get; set; }

        public List<AxisSegment> Segments { get; set; }

        protected double baseUnitSize;

        public double BaseUnitSize
        {
            get
            {
                return baseUnitSize;
            }
        }

        protected double segmentLength;
        public double SegmentLength
        {
            get
            {                
                return segmentLength;
            }
        }

        protected IVertex baseVertex;

        protected double zoomFactor;

        Line PositionMarkLine;

        public void CreateAndDrawPositionMark()
        {
            PositionMarkLine = Common.CreatePositionMark(this, visualiser.PositionMark_Screen, Height);
        }

        public event EventHandler PositionMarkChanged;

        public AxisDecoratorBase()
        {

        }

        protected void MouseDownHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            visualiser.PositionMark_Screen = p.X;

            Common.UpdatePositionMark(PositionMarkLine, visualiser.PositionMark_Screen, Height);
        }

        protected void DrawBackground()
        {
            Border b = new Border();

            b.Background = (Brush)WpfUtil.FindResource("0BackgroundBrush");

            WpfUtil.SetPosition(b, 0, 0, Width, Height);

            this.Children.Add(b);

            b.PreviewMouseDown += MouseDownHandler;
        }

    }
}
