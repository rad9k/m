using m0.Foundation;
using m0.Graph;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static m0.ZeroCode.String2ZeroCodeGraphProcessing;

namespace m0.ZeroCode.Helpers
{
    public class DictionariesForFormalTextLanguageFactory
    {
        static Dictionary<IVertex, DictionariesForFormalTextLanguage> dict = new Dictionary<IVertex, DictionariesForFormalTextLanguage>();

        static public DictionariesForFormalTextLanguage Get(IVertex formalTextLanguage)
        {
            if (dict.ContainsKey(formalTextLanguage))
                return dict[formalTextLanguage];
            else
            {
                DictionariesForFormalTextLanguage dictionaries = prepareDictionaries_ForFormalTextLanguage(formalTextLanguage);

                dict.Add(formalTextLanguage, dictionaries);

                return dictionaries;
            }
        }

        static private DictionariesForFormalTextLanguage prepareDictionaries_ForFormalTextLanguage(IVertex FormalTextLanguage)
        {
            DictionariesForFormalTextLanguage d = new DictionariesForFormalTextLanguage(FormalTextLanguage);

            prepareImportList_FormalTextLanguage(d, FormalTextLanguage);

            d.examinedKeywords_All = new Dictionary<string, List<keywordTryingData>>();

            d.examinedKeywords_All.Add("", new List<keywordTryingData>());

            d.examinedKeywords_StartInLocalRootOnly = new Dictionary<string, List<keywordTryingData>>();

            d.examinedKeywords_StartInLocalRootOnly.Add("", new List<keywordTryingData>());

            d.keywordInfoDict = new Dictionary<IVertex, KeywordInfo>();


            prepareSpecialKeywordsGroups(d, FormalTextLanguage);


            d.allKeywordsSubstringsDictionary = new Dictionary<char, List<string>>();
            d.allKeywordsSubstringsDictionary_witchoutAlpha = new Dictionary<char, List<string>>();
            d.allKeywordsSubstringsPositiveDictionary_witchoutLinkKeywordParts = new Dictionary<char, List<string>>();
            d.allKeywordsSubstringsNegativeDictionary_witchoutLinkKeywordParts = new Dictionary<char, List<string>>();

            //foreach (IEdge keyword in FormalTextLanguage.GetAll(false, @"Keywords:\$Keyword:"))
            IVertex keywords = GraphUtil.GetQueryOutFirst(FormalTextLanguage, "Keywords", null);

            foreach (IEdge keyword in GraphUtil.GetQueryOut(keywords, "$Keyword", null))
            {
                keywordTryingData ktd = new keywordTryingData(keyword.To, null);

                if (!isSpecialKeyword(keyword.To))
                {
                    // examinedKeywords_All

                    d.examinedKeywords_All[""].Add(ktd);

                    //foreach (IEdge v in ktd.keywordVertex.GetAll(false, "$$KeywordGroup:"))
                    foreach (IEdge v in GraphUtil.GetQueryOut(ktd.keywordVertex, "$$KeywordGroup", null))
                    {
                        string group = (string)v.To.Value;

                        if (!d.examinedKeywords_All.ContainsKey(group))
                            d.examinedKeywords_All.Add(group, new List<keywordTryingData>());

                        d.examinedKeywords_All[group].Add(ktd);
                    }

                    // examinedKeywords_StartInLocalRootOnly

                    //if (keyword.To.Get(false, @"\$$StartInLocalRoot:") != null)
                    bool anyHasStartInLocalRoot = false;

                    foreach (IEdge e in keyword.To.OutEdges)
                        if (GraphUtil.GetQueryOutFirst(e.To, "$$StartInLocalRoot", null) != null)
                            anyHasStartInLocalRoot = true;

                    if (anyHasStartInLocalRoot)
                    {
                        d.examinedKeywords_StartInLocalRootOnly[""].Add(ktd);

                        //foreach (IEdge v in ktd.keywordVertex.GetAll(false, "$$KeywordGroup:"))
                        foreach (IEdge v in GraphUtil.GetQueryOut(ktd.keywordVertex, "$$KeywordGroup", null))
                        {
                            string group = (string)v.To.Value;

                            if (!d.examinedKeywords_StartInLocalRootOnly.ContainsKey(group))
                                d.examinedKeywords_StartInLocalRootOnly.Add(group, new List<keywordTryingData>());

                            d.examinedKeywords_StartInLocalRootOnly[group].Add(ktd);
                        }
                    }

                    //

                    string keywordString = keyword.To.Value.ToString();

                    if (keywordString.Length > 0)
                        addKeywordsSubstrings(d, keywordString);
                }

                PrepareKeywordInfo(d, ktd);

            }

            AddSpaceToAllKeywordsSubstringsDictionaries(d);

            PrepareNegativeNegativeDictionary_witchoutLinkKeywordParts(d);

            //

            return d;
        }

