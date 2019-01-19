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
            IVertex smz = MinusZero.Instance.Root.Get(@"System\Meta\ZeroTypes");

            IVertex error = VertexOperations.AddInstance(null, smz.Get("Exception"));

            error.AddVertex(smz.Get(@"Exception\Where"), where);

            error.AddVertex(smz.Get(@"Exception\Type"), smz.Get(@"ExceptionTypeEnum\Error"));

            error.AddVertex(smz.Get(@"Exception\What"), what);

            return error;
        }
    }
}
