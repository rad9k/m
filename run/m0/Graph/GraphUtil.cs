using m0.Foundation;
using m0.Store;
using m0.Store.Text;
using m0.Util;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using m0.ZeroUML.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace m0.Graph
{
    public delegate bool GraphIteratorIterate(IEdge vertex);

    public class GraphIterator
    {
        object value;

        INoInEdgeInOutVertexVertex valueAsINoInEdgeInOutVertexVertex;

        IStore store;

        public GraphIterator(object value)
        {
            this.value = value;

            if (value is INoInEdgeInOutVertexVertex)
                valueAsINoInEdgeInOutVertexVertex = (INoInEdgeInOutVertexVertex)value;            
        }

        public bool Compare(IEdge vertex)
        {
            if (GeneralUtil.CompareStrings(value,vertex.To.Value))
                return true;

            return false;
        }

        public bool CompareMeta(IEdge vertex)
        {
            if (GeneralUtil.CompareStrings(value,vertex.Meta.Value))
                return true;

            return false;
        }        

        public bool AddToINoInEdgeInOutVertexVertex(IEdge vertex)
        {
            if (valueAsINoInEdgeInOutVertexVertex != null)
                valueAsINoInEdgeInOutVertexVertex.AddEdgeForNoInEdgeInOutVertexVertex_BAD_BEHAVIOR_IEdge_MANY_TIMES(vertex);            

            return true;
        }        
    }

    public class GraphUtil
    {
        public enum DebugOperationEnum { Init, OutEdgeAdd, OutEdgeRemove, InEdgeAdd, InEdgeRemove, Dispose, Value }

        static string DebugStore = "C:\\m0\\b.m0";
        static long[] DebugVertex = { 11 };
        static DebugOperationEnum[] DebugOperation = {
            DebugOperationEnum.InEdgeRemove,
            DebugOperationEnum.Init,
            DebugOperationEnum.InEdgeAdd,
            DebugOperationEnum.Value,
            DebugOperationEnum.Dispose
        };

        public static bool IsMetaDoubleDollar(IEdge e)
        {
            string meta = e.Meta.Value.ToString();

            if (meta.Length >= 2 && meta[0] == '$' && meta[1] == '$')
                return true;

            return false;
        }

        public static void Debug(IVertex v, DebugOperationEnum Operation)
        {
            return;

            if (v != null
                && v.Identifier != null
                && v.Identifier is long
                && DebugVertex.Contains((long)v.Identifier)
                && v.Store.Identifier == DebugStore
                && DebugOperation.Contains(Operation))
            {
                int x = 0;
            }
        }

        public static string GetSubVertexesAsString(IVertex v, int nestLimit)
        {
            StringBuilder s = new StringBuilder();

            s.Append(v.Value.ToString());

            GetSubVertexesAsString_Reccurent(s, v, 0, nestLimit);

            return s.ToString();
        }

        static void GetSubVertexesAsString_Reccurent(StringBuilder s, IVertex v, int level, int nestLimit)
        {
            if (nestLimit <= level)
                return;

            StringBuilder tab = new StringBuilder();

            for (int x = 0; x < level; x++)
                tab.Append("    ");

            foreach (IEdge e in v)
            {
                s.Append(tab);

                s.Append(e.Meta.ToString() + " :: " + e.To.ToString() + "\n");

                GetSubVertexesAsString_Reccurent(s, e.To, level + 1, nestLimit);
            }
        }

        public static string GetVertexIdString(IVertex v)
        {
            return "[" + v.Store.Identifier.ToString() + ":" + v.Identifier.ToString() + "]";
        }

        public static IList<IVertex> GetVertexListFromEdgeEnumerable(IEnumerable<IEdge> edges)
        {
            IList<IVertex> vList = new List<IVertex>();

            foreach (IEdge e in edges)
                vList.Add(e.To);

            return vList;
        }

        public static void LoadTXTAndParse(string fileName, IVertex baseVertex)
        {
            string text = System.IO.File.ReadAllText(fileName);

            IEdge baseEdge_new;
            MinusZero.Instance.DefaultFormalTextParser.Parse(new EdgeBase(null, null, baseVertex), text, CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new);
        }

        public static void LoadAndParse(string fileName, IVertex baseVertex, bool ReadOnly)
        {
            TextStore TextStore = (TextStore)baseVertex.Store.StoreUniverse.GetStore("m0.Store.Text.TextStore, m0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", fileName);

            TextStore.ReadOnly = ReadOnly;

            foreach (IEdge e in TextStore.Root) // this is enough for now
                baseVertex.AddEdge(e.Meta, e.To);

            //ZeroUMLInstructionHelpers.MoveEdgesIntoVertex(TextStore.Root, baseVertex); copies everything including

            // GraphUtil.DeepCopyByVertex(TextStore.Root, baseVertex); works but problem with meta (old metas remain and they are not copied)
        }

        public static void LoadTXTParseAndMove(string fileName, IVertex baseVertex, string vertexName)
        {
            IEdge tmp = baseVertex.AddVertexAndReturnEdge(null, null);

            LoadTXTAndParse(fileName, tmp.To);

            IEdge e = tmp.To.GetAll(false, vertexName).First();

            baseVertex.AddEdge(e.Meta, e.To);

            baseVertex.DeleteEdge(tmp);
        }

        public static IVertex AddEnum(IVertex baseVertex, string enumName, string[] values)
        {
            IVertex r = MinusZero.Instance.root;

            IVertex a = baseVertex.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Enum"), enumName);

            a.AddEdge(m0.MinusZero.Instance.Inherits, r.Get(false, @"System\Meta\ZeroTypes\EnumBase"));

            foreach (string v in values)
                a.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Enum\EnumValue"), v);

            return a;
        }

        public static IVertex AddClass(IVertex baseVertex, string className)
        {
            IVertex r = MinusZero.Instance.root;

            IVertex a = baseVertex.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Class"), className);

            a.AddEdge(MinusZero.Instance.Is, r.Get(false, @"System\Meta\ZeroUML\Class"));

            return a;
        }

        public static void AddInherits(IVertex baseVertex, IVertex inherit)
        {
            baseVertex.AddEdge(m0.MinusZero.Instance.Inherits, inherit);
        }

        public static IVertex AddAttribute(IVertex baseVertex, string attributeName, IVertex target, int MinCardinality, int MaxCardinality)
        {
            IVertex r = MinusZero.Instance.root;

            IVertex a = baseVertex.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Class\Attribute"), attributeName);

            a.AddEdge(MinusZero.Instance.Is, r.Get(false, @"System\Meta\ZeroUML\Class\Attribute"));

            a.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), target);

            a.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$IsAggregation"), MinusZero.Instance.Empty);

            a.AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$MinCardinality"), MinCardinality);

            a.AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$MaxCardinality"), MaxCardinality);

            return a;
        }

        public static IVertex AddAttribute(IVertex baseVertex, string attributeName, IVertex target, int MinCardinality, int MaxCardinality, object DefaultValue)
        {
            IVertex r = MinusZero.Instance.root;

            IVertex a = baseVertex.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Class\Attribute"), attributeName);

            a.AddEdge(MinusZero.Instance.Is, r.Get(false, @"System\Meta\ZeroUML\Class\Attribute"));

            a.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), target);

            a.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$IsAggregation"), MinusZero.Instance.Empty);

            a.AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$MinCardinality"), MinCardinality);

            a.AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$MaxCardinality"), MaxCardinality);

            a.AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$DefaultValue"), DefaultValue);

            return a;
        }

        public static IVertex AddAttribute(IVertex baseVertex, string attributeName, IVertex target, int MinCardinality, int MaxCardinality, object DefaultValue, object MinValue, object MaxValue)
        {
            IVertex r = MinusZero.Instance.root;

            IVertex a = baseVertex.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Class\Attribute"), attributeName);

            a.AddEdge(MinusZero.Instance.Is, r.Get(false, @"System\Meta\ZeroUML\Class\Attribute"));

            a.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), target);

            a.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$IsAggregation"), MinusZero.Instance.Empty);

            a.AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$MinCardinality"), MinCardinality);

            a.AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$MaxCardinality"), MaxCardinality);

            a.AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$DefaultValue"), DefaultValue);

            a.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Class\Attribute\MinValue"), MinValue);

            a.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Class\Attribute\MaxValue"), MaxValue);

            return a;
        }

        public static void AddAssociation(IVertex baseVertex, string attributeName, IVertex target, int MinCardinality, int MaxCardinality)
        {
            IVertex r = MinusZero.Instance.root;

            IVertex a = baseVertex.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Class\Association"), attributeName);

            a.AddEdge(MinusZero.Instance.Is, r.Get(false, @"System\Meta\ZeroUML\Class\Association"));

            a.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), target);

            a.AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$MinCardinality"), MinCardinality);

            a.AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$MaxCardinality"), MaxCardinality);
        }

        public static void AddAggregation(IVertex baseVertex, string attributeName, IVertex target, int MinCardinality, int MaxCardinality)
        {
            IVertex r = MinusZero.Instance.root;

            IVertex a = baseVertex.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Class\Aggregation"), attributeName);

            a.AddEdge(MinusZero.Instance.Is, r.Get(false, @"System\Meta\ZeroUML\Class\Aggregation"));

            a.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), target);

            a.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$IsAggregation"), MinusZero.Instance.Empty);

            a.AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$MinCardinality"), MinCardinality);

            a.AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$MaxCardinality"), MaxCardinality);
        }

        public static IVertex AddMetaEdge(IVertex baseVertex, string edgeName, IVertex target)
        {
            IVertex r = MinusZero.Instance.root;

            IVertex a = baseVertex.AddVertex(null, edgeName);

            a.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$EdgeTarget"), target);

            return a;
        }

        public static bool CompareEdges(IEdge one, IEdge two)
        {
            if (one.From == two.From && one.Meta == two.Meta && one.To == two.To)
                return true;

            return false;
        }

        public static IEdge CreateArtificialEdge(IVertex meta, IVertex to)
        {
            EdgeBase e = new EdgeBase(null, meta, to);

            return e;
        }

        public static IVertex DivideQueryAndGetByPart(IVertex baseVertex, string query)
        {
            IList<string> queryParts = query.Split('\\');

            int pos = 0;

            IVertex cursor = baseVertex;

            while (pos < queryParts.Count)
            {
                cursor = GraphUtil.GetQueryOutFirst(cursor, null, queryParts[pos]);

                if (cursor == null)
                    return null;

                pos++;
            }

            return cursor;
        }

        public static IVertex SimpleCreateVertexPath(IVertex baseVertex, string query)
        {
            IList<string> queryParts = query.Split('\\');

            int pos = 0;

            IVertex cursor = baseVertex;

            while (pos < queryParts.Count)
            {
                string actualQueryPart = queryParts[pos];

                IVertex possibleNewCursor = GraphUtil.GetQueryOutFirst(cursor, null, actualQueryPart);

                if (possibleNewCursor == null)
                    cursor = cursor.AddVertex(null, actualQueryPart);
                else
                    cursor = possibleNewCursor;

                pos++;
            }

            return cursor;
        }

        public static IVertex GetQueryOutFirst(IVertex baseVertex, object meta, object value)
        {
            IEdge result;
            IList<IEdge> results;

            baseVertex.QueryOutEdges(meta, value, out result, out results);

            if (result != null)
                return result.To;

            if (results != null && results.Count > 0)
                return results.First().To;

            return null;
        }

        public static IVertex GetQueryInFirst(IVertex baseVertex, object meta, object value)
        {
            IEdge result;
            IList<IEdge> results;

            baseVertex.QueryInEdges(meta, value, out result, out results);

            if (result != null)
                return result.From;

            if (results != null && results.Count > 0)
                return results.First().From;

            return null;
        }

        public static IEdge GetQueryOutFirstEdge(IVertex baseVertex, object meta, object value)
        {
            IEdge result;
            IList<IEdge> results;

            baseVertex.QueryOutEdges(meta, value, out result, out results);

            if (result != null)
                return result;

            if (results != null && results.Count > 0)
                return results.First();

            return null;
        }

        public static IEdge GetQueryInFirstEdge(IVertex baseVertex, object meta, object value)
        {
            IEdge result;
            IList<IEdge> results;

            baseVertex.QueryInEdges(meta, value, out result, out results);

            if (result != null)
                return result;

            if (results != null && results.Count > 0)
                return results.First();

            return null;
        }

        public static IList<IEdge> GetQueryOut(IVertex baseVertex, object meta, object value)
        {
            IEdge result;
            IList<IEdge> results;

            baseVertex.QueryOutEdges(meta, value, out result, out results);

            if (result != null)
            {
                results = new List<IEdge>();
                results.Add(result);
                return results;
            }

            if (results != null && results.Count > 0)
                return results;

            return new List<IEdge>();
        }

        public static IList<IEdge> GetQueryIn(IVertex baseVertex, object meta, object from)
        {
            IEdge result;
            IList<IEdge> results;

            baseVertex.QueryInEdges(meta, from, out result, out results);

            if (result != null)
            {
                results = new List<IEdge>();
                results.Add(result);
                return results;
            }

            if (results != null && results.Count > 0)
                return results;

            return new List<IEdge>();
        }

        public static int GetQueryOutCount(IVertex baseVertex, object meta, object value)
        {
            IEdge result;
            IList<IEdge> results;

            baseVertex.QueryOutEdges(meta, value, out result, out results);

            if (result != null)
            {
                return 1;
            }

            if (results != null)
                return results.Count();

            return 0;
        }

        public static bool ExistQueryOut(IVertex baseVertex, object meta, object value)
        {
            IEdge result;
            IList<IEdge> results;

            baseVertex.QueryOutEdges(meta, value, out result, out results);

            if (result != null)
            {
                return true;
            }

            if (results != null)
                return true;

            return false;
        }

        public static bool ExistQueryIn(IVertex baseVertex, object meta, object value)
        {
            IEdge result;
            IList<IEdge> results;

            baseVertex.QueryInEdges(meta, value, out result, out results);

            if (result != null)
            {
                return true;
            }

            if (results != null)
                return true;

            return false;
        }

        public static object GetMetaAndValueObject(object meta, object value)
        {
            int toRet = 0;

            if (meta != null)
                toRet = meta.GetHashCode();


            if (value != null)
                toRet += -2 * value.GetHashCode();

            return toRet;
        }



        public static void AddRange_NoNoInherit(IList<IEdge> to, IList<IEdge> from)
        {
            foreach (IEdge e in from)
                if (!GraphUtil.ExistQueryOut(e.Meta, "$NoInherit", null))
                    to.Add(e);
        }

        public static string GetQueryStringPart_MetaMode(FormalTextLanguageDictinaries dict, IVertex meta, IVertex to)
        {
            if (GeneralUtil.CompareStrings(meta.ToString(), "$Empty"))
                return ZeroCodeCommon.stringToPossiblyEscapedString(dict, to.ToString());
            else
                return ZeroCodeCommon.stringToPossiblyEscapedString(dict, meta.ToString()) + dict.MetaSeparator + ZeroCodeCommon.stringToPossiblyEscapedString(dict, to.ToString());
        }

        public static string GetIdentyfyingQuerySubString_MetaMode(FormalTextLanguageDictinaries dict, IEdge e) // this is used in String2Graph, so we need to reference ZeroCodeCommon.MetaSeparator
        {
            if (VertexOperations.IsToVertexEnoughToIdentifyEdge(e.From, e.To))
                return ZeroCodeCommon.stringToPossiblyEscapedString(dict, e.To.ToString() + ""); // there was no ToString. might cause problems. XXX why this "" as we do not have null To?
            else
                if (VertexOperations.IsMetaAndToVertexEnoughToIdentifyEdge(e.From, e.Meta, e.To))
                return GetQueryStringPart_MetaMode(dict, e.Meta, e.To);
            else
            {
                int pos = 0;
                IList<IEdge> q = GraphUtil.GetQueryOut(e.From, e.Meta.Value, e.To.Value);
                //IVertex q = e.From.GetAll(false, ZeroCodeCommon.stringToPossiblyEscapedString(e.Meta.ToString()) + ZeroCodeCommon.MetaSeparator + ZeroCodeCommon.stringToPossiblyEscapedString(e.To.ToString()));                    

                IVertex tv;
                do
                {
                    tv = q.ElementAt(pos).To;
                    pos++;
                } while (tv != e.To && pos > q.Count);

                return GetQueryStringPart_MetaMode(dict, e.Meta, e.To) + dict.SetIndexPrefix + "\"" + pos + "\"" + dict.SetIndexPostfix;
            }
        }

        public static IVertex GetMostInheritedMeta(IVertex baseVertex, IVertex startMeta)
        {
            IVertex _startMeta = startMeta;

            IVertex highestInheritanceLevel = null;
            int highestInheritanceLevel_level = 0;

            int tempLevel;

            foreach (IEdge e in GraphUtil.GetQueryOut(baseVertex, "$Is", null))
            {
                tempLevel = GetInheritanceLevel(e.To, _startMeta, 0);

                if (tempLevel >= highestInheritanceLevel_level)
                {
                    highestInheritanceLevel_level = tempLevel;
                    highestInheritanceLevel = e.To;
                }
            }

            return highestInheritanceLevel; // if highestInheritanceLevel_level==0 then startMeta was not found
        }

        private static int GetInheritanceLevel(IVertex testMeta, IVertex startMeta, int input)
        {
            if (testMeta == startMeta)
                return input;

            int biggest = 0;

            foreach (IEdge e in GraphUtil.GetQueryOut(testMeta, "$Inherits", false))
            {
                int temp = GetInheritanceLevel(e.To, startMeta, input + 1);
                if (temp > biggest)
                    biggest = temp;
            }

            return biggest;
        }

        public static object GetValue(IVertex vertex)
        {
            if (vertex != null)
                return vertex.Value;
            else
                return null;
        }

        public static string GetStringValue(IVertex vertex)
        {
            if (vertex != null && vertex.Value != null)
                return vertex.Value.ToString();
            else
                return "";
        }

        public static string GetStringValueOrNull(IVertex vertex)
        {
            if (vertex != null && vertex.Value != null)
                return vertex.Value.ToString();
            else
                return null;
        }

        public static bool GetValueAndCompareStrings(IVertex vertex, string s)
        {
            if (vertex == null)
                return false;

            if (vertex.Value != null)
                return vertex.Value.ToString() == s;
            else
            {
                if (s == null)
                    return true;
                else
                    return false;
            }
        }

        public static bool GetValueAndCompareStrings(IVertex vertexLeft, IVertex vertexRight)
        {
            if (vertexLeft == null || vertexRight == null)
                return false;

            if (vertexLeft.Value != null && vertexRight.Value != null)
                return vertexLeft.Value.ToString() == vertexRight.Value.ToString();

            return false;
        }

        public static IVertex SetVertexValue(IVertex vertex, IVertex metaVertex, object value)
        {
            if (vertex == null || metaVertex == null)
                return null;

            IEdge edgeByMeta = GetQueryOut(vertex, metaVertex.Value, null).FirstOrDefault();

            if (edgeByMeta == null)
                return vertex.AddVertex(metaVertex, value);
            else
            {
                if (VertexOperations.IsLink(metaVertex))
                {
                    edgeByMeta.From.DeleteEdge(edgeByMeta);

                    return edgeByMeta.From.AddVertexAndReturnEdge(metaVertex, value).To;
                }
                else
                {
                    edgeByMeta.To.Value = value;
                    return edgeByMeta.To;
                }
            }
        }

        static public void GetNumberValue(IVertex Vertex, out object number)
        {
            number = null;

            if (Vertex == null || Vertex.Value == null)
                return;

            if (Vertex.Value is int)
            {
                number = Vertex.Value;
                return;
            }

            if (Vertex.Value is double)
            {
                number = Vertex.Value;
                return;
            }

            if (Vertex.Value is decimal)
            {
                number = Vertex.Value;
                return;
            }

            if (Vertex.Value is string)
            {
                int _outInt;
                if (Int32.TryParse((string)Vertex.Value, out _outInt))
                {
                    number = _outInt;
                    return;
                }

                double _outDouble;
                if (Double.TryParse((string)Vertex.Value, out _outDouble))
                {
                    number = _outDouble;
                    return;
                }

                decimal _outDecimal;
                if (Decimal.TryParse((string)Vertex.Value, out _outDecimal))
                {
                    number = _outDecimal;
                    return;
                }
            }
        }

        static public T GetNumberValue<T>(IVertex Vertex)
        {
            if (typeof(T) == typeof(int?))
                return (T)(object)GetIntegerValue(Vertex);

            if (typeof(T) == typeof(decimal?))
                return (T)(object)GetDecimalValue(Vertex);

            if (typeof(T) == typeof(double?))
                return (T)(object)GetDoubleValue(Vertex);

            return default(T);
        }

        static public bool IsNullNumber<T>(T Value)
        {
            if (Value == null)
                return true;

            return false;
        }

        static public int? ToInt<T>(T Value)
        {
            if (typeof(T) == typeof(int?))
                return (int?)(object)Value;

            if (typeof(T) == typeof(decimal?))
                return (int?)(decimal?)(object)Value;

            if (typeof(T) == typeof(double?))
                return (int?)(double?)(object)Value;

            return null;
        }

        static public double? ToDouble<T>(T Value)
        {
            if (typeof(T) == typeof(int?))
                return (double?)(int?)(object)Value;

            if (typeof(T) == typeof(decimal?))
                return (double?)(decimal?)(object)Value;

            if (typeof(T) == typeof(double?))
                return (double?)(object)Value;

            return null;
        }

        static public T FromDouble<T>(double? Value)
        {
            if (typeof(T) == typeof(int?))
                return (T)(object)(int?)Value;

            if (typeof(T) == typeof(decimal?))
                return (T)(object)(decimal?)Value;

            if (typeof(T) == typeof(double?))
                return (T)(object)(double?)Value;

            return default(T);
        }

        static public int? GetIntegerValue(IVertex Vertex)
        {
            if (Vertex != null && Vertex.Value != null)
            {
                if (Vertex.Value is int)
                    return (int)Vertex.Value;

                if (Vertex.Value is string)
                {
                    int r;
                    if (Int32.TryParse((string)Vertex.Value, out r))
                        return r;

                    return null; // optimisation
                }

                try
                {
                    return Convert.ToInt32(Vertex.Value);
                }
                catch (Exception e) { }
            }

            return null;
        }

        static public bool GetBooleanValue(IVertex Vertex, ref bool isNull)
        {
            if (Vertex == null || Vertex.Value == null)
            {
                isNull = true;
                return false;
            }

            string val = Vertex.Value.ToString();

            if (val == "True" || val == "true" || val == "1" || val == "1.0")
                return true;

            return false;
        }

        static public bool GetBooleanValueOrFalse(IVertex Vertex)
        {
            if (Vertex == null || Vertex.Value == null)
                return false;

            string val = Vertex.Value.ToString();

            if (val == "True" || val == "true" || val == "1" || val == "1.0")
                return true;

            return false;
        }

        static public int GetIntegerValue(IVertex Vertex, ref bool isNull)
        {
            int? ret = GetIntegerValue(Vertex);

            if (ret == null)
            {
                isNull = true;
                return 0;
            }

            return (int)ret;
        }

        static public int GetIntegerValueOr0(IVertex Vertex)
        {
            int? ret = GetIntegerValue(Vertex);

            if (ret == null)
                return 0;

            return (int)ret;
        }

        static public decimal? GetDecimalValue(IVertex Vertex)
        {
            if (Vertex != null && Vertex.Value != null)
            {
                if (Vertex.Value is decimal)
                    return (decimal)Vertex.Value;

                if (Vertex.Value is string)
                {
                    decimal r;

                    if (Decimal.TryParse((string)Vertex.Value, out r))
                        return r;

                    return null; // optimisation
                }

                try
                {
                    return Convert.ToDecimal(Vertex.Value);
                }
                catch (Exception e) { }
            }
            return null;
        }

        static public decimal GetDecimalValue(IVertex Vertex, ref bool isNull)
        {
            decimal? ret = GetDecimalValue(Vertex);

            if (ret == null)
            {
                isNull = true;
                return 0;
            }

            return (decimal)ret;
        }

        static public decimal GetDecimalValueOr0(IVertex Vertex)
        {
            decimal? ret = GetDecimalValue(Vertex);

            if (ret == null)
                return 0;

            return (decimal)ret;
        }

        static public double? GetDoubleValue(IVertex Vertex)
        {
            if (Vertex != null && Vertex.Value != null)
            {
                if (Vertex.Value is double)
                    return (double)Vertex.Value;

                if (Vertex.Value is string)
                {
                    double r;
                    if (Double.TryParse((string)Vertex.Value, out r))
                        return r;

                    return null; // optimisation
                }

                try
                {
                    return Convert.ToDouble(Vertex.Value);
                }
                catch (Exception e) { }
            }
            return null;
        }

        static public double GetDoubleValue(IVertex Vertex, ref bool isNull)
        {
            double? ret = GetDoubleValue(Vertex);

            if (ret == null)
            {
                isNull = true;
                return 0;
            }

            return (double)ret;
        }

        static public double GetDoubleValueOr0(IVertex Vertex)
        {
            double? ret = GetDoubleValue(Vertex);

            if (ret == null)
                return 0;

            return (double)ret;
        }

        static public void CopyShallow(IVertex source, IVertex destination)
        {
            foreach (IEdge e in source.OutEdgesRaw)
                if (VertexOperations.CanCopyCountViewEdge(e))
                    destination.AddEdge(e.Meta, e.To);
        }

        static public IEdge CopyEdgeIntoVertexOneLevel(IEdge source, IVertex destination)
        {
            IEdge newEdge = destination.AddVertexAndReturnEdge(source.Meta, source.To.Value);

            foreach (IEdge e in source.To)
                if (VertexOperations.CanCopyCountViewEdge(e))
                    newEdge.To.AddVertex(e.Meta, e.To.Value);

            return newEdge;
        }

        static public void RemoveAllEdges(IVertex v)
        {
            IList<IEdge> el = GeneralUtil.CreateAndCopyList<IEdge>(v);

            foreach (IEdge e in el)
                v.DeleteEdge(e);
        }

        static public void RemoveAllEdges_WhereEdgeIsEdge(IVertex v)
        {
            IList<IEdge> el = GeneralUtil.CreateAndCopyList<IEdge>(v);

            foreach (IEdge e in el)
                if (GraphUtil.GetQueryOutCount(e.To, "$Is", "Edge") > 0)
                    v.DeleteEdge(e);

        }

        static public void DeleteEdgeByToVertex(IVertex source, IVertex toVertex)
        {
            IEdge e = FindEdgeByToVertex_fromVertex(source, toVertex);

            if (e != null)
                source.DeleteEdge(e);
        }

        static public void DeleteEdgeByMeta(IVertex source, string MetaValue)
        {
            IEdge e = GetQueryOutFirstEdge(source, MetaValue, null);

            if (e != null)
                source.DeleteEdge(e);
        }

        static public void DeleteEdgesByMeta(IVertex source, string MetaValue)
        {
            IList<IEdge> edges = GetQueryOut(source, MetaValue, null);

            foreach (IEdge e in edges)
                source.DeleteEdge(e);
        }

        static public void DeleteEdge(IVertex source, IVertex metaVertex, IVertex toVertex)
        {
            IEdge e = FindEdge(source, metaVertex, toVertex);

            if (e != null)
                source.DeleteEdge(e);
        }

        static public IEdge FindEdgeByMetaValue(IVertex Vertex, string MetaValue)
        {
            return GetQueryOutFirstEdge(Vertex, MetaValue, null);
        }

        static public IEdge FindEdgeByMetaVertex(IVertex Vertex, IVertex metaVertex)
        {
            return GetQueryOutFirstEdge(Vertex, metaVertex.Value.ToString(), null);
        }

        static public IEdge FindEdge(IVertex Vertex, IVertex metaVertex, IVertex toVertex)
        {
            foreach (IEdge e in GraphUtil.GetQueryOut(Vertex, metaVertex.Value.ToString(), toVertex.Value.ToString()))
                if (e.Meta == metaVertex && e.To == toVertex)
                    return e;
            return null;
        }

        static public IEdge FindEdgeByToVertex(IEnumerable<IEdge> edges, IVertex toVertex)
        {
            foreach (IEdge e in edges)
                if (e.To == toVertex)
                    return e;

            return null;
        }

        static public IEdge FindEdgeByToVertex_fromVertex(IVertex v, IVertex toVertex)
        {
            foreach (IEdge e in GraphUtil.GetQueryOut(v, null, toVertex.Value.ToString()))
                if (e.To == toVertex)
                    return e;

            return null;
        }

        static public IEdge ReplaceEdge(IVertex Vertex, string MetaValue, IVertex NewEdgeToVertex)
        {
            IEdge toReplace = GetQueryOutFirstEdge(Vertex, MetaValue, null);

            if (toReplace == null)
                throw new Exception("Vertex does not have \"" + MetaValue + "\" edge");

            IVertex meta = toReplace.Meta;

            Vertex.DeleteEdge(toReplace);

            return Vertex.AddEdge(meta, NewEdgeToVertex);
        }

        static public IEdge CreateOrReplaceEdge(IVertex Vertex, IVertex metaVertex, IVertex NewEdgeToVertex)
        {
            IEdge toReplace = FindEdgeByMetaVertex(Vertex, metaVertex);

            if (toReplace != null)
                Vertex.DeleteEdge(toReplace);

            return Vertex.AddEdge(metaVertex, NewEdgeToVertex);
        }

        static public IEdge CreateOrReplaceEdge_DeepCopy(IVertex Vertex, IVertex metaVertex, IVertex NewEdgeToVertex)
        {
            IEdge toReplace = FindEdgeByMetaVertex(Vertex, metaVertex);

            if (toReplace != null)
                Vertex.DeleteEdge(toReplace);

            IEdge newEdge = Vertex.AddVertexAndReturnEdge(metaVertex, NewEdgeToVertex.Value);

            DeepCopyByVertex(NewEdgeToVertex, newEdge.To);

            return newEdge;
        }

        static public IEdge ReplaceEdge(IVertex Vertex, IVertex metaVertex, IVertex NewEdgeToVertex)
        {
            IEdge toReplace = FindEdgeByMetaVertex(Vertex, metaVertex);

            if (toReplace == null)
                throw new Exception("Vertex does not have edge of supplied Meta Vertex");

            Vertex.DeleteEdge(toReplace);

            return Vertex.AddEdge(metaVertex, NewEdgeToVertex);
        }

        static public IEdge ReplaceEdge(IVertex Vertex, IEdge toReplace, IVertex NewEdgeToVertex)
        {
            IVertex meta = toReplace.Meta;

            Vertex.DeleteEdge(toReplace);

            return Vertex.AddEdge(meta, NewEdgeToVertex);
        }

        static public IVertex ReplaceEdgeByValue(IVertex Vertex, string MetaValue, object VertexValue)
        {
            IEdge toReplace = FindEdgeByMetaValue(Vertex, MetaValue);

            if (toReplace == null)
                throw new Exception("Vertex does not have \"" + MetaValue + "\" edge");

            IVertex meta = toReplace.Meta;

            Vertex.DeleteEdge(toReplace);

            return Vertex.AddVertex(meta, VertexValue);
        }

        static public IVertex CreateOrReplaceEdgeByValue(IVertex Vertex, IVertex metaVertex, object value)
        {
            IEdge toReplace = FindEdgeByMetaVertex(Vertex, metaVertex);

            Vertex.DeleteEdge(toReplace);

            return Vertex.AddVertex(metaVertex, value);
        }

        static public bool DoIEnumerableIEdgeContainsVertex(IEnumerable<IEdge> baseVertex, IVertex doContainVertex)
        {
            foreach (IEdge e in baseVertex)
                if (e.To == doContainVertex)
                    return true;

            return false;
        }

        static public IVertex DeepFindOneByValue(IVertex findRoot, string value, bool canGoIntoLinks)
        {
            GraphIterator i = new GraphIterator(value);

            return DeepIterator(findRoot, i.Compare, true, false, canGoIntoLinks).FirstOrDefault();
        }

        static public IVertex DeepFindOneByMeta(IVertex findRoot, string value, bool canGoIntoLinks)
        {
            GraphIterator i = new GraphIterator(value);

            return DeepIterator(findRoot, i.CompareMeta, true, false, canGoIntoLinks).FirstOrDefault();
        }

        static public IEnumerable<IVertex> DeepIterator(IVertex iterationRoot, GraphIteratorIterate iterate, bool isSingleResult, bool canModifyOutEdges, bool canGoIntoLinks)
        {
            HashSet<IVertex> visited = new HashSet<IVertex>();

            List<IVertex> returnList = new List<IVertex>();

            DeepIterator_Reccurent(iterationRoot, iterate, visited, returnList, isSingleResult, canModifyOutEdges, canGoIntoLinks);

            return returnList;
        }

        static bool DeepIterator_Reccurent(IVertex iterationRoot, GraphIteratorIterate iterate, HashSet<IVertex> visited, List<IVertex> returnList, bool isSingleResult, bool canModifyOutEdges, bool canGoIntoLinks)
        {
            bool toReturn = false;

            IEnumerable<IEdge> outEdges;

            if (canModifyOutEdges)
                outEdges = iterationRoot.OutEdges.ToList();
            else
                outEdges = iterationRoot.OutEdges;

            //foreach (IEdge e in iterationRoot.OutEdges)
            foreach (IEdge e in outEdges)
            {
                if (iterate(e))
                {
                    returnList.Add(e.To);
                    if (isSingleResult)
                        return true;
                }

                if (!visited.Contains(e.To) && (canGoIntoLinks || !VertexOperations.IsLink(e))) // this canGoIntoLinks looks bad, should be canGoIntoLinks XXX TO BE TESTED
                {
                    visited.Add(e.To);

                    if (DeepIterator_Reccurent(e.To, iterate, visited, returnList, isSingleResult, canModifyOutEdges, canGoIntoLinks))
                    {
                        toReturn = true;

                        break;
                    }
                }
            }

            return toReturn;
        }

        static public IEnumerable<IVertex> DeepIterator_OldVersion(IVertex iterationRoot, GraphIteratorIterate iterate, bool isSingleResult, bool canModifyOutEdges, bool canGoIntoLinks)
        {
            HashSet<IVertex> visited = new HashSet<IVertex>();

            List<IVertex> returnList = new List<IVertex>();

            DeepIterator_Reccurent_OldVersion(iterationRoot, iterate, visited, returnList, isSingleResult, canModifyOutEdges, canGoIntoLinks);

            return returnList;
        }

        static bool DeepIterator_Reccurent_OldVersion(IVertex iterationRoot, GraphIteratorIterate iterate, HashSet<IVertex> visited, List<IVertex> returnList, bool isSingleResult, bool canModifyOutEdges, bool canGoIntoLinks)
        {
            bool toReturn = false;

            IEnumerable<IEdge> outEdges;

            if (canModifyOutEdges)
                outEdges = iterationRoot.OutEdges.ToList();
            else
                outEdges = iterationRoot.OutEdges;

            //foreach (IEdge e in iterationRoot.OutEdges)
            foreach (IEdge e in outEdges)
            {
                if (iterate(e))
                {
                    returnList.Add(e.To);
                    if (isSingleResult)
                        return true;
                }

                if (!visited.Contains(e.To) && (canGoIntoLinks || !VertexOperations.IsLink_OldVersion(e))) // this canGoIntoLinks looks bad, should be canGoIntoLinks XXX TO BE TESTED
                {
                    visited.Add(e.To);

                    if (DeepIterator_Reccurent_OldVersion(e.To, iterate, visited, returnList, isSingleResult, canModifyOutEdges, canGoIntoLinks))
                    {
                        toReturn = true;

                        break;
                    }
                }
            }

            return toReturn;
        }

        static public void DeepCopy(IEdge edgeToCopy, IVertex copyTo)
        {
            HashSet<IVertex> visited = new HashSet<IVertex>();

            DeepCopy_Reccurent(edgeToCopy, copyTo, visited);
        }

        static void DeepCopy_Reccurent(IEdge edgeToCopy, IVertex copyTo, HashSet<IVertex> visited)
        {
            visited.Add(edgeToCopy.To);

            IVertex newVertex = copyTo.AddVertex(edgeToCopy.Meta, edgeToCopy.To.Value);

            foreach (IEdge e in edgeToCopy.To.OutEdgesRaw)
                if (!visited.Contains(e.To) && !VertexOperations.IsLink(e))
                    DeepCopy_Reccurent(e, newVertex, visited);
                else
                    newVertex.AddEdge(e.Meta, e.To);
        }

        static public void DeepCopyByVertex(IVertex vertexToCopy, IVertex copyTo)
        {
            HashSet<IVertex> visited = new HashSet<IVertex>();

            DeepCopyByVertex_Reccurent(vertexToCopy, copyTo, visited);
        }

        static void DeepCopyByVertex_Reccurent(IVertex vertexToCopy, IVertex copyTo, HashSet<IVertex> visited)
        {
            copyTo.Value = vertexToCopy.Value;

            visited.Add(vertexToCopy);

            foreach (IEdge e in vertexToCopy.OutEdgesRaw)
                if (VertexOperations.CanCopyCountViewEdge(e))
                {
                    if (!visited.Contains(e.To) && !VertexOperations.IsLink(e))
                    {
                        IVertex newVertex = copyTo.AddVertex(e.Meta, null);

                        DeepCopyByVertex_Reccurent(e.To, newVertex, visited);
                    }
                    else
                        copyTo.AddEdge(e.Meta, e.To);
                }
        }

        static public IEnumerable<IVertex> GetSubGraphWithoutLinksAsList(IVertex iterationRoot)
        {
            HashSet<IVertex> visited = new HashSet<IVertex>();

            GetSubGraphWithoutLinks_Reccurent(iterationRoot, visited);

            return visited;
        }

        static void GetSubGraphWithoutLinks_Reccurent(IVertex baseVertex, HashSet<IVertex> visited)
        {
            visited.Add(baseVertex);

            foreach (IEdge e in baseVertex.OutEdgesRaw)
                if (!visited.Contains(e.To) && !VertexOperations.IsLink(e))
                    GetSubGraphWithoutLinks_Reccurent(e.To, visited);
        }

        static public List<IEdge> GetSubGraphAsEdgesWithoutLinksAsList(IEdge iterationRoot)
        {
            HashSet<IVertex> visited = new HashSet<IVertex>();

            List<IEdge> edges = new List<IEdge>();

            if (!VertexOperations.IsLink(iterationRoot))
                GetSubGraphAsEdgesWithoutLinks_Reccurent(iterationRoot, visited, edges);
            else
                edges.Add(iterationRoot);

            return edges;
        }

        static void GetSubGraphAsEdgesWithoutLinks_Reccurent(IEdge baseEdge, HashSet<IVertex> visited, List<IEdge> edges)
        {
            edges.Add(baseEdge);

            if (!visited.Contains(baseEdge.To) && !VertexOperations.IsLink(baseEdge))
            {
                visited.Add(baseEdge.To);

                foreach (IEdge e in baseEdge.To.OutEdgesRaw)
                    GetSubGraphAsEdgesWithoutLinks_Reccurent(e, visited, edges);

            }
        }

        static public IEnumerable<IVertex> GetSubGraphWithLinksAsListButExcludeRoot(IVertex iterationRoot)
        {
            HashSet<IVertex> visited = new HashSet<IVertex>();

            GetSubGraphWithLinksButExcludeRoot_Reccurent(iterationRoot, visited);

            return visited;
        }

        static void GetSubGraphWithLinksButExcludeRoot_Reccurent(IVertex baseVertex, HashSet<IVertex> visited)
        {
            visited.Add(baseVertex);

            foreach (IEdge e in baseVertex.OutEdgesRaw)
                if (!visited.Contains(e.To) && e.To != MinusZero.Instance.root)
                    GetSubGraphWithLinksButExcludeRoot_Reccurent(e.To, visited);
        }

        static public IEnumerable<IVertex> GetSubGraphWithLinksAsListButExcludeList(IVertex iterationRoot, HashSet<IVertex> excludeList)
        {
            HashSet<IVertex> visited = new HashSet<IVertex>();

            GetSubGraph_Reccurent_ExcludeList(iterationRoot, visited, excludeList);

            return visited;
        }

        static void GetSubGraph_Reccurent_ExcludeList(IVertex baseVertex, HashSet<IVertex> visited, HashSet<IVertex> excludeList)
        {
            visited.Add(baseVertex);

            foreach (IEdge e in baseVertex.OutEdgesRaw)
                if (!visited.Contains(e.To) && !excludeList.Contains(e.To))
                    GetSubGraph_Reccurent_ExcludeList(e.To, visited, excludeList);
        }


        static public IEnumerable<IVertex> GetSubGraphWithoutLinksAsListButExcludeList(IVertex iterationRoot, HashSet<IVertex> excludeList)
        {
            HashSet<IVertex> visited = new HashSet<IVertex>();

            GetSubGraphWithoutLinks_Reccurent_ExcludeList(iterationRoot, visited, excludeList);

            return visited;
        }

        static void GetSubGraphWithoutLinks_Reccurent_ExcludeList(IVertex baseVertex, HashSet<IVertex> visited, HashSet<IVertex> excludeList)
        {
            visited.Add(baseVertex);

            foreach (IEdge e in baseVertex.OutEdgesRaw)
                if (!visited.Contains(e.To) && !excludeList.Contains(e.To) && !VertexOperations.IsLink(e))
                    GetSubGraphWithoutLinks_Reccurent_ExcludeList(e.To, visited, excludeList);
        }

        public static IVertex GetVertex(string storeName, long id)
        {
            MemoryStore store = (MemoryStore)MinusZero.Instance.GetStore(storeName);

            if (store.VertexIdentifiersDictionary.ContainsKey(id))
                return store.GetVertexByIdentifier(id);
            else
                return null;
        }

        public static string GetQueryBetweenVertexes_byInEdges(IVertex from, IVertex to)
        {
            if (from == null || to == null)
                throw new ArgumentNullException();

            if (from == to)
                return "";

            var visited = new HashSet<IVertex>();
            var parentEdge = new Dictionary<IVertex, IEdge>();

            var queue = new Queue<IVertex>();

            visited.Add(from);
            parentEdge[from] = null;
            queue.Enqueue(from);

            // --- BFS ---
            while (queue.Count > 0)
            {
                var v = queue.Dequeue();

                if (v == to)
                    break;

                foreach (IEdge e in v.InEdges)
                {
                    IVertex eFrom = e.From;

                    if (!visited.Contains(eFrom))
                    {
                        visited.Add(eFrom);
                        parentEdge[eFrom] = e;
                        queue.Enqueue(eFrom);
                    }
                }

                int x = 0;
            }

            if (!parentEdge.ContainsKey(to))
                return null;

            StringBuilder sb = new StringBuilder();
            bool isFirst = true;

            for (IEdge e = parentEdge[to]; e != null; e = parentEdge[e.To])
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(@"\");

                if (e.Meta != MinusZero.Instance.Empty)
                    sb.Append(e.Meta.ToString() + ":" + e.To.ToString());
                else
                    sb.Append(":" + e.To.ToString());
            }

            return sb.ToString();
        }
    }
}
