using m0.Foundation;
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
        public static void ShowError(object where, string what)
        {
            MinusZero.Instance.DefaultUserInteraction.ShowException(
                UserInteractionUtil.CreateErrorVertex(where, what));
        }
        
        public static IVertex CreateErrorVertex(object where, string what)
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
                IVertex smz = MinusZero.Instance.Root.Get(@"System\Meta\ZeroTypes");

                _exception = smz.Get("Exception");
                _where = smz.Get(@"Exception\Where");
                _type = smz.Get(@"Exception\Type");
                _what = smz.Get(@"Exception\What");
                _error = smz.Get(@"ExceptionTypeEnum\Error");
            }

            IVertex error = VertexOperations.AddInstance(null, _exception);

            error.AddVertex(_where, where);

            error.AddVertex(_type, _error);

            error.AddVertex(_what, what);

            return error;
        }
    }
}
