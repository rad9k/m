using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using m0.ZeroUML.Instructions;
using m0.ZeroCode.Helpers;

namespace m0.ZeroCode
{
    class ZeroCodeExecuter
    {
        IVertex baseVertex;
        IVertex inputVertex;

        const string colon = "|";
        const string slash = @"\ ";

        static IList<IEdge> dummy = new List<IEdge>();

        public IVertex Execute(IVertex baseVertex, IVertex expression)
        {
            throw new NotImplementedException();
        }

        public IVertex Get(IVertex baseVertex, IVertex expression)
        {
            throw new NotImplementedException();
        }

        public IVertex GetAll(IVertex baseVertex, IVertex expression)
        {
            ZeroCodeExecution exe = new ZeroCodeExecution();

            exe.metaMode = true;

            INoInEdgeInOutVertexVertex qs = InstructionHelpers.CreateQueryStack();

            InstructionHelpers.AddToStack(baseVertex, qs);

            IVertex ret=CallableEndPointDictionary.CallEndPoint(exe, qs, expression);

            if (ret != null)
                return ret;

            return MinusZero.Instance.CreateTempVertex();
        }
        
        public ZeroCodeExecuter()
        {

        }

    }
}
