using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using m0.TextLanguage;
using m0.Foundation;
using m0.TextLanguage.GoldParser;
using m0.Graph;
using m0.Util;
using m0.ZeroTypes;

namespace m0.ZeroCode
{
    public class ZeroCodeEngine_OLD:IParser, IExecuter
    {
        private GoldGenericParser Parser;

        private IVertex ZeroCodeTerminals;

        public bool GenerateWhiteSpaces { get; set; }

        public bool AlwaysGenerateQuotas { get; set; }        

 #region Parse

        public IVertex Parse(Foundation.IVertex rootVertex, string text)
        {
            return Parser.Parse(rootVertex, text);
        }

 #endregion

 #region Execute

        private string DequoteString(string s)
        {
            // DONE IN PARSER
           // if (s[0] == '"') 
             //   return s.Substring(1, s.Length - 2);

            return s;
        }
        
        public IVertex Execute(IVertex baseVertex, IVertex inputVertex, IVertex expression)
        {
            foreach (IEdge e in expression)
            {
                if(GeneralUtil.CompareStrings(e.Meta.Value,"$Empty"))
                    baseVertex.Value = DequoteString((string)e.To.Value);

                Execute_exec(e.To, baseVertex, baseVertex, inputVertex,0, false);
            }

            return null;
        }

        private void Execute_exec(IVertex v, IVertex baseVertex, IVertex current, IVertex inputVertex, int skip, bool wasPreviousTokenComa)
        {
            int cnt = 0;

            foreach (IEdge e in v)
            {
                if (skip > 0)
                    skip--;
                else
                {
                    cnt++;

                    if (GeneralUtil.CompareStrings(e.Meta.Value, "$Empty"))                    
                        current = baseVertex.AddVertex(null, DequoteString((string)e.To.Value));                                            

                    if (GeneralUtil.CompareStrings(e.Meta.Value, "$EmptyContainerTerminal"))                    
                        current = baseVertex.AddVertex(null, "");                        

                    if (GeneralUtil.CompareStrings(e.Meta.Value,":"))
                    {
                        current = Execute_colon(e.To, baseVertex, inputVertex);                        
                    }
                    else if (GeneralUtil.CompareStrings(e.Meta.Value,"{"))
                    {
                        baseVertex = current;

                        Execute_exec(e.To, baseVertex, current, inputVertex, 0, false);
                    }
                    else if (GeneralUtil.CompareStrings(e.Meta.Value, ","))
                    {                        
                        if(wasPreviousTokenComa&&cnt==1)
                            current = baseVertex.AddVertex(null, "");                        

                        Execute_exec(e.To, baseVertex, current, inputVertex, 0, true);
                    }else
                        Execute_exec(e.To, baseVertex, current, inputVertex, 0, false);

                }
            }
        }


        private IVertex Execute_colon(IVertex v, IVertex baseVertex, IVertex inputVertex)
        {
            IList<IEdge> el = v.OutEdges.ToList();

            if (v.OutEdges.Count() > 1 && GeneralUtil.CompareStrings(el[0].Meta.Value, "$Empty") && GeneralUtil.CompareStrings(el[1].Meta.Value, "$Empty"))
            {                
                IVertex ret=baseVertex.AddVertex(FindMetaVertex(inputVertex,el[0].To.Value), DequoteString((string)el[1].To.Value));

                Execute_exec(v, baseVertex, ret, inputVertex, 2, false);

                return ret;
            }
            else
            {
                IVertex ret=baseVertex.AddVertex(FindMetaVertex(inputVertex, el[0].To.Value), "");

                Execute_exec(v, baseVertex, ret, inputVertex, 1, false);

                return ret;
            }
        }

        private IVertex FindMetaVertex(IVertex inputVertex, object value)
        {
            if (inputVertex != null)
                // return (inputVertex.Get("*\"" + value.ToString() + "\""));
                return GraphUtil.DeepFindOneByValue(inputVertex, (string)value);
            else
                return null;
        }

#endregion

#region Get

        public IVertex Get(IVertex baseVertex, IVertex expression)
        {
            IEdge e = GetAll_Internal(baseVertex, expression,true).FirstOrDefault(); // what do U think ? :O)
                //.LastOrDefault(); // becouse of the inheritance (to get last edge that is defined in particular vertex and not inherenced one)
                
            if (e == null)
                return null;
            else
                return e.To;
        }

