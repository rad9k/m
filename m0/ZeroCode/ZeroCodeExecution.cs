using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    public class ZeroCodeExecution
    {
        public IVertex stack;

        public bool metaMode;

        public IVertex executeInstruction(IVertex inputQs, IVertex instructionVertex)
        {
            return CallableEndPointDictionary.CallEndPoint(this, inputQs, instructionVertex);
        }
    }
}
