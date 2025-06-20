using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public enum LineEndEnum { Straight, Arrow, Triangle, FilledTriangle, Diamond, FilledDiamond }

    class LineEndEnumHelper
    {
        static IVertex Straight_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineEndEnum\Straight");
        static IVertex Arrow_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineEndEnum\Arrow");
        static IVertex Triangle_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineEndEnum\Triangle");
        static IVertex FilledTriangle_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineEndEnum\FilledTriangle");
        static IVertex Diamond_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineEndEnum\Diamond");
        static IVertex FilledDiamond_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LineEndEnum\FilledDiamond");

        public static LineEndEnum GetEnum(IVertex v)
        {
            if (v == null || v.Value == null)
                return LineEndEnum.Straight;

            switch (v.Value.ToString())
            {
                case "Straight": return LineEndEnum.Straight;

                case "Arrow": return LineEndEnum.Arrow;

                case "Triangle": return LineEndEnum.Triangle;

                case "FilledTriangle": return LineEndEnum.FilledTriangle;

                case "Diamond": return LineEndEnum.Diamond;

                case "FilledDiamond": return LineEndEnum.FilledDiamond;

                default: return LineEndEnum.Straight;
            }
        }

        public static IVertex GetVertex(LineEndEnum e)
        {
            switch(e){
                case LineEndEnum.Straight: return Straight_meta;

                case LineEndEnum.Arrow: return Arrow_meta;

                case LineEndEnum.Triangle: return Triangle_meta;

                case LineEndEnum.FilledTriangle: return FilledTriangle_meta;

                case LineEndEnum.Diamond: return Diamond_meta;

                case LineEndEnum.FilledDiamond: return FilledDiamond_meta;
            }

            return Straight_meta;
        }
    }

}
