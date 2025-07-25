using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Lib.Net
{
    public enum HttpActionEnum { GET, PUT, POST, DELETE, PATCH, HEADOPTIONS, TRACE }

    public class HttpActionEnumHelper
    {
        static IVertex GET_meta = MinusZero.Instance.root.Get(false, @"System\Lib\Net\HttpActionEnum\GET");
        static IVertex PUT_meta = MinusZero.Instance.root.Get(false, @"System\Lib\Net\HttpActionEnum\PUT");
        static IVertex POST_meta = MinusZero.Instance.root.Get(false, @"System\Lib\Net\HttpActionEnum\POST");
        static IVertex DELETE_meta = MinusZero.Instance.root.Get(false, @"System\Lib\Net\HttpActionEnum\DELETE");
        static IVertex PATCH_meta = MinusZero.Instance.root.Get(false, @"System\Lib\Net\HttpActionEnum\PATCH");
        static IVertex HEADOPTIONS_meta = MinusZero.Instance.root.Get(false, @"System\Lib\Net\HttpActionEnum\HEADOPTIONS");
        static IVertex TRACE_meta = MinusZero.Instance.root.Get(false, @"System\Lib\Net\HttpActionEnum\TRACE");

        public static HttpActionEnum GetEnum(IVertex v)
        {
            if (v == null || v.Value == null)
                return HttpActionEnum.GET;

            switch (v.Value.ToString())
            {
                case "GET": return HttpActionEnum.GET;
                case "PUT": return HttpActionEnum.PUT;
                case "POST": return HttpActionEnum.POST;
                case "DELETE": return HttpActionEnum.DELETE;
                case "PATCH": return HttpActionEnum.PATCH;
                case "HEADOPTIONS": return HttpActionEnum.HEADOPTIONS;
                case "TRACE": return HttpActionEnum.TRACE;
                default: return HttpActionEnum.GET;
            }
        }

        public static IVertex GetVertex(HttpActionEnum e)
        {
            switch (e)
            {
                case HttpActionEnum.GET: return GET_meta;
                case HttpActionEnum.PUT: return PUT_meta;
                case HttpActionEnum.POST: return POST_meta;
                case HttpActionEnum.DELETE: return DELETE_meta;
                case HttpActionEnum.PATCH: return PATCH_meta;
                case HttpActionEnum.HEADOPTIONS: return HEADOPTIONS_meta;
                case HttpActionEnum.TRACE: return TRACE_meta;
                default: return GET_meta;
            }
        }
    }

}
