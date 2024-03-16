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
    public class ImportInformation
    {
        public IVertex keywordVertex;
        public string regexpString;
    }

    public class FormalTextLanguageDictinaries
    {
        public IVertex FormalTextLanguageVertex;

        // Graph2Text

        public Dictionary<string, List<IVertex>> firstEdge2KeywordVertex;
        public Dictionary<string, List<IVertex>> firstEdgeANYIs2KeywordVertex;

        // Text2Graph

        public IDictionary<string, IList<IVertex>> emptyKeywordByGroups;
        public IDictionary<string, IList<IVertex>> newVertexKeywordByGroups;
        public IDictionary<string, IList<IVertex>> linkKeywordByGroupsDictionary;
        public IDictionary<string, List<keywordTryingData>> examinedKeywords_All; // all keywords are here
        public IDictionary<string, List<keywordTryingData>> examinedKeywords_StartInLocalRootOnly; // StartInLocalRoot only?
        public IDictionary<char, List<string>> allKeywordsSubstringsDictionary;
        public IDictionary<char, List<string>> allKeywordsSubstringsDictionary_onlyFirstPart;      
        public IDictionary<char, List<string>> allKeywordsSubstringsDictionary_witchoutAlpha;
        public IDictionary<char, List<string>> allKeywordsSubstringsPositiveDictionary_witchoutLinkKeywordParts;
        public Dictionary<char, List<string>> allKeywordsSubstringsNegativeDictionary_witchoutLinkKeywordParts;

        public HashSet<IVertex> instructions_NextAtomRoot;
        public HashSet<IVertex> instructions_HasNextEdge;


        public IDictionary<IVertex, KeywordInfo> keywordInfoDict;

        public IVertex importList = MinusZero.Instance.CreateTempVertex();
        public IVertex importMetaList = MinusZero.Instance.CreateTempVertex();
        public IVertex importDirectList = MinusZero.Instance.CreateTempVertex();
        public IVertex importDirectMetaList = MinusZero.Instance.CreateTempVertex();


        public string CRLFoperator;
        public zstring zCRLFoperator;
        public string MetaSeparator;
        public string CodeGraphVertexPrefix;
        public zstring zCodeGraphVertexPrefix;
        public string CodeGraphVertexSuffix;
        public zstring zCodeGraphVertexSuffix;
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
        public IVertex NextAtomMeta;

        public HashSet<string> CodeViewTimeLinkKeywordParts;

        public ImportInformation Import;
        public ImportInformation ImportMeta;
        public ImportInformation ImportDirect;
        public ImportInformation ImportDirectMeta;

        string get(string what)
        {
            IVertex v = GraphUtil.GetQueryOutFirst(FormalTextLanguageVertex, what, null);

            if (v != null)
                return v.Value.ToString();

            return null;
        }

        HashSet<string> getHashSet(string what)
        {
            IList<IEdge> v = GraphUtil.GetQueryOut(FormalTextLanguageVertex, what, null);

            HashSet<string> set = new HashSet<string>();

            foreach (IEdge e in v)
                set.Add(e.To.Value.ToString());

            return set;
        }

        ImportInformation getImportInformation(string metaIdentyfication)
        {
            IVertex keywords = GraphUtil.GetQueryOutFirst(FormalTextLanguageVertex, "Keywords", null);

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

        public FormalTextLanguageDictinaries(IVertex formalTextLanguage)
        {
            FormalTextLanguageVertex = formalTextLanguage;

            importList.AddExternalReference();
            importMetaList.AddExternalReference();
            importDirectList.AddExternalReference();
            importDirectMetaList.AddExternalReference();

            CRLFoperator = get("CRLFoperator");
            zCRLFoperator = new zstring(CRLFoperator);
            MetaSeparator = get("MetaSeparator");
            CodeGraphVertexPrefix = get("CodeGraphVertexPrefix");
            zCodeGraphVertexPrefix = new zstring(CodeGraphVertexPrefix);
            CodeGraphVertexSuffix = get("CodeGraphVertexSuffix");
            zCodeGraphVertexSuffix = new zstring(CodeGraphVertexSuffix);
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
            QuerySlash = get("QuerySlash").ToCharArray()[0];

            NextAtomMeta = GraphUtil.GetQueryOutFirst(FormalTextLanguageVertex, "NextAtomEdge", null);

            CodeViewTimeLinkKeywordParts = getHashSet("CodeViewTimeLinkKeywordPart");

            Import = getImportInformation("$$Import");
            ImportMeta = getImportInformation("$$ImportMeta");
            ImportDirect = getImportInformation("$$ImportDirect");
            ImportDirectMeta = getImportInformation("$$ImportDirectMeta");
        }
    }
}
