using m0.Foundation;
using m0.Graph;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static m0.ZeroCode.Text2GraphProcessing;

namespace m0.ZeroCode.Helpers
{
    public class DictionariesForFormalTextLanguageFactory
    {
        static Dictionary<IVertex, FormalTextLanguageDictinaries> dict = new Dictionary<IVertex, FormalTextLanguageDictinaries>();        
        
        static public FormalTextLanguageDictinaries Get(IVertex formalTextLanguage)
        {
            if (dict.ContainsKey(formalTextLanguage))
                return dict[formalTextLanguage];
            else
            {
                FormalTextLanguageDictinaries dictionaries = prepareDictionaries_ForFormalTextLanguage(formalTextLanguage);

                dict.Add(formalTextLanguage, dictionaries);

                return dictionaries;
            }
        }

        static private FormalTextLanguageDictinaries prepareDictionaries_ForFormalTextLanguage(IVertex FormalTextLanguage)
        {
            FormalTextLanguageDictinaries d = new FormalTextLanguageDictinaries(FormalTextLanguage);

            prepareImportList_FormalTextLanguage(d, FormalTextLanguage);

            d.examinedKeywords_All = new Dictionary<string, List<keywordTryingData>>();

            d.examinedKeywords_All.Add("", new List<keywordTryingData>());

            d.examinedKeywords_StartInLocalRootOnly = new Dictionary<string, List<keywordTryingData>>();

            d.examinedKeywords_StartInLocalRootOnly.Add("", new List<keywordTryingData>());

            d.keywordInfoDict = new Dictionary<IVertex, KeywordInfo>();


            prepareSpecialKeywordsGroups(d, FormalTextLanguage);


            d.allKeywordsSubstringsDictionary = new Dictionary<char, List<string>>();
            d.allKeywordsSubstringsDictionary_onlyFirstPart = new Dictionary<char, List<string>>();
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

            d.instructions_HasNextEdge = new HashSet<IVertex>();
            d.instructions_NextAtomRoot = new HashSet<IVertex>();

            addInstructions(d);

            //

            prepare_Graph2Text(d);

            prepare_viewTokens(d);

            return d;
        }

        private static void prepare_viewTokens(FormalTextLanguageDictinaries d)
        {
            d.viewTokensDictionary = new Dictionary<char, List<ViewToken>>();

            foreach(IEdge e in GraphUtil.GetQueryOut(d.FormalTextLanguageVertex, "ViewToken", null))
            {
                IVertex tokenVertex = e.To;
                string tokenString = tokenVertex.Value.ToString();
                IVertex colorVertex = GraphUtil.GetQueryOutFirst(tokenVertex, "Color", null);

                ViewToken vt = new ViewToken();
                vt.tokenString = tokenString;
                vt.colorVertex = colorVertex;
                vt.tokenVertex = tokenVertex;

                char firstChar = tokenString[0];

                if (!d.viewTokensDictionary.ContainsKey(firstChar))
                    d.viewTokensDictionary.Add(firstChar, new List<ViewToken>());

                d.viewTokensDictionary[firstChar].Add(vt);
            }
        }

        static void prepare_Graph2Text(FormalTextLanguageDictinaries d)
        {
            d.firstEdge2KeywordVertex = new Dictionary<string, List<IVertex>>();
            d.firstEdgeANYIs2KeywordVertex = new Dictionary<string, List<IVertex>>();

            IVertex keywordsVertex = GraphUtil.GetQueryOutFirst(d.FormalTextLanguageVertex, "Keywords", null);

            foreach (IEdge keywordEdge in GraphUtil.GetQueryOut(keywordsVertex, "$Keyword", null))
            {
                IVertex keywordVertex = keywordEdge.To;

                IEdge firstEdge = null;

                foreach (IEdge e in keywordVertex)
                    if (!GraphUtil.IsMetaDoubleDollar(e))
                    {
                        firstEdge = e;
                        break;
                    }

                if (firstEdge != null)
                {
                    string firstEdgeMetaValue = firstEdge.Meta.Value.ToString();

                    if (firstEdgeMetaValue != "(?<ANY>)")
                        GeneralUtil.DictionaryAdd<string, IVertex>(d.firstEdge2KeywordVertex, firstEdgeMetaValue, keywordVertex);
                    else
                    {
                        IEdge firstEdgeANYIs = GraphUtil.GetQueryOutFirstEdge(firstEdge.To, "$Is", null);

                        if (firstEdgeANYIs != null) {
                            string firstEdgeANYIsToValue = firstEdgeANYIs.To.Value.ToString();
                            GeneralUtil.DictionaryAdd<string, IVertex>(d.firstEdgeANYIs2KeywordVertex, firstEdgeANYIsToValue, keywordVertex);
                        }
                    }
                }
            }
        }

        static void addInstructions(FormalTextLanguageDictinaries d)
        {
            IVertex root = MinusZero.Instance.root;

            IVertex system = GraphUtil.GetQueryOutFirst(root, null, "System");

            IVertex meta = GraphUtil.GetQueryOutFirst(system, null, "Meta");

            IVertex zeroUML = GraphUtil.GetQueryOutFirst(meta, null, "ZeroUML");


            foreach (IEdge e in zeroUML)
            {
                addInstructions_checkInstruction(d, e.To);

                foreach (IEdge ee in e.To)
                    addInstructions_checkInstruction(d, ee.To);
            }
        }