        static void prepareImportList_FormalTextLanguage(DictionariesForFormalTextLanguage d, IVertex FormalTextLanguage)
        {
            //IVertex formalTextLanguageDefaultImports = FormalTextLanguage.Get(false, "DefaultImports:");                
            IVertex formalTextLanguageDefaultImports = GraphUtil.GetQueryOutFirst(FormalTextLanguage, "DefaultImports", null);

            // named imports

            //foreach (IEdge e in formalTextLanguageDefaultImports.GetAll(false, "$ImportMeta:"))
            foreach (IEdge e in GraphUtil.GetQueryOut(formalTextLanguageDefaultImports, "$ImportMeta", null))
            {
                //IVertex v = formalTextLanguageDefaultImports.Get(false, e.To + ":");
                IVertex v = GraphUtil.GetQueryOutFirst(formalTextLanguageDefaultImports, e.To, null);

                if (v != null)
                    d.importMetaList.AddEdge(e.To, v);
            }

            //foreach (IEdge e in formalTextLanguageDefaultImports.GetAll(false, "$Import:"))
            foreach (IEdge e in GraphUtil.GetQueryOut(formalTextLanguageDefaultImports, "$Import", null))
            {
                //IVertex v = formalTextLanguageDefaultImports.Get(false, e.To + ":");
                IVertex v = GraphUtil.GetQueryOutFirst(formalTextLanguageDefaultImports, e.To, null);

                if (v != null)
                    d.importList.AddEdge(e.To, v);
            }

            // direct imports

            //foreach (IEdge e in formalTextLanguageDefaultImports.GetAll(false, "$DirectMeta:"))
            foreach (IEdge e in GraphUtil.GetQueryOut(formalTextLanguageDefaultImports, "$DirectMeta", null))
                d.importDirectMetaList.AddEdge(e.Meta, e.To);

            //foreach (IEdge e in formalTextLanguageDefaultImports.GetAll(false, "$Direct:"))
            foreach (IEdge e in GraphUtil.GetQueryOut(formalTextLanguageDefaultImports, "$Direct", null))
                d.importDirectList.AddEdge(e.Meta, e.To);

        }

        static private void prepareSpecialKeywordsGroups(DictionariesForFormalTextLanguage d, IVertex FormalTextLanguage)
        {
            d.emptyKeywordByGroupsDictionary = ZeroCodeUtil.getFilteredKeywordListByGroup(FormalTextLanguage, "$$EmptyKeyword");

            d.newVertexKeywordByGroupsDictionary = ZeroCodeUtil.getFilteredKeywordListByGroup(FormalTextLanguage, "$$NewVertexKeyword");

            d.linkKeywordByGroupsDictionary = ZeroCodeUtil.getFilteredKeywordListByGroup(FormalTextLanguage, "$$LinkKeyword");
        }

        static private bool isSpecialKeyword(IVertex keyword)
        {
            //if (keyword.Get(false, "$$EmptyKeyword:") != null)
            if (GraphUtil.GetQueryOutFirst(keyword, "$$EmptyKeyword", null) != null)
                return true;

            //if (keyword.Get(false, "$$NewVertexKeyword:") != null)
            if (GraphUtil.GetQueryOutFirst(keyword, "$$NewVertexKeyword", null) != null)
                return true;

            //if (keyword.Get(false, "$$LinkKeyword:") != null)
            if (GraphUtil.GetQueryOutFirst(keyword, "$$LinkKeyword", null) != null)
                return true;

            return false;
        }

        static private void addKeywordsSubstrings(DictionariesForFormalTextLanguage d, string keywordString)
        {
            if (keywordString == "")
                return;

            bool isInsideParameter = false;

            int prevPos = 0;

            int keywordPos;

            for (keywordPos = 0; keywordPos < keywordString.Length; keywordPos++)
            {
                if (!isInsideParameter && ZeroCodeUtil.tryStringMatch(keywordString, keywordPos, "(?<"))
                {
                    isInsideParameter = true;

                    addSubString(d, keywordString.Substring(prevPos, keywordPos - prevPos));
                }

                if (isInsideParameter && ZeroCodeUtil.tryStringMatch(keywordString, keywordPos, ">)"))
                {
                    isInsideParameter = false;
                    prevPos = keywordPos + 2;
                }

                if (ZeroCodeUtil.tryStringMatch(keywordString, keywordPos, "(*"))
                {
                    addSubString(d, keywordString.Substring(prevPos, keywordPos - prevPos));

                    prevPos = keywordPos + 2;
                }

                if (ZeroCodeUtil.tryStringMatch(keywordString, keywordPos, "*)"))
                {
                    addSubString(d, keywordString.Substring(prevPos, keywordPos - prevPos));

                    prevPos = keywordPos + 2;
                }

                if (ZeroCodeUtil.tryStringMatch(keywordString, keywordPos, "(+"))
                {
                    addSubString(d, keywordString.Substring(prevPos, keywordPos - prevPos));

                    prevPos = keywordPos + 2;
                }

                if (ZeroCodeUtil.tryStringMatch(keywordString, keywordPos, "+)"))
                {
                    addSubString(d, keywordString.Substring(prevPos, keywordPos - prevPos));

                    prevPos = keywordPos + 2;
                }
            }

            addSubString(d, keywordString.Substring(prevPos, keywordPos - prevPos));
        }

