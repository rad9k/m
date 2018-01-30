using m0.Foundation;
using m0.Graph;
using m0.TextLanguage;
using m0.Util;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    class getLinkStringProcessing
    {
        IVertex Vertex;

        ZeroCodeGraph2StringProcessing zcg2sp;

        IList<IVertex> linkBeenList;

        IEdge parent;

        string shortestLink;
        int shortestLinkLength;

        public string Process(ZeroCodeGraph2StringProcessing _zcg2sp, IVertex v, IEdge _parent)
        {       
            zcg2sp = _zcg2sp;

            parent = _parent;

            Vertex = v;

           if (_parent != null)
            {
                IVertex Is = _parent.To.Get("$Is:");

                if (Is != null && Is.Get(v.Value.ToString()) == v)
                    return (v.Value.ToString());
            }

            if (_parent != null)
            {
                foreach (IEdge e in VertexOperations.GetChildEdges(_parent.Meta))
                    if (v == e.To)
                        return (v.Value.ToString());
            }
                        

            if (zcg2sp.VertexesDictionary.ContainsKey(v))
                return zcg2sp.VertexesDictionary[v].LinkString;

            linkBeenList = new List<IVertex>();

            shortestLink = "LINK NOT FOUND";
            shortestLinkLength = 99999;

            // linkBeenList.Clear();       

            bool isMetaDirect=false;    

            GetLinkString_Recurrect(v, new List<IEdge>(),ref isMetaDirect);
            

            if(shortestLinkLength!=99999)
               zcg2sp.VertexesDictionary.Add(v, new VertexData(shortestLink, shortestLinkLength));

            if (shortestLinkLength == 99999 && zcg2sp.SubGraphVertexesDictionary.ContainsKey(v))
                return zcg2sp.SubGraphVertexesDictionary[v].LinkString;

            return shortestLink;
        }

        static void Append(StringBuilder s,IVertex v)
        {
            if (v == null || v.Value==null)
                return;

            if (GeneralUtil.CompareStrings(v.Value, ""))
                return;

            if (GeneralUtil.CompareStrings(v.Value, "$Empty"))
                return;

            //s.Append(v.Value);
            s.Append(ZeroCodeCommon.stringToPossiblyEscapedString(v.Value.ToString()));
        }

        public static string GetStringFromEdgesList(List<IEdge> edgesList, bool isImportMeta)
        {
            StringBuilder s = new StringBuilder();

            bool wasPrevious = false;

            for(int x=edgesList.Count-1; x!=-1; x--)
            {
                IEdge e = edgesList[x];

                if (wasPrevious)
                    s.Append("\\");

                if (e.Meta != null && e.To == null)
                    //s.Append(e.Meta.Value);
                    s.Append(ZeroCodeCommon.stringToPossiblyEscapedString(e.Meta.Value.ToString()));

                StringBuilder toAppend = new StringBuilder();

                if (e.To != null) {
                    if (isImportMeta)
                    {
                        if (!VertexOperations.IsToVertexEnoughToIdentifyEdge(e.From, e.To)
                            && !ZeroCodeGraph2StringProcessing.IsNullOrEmpty(e.Meta))
                        {
                            Append(toAppend, e.Meta);

                            toAppend.Append(":");
                        }

                        Append(toAppend,e.To);
                        
                    }
                    else
                    {
                        if (e.Meta == null || GeneralUtil.CompareStrings(e.Meta,"$Empty"))
                        {
                            toAppend.Append(":");
                            Append(toAppend, e.To);
                        }else {
                            if(!VertexOperations.IsToVertexEnoughToIdentifyEdge(e.From,e.To))
                                Append(toAppend, e.Meta);

                            toAppend.Append(":");
                            Append(toAppend, e.To);
                        }
                    }

                    if(VertexOperations.IsMetaAndToVertexEnoughToIdentifyEdge(e.From, e.Meta, e.To))
                        s.Append(ZeroCodeCommon.stringToPossiblyEscapedString(toAppend.ToString()));
                    else
                    {
                        int pos = 0;
                        IVertex q = e.From.GetAll(e.Meta + ":" + e.To);

                        IVertex tv;
                        do
                        {
                            tv = q.ElementAt(pos).To;
                            pos++;
                        } while (tv == e.To);

                        s.Append(ZeroCodeCommon.stringToPossiblyEscapedString(toAppend.ToString()) + "|" + pos );
                    }
                }    

                wasPrevious = true;
            }

            return s.ToString();
        }

        void GetLinkString_Recurrect(IVertex v, List<IEdge> edgesList, ref bool isMetaDirect)
        {
            foreach (IVertex ikv in zcg2sp.Imports.Keys)
                foreach (IVertex iv in zcg2sp.Imports[ikv])
                    if (v == iv)
                        if (GeneralUtil.CompareStrings(ikv.Value, "$Direct"))
                        {
                            if (edgesList.Count() > 0)
                            {
                                isMetaDirect = false;

                                string s = GetStringFromEdgesList(edgesList, false);

                                checkIfNewBest(edgesList, false, s);

                                return;
                            }
                        }
                        else if (GeneralUtil.CompareStrings(ikv.Value, "$DirectMeta"))
                        {
                            if (edgesList.Count() > 0)
                            {
                                isMetaDirect = true;

                                string s = GetStringFromEdgesList(edgesList, true);

                                checkIfNewBest(edgesList, true, s);

                                return;
                                
                            }
                        }
                        else
                        {
                            bool isMeta = false;

                            if (ikv.Get(@"$Is:$ImportMeta") != null)
                                isMeta = true;

                            IEdge ee = new NonActingEdge(null, ikv, null);
                            edgesList.Add(ee);
                            string s = GetStringFromEdgesList(edgesList, isMeta);
                            edgesList.RemoveAt(edgesList.Count - 1);

                            isMetaDirect = isMeta;

                            checkIfNewBest(edgesList, isMeta, s);

                            return;
                        }
                            

            linkBeenList.Add(v);

            foreach (IEdge e in v.InEdgesRaw) 
                if (!linkBeenList.Contains(e.From))
                {
                    IEdge ee = new NonActingEdge(e.From, e.Meta, e.To);

                    edgesList.Add(ee);

                    bool _isMetaDirect = false;

                    GetLinkString_Recurrect(e.From, edgesList, ref _isMetaDirect);

                    //checkIfNewBest(edgesList, _isMetaDirect, returnedLink);

                    edgesList.RemoveAt(edgesList.Count - 1);
                }

            return;
        }

        private void checkIfNewBest(List<IEdge> edgesList, bool _isMetaDirect, string returnedLink)
        {
            if (returnedLink != null)
            {
                bool canUse = true;

                if (this.zcg2sp.SubGraphVertexesDictionary.ContainsKey(Vertex))
                {
                    canUse = false;

                    string properVertexLink = this.zcg2sp.SubGraphVertexesDictionary[Vertex].LinkString;

                    string toTestLink = returnedLink;

                    if (_isMetaDirect == false)
                    {
                        int positionOfFirstSlash = returnedLink.IndexOf('\\');

                        if(positionOfFirstSlash!=-1) // might be used if needed
                            toTestLink = returnedLink.Substring(positionOfFirstSlash);
                    }

                    if (properVertexLink.EndsWith(toTestLink))
                        canUse = true;
                }

                //if (r.Length < shortestLinkLength)
                if (edgesList.Count() < shortestLinkLength && canUse)
                {
                    shortestLink = returnedLink;
                    //shortestLinkLength = shortestLink.Length;
                    shortestLinkLength = edgesList.Count(); // worse approach
                }
            }
        }
    }

    class getLinkStringProcessing_FromRoot
    {
        IVertex Vertex;

        ZeroCodeGraph2StringProcessing zcg2sp;

        IList<IVertex> linkBeenList;

        string shortestLink;
        int shortestLinkLength;

        public string Process(ZeroCodeGraph2StringProcessing _zcg2sp, IVertex v)
        {
            zcg2sp = _zcg2sp;

            Vertex = v;

            linkBeenList = new List<IVertex>();

            shortestLink = "### LINK NOT FOUND ###";
            shortestLinkLength = 99999;

            find_reccurent(new List<IEdge>(), v);

            return shortestLink;
        }

        void find_reccurent(List<IEdge> edgesList,IVertex v)
        {
            if (linkBeenList.Contains(v))
                return;

            linkBeenList.Add(v);
            
            if (v == MinusZero.Instance.Root)
            {
                //string toReturn = getLinkStringProcessing.GetStringFromEdgesList(edgesList,false);
                string toReturn = getLinkStringProcessing.GetStringFromEdgesList(edgesList, true); // this is temporary as .Get does not support meta quey syntax

                if (toReturn.Length < shortestLinkLength)
                {
                    shortestLink = toReturn;
                    shortestLinkLength = toReturn.Length;
                }
            }

            foreach (IEdge e in v.InEdgesRaw)
            {
                edgesList.Add(e);
                find_reccurent(edgesList,e.From);
                edgesList.RemoveAt(edgesList.Count - 1);
            }
        }
    }

    class KeywordMatch
    {
        public IVertex KeywordDefinition;

        public IEdge BaseEdge;

        public string BaseEdgePath;

        public int BaseEdgePathLength;

        public IList<IEdge> MatchedEdges;
    }

    class VertexData
    {
        public string LinkString;
        public int LinkLength;

        public VertexData(String s,int l)
        {
            LinkString = s;
            LinkLength = l;
        }
    }

    class ZeroCodeGraph2StringProcessing
    {
        public IEdge BaseEdge;

        public IList<IEdge> BeenList;

        public StringBuilder Source;

        public IDictionary<IVertex, IList<IVertex>> Imports;

        public IDictionary<IVertex, VertexData> VertexesDictionary;
        public IDictionary<IVertex, VertexData> SubGraphVertexesDictionary;
        public IDictionary<IEdge, KeywordMatch> KeywordMatchedSubGraphEdges;

        public ZeroCodeGraph2StringProcessing()
        {
         
        }

        string Tab = "\t";

        string NewLine = "\r\n";

        void SourceAppend(string s)
        {
            Source.Append(s);
        }

        void SourceAppend(char c)
        {
            Source.Append(c);
        }

        void ImportImports(IVertex baseVertex)
        {
            foreach (IEdge e in baseVertex.GetAll("{$Is:$ImportMeta}:"))
                ImportImports_internal(e);

            foreach (IEdge e in baseVertex.GetAll("{$Is:$Import}:"))
                ImportImports_internal(e);
        }

        private void ImportImports_internal(IEdge e)
        {
            if (Imports.ContainsKey(e.Meta))
                Imports[e.Meta].Add(e.To);
            else
            {
                IList<IVertex> l = new List<IVertex>();

                l.Add(e.To);

                Imports.Add(e.Meta, l);
            }
        }

        int tabTimes; 

        void AppendNewLineAndTabs()
        {
            SourceAppend(NewLine);

            for (int i = 0; i < tabTimes; i++)
                SourceAppend(Tab);
        }   

        void AppendAdditionalNewLines(IEdge e)
        {
            IVertex nl = e.To.Get(@"$NewLine:");

            if (nl == null)
                return;

            int nlAsInt = GraphUtil.GetIntegerValue(nl);

            if (nlAsInt != GraphUtil.NullInt)
                for (int x = 0; x < nlAsInt; x++)
                    SourceAppend(NewLine);
            else
                SourceAppend(NewLine);
        }

       

        void AppendAsLink(IVertex v, IEdge parent, bool hideLinkPrefix)
        {
            string toAppend;

            if (v == BaseEdge.To)
                toAppend = "$CodeRoot";
            else
            {
                getLinkStringProcessing glsp = new getLinkStringProcessing();            
                toAppend = glsp.Process(this, v, parent);
            }

            if(hideLinkPrefix)
                SourceAppend(ZeroCodeCommon.stringToLinkString(toAppend, true));
            else
                SourceAppend(ZeroCodeCommon.stringToLinkString(toAppend, false));
        }

        void AppendAsNew(IVertex v)
        {
            if (!IsNull(v))
            {
                //VertexesAsLink.Add(v, path + "\\" + v.Value); // what is it? not neccesarry now

                SourceAppend(ZeroCodeCommon.stringToNewVertexString(v.Value.ToString()));
            }
        }

        void AppendIs(IEdge e)
        {            
            SourceAppend("$Is");
            AppendDoubleColon();

            SourceAppend(ZeroCodeCommon.stringToLinkString(ZeroCodeCommon.stringToPossiblyEscapedString(e.To.Value.ToString()),true));            
        }

        void AppendDoubleColon()
        {
            SourceAppend(" :: ");
        }

        string FindKeywordEdge(string pre, IVertex baseVertex, string toFind)
        {
            string toAdd = "";

            if (pre != "")
                toAdd = @"\";

            if (GraphUtil.GetValueAndCompareStrings(baseVertex, toFind))
                return pre;

            foreach (IEdge e in baseVertex)
            {
                if (GraphUtil.GetValueAndCompareStrings(e.To, toFind))
                    return pre + toAdd + ZeroCodeCommon.stringToPossiblyEscapedString(e.Meta.Value.ToString()) + ":";
                
                if (!VertexOperations.IsLink(e))
                {
                    string ret = FindKeywordEdge(pre + toAdd + ZeroCodeCommon.stringToPossiblyEscapedString(e.Meta.Value.ToString()) + ":", e.To, toFind);

                    if (ret != null)
                        return ret;
                }
            }

            return null;
        }

        IEdge GetFirstLevelKeywordEdge(KeywordMatch km, string firstEdgeMetaValue)
        {
            string meta = firstEdgeMetaValue.Substring(0, firstEdgeMetaValue.Length - 1);

            IEdge firstEdge = km.MatchedEdges.FirstOrDefault();

            foreach (IEdge e in km.MatchedEdges)
                if (e.From == firstEdge.From)
                    if (GraphUtil.GetValueAndCompareStrings(e.Meta, meta))
                        return e;

            return null;
        }

        IEdge GetKewordEdgeByQuerystring(KeywordMatch km, string queryString)
        {
            string firstEdgeQueryPart;
            string secondQueryPart;

            if (queryString.Contains(@"\")) {
                int firstSlahPosition;

                if (queryString.StartsWith("(?<ANY>)")) // special handling of (?<ANY>) first level meta
                {
                    firstSlahPosition = queryString.IndexOf('\\');
                    
                    secondQueryPart = queryString.Substring(firstSlahPosition + 1, queryString.Length - firstSlahPosition - 1);                    

                    return km.BaseEdge.To.GetAll(secondQueryPart).FirstOrDefault();
                }

                firstSlahPosition = queryString.IndexOf('\\');

                firstEdgeQueryPart = queryString.Substring(0, firstSlahPosition);
                secondQueryPart = queryString.Substring(firstSlahPosition + 1, queryString.Length - firstSlahPosition - 1);

                IEdge firstLevelKeyworEdge = GetFirstLevelKeywordEdge(km, firstEdgeQueryPart);

                return firstLevelKeyworEdge.To.GetAll(secondQueryPart).FirstOrDefault();
            } else
                return GetFirstLevelKeywordEdge(km, queryString);
        }
        
        IEdge GetKeywordManyRoot(IVertex def, out string keywordManyRootQueryString, out int getKeywordManyRootBaseCount)
        {
            string queryString;

            getKeywordManyRootBaseCount = 0;

            NonActingEdge defAsEdge = new NonActingEdge(null, null, def);

            IEdge kmrEdge = GetKeywordManyRoot_reccurent("",defAsEdge, out queryString);

            keywordManyRootQueryString = queryString;

            if (kmrEdge == null)
                return null;

            int count = 1;

            foreach(IEdge e in kmrEdge.From)
            {
                if (e == kmrEdge)
                {
                    getKeywordManyRootBaseCount = count;
                    return kmrEdge;
                }

                if (GeneralUtil.CompareStrings(e.Meta, kmrEdge.Meta))
                    count++;
            }

            return null;
        }

        IEdge GetKeywordManyRoot_reccurent(string path, IEdge baseEdge, out string keywordManyRootQueryString)
        {
            keywordManyRootQueryString = "";

            if (baseEdge.To.Get("$KeywordManyRoot:") != null)
            {
                keywordManyRootQueryString = path;
                return baseEdge;
            }

            string toAdd;

            if (path == "")
                toAdd = "";
            else
                toAdd = path + "\\";

            foreach(IEdge e in baseEdge.To)
            {
                if (!VertexOperations.IsLink(e))
                {
                    IEdge found = GetKeywordManyRoot_reccurent(toAdd + ZeroCodeCommon.stringToPossiblyEscapedString(e.Meta.ToString()) +":",e, out keywordManyRootQueryString);

                    if (found != null)
                        return found;
                }             
            }

            return null;
        }

        void AppendAsLink_FromRoot(IVertex v)
        {
            getLinkStringProcessing_FromRoot glsp = new getLinkStringProcessing_FromRoot();
            SourceAppend(glsp.Process(this, v));
        }

        bool AppendImportKeyword(IEdge keywordEdge, bool isMeta)
        {
            KeywordMatch km = KeywordMatchedSubGraphEdges[keywordEdge];

            IEdge importEdge=null;

             foreach(IEdge e in km.MatchedEdges)
             {
                if (isMeta && GraphUtil.GetValueAndCompareStrings(e.Meta, "$ImportMeta"))
                    importEdge = e;

                if (!isMeta && GraphUtil.GetValueAndCompareStrings(e.Meta, "$Import"))
                    importEdge = e;
             }

            IEdge linkEdge=null;

            foreach (IEdge e in km.MatchedEdges)
                if (e.Meta == importEdge.To)
                    linkEdge = e;

            if (isMeta)
            {
                SourceAppend("import meta " + ZeroCodeCommon.stringToNewVertexString(importEdge.To.ToString()) + " ");
                AppendAsLink_FromRoot(linkEdge.To);
            }
            else
            {
                SourceAppend("import " + ZeroCodeCommon.stringToNewVertexString(importEdge.To.ToString()) + " ");
                AppendAsLink_FromRoot(linkEdge.To);
            }

            return false;
        }

        bool AppendImportDirectKeyword(IEdge keywordEdge, bool isMeta)
        {
            KeywordMatch km = KeywordMatchedSubGraphEdges[keywordEdge];

            IEdge linkEdge = km.MatchedEdges[0];
            
            if (isMeta)
            {
                SourceAppend("import direct meta ");
                AppendAsLink_FromRoot(linkEdge.To);
            }
            else
            {
                SourceAppend("import direct ");
                AppendAsLink_FromRoot(linkEdge.To);
            }

            return false;
        }

        bool AppendKeyword(IEdge keywordEdge, bool isNested)
        {
            bool whatToReturn = true;

            KeywordMatch km = KeywordMatchedSubGraphEdges[keywordEdge];

            if (km.BaseEdge == keywordEdge /*&& isVertexNew(keywordEdge, GetPathFromKeywordMatchAndKeywordEdge(km, keywordEdge, ""))*/)
            {
                if(!isNested)
                    AppendNewLineAndTabs();


                if (GraphUtil.GetValueAndCompareStrings(km.KeywordDefinition, "import (?<name>) (?<link>)"))
                    return AppendImportKeyword(keywordEdge, false);

                if (GraphUtil.GetValueAndCompareStrings(km.KeywordDefinition, "import meta (?<name>) (?<link>)"))
                    return AppendImportKeyword(keywordEdge, true);

                if (GraphUtil.GetValueAndCompareStrings(km.KeywordDefinition, "import direct (?<link>)"))
                    return AppendImportDirectKeyword(keywordEdge, false);

                if (GraphUtil.GetValueAndCompareStrings(km.KeywordDefinition, "import direct meta (?<link>)"))
                    return AppendImportDirectKeyword(keywordEdge, true);


                int keywordManyRootBaseCount;

                string keywordManyRootQueryString;

                IEdge keywordManyRoot = GetKeywordManyRoot(km.KeywordDefinition, out keywordManyRootQueryString, out keywordManyRootBaseCount);


                string sentence = (String)km.KeywordDefinition.Value;

                if (keywordManyRoot == null)
                {
                    ProcessSingleKeywordSentencePart(km, sentence, false);

                    if (sentence.Contains("(?<SUB>)"))
                        whatToReturn = false;
                }
                else
                {
                    int manyGroupEnd = sentence.IndexOf("*)");

                    int manyGroupStart = sentence.IndexOf("(*"); ;

                    string preManySentence = sentence.Substring(0, manyGroupStart);
                    string manySentenceFirst = sentence.Substring(manyGroupStart + 2, manyGroupEnd - manyGroupStart - 2);
                    string postManySentence = sentence.Substring(manyGroupEnd + 2, sentence.Length - manyGroupEnd - 2);

                    string manySentenceSecond = manySentenceFirst;

                    manySentenceSecond = manySentenceSecond.Substring(0, manySentenceSecond.IndexOf("(+"))
                        + manySentenceSecond.Substring(manySentenceSecond.IndexOf("(+") + 2, manySentenceSecond.IndexOf("+)") - manySentenceSecond.IndexOf("(+") - 2)
                        + manySentenceSecond.Substring(manySentenceSecond.IndexOf("+)") + 2);

                    manySentenceFirst = manySentenceFirst.Substring(0, manySentenceFirst.IndexOf("(+"))
                        + manySentenceFirst.Substring(manySentenceFirst.IndexOf("+)") + 2);

                    bool wasThereNewLine = false;

                    wasThereNewLine = ProcessSingleKeywordSentencePart(km, preManySentence, wasThereNewLine);

                    wasThereNewLine = ProcessManyKeywordSentencePart(km, manySentenceFirst, manySentenceSecond, keywordManyRoot, keywordManyRootQueryString, keywordManyRootBaseCount, wasThereNewLine);

                    ProcessSingleKeywordSentencePart(km, postManySentence, wasThereNewLine);
                }

                return whatToReturn;
            }

            return false;
        }

        string GetPathFromKeywordMatchAndKeywordEdge(KeywordMatch km, IEdge e, string path)
        {
            string suffix = "";

            if (path != null)
                suffix = "\\";

            if (e.To == km.BaseEdge.To)
                return km.BaseEdgePath + suffix + path;

            foreach(IEdge ee in e.From.InEdgesRaw)
            {
                string ret = null;

                if(!VertexOperations.IsLink(ee))
                    ret=GetPathFromKeywordMatchAndKeywordEdge(km, ee, GraphUtil.GetIdentyfyingQuerySubString_ImportMeta(e) + suffix + path);

                if (ret != null)
                    return ret;
            }

            return null;

        }

        private bool ProcessSingleKeywordSentencePart(KeywordMatch km, string sentence, bool wasThereNewLine)
        {
            // find matches
            Regex rgx = new Regex(@"\(\?(A)?(V)?<[a-zA-Z0-9]+>\)");

            int prevPos = 0;

            //bool wasThereNewLine = false;

            foreach (Match match in rgx.Matches(sentence))
                if (match.Value == "(?<SUB>)")
                {
                    //

                    string part = sentence.Substring(prevPos, match.Index - prevPos);

                    prevPos = match.Index + match.Length;

                    //

                    if (wasThereNewLine)
                        SourceAppend(ZeroCodeCommon.LineContinuationPrefix + part);
                    else
                        SourceAppend(part);

                    wasThereNewLine = AppendSubVertexes(km, km.BaseEdge, km.BaseEdgePath);
                }
                else
                {
                    string queryString = FindKeywordEdge("", km.KeywordDefinition, match.Value);

                    IEdge e = GetKewordEdgeByQuerystring(km, queryString);

                    BeenList.Add(e); // :O)

                    ProcessSentencePart(km, sentence, ref prevPos, ref wasThereNewLine, match, e);
                }

            if (wasThereNewLine)
                SourceAppend(ZeroCodeCommon.LineContinuationPrefix + sentence.Substring(prevPos));
            else
                SourceAppend(sentence.Substring(prevPos));
            //SourceAppend(sentence.Substring(prevPos));

            return wasThereNewLine;
        }

        private bool ProcessManyKeywordSentencePart(KeywordMatch km, string sentenceFirst, string sentenceSecond, IEdge keywordManyRoot, string keywordManyRootQueryString, int keywordManyRootBaseCount, bool wasThereNewLine)
        {
            int keywordManyRootCount = 0;

            bool isFirst = true;

            string sentence;

            //bool wasThereNewLine = false;

            IEdge edgeFromKeywordManyRootQueryString = GetKewordEdgeByQuerystring(km, keywordManyRootQueryString);

            if (edgeFromKeywordManyRootQueryString == null)
                return wasThereNewLine;

            foreach (IEdge ee in edgeFromKeywordManyRootQueryString.From)
            {
                if (ee.Meta == keywordManyRoot.Meta)
                {
                    keywordManyRootCount++;

                    if (keywordManyRootCount >= keywordManyRootBaseCount)
                    {
                        if (isFirst)
                        {
                            sentence = sentenceFirst;
                            isFirst = false;
                        }
                        else
                            sentence = sentenceSecond;

                        Regex rgx = new Regex(@"\(\?(A)?(V)?<[a-zA-Z0-9]+>\)");

                        int prevPos = 0;

                        foreach (Match match in rgx.Matches(sentence))
                        {
                            string queryString = FindKeywordEdge("", keywordManyRoot.To, match.Value);

                            IEdge e;

                            if (queryString == "")
                                e = ee;
                            else
                                e = ee.To.GetAll(queryString).FirstOrDefault();

                            BeenList.Add(ee); // :O)

                            ProcessSentencePart(km, sentence, ref prevPos, ref wasThereNewLine, match, e);
                        }

                      //  if (wasThereNewLine)
                      //      SourceAppend("Y" + sentence.Substring(prevPos));
                       // else
                       //     SourceAppend(sentence.Substring(prevPos));
                        SourceAppend(sentence.Substring(prevPos));
                    }
                }
            }

            return wasThereNewLine;
        }

        private void ProcessSentencePart(KeywordMatch km, string sentence, ref int prevPos, ref bool wasThereNewLine, Match match, IEdge e)
        {
            //

            string part = sentence.Substring(prevPos, match.Index - prevPos);

            prevPos = match.Index + match.Length;

            //

            if (wasThereNewLine)
                SourceAppend(ZeroCodeCommon.LineContinuationPrefix + part);
            else
                SourceAppend(part);

            wasThereNewLine = false;

            bool edgeCovered = false;

            //if (match.Value.Contains("(?<")) // do not need all that ?A and ?V
            if (e != km.BaseEdge)
                if (KeywordMatchedSubGraphEdges.ContainsKey(e)
                    && KeywordMatchedSubGraphEdges[e].BaseEdge == e
                    && isVertexNew(e, GetPathFromKeywordMatchAndKeywordEdge(km, e, null)))
                {
                    AppendKeyword(e, true);

                    edgeCovered = true;
                }

            if (!edgeCovered)
            {
                string path = GetPathFromKeywordMatchAndKeywordEdge(km, e, null);

                AppendVertex(e, path, false, false);

                if(!VertexOperations.IsLink(e) && e != km.BaseEdge)
                    wasThereNewLine = AppendSubVertexes(km, e, path);
            }
        }

        private bool AppendSubVertexes(KeywordMatch km, IEdge baseEdge, string basePath)
        {
            bool wasThereNewLine = false;

            bool wasFirstNewLine = false;

            foreach (IEdge e in baseEdge.To)
                if (!km.MatchedEdges.Contains(e))
                {
                    if (wasFirstNewLine == false)
                    {
                        tabTimes ++;
                        wasFirstNewLine = true;
                        wasThereNewLine = true;
                    }

                    AppendEdge(e, null, basePath + "\\" + GraphUtil.GetIdentyfyingQuerySubString_ImportMeta(e));
                }

            if (wasFirstNewLine)
            {
                tabTimes--;

                AppendNewLineAndTabs();
            }

            return wasThereNewLine;
        }

        bool AppendEdge(IEdge e, IEdge parent, string path)
        {
            if (KeywordMatchedSubGraphEdges.ContainsKey(e))
               if (SubGraphVertexesDictionary.ContainsKey(e.To) && SubGraphVertexesDictionary[e.To].LinkString == path)
                    return AppendKeyword(e, false);
                else
                    if(KeywordMatchedSubGraphEdges[e].BaseEdge.To!=e.To) // :O)
                          return true; // ?????????????????????? or true?

            AppendNewLineAndTabs();

            AppendPrefix();

            if (!IsNullOrEmpty(e.Meta))
            {
                AppendAsLink(e.Meta, parent, true);

                AppendDoubleColon();
            }

            return AppendVertex(e, path, true, true);
        }

        private bool AppendVertex(IEdge e, string path, bool appendSuffix, bool hideLinkPrefix)
        {
            if (VertexOperations.IsLink(e))
            {
                //SourceAppend("L1!");
                AppendAsLink(e.To, null, hideLinkPrefix);

                if (appendSuffix)
                      AppendSuffix();

                return false;
            }
            else
            {
                //  SourceAppend("<" + SubGraphVertexesAsLink[e.To].String + ":" + path + ">");

                if (isVertexNew(e, path))
                {
                    AppendAsNew(e.To);

                    if (appendSuffix)
                        AppendSuffix();

                    return true;
                }
                else
                {
                    // SourceAppend(" > " + firstQuery+" | "+secondQuery + " < ");
                    // 
                    // IN CASE OF EXPLOSION - uncomment

                    //SourceAppend("L2!");
                    AppendAsLink(e.To, null, hideLinkPrefix);

                    if (appendSuffix)
                        AppendSuffix();

                    return false;
                }
            }
        }

        private bool isVertexNew(IEdge e, string path)
        {
            if (!SubGraphVertexesDictionary.ContainsKey(e.To))
                return false; // is it possible?

            string firstQuery = SubGraphVertexesDictionary[e.To].LinkString;
            string secondQuery = path;

            if (path == null ||
                firstQuery == secondQuery)
                return true;

            return false;
        }

        public static bool IsNull(IVertex v)
        {
            if (v == null)
                return true;

            if (v.Value == null)
                return true;

            return false;
        }

        public static bool IsNullOrEmpty(IVertex v)
        {
            if (v == null)
                return true;

            if (v.Value == null)
                return true;

            if (GeneralUtil.CompareStrings(v.Value, "$Empty"))
                return true;

            return false;
        }

        bool ShallProcess(IEdge e)
        {
            if(GeneralUtil.CompareStrings(e.Meta.Value, "$NewLine"))
                return false;

            /*if (GeneralUtil.CompareStrings(e.Meta.Value, "$Import"))
                return false;

            if (GeneralUtil.CompareStrings(e.Meta.Value, "$ImportMeta"))
                return false;

            if (e.Meta.Get(@"$Is:$Import") != null)
                return false;

            if (e.Meta.Get(@"$Is:$ImportMeta") != null)
                return false;*/

            return true;
        }

        public static bool IsKeywordVertexWildcard(IVertex v)
        {
            if (v.Value == null)
                return false;

            if (((String)v.Value).StartsWith("(?<")
                /*|| ((String)v.Value).StartsWith("(?<")
                || ((String)v.Value).StartsWith("(?<")*/)
                return true;

            return false;
        }

        public bool GetGraphMatch(IVertex parentToCheck, IEdge keywordEdge)
        {
            if (keywordEdge.To.Get("$KeywordManyRoot:") != null)
                return true;

            string searchString;

            if (IsKeywordVertexWildcard(keywordEdge.To))
                searchString = ZeroCodeCommon.stringToPossiblyEscapedString(keywordEdge.Meta.ToString()) + ":";
            else
                searchString = ZeroCodeCommon.stringToPossiblyEscapedString(keywordEdge.Meta.ToString()) + ":" + ZeroCodeCommon.stringToPossiblyEscapedString(keywordEdge.To.ToString());

            IVertex search = parentToCheck.GetAll(searchString);

            bool toReturn = false;

            foreach (IEdge searchResult in search)
            {
                if (/*!KeywordMatchedSubGraphEdges.ContainsKey(searchResult) &&*/ !currentMatchGraphEdgeList.Contains(searchResult))
                {
                    if(!VertexOperations.IsLink(keywordEdge))
                        foreach (IEdge subKeywordEdge in keywordEdge.To)
                            if (/*!IsLink(subKeywordEdge) 
                                && */!GeneralUtil.CompareStrings(subKeywordEdge.Meta,"$KeywordManyRoot") 
                                && GetGraphMatch(searchResult.To, subKeywordEdge) == false)
                                return false;

                    //if (!currentMatchGraphEdgeList.Contains(searchResult))
                    {
                        currentMatchGraphEdgeList.Add(searchResult);

                        if (keywordEdge.To.Get("$KeywordManyRoot:") == null)
                            return true;
                        else
                            toReturn = true;
                    }
                }
            }

            return toReturn;
        }

        List<IEdge> currentMatchGraphEdgeList;

        public IList<IEdge> MatchGraphs_import(IEdge edgeToCheck)
        {
            IEdge secondEdge = edgeToCheck.From.GetAll(ZeroCodeCommon.stringToPossiblyEscapedString(edgeToCheck.To.ToString()) + ":").FirstOrDefault();

            if (secondEdge != null)
            {
                currentMatchGraphEdgeList.Add(secondEdge);
                return currentMatchGraphEdgeList;
            }

            return null;
        }

        public IList<IEdge> MatchGraphs(IEdge edgeToCheck, IVertex graphToCompare)
        {
            currentMatchGraphEdgeList = new List<IEdge>();

            IVertex firstMatchingEdgesInGraphToCompare = graphToCompare.GetAll(ZeroCodeCommon.stringToPossiblyEscapedString(edgeToCheck.Meta.ToString()) + ":");

            IEdge firstMatchEdgeInGraphToCompare = null;

            foreach(IEdge e in firstMatchingEdgesInGraphToCompare)
                if(firstMatchEdgeInGraphToCompare == null)
                {
                    if (IsKeywordVertexWildcard(e.To))
                        firstMatchEdgeInGraphToCompare = e;
                    else
                        if (GraphUtil.GetValueAndCompareStrings(edgeToCheck.To, (String)e.To.Value)) 
                            firstMatchEdgeInGraphToCompare = e;
                }

            if (firstMatchEdgeInGraphToCompare == null) // lets try with (?<ANY>) @ meta
            {
                firstMatchingEdgesInGraphToCompare = graphToCompare.GetAll("(?<ANY>):");

                if (firstMatchingEdgesInGraphToCompare.Count() > 0)
                    if (GraphUtil.GetValueAndCompareStrings(edgeToCheck.To, (String)firstMatchingEdgesInGraphToCompare.FirstOrDefault().To.Value))
                        firstMatchEdgeInGraphToCompare = firstMatchingEdgesInGraphToCompare.FirstOrDefault();
            }

            if (firstMatchEdgeInGraphToCompare != null)
            {
                currentMatchGraphEdgeList.Add(edgeToCheck);

                // if this is $ImportMeta or $Import we will handle it separetly

                if (GraphUtil.GetValueAndCompareStrings(graphToCompare, "import (?<name>) (?<link>)")
                    || GraphUtil.GetValueAndCompareStrings(graphToCompare, "import meta (?<name>) (?<link>)"))
                    return MatchGraphs_import(edgeToCheck);

                // end of $ImportMeta and $Import special handling

                        foreach (IEdge keywordEdge in graphToCompare)
                    //if (!IsLink(keywordEdge))
                    {
                        if (keywordEdge == firstMatchEdgeInGraphToCompare)
                        {
                            foreach (IEdge keywordEdgeNested in firstMatchEdgeInGraphToCompare.To)
                               // if (!IsLink(keywordEdgeNested))
                                if (GetGraphMatch(edgeToCheck.To, keywordEdgeNested) == false)
                                    return null;                                                                 
                        }
                        else
                          if (GetGraphMatch(edgeToCheck.From, keywordEdge) == false)
                                return null;                       
                    }
            }

            return currentMatchGraphEdgeList;
        }

        public int getNumberOfOccurances(string baseString, char subString)
        {
            int n = 0;
            int count = 0 ;

            while ((n = baseString.IndexOf(subString, n)) != -1)
            {
                n++;
                count++;
            }
    

            return count;
        }

        public void CheckVertexIfItMachesAnyKeywordGraphs(IEdge edgeToCheck, string path)
        {
            //if (KeywordMatchedSubGraphEdges.ContainsKey(edgeToCheck))
            //   return;

            foreach (IEdge keyword in MinusZero.Instance.Root.GetAll(@"User\CurrentUser:\CodeSettings:\Keyword:\"))
            {              
                IList<IEdge> matchedEdges = MatchGraphs(edgeToCheck, keyword.To);

                if (matchedEdges!=null && matchedEdges.Count > 0)
                {
                    KeywordMatch match = new KeywordMatch();

                    // match.BaseEdge = matchedEdges[0];
                    match.BaseEdge = edgeToCheck; // same as above
                    match.BaseEdgePath = path;

                    match.BaseEdgePathLength = getNumberOfOccurances(match.BaseEdgePath, '\\');

                    match.KeywordDefinition = keyword.To;
                    match.MatchedEdges = new List<IEdge>();

                    foreach (IEdge e in matchedEdges)
                    {
                        match.MatchedEdges.Add(e);

                        if (KeywordMatchedSubGraphEdges.ContainsKey(e))
                        { 
                            if(match.BaseEdge.To == e.To)
                            {
                                KeywordMatch oldMatch = KeywordMatchedSubGraphEdges[e];

                                if (oldMatch.BaseEdge == match.BaseEdge)
                                {
                                    if (match.BaseEdgePathLength < oldMatch.BaseEdgePathLength)
                                    {
                                        oldMatch.BaseEdgePath = match.BaseEdgePath;
                                        oldMatch.BaseEdgePathLength = match.BaseEdgePathLength;
                                    }
                                }
                                else
                                {
                                    KeywordMatchedSubGraphEdges.Remove(e);
                                    KeywordMatchedSubGraphEdges.Add(e, match);
                                }

                                //KeywordMatch oldMatch = KeywordMatchedSubGraphEdges[e];

                                //oldMatch.BaseEdgePath = match.BaseEdgePath;
                                //oldMatch.BaseEdgePathLength = match.BaseEdgePathLength;
                            }

                          /*  if (KeywordMatchedSubGraphEdges[e].BaseEdge.To != e.To
                                || 
                                && match.BaseEdgePathLength < KeywordMatchedSubGraphEdges[e].BaseEdgePathLength) 
                            {
                                KeywordMatchedSubGraphEdges.Remove(e);
                                KeywordMatchedSubGraphEdges.Add(e, match);
                            }*/
                          
                        }else
                           KeywordMatchedSubGraphEdges.Add(e, match);
                    }
                }
            }
        }

        public void GetLinksForSubGraphVertexes(IEdge e, string path, int nestedLevel)
        {
            BeenList.Add(e);

            string suffix = "";

            if (path != null)
                suffix = "\\";

            foreach (IEdge ee in e.To.OutEdgesRaw)
                if (!VertexOperations.IsLink(ee)) 
                {
                    string LinkString = path + suffix + GraphUtil.GetIdentyfyingQuerySubString_ImportMeta(ee);

                    bool beenThereButNeedToReEnter = false;

                    if (SubGraphVertexesDictionary.ContainsKey(ee.To))
                    {
                        VertexData l = SubGraphVertexesDictionary[ee.To];

                        if (nestedLevel < l.LinkLength)
                        {
                            VertexData vd = SubGraphVertexesDictionary[ee.To];

                            vd.LinkString = LinkString;
                            vd.LinkLength = nestedLevel;

                            beenThereButNeedToReEnter = true;
                        }
                    }else
                        SubGraphVertexesDictionary.Add(ee.To, new VertexData(LinkString, nestedLevel));

                    if(beenThereButNeedToReEnter || !BeenList.Contains(ee))
                        GetLinksForSubGraphVertexes(ee, LinkString, nestedLevel+1);
                }
        }

        public void MatchKeywords(IEdge e, string path)
        {
            BeenList.Add(e);

            string suffix = "";

            if (path != null)
                suffix = "\\";

            foreach (IEdge ee in e.To.OutEdgesRaw)
                //if (!IsLink(ee))
                {
                    string LinkString = path + suffix + GraphUtil.GetIdentyfyingQuerySubString_ImportMeta(ee);

                    CheckVertexIfItMachesAnyKeywordGraphs(ee, LinkString);                    

                    if (!BeenList.Contains(ee)&&!VertexOperations.IsLink(ee))
                        MatchKeywords(ee, LinkString);
                }
        }

        public string Process(IEdge graphBaseEdge)
        {
            BeenList = new List<IEdge>();
            Source = new StringBuilder();
            Imports = new Dictionary<IVertex, IList<IVertex>>();
            VertexesDictionary = new Dictionary<IVertex, VertexData>();
            SubGraphVertexesDictionary = new Dictionary<IVertex, VertexData>();
            KeywordMatchedSubGraphEdges = new Dictionary<IEdge, KeywordMatch>();

            // 

            BaseEdge = graphBaseEdge;

            GetLinksForSubGraphVertexes(BaseEdge, null,0);

            BeenList.Clear();

            MatchKeywords(BaseEdge,null);

            BeenList.Clear();

            //

            BeenList.Add(graphBaseEdge);

            //



            //ImportImports(MinusZero.Instance.Root.Get(@"System\TextLanguage\ZeroCode\DefaultImports"));

            ImportImports(MinusZero.Instance.Root.Get(@"User\CurrentUser:\CodeSettings:"));

            ImportImports(graphBaseEdge.To);

            AppendPrefix();
            AppendAsNew(graphBaseEdge.To);
            AppendSuffix();

            foreach (IEdge e in graphBaseEdge.To.OutEdgesRaw)
                ZeroCodeGraph2String_Reccurent(e, 1, graphBaseEdge,null);

            return Source.ToString();
        }

        void AppendPrefix()
        {
            SourceAppend(ZeroCodeCommon.CodeGraphVertexPrefix);
        }

        void AppendSuffix()
        {
            SourceAppend(ZeroCodeCommon.CodeGraphVertexSuffix);
        }

        void ZeroCodeGraph2String_Reccurent(IEdge baseEdge, int level, IEdge parent, string path)
        {
            if (BeenList.Contains(baseEdge))
                return;

            tabTimes = level;

            if (!ShallProcess(baseEdge))
                return;

            if (path != null)
                path = path + "\\" + GraphUtil.GetIdentyfyingQuerySubString_ImportMeta(baseEdge);
            else
                path = GraphUtil.GetIdentyfyingQuerySubString_ImportMeta(baseEdge);

            if (GeneralUtil.CompareStrings(baseEdge.Meta, "$Is") && baseEdge.To == parent.Meta && !KeywordMatchedSubGraphEdges.ContainsKey(baseEdge))
            {
                AppendNewLineAndTabs();

                AppendPrefix();
                AppendIs(baseEdge);
                AppendSuffix();
                return;
            }
            
            bool been = false;

            bool isLink = VertexOperations.IsLink(baseEdge);

            if (BeenList.Contains(baseEdge)&&!isLink)
                been = true;            

            bool appendAsNew = AppendEdge(baseEdge, parent, path);

            if(!isLink)
                BeenList.Add(baseEdge);

            if (baseEdge == BaseEdge)
                been = false; // hack

            if (appendAsNew /*&& !been*/ && !isLink)
                foreach (IEdge e in baseEdge.To.OutEdgesRaw)
                    ZeroCodeGraph2String_Reccurent(e, level + 1, baseEdge, path);

            AppendAdditionalNewLines(baseEdge);
        }
    }
}
