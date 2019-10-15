using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
                IVertex Is = _parent.To.Get(false, "$Is:");

                if (Is != null && Is.Get(false, v.Value.ToString()) == v)
                    return (v.Value.ToString());
            }

            if (_parent != null)
            {
                foreach (IEdge e in VertexOperations.GetChildEdges(_parent.Meta))
                    if (v == e.To)
                        return (v.Value.ToString());
            }
                        

            if (zcg2sp.VerticesDictionary.ContainsKey(v))
                return zcg2sp.VerticesDictionary[v].LinkString;

            linkBeenList = new List<IVertex>();

            shortestLink = "LINK NOT FOUND";
            shortestLinkLength = 99999;

            // linkBeenList.Clear();       

            bool isMetaDirect=false;    

            GetLinkString_Recurrect(v, new List<IEdge>(),ref isMetaDirect);
            

            if(shortestLinkLength!=99999)
               zcg2sp.VerticesDictionary.Add(v, new VertexData(shortestLink, shortestLinkLength));

            if (shortestLinkLength == 99999 && zcg2sp.SubGraphVerticesDictionary.ContainsKey(v))
                return zcg2sp.SubGraphVerticesDictionary[v].LinkString;

            if(shortestLink == "LINK NOT FOUND")
            {
                int x = 0;
            }

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

                            toAppend.Append(ZeroCodeCommon.MetaSeparator);
                        }

                        Append(toAppend,e.To);
                        
                    }
                    else
                    {
                        if (e.Meta == null || GeneralUtil.CompareStrings(e.Meta,"$Empty"))
                        {
                            toAppend.Append(ZeroCodeCommon.MetaSeparator);
                            Append(toAppend, e.To);
                        }else {
                            if(!VertexOperations.IsToVertexEnoughToIdentifyEdge(e.From,e.To))
                                Append(toAppend, e.Meta);

                            toAppend.Append(ZeroCodeCommon.MetaSeparator);
                            Append(toAppend, e.To);
                        }
                    }

                    if(VertexOperations.IsMetaAndToVertexEnoughToIdentifyEdge(e.From, e.Meta, e.To))
                        s.Append(ZeroCodeCommon.stringToPossiblyEscapedString(toAppend.ToString()));
                    else
                    {
                        int pos = 0;
                        IVertex q = e.From.GetAll(false, e.Meta + ":" + e.To);

                        IEdge result;
                        IList<IEdge> results;

                        e.From.QueryOutEdges(e.Meta.Value, e.To.Value, out result, out results);


                        IList<IEdge> listToUse;

                        if (results != null)
                            listToUse = results;
                        else
                        {
                            listToUse = new List<IEdge>();
                            if(result!=null)
                                listToUse.Add(result);
                        }

                        IVertex tv;
                        do
                        {
                            tv = listToUse.ElementAt(pos).To;
                            pos++;
                        } while (tv != e.To);

                        s.Append(ZeroCodeCommon.stringToPossiblyEscapedString(toAppend.ToString()) + ";\"" + pos +"\"");
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

                            if (ikv.Get(false, @"$Is:$ImportMeta") != null)
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

                if (this.zcg2sp.SubGraphVerticesDictionary.ContainsKey(Vertex))
                {
                    canUse = false;

                    string properVertexLink = this.zcg2sp.SubGraphVerticesDictionary[Vertex].LinkString;

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

        public bool DoKeywordDefinitionContainLocalRoot;

        public bool DoKeywordDefinitionContainStartInLocalRoot;

        public bool IsStartInLocalRoot;

        public string newValue;

        public int tabTimesForRootVertex;

        public KeywordMatch(IVertex _KeywordDefinition, ZeroCodeGraph2StringProcessing processing)
        {
            KeywordDefinition = _KeywordDefinition;

            MatchedEdges = new List<IEdge>();

            if (processing.DoKeywordDefinitionContainLocalRoot_Dictionary.ContainsKey(KeywordDefinition))
                DoKeywordDefinitionContainLocalRoot = processing.DoKeywordDefinitionContainLocalRoot_Dictionary[KeywordDefinition];
            else
            {
                if (GraphUtil.DeepFindOneByMeta(KeywordDefinition, "$$LocalRoot", false) != null)
                    DoKeywordDefinitionContainLocalRoot = true;

                processing.DoKeywordDefinitionContainLocalRoot_Dictionary.Add(KeywordDefinition, DoKeywordDefinitionContainLocalRoot);
            }

            if (processing.DoKeywordDefinitionContainStartInLocalRoot_Dictionary.ContainsKey(KeywordDefinition))
                DoKeywordDefinitionContainStartInLocalRoot = processing.DoKeywordDefinitionContainStartInLocalRoot_Dictionary[KeywordDefinition];
            else
            {
                if (GraphUtil.DeepFindOneByMeta(KeywordDefinition, "$$StartInLocalRoot", false) != null)
                    DoKeywordDefinitionContainStartInLocalRoot = true;

                processing.DoKeywordDefinitionContainStartInLocalRoot_Dictionary.Add(KeywordDefinition, DoKeywordDefinitionContainStartInLocalRoot);
            }
        }
    }

    class VertexData
    {
        public string LinkString;
        public int NestedLevel;
        public bool VertexHasBeenAppendedAsNew;

        public VertexData(String s,int l)
        {
            LinkString = s;
            NestedLevel = l;
            VertexHasBeenAppendedAsNew = false;
        }
    }

    class ZeroCodeGraph2StringProcessing
    {
        public IEdge BaseEdge;

        public IList<IEdge> BeenList;
        public IList<IEdge> BeenList_Keyword;

        public StringBuilder Source;

        public IDictionary<IVertex, IList<IVertex>> Imports;

        public IDictionary<IVertex, VertexData> VerticesDictionary;
        public IDictionary<IVertex, VertexData> SubGraphVerticesDictionary;
        public IDictionary<IEdge, KeywordMatch> KeywordMatchedSubGraphEdges;

        public IDictionary<IVertex, bool> DoKeywordDefinitionContainLocalRoot_Dictionary;
        public IDictionary<IVertex, bool> DoKeywordDefinitionContainStartInLocalRoot_Dictionary;

        IVertex FormalTextLanguage;
        IList<IVertex> newVertexKeywordVertexList;
        IList<IVertex> emptyKeywordVertexList;

        public ZeroCodeGraph2StringProcessing(IVertex formalTextLanguage)
        {
            FormalTextLanguage = formalTextLanguage;

            newVertexKeywordVertexList = ZeroCodeUtil.getFilteredKeywordList(FormalTextLanguage, "$$NewVertexKeyword:");
            emptyKeywordVertexList = ZeroCodeUtil.getFilteredKeywordList(FormalTextLanguage, "$$EmptyKeyword:");

        }

        string Tab = "\t";

        string NewLine = "\r\n";

        void SourceAppend(string s)
        {
            string NewLineStringPlusNewLine = getNewLineAndTabsString();            

            s = s.Replace("\r\n", NewLineStringPlusNewLine);

            Source.Append(s);            
        }

        void SourceAppend(char c)
        {
            Source.Append(c);
        }

        void ImportImports(IVertex baseVertex)
        {
            foreach (IEdge e in baseVertex.GetAll(false, "{$Is:$ImportMeta}:"))
                ImportImports_internal(e);

            foreach (IEdge e in baseVertex.GetAll(false, "{$Is:$Import}:"))
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

        string getNewLineAndTabsString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(NewLine);

            for (int i = 0; i < tabTimes; i++)
                sb.Append(Tab);                       

            return sb.ToString();
        }

        void AppendNewLineAndTabs()
        {
            // SourceAppend(getNewLineAndTabsString()); << no as SourceAppend adds getNewLineAndTabsString() on its own

            Source.Append(getNewLineAndTabsString());
        }   

        void AppendAdditionalNewLines(IEdge e)
        {
            IVertex nl = e.To.Get(false, @"$NewLine:");

            if (nl == null)
                return;

            int? nlAsInt = GraphUtil.GetIntegerValue(nl);

            if (nlAsInt != null)
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
                //VerticesAsLink.Add(v, path + "\\" + v.Value); // what is it? not neccesarry now

                SourceAppend(ZeroCodeCommon.stringToNewVertexString(v.Value.ToString())); // XXX we are catching newVertices as keywords so...
                //SourceAppend(v.Value.ToString());
            }
        }

        void AppendIs(IEdge e)
        {            
            SourceAppend("@$Is");
            AppendDoubleColon();

            //SourceAppend(ZeroCodeCommon.stringToLinkString(ZeroCodeCommon.stringToPossiblyEscapedString(e.To.Value.ToString()), true));
            SourceAppend(ZeroCodeCommon.stringToLinkString(ZeroCodeCommon.stringToPossiblyEscapedString(e.To.Value.ToString()),false));            
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
            if (firstEdgeMetaValue == "") // new :)
                return km.MatchedEdges.FirstOrDefault();

            string meta = firstEdgeMetaValue.Substring(0, firstEdgeMetaValue.Length - 1);

            IEdge firstEdge = km.MatchedEdges.FirstOrDefault();

            foreach (IEdge e in km.MatchedEdges)
                if (e.From == firstEdge.From)
                    if (GraphUtil.GetValueAndCompareStrings(e.Meta, meta) || meta== "(?<ANY>)")
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

                    return km.BaseEdge.To.GetAll(false, secondQueryPart).FirstOrDefault();
                }

                firstSlahPosition = queryString.IndexOf('\\');

                firstEdgeQueryPart = queryString.Substring(0, firstSlahPosition);
                secondQueryPart = queryString.Substring(firstSlahPosition + 1, queryString.Length - firstSlahPosition - 1);

                IEdge firstLevelKeyworEdge = GetFirstLevelKeywordEdge(km, firstEdgeQueryPart);

                return firstLevelKeyworEdge.To.GetAll(false, secondQueryPart).FirstOrDefault();
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

            if (baseEdge.To.Get(false, "$$KeywordManyRoot:") != null)
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
            KeywordMatch km = KeywordMatchedSubGraphEdges[keywordEdge];

            if (BeenList_Keyword.Contains(keywordEdge))
                return false;

            BeenList_Keyword.Add(keywordEdge);

            bool whatToReturn = true;

            bool shouldDecreaseTabTimes = false;


            if (km.BaseEdge == keywordEdge /*&& isVertexNew(keywordEdge, GetPathFromKeywordMatchAndKeywordEdge(km, keywordEdge, ""))*/)
            {
                if (!isNested && !km.IsStartInLocalRoot)
                    AppendNewLineAndTabs();
                else if(km.tabTimesForRootVertex==0) // WTF??? /*if(!km.IsStartInLocalRoot)*/ // XXX hmmmmmm
                {                    
                    tabTimes++;
                    shouldDecreaseTabTimes = true;
                }


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

                if (km.newValue != null && km.newValue!="") // if there is ANY:ANY new value
                    SourceAppend("\""+ km.newValue + "\" ");

                if (keywordManyRoot == null)
                {
                    bool zeroMatch;

                    ProcessSingleKeywordSentencePart(km, sentence, false, out zeroMatch);

                    if (zeroMatch&&km.DoKeywordDefinitionContainStartInLocalRoot)
                    { // hack if there are no params but there are local roots
                        IEdge e = km.BaseEdge;

                        string path = GetPathFromKeywordMatchAndKeywordEdge(km, e, null);

                        if (!VertexOperations.IsLink(e))
                            AppendSubVertices(km, e, path);
                    }

                    if (sentence.Contains("(?<SUB>)"))
                        whatToReturn = false;

                    if (shouldDecreaseTabTimes)
                        tabTimes--;
                 
                }
                else
                {
                    int manyGroupEnd = sentence.IndexOf("*)");

                    int manyGroupStart = sentence.IndexOf("(*"); ;

                    string preManySentence = sentence.Substring(0, manyGroupStart);
                    string manySentenceFirst = sentence.Substring(manyGroupStart + 2, manyGroupEnd - manyGroupStart - 2);
                    string postManySentence = sentence.Substring(manyGroupEnd + 2, sentence.Length - manyGroupEnd - 2);

                    string manySentenceSecond = manySentenceFirst;

                    if(sentence.Contains("(+") && sentence.Contains("+)"))
                    {
                        manySentenceSecond = manySentenceSecond.Substring(0, manySentenceSecond.IndexOf("(+"))
                            + manySentenceSecond.Substring(manySentenceSecond.IndexOf("(+") + 2, manySentenceSecond.IndexOf("+)") - manySentenceSecond.IndexOf("(+") - 2)
                            + manySentenceSecond.Substring(manySentenceSecond.IndexOf("+)") + 2);

                        manySentenceFirst = manySentenceFirst.Substring(0, manySentenceFirst.IndexOf("(+"))
                            + manySentenceFirst.Substring(manySentenceFirst.IndexOf("+)") + 2);
                    }                  

                    bool wasThereNewLine = false;

                    bool notInterested;

                    wasThereNewLine = ProcessSingleKeywordSentencePart(km, preManySentence, wasThereNewLine ,out notInterested);

                    wasThereNewLine = ProcessManyKeywordSentencePart(km, manySentenceFirst, manySentenceSecond, keywordManyRoot, keywordManyRootQueryString, keywordManyRootBaseCount, wasThereNewLine);

                    ProcessSingleKeywordSentencePart(km, postManySentence, wasThereNewLine, out notInterested);

                    if (shouldDecreaseTabTimes)                    
                        tabTimes--;
                 
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

        private bool ProcessSingleKeywordSentencePart(KeywordMatch km, string sentence, bool wasThereNewLine, out bool zeroMatch)
        {
            zeroMatch = false;

            // find matches
            Regex rgx = new Regex(@"\(\?(A)?(V)?<[a-zA-Z0-9_]+>\)");

            int prevPos = 0;

            //bool wasThereNewLine = false;

            MatchCollection matchCollection= rgx.Matches(sentence);

            if (matchCollection.Count == 0)
                zeroMatch = true;

            foreach (Match match in matchCollection)
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

                    wasThereNewLine = AppendSubVertices(km, km.BaseEdge, km.BaseEdgePath);
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

                        Regex rgx = new Regex(@"\(\?(A)?(V)?<[a-zA-Z0-9_]+>\)");

                        int prevPos = 0;

                        foreach (Match match in rgx.Matches(sentence))
                        {
                            string queryString = FindKeywordEdge("", keywordManyRoot.To, match.Value);

                            IEdge e;

                            if (queryString == "")
                                e = ee;
                            else
                                e = ee.To.GetAll(false, queryString).FirstOrDefault();

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

                bool wasNewVertex = true; // for empty

                if(!emptyKeywordVertexList.Contains(km.KeywordDefinition))                
                    wasNewVertex = AppendVertex(e, path, false, false, false); // non emptyKeword (standard)
                else
                    SourceAppend(e.To.Value.ToString()); // emptyKeyword handling

                if (wasNewVertex && !VertexOperations.IsLink(e) /*&& e != km.BaseEdge*/)
                    wasThereNewLine = AppendSubVertices(km, e, path);
            }
        }

        private bool AppendSubVertices(KeywordMatch km, IEdge baseEdge, string basePath)
        {
            bool wasThereNewLine = false;

            bool wasFirstNewLine = false;

            foreach (IEdge e in baseEdge.To)
            {
                if (km.BaseEdge != baseEdge && !km.MatchedEdges.Contains(e))
                {
                    if (KeywordMatchedSubGraphEdges.ContainsKey(e)) { // XXX do not quite know what I'm doing, but this is for this below to work :/
                        //	"10"
                        //      <$Is::String >    
                        //   A = @10
                        KeywordMatch km_for_e = KeywordMatchedSubGraphEdges[e];

                        if (wasFirstNewLine == false && km_for_e.IsStartInLocalRoot == false) //  && km_for_e.IsStartInLocalRoot==false XXX
                        {
                            tabTimes++;
                            wasFirstNewLine = true;
                            wasThereNewLine = true;
                        }
                    }
                    AppendEdge(e, null, basePath + "\\" + GraphUtil.GetIdentyfyingQuerySubString_ImportMeta(e));
                }

                if (km.DoKeywordDefinitionContainLocalRoot && km.MatchedEdges.Contains(e) && KeywordMatchedSubGraphEdges[e]!=km)
                    AppendEdge(e, null, basePath + "\\" + GraphUtil.GetIdentyfyingQuerySubString_ImportMeta(e));
                
            }

            if (wasFirstNewLine)
            {
                tabTimes--;

                AppendNewLineAndTabs();
            }

            return wasThereNewLine;
        }
        
        bool ShouldAppendKeywordHere(IEdge e, string path)
        {
            if (SubGraphVerticesDictionary.ContainsKey(e.To) && SubGraphVerticesDictionary[e.To].LinkString == path)
                return true;
            else
                return false;
        }

        bool AppendEdge(IEdge e, IEdge parent, string path)
        {
            if (KeywordMatchedSubGraphEdges.ContainsKey(e))
               if (ShouldAppendKeywordHere(e,path))
                    return AppendKeyword(e, false);
                else
                    if(KeywordMatchedSubGraphEdges[e].BaseEdge.To!=e.To) // :O)
                          return true; // ?????????????????????? or true?

            AppendNewLineAndTabs();

            bool prefixAppended = false;

            if (!IsNullOrEmpty(e.Meta))
            {
                AppendPrefix();

                prefixAppended = true;

                //AppendAsLink(e.Meta, parent, true);

                AppendAsLink(e.Meta, parent, false); // XXX we want cvtq linx in <> with @

                AppendDoubleColon();
            }

            //return AppendVertex(e, path, prefixAppended, true, true);
            return AppendVertex(e, path, prefixAppended, true, false); // XXX we want cvtq linx in <> with @
        }

        private bool AppendVertex(IEdge e, string path, bool prefixAppended, bool appendSuffix, bool hideLinkPrefix)
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
                //  SourceAppend("<" + SubGraphVerticesAsLink[e.To].String + ":" + path + ">");

                if (isVertexNew(e, path))
                {
                    AppendAsNew(e.To);

                    if (appendSuffix && prefixAppended)
                        AppendSuffix();

                    return true;
                }
                else
                {
                    if (appendSuffix)
                        AppendPrefix();

                    AppendAsLink(e.To, null, hideLinkPrefix);

                    if (appendSuffix)
                        AppendSuffix();

                    return false;
                }
            }
        }

        private bool isVertexNew(IEdge e, string path)
        {
            if (!SubGraphVerticesDictionary.ContainsKey(e.To))
                return false; // is it possible?

            VertexData eVertexData = SubGraphVerticesDictionary[e.To];
            string firstQuery = eVertexData.LinkString;
            string secondQuery = path;

            if ((path == null || firstQuery == secondQuery) && !eVertexData.VertexHasBeenAppendedAsNew)
            {
                eVertexData.VertexHasBeenAppendedAsNew = true;
                return true;
            }

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

            if (e.Meta.Get(false, @"$Is:$Import") != null)
                return false;

            if (e.Meta.Get(false, @"$Is:$ImportMeta") != null)
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
            //if (keywordEdge.To.Get(false, "$$KeywordManyRoot:") != null)
            // return true; // WTF ???? or Meta? eigher does not work for function parameters

            if(ZeroCodeUtil.IsDoubleDolarMeta(keywordEdge))
            //if (isNotComparableKeywordEdge(keywordEdge.Meta.ToString()))
                return true;

            //string searchString;

            string searchString_firstPart = ZeroCodeCommon.stringToPossiblyEscapedString(keywordEdge.Meta.ToString());
            string searchString_secondPart = "";

            if (!IsKeywordVertexWildcard(keywordEdge.To))
                //searchString_secondPart = ZeroCodeCommon.stringToPossiblyEscapedString(keywordEdge.To.ToString());
                searchString_secondPart = keywordEdge.To.ToString();

            //searchString = searchString_firstPart + ":" + searchString_secondPart;

            //IVertex search = parentToCheck.GetAll(false, searchString); // current query implementation does not handle quotas properly : {}, \ keywords does not work properly

            bool toReturn = false;

            // foreach (IEdge searchResult in search)
            foreach (IEdge searchResult in parentToCheck)
                if(GeneralUtil.CompareStrings(searchString_firstPart,searchResult.Meta.Value)&& 
                    (searchString_secondPart == "" || GeneralUtil.CompareStrings(searchString_secondPart, searchResult.To.Value)))
            {
           

                if (/*!KeywordMatchedSubGraphEdges.ContainsKey(searchResult) &&*/ !currentMatchGraphEdgeList.Contains(searchResult))
                {
                        if (!VertexOperations.IsLink(keywordEdge))
                            foreach (IEdge subKeywordEdge in keywordEdge.To)
                                if (/*!IsLink(subKeywordEdge) 
                                && !isNotComparableKeywordEdge(subKeywordEdge.Meta.ToString()) // WTF ????*/
                                !ZeroCodeUtil.IsDoubleDolarMeta(subKeywordEdge)
                                && GetGraphMatch(searchResult.To, subKeywordEdge) == false)
                                return false;

                    //if (!currentMatchGraphEdgeList.Contains(searchResult))
                    {
                        currentMatchGraphEdgeList.Add(searchResult);

                        if (keywordEdge.To.Get(false, "$$KeywordManyRoot:") == null)
                            return true;
                        else
                            toReturn = true; // this is strange. but we are leaving it AS IS. not to break something
                    }
                }
            }

            if (keywordEdge.To.Get(false, "$$KeywordManyRoot:") != null || keywordEdge.To.Get(false, "$$LocalRoot:") != null)
                return true;

            return toReturn;
        }

        List<IEdge> currentMatchGraphEdgeList;

        public IList<IEdge> MatchGraphs_import(IEdge edgeToCheck)
        {
            IEdge secondEdge = edgeToCheck.From.GetAll(false, ZeroCodeCommon.stringToPossiblyEscapedString(edgeToCheck.To.ToString()) + ":").FirstOrDefault();

            if (secondEdge != null)
            {
                currentMatchGraphEdgeList.Add(secondEdge);
                return currentMatchGraphEdgeList;
            }

            return null;
        }

        public IList<IEdge> MatchGraphs(IEdge edgeToCheck, IVertex graphToCompare, out string newValueString)
        {
            newValueString = null;

            currentMatchGraphEdgeList = new List<IEdge>();

            IVertex firstMatchingEdgesInGraphToCompare = graphToCompare.GetAll(false, ZeroCodeCommon.stringToPossiblyEscapedString(edgeToCheck.Meta.ToString()) + ":");

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
                firstMatchingEdgesInGraphToCompare = graphToCompare.GetAll(false, "(?<ANY>):");

                if (firstMatchingEdgesInGraphToCompare.Count() > 0)
                {
                    IEdge e = firstMatchingEdgesInGraphToCompare.FirstOrDefault();

                    if (IsKeywordVertexWildcard(e.To))
                    {
                        firstMatchEdgeInGraphToCompare = e;

                        if (GeneralUtil.CompareStrings(e.To, "(?<ANY>)") && edgeToCheck.To.Value!=null) // we are going to have newValueKeyword here :)
                            newValueString = edgeToCheck.To.Value.ToString();
                    }
                    else
                        if (GraphUtil.GetValueAndCompareStrings(edgeToCheck.To, (String)e.To.Value))
                        firstMatchEdgeInGraphToCompare = e;
                }
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

        public void CheckVertexIfItMachesAnyKeywordGraphs(IEdge edgeToCheck, string path, IEdge edgeToCheck_parent)
        {
            //if (KeywordMatchedSubGraphEdges.ContainsKey(edgeToCheck))
            //   return;

            bool thereWasMatch = false;

            foreach (IEdge keyword in FormalTextLanguage.GetAll(false, @"Keywords:\$Keyword:"))
                if (!newVertexKeywordVertexList.Contains(keyword.To))
                    if (CheckMatchForKeywordAndAddKeywordMatchIfThereIsMatch(edgeToCheck, path, edgeToCheck_parent, keyword.To))
                        thereWasMatch = true;

            //if(!thereWasMatch && !KeywordMatchedSubGraphEdges.ContainsKey(edgeToCheck)) // WE CAN HAVE KEYWORD MATCH FOR NEW VERTEX HERE!
              //  CheckMatchForKeywordAndAddKeywordMatchIfThereIsMatch(edgeToCheck, path, edgeToCheck_parent, newVertexKeywordVertexList[0]);
        }

        private bool CheckMatchForKeywordAndAddKeywordMatchIfThereIsMatch(IEdge edgeToCheck, string path, IEdge edgeToCheck_parent, IVertex keywordVertex)
        {
            bool thereWasMatch = false;

            string newValueKeyword;

            IList<IEdge> matchedEdges = MatchGraphs(edgeToCheck, keywordVertex, out newValueKeyword);

            if (matchedEdges != null && matchedEdges.Count > 0)
            {
                thereWasMatch = true;                

                KeywordMatch match = new KeywordMatch(keywordVertex, this);

                if (newValueKeyword != null)
                    match.newValue = newValueKeyword;

                // match.BaseEdge = matchedEdges[0];
                match.BaseEdge = edgeToCheck; // same as above
                match.BaseEdgePath = path;

                match.BaseEdgePathLength = getNumberOfOccurances(match.BaseEdgePath, '\\');

                /*if (KeywordMatchedSubGraphEdges.ContainsKey(edgeToCheck_parent))
                {
                    KeywordMatch match_parent = KeywordMatchedSubGraphEdges[edgeToCheck_parent];

                    if (match_parent.DoKeywordDefinitionContainLocalRoot && match.DoKeywordDefinitionContainStartInLocalRoot)
                        match.IsStartInLocalRoot = true;
                }
                else
                {*/
                    if (!GraphUtil.GetValueAndCompareStrings(edgeToCheck.Meta, "$Empty") && match.DoKeywordDefinitionContainStartInLocalRoot)
                        match.IsStartInLocalRoot = true; // XXX this is done for "a"\
                //} // XXX

                foreach (IEdge e in matchedEdges)
                {
                    match.MatchedEdges.Add(e);

                    if (KeywordMatchedSubGraphEdges.ContainsKey(e))
                    {
                        if (match.BaseEdge.To == e.To)
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

                    }
                    else
                        KeywordMatchedSubGraphEdges.Add(e, match);
                }
            }

            return thereWasMatch;
        }

        public void GetLinksForSubGraphVertices(IEdge e, string path, int nestedLevel)
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

                    if (SubGraphVerticesDictionary.ContainsKey(ee.To))
                    {
                        VertexData l = SubGraphVerticesDictionary[ee.To];

                        if (nestedLevel < l.NestedLevel)
                        {
                            VertexData vd = SubGraphVerticesDictionary[ee.To];

                            vd.LinkString = LinkString;
                            vd.NestedLevel = nestedLevel;

                            beenThereButNeedToReEnter = true;
                        }
                    }else
                        SubGraphVerticesDictionary.Add(ee.To, new VertexData(LinkString, nestedLevel));

                    if(beenThereButNeedToReEnter || !BeenList.Contains(ee))
                        GetLinksForSubGraphVertices(ee, LinkString, nestedLevel+1);
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

                    CheckVertexIfItMachesAnyKeywordGraphs(ee, LinkString, e);                    

                    if (!BeenList.Contains(ee)&&!VertexOperations.IsLink(ee))
                        MatchKeywords(ee, LinkString);
                }
        }

        void AppendPrefix()
        {
            SourceAppend(ZeroCodeCommon.CodeGraphVertexPrefix);
        }

        void AppendSuffix()
        {
            SourceAppend(ZeroCodeCommon.CodeGraphVertexSuffix);
        }

        int levelCorrection = 0;

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

            //if (BeenList.Contains(baseEdge)&&!isLink)
            //   been = true;     /?????????  

            KeywordMatch thisKm = null;

            if (KeywordMatchedSubGraphEdges.ContainsKey(baseEdge))
                if (ShouldAppendKeywordHere(baseEdge, path))
                {
                    thisKm = KeywordMatchedSubGraphEdges[baseEdge];
                    thisKm.tabTimesForRootVertex = level;
                }

            bool appendAsNew = AppendEdge(baseEdge, parent, path);

            if(!isLink)
                BeenList.Add(baseEdge);

            if (baseEdge == BaseEdge)
                been = false; // hack

            if (/*appendAsNew && !been &&*/ !isLink)
                foreach (IEdge e in baseEdge.To.OutEdgesRaw)
                {
                    int newLevel = level + 1;

                    if (KeywordMatchedSubGraphEdges.ContainsKey(e))
                    {
                        KeywordMatch localKm = KeywordMatchedSubGraphEdges[e];

                        if (localKm.IsStartInLocalRoot)
                        {
                            if (thisKm != null)
                                newLevel = thisKm.tabTimesForRootVertex;
                            else
                                newLevel = level; // XXX :) should work level should be preserved at the km level
                        }
                    }

                    ZeroCodeGraph2String_Reccurent(e, newLevel, baseEdge, path);
                }

            AppendAdditionalNewLines(baseEdge);
        }


        public string Process(IEdge graphBaseEdge)
        {
            BeenList = new List<IEdge>();
            BeenList_Keyword = new List<IEdge>();
            Source = new StringBuilder();
            Imports = new Dictionary<IVertex, IList<IVertex>>();
            VerticesDictionary = new Dictionary<IVertex, VertexData>();
            SubGraphVerticesDictionary = new Dictionary<IVertex, VertexData>();
            KeywordMatchedSubGraphEdges = new Dictionary<IEdge, KeywordMatch>();

            DoKeywordDefinitionContainLocalRoot_Dictionary = new Dictionary<IVertex, bool>();
            DoKeywordDefinitionContainStartInLocalRoot_Dictionary = new Dictionary<IVertex, bool>();

            // 

            BaseEdge = graphBaseEdge;

            GetLinksForSubGraphVertices(BaseEdge, null, 0);

            BeenList.Clear();

            MatchKeywords(BaseEdge, null);

            BeenList.Clear();

            //

            BeenList.Add(graphBaseEdge);

            //

            ImportImports(FormalTextLanguage.Get(false, "DefaultImports:"));
            ImportImports(graphBaseEdge.To);

            //AppendPrefix();
            AppendAsNew(graphBaseEdge.To);
            //AppendSuffix();

            foreach (IEdge e in graphBaseEdge.To.OutEdgesRaw)
                ZeroCodeGraph2String_Reccurent(e, 1, graphBaseEdge, null);

            return Source.ToString();
        }
    }
}
