using m0.Foundation;
using m0.Graph;
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
    public class String2ZeroCodeGraphProcessing
    {
        class ParsingStack
        {
            String2ZeroCodeGraphProcessing parent;

            public int begLine;
            public int endLine;

            public int lineNo;
            public LineInfo currentLineInfo;

            public string currentLineNoTabs;

            public IVertex LocalRoot;

            public IVertex lastAddedVertex;
            public IVertex lastAddedVertexParent;
            public int newLineCount;

            public bool skipParse = false;
            public int parseRecurrentReturnNo = -1;

            public ParsingStack(String2ZeroCodeGraphProcessing _parent, int _begLine, int _endLine)
            {
                parent = _parent;
                begLine = _begLine;
                endLine = _endLine;
               
                lineNo = begLine - 1;
            }

            public int getThisTabCount()
            {
                return currentLineInfo.tabCount;
            }

            public int getPrevTabCount()
            {
                return parent.lineInfoList[lineNo - 1].tabCount; // to be corrected
            }
        }

        //

        List<keywordTryingData> examinedKeywords_All; // all keywords are here
        List<keywordTryingData> examinedKeywords_LocalRootOnly; // all keywords are here
        Dictionary<char, List<string>> allKeywordsSubstringsDictionary_onlyFirstSubstring;
        Dictionary<char, List<string>> allKeywordsSubstringsDictionary;

        IVertex emptyKeywordVertex;

        // PROCESS dependent

        public IVertex baseVertex;

        string text;

        List<LineInfo> lineInfoList;

        //

        IVertex r = m0.MinusZero.Instance.Root;

        bool ParseLine(ParsingStack s)
        {
            if (s.skipParse)
            {
                s.skipParse = false;

                return true;
            }

            s.lineNo++;

            if (s.lineNo > s.endLine) 
                return false;          

            s.currentLineInfo = lineInfoList[s.lineNo];

            if (s.currentLineInfo.isEmpty)
                s.currentLineNoTabs = "";
            else
                s.currentLineNoTabs = text.Substring(s.currentLineInfo.lineBeg, s.currentLineInfo.lineEnd - s.currentLineInfo.lineBeg + 1);
            
            // and now check if there are only whitespaces

            if (ZeroCodeUtil.isStringOnlyWhiteSpaces(s.currentLineNoTabs))
            {
                s.newLineCount++;
                return ParseLine(s);
            }

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

            string firstPart;

            ZeroCodeUtil.getQueryFirstAndSecondPart(link, out firstPart, out secondPart);

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

            // try from local root

            tryIf = query(baseVertex, link);

            if (tryIf != null)
                return tryIf;

            // try from global root

            tryIf = MinusZero.Instance.Root.Get(link);

            if (tryIf != null)
                return tryIf;

            return MinusZero.Instance.Empty;
        }

        enum keywordTryingState { keywordCharacter, parameter, waiting, matched}

        class keywordTryingData
        {
            public String2ZeroCodeGraphProcessing processing;
            public IVertex keywordVertex;
            public String keyword;

            public keywordTryingState state;
            public int currentPositionInKeyword;
            
            public int waitingUntilPositionInText;
            public int matchedOnPositionInText;

            public string currentlyProcessedParameterName;
            public string afterParameterString;

            public int multiParameterCount = 0;

            public Dictionary<string, List<object>> parameters = new Dictionary<string, List<object>>();

            public keywordTryingData LocalRootNext;

            //

            string multiParameterSeparator;
            string multiParameterString;
            string multiParamPlusSeparatorString;

            int currentPositionInMultiParamPlusSeparatorString = -1;

            int multiParameterStringBegPosition = -1;
            int multiParameterStringEndPosition = -1;

            public keywordTryingData(keywordTryingData source)
            {
                processing = source.processing;
                keywordVertex = source.keywordVertex;
                keyword = source.keyword;
                currentPositionInKeyword = source.currentPositionInKeyword;
                state = source.state;
                currentlyProcessedParameterName = source.currentlyProcessedParameterName;
                afterParameterString = source.afterParameterString;
            }

            public keywordTryingData(IVertex k, String2ZeroCodeGraphProcessing _processing)
            {
                processing = _processing;
                keywordVertex = k;
                keyword = (String)keywordVertex.Value;

                currentPositionInKeyword = 0;
                state = keywordTryingState.keywordCharacter; // that and rest of the fields will be updated in the _tryKeyword
            }

            public int getMatchedOnPositionInText_Reccurent()
            {
                if (LocalRootNext == null)
                    return this.matchedOnPositionInText;

                return LocalRootNext.getMatchedOnPositionInText_Reccurent();
            }

            public bool currentPositionInKeyword_isParameterMatch(char v)
            {
                if(ZeroCodeUtil.tryStringMatch(keyword, currentPositionInKeyword, "(*(+")
                    && currentPositionInMultiParamPlusSeparatorString == -1)
                {
                    int multiParameterSeparatorEndPos = ZeroCodeUtil.getNextMatch(keyword, currentPositionInKeyword + 4, "+)");

                    multiParameterSeparator = keyword.Substring(currentPositionInKeyword + 4, multiParameterSeparatorEndPos - currentPositionInKeyword - 4);

                    multiParameterStringBegPosition = multiParameterSeparatorEndPos + 2;

                    multiParameterStringEndPosition = ZeroCodeUtil.getNextMatch(keyword, multiParameterSeparatorEndPos, "*)") + 1;

                    multiParameterString = keyword.Substring(multiParameterStringBegPosition, multiParameterStringEndPosition - multiParameterStringBegPosition - 1);

                    multiParamPlusSeparatorString = multiParameterString + multiParameterSeparator;

                    if (v == keyword[multiParameterStringEndPosition + 1])
                    {
                        currentPositionInKeyword = multiParameterStringEndPosition + 1;
                    }
                    else
                    {// if there ARE muli parameters at all!
                        currentPositionInMultiParamPlusSeparatorString = 0;
                        multiParameterCount = 1;
                    }
                }

                if (isInMultiParameter())
                {
                    if (ZeroCodeUtil.tryStringMatch(multiParamPlusSeparatorString, currentPositionInMultiParamPlusSeparatorString, "(?<"))
                        return true;
                    else
                        return false;
                }

                if (ZeroCodeUtil.tryStringMatch(keyword, currentPositionInKeyword, "(?<"))
                    return true;
                else
                    return false;
            }

            bool isInMultiParameter() // do not need that now, but maybe in the future?
            {
                return currentPositionInMultiParamPlusSeparatorString != -1;
                //return currentPositionInKeyword >= multiParameterStringBegPosition && currentPositionInKeyword <= multiParameterStringEndPosition;
            }

            public bool currentPositionCharacter_isCharacterMatch(char v)
            {
                MinusZero.Instance.Log(1, "isCharacterMatch",v+" ? "+ keyword[currentPositionInKeyword] + " | curPositionInKeyword:"+currentPositionInKeyword);
                
                if (isInMultiParameter())
                {
                    if (currentPositionInMultiParamPlusSeparatorString == multiParameterString.Length // after multi param string
                        && keyword[multiParameterStringEndPosition + 1] == v) // we are going out of multi
                    {
                        currentPositionInKeyword = multiParameterStringEndPosition + 1;
                        currentPositionInMultiParamPlusSeparatorString = -1; // out of multi
                        return true;
                    }

                    if (multiParamPlusSeparatorString[currentPositionInMultiParamPlusSeparatorString] == v)
                        return true;
                    else
                        return false;
                }

                if (keyword[currentPositionInKeyword] == v)
                    return true;
                else
                    return false;
            }

            public void GetParameter(int curPos)
            {
                int currentPosition;
                string str;

                if (isInMultiParameter())
                {
                    MinusZero.Instance.Log(1, "GetParameter", "MULTI: currentPositionInMultiParamPlusSeparatorString:"+ currentPositionInMultiParamPlusSeparatorString);
                    currentPosition = currentPositionInMultiParamPlusSeparatorString;
                    str = multiParamPlusSeparatorString;
                }
                else
                {
                    MinusZero.Instance.Log(1, "GetParameter", "NORMAL");
                    currentPosition = currentPositionInKeyword;
                    str = keyword;
                }

                int begCurrentPosition = currentPosition;

                currentPosition = ZeroCodeUtil.getNextMatch(str, currentPosition + 2, ">)") + 2;
                currentlyProcessedParameterName = str.Substring(begCurrentPosition + 3, currentPosition - begCurrentPosition - 5);

                if (isInMultiParameter() && currentPosition == multiParameterString.Length)
                {
                    int whatMatch;

                    string afterSeparatorString = ZeroCodeUtil.getNextCharacterPartFromKeyword_startingFromNonParameter(keyword, multiParameterStringEndPosition + 1);

                    int twoPos = ZeroCodeUtil.getNextMatch_twoAtOnce(processing.text, curPos, 
                        multiParameterSeparator,
                        afterSeparatorString, 
                        out whatMatch);

                    if(whatMatch==0)
                        afterParameterString = "";

                    if (whatMatch == 1)
                        afterParameterString = multiParameterSeparator;

                    if (whatMatch == 2)
                        afterParameterString = afterSeparatorString;
                }
                else
                    afterParameterString = ZeroCodeUtil.getNextCharacterPartFromKeyword_startingFromNonParameter(str, currentPosition);

                MinusZero.Instance.Log(1, "GetParameter", "currentlyProcessedParameterName:"+ currentlyProcessedParameterName+" curPosition:" + currentPosition+ " afterParameterString:"+ afterParameterString);

                state = keywordTryingState.parameter;

                if (isInMultiParameter())
                    currentPositionInMultiParamPlusSeparatorString = currentPosition;
                else
                    currentPositionInKeyword = currentPosition;
                   
            }

            public void currentPositionInKeyword_Increase()
            {
                if (isInMultiParameter())
                {
                    if (currentPositionInMultiParamPlusSeparatorString < multiParamPlusSeparatorString.Length - 1)
                        currentPositionInMultiParamPlusSeparatorString++;
                    else
                    {
                        currentPositionInMultiParamPlusSeparatorString = 0;
                        multiParameterCount++;
                    }

                        MinusZero.Instance.Log(1, "currentPositionInKeyword_Increase", "MULTI:" + currentPositionInMultiParamPlusSeparatorString);
                    }
                else
                {
                    currentPositionInKeyword++;
                    MinusZero.Instance.Log(1, "currentPositionInKeyword_Increase", "NORMAL:" + currentPositionInKeyword);
                }
            }

            public void AddParameter(string name, object val)
            {
                if (!parameters.ContainsKey(name))
                    parameters.Add(name, new List<object>());

                parameters[name].Add(val);
            }
        }

   ///////////////////////////////////////////////////
   ///////////////////////////////////////////////////
   ///////////////////////////////////////////////////

        void copyExaminedKeywords(List<keywordTryingData> source, List<keywordTryingData> target)
        {
            foreach(keywordTryingData ktd in source)
            {
                keywordTryingData _ktd = new keywordTryingData(ktd);
                target.Add(_ktd);
            }              
        }

        List<keywordTryingData> TryIfIsKeywordLine(ParsingStack s)
        {
            List<keywordTryingData> examinedKeywords;

            if (s.currentLineInfo.lineBeg >= text.Length)
                return null;

            if (!ZeroCodeUtil.tryStringMatch(s.currentLineNoTabs, 0, ZeroCodeCommon.CodeGraphVertexPrefix) 
                || !ZeroCodeUtil.tryStringEndMatch(s.currentLineNoTabs, ZeroCodeCommon.CodeGraphVertexSuffix))
            {
                string newVertex;
                string link;

                int tryPos = 0; 

                _tryIsKeyword(s, "", s.currentLineInfo.lineBeg, -1, 0, text.Length - 1, text.Length - 1, out examinedKeywords, out newVertex, out link, true, ref tryPos, false);

                if (examinedKeywords.Count() > 0)
                    return examinedKeywords;                

                return null;
            }
            else
                return null;
        }

        keywordTryingData createEmptyKeyword(ParsingStack s, string value, int matchedOnPositionInText)
        {
            if (value == null || value == "")
            {
                int x = 0; // WTF???
            }

            keywordTryingData ktd = new keywordTryingData(emptyKeywordVertex, this);

            ktd.matchedOnPositionInText = matchedOnPositionInText;

            List<object> l = new List<object>();
            l.Add(value);

            ktd.parameters.Add("EmptyKeyword", l);

            return ktd;
        }

        void _tryIsKeyword(ParsingStack s, string LOGPREFIX, int startPos, int prev_startPos, int isPrevStartPosSameAsStartPosParentCount, int endPos, int endPos_forAtomParts, out List<keywordTryingData> examinedKeywords, out string newVertex, out string link, bool isTopLevelCall, ref int newPos, bool lookForLocalRootOnly)
        {
            examinedKeywords = new List<keywordTryingData>();

            newVertex = null;

            link = null;

            //

            if (text[startPos] == '\r' || text[startPos] == '\n')
                return;

            if (startPos == endPos_forAtomParts)
                return;

            //

            bool isPrevStartPosSameAsStartPos = false;

            int isPrevStartPosSameAsStartPosThisCount = isPrevStartPosSameAsStartPosParentCount;

            if (startPos == prev_startPos)
            {
                isPrevStartPosSameAsStartPos = true;
                isPrevStartPosSameAsStartPosThisCount++;
            }

            string xx = "";

            for (int x = startPos; x <= endPos; x++)
                xx += " "+x+":"+text[x];

            MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"BEG startPos:" + startPos + " prevSpos:"+prev_startPos+" same:"+isPrevStartPosSameAsStartPos+" endPos:" + endPos+" "+xx);


            int sPos = startPos;

            int try_sPos = 0;

            if (sPos == endPos)
                return;

            bool shallProceed = true;

            int tryNewPos = 0;
            string tryNewVertex = null;
            string tryLink = null;
            string tryEmptyKeyword = null;

            //

            if (!testIfIsKeywordSubstring(startPos))
            {
                while (shallProceed)
                {
                    sPos++;

                    if (testIfIsKeywordSubstring(sPos))
                        shallProceed = false;

                    if (text[sPos] == '\r' || text[sPos] == '\n')
                        shallProceed = false;

                    if (sPos == endPos_forAtomParts)
                        shallProceed = false;
                }

                string foundString = text.Substring(startPos, sPos - startPos);

                if (!isTopLevelCall
                    && ZeroCodeCommon.isNewVertexString(foundString))
                    tryNewVertex = ZeroCodeCommon.stringFromNewVertexString(foundString);
                else if (!isTopLevelCall
                    && ZeroCodeCommon.isLinkString(foundString))
                    tryLink = ZeroCodeCommon.stringFromLinkString(foundString, false);
                else
                {
                    tryEmptyKeyword = foundString;

                    sPos++; // hmmm ????

                    try_sPos = sPos;
                }


                if ( //(tryEmptyKeyword != null || lookForLocalRootOnly==false) &&
                     ( sPos == endPos_forAtomParts || isPrevStartPosSameAsStartPos) )
                {
                    newVertex = tryNewVertex;

                    link = tryLink;

                    if (tryEmptyKeyword != null)
                    {
                        keywordTryingData ktd = createEmptyKeyword(s, tryEmptyKeyword, sPos - 1);

                        //

                        newPos = _tryIsNextLocalRootKeyword(s, LOGPREFIX, newPos, sPos, ktd);

                        //

                        examinedKeywords.Add(ktd);
                    }

                    newPos = sPos;

                    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "RETURN:" + tryEmptyKeyword + " newPos:" + newPos);

                    return;
                }
                else
                {
                    tryNewPos = sPos;

                    sPos = startPos;
                }
                
            }
            
            // no infinite reccursion

            if (isPrevStartPosSameAsStartPos && isPrevStartPosSameAsStartPosParentCount == 1) // do not want inifinite recursion
                return;

            // keyword

            if(lookForLocalRootOnly)
                copyExaminedKeywords(examinedKeywords_LocalRootOnly, examinedKeywords);
            else
                copyExaminedKeywords(examinedKeywords_All, examinedKeywords);

            shallProceed = true;

            while (shallProceed)
            {
                List<keywordTryingData> newExaminedKeywords = new List<keywordTryingData>();

                foreach (keywordTryingData ktd in examinedKeywords)
                {
                    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"*** "+ktd.keyword+" sPos:" + sPos + " waitingUntilPositionInText:" + ktd.waitingUntilPositionInText + " currentPositionInKeyword:" + ktd.currentPositionInKeyword + " state:"+ktd.state);

                    String keyword = (String)ktd.keywordVertex.Value;
                    
                    if (ktd.state == keywordTryingState.matched)
                    {// matched => matched
                        //if (sPos <= ktd.waitingUntilPositionInText)
                        {
                            newExaminedKeywords.Add(ktd);
                            MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"matched => matched");
                        }
                    }
                    else
                    {
                        // (any) => matched
                        if(ktd.keyword.Length == ktd.currentPositionInKeyword)
                        {
                            bool canDo = true;

                            if (ktd.state == keywordTryingState.waiting && sPos < ktd.waitingUntilPositionInText)
                                canDo = false;

                            if (canDo)
                            {
                                ktd.state = keywordTryingState.matched;
                                ktd.matchedOnPositionInText = sPos;

                                newExaminedKeywords.Add(ktd);
                                MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "(any) => matched");
                            }
                        }

                        // waiting => keywordCharacter
                        // waiting => waiting
                        if (ktd.state == keywordTryingState.waiting)
                        {
                            if (sPos == ktd.waitingUntilPositionInText)
                            {
                                MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"waiting => keywordCharacter sPos:" + sPos + " fiished waiting");
                                ktd.state = keywordTryingState.keywordCharacter; // waiting => keywordCharacter
                            }
                            else
                            {
                                MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"waiting => waiting sPos:" + sPos + " waiting "+text[sPos]);
                                newExaminedKeywords.Add(ktd);  // waiting => waiting
                            }
                        }

                        // keywordCharacter => parameter
                        // keywordCharacter => keywordCharacer
                        if(ktd.state== keywordTryingState.keywordCharacter)
                        {
                            // keywordCharacter => parameter
                            if(ktd.currentPositionInKeyword_isParameterMatch(text[sPos]))
                            {
                                ktd.GetParameter(sPos);
                            }
                            else
                                // keywordCharacter => keywordCharacer
                                if ( ktd.currentPositionCharacter_isCharacterMatch(text[sPos]) )
                                {
                                    ktd.currentPositionInKeyword_Increase();

                                    newExaminedKeywords.Add(ktd);
                                MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"sPos:" + sPos + " keywordCharacter -> keywordCharacter");
                                }
                                else
                                {
                                    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"sPos:" + sPos + " out of. not keywordCharacter match");
                                }
                        }                    
                    }
                }

                // store found keyword parameters

                Dictionary<string, object> foundParameters = new Dictionary<string, object>();

                Dictionary<string, int> foundParameters_waitingUntilPositionInText = new Dictionary<string, int>();

                // check if anything fits info keyword parameters
                foreach (keywordTryingData ktd in examinedKeywords)
                    if (ktd.state == keywordTryingState.parameter)
                    {
                        object foundParameter = null;

                        int _waitingUntilPositionInText=0;

                        bool allreadyAdded = false;

                        if (foundParameters.ContainsKey(ktd.afterParameterString))
                        {
                            allreadyAdded = true;
                            foundParameter = foundParameters[ktd.afterParameterString];
                            _waitingUntilPositionInText = foundParameters_waitingUntilPositionInText[ktd.afterParameterString];
                        }
                        else // THIS MIGHT NOT WORK GOOD NOW. TO BE CHECKED / CORRECTED
                        {
                            int isTryKeyword_endPos = endPos_forAtomParts;

                            MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"TRY for parameter:" +ktd.currentlyProcessedParameterName+" for keyword:" + ktd.keyword + " afterParameterString:" + ktd.afterParameterString + "| ("+sPos+","+endPos+")");

                            if (ktd.afterParameterString != "")
                            {
                                int sPosAfterParameter = ZeroCodeUtil.getNextMatch(text, sPos, ktd.afterParameterString);

                                MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"sPosAfterParameter:" + sPosAfterParameter+ " for afterParameterString:"+ktd.afterParameterString);

                                if (sPosAfterParameter != -1
                                    && ((sPosAfterParameter < endPos) || (endPos == 0)))
                                {
                                    isTryKeyword_endPos = sPosAfterParameter; 
                                }
                                else
                                    isTryKeyword_endPos = -1; // do not search; this keyword does not fit in text
                            }

                            MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"isTryKeyword_endPos:" + isTryKeyword_endPos);

                            if (isTryKeyword_endPos != -1)
                            {
                                List<keywordTryingData> foundKeywords = null;
                                string foundNewVertex = null;
                                string foundLink = null;
                                int _newPos = 0;

                                MinusZero.Instance.Log(1, LOGPREFIX+"_tryIsKeyword", "will run _tryIs for:"+ ktd.currentlyProcessedParameterName);

                                _tryIsKeyword(s, LOGPREFIX+"    ",sPos, startPos, isPrevStartPosSameAsStartPosThisCount, endPos, isTryKeyword_endPos, out foundKeywords, out foundNewVertex, out foundLink, false, ref _newPos, false);

                                if (foundNewVertex != null)
                                {
                                    ktd.waitingUntilPositionInText = _newPos;
                                    foundParameter = foundNewVertex;

                                    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "FOUND PARAMETER VERTEX:" + foundParameter.ToString() + " waitUntil:" + ktd.waitingUntilPositionInText);
                                }

                                if (foundLink != null)
                                {
                                    ktd.waitingUntilPositionInText = _newPos;
                                    foundParameter = new ToVertexMock(foundLink);
     
                                    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "FOUND PARAMETER LINK:" + foundParameter.ToString() + " waitUntil:" + ktd.waitingUntilPositionInText);
                                }

                                if (foundKeywords.Count() > 0)
                                {
                                    ktd.waitingUntilPositionInText = _newPos - 1;

                                    foundParameter = foundKeywords[0];

                                    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "FOUND PARAMETER KEYWORDS:" + foundParameter.ToString() + " waitUntil:" + ktd.waitingUntilPositionInText);

                                    if (foundKeywords.Count() > 1)
                                    {
                                        int x = 0; // HOW IS THAT
                                    }
                                }
                            }
                        }

                        if (foundParameter != null)
                        {
                            if (!allreadyAdded)
                            {
                                foundParameters.Add(ktd.afterParameterString, foundParameter);
                                foundParameters_waitingUntilPositionInText.Add(ktd.afterParameterString, ktd.waitingUntilPositionInText);
                            }
                            else
                                ktd.waitingUntilPositionInText = _waitingUntilPositionInText; // TURNED OFF NOW

                            ktd.AddParameter(ktd.currentlyProcessedParameterName, foundParameter);

                            MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"sub add:" + ktd.currentlyProcessedParameterName+" foundParameter:"+foundParameter);

                            newExaminedKeywords.Add(ktd);
                        }
                        
                        ktd.state = keywordTryingState.waiting;
                    }

                examinedKeywords = newExaminedKeywords;

                // check how many matched

                int matchedKeywords = 0;

                foreach (keywordTryingData ktd in examinedKeywords)
                {
                   // MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "check ktd.state:"+ktd.state);
                    if (ktd.state == keywordTryingState.matched)
                        matchedKeywords++;
                }

               // MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "matchedKeywords:"+matchedKeywords);

                if (matchedKeywords == examinedKeywords.Count && matchedKeywords >= 1)
                {
                    shallProceed = false;
                    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "SHALPROCEED FALSE only matched");
                }

                if (sPos == endPos)
                {
                    shallProceed = false; // end of this part of text
                    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"SHALPROCEED FALSE end of this part of text");
                }
                else
                {
                    if (text[sPos] == '\r' || text[sPos] == '\n')
                        foreach (keywordTryingData ktd in examinedKeywords)
                            if (ktd.state == keywordTryingState.matched)
                            {
                                shallProceed = false; // end of line and one of keywords matched
                                MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"SHALPROCEED FALSE end of line and one of keywords matched");
                            }

                    if (examinedKeywords.Count == 0)
                    {
                        shallProceed = false; // no keyword found
                        MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"SHALPROCEED FALSE  no keyword found");
                    }
                }
                
                // ++

                sPos++;
                MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "sPos++ " + sPos);


                if (shallProceed==false)
                {
                    int x = 0;
                }
            }

            // copy only matched and of maxMatchedOnPositionText

            if (examinedKeywords.Count > 0)
            {
                int maxMatchedOnPositionInText = examinedKeywords.Max(m => m.matchedOnPositionInText);

                examinedKeywords = examinedKeywords.Where(m => m.state == keywordTryingState.matched
                    && m.matchedOnPositionInText == maxMatchedOnPositionInText).ToList();

                /////////////////////////////////////////////////////
                //                                                 //
                //    >   >  > >> A S S U M P T I O N <<<  <   <   //
                //                                                 //
                /////////////////////////////////////////////////////

                // WE ASSUME THAT ONLY examinedKeywords[0] will be used forther

                //

                keywordTryingData ktd = examinedKeywords[0]; // ASSUMPTION

                if(  ktd.matchedOnPositionInText <= s.currentLineInfo.lineEnd 
                    && isLocalRootKeyword(ktd))
                    newPos = _tryIsNextLocalRootKeyword(s, LOGPREFIX, newPos, ktd.matchedOnPositionInText + 1, ktd);

                //

            }

            // if no keywords found, we can use tryNewVertex/tryLink, that are:
            // - filling needed space
            // - endPos==0 => needed space not defined 

            if (examinedKeywords.Count == 0)
            {
                newVertex = tryNewVertex;
                link = tryLink;

                if (tryEmptyKeyword != null)
                {
                    keywordTryingData ktd = createEmptyKeyword(s, tryEmptyKeyword, tryNewPos - 1);

                    //

                    newPos = _tryIsNextLocalRootKeyword(s, LOGPREFIX, newPos, try_sPos, ktd);

                    //

                    examinedKeywords.Add(ktd);
                }

                newPos = tryNewPos;
            }else
                newPos = sPos;

            MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"END newVertex:" +newVertex+" link:"+link+" keywordsCount:"+examinedKeywords.Count);

            log_keywords(examinedKeywords, 0, LOGPREFIX);
        }

        bool isLocalRootKeyword(keywordTryingData ktd)
        {
            if (ktd.keywordVertex.Get(@"\$StartInLocalRoot:") != null)
                return true;
            else
                return false;
        }

        private int _tryIsNextLocalRootKeyword(ParsingStack s, string LOGPREFIX, int newPos, int sPos, keywordTryingData ktd)
        {
            List<keywordTryingData> _examinedKeywords = new List<keywordTryingData>();

            string _newVertex;
            string _link;

            int _tryPos = 0;

            _tryIsKeyword(s, LOGPREFIX + "    ", sPos - 1, -1, 0, text.Length - 1, text.Length - 1, out _examinedKeywords, out _newVertex, out _link, true, ref _tryPos, true);

            if (_examinedKeywords.Count > 0)
            {
                ktd.LocalRootNext = _examinedKeywords[0];

                newPos = _tryPos;
            }

            return newPos;
        }

        private bool testIfIsKeyword_noStartingWithParameter(int startPos)
        {
            char charAtPos = text[startPos];

            if (!allKeywordsSubstringsDictionary.ContainsKey(charAtPos))
                return false;

            List<string> l = allKeywordsSubstringsDictionary[charAtPos];

            foreach (string s in l)
                if (ZeroCodeUtil.tryStringMatch(text, startPos, s))
                    return true;

            return false;
        }

        private bool testIfIsKeywordSubstring(int startPos)
        {
            char charAtPos = text[startPos];

            if (!allKeywordsSubstringsDictionary.ContainsKey(charAtPos))
                return false;

            List<string> l = allKeywordsSubstringsDictionary[charAtPos];

            foreach (string s in l)
                if (ZeroCodeUtil.tryStringMatch(text, startPos, s))
                    return true;

            return false;
        }

        void log_keywords(List<keywordTryingData> examinedKeywords, int pos, string LOGPREFIX)
        {
            string pre = "";

            for (int x = 0; x < pos; x++)
                pre += " ";

            foreach (keywordTryingData ktd in examinedKeywords)
            {
                MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+pre + "-keyword:" + ktd.keyword + " sub count:" + ktd.parameters.Count);

                foreach (KeyValuePair<string, List<object>> o in ktd.parameters)
                {
                    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+pre + "-sub:" + o);

                    foreach(object oo in o.Value)
                    if (oo is keywordTryingData)
                    {
                        List<keywordTryingData> l = new List<keywordTryingData>();
                        l.Add((keywordTryingData)oo);
                        log_keywords(l, pos + 1, LOGPREFIX);
                    }
                }
            }
        }

        

        void DoesContainLocalRoot(ParsingStack s, IVertex keyword)
        {
            /*
            if (keyword.Get(@"\$LocalRoot:") == null)
            //if (keyword.Get(@"\$StartInLocalRoot:") == null)
                s.LocalRoot = null;
                */
        }

        IVertex AddKeywordVertex(ParsingStack s, IVertex parent, keywordTryingData ktd)
        {
            DoesContainLocalRoot(s, ktd.keywordVertex);

            return _AddKeywordVertex(s, parent,ktd,ktd.keywordVertex,null,0);
        }

        void AddKeywordVertex_AddVertex(ParsingStack s, IVertex baseVertex, IEdge edgeForMeta, IVertex meta, object val, ref IVertex nv, keywordTryingData ktd)
        {
            if (GeneralUtil.CompareStrings("$LocalRoot", edgeForMeta.Meta.Value)
            || GeneralUtil.CompareStrings("$StartInLocalRoot", edgeForMeta.Meta.Value))
                return;

          //  if (edgeForMeta.To.Get("$StartInLocalRoot:") != null && s.LocalRoot != null)
          //      nv = AddVertex(s, s.LocalRoot, meta, val);
         //   else
                nv = AddVertex(s, baseVertex, meta, val);

            if(ktd.LocalRootNext!=null)
            {
                int x = 0;
            }

            if (edgeForMeta.To.Get("$LocalRoot:") != null && ktd.LocalRootNext != null)
                _AddKeywordVertex(s, nv,ktd.LocalRootNext,ktd.LocalRootNext.keywordVertex,null,0);

          //  if (edgeForMeta.To.Get("$LocalRoot:") != null)
          //     s.LocalRoot = nv;
        }

        IEdge AddKeywordVertex_AddEdge(ParsingStack s, IVertex baseVertex, IEdge edgeForMeta, IVertex meta, IVertex to)
        {
           // if(edgeForMeta.To.Get("$StartInLocalRoot:") != null && s.LocalRoot !=null)
            //    return AddEdge(s, s.LocalRoot, meta, to);

            return AddEdge(s, baseVertex, meta, to);
        }

        IVertex _AddKeywordVertex(ParsingStack s, IVertex parent, keywordTryingData ktd, IVertex keywordAddingVertex, IVertex useMetaWhenANY, int subCount)
        {
            IVertex nv=null;

            int min_subCount = 0;
            int max_subCount = 0;

            if (subCount != 0)
            {
                min_subCount = subCount;
                max_subCount = subCount;
            }

            foreach (IEdge e in keywordAddingVertex) {
                if (e.To.Get(@"$KeywordManyRoot:") != null)
                {
                    min_subCount = 0;
                    max_subCount = ktd.multiParameterCount - 1;
                }

                for (int cnt_subCount = min_subCount; cnt_subCount <= max_subCount; cnt_subCount++)
                {
                    if (GeneralUtil.CompareStrings(e.Meta, "$KeywordManyRoot"))
                        continue;

                    IVertex meta = e.Meta;

                    if ((string)e.Meta.Value == "(?<ANY>)" && useMetaWhenANY != null)
                        meta = useMetaWhenANY;

                    if (VertexOperations.IsLink(e))
                    {
                        if (ZeroCodeUtil.tryStringMatch((string)e.To.Value, 0, "(?<"))
                        {
                            string name = ZeroCodeUtil.getRegexp((string)e.To.Value, Regex.Escape("(?<") + "(?<EXTRACT>.*)" + Regex.Escape(">)"));

                            List<object> subs = ktd.parameters[name];

                            object sub = subs[cnt_subCount];

                            if (sub is string)
                                AddKeywordVertex_AddVertex(s, parent, e, meta, sub, ref nv, ktd); // was marked: ERROR. why?? 

                            if (sub is ToVertexMock)
                                nv = AddKeywordVertex_AddEdge(s, parent, e, meta, (IVertex)sub).To;

                            if (sub is keywordTryingData)
                            {
                                DoesContainLocalRoot(s, ((keywordTryingData)sub).keywordVertex);
                                nv = _AddKeywordVertex(s, parent, (keywordTryingData)sub, ((keywordTryingData)sub).keywordVertex, meta, 0); // cnt_subCount);
                            }
                        }
                        else
                            nv = AddKeywordVertex_AddEdge(s, parent, e, meta, e.To).To;
                        }
                    else
                    {
                        if (ZeroCodeUtil.tryStringMatch((string)e.To.Value, 0, "(?<"))
                        {
                            string name = ZeroCodeUtil.getRegexp((string)e.To.Value, Regex.Escape("(?<") + "(?<EXTRACT>.*)" + Regex.Escape(">)"));

                            List<object> subs = ktd.parameters[name];

                            object sub = subs[cnt_subCount];

                            if (sub is string)
                                AddKeywordVertex_AddVertex(s, parent, e, meta, sub, ref nv, ktd);

                            if (sub is ToVertexMock)
                                nv = AddKeywordVertex_AddEdge(s, parent, e, meta, (IVertex)sub).To;

                            if (sub is keywordTryingData)
                            {
                                DoesContainLocalRoot(s, ((keywordTryingData)sub).keywordVertex);

                                nv = _AddKeywordVertex(s, parent, (keywordTryingData)sub, ((keywordTryingData)sub).keywordVertex, meta, 0);// cnt_subCount);
                            }
                        }
                        else
                            AddKeywordVertex_AddVertex(s, parent, e, meta, e.To, ref nv, ktd);

                       _AddKeywordVertex(s, nv, ktd, e.To, null, cnt_subCount);
                    }
                }
            }

            return nv;
        }

        int getDoubleColonPos(string s)
        {
            return s.IndexOf("::");            
        }

        private void PrepareDictionaries()
        {
            examinedKeywords_All = new List<keywordTryingData>();

            examinedKeywords_LocalRootOnly = new List<keywordTryingData>();

            allKeywordsSubstringsDictionary_onlyFirstSubstring = new Dictionary<char, List<string>>();

            allKeywordsSubstringsDictionary = new Dictionary<char, List<string>>();

            foreach (IEdge keyword in MinusZero.Instance.Root.GetAll(@"User\CurrentUser:\CodeSettings:\Keyword:\$Keyword:"))
            {
                if (GeneralUtil.CompareStrings("(?<EmptyKeyword>)", keyword.To.Value))
                    continue;

                // examinedKeywords_All

                keywordTryingData ktd = new keywordTryingData(keyword.To, this);

                examinedKeywords_All.Add(ktd);

                // examinedKeywords_LocalRootOnly

                if (keyword.To.Get(@"\$StartInLocalRoot:") != null)
                {
                    keywordTryingData ktd2 = new keywordTryingData(keyword.To, this);

                    examinedKeywords_LocalRootOnly.Add(ktd2);
                }

                string keywordString = keyword.To.Value.ToString();

                char firstCharacter;

                if (keywordString.Length > 0)
                {
                    // allKeywordsDictionary_onlyFirstSubstring

                    if (!beginsWithParameter(keywordString))
                        addSubString(allKeywordsSubstringsDictionary_onlyFirstSubstring, 
                            ZeroCodeUtil.getNextCharacterPartFromKeyword_startingFromNonParameter(keywordString, 0));

                    // allKeywordsDictionary

                    addNonParameterKeywordSubstrings(keywordString);
                }
            }
                
        }
        private bool beginsWithParameter(string keywordString)
        {
            if (ZeroCodeUtil.tryStringMatch(keywordString, 0, "(?<"))
                return true;

            if (ZeroCodeUtil.tryStringMatch(keywordString, 0, "(*")) // needs some clever tests ideas, if this is valid????
                return true;

            return false;
        }

        private void addNonParameterKeywordSubstrings(string keywordString)
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

                    addSubString(allKeywordsSubstringsDictionary, keywordString.Substring(prevPos, keywordPos - prevPos));
                }

                if (isInsideParameter && ZeroCodeUtil.tryStringMatch(keywordString, keywordPos, ">)"))
                {
                    isInsideParameter = false;
                    prevPos = keywordPos + 2;
                }

                if (ZeroCodeUtil.tryStringMatch(keywordString, keywordPos, "(*"))
                {
                    addSubString(allKeywordsSubstringsDictionary, keywordString.Substring(prevPos, keywordPos - prevPos));

                    prevPos = keywordPos + 2;
                }

                if (ZeroCodeUtil.tryStringMatch(keywordString, keywordPos, "*)"))
                {
                    addSubString(allKeywordsSubstringsDictionary, keywordString.Substring(prevPos, keywordPos - prevPos));

                    prevPos = keywordPos + 2;
                }

                if (ZeroCodeUtil.tryStringMatch(keywordString, keywordPos, "(+"))
                {
                    addSubString(allKeywordsSubstringsDictionary, keywordString.Substring(prevPos, keywordPos - prevPos));

                    prevPos = keywordPos + 2;
                }

                if (ZeroCodeUtil.tryStringMatch(keywordString, keywordPos, "+)"))
                {
                    addSubString(allKeywordsSubstringsDictionary, keywordString.Substring(prevPos, keywordPos - prevPos));

                    prevPos = keywordPos + 2;
                }
            }

            addSubString(allKeywordsSubstringsDictionary, keywordString.Substring(prevPos, keywordPos - prevPos));
        }

        private void addSubString(Dictionary<char, List<string>> dict, string subString)
        {
            if (subString.Length == 0)
                return;

            char firstCharacter = subString[0];

            if (dict.ContainsKey(firstCharacter))
            {
                if (!dict[firstCharacter].Contains(subString))
                    dict[firstCharacter].Add(subString);
            }
            else
            {
                List<string> kl = new List<string>();

                dict.Add(firstCharacter, kl);

                kl.Add(subString);
            }

        }

        ///

        class LineInfo
        {
            public int tabCount;
            public bool lineContinuation;
            public int lineBeg;
            public int lineEnd;

            public int lineEnd_NoTrim;
            public bool isEmpty;
        }

        ///

        public void prepareLineInfoList()
        {
            lineInfoList = new List<LineInfo>();

            int p = 0;

            while (true)
            {
                LineInfo li = new LineInfo();

                int lineEndWithoutTrim, next;

                if (ZeroCodeUtil.isCRLF(text[p]))
                {
                    li.lineBeg = p;
                    li.lineEnd = p;
                    li.lineEnd_NoTrim = p;
                    li.tabCount = 0;
                    li.lineContinuation = false;
                    li.isEmpty = true;

                    next = p;
                }
                else
                {
                    lineEndWithoutTrim = ZeroCodeUtil.getNextCRLF(text, p) - 1;

                    if (lineEndWithoutTrim == -1)
                        return;

                    li.tabCount = 0;

                    while (text[p] == '\t')
                    {
                        li.tabCount++;
                        p++;
                    }

                    int lineBegWithoutTrim = p;

                    li.lineBeg = ZeroCodeUtil.trimRight(text, lineBegWithoutTrim);
                    li.lineEnd = ZeroCodeUtil.trimLeft(text, lineEndWithoutTrim);

                    li.lineEnd_NoTrim = lineEndWithoutTrim;

                    if (text[li.lineBeg] == ZeroCodeCommon.LineContinuationPrefix)
                        li.lineContinuation = true;

                    if (li.lineEnd < li.lineBeg)
                    {
                        li.isEmpty = true;
                        next = lineEndWithoutTrim + 1;
                    }
                    else
                        next = lineEndWithoutTrim + 1;
                }

                lineInfoList.Add(li);

                next++;

                if (next + 1 >= text.Length)
                    return;

                if (ZeroCodeUtil.isCRLF(text[next]))
                    next++;

                if (next >= text.Length)
                    return;

                p = next;
            }
        }

        void AddNewLines(ParsingStack s)
        {
            if (s.newLineCount != 0)
            {
                if (s.lastAddedVertex != null)
                    s.lastAddedVertex.AddVertex(smb.Get("$NewLine"), s.newLineCount);
                else
                    s.lastAddedVertexParent.AddVertex(smb.Get("$NewLine"), s.newLineCount);
            }

            s.newLineCount = 0;
        }

        IVertex AddVertex(ParsingStack s, IVertex baseVertex, IVertex meta, object val)
        {
            s.lastAddedVertexParent = baseVertex;
            s.lastAddedVertex = baseVertex.AddVertex(meta, val);
            return s.lastAddedVertex;
        }

        IEdge AddEdge(ParsingStack s, IVertex baseVertex, IVertex meta, IVertex to)
        {
            s.lastAddedVertexParent = baseVertex;
            s.lastAddedVertex = null;
            return baseVertex.AddEdge(meta, to);
        }

        IVertex ProcessLine(ParsingStack s, IVertex _baseVertex)
        {
            AddNewLines(s);

            bool shallProcess = true;
            IVertex toReturnVertex = null;
            s.LocalRoot = null;

            while (shallProcess)
            {
                List<keywordTryingData> examinedKeywords = TryIfIsKeywordLine(s);

                if (examinedKeywords != null)
                {
                    if (examinedKeywords.Count > 1)
                    {
                        int x = 0; // HOW IS THAT
                    }

                    keywordTryingData chosenKeyword = examinedKeywords[0];

                    int posAfterMatch = chosenKeyword.getMatchedOnPositionInText_Reccurent();

                    if (toReturnVertex == null)
                        toReturnVertex = AddKeywordVertex(s, _baseVertex, chosenKeyword);
                    else
                        AddKeywordVertex(s, _baseVertex, chosenKeyword);

                    if (posAfterMatch >= text.Length || text[posAfterMatch] == '\r')
                    {
                        return toReturnVertex;
                    }

                    s.currentLineInfo.lineBeg = posAfterMatch;
                }
                else
                {
                    if (s.currentLineNoTabs.Length == 0)
                        return null;

                    if (s.currentLineNoTabs[0] != ZeroCodeCommon.CodeGraphVertexPrefix[0]
                        || s.currentLineNoTabs[s.currentLineNoTabs.Length - 1] != ZeroCodeCommon.CodeGraphVertexSuffix[0])
                        return AddVertex(s, _baseVertex, null, "SYNTAX ERROR");

                    shallProcess = false;

                    string currentLineInner = s.currentLineNoTabs.Substring(ZeroCodeCommon.CodeGraphVertexPrefix.Length,
                        s.currentLineNoTabs.Length - ZeroCodeCommon.CodeGraphVertexPrefix.Length - ZeroCodeCommon.CodeGraphVertexSuffix.Length);

                    int doubleColonPos = getDoubleColonPos(currentLineInner);

                    if (doubleColonPos == -1) // no meta (before ::)
                    {
                        string afterColon = currentLineInner.Trim();

                        if (afterColon[0] == ZeroCodeCommon.NewVertexPrefix) // if is new value
                            return AddVertex(s, _baseVertex, null, ZeroCodeCommon.stringFromNewVertexString(afterColon));

                        //if (afterColon[0] == ZeroCodeCommon.CodeGraphLinkPrefix)
                        return AddEdge(s, _baseVertex, null, processLink(ZeroCodeCommon.stringFromLinkString(afterColon, true))).To;

                        //return AddVertex(_baseVertex, null, "SYNTAX ERROR");
                    }
                    else
                    {
                        string beforeColon = currentLineInner.Substring(0, doubleColonPos).Trim();

                        string afterColon = currentLineInner.Substring(doubleColonPos + 2, currentLineInner.Length - doubleColonPos - 2).Trim();

                        IVertex meta = processLink(beforeColon);

                        if (afterColon[0] == ZeroCodeCommon.NewVertexPrefix) // if is new value
                            return AddVertex(s, _baseVertex, meta, ZeroCodeCommon.stringFromNewVertexString(afterColon));
                        //else
                        //  return AddEdge(_baseVertex, meta, processLink(afterColon)).To;

                        //if (afterColon[0] == ZeroCodeCommon.CodeGraphLinkPrefix)
                        return AddEdge(s, _baseVertex, meta, processLink(ZeroCodeCommon.stringFromLinkString(afterColon, true))).To;

                        //return AddVertex(_baseVertex, null, "SYNTAX ERROR");
                    }
                }
            }

            return null;
        }

        void Process_reccurent(ParsingStack s, IVertex _baseVertex)
        {
            IVertex prevVertex = ProcessLine(s, _baseVertex);

            while (ParseLine(s))
            {
                if (s.parseRecurrentReturnNo > 0)
                {
                    s.parseRecurrentReturnNo--;
                    s.skipParse = true;
                    return;
                }

                int thisTabCount = s.getThisTabCount();
                int prevTabCount = s.getPrevTabCount();

                if (s.parseRecurrentReturnNo == 0)
                {
                    s.parseRecurrentReturnNo = -1;
                    prevTabCount = thisTabCount - 1; // need to simulate
                }

                if (thisTabCount > prevTabCount)
                {
                    Process_reccurent(s, prevVertex);

                    continue;
                }

                if (thisTabCount == prevTabCount)
                    prevVertex = ProcessLine(s, _baseVertex);

                if (thisTabCount < prevTabCount)
                {
                    s.skipParse = true;

                    s.parseRecurrentReturnNo = prevTabCount - thisTabCount;

                    return;
                }
            }
        }

        private IVertex ProcessTextPart(IVertex baseVertex, int begLine, int endLine)
        {
            ParsingStack stack = new ParsingStack(this, begLine, endLine);

            ParseLine(stack);

            Process_reccurent(stack, baseVertex);

            AddNewLines(stack);

            return null;
        }

        public IVertex Process(IVertex _baseVertex, string _text)
        {
            baseVertex = _baseVertex;

            text = _text + "\r\n"; // for regexpes

            prepareLineInfoList();

            prepareImportList();


            ProcessTextPart(_baseVertex, 0, lineInfoList.Count - 1);


            return null;
        }

        public String2ZeroCodeGraphProcessing()
        {
            setupHelpVariables();

            PrepareDictionaries();

            emptyKeywordVertex = MinusZero.Instance.Root.Get(@"User\CurrentUser:\CodeSettings:\Keyword:\$Keyword:(?<EmptyKeyword>)");
        }
    }
}