        static private void addSubString(DictionariesForFormalTextLanguage d, string subString)
        {
            subString = subString.Trim();

            if (subString.Length == 0 || subString[0] == d.CodeGraphLinkPrefix) // XXX CodeGraphLinkPrefix hack for @@
                return;

            addSubString_dictionary(d.allKeywordsSubstringsDictionary, subString);

            if (!Char.IsLetter(subString[0]))
                addSubString_dictionary(d.allKeywordsSubstringsDictionary_witchoutAlpha, subString);

            if (!d.CodeViewTimeLinkKeywordParts.Contains(subString) && !Char.IsLetter(subString[0]))
                addSubString_dictionary(d.allKeywordsSubstringsPositiveDictionary_witchoutLinkKeywordParts, subString);
        }

        static private void addSubString_dictionary(IDictionary<char, List<string>> dict, string subString)
        {
            if (subString.Length == 0)
                return;

            char firstCharacter = subString[0];

            if (dict.ContainsKey(firstCharacter))
            {
                if (!dict[firstCharacter].Contains(subString))
                {
                    dict[firstCharacter].Add(subString);

                    //  MinusZero.Instance.Log(-1, "XX", subString);
                }
            }
            else
            {
                List<string> kl = new List<string>();

                dict.Add(firstCharacter, kl);

                kl.Add(subString);
                //  MinusZero.Instance.Log(-1, "XX", subString);
            }

        }

        static private void PrepareKeywordInfo(DictionariesForFormalTextLanguage d, keywordTryingData ktd)
        {
            KeywordInfo ki = new KeywordInfo();

            if (ktd.keyword.Contains("\r\n"))
                ki.hasCRLF = true;

            IVertex localRoot = GraphUtil.DeepFindOneByMeta(ktd.keywordVertex, "$$LocalRoot", false);

            if (localRoot != null)
            {
                ki.HasLocalRoot = true;

                if ((string)localRoot.Value != "")
                    ki.LocalRootKeywordsGroup = (string)localRoot.Value;
            }

            //if (ktd.keywordVertex.Get(false, "$$NonSelfRecursiveParameters:") != null)
            if (GraphUtil.GetQueryOutFirst(ktd.keywordVertex, "$$NonSelfRecursiveParameters", null) != null)
                ki.NonSelfRecursiveParameters = true;

            d.keywordInfoDict.Add(ktd.keywordVertex, ki);
        }

        static private void AddSpaceToAllKeywordsSubstringsDictionaries(DictionariesForFormalTextLanguage d)
        {
            List<string> l = new List<string>();

            l.Add(" ");

            if (!d.allKeywordsSubstringsDictionary.ContainsKey(' '))
                d.allKeywordsSubstringsDictionary.Add(' ', l);

            if (!d.allKeywordsSubstringsDictionary_witchoutAlpha.ContainsKey(' '))
                d.allKeywordsSubstringsDictionary_witchoutAlpha.Add(' ', l);

            if (!d.allKeywordsSubstringsPositiveDictionary_witchoutLinkKeywordParts.ContainsKey(' '))
                d.allKeywordsSubstringsPositiveDictionary_witchoutLinkKeywordParts.Add(' ', l);
        }

        static private void PrepareNegativeNegativeDictionary_witchoutLinkKeywordParts(DictionariesForFormalTextLanguage d)
        {
            foreach (string s in d.CodeViewTimeLinkKeywordParts)
            {
                char key = s[0];

                GeneralUtil.DictionaryAdd<char, string>(d.allKeywordsSubstringsNegativeDictionary_witchoutLinkKeywordParts, key, s);
            }
        }
    }

    public class ImportInformation
    {
        public IVertex keywordVertex;
        public string regexpString;
    }

