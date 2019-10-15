using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using m0.Foundation;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroCode;

namespace m0.Graph
{
    public delegate bool GraphIteratorIterate(IEdge vertex);

    public class GraphIterator
    {
        object Value;

        INoInEdgeInOutVertexVertex ValueAsINoInEdgeInOutVertexVertex;

        public GraphIterator(object value)
        {
            Value = value;

            if (value is INoInEdgeInOutVertexVertex)
                ValueAsINoInEdgeInOutVertexVertex = (INoInEdgeInOutVertexVertex)value;           
        }

        public bool Compare(IEdge vertex)
        {
            if (GeneralUtil.CompareStrings(Value,vertex.To.Value))
                return true;

            return false;
        }

        public bool CompareMeta(IEdge vertex)
        {
            if (GeneralUtil.CompareStrings(Value,vertex.Meta.Value))
                return true;

            return false;
        }

        public bool AddToINoInEdgeInOutVertexVertex(IEdge vertex)
        {
            if (ValueAsINoInEdgeInOutVertexVertex != null)
                ValueAsINoInEdgeInOutVertexVertex.AddEdgeForNoInEdgeInOutVertexVertex(vertex);            

            return true;
        }
    }

    public class GraphUtil
    {    
        public static IEdge CreateArtificialEdge(IVertex meta, IVertex to)
        {
            //EasyEdge e = new EasyEdge(null, meta, to);

            NoInEdgeInOutVertexEdge e = new NoInEdgeInOutVertexEdge(null, meta, to);

            return e;
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
                return result.To;

            if (results != null && results.Count > 0)
                return results.First().To;

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

        public static object GetMetaAndValueObject(object meta, object value)
        {
            /*StringBuilder sb = new StringBuilder();
            if (meta != null)
                sb.Append(meta.ToString());

            sb.Append("@#$#@");

            if (value != null)
                sb.Append(value.ToString());            

            return sb.ToString();*/

            int toRet = 0;
            if (meta != null)
                toRet = meta.GetHashCode();


            if (value != null)
                toRet += value.GetHashCode();

            return toRet;
        }
        public static HashSet<IVertex> GetInheritChilds_RawEnumerate(IVertex baseVertex)
        {
            HashSet<IVertex> inheritsSet = new HashSet<IVertex>();

            GetInheritChilds_RawEnumerate_recurrent(baseVertex, inheritsSet);

            return inheritsSet;
        }

        private static void GetInheritChilds_RawEnumerate_recurrent(IVertex baseVertex, HashSet<IVertex> inheritedSet)
        {
            foreach (IEdge e in baseVertex.InEdgesRaw)
                if (GeneralUtil.CompareStrings(e.Meta, "$Inherits") && !inheritedSet.Contains(e.From))
                {
                    inheritedSet.Add(e.From);
                    GetInheritChilds_RawEnumerate_recurrent(e.From, inheritedSet);
                }
        }

        public static HashSet<IVertex> GetInheritParents_RawEnumerate(IVertex baseVertex)
        {
            HashSet<IVertex> inheritsSet = new HashSet<IVertex>();

            GetInheritParents_RawEnumerate_recurrent(baseVertex, inheritsSet);

            return inheritsSet;
        }

        private static void GetInheritParents_RawEnumerate_recurrent(IVertex baseVertex, HashSet<IVertex> inheritedSet)
        {
            foreach (IEdge e in baseVertex.OutEdgesRaw)
                if (GeneralUtil.CompareStrings(e.Meta, "$Inherits") && !inheritedSet.Contains(e.To))
                {
                    inheritedSet.Add(e.To);
                    GetInheritParents_RawEnumerate_recurrent(e.To, inheritedSet);
                }
        }

        public static string GetQueryStringPart(IVertex meta, IVertex to)
        {
            if (GeneralUtil.CompareStrings(meta.ToString(), "$Empty"))
                return ZeroCodeCommon.stringToPossiblyEscapedString(to.ToString());
            else
                return ZeroCodeCommon.stringToPossiblyEscapedString(meta.ToString()) + ZeroCodeCommon.MetaSeparator + ZeroCodeCommon.stringToPossiblyEscapedString(to.ToString());
        }

        public static string GetIdentyfyingQuerySubString_ImportMeta(IEdge e) // this is used in String2Graph, so we need to reference ZeroCodeCommon.MetaSeparator
        {
            if (VertexOperations.IsToVertexEnoughToIdentifyEdge(e.From, e.To))
                return ZeroCodeCommon.stringToPossiblyEscapedString(e.To.ToString()+""); // there was no ToString. might cause problems. XXX why this "" as we do not have null To?
            else
                if (VertexOperations.IsMetaAndToVertexEnoughToIdentifyEdge(e.From, e.Meta, e.To))
                    return GetQueryStringPart(e.Meta, e.To);
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
                    } while (tv != e.To);

                    return GetQueryStringPart(e.Meta,e.To) + "<" + pos + ">"; 
                }
        }

