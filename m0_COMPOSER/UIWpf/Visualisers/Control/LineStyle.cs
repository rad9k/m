using m0.UIWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace m0_COMPOSER.UIWpf.Visualisers.Control
{
    public class LineStyle
    {
        public double StrokeThickness;
        public Brush Stroke;
        public DoubleCollection StrokeDashArray;

        public LineStyle()
        {
            StrokeThickness = 1;
            Stroke = (Brush)WpfUtil.FindResource("0ForegroundBrush");
            StrokeDashArray = null;
        }

        public LineStyle(double strokeThickness, Brush stroke, DoubleCollection strokeDashArray)
        {
            StrokeThickness = strokeThickness;
            Stroke = stroke;
            StrokeDashArray = strokeDashArray;
        }

        public void SetStyle(Line line)
        {
            line.StrokeThickness = StrokeThickness;
            line.Stroke = Stroke;
            line.StrokeDashArray = StrokeDashArray;
        }
    }
}
