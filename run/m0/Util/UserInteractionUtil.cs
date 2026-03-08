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
        public enum ExceptionLevel { trace, debug, info, warning, error, fatal };

        public static string Ask(string question)
        {            
            return MinusZero.Instance.UserInteraction.InteractionInput(question);
        }

        public static void ShowException(object where, string what)
        {
            MinusZero.Instance.UserInteraction.InteractionOutputException(
                UserInteractionUtil.CreateExceptionVertex(where, what));
        }

        public static void ShowException(object where, string what, ExceptionLevel level)
        {

        }


        public static IVertex CreateExceptionVertex(object where, string what)
        {            
            IVertex _exception, _where, _type, _what, _error;

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

                _error = MinusZero.Instance.CreateTempVertex();
                _error.Value = "Error";
            }
            else
            {
                IVertex smz = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes");

                _exception = smz.Get(false, "Exception");
                _where = smz.Get(false, @"Exception\Where");
                _type = smz.Get(false, @"Exception\Type");
                _what = smz.Get(false, @"Exception\What");
                _error = smz.Get(false, @"ExceptionTypeEnum\Error");
            }

            IVertex error = VertexOperations.AddInstance(null, _exception);

            GraphUtil.SetVertexValue(error, _where, where);
            GraphUtil.CreateOrReplaceEdge(error, _type, _error);
            GraphUtil.SetVertexValue(error, _what, what);            

            return error;
        }
    }
}
