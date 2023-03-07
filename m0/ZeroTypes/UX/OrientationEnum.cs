using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public enum OrientationEnum { Horizontal, Vertical }

    public class OrientationEnumHelper
    {
        static IVertex Horizontal_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\OrientationEnum\Horizontal");
        static IVertex Vertical_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\OrientationEnum\Vertical");
        

        public static OrientationEnum GetEnum(IVertex v)
        {
            if (v == null || v.Value == null)
                return OrientationEnum.Horizontal;

            switch (v.Value.ToString())
            {
                case "Horizontal": return OrientationEnum.Horizontal;

                case "Vertical": return OrientationEnum.Vertical;

                default: return OrientationEnum.Horizontal;
            }
        }

        public static IVertex GetVertex(OrientationEnum e)
        {
            switch(e){
                case OrientationEnum.Horizontal: return Horizontal_meta;

                case OrientationEnum.Vertical: return Vertical_meta;
            }

            return Horizontal_meta;
        }
    }

}
