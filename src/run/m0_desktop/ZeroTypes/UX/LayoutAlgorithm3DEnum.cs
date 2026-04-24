using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public enum LayoutAlgorithm3DEnum
    {
        FibonacciSphereShells,
        OrbitalPlanes,
        Force3D,
        ConcentricSpiral3D,
        Sugiyama3DLayers
    }

    public class LayoutAlgorithm3DEnumHelper
    {
        static IVertex FibonacciSphereShells_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LayoutAlgorithm3DEnumHelper\FibonacciSphereShells");
        static IVertex OrbitalPlanes_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LayoutAlgorithm3DEnumHelper\OrbitalPlanes");
        static IVertex Force3D_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LayoutAlgorithm3DEnumHelper\Force3D");
        static IVertex ConcentricSpiral3D_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LayoutAlgorithm3DEnumHelper\ConcentricSpiral3D");
        static IVertex Sugiyama3DLayers_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\LayoutAlgorithm3DEnumHelper\Sugiyama3DLayers");

        public static LayoutAlgorithm3DEnum GetEnum(IVertex v)
        {
            if (v == null || v.Value == null)
                return LayoutAlgorithm3DEnum.FibonacciSphereShells;

            switch (v.Value.ToString())
            {
                case "FibonacciSphereShells": return LayoutAlgorithm3DEnum.FibonacciSphereShells;
                case "OrbitalPlanes":         return LayoutAlgorithm3DEnum.OrbitalPlanes;
                case "Force3D":               return LayoutAlgorithm3DEnum.Force3D;
                case "ConcentricSpiral3D":    return LayoutAlgorithm3DEnum.ConcentricSpiral3D;
                case "Sugiyama3DLayers":      return LayoutAlgorithm3DEnum.Sugiyama3DLayers;
                default:                      return LayoutAlgorithm3DEnum.FibonacciSphereShells;
            }
        }

        public static IVertex GetVertex(LayoutAlgorithm3DEnum e)
        {
            switch (e)
            {
                case LayoutAlgorithm3DEnum.FibonacciSphereShells: return FibonacciSphereShells_meta;

                case LayoutAlgorithm3DEnum.OrbitalPlanes: return OrbitalPlanes_meta;

                case LayoutAlgorithm3DEnum.Force3D: return Force3D_meta;

                case LayoutAlgorithm3DEnum.ConcentricSpiral3D: return ConcentricSpiral3D_meta;

                case LayoutAlgorithm3DEnum.Sugiyama3DLayers: return Sugiyama3DLayers_meta;
            }

            return FibonacciSphereShells_meta;
        }
    }
}
