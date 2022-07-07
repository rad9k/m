using m0.Foundation;
using m0.UIWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public bool PositionMarkPrimEnabled { get; set; }

        Line PositionMarkLine;

        Line PositionMarkPrimLine_Beg;
        Line PositionMarkPrimLine_End;

        public void CreateAndDrawPositionMark()
        {
            visualiser.PositionMark = visualiser.PositionMark;

            PositionMarkLine = Common.CreatePositionMark(this, visualiser.PositionMark_Screen, Height);

            if (PositionMarkPrimEnabled)
            {
                PositionMarkPrimLine_Beg = Common.CreatePositionMarkPrim(this, visualiser.PositionMarkPrim_Beg_Screen, Height);
                PositionMarkPrimLine_End = Common.CreatePositionMarkPrim(this, visualiser.PositionMarkPrim_End_Screen, Height);
            }                
        }

        public event EventHandler PositionMarkChanged;

        public AxisDecoratorBase()
        {

        }

        protected void MouseDownHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)){
                if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
                    visualiser.PositionMarkPrim_End_Screen = p.X;
                else
                    visualiser.PositionMarkPrim_Beg_Screen = p.X;
            }
            else
                visualiser.PositionMark_Screen = p.X;                            

            PositionMarkUpdate();
        }

        public virtual void PositionMarkUpdate()
        {
            Common.UpdatePositionMark(PositionMarkLine, visualiser.PositionMark_Screen, Height);

            if (PositionMarkPrimEnabled)
            {
                Common.UpdatePositionMark(PositionMarkPrimLine_Beg, visualiser.PositionMarkPrim_Beg_Screen, Height);
                Common.UpdatePositionMark(PositionMarkPrimLine_End, visualiser.PositionMarkPrim_End_Screen, Height);
            }                        
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
