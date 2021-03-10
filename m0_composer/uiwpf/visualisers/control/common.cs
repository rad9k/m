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
    class Common
    {
        public static Line CreatePositionMark(Canvas c, double position, double height)
        {
            Line l = WpfUtil.DrawLine(c, position, 0, position, height, 3, (Brush)WpfUtil.FindResource("0HardHighlightBrush"));

            Panel.SetZIndex(l, 1001);

            return l;
        }

        public static Line CreatePositionMarkPrim(Canvas c, double position, double height)
        {
            Line l = WpfUtil.DrawLine(c, position, 0, position, height, 3, (Brush)WpfUtil.FindResource("0HardHighlightPrimBrush"));

            Panel.SetZIndex(l, 1000);

            return l;
        }

        public static void UpdatePositionMark(Line l, double position, double height)
        {
            WpfUtil.SetLinePosition(l, position, 0, position, height);
        }
    }
}
