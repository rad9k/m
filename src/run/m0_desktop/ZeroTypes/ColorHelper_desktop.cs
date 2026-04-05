using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace m0.ZeroTypes
{
    public class ColorHelper_desktop
    {        
        public static Color GetColorFromColorVertex(IVertex colorVertex)
        {
            if (GraphUtil.GetIntegerValue(colorVertex.Get(false, "Red:")) == null ||
                GraphUtil.GetIntegerValue(colorVertex.Get(false, "Green:")) == null ||
                GraphUtil.GetIntegerValue(colorVertex.Get(false, "Blue:")) == null)

                return Colors.Black;

            if (colorVertex.Get(false, "Opacity:") == null)
                return Color.FromArgb(255, (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Red:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Green:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Blue:")));
            else
                return Color.FromArgb((byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Opacity:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Red:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Green:"))
                    , (byte)GraphUtil.GetIntegerValue(colorVertex.Get(false, "Blue:")));
        }
    }
}
