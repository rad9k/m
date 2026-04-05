using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroTypes
{    
    public enum ExceptionLevelEnum { Trace, Debug, Info, Warning, Error, Fatal };

    public class ExceptionLevelEnumHelper
    {
        static IVertex Trace_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExceptionLevelEnum\Trace");
        static IVertex Debug_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExceptionLevelEnum\Debug");
        static IVertex Info_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExceptionLevelEnum\Info");
        static IVertex Warning_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExceptionLevelEnum\Warning");
        static IVertex Error_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExceptionLevelEnum\Error");
        static IVertex Fatal_meta = MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\ExceptionLevelEnum\Fatal");


        public static ExceptionLevelEnum GetEnum(IVertex v)
        {
            if (v == null || v.Value == null)
                return ExceptionLevelEnum.Trace;

            switch (v.Value.ToString())
            {
                case "Trace": return ExceptionLevelEnum.Trace;
                case "Debug": return ExceptionLevelEnum.Debug;
                case "Info": return ExceptionLevelEnum.Info;
                case "Warning": return ExceptionLevelEnum.Warning;
                case "Error": return ExceptionLevelEnum.Error;
                case "Fatal": return ExceptionLevelEnum.Fatal;

                default: return ExceptionLevelEnum.Trace;
            }
        }

        public static IVertex GetVertex(ExceptionLevelEnum e)
        {
            switch (e)
            {
                case ExceptionLevelEnum.Trace: return Trace_meta;
                case ExceptionLevelEnum.Debug: return Debug_meta;
                case ExceptionLevelEnum.Info: return Info_meta;
                case ExceptionLevelEnum.Warning: return Warning_meta;
                case ExceptionLevelEnum.Error: return Error_meta;
                case ExceptionLevelEnum.Fatal: return Fatal_meta;
            }

            return Trace_meta;
        }
    }
}
