using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace m0.Lib
{
    public class Sys
    {
        public static INoInEdgeInOutVertexVertex SleepUntilGracefullExit(IExecution exe)
        {
            MinusZero.Instance.Log(1, "Sys.SleepUntilGracefullExit",
                "ENTER " + DescribeTransactionStack()
                + " platform=" + SafePlatform()
                + " stack=" + FormatRelevantStackTrace());

            MinusZero.Instance.GracefullExitToken.Token.WaitHandle.WaitOne();

            MinusZero.Instance.Log(1, "Sys.SleepUntilGracefullExit",
                "EXIT woken " + DescribeTransactionStack());

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

            MinusZero.Instance.Log(1, "Sys.StartTransaction",
                "AFTER depth=" + GetTransactionDepth()
                + " newHash=" + GetTransactionHash(newTransaction)
                + " prevHash=" + GetTransactionHash(prevTransaction)
                + " previousNull=" + (prevTransaction == null)
                + " platform=" + SafePlatform()
                + " stack=" + FormatRelevantStackTrace());

            return exe.Stack;
        }

        public static INoInEdgeInOutVertexVertex CommitTransaction(IExecution exe)
        {
            ITransaction currentTransaction = MinusZero.Instance.GetTopTransaction();

            ITransaction prevTransaction = currentTransaction.Previous;

            MinusZero.Instance.Log(1, "Sys.CommitTransaction",
                "BEFORE depth=" + GetTransactionDepth()
                + " currentHash=" + GetTransactionHash(currentTransaction)
                + " prevHash=" + GetTransactionHash(prevTransaction)
                + " platform=" + SafePlatform()
                + " stack=" + FormatRelevantStackTrace());

            currentTransaction.Commit(exe);

            MinusZero.Instance.SetTopTransaction(prevTransaction);

            MinusZero.Instance.Log(1, "Sys.CommitTransaction",
                "AFTER depth=" + GetTransactionDepth()
                + " topHash=" + GetTransactionHash(MinusZero.Instance.GetTopTransaction()));

            return exe.Stack;
        }

        public static INoInEdgeInOutVertexVertex RollbackTransaction(IExecution exe)
        {
            ITransaction currentTransaction = MinusZero.Instance.GetTopTransaction();

            ITransaction prevTransaction = currentTransaction.Previous;

            MinusZero.Instance.Log(1, "Sys.RollbackTransaction",
                "BEFORE depth=" + GetTransactionDepth()
                + " currentHash=" + GetTransactionHash(currentTransaction)
                + " prevHash=" + GetTransactionHash(prevTransaction)
                + " platform=" + SafePlatform()
                + " stack=" + FormatRelevantStackTrace());

            currentTransaction.Rollback(exe);

            MinusZero.Instance.SetTopTransaction(prevTransaction);

            MinusZero.Instance.Log(1, "Sys.RollbackTransaction",
                "AFTER depth=" + GetTransactionDepth()
                + " topHash=" + GetTransactionHash(MinusZero.Instance.GetTopTransaction()));

            return exe.Stack;
        }

        public static int GetTransactionDepth()
        {
            int depth = 0;
            ITransaction current = MinusZero.Instance.GetTopTransaction();

            while (current != null)
            {
                depth++;
                current = current.Previous;
            }

            return depth;
        }

        public static string DescribeTransactionStack()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("depth=").Append(GetTransactionDepth());

            ITransaction current = MinusZero.Instance.GetTopTransaction();
            int level = 0;
            while (current != null)
            {
                sb.Append(" [").Append(level).Append("]=").Append(GetTransactionHash(current));
                sb.Append("{previousNull=").Append(current.Previous == null).Append("}");
                current = current.Previous;
                level++;
            }

            if (level == 0)
                sb.Append(" [empty]");

            return sb.ToString();
        }

        private static string GetTransactionHash(ITransaction transaction)
        {
            if (transaction == null)
                return "<null>";

            return transaction.GetHashCode().ToString("X");
        }

        private static string SafePlatform()
        {
            try
            {
                if (MinusZero.Instance.UserInteraction == null)
                    return "<no-ui>";

                return MinusZero.Instance.UserInteraction.GetPlatformType().ToString();
            }
            catch (Exception ex)
            {
                return "<platform-error:" + ex.Message + ">";
            }
        }

        private static string FormatRelevantStackTrace()
        {
            try
            {
                StackTrace stackTrace = new StackTrace(1, false);
                StackFrame[] frames = stackTrace.GetFrames();
                if (frames == null || frames.Length == 0)
                    return "<no-frames>";

                StringBuilder sb = new StringBuilder();
                int kept = 0;

                foreach (StackFrame frame in frames)
                {
                    var method = frame.GetMethod();
                    if (method == null)
                        continue;

                    string typeName = method.DeclaringType != null
                        ? method.DeclaringType.FullName
                        : "<unknown>";

                    if (typeName.StartsWith("System.") ||
                        typeName.StartsWith("Microsoft.") ||
                        typeName.Contains("Sys.StartTransaction") ||
                        typeName.Contains("Sys.CommitTransaction") ||
                        typeName.Contains("Sys.RollbackTransaction") ||
                        typeName.Contains("Sys.FormatRelevantStackTrace") ||
                        typeName.Contains("Sys.SleepUntilGracefullExit"))
                        continue;

                    if (sb.Length > 0)
                        sb.Append(" <- ");

                    sb.Append(typeName).Append(".").Append(method.Name);
                    kept++;

                    if (kept >= 12)
                        break;
                }

                return sb.Length == 0 ? "<no-m0-frames>" : sb.ToString();
            }
            catch (Exception ex)
            {
                return "<stack-error:" + ex.Message + ">";
            }
        }
    }
}
