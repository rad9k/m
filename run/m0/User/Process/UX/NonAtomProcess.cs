using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using m0.Foundation;

namespace m0.User.Process.UX
{
    public class NonAtomProcess
    {
        public static void StartNonAtomProcess()
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex process = ZeroTypes.VertexOperations.AddInstance(r.Get(false, @"User\CurrentUser:\CurrentSession:"), r.Get(false, @"System\Meta\ZeroTypes\User\NonAtomProcess"), r.Get(false, @"System\Meta\ZeroTypes\User\Session\Process"));

            process.AddVertex(r.Get(false, @"System\Meta\ZeroTypes\User\NonAtomProcess\StartTimeStamp"), "");

            ZeroTypes.DateTime.FillDateTime(process.Get(false, "StartTimeStamp:"), DateTime.Now);
        }

        public static void StopNonAtomProcess()
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex processes = r.GetAll(false, @"User\CurrentUser:\CurrentSession:\Process:");

            r.Get(false, @"User\CurrentUser:\CurrentSession:").DeleteEdge(processes.First());
        }

        public static void AddUserChoice(IVertex question, IVertex answer, bool toSession)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex toAdd;

            if(toSession)
                toAdd = r.Get(false, @"User\CurrentUser:\CurrentSession:");
            else  
                toAdd = r.Get(false, @"User\CurrentUser:\CurrentSession:\Process:");

            toAdd.AddEdge(question, answer);
        
        }

        public static IVertex GetUserChoice(IVertex question)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex answer = r.Get(false, @"User\CurrentUser:\CurrentSession:\Process:\'"+question.Value+"':");

            if(answer==null)
                answer = r.Get(false, @"User\CurrentUser:\CurrentSession:\'" + question.Value + "':");

            return answer;
        }
    }
}