        public IVertex GetAll(IVertex baseVertex, IVertex expression)
        {
            return GetAll_Internal(baseVertex, expression, false);
        }

        protected IVertex GetAll_Internal(IVertex baseVertex, IVertex expression, bool isSingleResult)
        {
            // FOR DEBUG 
            //Console.WriteLine(m0.Instance.DefaultGraphCreationCodeGenerator.GraphCreationCodeGenerateAsString(expression));

            NoInEdgeInOutVertexVertex currentSet = new NoInEdgeInOutVertexVertex(MinusZero.Instance.TempStore);

            foreach (IEdge e in baseVertex)
                //currentSet.AddEdge(e.Meta, e.To);
                currentSet.AddEdgeForNoInEdgeInOutVertexVertex(e);
                       

            if (expression.OutEdges.Count() == 0)
                return currentSet;

            return RemoveDuplicates(GetAll_Iterate(currentSet, expression, isSingleResult));            
        }

        public static IVertex RemoveDuplicates(IEnumerable<IEdge> inVertex)
        {
            NoInEdgeInOutVertexVertex outVertex = new NoInEdgeInOutVertexVertex(MinusZero.Instance.TempStore);

            foreach (IEdge e in inVertex)
            {
                bool donocopy = false;

                foreach (IEdge ee in outVertex)
                    if (ee.To == e.To && ee.Meta == e.Meta)
                        donocopy = true;

                if (!donocopy)
                    outVertex.AddEdgeForNoInEdgeInOutVertexVertex(e);
            }

            return outVertex;
        }

        private NoInEdgeInOutVertexVertex GetAll_Iterate(NoInEdgeInOutVertexVertex currentSet, IVertex expression, bool isSingleResult)
        {
            foreach (IEdge e in expression)
            {
                if(GeneralUtil.CompareStrings(e.Meta.Value,"$Empty")
                    ||GeneralUtil.CompareStrings(e.Meta.Value,":")
                    || GeneralUtil.CompareStrings(e.Meta.Value, "$EmptyContainerTerminal"))
                    currentSet = GetAll_Filter(currentSet, e);

                if (GeneralUtil.CompareStrings(e.Meta.Value, "\\"))
                {
                    currentSet = GetAll_OneLevelDeeper(currentSet);
                    currentSet = GetAll_Iterate(currentSet, e.To, isSingleResult);
                }

                if (GeneralUtil.CompareStrings(e.Meta.Value, "*"))
                {
                    //currentSet = GetAll_OneLevelDeeper(currentSet); // first, one level deeper - BUT NAH, WE DO NOT WANT. at lest for now

                    NoInEdgeInOutVertexVertex newCurrentSet = new NoInEdgeInOutVertexVertex(MinusZero.Instance.TempStore);
                    
                    GetlAll_AllLevelsDeeper(newCurrentSet, currentSet);

                    currentSet = newCurrentSet;

                    currentSet = GetAll_Iterate(currentSet, e.To, isSingleResult);
                }
            }

            return currentSet;
        }

        private NoInEdgeInOutVertexVertex GetAll_OneLevelDeeper(NoInEdgeInOutVertexVertex currentSet)
        {
            NoInEdgeInOutVertexVertex newSet = new NoInEdgeInOutVertexVertex(MinusZero.Instance.TempStore);

            foreach (IEdge e in currentSet)
                foreach (IEdge ee in e.To)
                    //newSet.AddEdge(ee.Meta, ee.To);
                    newSet.AddEdgeForNoInEdgeInOutVertexVertex(ee);

            return newSet;
        }

        private void GetlAll_AllLevelsDeeper(NoInEdgeInOutVertexVertex currentSet, IVertex iterateSet)
        {            
            foreach(IEdge e in iterateSet)
                if(GraphUtil.DoIEnumerableIEdgeContainsVertex(currentSet,e.To)==false){
                    //currentSet.AddEdge(e.Meta, e.To);
                    currentSet.AddEdgeForNoInEdgeInOutVertexVertex(e);
                    GetlAll_AllLevelsDeeper(currentSet,e.To);
                }            
        }

