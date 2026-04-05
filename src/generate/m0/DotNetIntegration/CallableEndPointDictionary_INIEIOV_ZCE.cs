using m0.Foundation;
using m0.Graph;
using m0.ZeroCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace m0.DotNetIntegration
{
    // EasyVertex.Execute
    //
    // INoInEdgeInOutVertexVertex CallEndPoint(IExecution exe, IVertex callableEndPointVertex)

    public class CallableEndPointDictionary_INIEIOV_ZCE
    {
        delegate INoInEdgeInOutVertexVertex CallableEndPointDelegate(IExecution exe);

        static Dictionary<IVertex, CallableEndPointDelegate> DotNetEndPointDictionary = new Dictionary<IVertex, CallableEndPointDelegate>();

        public static INoInEdgeInOutVertexVertex CallEndPoint(IExecution exe, IVertex callableEndPointVertex)
        {
            CallableEndPointDelegate del = null;

            if (DotNetEndPointDictionary.ContainsKey(callableEndPointVertex))
                del = DotNetEndPointDictionary[callableEndPointVertex];
            else
            {            
                if (GraphUtil.GetQueryOutFirst(callableEndPointVertex, "$Is", "DotNetStaticMethod") != null)
                {
                    string typeString = (string)GraphUtil.GetQueryOutFirst(callableEndPointVertex, "DotNetTypeName", null).Value;
                    string methodString = (string)GraphUtil.GetQueryOutFirst(callableEndPointVertex, "DotNetMethodName", null).Value;

                    Type type = Type.GetType(typeString);
                    MethodInfo method = type.GetMethod(methodString);

                    del = (CallableEndPointDelegate)method.CreateDelegate(typeof(CallableEndPointDelegate));
                }

                if(del!=null)
                    DotNetEndPointDictionary.Add(callableEndPointVertex, del);
            }

            if (del == null)
                return null;

            return del.Invoke(exe);            
        }
    }
}
