using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    public class CallableEndPointDictionary
    {
        static Dictionary<IVertex, Func<ZeroCodeExecution, IVertex, IVertex, INoInEdgeInOutVertexVertex>> Dictionary = new Dictionary<IVertex, Func<ZeroCodeExecution, IVertex, IVertex, INoInEdgeInOutVertexVertex>>();
        
        public static INoInEdgeInOutVertexVertex CallEndPoint(ZeroCodeExecution exe, IVertex inputQs, IVertex instructionVertex)
        {
            Func<ZeroCodeExecution, IVertex, IVertex, INoInEdgeInOutVertexVertex> del = null;

            IVertex _is = GraphUtil.FindOneByMeta(instructionVertex, "$Is");

            if (_is == null)
                return null;

            if (Dictionary.ContainsKey(_is))
                del = Dictionary[_is];
            else
            {            
                IVertex ep = GraphUtil.FindOneByMeta(_is, "$CallableEndPoint");

                if (ep == null)
                    return null;

                if (GraphUtil.GetQueryOutFirst(ep, "$Is", "DotNetEndPoint") != null)
                {
                    string typeString = (string)GraphUtil.FindOneByMeta(ep, "TypeName").Value;
                    string methodString = (string)GraphUtil.FindOneByMeta(ep, "MethodName").Value;

                    Type type = Type.GetType(typeString);
                    MethodInfo method = type.GetMethod(methodString);

                    del = (Func<ZeroCodeExecution, IVertex, IVertex, INoInEdgeInOutVertexVertex>)method.CreateDelegate(typeof(Func<ZeroCodeExecution, IVertex, IVertex, INoInEdgeInOutVertexVertex>));
                }

                Dictionary.Add(_is, del);
            }

            if (del == null)
                return null;

            return del.Invoke(exe, inputQs, instructionVertex);            
        }
    }
}
