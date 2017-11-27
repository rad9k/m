using m0.Foundation;
using m0.Graph;
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

            return MinusZero.Instance.Root.Get(link);
        }

        IVertex ProcessLine(IVertex _baseVertex)
        {
            if (TryIsKeyword(currentLineNoTabs))
            {
                if (examinedKeywords.Count > 1)
                {
                    int x = 0; // HOW IS THAT
                }

                return AddKeywordVertex(_baseVertex, examinedKeywords[0]);
            }
            else {

                if (currentLineNoTabs.Length == 0)
                    return null;

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

        enum keywordTryingState { keywordCharacter, parameter, waiting, matched}

        class keywordTryingData
        {
            public IVertex keywordVertex;
            public String keyword;

            public keywordTryingState state;
            public int currentPositionInKeyword;
            
            public int waitingUntilPositionInText;
            public int matchedOnPositionInText;

            public string currentlyProcessedParameterName;
            public string afterParameterString;

            // below attributes are hidden

            string multiParameterSeparator;
            string multiParameterString;
            int currentPositionInMultiParameterString = -1; // if >0 => we are in the multiParam region
            string postMultiParameterString;

            public Dictionary<string, List<object>> parameters = new Dictionary<string, List<object>>();

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

            public bool currentPositionInKeyword_isParameterMatch()
            {
                if(ZeroCodeUtil.tryStringMatch(keyword, currentPositionInKeyword, "(*(+")){
                    int multiParameterSeparatorEndPos = ZeroCodeUtil.getNextMatch(keyword, currentPositionInKeyword + 4, "+)");

                    multiParameterSeparator = keyword.Substring(currentPositionInKeyword + 4, multiParameterSeparatorEndPos - currentPositionInKeyword - 4);

                    int multiParameterStringEndPos = ZeroCodeUtil.getNextMatch(keyword, multiParameterSeparatorEndPos, "*)");

                    multiParameterString = keyword.Substring(multiParameterSeparatorEndPos + 2, multiParameterStringEndPos - multiParameterSeparatorEndPos - 2);

                    currentPositionInMultiParameterString = 0;
                    //currentPositionInKeyword = multiParameterSeparatorEndPos + 2;
                }

                if (currentPositionInMultiParameterString != -1 &&
                    ZeroCodeUtil.tryStringMatch(keyword, currentPositionInKeyword, "(?<"))
                    return true;
                else
                    return false;

                if (ZeroCodeUtil.tryStringMatch(keyword, currentPositionInKeyword, "(?<"))
                    return true;
                else
                    return false;
            }

            public bool currentPositionCharacter_isCharacterMatch(char v)
            {
                MinusZero.Instance.Log(1, "isCharacterMatch",v+" ? "+ keyword[currentPositionInKeyword] + " | curPositionInKeyword:"+currentPositionInKeyword);


                if (keyword[currentPositionInKeyword] == v)
                    return true;
                else
                    return false;
            }

            public void GetParameter()
            {
                int begCurrentPositionInKeyword = currentPositionInKeyword;

                currentPositionInKeyword = ZeroCodeUtil.getNextMatch(keyword, currentPositionInKeyword + 2, ">)") + 2;
                currentlyProcessedParameterName = keyword.Substring(begCurrentPositionInKeyword + 3, currentPositionInKeyword - begCurrentPositionInKeyword - 5);
                afterParameterString = ZeroCodeUtil.getNextCharacterPartFromKeyword(keyword, currentPositionInKeyword);

                MinusZero.Instance.Log(1, "GetParameter", "currentlyProcessedParameterName:"+ currentlyProcessedParameterName+" curPositionInKeyword:" + currentPositionInKeyword+ " afterParameterString:"+ afterParameterString);

                state = keywordTryingState.parameter;
            }

            public void currentPositionInKeyword_Increase()
            {
                currentPositionInKeyword++;
            }

            public void AddParameter(string name, object val)
            {
                if (!parameters.ContainsKey(name))
                    parameters.Add(name, new List<object>());

                parameters[name].Add(val);
            }
        }

        List<keywordTryingData> examinedKeywords_All; // all keywords are here
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
                || !ZeroCodeUtil.tryStringEndMatch(s, ZeroCodeCommon.CodeGraphVertexSuffix))
            {
                string newVertex;
                string link;

                int tryPos = 0; 

                _tryIsKeyword("",firstCharacterPos_relativeToText, -1, 0, text.Length - 1, out examinedKeywords, out newVertex, out link, true, ref tryPos);

                if (examinedKeywords.Count() > 0)
                {
                    if (pos < tryPos)
                        pos = tryPos;

                    return true;
                }

                return false;
            }
            else
                return false;
        }

        void _tryIsKeyword(string LOGPREFIX, int startPos, int prev_startPos, int isPrevStartPosSameAsStartPosParentCount, int endPos,out List<keywordTryingData> examinedKeywords, out string newVertex, out string link, bool isTopLevelCall, ref int newPos)
        {
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

            examinedKeywords = new List<keywordTryingData>();            

            newVertex = null;

            link = null;

            int sPos = startPos;

            if (sPos == endPos)
                return;

            bool shallProceed = true;

            int tryNewPos = 0;
            string tryNewVertex = null;
            string tryLink = null;

            if (!isTopLevelCall)
            {
                // newVertex

                tryNewVertex = ZeroCodeCommon.tryStringFromNewVertexString(text, startPos, ref tryNewPos);

                if (tryNewVertex != null && 
                    ( (tryNewPos == endPos) || isPrevStartPosSameAsStartPos ))
                    // check if found fills all the needed space
                {
                    newVertex = tryNewVertex;
                    newPos = tryNewPos;

                    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "RETURN newVertex: " + newVertex);
                    return;
                }

                // link

                tryLink = ZeroCodeCommon.tryStringFromLinkString(text, startPos, ref tryNewPos);

                if (tryLink != null &&
                    ( (tryNewPos == endPos) || isPrevStartPosSameAsStartPos )) 
                    // check if found fills all the needed space
                {
                    link = tryLink;
                    newPos = tryNewPos;

                    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "RETURN link: " + link);
                    return;
                }
            }

            if (isPrevStartPosSameAsStartPos && isPrevStartPosSameAsStartPosParentCount==1) // do not want inifinite recursion
                return;

            // keyword

            copyExaminedKeywords(examinedKeywords_All, examinedKeywords);

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
                            if(ktd.currentPositionInKeyword_isParameterMatch())
                            {
                                ktd.GetParameter();
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
                            int isTryKeyword_endPos = endPos;

                            MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"TRY for parameter:" +ktd.currentlyProcessedParameterName+" for keyword:" + ktd.keyword + " afterParameterString:" + ktd.afterParameterString + "| ("+sPos+","+endPos+")");

                            if (ktd.afterParameterString != "")
                            {
                                int sPosAfterParameter = ZeroCodeUtil.getNextMatch(text, sPos, ktd.afterParameterString);

                                MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"sPosAfterParameter:" + sPosAfterParameter+ " for afterParameterString:"+ktd.afterParameterString);

                                if (sPosAfterParameter != -1
                                    && ((sPosAfterParameter < endPos) || (endPos == 0)))
                                {
                                    //isTryKeyword_endPos = sPosAfterParameter; // that will not work
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

                                _tryIsKeyword(LOGPREFIX+"    ",sPos, startPos, isPrevStartPosSameAsStartPosThisCount, isTryKeyword_endPos, out foundKeywords, out foundNewVertex, out foundLink, false, ref _newPos);

                                MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"waitingUntilPositionInText <= _newPos:" + _newPos);

                                if (foundNewVertex != null)
                                {
                                    ktd.waitingUntilPositionInText = _newPos;
                                    foundParameter = foundNewVertex;
                                }

                                if (foundLink != null)
                                {
                                    ktd.waitingUntilPositionInText = _newPos;
                                    foundParameter = new ToVertexMock(foundLink);
                                }

                                if (foundKeywords.Count() > 0)
                                {
                                    ktd.waitingUntilPositionInText = _newPos - 1;

                                    foundParameter = foundKeywords[0];

                                    if(foundParameters.Count() > 1)
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

                // check if only one matched

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
            }

            // if no keywords found, we can use tryNewVertex/tryLink, that are:
            // - filling needed space
            // - endPos==0 => needed space not defined 

            if (examinedKeywords.Count == 0)
            {
                newVertex = tryNewVertex;
                link = tryLink;
                newPos = tryNewPos;
            }else
                newPos = sPos;

            MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"END newVertex:" +newVertex+" link:"+link+" keywordsCount:"+examinedKeywords.Count);

            log_keywords(examinedKeywords, 0, LOGPREFIX);
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

        IVertex AddKeywordVertex(IVertex parent, keywordTryingData keyword)
        {
            return _AddKeywordVertex(parent,keyword,keyword.keywordVertex,null);
        }

        IVertex _AddKeywordVertex(IVertex parent, keywordTryingData ktd, IVertex keywordAddingVertex, IVertex useMetaWhenANY)
        {
            IVertex nv=null;

            foreach (IEdge e in keywordAddingVertex) {
                IVertex meta = e.Meta;

                if ((string)e.Meta.Value == "(?<ANY>)")
                    meta = useMetaWhenANY;

                if (VertexOperations.IsLink(e))
                {
                    if (ZeroCodeUtil.tryStringMatch((string)e.To.Value, 0, "(?<"))
                    {
                        string name = ZeroCodeUtil.getRegexp((string)e.To.Value, Regex.Escape("(?<") + "(?<EXTRACT>.*)" + Regex.Escape(">)"));

                        List<object> subs = ktd.parameters[name];

                        foreach (object sub in subs)
                        {
                            if (sub is string)
                                nv = parent.AddVertex(meta, sub); // ERROR

                            if (sub is ToVertexMock)
                                nv = parent.AddEdge(meta, (IVertex)sub).To;

                            if (sub is keywordTryingData)
                                nv = _AddKeywordVertex(parent, (keywordTryingData)sub, ((keywordTryingData)sub).keywordVertex, meta);
                        }
                    }
                    else
                        nv = parent.AddEdge(meta, e.To).To;
                }
                else
                {
                    if (ZeroCodeUtil.tryStringMatch((string)e.To.Value, 0, "(?<"))
                    {
                        string name = ZeroCodeUtil.getRegexp((string)e.To.Value, Regex.Escape("(?<") + "(?<EXTRACT>.*)" + Regex.Escape(">)"));

                        List<object> subs = ktd.parameters[name];

                        foreach (object sub in subs)
                        {
                            if (sub is string)
                                nv = parent.AddVertex(meta, sub);

                            if (sub is ToVertexMock)
                                nv = parent.AddEdge(meta, (IVertex)sub).To;

                            if (sub is keywordTryingData)
                                nv = _AddKeywordVertex(parent, (keywordTryingData)sub, ((keywordTryingData)sub).keywordVertex, meta);
                        }
                    }
                    else
                        nv = parent.AddVertex(meta, e.To);

                    _AddKeywordVertex(nv, ktd, e.To, null);
                }
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


