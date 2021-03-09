using m0.UIWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace m0_COMPOSER.UIWpf.Visualisers.Control
{

    class PrimLines
    {
        public Line LineBeg;
        public Line LineEnd;

        public double LineBegPosition;
        public double LineEndPosition;

        public PrimLines(Canvas c, double position1, double position2, double height)
        {         
            LineBeg = WpfUtil.DrawLine(c, position1, 0, position1, height, 3, (Brush)WpfUtil.FindResource("0HardHighlightPrimBrush"));

            LineEnd = WpfUtil.DrawLine(c, position2, 0, position2, height, 3, (Brush)WpfUtil.FindResource("0HardHighlightPrimBrush"));

            Panel.SetZIndex(LineBeg, 1000);

            Panel.SetZIndex(LineEnd, 1000);            
        }

        public static void UpdatePositionMarkPrim(PrimLines pl, double position, double height)
        {
            WpfUtil.SetLinePosition(l, position, 0, position, height);
        }

    }

    class Common
    {
        public static Line CreatePositionMark(Canvas c, double position, double height)
        {
            Line l = WpfUtil.DrawLine(c, position, 0, position, height, 3, (Brush)WpfUtil.FindResource("0HardHighlightBrush"));

            Panel.SetZIndex(l, 1000);

            return l;
        }        
    }
}
