using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace m0.Lib.StdView
{
    public class MermaidToVertex
    {
        public static INoInEdgeInOutVertexVertex MermaidToVertex_Transform(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex from = GraphUtil.GetQueryOutFirst(stack, "from", null);
            IVertex to = GraphUtil.GetQueryOutFirst(stack, "to", null);

            string mermaid = GraphUtil.GetStringValueOrNull(from);

            MermaidToVertex_Process(mermaid, to);

            return exe.Stack;
        }

        public static void MermaidToVertex_Process(string mermaid, IVertex to)
        {
            if (to == null || string.IsNullOrWhiteSpace(mermaid))
                return;

            MermaidErdDiagram diagram = ParseMermaidErd(mermaid);
            if (diagram.Tables.Count == 0 && diagram.Relations.Count == 0)
                return;

            IVertex sqlRoot = MermaidErdUtil.GetOrCreateSqlRoot(to);
            IVertex tableMeta = MermaidErdUtil.GetTableMeta();
            IVertex columnMeta = MermaidErdUtil.GetColumnMeta();
            IVertex associationMeta = MermaidErdUtil.GetAssociationMeta();
            IVertex edgeTargetMeta = MermaidErdUtil.GetEdgeTargetMeta();
            IVertex isAggregationMeta = MermaidErdUtil.GetIsAggregationMeta();
            IVertex isPkMeta = MermaidErdUtil.GetIsPkMeta();
            IVertex isFkMeta = MermaidErdUtil.GetIsFkMeta();

            var tableVerticesByName = new Dictionary<string, IVertex>(StringComparer.OrdinalIgnoreCase);

            foreach (MermaidErdTable table in diagram.Tables.Values)
            {
                IVertex tableVertex = CreateTable(sqlRoot, tableMeta, table.Name);
                tableVerticesByName[table.Name] = tableVertex;

                foreach (MermaidErdColumn column in table.Columns)
                    CreateColumn(tableVertex, columnMeta, edgeTargetMeta, isAggregationMeta, isPkMeta, isFkMeta, column);
            }

            foreach (MermaidErdRelation relation in diagram.Relations)
            {
                IVertex parentTable = GetOrCreateTableVertex(sqlRoot, tableMeta, tableVerticesByName, relation.ParentTableName);
                IVertex childTable = GetOrCreateTableVertex(sqlRoot, tableMeta, tableVerticesByName, relation.ChildTableName);

                if (associationMeta != null && parentTable != null && childTable != null)
                    parentTable.AddEdge(associationMeta, childTable);
            }
        }

        private static IVertex GetOrCreateTableVertex(IVertex sqlRoot, IVertex tableMeta, IDictionary<string, IVertex> tableVerticesByName, string tableName)
        {
            if (tableVerticesByName.TryGetValue(tableName, out IVertex tableVertex))
                return tableVertex;

            tableVertex = CreateTable(sqlRoot, tableMeta, tableName);
            tableVerticesByName[tableName] = tableVertex;
            return tableVertex;
        }

        private static IVertex CreateTable(IVertex sqlRoot, IVertex tableMeta, string tableName)
        {
            IVertex tableVertex = sqlRoot.AddVertex(tableMeta, tableName);

            if (tableMeta != null)
                tableVertex.AddEdge(MinusZero.Instance.Is, tableMeta);

            return tableVertex;
        }

        private static void CreateColumn(
            IVertex tableVertex,
            IVertex columnMeta,
            IVertex edgeTargetMeta,
            IVertex isAggregationMeta,
            IVertex isPkMeta,
            IVertex isFkMeta,
            MermaidErdColumn column)
        {
            IVertex columnVertex = tableVertex.AddVertex(columnMeta, column.Name);

            if (columnMeta != null)
                columnVertex.AddEdge(MinusZero.Instance.Is, columnMeta);

            if (isAggregationMeta != null)
                columnVertex.AddEdge(isAggregationMeta, MinusZero.Instance.Empty);

            IVertex typeVertex = MermaidErdUtil.GetSqlTypeVertex(column.MermaidType);
            if (edgeTargetMeta != null && typeVertex != null)
                columnVertex.AddEdge(edgeTargetMeta, typeVertex);

            if (column.IsPrimaryKey && isPkMeta != null)
                columnVertex.AddVertex(isPkMeta, "True");

            if (column.IsForeignKey && isFkMeta != null)
                columnVertex.AddVertex(isFkMeta, "True");
        }

        private static MermaidErdDiagram ParseMermaidErd(string mermaid)
        {
            var diagram = new MermaidErdDiagram();
            MermaidErdTable currentTable = null;

            foreach (string rawLine in mermaid.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
            {
                string line = StripLineComment(rawLine).Trim();
                if (line.Length == 0 || line.Equals("erDiagram", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (currentTable != null)
                {
                    if (line == "}")
                    {
                        currentTable = null;
                        continue;
                    }

                    MermaidErdColumn column = ParseColumn(line);
                    if (column != null)
                        currentTable.Columns.Add(column);

                    continue;
                }

                if (line.EndsWith("{", StringComparison.Ordinal))
                {
                    string tableName = MermaidErdUtil.NormalizeIdentifier(line.Substring(0, line.Length - 1));
                    if (tableName.Length == 0)
                        continue;

                    currentTable = diagram.GetOrCreateTable(tableName);
                    continue;
                }

                MermaidErdRelation relation = ParseRelation(line);
                if (relation != null)
                {
                    diagram.GetOrCreateTable(relation.ParentTableName);
                    diagram.GetOrCreateTable(relation.ChildTableName);
                    diagram.Relations.Add(relation);
                }
            }

            return diagram;
        }

        private static string StripLineComment(string line)
        {
            int commentIndex = line.IndexOf("%%", StringComparison.Ordinal);
            return commentIndex >= 0 ? line.Substring(0, commentIndex) : line;
        }

        private static MermaidErdColumn ParseColumn(string line)
        {
            string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return null;

            var column = new MermaidErdColumn
            {
                MermaidType = MermaidErdUtil.NormalizeIdentifier(parts[0]),
                Name = MermaidErdUtil.NormalizeIdentifier(parts[1])
            };

            for (int i = 2; i < parts.Length; i++)
            {
                string key = parts[i].Trim(',', '"').ToUpperInvariant();
                if (key == "PK")
                    column.IsPrimaryKey = true;
                else if (key == "FK")
                    column.IsForeignKey = true;
            }

            return column.Name.Length == 0 ? null : column;
        }

        private static MermaidErdRelation ParseRelation(string line)
        {
            int colonIndex = line.IndexOf(':');
            string relationPart = colonIndex >= 0 ? line.Substring(0, colonIndex).Trim() : line;
            string[] parts = relationPart.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
                return null;

            string leftTableName = MermaidErdUtil.NormalizeIdentifier(parts[0]);
            string relationToken = parts[1];
            string rightTableName = MermaidErdUtil.NormalizeIdentifier(parts[2]);

            if (leftTableName.Length == 0 || rightTableName.Length == 0)
                return null;

            if (IsOneToManyFromLeft(relationToken))
                return new MermaidErdRelation { ParentTableName = leftTableName, ChildTableName = rightTableName };

            if (IsOneToManyFromRight(relationToken))
                return new MermaidErdRelation { ParentTableName = rightTableName, ChildTableName = leftTableName };

            return null;
        }

        private static bool IsOneToManyFromLeft(string relationToken)
        {
            return relationToken.StartsWith("||--", StringComparison.Ordinal)
                && (relationToken.EndsWith("o{", StringComparison.Ordinal) || relationToken.EndsWith("|{", StringComparison.Ordinal));
        }

        private static bool IsOneToManyFromRight(string relationToken)
        {
            return relationToken.EndsWith("--||", StringComparison.Ordinal)
                && (relationToken.StartsWith("}o", StringComparison.Ordinal) || relationToken.StartsWith("}|", StringComparison.Ordinal));
        }
    }

    internal class MermaidErdDiagram
    {
        public IDictionary<string, MermaidErdTable> Tables { get; } = new Dictionary<string, MermaidErdTable>(StringComparer.OrdinalIgnoreCase);
        public IList<MermaidErdRelation> Relations { get; } = new List<MermaidErdRelation>();

        public MermaidErdTable GetOrCreateTable(string tableName)
        {
            if (Tables.TryGetValue(tableName, out MermaidErdTable table))
                return table;

            table = new MermaidErdTable { Name = tableName };
            Tables[tableName] = table;
            return table;
        }
    }

    internal class MermaidErdTable
    {
        public string Name { get; set; }
        public IList<MermaidErdColumn> Columns { get; } = new List<MermaidErdColumn>();
    }

    internal class MermaidErdColumn
    {
        public string MermaidType { get; set; }
        public string Name { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
    }

    internal class MermaidErdRelation
    {
        public string ParentTableName { get; set; }
        public string ChildTableName { get; set; }
    }

    internal static class MermaidErdUtil
    {
        private const string SqlRootValue = "SQL";
        private const string MissingMetaValue = "__MermaidMissingMeta__";

        private static readonly IVertex TableMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\CustomDomain\SQL\Table");
        private static readonly IVertex ColumnMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\CustomDomain\SQL\Table\Column");
        private static readonly IVertex AssociationMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\CustomDomain\SQL\Table\Association");
        private static readonly IVertex IsPkMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\CustomDomain\SQL\Table\Column\IsPK");
        private static readonly IVertex IsFkMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\CustomDomain\SQL\Table\Column\IsFK");
        private static readonly IVertex SqlTypesRoot = MinusZero.Instance.Root.Get(false, @"System\Meta\CustomDomain\SQL\Types");
        private static readonly IVertex EdgeTargetMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget");
        private static readonly IVertex IsAggregationMeta = MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex\$IsAggregation");

        private static readonly IDictionary<string, string> MermaidTypeToSqlTypeName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "int", "INTEGER" },
            { "integer", "INTEGER" },
            { "string", "TEXT" },
            { "text", "TEXT" },
            { "float", "REAL" },
            { "double", "REAL" },
            { "real", "REAL" },
            { "decimal", "NUMERIC" },
            { "number", "NUMERIC" },
            { "numeric", "NUMERIC" },
            { "blob", "BLOB" },
            { "binary", "BLOB" }
        };

        public static IVertex GetOrCreateSqlRoot(IVertex target)
        {
            if (GraphUtil.GetStringValue(target) == SqlRootValue)
                return target;

            if (target.Value == null || GraphUtil.GetStringValue(target) == "")
            {
                target.Value = SqlRootValue;
                return target;
            }

            IVertex existingSqlRoot = GraphUtil.GetQueryOutFirst(target, null, SqlRootValue);
            return existingSqlRoot ?? target.AddVertex(null, SqlRootValue);
        }

        public static IVertex ResolveSqlRoot(IVertex baseVertex)
        {
            if (baseVertex == null)
                return null;

            if (GraphUtil.GetStringValue(baseVertex) == SqlRootValue || GraphUtil.GetQueryOut(baseVertex, GetMetaValue(TableMeta), null).Count > 0)
                return baseVertex;

            return GraphUtil.GetQueryOutFirst(baseVertex, null, SqlRootValue);
        }

        public static IVertex GetTableMeta()
        {
            return TableMeta;
        }

        public static IVertex GetColumnMeta()
        {
            return ColumnMeta;
        }

        public static IVertex GetAssociationMeta()
        {
            return AssociationMeta;
        }

        public static IVertex GetIsPkMeta()
        {
            return IsPkMeta;
        }

        public static IVertex GetIsFkMeta()
        {
            return IsFkMeta;
        }

        public static IVertex GetEdgeTargetMeta()
        {
            return MinusZero.Instance.EdgeTarget ?? EdgeTargetMeta;
        }

        public static IVertex GetIsAggregationMeta()
        {
            return MinusZero.Instance.IsAggregation ?? IsAggregationMeta;
        }

        public static object GetMetaValue(IVertex metaVertex)
        {
            return metaVertex != null ? metaVertex.Value : MissingMetaValue;
        }

        public static IVertex GetSqlTypeVertex(string mermaidType)
        {
            string sqlTypeName = GetSqlTypeName(mermaidType);
            return SqlTypesRoot?.Get(false, sqlTypeName);
        }

        public static string GetSqlTypeName(string mermaidType)
        {
            string normalizedType = NormalizeIdentifier(mermaidType);
            int genericStartIndex = normalizedType.IndexOf('(');
            if (genericStartIndex > 0)
                normalizedType = normalizedType.Substring(0, genericStartIndex);

            if (MermaidTypeToSqlTypeName.TryGetValue(normalizedType, out string sqlTypeName))
                return sqlTypeName;

            string upperType = normalizedType.ToUpperInvariant();
            if (upperType == "INTEGER" || upperType == "TEXT" || upperType == "REAL" || upperType == "NUMERIC" || upperType == "BLOB")
                return upperType;

            return "TEXT";
        }

        public static string GetMermaidTypeName(IVertex sqlTypeVertex)
        {
            string sqlTypeName = GraphUtil.GetStringValue(sqlTypeVertex).ToUpperInvariant();
            if (sqlTypeName == "INTEGER")
                return "int";
            if (sqlTypeName == "TEXT")
                return "string";
            if (sqlTypeName == "REAL")
                return "float";
            if (sqlTypeName == "NUMERIC")
                return "decimal";
            if (sqlTypeName == "BLOB")
                return "blob";

            return "string";
        }

        public static string NormalizeIdentifier(string identifier)
        {
            if (identifier == null)
                return "";

            return identifier.Trim().Trim('"', '`');
        }

        public static string EscapeMermaidLabel(string label)
        {
            return (label ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
