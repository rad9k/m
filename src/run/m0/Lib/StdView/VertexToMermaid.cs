using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace m0.Lib.StdView
{
    public class VertexToMermaid
    {
        const string LogWhere = "VertexToMermaid";

        public static INoInEdgeInOutVertexVertex VertexToMermaid_Transform(IExecution exe)
        {
            MinusZero.Instance.Log(0, LogWhere, "Transform: enter");

            try
            {
                INoInEdgeInOutVertexVertex stack = exe.Stack;

                IVertex from = GraphUtil.GetQueryOutFirst(stack, "from", null);
                IVertex to = GraphUtil.GetQueryOutFirst(stack, "to", null);

                MinusZero.Instance.Log(0, LogWhere, "Transform: from=" + DescribeVertex(from));
                MinusZero.Instance.Log(0, LogWhere, "Transform: to=" + DescribeVertex(to));

                if (from == null || to == null)
                {
                    MinusZero.Instance.Log(0, LogWhere, "Transform: from or to is null - bailing out");
                    return exe.Stack;
                }

                string mermaid;
                try
                {
                    mermaid = VertexToMermaid_Process(from);
                }
                catch (Exception exProcess)
                {
                    MinusZero.Instance.Log(0, LogWhere, "Transform: VertexToMermaid_Process THREW " + exProcess.GetType().Name + ": " + exProcess.Message + "\n" + exProcess.StackTrace);
                    throw;
                }

                MinusZero.Instance.Log(0, LogWhere, "Transform: process result length=" + (mermaid == null ? -1 : mermaid.Length));

                try
                {
                    to.Value = mermaid;
                    MinusZero.Instance.Log(0, LogWhere, "Transform: to.Value assigned OK");
                }
                catch (Exception exValueSet)
                {
                    MinusZero.Instance.Log(0, LogWhere, "Transform: to.Value SETTER THREW " + exValueSet.GetType().Name + ": " + exValueSet.Message + "\n" + exValueSet.StackTrace);
                    throw;
                }

                MinusZero.Instance.Log(0, LogWhere, "Transform: exit OK");
                return exe.Stack;
            }
            catch (Exception ex)
            {
                MinusZero.Instance.Log(0, LogWhere, "Transform: UNHANDLED EXCEPTION " + ex.GetType().Name + ": " + ex.Message + "\n" + ex.StackTrace);
                throw;
            }
        }

        public static string VertexToMermaid_Process(IVertex baseVertex)
        {
            MinusZero.Instance.Log(0, LogWhere, "Process: baseVertex=" + DescribeVertex(baseVertex));

            IVertex sqlRoot = MermaidErdUtil.ResolveSqlRoot(baseVertex);
            MinusZero.Instance.Log(0, LogWhere, "Process: sqlRoot=" + DescribeVertex(sqlRoot));

            var builder = new StringBuilder();
            builder.AppendLine("erDiagram");

            if (sqlRoot == null)
            {
                MinusZero.Instance.Log(0, LogWhere, "Process: sqlRoot is null - returning empty erDiagram");
                return builder.ToString();
            }

            object tableMetaValue = MermaidErdUtil.GetMetaValue(MermaidErdUtil.GetTableMeta());
            IList<IEdge> tableEdges = GraphUtil.GetQueryOut(sqlRoot, tableMetaValue, null);
            MinusZero.Instance.Log(0, LogWhere, "Process: tableMetaValue=" + tableMetaValue + " tableEdges.Count=" + tableEdges.Count);

            var tableNames = new HashSet<string>(tableEdges.Select(edge => MermaidErdUtil.NormalizeIdentifier(GraphUtil.GetStringValue(edge.To))));

            foreach (IEdge tableEdge in tableEdges)
                AppendTable(builder, tableEdge.To);

            AppendRelations(builder, tableEdges, tableNames);

            string result = builder.ToString().TrimEnd();
            MinusZero.Instance.Log(0, LogWhere, "Process: built mermaid length=" + result.Length);
            return result;
        }

        private static void AppendTable(StringBuilder builder, IVertex tableVertex)
        {
            string tableName = MermaidErdUtil.NormalizeIdentifier(GraphUtil.GetStringValue(tableVertex));

            builder.Append("    ");
            builder.AppendLine(tableName + " {");

            foreach (IEdge columnEdge in GraphUtil.GetQueryOut(tableVertex, MermaidErdUtil.GetMetaValue(MermaidErdUtil.GetColumnMeta()), null))
                AppendColumn(builder, columnEdge.To);

            builder.AppendLine("    }");
            builder.AppendLine();
        }

        private static void AppendColumn(StringBuilder builder, IVertex columnVertex)
        {
            IVertex typeVertex = GraphUtil.GetQueryOutFirst(columnVertex, MermaidErdUtil.GetMetaValue(MermaidErdUtil.GetEdgeTargetMeta()), null);
            string typeName = MermaidErdUtil.GetMermaidTypeName(typeVertex);
            string columnName = MermaidErdUtil.NormalizeIdentifier(GraphUtil.GetStringValue(columnVertex));

            builder.Append("        ");
            builder.Append(typeName);
            builder.Append(' ');
            builder.Append(columnName);

            if (IsColumnFlagTrue(columnVertex, MermaidErdUtil.GetIsPkMeta()))
                builder.Append(" PK");

            if (IsColumnFlagTrue(columnVertex, MermaidErdUtil.GetIsFkMeta()))
                builder.Append(" FK");

            builder.AppendLine();
        }

        private static bool IsColumnFlagTrue(IVertex columnVertex, IVertex flagMeta)
        {
            IVertex flagVertex = GraphUtil.GetQueryOutFirst(columnVertex, MermaidErdUtil.GetMetaValue(flagMeta), null);
            return GraphUtil.GetBooleanValueOrFalse(flagVertex);
        }

        private static void AppendRelations(StringBuilder builder, IList<IEdge> tableEdges, ISet<string> tableNames)
        {
            foreach (IEdge tableEdge in tableEdges)
            {
                IVertex childTable = tableEdge.To;
                string childTableName = MermaidErdUtil.NormalizeIdentifier(GraphUtil.GetStringValue(childTable));

                foreach (IEdge relationEdge in GraphUtil.GetQueryOut(childTable, MermaidErdUtil.GetMetaValue(MermaidErdUtil.GetRelationMeta()), null))
                {
                    IVertex relationVertex = relationEdge.To;
                    IVertex parentTable = GraphUtil.GetQueryOutFirst(relationVertex, MermaidErdUtil.GetMetaValue(MermaidErdUtil.GetEdgeTargetMeta()), null);
                    if (parentTable == null)
                        continue;

                    string parentTableName = MermaidErdUtil.NormalizeIdentifier(GraphUtil.GetStringValue(parentTable));
                    if (!tableNames.Contains(parentTableName))
                        continue;

                    builder.Append("    ");
                    builder.Append(parentTableName);
                    builder.Append(" ||--o{ ");
                    builder.Append(childTableName);
                    builder.Append(" : \"");
                    builder.Append(MermaidErdUtil.EscapeMermaidLabel(GraphUtil.GetStringValue(relationVertex)));
                    builder.AppendLine("\"");
                }
            }
        }

        // Renders a compact, log-friendly description of an IVertex (id, value, store).
        internal static string DescribeVertex(IVertex vertex)
        {
            if (vertex == null)
                return "null";

            string identifier = vertex.Identifier == null ? "?" : vertex.Identifier.ToString();
            string value = GraphUtil.GetStringValueOrNull(vertex);
            string valueRender = value == null ? "<null>" : "\"" + value + "\"";
            string storeRender;
            try
            {
                IStore store = vertex.Store;
                if (store == null)
                    storeRender = "<no-store>";
                else
                    storeRender = store.GetType().Name + "[" + store.Identifier + "]";
            }
            catch (Exception ex)
            {
                storeRender = "<store-threw:" + ex.GetType().Name + ">";
            }

            return "{id=" + identifier + " val=" + valueRender + " store=" + storeRender + "}";
        }
    }
}