        private IEdge GetNextExpressionEdgeTerminal(IEdge expressionEdge){
            if (GeneralUtil.CompareStrings(expressionEdge.Meta.Value, ":"))
            {
                IList<IEdge> el = expressionEdge.To.OutEdges.ToList();

                if (el.Count() > 1
                    && GeneralUtil.CompareStrings(el[0].Meta, "$Empty")
                    && GeneralUtil.CompareStrings(el[1].Meta, "$Empty"))
                {
                    if (el.Count() > 2)
                        return el[2];
                }
                else
                    if (el.Count() > 1)
                        return el[1];
            }
            else
                if (expressionEdge.To.Count() > 0)
                    return expressionEdge.To.FirstOrDefault();
                else
                    return null;

            return null; // will never hit this spot
        }

        private bool EdgeExpressionEdgeCompare(IEdge testEdge, IEdge expressionEdge)
        {
            if (GeneralUtil.CompareStrings(expressionEdge.Meta.Value, ":"))
            {
                IList<IEdge> el = expressionEdge.To.OutEdges.ToList();

                bool firstTest = false;

                if (el.Count() > 1
                    && GeneralUtil.CompareStrings(el[0].Meta, "$Empty")
                    && GeneralUtil.CompareStrings(el[1].Meta, "$Empty"))
                {
                    if (GeneralUtil.CompareStrings(testEdge.To.Value, el[1].To.Value)
                        //&& GeneralUtil.CompareStrings(testEdge.Meta.Value, el[0].To.Value))
                        && VertexOperations.InheritanceCompare(testEdge.Meta, (string)el[0].To.Value)) // Inheritance support
                        firstTest = true;
                    else
                        if (GeneralUtil.CompareStrings(el[0].To.Value, "$Is") && GeneralUtil.CompareStrings(testEdge.Meta, "$Is")) // Inheritance support
                            firstTest = VertexOperations.InheritanceCompare(testEdge.To, (string)el[1].To.Value);
                }
                else
                    //if (GeneralUtil.CompareStrings(testEdge.Meta, el[0].To.Value))
                    if (VertexOperations.InheritanceCompare(testEdge.Meta, (string)el[0].To.Value)) // Inheritance support
                        firstTest = true;
                    else
                        if (GeneralUtil.CompareStrings(el[0].Meta, "$EmptyContainerTerminal")) // is is for sure ok? 2014.11.21
                            firstTest = true;

                if (firstTest
                    && el[0].To.Count() > 0) // meta{xxx}:
                {
                    return GetAll_Filter_nextTerminal(testEdge.Meta, el[0].To.FirstOrDefault());
                }
                else
                    return firstTest;

            }
            else{
                if (GeneralUtil.CompareStrings(expressionEdge.Meta.Value, "$Empty")
                    && GeneralUtil.CompareStrings(testEdge.To.Value, expressionEdge.To.Value))
                    return true;
                if(GeneralUtil.CompareStrings(expressionEdge.Meta.Value, "$EmptyContainerTerminal"))
                    return true;
            }

            return false;
        }

        private NoInEdgeInOutVertexVertex GetAll_Filter(NoInEdgeInOutVertexVertex currentSet, IEdge expressionEdge)
        {
            NoInEdgeInOutVertexVertex newSet = new NoInEdgeInOutVertexVertex(MinusZero.Instance.TempStore);

            IEdge nextTerminal = GetNextExpressionEdgeTerminal(expressionEdge);
            
            foreach (IEdge ee in currentSet)            
                if (nextTerminal == null)
                {
                    if (EdgeExpressionEdgeCompare(ee, expressionEdge))
                        //newSet.AddEdge(ee.Meta, ee.To);
                        newSet.AddEdgeForNoInEdgeInOutVertexVertex(ee);
                }else
                    if (EdgeExpressionEdgeCompare(ee, expressionEdge)
                        &&GetAll_Filter_nextTerminal(ee.To,nextTerminal))
                        //newSet.AddEdge(ee.Meta, ee.To);                                
                        newSet.AddEdgeForNoInEdgeInOutVertexVertex(ee);

            return newSet;
        }