        static void addInstructions_checkInstruction(FormalTextLanguageDictinaries d, IVertex v)
        {
            if (GraphUtil.ExistQueryOut(v, "$$NextAtomRoot", null))
                d.instructions_NextAtomRoot.Add(v);

            if (GraphUtil.ExistQueryOut(v, null, "Next"))
                d.instructions_HasNextEdge.Add(v);
        }

        static void prepareImportList_FormalTextLanguage(FormalTextLanguageDictinaries d, IVertex FormalTextLanguage)
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

            //foreach (IEdge e in formalTextLanguageDefaultImports.GetAll(false, "$ImportDirectMeta:"))
            foreach (IEdge e in GraphUtil.GetQueryOut(formalTextLanguageDefaultImports, "$ImportDirectMeta", null))
                d.importDirectMetaList.AddEdge(e.Meta, e.To);

            //foreach (IEdge e in formalTextLanguageDefaultImports.GetAll(false, "$ImportDirect:"))
            foreach (IEdge e in GraphUtil.GetQueryOut(formalTextLanguageDefaultImports, "$ImportDirect", null))
                d.importDirectList.AddEdge(e.Meta, e.To);

        }

        static private void prepareSpecialKeywordsGroups(FormalTextLanguageDictinaries d, IVertex FormalTextLanguage)
        {
            d.emptyKeywordByGroups = ZeroCodeUtil.GetFilteredKeywordListByGroup(FormalTextLanguage, "$$EmptyKeyword");

            d.newVertexKeywordByGroups = ZeroCodeUtil.GetFilteredKeywordListByGroup(FormalTextLanguage, "$$NewVertexKeyword");

            d.linkKeywordByGroupsDictionary = ZeroCodeUtil.GetFilteredKeywordListByGroup(FormalTextLanguage, "$$LinkKeyword");
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

        static private void addKeywordsSubstrings(FormalTextLanguageDictinaries d, string keywordString)
        {
            if (keywordString == "")
                return;

            bool isInsideParameter = false;

            int prevPos = 0;

            int keywordPos;

            bool isFirstAdd = true;

            for (keywordPos = 0; keywordPos < keywordString.Length; keywordPos++)
            {
                if (!isInsideParameter && ZeroCodeUtil.TryStringMatch(keywordString, keywordPos, "(?<"))
                {
                    isInsideParameter = true;

                    addSubString(d, keywordString.Substring(prevPos, keywordPos - prevPos), isFirstAdd);
                    isFirstAdd = false;
                }

                if (isInsideParameter && ZeroCodeUtil.TryStringMatch(keywordString, keywordPos, ">)"))
                {
                    isInsideParameter = false;
                    prevPos = keywordPos + 2;
                }

                if (ZeroCodeUtil.TryStringMatch(keywordString, keywordPos, "(*"))
                {
                    addSubString(d, keywordString.Substring(prevPos, keywordPos - prevPos), isFirstAdd);
                    isFirstAdd = false;

                    prevPos = keywordPos + 2;
                }

                if (ZeroCodeUtil.TryStringMatch(keywordString, keywordPos, "*)"))
                {
                    addSubString(d, keywordString.Substring(prevPos, keywordPos - prevPos), isFirstAdd);
                    isFirstAdd = false;

                    prevPos = keywordPos + 2;
                }

                if (ZeroCodeUtil.TryStringMatch(keywordString, keywordPos, "(+"))
                {
                    addSubString(d, keywordString.Substring(prevPos, keywordPos - prevPos), isFirstAdd);
                    isFirstAdd = false;

                    prevPos = keywordPos + 2;
                }

                if (ZeroCodeUtil.TryStringMatch(keywordString, keywordPos, "+)"))
                {
                    addSubString(d, keywordString.Substring(prevPos, keywordPos - prevPos), isFirstAdd);
                    isFirstAdd = false;

                    prevPos = keywordPos + 2;
                }
            }

            addSubString(d, keywordString.Substring(prevPos, keywordPos - prevPos), isFirstAdd);
        }

        static private void addSubString(FormalTextLanguageDictinaries d, string subString, bool isFirstAdd)
        {
            subString = subString.Trim();

            if (subString.Length == 0 || subString[0] == d.CodeGraphLinkPrefix) // XXX CodeGraphLinkPrefix hack for @@
                return;

            addSubString_dictionary(d.allKeywordsSubstringsDictionary, subString);

            if (isFirstAdd)
                addSubString_dictionary(d.allKeywordsSubstringsDictionary_onlyFirstPart, subString);

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

        static private void PrepareKeywordInfo(FormalTextLanguageDictinaries d, keywordTryingData ktd)
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

        static private void AddSpaceToAllKeywordsSubstringsDictionaries(FormalTextLanguageDictinaries d)
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

        static private void PrepareNegativeNegativeDictionary_witchoutLinkKeywordParts(FormalTextLanguageDictinaries d)
        {
            foreach (string s in d.CodeViewTimeLinkKeywordParts)
            {
                char key = s[0];

                GeneralUtil.DictionaryAdd<char, string>(d.allKeywordsSubstringsNegativeDictionary_witchoutLinkKeywordParts, key, s);
            }
        }
    }
}
