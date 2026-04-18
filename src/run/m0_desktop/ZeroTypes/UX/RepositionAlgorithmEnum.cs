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


        public static RepositionAlgorithmEnum GetEnum(IVertex v)
        {
            if (v == null || v.Value == null)
                return RepositionAlgorithmEnum.Radial;

            switch (v.Value.ToString())
            {
                case "Radial": return RepositionAlgorithmEnum.Radial;

                case "Force": return RepositionAlgorithmEnum.Force;

                case "Sugiyama": return RepositionAlgorithmEnum.Sugiyama;

                case "Kamada": return RepositionAlgorithmEnum.Kamada;

                case "Tree": return RepositionAlgorithmEnum.Tree;

                default: return RepositionAlgorithmEnum.Radial;
            }
        }

        public static IVertex GetVertex(RepositionAlgorithmEnum e)
        {
            switch(e){
                case RepositionAlgorithmEnum.Radial: return Radial_meta;

                case RepositionAlgorithmEnum.Force: return Force_meta;

                case RepositionAlgorithmEnum.Sugiyama: return Sugiyama_meta;

                case RepositionAlgorithmEnum.Kamada: return Kamada_meta;

                case RepositionAlgorithmEnum.Tree: return Tree_meta;
            }

            return Radial_meta;
        }
    }
}
