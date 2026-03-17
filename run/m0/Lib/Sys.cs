using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace m0.Lib
{
    public class Sys
    {
        public static INoInEdgeInOutVertexVertex SleepUntilGracefullExit(IExecution exe)
        {
            MinusZero.Instance.GracefullExitToken.Token.WaitHandle.WaitOne();

            return exe.Stack;
        }

        public static INoInEdgeInOutVertexVertex GetPlatformType(IExecution exe)
        {
            IVertex PlatformTypeEnumVertex = PlatformTypeEnumHelper.GetVertex(m0.MinusZero.Instance.UserInteraction.GetPlatformType());

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

             newStack.AddEdge(null, PlatformTypeEnumVertex);

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex StartTransaction(IExecution exe)
        {
            ITransaction prevTransaction = MinusZero.Instance.GetTopTransaction();

            ITransaction newTransaction = new Transaction(prevTransaction);

            newTransaction.Start();

            MinusZero.Instance.SetTopTransaction(newTransaction);

            return exe.Stack;
        }

        public static INoInEdgeInOutVertexVertex CommitTransaction(IExecution exe)
        {
            ITransaction currentTransaction = MinusZero.Instance.GetTopTransaction();

            ITransaction prevTransaction = currentTransaction.Previous;

            currentTransaction.Commit(exe);

            MinusZero.Instance.SetTopTransaction(prevTransaction);

            return exe.Stack;
        }

        public static INoInEdgeInOutVertexVertex RollbackTransaction(IExecution exe)
        {
            ITransaction currentTransaction = MinusZero.Instance.GetTopTransaction();

            ITransaction prevTransaction = currentTransaction.Previous;

            currentTransaction.Rollback(exe);

            MinusZero.Instance.SetTopTransaction(prevTransaction);

            return exe.Stack;
        }
    }
}
