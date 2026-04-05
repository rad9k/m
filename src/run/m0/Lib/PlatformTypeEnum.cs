using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes
{    
    public enum PlatformTypeEnum { Console, Desktop, Browser };

    public class PlatformTypeEnumHelper
    {
        static IVertex Console_meta = MinusZero.Instance.root.Get(false, @"System\Lib\Sys\PlatformTypeEnum\Console");
        static IVertex Desktop_meta = MinusZero.Instance.root.Get(false, @"System\Lib\Sys\PlatformTypeEnum\Desktop");
        static IVertex Browser_meta = MinusZero.Instance.root.Get(false, @"System\Lib\Sys\PlatformTypeEnum\Browser");


        public static PlatformTypeEnum GetEnum(IVertex v)
        {
            if (v == null || v.Value == null)
                return PlatformTypeEnum.Console;

            switch (v.Value.ToString())
            {
                case "Console": return PlatformTypeEnum.Console;
                case "Desktop": return PlatformTypeEnum.Desktop;
                case "Browser": return PlatformTypeEnum.Browser;

                default: return PlatformTypeEnum.Console;
            }
        }

        public static IVertex GetVertex(PlatformTypeEnum e)
        {
            switch (e)
            {
                case PlatformTypeEnum.Console: return Console_meta;
                case PlatformTypeEnum.Desktop: return Desktop_meta;
                case PlatformTypeEnum.Browser: return Browser_meta;
            }

            return Console_meta;
        }
    }
}
