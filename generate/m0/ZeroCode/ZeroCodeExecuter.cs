using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using m0.ZeroUML.Instructions;
using m0.ZeroCode.Helpers;
using m0.DotNetIntegration;

namespace m0.ZeroCode
{
    class ZeroCodeExecuter
    {                
        IVertex dolar;


        public IVertex Execute(IVertex toBeStackVertex, IVertex expression)
        {
            ZeroCodeExecution exe = new ZeroCodeExecution(toBeStackVertex, expression);

            bool local_isStackFrameReturn;

            return ZeroCodeExecutonUtil.SequentiallyExecuteInstructions(exe, exe.Stack, expression, out local_isStackFrameReturn);            
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

            exe.MetaMode = metaMode;

            INoInEdgeInOutVertexVertex qs = InstructionHelpers.CreateStack();

            InstructionHelpers.AddToStack_BAD_BEHAVIOR_IEdge_MANY_TIMES(qs, baseVertex);

            IVertex ret= CallableEndPointDictionary_INIEIOV_ZCE_IV_IV_B.CallEndPoint(exe, qs, expression);

            if (ret != null)
                return ret;

            return MinusZero.Instance.CreateTempVertex();
        }
    }
}
