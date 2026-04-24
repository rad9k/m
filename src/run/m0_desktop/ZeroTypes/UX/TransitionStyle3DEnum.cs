using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{

    public enum TransitionStyle3DEnum
    {
        Cut,
        OrbitTransition,
        FlyToAndSwap,
        GravityMorph,
        HyperspaceJump
    }
 class TransitionStyle3DEnumHelper
    {
        static IVertex Cut_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\TransitionStyle3DEnum\Cut");
        static IVertex OrbitTransition_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\TransitionStyle3DEnum\OrbitTransition");
        static IVertex FlyToAndSwap_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\TransitionStyle3DEnum\FlyToAndSwap");
        static IVertex GravityMorph_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\TransitionStyle3DEnum\GravityMorph");
        static IVertex HyperspaceJump_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\TransitionStyle3DEnum\HyperspaceJump");

        public static TransitionStyle3DEnum GetEnum(IVertex v)
        {
            if (v == null || v.Value == null)
                return TransitionStyle3DEnum.OrbitTransition;

            switch (v.Value.ToString())
            {
                case "Cut":             return TransitionStyle3DEnum.Cut;
                case "OrbitTransition": return TransitionStyle3DEnum.OrbitTransition;
                case "FlyToAndSwap":    return TransitionStyle3DEnum.FlyToAndSwap;
                case "GravityMorph":    return TransitionStyle3DEnum.GravityMorph;
                case "HyperspaceJump":  return TransitionStyle3DEnum.HyperspaceJump;
                default:                return TransitionStyle3DEnum.OrbitTransition;
            }
        }

        public static IVertex GetVertex(TransitionStyle3DEnum e)
        {
            switch (e)
            {
                case TransitionStyle3DEnum.Cut: return Cut_meta;

                case TransitionStyle3DEnum.OrbitTransition: return OrbitTransition_meta;

                case TransitionStyle3DEnum.FlyToAndSwap: return FlyToAndSwap_meta;

                case TransitionStyle3DEnum.GravityMorph: return GravityMorph_meta;

                case TransitionStyle3DEnum.HyperspaceJump: return HyperspaceJump_meta;
            }

            return Cut_meta;
        }
    }
}
