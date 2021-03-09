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

        double Height;

        public PrimLines(Canvas c, double position1, double position2, double height)
        {
            Height = height;

            if (position1 > position2)
            {
                LineBegPosition = position2;

                LineEndPosition = position1;
            }
            else
            {
                LineBegPosition = position1;

                LineEndPosition = position2;
            }

            LineBeg = WpfUtil.DrawLine(c, LineBegPosition, 0, LineBegPosition, height, 3, (Brush)WpfUtil.FindResource("0HardHighlightPrimBrush"));

            LineEnd = WpfUtil.DrawLine(c, LineEndPosition, 0, LineEndPosition, height, 3, (Brush)WpfUtil.FindResource("0HardHighlightPrimBrush"));

            Panel.SetZIndex(LineBeg, 1000);

            Panel.SetZIndex(LineEnd, 1000);
        }

        public void CopyFrom(PrimLines source)
        {
            LineBegPosition = source.LineBegPosition;
            LineEndPosition = source.LineEndPosition;

            SetLinePositions();
        }

        public void Update(double position)
        {
            if (position <= LineBegPosition)
                LineBegPosition = position;
            else
                LineEndPosition = position;

            SetLinePositions();
        }

        void SetLinePositions()
        {
            WpfUtil.SetLinePosition(LineBeg, LineBegPosition, 0, LineBegPosition, Height);

            WpfUtil.SetLinePosition(LineBeg, LineBegPosition, 0, LineBegPosition, Height);
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

        public static void UpdatePositionMark(Line l, double position, double height)
        {
            WpfUtil.SetLinePosition(l, position, 0, position, height);
        }
    }
}
