using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public enum RepositionAlgorithmEnum {Radial, Force, Sugiyama, Kamada, Tree}

    class RepositionAlgorithmEnumHelper
    {
        static IVertex Radial_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RepositionAlgorithmEnum\Radial");
        static IVertex Force_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RepositionAlgorithmEnum\Force");
        static IVertex Sugiyama_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RepositionAlgorithmEnum\Sugiyama");
        static IVertex Kamada_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RepositionAlgorithmEnum\Kamada");
        static IVertex Tree_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\RepositionAlgorithmEnum\Tree");


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

                case "Hidden": return LayoutTypeEnum.Hidden;

                default: return LayoutTypeEnum.Hidden;
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

                case LayoutTypeEnum.Hidden: return Hidden_meta;
            }

            return Auto_meta;
        }
    }

}
