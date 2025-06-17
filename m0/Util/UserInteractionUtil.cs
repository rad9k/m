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
            /*IVertex v = m0.MinusZero.Instance.CreateTempVertex();

            IVertex c = GraphUtil.AddClass(v, question);

            IVertex a=GraphUtil.AddAttribute(c, question+"2", MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\String"), 1, 1);

            a.AddVertex(MinusZero.Instance.root.Get(false, @"System\Meta\Presentation\$Hide"), null);

            IVertex o = VertexOperations.AddInstance(v, c);*/            

            //return o.Get(false, @"\").Value.ToString();

            return MinusZero.Instance.UserInteraction.InteractionInput(question);

        }

        public static void ShowError(object where, string what)
        {
            MinusZero.Instance.UserInteraction.InteractionOutputException(
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
