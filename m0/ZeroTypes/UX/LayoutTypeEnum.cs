using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    enum LayoutTypeEnum { Vertical, Horizontal, Wrap, Manual, Auto }

    class LayoutTypeEnumHelper
    {
        static IVertex Vertical_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LayoutTypeEnum\Vertical");
        static IVertex Horizontal_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LayoutTypeEnum\Horizontal");
        static IVertex Wrap_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LayoutTypeEnum\Wrap");
        static IVertex Manual_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LayoutTypeEnum\Manual");
        static IVertex Auto_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LayoutTypeEnum\Auto");

        public static LayoutTypeEnum GetEnum(IVertex v)
        {
            if (v == null || v.Value == null)
                return LayoutTypeEnum.Auto;

            switch (v.Value.ToString())
            {
                case "Vertical": return LayoutTypeEnum.Vertical;

                case "Horizontal": return LayoutTypeEnum.Horizontal;

                case "Wrap": return LayoutTypeEnum.Wrap;

                case "Manual": return LayoutTypeEnum.Manual;

                case "Auto": return LayoutTypeEnum.Auto;

                default: return LayoutTypeEnum.Auto;
            }
        }

        public static IVertex GetVertex(LayoutTypeEnum e)
        {
            switch(e){
                case LayoutTypeEnum.Auto: return Auto_meta;

                case LayoutTypeEnum.Horizontal: return Horizontal_meta;

                case LayoutTypeEnum.Manual: return Manual_meta;

                case LayoutTypeEnum.Vertical: return Vertical_meta;

                case LayoutTypeEnum.Wrap: return Wrap_meta;                
            }

            return Auto_meta;
        }
    }

}
