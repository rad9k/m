using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Util
{
    public class UserInteractionUtil
    {        
        public static string Ask(string question)
        {            
            return MinusZero.Instance.UserInteraction.InteractionInput(question);
        }

        public static void ShowException(object where, string what, ExceptionLevelEnum type)
        {
            MinusZero.Instance.UserInteraction.InteractionOutputException(
                UserInteractionUtil.CreateExceptionVertex(where, what, type));
        }

        public static IVertex CreateExceptionVertex(object where, string what, ExceptionLevelEnum level)
        {            
            IVertex _exception, _where, _type, _what;

            if (MinusZero.Instance.Root.Store.DetachState!=DetachStateEnum.Attached) // we are in detached mode
            {
                _exception = MinusZero.Instance.CreateTempVertex();
                _exception.Value = "Exception";

                _where = MinusZero.Instance.CreateTempVertex();
                _where.Value = "Where";

                _type = MinusZero.Instance.CreateTempVertex();
                _type.Value = "Type";

                _what = MinusZero.Instance.CreateTempVertex();
                _what.Value = "What";                
            }
            else
            {
                IVertex smz = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes");

                _exception = smz.Get(false, "Exception");
                _where = smz.Get(false, @"Exception\Where");
                _type = smz.Get(false, @"Exception\Type");
                _what = smz.Get(false, @"Exception\What");                
            }

            IVertex error = VertexOperations.AddInstance(null, _exception);

            GraphUtil.SetVertexValue(error, _where, where);
            GraphUtil.CreateOrReplaceEdge(error, _type, ExceptionLevelEnumHelper.GetVertex(level));
            GraphUtil.SetVertexValue(error, _what, what);            

            return error;
        }
    }
}
