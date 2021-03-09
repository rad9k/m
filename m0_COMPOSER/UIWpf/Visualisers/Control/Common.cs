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

        public double BegPosition;
        public double EndPosition;

        double Height;

        public PrimLines(Canvas c, double position1, double position2, double height)
        {
            Height = height;

            if (position1 > position2)
            {
                BegPosition = position2;

                EndPosition = position1;
            }
            else
            {
                BegPosition = position1;

                EndPosition = position2;
            }

            LineBeg = WpfUtil.DrawLine(c, BegPosition, 0, BegPosition, height, 3, (Brush)WpfUtil.FindResource("0HardHighlightPrimBrush"));

            LineEnd = WpfUtil.DrawLine(c, EndPosition, 0, EndPosition, height, 3, (Brush)WpfUtil.FindResource("0HardHighlightPrimBrush"));

            Panel.SetZIndex(LineBeg, 1000);

            Panel.SetZIndex(LineEnd, 1000);
        }

        public void CopyFrom(PrimLines source)
        {
            BegPosition = source.BegPosition;
            EndPosition = source.EndPosition;

            Update();
        }

        public void SetOneOfPositions(double position)
        {
            if (position <= BegPosition)
                BegPosition = position;
            else
                EndPosition = position;

            Update();
        }

        public void Update()
        {
            WpfUtil.SetLinePosition(LineBeg, BegPosition, 0, BegPosition, Height);

            WpfUtil.SetLinePosition(LineBeg, BegPosition, 0, BegPosition, Height);
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
