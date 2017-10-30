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

        public GraphIterator(object value)
        {
            Value = value;
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
    }

    public class GraphUtil
    {

        public static string GetIdentyfyingQuerySubString_ImportMeta(IEdge e)
        {
            if (VertexOperations.IsToVertexEnoughToIdentifyEdge(e.From, e.To))
                return ZeroCodeCommon.stringToPossiblyEscapedString(e.To.ToString()+""); // there was no ToString. might cause problems
            else
                if (VertexOperations.IsMetaAndToVertexEnoughToIdentifyEdge(e.From, e.Meta, e.To))
                    return ZeroCodeCommon.stringToPossiblyEscapedString(e.Meta.ToString()) + ":" + ZeroCodeCommon.stringToPossiblyEscapedString(e.To.ToString());
                else
                {
                    int pos = 0;
                    IVertex q = e.From.GetAll(ZeroCodeCommon.stringToPossiblyEscapedString(e.Meta.ToString()) + ":" + ZeroCodeCommon.stringToPossiblyEscapedString(e.To.ToString()));

                    IVertex tv;
                    do
                    {
                        tv = q.ElementAt(pos).To;
                        pos++;
                    } while (tv != e.To);

                    return ZeroCodeCommon.stringToPossiblyEscapedString(e.Meta.ToString()) + ":" + ZeroCodeCommon.stringToPossiblyEscapedString(e.To.ToString()) + "|" + pos ;
                }
        }

        public static IVertex GetMostInheritedMeta(IVertex baseVertex, IVertex startMeta)
        {
            IVertex _startMeta = startMeta;

            if (startMeta.Get("$EdgeTarget") != null)
                _startMeta = startMeta.Get("$EdgeTarget:");

            IVertex highestInheritanceLevel=null;
            int highestInheritanceLevel_level = 0;

            int tempLevel;

            foreach (IEdge e in baseVertex.GetAll("$Is:"))
            {
                tempLevel = getInheritanceLevel(e.To, _startMeta, 0);

                if (tempLevel >= highestInheritanceLevel_level)
                {
                    highestInheritanceLevel_level = tempLevel;
                    highestInheritanceLevel = e.To;
                }
            }

            return highestInheritanceLevel; // if highestInheritanceLevel_level==0 then startMeta was not found
        }

        private static int getInheritanceLevel(IVertex testMeta, IVertex startMeta, int input)
        {
            if (testMeta == startMeta)
                return input;

            int biggest = 0;

            foreach (IEdge e in testMeta.GetAll("$Inherits:"))
            {
                int temp = getInheritanceLevel(e.To, startMeta, input + 1);
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

        public static IVertex SetVertexValue(IVertex vertex, IVertex metaVertex, object value)
        {
            IVertex getByMeta=vertex.Get(metaVertex.Value + ":");

            if (getByMeta == null)
                return vertex.AddVertex(metaVertex, value);
            else
            {
                getByMeta.Value = value;
                return getByMeta;                
            }            
        }

        static public T GetNumberValue<T>(IVertex Vertex)
        {
            T test=default(T);

            if (test is int)
                return (T)(object)GetIntegerValue(Vertex);

            if (test is decimal)
                return (T)(object)GetDecimalValue(Vertex);

            if (test is double)
                return (T)(object)GetDoubleValue(Vertex);

            return default(T);
        }

        static public bool IsNullNumber<T>(T Value)
        {
            T test = default(T);

            if (test is int)
                if ((int)(object)Value == NullInt)
                    return true;

            if (test is decimal)
                if ((decimal)(object)Value == NullDecimal)
                    return true;

            if (test is double)
                if ((double)(object)Value == NullDouble)
                    return true;

            return false;
        }

        static public int ToInt<T>(T Value)
        {
            T test = default(T);

            if (test is int)
                return (int)(object)Value;

            if (test is decimal)
                return (int)(decimal)(object)Value;

            if (test is double)
                return (int)(double)(object)Value;

            return NullInt;
        }

        static public double ToDouble<T>(T Value)
        {
            T test = default(T);

            if (test is int)
                return (double)(int)(object)Value;

            if (test is decimal)
                return (double)(decimal)(object)Value;

            if (test is double)
                return (double)(object)Value;

            return NullDouble;
        }

        static public T FromDouble<T>(double Value)
        {
            T test = default(T);

            if (test is int)
                return (T)(object)(int)Value;

            if (test is decimal)
                return (T)(object)(decimal)Value;

            if (test is double)
                return (T)(object)(double)Value;

            return test;
        }

        static public int NullInt = -99999;

        static public int GetIntegerValue(IVertex Vertex)
        {
            if (Vertex!=null&&Vertex.Value!=null)
                {
                    if (Vertex.Value is string)
                    {
                        int r;
                        if (Int32.TryParse((string)Vertex.Value, out r))
                            return r;

                        return NullInt;
                    }

                    if (Vertex.Value is int)
                        return (int)Vertex.Value;                      
                }
            return NullInt;
        }

        static public decimal NullDecimal = -99999;

        static public decimal GetDecimalValue(IVertex Vertex)
        {
            if (Vertex != null && Vertex.Value != null)
            {
                if (Vertex.Value is string)
                    return Decimal.Parse((string)Vertex.Value);

                if (Vertex.Value is decimal)
                    return (decimal)Vertex.Value;
            }
            return NullDecimal;
        }

        static public double NullDouble = -99999;

        static public double GetDoubleValue(IVertex Vertex)
        {
            if (Vertex != null && Vertex.Value != null)
            {
                if (Vertex.Value is string)
                    return Double.Parse((string)Vertex.Value);

                if (Vertex.Value is double)
                    return (double)Vertex.Value;
            }
            return NullDouble;
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

        static public void DeleteEdgeByToVertex(IVertex source, IVertex toVertex)
        {
            IEdge e = FindEdgeByToVertex(source, toVertex);

            if(e!=null)
                source.DeleteEdge(e);
        }

        static public void DeleteEdgeByMeta(IVertex source, string MetaValue)
        {
            IEdge e = FindEdgeByMetaValue(source, MetaValue);

            if (e != null)
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
            foreach (IEdge e in Vertex)
                if (GeneralUtil.CompareStrings(e.Meta.Value, MetaValue))
                    return e;
            return null;
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
            IEdge toReplace = FindEdgeByMetaValue(Vertex, MetaValue);

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
            foreach (IEdge e in findRoot)
                if (GeneralUtil.CompareStrings(e.To.Value, value))
                    return e.To;

            return null;
        }

        static public IVertex FindOneByMeta(IVertex findRoot, string value)
        {
            foreach (IEdge e in findRoot)
                if (GeneralUtil.CompareStrings(e.Meta.Value, value))
                    return e.To;

            return null;            
        }

        static public IVertex DeepFindOneByValue(IVertex findRoot, string value)
        {
            GraphIterator i = new GraphIterator(value);

            return DeepIterator(findRoot, i.Compare,true).FirstOrDefault();
        }

        static public IVertex DeepFindOneByMeta(IVertex findRoot, string value)
        {
            GraphIterator i = new GraphIterator(value);

            return DeepIterator(findRoot, i.CompareMeta,true).FirstOrDefault();
        }        

        static public IEnumerable<IVertex> DeepIterator(IVertex iterationRoot, GraphIteratorIterate iterate, bool isSingleResult)
        {
            List<IVertex> visited = new List<IVertex>();

            List<IVertex> returnList = new List<IVertex>();

            DeepIterator_Reccurent(iterationRoot, iterate, visited, returnList, isSingleResult);

            return returnList;
        }

        static bool DeepIterator_Reccurent(IVertex iterationRoot, GraphIteratorIterate iterate, List<IVertex> visited, List<IVertex> returnList, bool isSingleResult)
        {
            bool toReturn = false;

            foreach (IEdge e in iterationRoot.OutEdges)
            {
                if (!visited.Contains(e.To))
                {
                    visited.Add(e.To);
                    if (iterate(e))
                    {
                        returnList.Add(e.To);
                        if (isSingleResult)
                            return true;
                    }

                    if (DeepIterator_Reccurent(e.To, iterate, visited, returnList, isSingleResult))
                    {
                        toReturn = true;

                        break;
                    }
                }                
            }

            return toReturn;
        }

        public static void AddHandlerIfDelegateListDoesNotContainsIt(IVertex baseVertex, VertexChange _delegate)
        {
            if (baseVertex!=null&& !GeneralUtil.DoDelegateListContainDelegate(baseVertex.GetChangeDelegateInvocationList(), _delegate))
                baseVertex.Change += _delegate;
        }
        
    }
}