    public class DictionariesForFormalTextLanguage
    {
        public IDictionary<string, IList<IVertex>> emptyKeywordByGroupsDictionary;
        public IDictionary<string, IList<IVertex>> newVertexKeywordByGroupsDictionary;
        public IDictionary<string, IList<IVertex>> linkKeywordByGroupsDictionary;
        public IDictionary<string, List<keywordTryingData>> examinedKeywords_All; // all keywords are here
        public IDictionary<string, List<keywordTryingData>> examinedKeywords_StartInLocalRootOnly; // StartInLocalRoot only?
        public IDictionary<char, List<string>> allKeywordsSubstringsDictionary;
        public IDictionary<char, List<string>> allKeywordsSubstringsDictionary_witchoutAlpha;
        public IDictionary<char, List<string>> allKeywordsSubstringsPositiveDictionary_witchoutLinkKeywordParts;
        public Dictionary<char, List<string>> allKeywordsSubstringsNegativeDictionary_witchoutLinkKeywordParts;



        public IDictionary<IVertex, KeywordInfo> keywordInfoDict;

        public IVertex importList = MinusZero.Instance.CreateTempVertex();
        public IVertex importMetaList = MinusZero.Instance.CreateTempVertex();
        public IVertex importDirectList = MinusZero.Instance.CreateTempVertex();
        public IVertex importDirectMetaList = MinusZero.Instance.CreateTempVertex();


        public string CRLFoperator;
        public string MetaSeparator;
        public string CodeGraphVertexPrefix;
        public string CodeGraphVertexSuffix;
        public char LineContinuationPrefix;
        public char CodeGraphLinkPrefix;
        public string CodeGraphLinkKeywordPrefix; // we store it here and in the textlanguage
        public char NewVertexPrefix;
        public char NewVertexSuffix;
        public char EscapedSequencePrefix;
        public char EscapedSequenceSuffix;
        public char EscapeCharacter;
        public string SetIndexPrefix;
        public string SetIndexPostfix;
        public char QuerySlash;

        public HashSet<string> CodeViewTimeLinkKeywordParts;

        public ImportInformation Import;
        public ImportInformation ImportMeta;
        public ImportInformation ImportDirect;
        public ImportInformation ImportDirectMeta;

        string get(string what)
        {
            IVertex v = GraphUtil.GetQueryOutFirst(FormalTextLanguage, what, null);

            if (v != null)
                return v.Value.ToString();

            return null;
        }

        HashSet<string> getHashSet(string what)
        {
            IList<IEdge> v = GraphUtil.GetQueryOut(FormalTextLanguage, what, null);

            HashSet<string> set = new HashSet<string>();

            foreach (IEdge e in v)
                set.Add(e.To.Value.ToString());

            return set;
        }

        ImportInformation getImportInformation(string metaIdentyfication)
        {
            IVertex keywords = GraphUtil.GetQueryOutFirst(FormalTextLanguage, "Keywords", null);

            foreach(IEdge e in keywords)
            {
                IVertex importInfo = GraphUtil.GetQueryOutFirst(e.To, metaIdentyfication, null);

                if (importInfo != null)
                {
                    ImportInformation i = new ImportInformation();
                    i.keywordVertex = e.To;
                    i.regexpString = importInfo.Value.ToString();

                    return i;
                }
            }

            return null;
        }

        IVertex FormalTextLanguage;

        public DictionariesForFormalTextLanguage(IVertex formalTextLanguage)
        {
            FormalTextLanguage = formalTextLanguage;

            CRLFoperator = get("CRLFoperator");
            MetaSeparator = get("MetaSeparator");
            CodeGraphVertexPrefix = get("CodeGraphVertexPrefix");
            CodeGraphVertexSuffix = get("CodeGraphVertexSuffix");
            LineContinuationPrefix = get("LineContinuationPrefix").ToCharArray()[0];
            CodeGraphLinkPrefix = get("CodeGraphLinkPrefix").ToCharArray()[0];
            CodeGraphLinkKeywordPrefix = get("CodeGraphLinkKeywordPrefix"); // we store it here and in the textlanguage
            NewVertexPrefix = get("NewVertexPrefix").ToCharArray()[0];
            NewVertexSuffix = get("NewVertexSuffix").ToCharArray()[0];
            EscapedSequencePrefix = get("EscapedSequencePrefix").ToCharArray()[0];
            EscapedSequenceSuffix = get("EscapedSequenceSuffix").ToCharArray()[0];
            EscapeCharacter = get("EscapeCharacter").ToCharArray()[0];
            SetIndexPrefix = get("SetIndexPrefix");
            SetIndexPostfix = get("SetIndexPostfix");
            QuerySlash = get("QuerySlash").ToCharArray()[0]; ;

            CodeViewTimeLinkKeywordParts = getHashSet("CodeViewTimeLinkKeywordPart");

            Import = getImportInformation("$$Import");
            ImportMeta = getImportInformation("$$ImportMeta");
            ImportDirect = getImportInformation("$$ImportDirect");
            ImportDirectMeta = getImportInformation("$$ImportDirectMeta");
        }
    }
}
