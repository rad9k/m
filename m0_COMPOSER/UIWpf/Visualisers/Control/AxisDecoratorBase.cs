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
        protected bool PositionMarkEnabled = false;

        double positionMark;
        public double PositionMark {
            get {
                return positionMark;
            }
            set {
                positionMark = value;

                if(PositionMarkEnabled)
                    UpdatePositionMark();
            }
        }

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
            if (!PositionMarkEnabled)
                return;

            PositionMarkLine = WpfUtil.DrawLine(this, PositionMark, 0, PositionMark, Height, 3, (Brush)WpfUtil.FindResource("0HardHighlightBrush"));
        }

        public void UpdatePositionMark()
        {
            if (PositionMarkEnabled)
                WpfUtil.SetLinePosition(PositionMarkLine, PositionMark, 0, PositionMark, Height);
        }

        public event EventHandler PositionMarkChanged;

        public AxisDecoratorBase()
        {

        }

        protected void MouseDownHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!PositionMarkEnabled)
                return;

            Point p = e.GetPosition(this);

            PositionMark = p.X;

            if (PositionMarkChanged != null)
                PositionMarkChanged(sender, e);
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
