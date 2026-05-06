using m0.Foundation;
using m0.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace m0.Lib.StdView
{
    public class VertexToMermaid
    {
        public static INoInEdgeInOutVertexVertex VertexToMermaid_Transform(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex from = GraphUtil.GetQueryOutFirst(stack, "from", null);
            IVertex to = GraphUtil.GetQueryOutFirst(stack, "to", null);

            if (from == null || to == null)
                return exe.Stack;

            to.Value = VertexToMermaid_Process(from);

            return exe.Stack;
        }

        public static string VertexToMermaid_Process(IVertex baseVertex)
        {
            IVertex sqlRoot = MermaidErdUtil.ResolveSqlRoot(baseVertex);
            var builder = new StringBuilder();
            builder.AppendLine("erDiagram");

            if (sqlRoot == null)
                return builder.ToString();

            IList<IEdge> tableEdges = GraphUtil.GetQueryOut(sqlRoot, MermaidErdUtil.GetMetaValue(MermaidErdUtil.GetTableMeta()), null);
            var tableNames = new HashSet<string>(tableEdges.Select(edge => MermaidErdUtil.NormalizeIdentifier(GraphUtil.GetStringValue(edge.To))));

            foreach (IEdge tableEdge in tableEdges)
                AppendTable(builder, tableEdge.To);

            AppendRelations(builder, tableEdges, tableNames);

            return builder.ToString().TrimEnd();
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
    }
}
