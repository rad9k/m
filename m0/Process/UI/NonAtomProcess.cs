using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using m0.Foundation;

namespace m0.Process.UI
{
    public class NonAtomProcess
    {
        public static void StartNonAtomProcess()
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex process = ZeroTypes.VertexOperations.AddInstance(r.Get(@"User\CurrentUser:\CurrentSession:"), r.Get(@"System\Meta\User\NonAtomProcess"), r.Get(@"System\Meta\User\Session\Process"));

            process.AddEdge(r.Get(@"System\Meta\User\NonAtomProcess\StartTimeStamp"), MinusZero.Instance.Empty);

            ZeroTypes.DateTime.FillDateTime(process.Get("StartTimeStamp:"), DateTime.Now);
        }

        public static void StopNonAtomProcess()
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex processes = r.GetAll(@"User\CurrentUser:\CurrentSession:\Process:");

            r.Get(@"User\CurrentUser:\CurrentSession:").DeleteEdge(processes.First());
        }

        public static void AddUserChoice(IVertex question, IVertex answer, bool toSession)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex toAdd;

            if(toSession)
                toAdd = r.Get(@"User\CurrentUser:\CurrentSession:");
            else  
                toAdd = r.Get(@"User\CurrentUser:\CurrentSession:\Process:");

            toAdd.AddEdge(question, answer);
        
        }

        public static IVertex GetUserChoice(IVertex question)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex answer = r.Get(@"User\CurrentUser:\CurrentSession:\Process:\"+question.Value+":");

            if(answer==null)
                answer = r.Get(@"User\CurrentUser:\CurrentSession:\" + question.Value + ":");

            return answer;
        }
    }
}