        public static IVertex GetMostInheritedMeta(IVertex baseVertex, IVertex startMeta)
        {
            IVertex _startMeta = startMeta;

           // if (startMeta.Get(false, "$EdgeTarget") != null) // XXX this is error!!!!! but before correcting it, we need to check what will happen
            //    _startMeta = startMeta.Get(false, "$EdgeTarget:");

            IVertex highestInheritanceLevel=null;
            int highestInheritanceLevel_level = 0;

            int tempLevel;

            foreach (IEdge e in GraphUtil.GetQueryOut(baseVertex,"$Is",null))
            //foreach (IEdge e in baseVertex.GetAll(false, "$Is:"))
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

            foreach(IEdge e in GraphUtil.GetQueryOut(testMeta, "$Inherits", false))
            //foreach (IEdge e in testMeta.GetAll(false, "$Inherits:"))
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

            if (vertexLeft.Value != null && vertexRight.Value!=null)
                return vertexLeft.Value.ToString() == vertexRight.Value.ToString();

            return false;
        }

        public static bool IsEqual(IVertex leftVertex, IVertex rightVertex)
        {

            return false;
        }

        public static IVertex SetVertexValue(IVertex vertex, IVertex metaVertex, object value)
        {
            //IVertex getByMeta=vertex.Get(false, metaVertex.Value + ":");
            IVertex getByMeta = GetQueryOutFirst(vertex, metaVertex.Value, null);                

            if (getByMeta == null)
                return vertex.AddVertex(metaVertex, value);
            else
            {
                getByMeta.Value = value;
                return getByMeta;                
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
            if (Vertex!=null&&Vertex.Value!=null)
                {
                if (Vertex.Value is int)
                    return (int)Vertex.Value;

                if (Vertex.Value is string)
                    {
                        int r;
                        if (Int32.TryParse((string)Vertex.Value, out r))
                            return r;

                        return null;
                    }                    
                }
            return null;
        }

        static public decimal? GetDecimalValue(IVertex Vertex)
        {
            if (Vertex != null && Vertex.Value != null)
            {
                if (Vertex.Value is decimal)
                    return (decimal)Vertex.Value;

                if (Vertex.Value is string)
                    return Decimal.Parse((string)Vertex.Value);                
            }
            return null;
        }
        
        static public double? GetDoubleValue(IVertex Vertex)
        {
            if (Vertex != null && Vertex.Value != null)
            {
                if (Vertex.Value is double)
                    return (double)Vertex.Value;

                if (Vertex.Value is string)
                    return Double.Parse((string)Vertex.Value);                
            }
            return null;
        }        

        static public void CopyEdges(IVertex source, IVertex destination)
        {
            foreach (IEdge e in source)
                destination.AddEdge(e.Meta, e.To);
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
            {
                RemoveAllEdges(e.To);

                v.DeleteEdge(e);
            }
        }

        static public void DeleteEdgeByToVertex(IVertex source, IVertex toVertex)
        {
            IEdge e = FindEdgeByToVertex(source, toVertex);

            if(e!=null)
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

            foreach(IEdge e in edges)            
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
            foreach (IEdge e in Vertex)
                if (e.Meta == metaVertex)
                    return e;
            return null;
        }

        static public IEdge FindEdge(IVertex Vertex, IVertex metaVertex, IVertex toVertex)
        {
            foreach (IEdge e in Vertex)
                if (e.Meta == metaVertex&&e.To==toVertex)
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

        static public IEdge FindEdgeByToVertex(IEnumerable<IEdge> edges, string toVertexValue)
        {
            foreach (IEdge e in edges)
                if (GeneralUtil.CompareStrings(e.To.Value,toVertexValue))
                    return e;

            return null;
        }

        static public IEdge ReplaceEdge(IVertex Vertex, string MetaValue, IVertex NewEdgeToVertex)
        {
            //IEdge toReplace = FindEdgeByMetaValue(Vertex, MetaValue);
            IEdge toReplace = GetQueryOutFirstEdge(Vertex, MetaValue, null);

            if (toReplace == null)
                throw new Exception("Vertex does not have \"" + MetaValue + "\" edge");

            Vertex.DeleteEdge(toReplace);                       

            return Vertex.AddEdge(toReplace.Meta,NewEdgeToVertex);            
        }

        static public IEdge CreateOrReplaceEdge(IVertex Vertex, IVertex metaVertex, IVertex NewEdgeToVertex)
        {
            IEdge toReplace = FindEdgeByMetaVertex(Vertex, metaVertex);

            if (toReplace != null)
                Vertex.DeleteEdge(toReplace);

            return Vertex.AddEdge(metaVertex, NewEdgeToVertex);
        }

        static public IEdge ReplaceEdge(IVertex Vertex, IVertex metaVertex, IVertex NewEdgeToVertex)
        {
            IEdge toReplace = FindEdgeByMetaVertex(Vertex, metaVertex);

            if (toReplace == null)
                throw new Exception("Vertex does not have edge of supplied Meta Vertex");

            Vertex.DeleteEdge(toReplace);

            return Vertex.AddEdge(toReplace.Meta, NewEdgeToVertex);
        }

        static public IEdge ReplaceEdge(IVertex Vertex, IEdge toReplace, IVertex NewEdgeToVertex)
        {
            Vertex.DeleteEdge(toReplace);

            return Vertex.AddEdge(toReplace.Meta, NewEdgeToVertex);
        }

        static public IVertex ReplaceEdgeByValue(IVertex Vertex, string MetaValue, object VertexValue){
            IEdge toReplace = FindEdgeByMetaValue(Vertex, MetaValue);

            if (toReplace == null)
                throw new Exception("Vertex does not have \"" + MetaValue + "\" edge");

            Vertex.DeleteEdge(toReplace);   

            IVertex nv=Vertex.AddVertex(toReplace.Meta, VertexValue);            

            return nv;
        }
       
        static public bool DoIEnumerableIEdgeContainsVertex(IEnumerable<IEdge> baseVertex, IVertex doContainVertex)
        {
            foreach (IEdge e in baseVertex)
                if (e.To == doContainVertex)
                    return true;

            return false;
        }     

        static public IVertex FindOneByValue(IVertex findRoot, string value)
        {
            return GetQueryOutFirst(findRoot, null, value);
            /*foreach (IEdge e in findRoot)
                if (GeneralUtil.CompareStrings(e.To.Value, value))
                    return e.To;

            return null;*/
        }

        static public IVertex FindOneByMeta(IVertex findRoot, string value)
        {
            return GetQueryOutFirst(findRoot, value, null);

            /*foreach (IEdge e in findRoot)
                if (GeneralUtil.CompareStrings(e.Meta.Value, value))
                    return e.To;

            return null;            */
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
            List<IVertex> visited = new List<IVertex>();

            List<IVertex> returnList = new List<IVertex>();

            DeepIterator_Reccurent(iterationRoot, iterate, visited, returnList, isSingleResult, canModifyOutEdges, canGoIntoLinks);

            return returnList;
        }

        static bool DeepIterator_Reccurent(IVertex iterationRoot, GraphIteratorIterate iterate, List<IVertex> visited, List<IVertex> returnList, bool isSingleResult, bool canModifyOutEdges, bool canGoIntoLinks)
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

        static public void DeepCopy(IEdge edgeToCopy, IVertex copyTo)
        {
            List<IVertex> visited = new List<IVertex>();            

            DeepCopy_Reccurent(edgeToCopy, copyTo, visited);            
        }

        static void DeepCopy_Reccurent(IEdge edgeToCopy, IVertex copyTo, List<IVertex> visited)
        {
            visited.Add(edgeToCopy.To);

            IVertex newVertex = copyTo.AddVertex(edgeToCopy.Meta, edgeToCopy.To.Value);

            foreach (IEdge e in edgeToCopy.To)
                if (!visited.Contains(e.To) && !VertexOperations.IsLink(e))
                    DeepCopy_Reccurent(e, newVertex, visited);
                else
                    newVertex.AddEdge(e.Meta, e.To);                      
        }

        static public IEnumerable<IVertex> GetSubGraph(IVertex iterationRoot)
        {
            List<IVertex> visited = new List<IVertex>();            

            GetSubGraph_Reccurent(iterationRoot, visited);

            return visited;
        }

        static void GetSubGraph_Reccurent(IVertex baseVertex, List<IVertex> visited)
        {
            visited.Add(baseVertex);

            foreach (IEdge e in baseVertex.OutEdges)
                if (!visited.Contains(e.To) && !VertexOperations.IsLink(e))
                        GetSubGraph_Reccurent(e.To, visited);                           
        }

        public static void AddHandlerIfDelegateListDoesNotContainsIt(IVertex baseVertex, VertexChange _delegate)
        {
            if (baseVertex!=null&& !GeneralUtil.DoDelegateListContainDelegate(baseVertex.GetChangeDelegateInvocationList(), _delegate))
                baseVertex.Change += _delegate;
        }
        
    }
}
