using m0.Foundation;
using m0.Graph.ExecutionFlow;
using System;
using System.Runtime.CompilerServices;

namespace m0.Graph
{
    internal static class GraphLifecycleLog
    {
        private const int LogLevel = 4;

        internal static void VertexCreated(
            IVertex vertex,
            VertexIdentifierRegistrationMode registrationMode)
        {
            if (!IsEnabled)
                return;

            Write(
                "VertexCreated",
                DescribeVertex(vertex) +
                " registration=" + registrationMode);
        }

        internal static void OrphanCheck(
            IVertex vertex,
            string reason,
            int incomingEdgeCount,
            int metaIncomingEdgeCount,
            int externalReferenceCount,
            bool eligible)
        {
            if (!IsEnabled)
                return;

            Write(
                "OrphanCheck",
                DescribeVertex(vertex) +
                " reason=" + reason +
                " in=" + incomingEdgeCount +
                " metaIn=" + metaIncomingEdgeCount +
                " external=" + externalReferenceCount +
                " eligible=" + eligible +
                " state=" + vertex.DisposedState +
                " root=" + vertex.IsRoot +
                " detach=" + vertex.Store.DetachState);
        }

        internal static void EdgeRemovalCandidate(
            string collection,
            IVertex candidate,
            IEdge removedEdge)
        {
            if (!IsEnabled)
                return;

            Write(
                "EdgeRemovalCandidate",
                "collection=" + collection +
                " candidate=" + DescribeVertex(candidate) +
                " edge=" + DescribeEdge(removedEdge));
        }

        internal static void SecondStageQueued(
            ISecondStageCommitAction action,
            ITransaction transaction)
        {
            if (!IsEnabled)
                return;

            Write(
                "SecondStageQueued",
                "action=" + DescribeAction(action) +
                " transaction=" + DescribeTransaction(transaction));
        }

        internal static void SecondStageDeferred(
            ISecondStageCommitAction action)
        {
            if (!IsEnabled)
                return;

            Write(
                "SecondStageDeferred",
                "action=" + DescribeAction(action) +
                " reason=no-current-transaction");
        }

        internal static void SecondStageWave(
            Transaction transaction,
            string phase,
            int wave,
            int actionCount)
        {
            if (!IsEnabled)
                return;

            Write(
                "SecondStageWave",
                "transaction=" + DescribeTransaction(transaction) +
                " phase=" + phase +
                " wave=" + wave +
                " actions=" + actionCount);
        }

        internal static void SecondStageExecute(
            IVertex vertex,
            bool eligible)
        {
            if (!IsEnabled)
                return;

            Write(
                "SecondStageExecute",
                DescribeVertex(vertex) +
                " eligible=" + eligible);
        }

        internal static void DisposeVertex(
            IVertex vertex,
            string phase,
            int incomingEdgeCount,
            int metaIncomingEdgeCount,
            int outgoingEdgeCount)
        {
            if (!IsEnabled)
                return;

            Write(
                "VertexDispose",
                DescribeVertex(vertex) +
                " phase=" + phase +
                " in=" + incomingEdgeCount +
                " metaIn=" + metaIncomingEdgeCount +
                " out=" + outgoingEdgeCount +
                " external=" + vertex.ExternalReferenceCount);
        }

        internal static void TransactionState(
            Transaction transaction,
            string operation)
        {
            if (!IsEnabled)
                return;

            Write(
                "Transaction",
                "operation=" + operation +
                " transaction=" + DescribeTransaction(transaction) +
                " previous=" + DescribeTransaction(transaction.Previous));
        }

        internal static void EventVertex(
            IVertex eventVertex,
            GraphChangeTransactionAtom atom,
            string phase)
        {
            if (!IsEnabled)
                return;

            Write(
                "EventVertex",
                DescribeVertex(eventVertex) +
                " phase=" + phase +
                " eventType=" + atom.Type +
                " source=" + DescribeVertex(atom.ChangedVertex));
        }

        internal static void EventExternalReference(
            IVertex eventVertex,
            string operation)
        {
            if (!IsEnabled)
                return;

            Write(
                "EventExternalReference",
                DescribeVertex(eventVertex) +
                " operation=" + operation +
                " external=" + eventVertex.ExternalReferenceCount);
        }

        internal static string DescribeVertex(IVertex vertex)
        {
            if (vertex == null)
                return "null";

            string value;
            try
            {
                value = vertex.Value == null
                    ? "null"
                    : vertex.Value.ToString();
            }
            catch (Exception exception)
            {
                value = "<error:" + exception.GetType().Name + ">";
            }

            return "[store=" + vertex.Store.Identifier +
                " id=" + vertex.Identifier +
                " object=" + RuntimeHelpers.GetHashCode(vertex) +
                " value=" + value + "]";
        }

        private static string DescribeEdge(IEdge edge)
        {
            if (edge == null)
                return "null";

            return "{from=" + DescribeVertex(edge.From) +
                " meta=" + DescribeVertex(edge.Meta) +
                " to=" + DescribeVertex(edge.To) + "}";
        }

        private static string DescribeAction(
            ISecondStageCommitAction action)
        {
            if (action is IVertex vertex)
                return DescribeVertex(vertex);

            return action == null
                ? "null"
                : action.GetType().FullName;
        }

        private static string DescribeTransaction(
            ITransaction transaction)
        {
            if (transaction == null)
                return "null";

            if (transaction is Transaction graphTransaction)
            {
                return "[id=" + graphTransaction.DiagnosticId +
                    " ambient=" + graphTransaction.IsAmbient +
                    " state=" + graphTransaction.State + "]";
            }

            return "[type=" + transaction.GetType().FullName +
                " state=" + transaction.State + "]";
        }

        private static void Write(
            string operation,
            string details)
        {
            MinusZero.Instance.Log(
                LogLevel,
                "GraphLifecycle." + operation,
                details);
        }

        private static bool IsEnabled =>
            MinusZero.Instance.DoLog &&
            MinusZero.Instance.LogLevel >= LogLevel;
    }
}
