using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes.UX
{
    public enum InstanceCreationEnum { Instance, InstanceAndDirect, Direct }

    class InstanceCreationEnumHelper
    {
        static IVertex Instance_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\InstanceCreationEnum\Instance");
        static IVertex InstanceAndDirect_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\InstanceCreationEnum\InstanceAndDirect");
        static IVertex Direct_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\UX\InstanceCreationEnum\Direct");

        public static InstanceCreationEnum GetEnum(IVertex v)
        {
            if (v == null || v.Value == null)
                return InstanceCreationEnum.Direct;

            switch (v.Value.ToString())
            {
                case "Instance": return InstanceCreationEnum.Instance;

                case "InstanceAndDirect": return InstanceCreationEnum.InstanceAndDirect;

                case "Direct": return InstanceCreationEnum.Direct;

                default: return InstanceCreationEnum.Direct;
            }
        }

        public static IVertex GetVertex(InstanceCreationEnum e)
        {
            switch(e){
                case InstanceCreationEnum.Instance: return Instance_meta;

                case InstanceCreationEnum.InstanceAndDirect: return InstanceAndDirect_meta;

                case InstanceCreationEnum.Direct: return Direct_meta;
            }

            return Direct_meta;
        }
    }

}
