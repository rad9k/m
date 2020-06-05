using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace m0_COMPOSER.UIWpf.Visualisers.Controls
{
    public class LineStyle
    {
        public double StrokeThickness;
        public Brush Stroke;
        public DoubleCollection StrokeDashArray;

        public void SetStyle(Line line)
        {
            line.StrokeThickness = StrokeThickness;
            line.Stroke = Stroke;
            line.StrokeDashArray = StrokeDashArray;
        }
    }
}
