using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes
{
    public class ColorType
    {
        static IVertex vColor = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Color");
        static IVertex vRed = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Color\Red");
        static IVertex vGreen = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Color\Green");
        static IVertex vBlue = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Color\Blue");
        static IVertex vOpacity = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Color\Opacity");

        public static IVertex AddColor(IVertex baseVertex, string name, int red, int green, int blue, int opacity)
        {
            IVertex color = baseVertex.AddVertex(vColor, name);

            color.AddVertex(vRed, red);
            color.AddVertex(vGreen, green);
            color.AddVertex(vBlue, blue);
            color.AddVertex(vOpacity, opacity);

            return color;
        }
    }
}
