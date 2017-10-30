using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    public class String2ZeroCodeGraphProcessing
    {
        IVertex baseVertex;
        string text;

        int pos;
        int lineNo;

        int firstCharacterPos_relativeToText; 

        string currentLine;
        string currentLineNoTabs;

        int firstCharacterPos_relativeToCurrentLine;
        int prevFirstCharacterPos_relativeToCurrentLine;

        //

        IVertex r = m0.MinusZero.Instance.Root;

        bool ParseLine()
        {
            if (skipParse)
            {
                skipParse = false;
                return true;
            }

            prevFirstCharacterPos_relativeToCurrentLine = firstCharacterPos_relativeToCurrentLine;
            firstCharacterPos_relativeToCurrentLine = 0;

            if (pos >= text.Length)
                return false;

            char c = text[pos];

            int begPos = pos;

            bool endOfTabsReached = false;

            while (c != '\n' && c != '\r' && pos < (text.Length-1))
            {
                if (c != '\t' && !endOfTabsReached)
                {
                    endOfTabsReached = true;
                    firstCharacterPos_relativeToCurrentLine = pos-begPos;
                    firstCharacterPos_relativeToText = pos;
                }

                pos++;
                c = text[pos];
            }

            if(pos == (text.Length - 1))
                currentLine = text.Substring(begPos,pos - begPos + 1);
            else
                currentLine = text.Substring(begPos, pos - begPos);

            currentLineNoTabs = currentLine.Substring(firstCharacterPos_relativeToCurrentLine).Trim(); // can try witchout Trim

            lineNo++;

            pos += 2;

            return true;
        }

        IVertex importList;
        IVertex importMetaList;
        IVertex importDirectList;
        IVertex importDirectMetaList;

        void prepareImportList()
        {
            importList = MinusZero.Instance.CreateTempVertex();
            importMetaList = MinusZero.Instance.CreateTempVertex();
            importDirectList = MinusZero.Instance.CreateTempVertex();
            importDirectMetaList = MinusZero.Instance.CreateTempVertex();

            prepareImportList_User();

            prepareImportList_FromString();
        }

        void prepareImportList_User()
        {
            IVertex codeSettings = r.Get(@"User\CurrentUser:\CodeSettings:");

            // named imports

            foreach (IEdge e in codeSettings.GetAll("$ImportMeta:"))
            {
                IVertex v = codeSettings.Get(e.To + ":");

                if (v != null)
                    importMetaList.AddEdge(e.To, v);
            }

            foreach (IEdge e in codeSettings.GetAll("$Import:"))
            {
                IVertex v = codeSettings.Get(e.To + ":");

                if (v != null)
                    importList.AddEdge(e.To, v);
            }

            // direct imports

            foreach (IEdge e in codeSettings.GetAll("$DirectMeta:"))
                importDirectMetaList.AddEdge(e.Meta, e.To);
            

            foreach (IEdge e in codeSettings.GetAll("$Direct:"))
                importDirectList.AddEdge(e.Meta, e.To);
            
        }
        void prepareImportList_FromString()
        {

            // "import (?<name>) (?<link>)"

            prepareImportList_FromString_import();

            // "import meta (?<name>) (?<link>)"

            prepareImportList_FromString_importMeta();

            // "import direct (?<link>)"

            prepareImportList_FromString_importDirect();

            // "import direct meta (?<link>)"

            prepareImportList_FromString_importDirectMeta();


        }

        void prepareImportList_FromString_import()
        {
            Regex rgx = new Regex("import[ ]+\"(?<name>.*)\"[ ]+@(?<link>.*[^ ])[ ]*\\r");

            foreach (Match match in rgx.Matches(text))
            {
                string name = match.Groups["name"].Value;

                string link = match.Groups["link"].Value;

                IVertex namev = MinusZero.Instance.CreateTempVertex();

                namev.Value = name;

                IVertex target=r.Get(link);

                if (target != null)
                    importList.AddEdge(namev, target);
                else
                    importList.AddEdge(namev, MinusZero.Instance.Empty);
            }
        }

        void prepareImportList_FromString_importMeta()
        {
            Regex rgx = new Regex("import[ ]+meta[ ]+\"(?<name>.*)\"[ ]+@(?<link>.*[^ ])[ ]*\\r");

            foreach (Match match in rgx.Matches(text))
            {
                string name = match.Groups["name"].Value;

                string link = match.Groups["link"].Value;


                IVertex namev = MinusZero.Instance.CreateTempVertex();

                namev.Value = name;


                IVertex target = r.Get(link);

                if (target != null)
                    importMetaList.AddEdge(namev, target);
                else
                    importMetaList.AddEdge(namev, MinusZero.Instance.Empty);
            }
        }

        IVertex smb;

        IVertex Direct;

        IVertex DirectMeta;

        void setupHelpVariables()
        {
            smb = r.Get(@"System\Meta\Base");

            Direct = smb.Get("$Direct");

            DirectMeta = smb.Get("$DirectMeta");
        }

        void prepareImportList_FromString_importDirect()
        {
            Regex rgx = new Regex("import[ ]+direct[ ]+@(?<link>.*[^ ])[ ]*\\r");

            foreach (Match match in rgx.Matches(text))
            {
                string link = match.Groups["link"].Value;

                IVertex target = r.Get(link);

                if (target != null)
                    importDirectList.AddEdge(Direct, target);
                else
                    importDirectList.AddEdge(Direct, MinusZero.Instance.Empty);
            }
        }

        void prepareImportList_FromString_importDirectMeta()
        {
            Regex rgx = new Regex("import[ ]+direct[ ]+meta[ ]+@(?<link>.*[^ ])[ ]*\\r");

            foreach (Match match in rgx.Matches(text))
            {
                string link = match.Groups["link"].Value;

                IVertex target = r.Get(link);

                if (target != null)
                    importDirectMetaList.AddEdge(Direct, target);
                else
                    importDirectMetaList.AddEdge(Direct, MinusZero.Instance.Empty);
            }
        }

        void initVariables()
        {
            pos = 0;
            firstCharacterPos_relativeToText = 0;
            lineNo = 0;
        }

     

     

        IVertex query(IVertex baseVertex, string query)
        {
            return baseVertex.Get(query);
        }

        IVertex queryMetaMode(IVertex baseVertex, string query)
        {
            return baseVertex.Get(query); // TODO: to be corected
        }   

        IVertex processLink(string link)
        {
            // try named link

            string secondPart;

            string firstPart = ZeroCodeUtil.getQueryFirstAndSecondPart(link, out secondPart);

            IVertex tryIf;

            // named link

            IVertex importLink = importList.Get(firstPart + ":");

            if (importLink != null)
            {
                tryIf = query(importLink, secondPart);

                if (tryIf != null)
                    return tryIf;
            }                 
          
            IVertex importMetaLink = importMetaList.Get(firstPart + ":");

            if (importMetaLink != null)
            {
                tryIf = queryMetaMode(importMetaLink, secondPart);

                if (tryIf != null)
                    return tryIf;
            }


            // try direct link

            tryIf = query(importDirectList, "\\"+link);

            if (tryIf != null)
                return tryIf;

            tryIf = queryMetaMode(importDirectMetaList, "\\" + link);

            if (tryIf != null)
                return tryIf;          

            return MinusZero.Instance.Root.Get(link);
        }

        IVertex ProcessLine(IVertex _baseVertex)
        {
            if (TryIsKeyword(currentLineNoTabs))
            {
                AddKeywordVertex(_baseVertex, examinedKeywords);
            }
            else {

                string currentLineInner = currentLineNoTabs.Substring(ZeroCodeCommon.CodeGraphVertexPrefix.Length, 
                   currentLineNoTabs.Length - ZeroCodeCommon.CodeGraphVertexPrefix.Length - ZeroCodeCommon.CodeGraphVertexSuffix.Length);
                    
                int doubleColonPos = getDoubleColonPos(currentLineInner);

                if (doubleColonPos == -1) // no meta (before ::)
                {
                    string afterColon = currentLineInner.Trim();

                    if (afterColon[0] == ZeroCodeCommon.NewVertexPrefix) // if is new value
                        return _baseVertex.AddVertex(null, ZeroCodeCommon.stringFromNewVertexString(afterColon));

                    if (afterColon[0] == ZeroCodeCommon.CodeGraphLinkPrefix)
                        return _baseVertex.AddEdge(null, processLink(ZeroCodeCommon.stringFromLinkString(afterColon))).To;

                    return _baseVertex.AddVertex(null, "SYNTAX ERROR");
                }
                else
                {
                    string beforeColon = currentLineInner.Substring(0, doubleColonPos).Trim();

                    string afterColon = currentLineInner.Substring(doubleColonPos + 2, currentLineInner.Length - doubleColonPos-2).Trim();

                    IVertex meta = processLink(beforeColon);

                    if (afterColon[0] == ZeroCodeCommon.NewVertexPrefix) // if is new value
                        return _baseVertex.AddVertex(meta, ZeroCodeCommon.stringFromNewVertexString(afterColon));
                    else
                        return _baseVertex.AddEdge(meta, processLink(afterColon)).To;
                }
            }

            return null;
        }

        enum keywordTryingState { keywordCharacter, parameter, waiting}

        class keywordTryingData
        {
            public IVertex keywordVertex;
            public String keyword;

            public keywordTryingState state;
            public int currentPositionInKeyword;
            public int untilPositionWaiting;
            

            public string currentlyProcessedParameterName;
            public string afterParameterString;

            public bool matched;

            public Dictionary<string, object> sub = new Dictionary<string, object>();

            public keywordTryingData(keywordTryingData source)
            {
                keywordVertex = source.keywordVertex;
                keyword = source.keyword;
                currentPositionInKeyword = source.currentPositionInKeyword;
                state = source.state;
                currentlyProcessedParameterName = source.currentlyProcessedParameterName;
                afterParameterString = source.afterParameterString;
            }

            public keywordTryingData(IVertex k)
            {
                keywordVertex = k;
                keyword = (String)keywordVertex.Value;

                currentPositionInKeyword = 0;
                state = keywordTryingState.keywordCharacter; // that and rest of the fields will be updated in the _tryKeyword
            }
        }

        List<keywordTryingData> examinedKeywords_All;
        List<keywordTryingData> examinedKeywords;

        void copyExaminedKeywords(List<keywordTryingData> source, List<keywordTryingData> target)
        {
            foreach(keywordTryingData ktd in source)
            {
                keywordTryingData _ktd = new keywordTryingData(ktd);
                target.Add(_ktd);
            }              
        }

        bool TryIsKeyword(string s)
        {
            if (!ZeroCodeUtil.tryStringMatch(s, 0, ZeroCodeCommon.CodeGraphVertexPrefix) 
                && !ZeroCodeUtil.tryStringEndMatch(s, ZeroCodeCommon.CodeGraphVertexSuffix))
            {
                examinedKeywords = new List<keywordTryingData>();

                copyExaminedKeywords(examinedKeywords_All, examinedKeywords);

                _tryIsKeyword(firstCharacterPos_relativeToText, text.Length);

                if (examinedKeywords.Count() > 0)
                    return true;

                return false;
            }
            else
                return false;
        }

        void _tryIsKeyword(int startPos, int endPos)
        {
            int sPos = startPos;

            if (sPos == endPos)
                return;

            bool shallProceed = true;

            while (shallProceed)
            {
                List<keywordTryingData> newExaminedKeywords = new List<keywordTryingData>();

                foreach (keywordTryingData ktd in examinedKeywords)
                {
                    String keyword = (String)ktd.keywordVertex.Value;

                    if (ktd.state == keywordTryingState.waiting)
                    {                     
                        if (sPos == ktd.untilPositionWaiting)
                            ktd.state = keywordTryingState.keywordCharacter;
                        else
                            newExaminedKeywords.Add(ktd);
                    }

                    if (ZeroCodeUtil.tryStringMatch(keyword, ktd.currentPositionInKeyword, "(?<")
                        && ktd.state == keywordTryingState.keywordCharacter)
                    {
                        int begCurrentPositionInKeyword = ktd.currentPositionInKeyword;

                        ktd.currentPositionInKeyword = ZeroCodeUtil.getNextMatch(keyword, ktd.currentPositionInKeyword + 2, ">)") + 2;
                        ktd.currentlyProcessedParameterName = keyword.Substring(begCurrentPositionInKeyword + 3, ktd.currentPositionInKeyword - begCurrentPositionInKeyword - 5);
                        ktd.afterParameterString = ZeroCodeUtil.getNextCharacterPartFromKeyword(keyword, ktd.currentPositionInKeyword);
                        ktd.state = keywordTryingState.parameter;
                    }

                    if (ktd.state == keywordTryingState.keywordCharacter)
                    {
                        if (keyword.Length > ktd.currentPositionInKeyword && text[sPos] == keyword[ktd.currentPositionInKeyword])
                        {
                            if (ktd.keyword.Length == ktd.currentPositionInKeyword + 1)
                                ktd.matched = true;
                            else
                                ktd.currentPositionInKeyword++;

                            newExaminedKeywords.Add(ktd);
                        }
                    }                    
                }

                // store found keyword parameters

                Dictionary<string, object> foundParameters = new Dictionary<string, object>();

                // check if anything fits info keyword parameters
                foreach (keywordTryingData ktd in examinedKeywords)
                    if (ktd.state == keywordTryingState.parameter)
                    {
                        object foundParameter = null;

                        if (foundParameters.ContainsKey(ktd.afterParameterString))
                            foundParameter = foundParameters[ktd.afterParameterString];
                        else
                        {
                            int sPosAfterParameter = -1;

                            // if(ktd.afterParameterString=="")
                            //else
                            sPosAfterParameter=ZeroCodeUtil.getNextMatch(text, sPos, ktd.afterParameterString);

                            if (sPosAfterParameter != -1)
                            {
                                ktd.untilPositionWaiting = sPosAfterParameter;


                                if (ZeroCodeCommon.isNewVertex(text, sPos, sPosAfterParameter - 1))
                                    foundParameter = ZeroCodeCommon.stringFromNewVertexString(text.Substring(sPos, sPosAfterParameter - sPos));

                                if (ZeroCodeCommon.isLink(text, sPos, sPosAfterParameter - 1))
                                    foundParameter = new ToVertexMock(ZeroCodeCommon.stringFromLinkString(text.Substring(sPos, sPosAfterParameter - sPos)));

                            }
                        }

                        if (foundParameter != null)
                        {
                            foundParameters.Add(ktd.afterParameterString, foundParameter);

                            // adding as string
                            ktd.sub.Add(ktd.currentlyProcessedParameterName, foundParameter);

                            newExaminedKeywords.Add(ktd);
                        }

                        ktd.state = keywordTryingState.waiting;
                    }

                examinedKeywords = newExaminedKeywords;

                sPos++;

                if (text[sPos] == '\r' || text[sPos] == '\n')
                    foreach (keywordTryingData ktd in examinedKeywords)
                        if (ktd.matched)
                            shallProceed = false; // end of line and one of keywords matched

                if (sPos == endPos)
                    shallProceed = false; // end of this part of text

                if (examinedKeywords.Count == 0)
                    shallProceed = false; // no keyword found
            }
        }

        IVertex AddKeywordVertex(IVertex parent, List<keywordTryingData> keywords)
        {
            if (keywords.Count > 1)
            {
                int x = 0;
            }

            return _AddKeywordVertex(parent,keywords[0],keywords[0].keywordVertex);
        }

        IVertex _AddKeywordVertex(IVertex parent, keywordTryingData ktd, IVertex keywordAddingVertex)
        {
            IVertex nv=null;

            foreach (IEdge e in keywordAddingVertex) {
                if (ZeroCodeUtil.tryStringMatch((string)e.To.Value, 0, "(?<"))
                {
                    string name = ZeroCodeUtil.getRegexp((string)e.To.Value, Regex.Escape("(?<") + "(?<EXTRACT>.*)" + Regex.Escape(">)"));

                    object sub = ktd.sub[name];

                    if(sub is string)
                        nv = parent.AddVertex(e.Meta, ktd.sub[name]);

                    if (sub is ToVertexMock)
                        nv = parent.AddEdge(e.Meta, (IVertex)sub).To;
                }
                else
                    nv=parent.AddVertex(e.Meta, e.To);

                _AddKeywordVertex(nv, ktd, e.To);
            }

            return nv;
        }

        int getDoubleColonPos(string s)
        {
            return s.IndexOf("::");            
        }

        public IVertex Process(IVertex _baseVertex, string _text)
        {
            baseVertex = _baseVertex;
            text = _text+"\r\n"; // for regexpes


            initVariables();


            prepareImportList();

            ParseLine();


            Process_reccurent(baseVertex);

            return null;
        }

        bool skipParse = false;

        void Process_reccurent(IVertex _baseVertex)
        {
            IVertex prevVertex = ProcessLine(_baseVertex);

            while (ParseLine())
            {
                if (firstCharacterPos_relativeToCurrentLine > prevFirstCharacterPos_relativeToCurrentLine)
                {
                    int prevFirstCharacterPos_memory = prevFirstCharacterPos_relativeToCurrentLine;
                    //int currentLineFirstCharacterPos_memory = currentLineFirstCharacterPos;

                    Process_reccurent(prevVertex);

                    prevFirstCharacterPos_relativeToCurrentLine = prevFirstCharacterPos_memory;
                    // currentLineFirstCharacterPos = currentLineFirstCharacterPos_memory;

                    continue;
                }

                if (firstCharacterPos_relativeToCurrentLine == prevFirstCharacterPos_relativeToCurrentLine)
                    prevVertex = ProcessLine(_baseVertex);

                if (firstCharacterPos_relativeToCurrentLine < prevFirstCharacterPos_relativeToCurrentLine)
                {
                    skipParse = true;

                    return;
                }
            }
        }

        private void PrepareExamineKeywords()
        {
            examinedKeywords_All = new List<keywordTryingData>();

            foreach (IEdge keyword in MinusZero.Instance.Root.GetAll(@"User\CurrentUser:\CodeSettings:\Keyword:\$Keyword:"))
            {
                keywordTryingData ktd = new keywordTryingData(keyword.To);

                examinedKeywords_All.Add(ktd);
            }
                
        }

        public String2ZeroCodeGraphProcessing()
        {
            setupHelpVariables();

            PrepareExamineKeywords();
        }
    }
}