    private bool GetAll_Filter_nextTerminal(IVertex testVertex, IEdge nextTerminal)
    {
        if (nextTerminal == null)
            return true;

        bool foundAll = true;

        if (GeneralUtil.CompareStrings(nextTerminal.Meta, "{") 
            || GeneralUtil.CompareStrings(nextTerminal.Meta, ","))
            foreach (IEdge expressionEdge in nextTerminal.To)
            {
                if (GeneralUtil.CompareStrings(expressionEdge.Meta, ":") 
                    || GeneralUtil.CompareStrings(expressionEdge.Meta, "$Empty")
                    || GeneralUtil.CompareStrings(expressionEdge.Meta, "$EmptyContainerTerminal"))
                {
                    bool found = false;

                    foreach (IEdge testEdge in testVertex)
                    {
                        IEdge _nextTerminal = GetNextExpressionEdgeTerminal(expressionEdge);

                        if (_nextTerminal == null)
                        {
                            if (EdgeExpressionEdgeCompare(testEdge, expressionEdge))
                                found = true;
                        }
                        else
                            if (EdgeExpressionEdgeCompare(testEdge, expressionEdge)
                            && GetAll_Filter_nextTerminal(testEdge.To, _nextTerminal))
                                found = true;
                    }

                    if (found == false)
                        foundAll=false;
                }
                if(GeneralUtil.CompareStrings(expressionEdge.Meta,","))                
                    if(!GetAll_Filter_nextTerminal(testVertex,expressionEdge))
                        foundAll=false;
                
               // if(GeneralUtil.CompareStrings(expressionEdge.Meta,"$EmptyContainerTerminal"))
                //    if (!GetAll_Filter_nextTerminal(testVertex, expressionEdge))
                    //    foundAll = false;
            }

        return foundAll;
    }

#endregion

#region GraphCreationCodeGenerate

        string Tab = "    ";

        string NewLine = "\r\n";

        void appendTabs(StringBuilder s, int times)
        {
            if (GenerateWhiteSpaces)
                for (int i = 0; i < times; i++)
                    s.Append(Tab);
        }

        void appendNewLine(StringBuilder s)
        {
            if (GenerateWhiteSpaces)
                s.Append(NewLine);
        }

        void appendLiteral(StringBuilder s, string literal)
        {
            literal = literal.Replace("\"", "\\\"");

            if (AlwaysGenerateQuotas)
            {
                s.Append("\"");
                s.Append(literal);
                s.Append("\"");
            }
            else
            {
                bool needToUseQuotas = false;

                if (literal.Contains("\""))
                    needToUseQuotas = true;

                foreach (IEdge e in ZeroCodeTerminals)
                {
                    if (literal.Contains((string)e.To.Value))
                        needToUseQuotas = true;
                }

                if (needToUseQuotas)
                {
                    s.Append("\"");
                    s.Append(literal);
                    s.Append("\"");
                }
                else
                    s.Append(literal);
            }
        }

        public string ZeroCodeGraph2String(IVertex graphRoot)
        {
            StringBuilder s = new StringBuilder();

            if(graphRoot.Value!=null)
                appendLiteral(s, graphRoot.Value.ToString());

            List<IVertex> beenList = new List<IVertex>();

            GraphCreationCodeGenerateAsString_Reccurent(graphRoot, s, 0,beenList);

            return s.ToString();
        }

        void GraphCreationCodeGenerateAsString_Reccurent(IVertex v, StringBuilder s, int level, List<IVertex> beenList){
            if (v.OutEdges.Count() > 0)
            {
              //  s.Append("{");
                appendNewLine(s);

                bool isFirst = true;
                
                foreach (IEdge e in v.OutEdges)
                {
                    if (!beenList.Contains(e.To))
                    {
                        beenList.Add(e.To);

                        if (isFirst)
                            isFirst = false;
                        else
                        {
                         //   s.Append(",");
                            appendNewLine(s);
                        }


                        if (GeneralUtil.CompareStrings(e.Meta.Value, "$Empty"))
                        {
                            appendTabs(s, level + 1);

                            appendLiteral(s, e.To.Value.ToString());

                        }
                        else
                        {
                            appendTabs(s, level + 1);

                            appendLiteral(s, e.Meta.Value.ToString());

                            s.Append(":");

                            if (e.To.Value != null)
                                appendLiteral(s, e.To.Value.ToString());
                        }

                        GraphCreationCodeGenerateAsString_Reccurent(e.To, s, level + 1, beenList);
                    }
                }

             //   appendNewLine(s);
                appendTabs(s, level);
              //  s.Append("}");                
            }
        }

#endregion

        public ZeroCodeEngine_OLD()
        {            
            GenerateWhiteSpaces = true;

            AlwaysGenerateQuotas = false;

            ZeroCodeTerminals = MinusZero.Instance.DefaultLanguageDefinition_OLD;

            Parser = new GoldGenericParser("ZeroCode.egt", ZeroCodeTerminals);
                        
        }
    }
}
