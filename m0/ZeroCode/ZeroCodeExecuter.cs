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

        IVertex dolar;

        private void AddDolarToStack(ZeroCodeExecution exe, IVertex expression)
        {
            exe.stack.AddEdge(dolar, expression);   
        }

        private void AddRootToStack(ZeroCodeExecution exe)
        {
            exe.stack.AddEdge(MinusZero.Instance.StackFrameInherits, MinusZero.Instance.Root);
        }

        public IVertex Execute(IVertex baseVertex, IVertex expression)
        {
            ZeroCodeExecution exe = new ZeroCodeExecution();

            exe.metaMode = true;

            exe.stack = InstructionHelpers.Create_INoInEdgeInOutVertexVertex_FromEdgesList(baseVertex);

            exe.newVertexCreationSpace = exe.stack;

            AddRootToStack(exe);

            AddDolarToStack(exe, expression);

            bool local_isStackFrameReturn;

            return InstructionHelpers.SequentiallyExecuteInstructions(exe, exe.stack, expression, out local_isStackFrameReturn);            
        }

        public IVertex Get(bool metaMode, IVertex baseVertex, IVertex expression)
        {
            IVertex res = GetAll(metaMode, baseVertex, expression);

            if (res != null && res.OutEdgesRaw.Count > 0)
                return res.OutEdgesRaw[0].To;

            return null;
        }

        public IVertex GetAll(bool metaMode, IVertex baseVertex, IVertex expression)
        {
            ZeroCodeExecution exe = new ZeroCodeExecution();

            exe.metaMode = metaMode;

            INoInEdgeInOutVertexVertex qs = InstructionHelpers.CreateStack();

            InstructionHelpers.AddToStack(qs, baseVertex);

            IVertex ret=CallableEndPointDictionary.CallEndPoint(exe, qs, expression);

            if (ret != null)
                return ret;

            return MinusZero.Instance.CreateTempVertex();
        }
        
        public ZeroCodeExecuter()
        {
            dolar = MinusZero.Instance.Root.Get(false, @"System\Meta\Base\$");
        }

    }
}
