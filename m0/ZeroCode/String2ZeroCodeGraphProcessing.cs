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
        class TextRange
        {
            public int begLine;
            public int endLine;
            public bool isNonParameterRange;
        }


        class ParsingStack
        {
            public Dictionary<IVertex, int> sameStartPosKewords = new Dictionary<IVertex, int>();

            //

            String2ZeroCodeGraphProcessing processing;

            public ParsingStack parentStack;

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

            public int memory_tabCount = -1;
            public bool can_initialize_memory_tabCount = true;

            public Dictionary<object, TextRange> subTextRanges;


            public ParsingStack(String2ZeroCodeGraphProcessing _parent, ParsingStack _parentStack, int _begLine, int _endLine)
            {
                processing = _parent;
                parentStack = _parentStack;
                begLine = _begLine;
                endLine = _endLine;
               
                lineNo = begLine - 1;

                subTextRanges = new Dictionary<object, TextRange>();
            }

            public ParsingStack(ParsingStack _parentStack)
            {
                sameStartPosKewords = _parentStack.sameStartPosKewords;

                ///

                processing = _parentStack.processing;

                parentStack = _parentStack;

                begLine = _parentStack.begLine;
                endLine = _parentStack.endLine;

                lineNo = _parentStack.lineNo;
                currentLineInfo = _parentStack.currentLineInfo;

                currentLineNoTabs = _parentStack.currentLineNoTabs;

                LocalRoot = _parentStack.LocalRoot;

                lastAddedVertex = _parentStack.lastAddedVertex;
                lastAddedVertexParent = _parentStack.lastAddedVertexParent;
                newLineCount = _parentStack.newLineCount;

                skipParse = _parentStack.skipParse;
                parseRecurrentReturnNo = _parentStack.parseRecurrentReturnNo;

                memory_tabCount = _parentStack.memory_tabCount;

                can_initialize_memory_tabCount = _parentStack.can_initialize_memory_tabCount;

                subTextRanges = _parentStack.subTextRanges;
            }

            public void copyFrom(ParsingStack copyFrom)
            {
                sameStartPosKewords = copyFrom.sameStartPosKewords;

                ///

                processing = copyFrom.processing;

                parentStack = copyFrom;

                begLine = copyFrom.begLine;
                endLine = copyFrom.endLine;

                lineNo = copyFrom.lineNo;
                currentLineInfo = copyFrom.currentLineInfo;

                currentLineNoTabs = copyFrom.currentLineNoTabs;

                LocalRoot = copyFrom.LocalRoot;

                lastAddedVertex = copyFrom.lastAddedVertex;
                lastAddedVertexParent = copyFrom.lastAddedVertexParent;
                newLineCount = copyFrom.newLineCount;

                skipParse = copyFrom.skipParse;
                parseRecurrentReturnNo = copyFrom.parseRecurrentReturnNo;

                memory_tabCount = copyFrom.memory_tabCount;
                can_initialize_memory_tabCount = copyFrom.can_initialize_memory_tabCount;
            }

            public int getThisTabCount()
            {
                return currentLineInfo.tabCount;
            }

            public int getPrevTabCount()
            {
                return processing.lineInfoList[lineNo - 1].tabCount; // to be corrected
            }

            public int getNextLineWithSameTabCount()
            {
                List<LineInfo> li = processing.lineInfoList;
                int iterationLineNo = lineNo + 1;

                while (iterationLineNo < li.Count 
                    && li[iterationLineNo].tabCount != li[lineNo].tabCount)
                    iterationLineNo++;

                if (iterationLineNo == li.Count) 
                    return -1;

                if (li[iterationLineNo].tabCount != li[lineNo].tabCount)
                    return -1;

                return iterationLineNo;
            }

            public bool parseNextLine()
            {
                if (skipParse)
                {
                    skipParse = false;

                    return true;
                }

                if (lineNo >= endLine)
                    return false;

                lineNo++;

                currentLineInfo = processing.lineInfoList[lineNo];

                if (currentLineInfo.isEmpty)
                    currentLineNoTabs = "";
                else
                    currentLineNoTabs = processing.text.Substring(currentLineInfo.lineBeg, currentLineInfo.lineEnd - currentLineInfo.lineBeg + 1);

                // and now check if there are only whitespaces

                if (ZeroCodeUtil.isStringOnlyWhiteSpaces(currentLineNoTabs))
                {
                    newLineCount++;
                    return parseNextLine();
                }

                return true;
            }

            public bool goToLine(int newLineNo)
            {

                if (newLineNo > endLine)
                    return false;
                
                currentLineInfo = processing.lineInfoList[newLineNo];
                lineNo = newLineNo;

                if (currentLineInfo.isEmpty)
                    currentLineNoTabs = "";
                else
                    currentLineNoTabs = processing.text.Substring(currentLineInfo.lineBeg, currentLineInfo.lineEnd - currentLineInfo.lineBeg + 1);

                // we will do not do this now BUT might think about it in future

                // and now check if there are only whitespaces

                /*if (ZeroCodeUtil.isStringOnlyWhiteSpaces(currentLineNoTabs))
                {
                    newLineCount++;
                    return parseNextLine();
                }*/

                return true;
            }

        }

        // PARSER AUTO TEST SECTION

        /*int l1089 = 10;
        int l1149_dict = 0;
        int l1149_parent = 1;

        int L1009_left = 0;
        int L1009_right = 3;*/

        int l1089 = -1;
        int l1149_dict = -1;
        int l1149_parent = 1;

        int L1009_left = 0;
        int L1009_right = 1;

        //

        List<string> specialKeywordGroups;
        Dictionary<string, List<keywordTryingData>> examinedKeywords_All; // all keywords are here
        Dictionary<string, List<keywordTryingData>> examinedKeywords_LocalRootOnly; // all keywords are here
        Dictionary<char, List<string>> allKeywordsSubstringsDictionary;

        // special keywords

        IVertex emptyKeywordVertex;
        IVertex newValueKeywordVertex;

        // PROCESS dependent

        public IVertex baseVertex;

        string text;

        List<LineInfo> lineInfoList;

        //

        IVertex r = m0.MinusZero.Instance.Root;

    
        
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
            public String2ZeroCodeGraphProcessing parent;
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

            public object lastAddedParameter;
            public bool isCurrentlyProcessedSubParameter;

            //

            string multiParameterSeparator;
            string multiParameterString;
            string multiParamPlusSeparatorString;
            string multiParameterAfterParamBeforeSeparator;
            string multiParameterAfterSeparatorString;

            int currentPositionInMultiParamPlusSeparatorString = -1;

            int multiParameterStringBegPosition = -1;
            int multiParameterStringEndPosition = -1;

            bool lastCharWasSkippedSpace; // space support

            public keywordTryingData(keywordTryingData source)
            {
                parent = source.parent;
                keywordVertex = source.keywordVertex;
                keyword = source.keyword;
                currentPositionInKeyword = source.currentPositionInKeyword;
                state = source.state;
                currentlyProcessedParameterName = source.currentlyProcessedParameterName;
                afterParameterString = source.afterParameterString;
            }

            static Dictionary<string, string> keywordNoSpacesDict = new Dictionary<string, string>();

            public string removeSpaces(string s)
            {
                if (keywordNoSpacesDict.ContainsKey(s))
                    return keywordNoSpacesDict[s];
                else
                {
                    string noSpaces = "";

                    List<string> l = ZeroCodeUtil.tokenizeKeyword(s, true);

                    foreach (string t in l)
                    {
                        MinusZero.Instance.Log(-1, "T", t);
                        if (!ZeroCodeUtil.isStringOnlyWhiteSpaces(t))
                            noSpaces += t.Trim(new Char[]{' '});
                        else
                            noSpaces += t;
                    }

                    keywordNoSpacesDict.Add(s, noSpaces);

                    return noSpaces;
                }
            }

            public keywordTryingData(IVertex k, String2ZeroCodeGraphProcessing _processing)
            {
                parent = _processing;
                keywordVertex = k;
                
                //keyword = (String)keywordVertex.Value;

                keyword = removeSpaces((String)keywordVertex.Value);

                MinusZero.Instance.Log(-1, "KTD", keyword);

                currentPositionInKeyword = 0;
                state = keywordTryingState.keywordCharacter; // that and rest of the fields will be updated in the _tryKeyword
            }

            public int getMatchedOnPositionInText_Reccurent()
            {
                if (LocalRootNext == null)
                    return this.matchedOnPositionInText;

                return LocalRootNext.getMatchedOnPositionInText_Reccurent();
            }

            public bool currentPositionInKeyword_isParameterMatch(ParsingStack s, int curSpos)
            {
                 if(ZeroCodeUtil.tryStringMatch(keyword, currentPositionInKeyword, "(*") &&!isInMultiParameter())
                  {
                      if(ZeroCodeUtil.tryStringMatch(keyword, currentPositionInKeyword+2, "(+"))
                      {
                        int multiParameterSeparatorEndPos = ZeroCodeUtil.getNextMatch(keyword, currentPositionInKeyword + 4, "+)");

                        multiParameterSeparator = keyword.Substring(currentPositionInKeyword + 4, multiParameterSeparatorEndPos - currentPositionInKeyword - 4);

                        multiParameterStringBegPosition = multiParameterSeparatorEndPos + 2;

                        multiParameterStringEndPosition = ZeroCodeUtil.getNextMatch(keyword, multiParameterSeparatorEndPos, "*)") + 1;

                        multiParameterString = keyword.Substring(multiParameterStringBegPosition, multiParameterStringEndPosition - multiParameterStringBegPosition - 1);

                        multiParamPlusSeparatorString = multiParameterString + multiParameterSeparator;
                    }
                    else
                    {
                        multiParameterSeparator = "";

                        multiParameterStringBegPosition = currentPositionInKeyword + 2;

                        multiParameterStringEndPosition = ZeroCodeUtil.getNextMatch(keyword, multiParameterStringBegPosition, "*)") + 1;

                        multiParameterString = keyword.Substring(multiParameterStringBegPosition, multiParameterStringEndPosition - multiParameterStringBegPosition - 1);

                        multiParamPlusSeparatorString = multiParameterString;
                    }

                    int multiParameterAfterParamBeg = ZeroCodeUtil.getNextMatch(multiParameterString, 4, ">)") + 2;

                    multiParameterAfterParamBeforeSeparator = multiParameterString.Substring(multiParameterAfterParamBeg);

                    multiParameterAfterSeparatorString = ZeroCodeUtil.getNextCharacterPartFromKeyword_startingFromNonParameter(keyword, multiParameterStringEndPosition + 1);

                    if(ZeroCodeUtil.tabRemove_tryStringMatch(parent.text, curSpos, keyword.Substring(multiParameterStringEndPosition + 1), s.currentLineInfo.tabCount))
                    {
                        currentPositionInKeyword = multiParameterStringEndPosition + 1;
                    }
                    else
                    {// if there ARE muli parameters at all!
                        currentPositionInMultiParamPlusSeparatorString = 0;
                        multiParameterCount ++;
                    }
                }

                if (isInMultiParameter())
                {
                    if (ZeroCodeUtil.tryStringMatch(multiParamPlusSeparatorString, currentPositionInMultiParamPlusSeparatorString, "(?<"))
                        return true;
                    else
                        return false;
                }
                
                if (isSubPlus1(curSpos))
                {
                    isCurrentlyProcessedSubParameter = true;
                    return false;
                }else
                    isCurrentlyProcessedSubParameter = false;

                if (ZeroCodeUtil.tryStringMatch(keyword, currentPositionInKeyword, "(?<"))
                    return true;
                else
                    return false;
            }

            public bool isCurrentlyProcessedParameterAtom()
            {
                if (currentlyProcessedParameterName.EndsWith("Atom"))
                    return true;

                return false;
            }

            bool isInMultiParameter() // do not need that now, but maybe in the future?
            {
                return currentPositionInMultiParamPlusSeparatorString != -1;
                //return currentPositionInKeyword >= multiParameterStringBegPosition && currentPositionInKeyword <= multiParameterStringEndPosition;
            }

            public bool currentPositionCharacter_isCharacterMatch(ParsingStack s, int curPos)
            {
                MinusZero.Instance.Log(1, "isCharacterMatch", curPos + " ? "+ keyword[currentPositionInKeyword] + " | curPositionInKeyword:"+currentPositionInKeyword);

                lastCharWasSkippedSpace = false;

                if (isInMultiParameter())
                {
                    if ((currentPositionInMultiParamPlusSeparatorString == multiParameterString.Length // after multi param string
                       || (multiParameterCount > 1 && currentPositionInMultiParamPlusSeparatorString == 0)) // after multi param and no separator
                       // && keyword[multiParameterStringEndPosition + 1] == v) // we are going out of multi
                       && ZeroCodeUtil.tabRemove_tryStringMatch(parent.text,curPos,keyword.Substring(multiParameterStringEndPosition + 1), s.currentLineInfo.tabCount)) // this is the way
                    {
                        currentPositionInKeyword = multiParameterStringEndPosition + 1;
                        currentPositionInMultiParamPlusSeparatorString = -1; // out of multi
                        return true;
                    }

                    if (multiParamPlusSeparatorString[currentPositionInMultiParamPlusSeparatorString] == parent.text[curPos])
                        return true;
                    else
                    {
                        if (parent.text[curPos] == ' ')
                        {
                            lastCharWasSkippedSpace = true;
                            return true;
                        }

                        return false;
                    }
                }

                if (keyword[currentPositionInKeyword] == parent.text[curPos])                   
                    return true;
                else
                {
                    if (parent.text[curPos] == ' ')
                    {
                        lastCharWasSkippedSpace = true;
                        return true;
                    }

                    return false;
                }
            }

            public void PrepareParameterAndAfterParameterString(int curSpos)
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

                if (isInMultiParameter()) {
                    if(currentPosition == multiParameterString.Length)
                    {
                            int whatMatch;

                            multiParameterAfterSeparatorString = ZeroCodeUtil.getNextCharacterPartFromKeyword_startingFromNonParameter(keyword, multiParameterStringEndPosition + 1);

                            int twoPos = ZeroCodeUtil.getNextMatch_twoAtOnce(parent.text, curSpos,
                                multiParameterSeparator,
                                multiParameterAfterSeparatorString,
                                out whatMatch);

                            if (whatMatch == 0)
                                afterParameterString = "";

                            if (whatMatch == 1)
                                afterParameterString = multiParameterSeparator;

                            if (whatMatch == 2)
                                afterParameterString = multiParameterAfterSeparatorString;
                    }
                    else
                    {
                        int whatMatch;

                        ZeroCodeUtil.getNextMatch_twoAtOnce(parent.text, curSpos,
                            multiParameterAfterParamBeforeSeparator + multiParameterSeparator,
                            multiParameterAfterParamBeforeSeparator + multiParameterAfterSeparatorString,
                            out whatMatch);

                        if (whatMatch == 0)
                            afterParameterString = "";

                        if (whatMatch == 1)
                            afterParameterString = multiParameterAfterParamBeforeSeparator + multiParameterSeparator;

                        if (whatMatch == 2)
                            afterParameterString = multiParameterAfterParamBeforeSeparator + multiParameterAfterSeparatorString;
                    }
                } else
                    afterParameterString = ZeroCodeUtil.getNextCharacterPartFromKeyword_startingFromNonParameter(str, currentPosition);

                MinusZero.Instance.Log(1, "GetParameter", "currentlyProcessedParameterName:"+ currentlyProcessedParameterName+" curPosition:" + currentPosition+ " afterParameterString:"+ afterParameterString);

                state = keywordTryingState.parameter;

                if (isInMultiParameter())
                {
                    currentPositionInMultiParamPlusSeparatorString = currentPosition - 1;
                    currentPositionInKeyword_Increase(curSpos);
                }
                else
                    currentPositionInKeyword = currentPosition;
                   
            }

            private bool isSubPlus1(int curSpos)
            {
                if (ZeroCodeUtil.tryStringMatch(keyword, currentPositionInKeyword + 1, "(?<SUB>)") && parent.text[curSpos] != ' ')
                    return true;

                return false;
            }

            public void currentPositionInKeyword_Increase(int curSpos)
            {

                if (isSubPlus1(curSpos))
                {
                    currentPositionInKeyword += 9;
                    return;
                }
                
                if (isInMultiParameter())
                {
                    if (!lastCharWasSkippedSpace)
                    {
                        if (currentPositionInMultiParamPlusSeparatorString < multiParamPlusSeparatorString.Length - 1)
                        {
                            currentPositionInMultiParamPlusSeparatorString++;
                        }
                        else
                        {
                            currentPositionInMultiParamPlusSeparatorString = -1;
                            //  multiParameterCount++;
                        }
                    }

                        MinusZero.Instance.Log(1, "currentPositionInKeyword_Increase", "MULTI:" + currentPositionInMultiParamPlusSeparatorString);
                    }
                else
                {
                    if (!lastCharWasSkippedSpace)
                        currentPositionInKeyword++;
                    MinusZero.Instance.Log(1, "currentPositionInKeyword_Increase", "NORMAL:" + currentPositionInKeyword);
                }
            }

            public void AddParameter(string name, object val)
            {
                if (!parameters.ContainsKey(name))
                    parameters.Add(name, new List<object>());

                parameters[name].Add(val);

                lastAddedParameter = val;
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
            string xx = "";
            for (int x = s.currentLineInfo.lineBeg; x <= s.currentLineInfo.lineEnd; x++)
              xx += " "+x+":"+text[x];

            MinusZero.Instance.Log(-1, "TryIfIsKeywordLine", xx);

          

            List <keywordTryingData> examinedKeywords;

            if (s.currentLineInfo.lineBeg >= text.Length)
                return null;

            if (!ZeroCodeUtil.tryStringMatch(s.currentLineNoTabs, 0, ZeroCodeCommon.CodeGraphVertexPrefix) 
                || !ZeroCodeUtil.tryStringEndMatch(s.currentLineNoTabs, ZeroCodeCommon.CodeGraphVertexSuffix))
            {
                string link;

                int tryPos = 0;


                _tryIsKeyword(s, "", s.currentLineInfo.lineBeg, s.currentLineInfo.lineBeg, 0, text.Length - 1, text.Length - 1, false, out examinedKeywords, out link, true, ref tryPos, false, null, null, "", false);

                if (examinedKeywords.Count() > 0)
                    return examinedKeywords;                

                return null;
            }
            else
                return null;
        }

        enum SpecialKeywordType { EmptyKeyword, NewVertexKeyword}

        keywordTryingData createSpecialKeyword(ParsingStack s, string value, int matchedOnPositionInText, SpecialKeywordType type)
        {
            if (value == null || value == "")
            {
                int x = 0; // WTF???
            }

            IVertex toUseVertex=null;

            switch (type)
            {
                case SpecialKeywordType.EmptyKeyword:
                    toUseVertex = emptyKeywordVertex;
                    break;

                case SpecialKeywordType.NewVertexKeyword:
                    toUseVertex = newValueKeywordVertex;
                    break;
            }

            keywordTryingData ktd = new keywordTryingData(toUseVertex, this);

            ktd.matchedOnPositionInText = matchedOnPositionInText;

            List<object> l = new List<object>();
            l.Add(value);

            ktd.parameters.Add("value", l);

            //ktd.AddParameter("value", value); // been tested with above // no lastparam modification

            return ktd;
        }

        class ParameterChache
        {
            public object Parameter = null;
            public int waitingUntilPositionInText = 0;
        }

        void _tryAtom(ParsingStack s, string LOGPREFIX, int startPos, int endPos, out List<keywordTryingData> examinedKeywords, out string link, ref int newPos)
        {
            examinedKeywords = new List<keywordTryingData>();

            link = null;

            //

            if (s.currentLineInfo.IsLineEnd(startPos))
            { // the + 2 might be not needed, but who knows....
              // if (text[startPos] == '\r' || text[startPos] == '\n')
                newPos = s.currentLineInfo.lineEnd_NoTrim + 1;
                return;
            }

            if (startPos == endPos)
                return;

            //
            
            MinusZero.Instance.Log(1, "_tryAtom", LOGPREFIX + "BEG startPos:" + startPos + " endPos:" + endPos);

            int sPos = startPos;

            if (sPos == endPos)
                return;

            //

            SpecialKeywordType specialType = SpecialKeywordType.NewVertexKeyword; // got to intialize

            //

            string tryEmptyKeyword;
            bool shallProceed = true;

            if (!testIfIsKeywordSubstring(startPos))
            {
                tryEmptyKeyword = ZeroCodeCommon.tryStringFromNewVertexString(text, startPos, ref sPos);

                if (tryEmptyKeyword != null)
                {
                    specialType = SpecialKeywordType.NewVertexKeyword;

                    sPos++; // hmmm ????
                }
                else
                {
                    while (shallProceed)
                    {
                        sPos++;

                        if (testIfIsKeywordSubstring(sPos))
                            shallProceed = false;

                        if (s.currentLineInfo.IsLineEnd(sPos))
                        { // the + 2 might be not needed, but who knows....
                          //if (text[sPos] == '\r' || text[sPos] == '\n')
                            sPos = s.currentLineInfo.lineEnd_NoTrim + 1;
                            shallProceed = false;
                        }

                        if (sPos == endPos)
                            shallProceed = false;
                    }

                    string foundString = text.Substring(startPos, sPos - startPos);

                    if (ZeroCodeCommon.isLinkString(foundString))
                        link = ZeroCodeCommon.stringFromLinkString(foundString, false);
                    else
                    {
                        tryEmptyKeyword = foundString;

                        specialType = SpecialKeywordType.EmptyKeyword;

                        sPos++; // hmmm ????
                    }
                }

                newPos = sPos;

                if (tryEmptyKeyword != null)
                {
                    keywordTryingData ktd = createSpecialKeyword(s, tryEmptyKeyword, sPos - 1, specialType);

                    //

                    newPos = _tryIsNextLocalRootKeyword(s, LOGPREFIX, newPos, sPos, ktd, false);

                    //

                    examinedKeywords.Add(ktd);
                }

                MinusZero.Instance.Log(1, "_tryAtom", LOGPREFIX + "RETURN:" + tryEmptyKeyword + " newPos:" + newPos);
            }
        }

        class tryIsKeyword_Parameters
        {
            public ParsingStack s;
            public string LOGPREFIX;
            public int startPos;
            public int prev_startPos;
            public int isPrevStartPosSameAsStartPosParentCount;
            public int endPos;
            public int endPos_forAtomParts;
            public bool afterKeywordPartExist;
            public IVertex parentKeyword;
            public tryIsKeyword_Parameters parentParams;

            public tryIsKeyword_Parameters(ParsingStack _s, string _LOGPREFIX, int _startPos, int _prev_startPos, int _isPrevStartPosSameAsStartPosParentCount, int _endPos, int _endPos_forAtomParts, bool _afterKeywordPartExist, IVertex _parentKeyword, tryIsKeyword_Parameters _parentParams)
            {
                s = _s;
                LOGPREFIX = _LOGPREFIX;
                startPos = _startPos;
                prev_startPos = _prev_startPos;
                isPrevStartPosSameAsStartPosParentCount = _isPrevStartPosSameAsStartPosParentCount;
                endPos = _endPos;
                endPos_forAtomParts = _endPos_forAtomParts;
                afterKeywordPartExist = _afterKeywordPartExist;
                parentKeyword = _parentKeyword;
                parentParams = _parentParams;
            }

            public override string ToString()
            {
                if(parentKeyword!=null)
                    return "startPos: " + startPos + " parentKeyword: " + parentKeyword.Value + " prev_startPos: " + prev_startPos + " PrevStartCount: " + isPrevStartPosSameAsStartPosParentCount + " endPos: " + endPos + " endPos_forAtomParts: " + endPos_forAtomParts + " afterKeywordPartExist: " + afterKeywordPartExist;
                else
                    return "startPos: " + startPos + " parentKeyword: NULL" + " prev_startPos: " + prev_startPos + " PrevStartCount: " + isPrevStartPosSameAsStartPosParentCount + " endPos: " + endPos + " endPos_forAtomParts: " + endPos_forAtomParts + " afterKeywordPartExist: " + afterKeywordPartExist;
            }
        }

        void _tryIsKeyword(ParsingStack s, string LOGPREFIX, int startPos, int prev_startPos, int isPrevStartPosSameAsStartPosParentCount, int endPos, int endPos_forAtomParts, bool afterKeywordPartExist, out List<keywordTryingData> examinedKeywords, out string link, bool isTopLevelCall, ref int newPos, bool lookForLocalRootOnly, IVertex parentKeyword, tryIsKeyword_Parameters parentParams, string keywordsFilter, bool isSpaceNext)
        {
            tryIsKeyword_Parameters callParams = new tryIsKeyword_Parameters(s, LOGPREFIX, startPos, prev_startPos, isPrevStartPosSameAsStartPosParentCount, endPos, endPos_forAtomParts, afterKeywordPartExist, parentKeyword, parentParams);
            MinusZero.Instance.Log(0, "_tryIfKeyword", LOGPREFIX + "RUN "+callParams.ToString());

            //

            examinedKeywords = new List<keywordTryingData>();

            link = null;

            //

            if (s.currentLineInfo.IsLineEnd(startPos))
            { // the + 2 might be not needed, but who knows....
              //if (text[startPos] == '\r' || text[startPos] == '\n')
                newPos = s.currentLineInfo.lineEnd_NoTrim + 1;
                return;
            }

            if (startPos == endPos_forAtomParts)
                return;

            //

            bool isPrevStartPosSameAsStartPos = false;

            int isPrevStartPosSameAsStartPosThisCount = isPrevStartPosSameAsStartPosParentCount;

            int left=0;
            int right=-1;

            if (parentParams != null) {

                switch (L1009_left)
                {
                    case 0: left = startPos; break;
                    case 1: left = prev_startPos; break;
                    case 2: left = parentParams.startPos; break;
                    case 3: left = parentParams.prev_startPos; break;
                }

                switch (L1009_right)
                {
                    case 0: right = startPos; break;
                    case 1: right = prev_startPos; break;
                    case 2: right = parentParams.startPos; break;
                    case 3: right = parentParams.prev_startPos; break;
                }

                //if (startPos == prev_startPos)
                //if (prev_startPos == parentParams.prev_startPos)
                if(left == right)
                {
                    isPrevStartPosSameAsStartPos = true;
                    isPrevStartPosSameAsStartPosThisCount++;
                }
                else
                {
                    isPrevStartPosSameAsStartPosThisCount = 0;
                    s.sameStartPosKewords.Clear();
                }
            }

            string xx = "";

            MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"BEG startPos:" + startPos + " prevSpos:"+prev_startPos+" same:"+isPrevStartPosSameAsStartPos+" endPos:" + endPos+" "+xx);


            int sPos = startPos;

            if (sPos == endPos)
                return;

            bool shallProceed = true;

            int tryNewPos = 0;
            string tryLink = null;
            string tryEmptyKeyword = null;
            //ParsingStack tryEmptyKeywordStack=null;
            int tryEmptyKeywordStack_LineNoMemory=-1;

            //

            SpecialKeywordType specialType = SpecialKeywordType.NewVertexKeyword; // got to intialize

            //

            int sPos_copy;

            if (!testIfIsKeywordSubstring(sPos))
            {
                tryEmptyKeyword = ZeroCodeCommon.tryStringFromNewVertexString(text, startPos, ref sPos);

                if (tryEmptyKeyword != null)
                {
                    specialType = SpecialKeywordType.NewVertexKeyword;

                    sPos_copy = sPos;

                    sPos++; // hmmm ????
                }
                else
                {
                    while (shallProceed)
                    {
                        sPos++;

                        if (testIfIsKeywordSubstring(sPos))
                            shallProceed = false;

                        if (s.currentLineInfo.IsLineEnd(sPos))
                        { // the + 2 might be not needed, but who knows....
                          //if (text[sPos] == '\r' || text[sPos] == '\n')
                            sPos = s.currentLineInfo.lineEnd_NoTrim + 1;
                            shallProceed = false;
                        }

                        if (sPos == endPos_forAtomParts)
                            shallProceed = false;
                    }

                    sPos_copy = sPos;

                    string foundString = text.Substring(startPos, sPos - startPos);

                    if (!isTopLevelCall
                        && ZeroCodeCommon.isLinkString(foundString))
                        tryLink = ZeroCodeCommon.stringFromLinkString(foundString, false);
                    else
                    {
                        tryEmptyKeyword = foundString;

                        specialType = SpecialKeywordType.EmptyKeyword;

                        sPos++; // hmmm ????
                    }
                }

                //MinusZero.Instance.Log(0, "_tryIfKeyword:", LOGPREFIX + "conditions 0: TRY / sPos_copy:" + sPos_copy + " isPrevStartPosSameAsStartPos: " + isPrevStartPosSameAsStartPos);

                bool c1089 = false;

                if (l1089 == -1)
                {
                    if ((afterKeywordPartExist && sPos_copy == endPos_forAtomParts && isPrevStartPosSameAsStartPos && isPrevStartPosSameAsStartPosThisCount > 2)
                    || (!afterKeywordPartExist && (sPos == endPos_forAtomParts || isPrevStartPosSameAsStartPos)))
                        c1089 = true;
                }

                if(l1089 == 0)
                {
                    if (sPos == endPos_forAtomParts || (isPrevStartPosSameAsStartPos /*&& isPrevStartPosSameAsStartPosThisCount > 1*/))
                        c1089 = true;
                }

                if (l1089 > 0)
                {
                    if (sPos == endPos_forAtomParts || (isPrevStartPosSameAsStartPos && isPrevStartPosSameAsStartPosThisCount > l1089))
                        c1089 = true;
                }

                if (c1089 || specialKeywordGroups.Contains(keywordsFilter) /*keywordsFilter=="Atom"*/)
                    //( (afterKeywordPartExist && sPos_copy == endPos_forAtomParts && isPrevStartPosSameAsStartPos)
                    //|| (!afterKeywordPartExist && (sPos == endPos_forAtomParts || isPrevStartPosSameAsStartPos)))
                    //( //(tryEmptyKeyword != null || lookForLocalRootOnly==false) &&
                    // ( sPos == endPos_forAtomParts || (isPrevStartPosSameAsStartPos /*&& isPrevStartPosSameAsStartPosThisCount > 1*/)) ) // !!!
                {
                   // MinusZero.Instance.Log(0, "_tryIfKeyword", LOGPREFIX + "conditions 0: ENTER");
                    link = tryLink;

                    newPos = sPos;

                    if (tryEmptyKeyword != null)
                    {
                        keywordTryingData ktd = createSpecialKeyword(s, tryEmptyKeyword, sPos - 1, specialType);

                        //

                        newPos = _tryIsNextLocalRootKeyword(s, LOGPREFIX, newPos, sPos, ktd, isSpaceNext);

                        //

                        examinedKeywords.Add(ktd);
                    }
                    
                    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "RETURN:" + tryEmptyKeyword + " newPos:" + newPos);

                    return;
                }
                else
                {
                    tryNewPos = sPos;

                    sPos = startPos;

                    if (tryEmptyKeyword != null)
                    {
                        // SAVE LINE NO MEMORY

                        //tryEmptyKeywordStack = new ParsingStack(s); // COPY STACK
                        tryEmptyKeywordStack_LineNoMemory = s.lineNo - 1;
                        //
                    }
                }
                
            }

            // no infinite reccursion

            string _s = "";
            foreach (KeyValuePair<IVertex,int> v in s.sameStartPosKewords)
                //if(v!=null)
                    _s += ", " + v.Key.Value+"["+v.Value+"]";

           // MinusZero.Instance.Log(0, "_tryIsKeyword", LOGPREFIX + "REKURSION " + _s);

            bool containsCondition = false;

            if (parentKeyword!=null && s.sameStartPosKewords.ContainsKey(parentKeyword))
            {
                if (s.sameStartPosKewords[parentKeyword] > l1149_dict)
                    containsCondition = true;
            }

           // if (isPrevStartPosSameAsStartPos && isPrevStartPosSameAsStartPosParentCount == 1)
            if((l1149_dict>-1 && containsCondition) 
                || (l1149_parent>-1 && isPrevStartPosSameAsStartPos && isPrevStartPosSameAsStartPosThisCount > l1149_parent)
                || keywordsFilter == "Atom")
            { // do not want inifinite recursion
               // if(containsCondition)
               //     MinusZero.Instance.Log(0, "_tryIsKeyword", LOGPREFIX + "HARD RETURN PARENT "+ parentKeyword.Value);
               // else
                    MinusZero.Instance.Log(0, "_tryIsKeyword", LOGPREFIX + "HARD RETURN");

                return;
            }
            else
            {
                if (parentKeyword != null)
                    if (s.sameStartPosKewords.ContainsKey(parentKeyword))
                        s.sameStartPosKewords[parentKeyword]++;
                    else
                        s.sameStartPosKewords.Add(parentKeyword,1);
            }

            // keyword

            if(lookForLocalRootOnly)
                copyExaminedKeywords(examinedKeywords_LocalRootOnly[keywordsFilter], examinedKeywords);
            else
                copyExaminedKeywords(examinedKeywords_All[keywordsFilter], examinedKeywords);

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
                            if(ktd.currentPositionInKeyword_isParameterMatch(s, sPos))
                            {
                                ktd.PrepareParameterAndAfterParameterString(sPos);
                            }
                            else
                                // keywordCharacter => keywordCharacer
                                if ( ktd.currentPositionCharacter_isCharacterMatch(s, sPos) )
                                {
                                    ktd.currentPositionInKeyword_Increase(sPos);

                                    newExaminedKeywords.Add(ktd);
                                    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"sPos:" + sPos + " keywordCharacter -> keywordCharacter");
                                }
                                else
                                {
                                    MinusZero.Instance.Log(0, "_tryIsKeyword", LOGPREFIX+"sPos:" + sPos + "***OUT OF*** keywordCharacter match");
                                }
                        }                    
                    }
                }

                Dictionary<string, ParameterChache> ParameterChache = new Dictionary<string, ParameterChache>();

                ParameterChache NonAtomParameterChache = null;
                ParameterChache AtomParameterChache = null;

                // check if anything fits info keyword parameters
                foreach (keywordTryingData ktd in examinedKeywords)
                    if (ktd.state == keywordTryingState.parameter)
                    {
                        while (text[sPos] == ' ') // spaces handling
                            sPos++;

                        bool chacheHit = false;

                        object foundParameter = null;

                        string paramFilterName = getKewordFilterFromParamName(ktd.currentlyProcessedParameterName);

                        if (ParameterChache.ContainsKey(paramFilterName))
                        {
                            chacheHit = true;

                            foundParameter = ParameterChache[paramFilterName].Parameter;
                            ktd.waitingUntilPositionInText = ParameterChache[paramFilterName].waitingUntilPositionInText;
                        }

                        /*if(ktd.isCurrentlyProcessedParameterAtom() && AtomParameterChache != null)
                        {
                            chacheHit = true;
                            foundParameter = AtomParameterChache.Parameter;
                            ktd.waitingUntilPositionInText = AtomParameterChache.waitingUntilPositionInText;
                        }

                        if (!chacheHit &&
                            !ktd.isCurrentlyProcessedParameterAtom() && NonAtomParameterChache != null)
                        {
                            chacheHit = true;
                            foundParameter = NonAtomParameterChache.Parameter;
                            ktd.waitingUntilPositionInText = NonAtomParameterChache.waitingUntilPositionInText;
                        }*/

                        if(!chacheHit) 
                        {
                            int isTryKeyword_endPos = endPos_forAtomParts;

                            bool _afterKeywordPartExist = false;

                            MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"TRY for parameter:" +ktd.currentlyProcessedParameterName+" for keyword:" + ktd.keyword + " afterParameterString:" + ktd.afterParameterString + "| ("+sPos+","+endPos+")");

                            bool _isSpaceNext = false;

                            if (ktd.afterParameterString != "")
                            {
                                if (ktd.afterParameterString.Length > 0 && ktd.afterParameterString[0] == ' ')
                                    _isSpaceNext = true;

                                int sPosAfterParameter = ZeroCodeUtil.getNextMatch(text, sPos, ktd.afterParameterString);

                                MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"sPosAfterParameter:" + sPosAfterParameter+ " for afterParameterString:"+ktd.afterParameterString);

                                if (sPosAfterParameter != -1
                                    && ((sPosAfterParameter < endPos) || (endPos == 0)))
                                {
                                    isTryKeyword_endPos = sPosAfterParameter;
                                    _afterKeywordPartExist = true;
                                }
                                else
                                    isTryKeyword_endPos = -1; // do not search; this keyword does not fit in text
                            }

                            MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"isTryKeyword_endPos:" + isTryKeyword_endPos);

                            if (isTryKeyword_endPos != -1)
                            {
                                List<keywordTryingData> foundKeywords = null;
                                string foundLink = null;
                                int _newPos = 0;

                                MinusZero.Instance.Log(1, LOGPREFIX+"_tryIsKeyword", "will run _tryIs for:"+ ktd.currentlyProcessedParameterName);

                              /*  if (ktd.isCurrentlyProcessedParameterAtom())
                                {
                                    //MinusZero.Instance.Log(0, "_tryIfKeyword", LOGPREFIX + "_tryATOM / " + ktd.currentlyProcessedParameterName + " / " + ktd.keywordVertex.Value);

                                    _tryAtom(s, LOGPREFIX + "    ", sPos, isTryKeyword_endPos, out foundKeywords, out foundLink, ref _newPos);
                                }
                                else
                                {*/
                                    // NEW STACK

                                    ParsingStack newStack = new ParsingStack(s);

                                    s = newStack;

                                    s.can_initialize_memory_tabCount = true;

                                    //

                                    //MinusZero.Instance.Log(0, "_tryIfKeyword", LOGPREFIX + "_tryIsCall / " + ktd.currentlyProcessedParameterName + " / " + ktd.keywordVertex.Value);

                                    MinusZero.Instance.Log(-1, "_tryIfKeyword", LOGPREFIX + startPos + " " + ktd.keywordVertex.Value + ktd.currentlyProcessedParameterName);
                                   // int modified_isPrevStartPosSameAsStartPosThisCount = isPrevStartPosSameAsStartPosThisCount;

                                   // if (sPos != startPos)
                                      //  modified_isPrevStartPosSameAsStartPosThisCount = 0;

                                    _tryIsKeyword(s, LOGPREFIX + "    ", sPos, startPos, isPrevStartPosSameAsStartPosThisCount, endPos, isTryKeyword_endPos, _afterKeywordPartExist, out foundKeywords, out foundLink, false, ref _newPos, false, ktd.keywordVertex, callParams, paramFilterName, _isSpaceNext);

                                    // BACK TO OLD STACK

                                    s = s.parentStack;

                                    //
                                //}

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
                            if (!chacheHit) {
                                ParameterChache p = new ParameterChache();

                                p.Parameter = foundParameter;
                                p.waitingUntilPositionInText = ktd.waitingUntilPositionInText;

                                ParameterChache.Add(paramFilterName, p);

                              /*  if (ktd.isCurrentlyProcessedParameterAtom())
                                {
                                    AtomParameterChache = new ParameterChache();
                                    AtomParameterChache.Parameter = foundParameter;
                                    AtomParameterChache.waitingUntilPositionInText = ktd.waitingUntilPositionInText;
                                }
                                else
                                {
                                    NonAtomParameterChache = new ParameterChache();
                                    NonAtomParameterChache.Parameter = foundParameter;
                                    NonAtomParameterChache.waitingUntilPositionInText = ktd.waitingUntilPositionInText;
                                }*/
                            }                                                        

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

                if(shallProceed)
                {
                    if (sPos + 1 < endPos && s.currentLineInfo.IsLineEnd(sPos + 1)) // SUB TEXT
                    //if(sPos + 1 < endPos && text[sPos + 1] == '\r')
                    {
                        int nextLineWithSameTabCount = s.getNextLineWithSameTabCount();

                        if (nextLineWithSameTabCount != -1 && lineInfoList[nextLineWithSameTabCount].startsWithLineContinuation)
                        {
                            foreach (keywordTryingData ktd in examinedKeywords)
                            {
                                TextRange subText = new TextRange();
                                subText.begLine = s.lineNo + 1;
                                subText.endLine = nextLineWithSameTabCount - 1;

                                bool canAddRange = true;

                                foreach (KeyValuePair<object, TextRange> kvp in s.subTextRanges)
                                    if (kvp.Key is keywordTryingData)
                                        if (kvp.Value.begLine == subText.begLine && checkIfKtdContainsKtdAsAParent(ktd, (keywordTryingData)kvp.Key))
                                            canAddRange = false;                             

                                if(canAddRange)
                                    if (ktd.lastAddedParameter == null || ktd.isCurrentlyProcessedSubParameter)
                                    {
                                        subText.isNonParameterRange = true;
                                        s.subTextRanges.Add(ktd, subText);
                                    }
                                    else
                                        if (!s.subTextRanges.ContainsKey(ktd.lastAddedParameter))
                                    {
                                       // MinusZero.Instance.Log(-1, "XXX", ((keywordTryingData)ktd.lastAddedParameter).keyword);
                                        s.subTextRanges.Add(ktd.lastAddedParameter, subText);
                                    }
                            }

                            s.goToLine(nextLineWithSameTabCount);

                            foreach (keywordTryingData ktd in examinedKeywords)
                                if (ktd.state == keywordTryingState.waiting && ktd.waitingUntilPositionInText == sPos + 1)
                                    ktd.state = keywordTryingState.keywordCharacter;

                            sPos = s.currentLineInfo.lineBeg;                            
                        }
                    }

                    if(s.currentLineInfo.IsLineEnd(sPos)) // NEW LINE
                    //if (text[sPos] == '\r')
                    {
                        sPos = s.currentLineInfo.lineEnd_NoTrim + 1;

                        foreach (keywordTryingData ktd in examinedKeywords)
                            if (ktd.state == keywordTryingState.matched)
                            {
                                shallProceed = false; // end of line and one of keywords matched
                                MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "SHALPROCEED FALSE end of line and one of keywords matched");
                            }

                        if (shallProceed) // jump to next line
                        {
                            if (s.can_initialize_memory_tabCount)
                            {
                                s.memory_tabCount = s.currentLineInfo.tabCount;
                                s.can_initialize_memory_tabCount = false;
                            }

                            //

                            sPos++;

                            newExaminedKeywords = new List<keywordTryingData>();

                            foreach (keywordTryingData ktd in examinedKeywords)
                            {
                                if (ktd.state == keywordTryingState.keywordCharacter &&
                                    ktd.currentPositionCharacter_isCharacterMatch(s, sPos))
                                {
                                    ktd.currentPositionInKeyword_Increase(sPos);

                                    newExaminedKeywords.Add(ktd);
                                }

                                if (ktd.state == keywordTryingState.waiting &&
                                    sPos <= ktd.waitingUntilPositionInText)
                                    newExaminedKeywords.Add(ktd);
                            }

                            examinedKeywords = newExaminedKeywords;

                            LineInfo prevLine = s.currentLineInfo;

                            shallProceed = s.parseNextLine();

                            if (shallProceed)
                                sPos = s.currentLineInfo.getPosWithParentTabsTrimmed(s) - 1;
                        }                        
                    }

                    if (examinedKeywords.Count == 0)
                    {
                        shallProceed = false; // no keyword found
                        MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"SHALPROCEED FALSE  no keyword found");
                    }
                }
                
                // ++

                sPos++;

                if (s.currentLineInfo.IsLineEnd(sPos))
                    sPos = s.currentLineInfo.lineEnd_NoTrim + 1;

                MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "sPos++ " + sPos);

            }

            // out of shallProced

            s.memory_tabCount = -1;
            s.can_initialize_memory_tabCount = true;

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

                // WE ASSUME THAT ONLY examinedKeywords[0] will be used further

                //

                if(examinedKeywords.Count > 1)
                {
                    int x = 0;
                }

                if (examinedKeywords.Count > 0)
                {

                    keywordTryingData ktd = examinedKeywords[0]; // ASSUMPTION

                    if (ktd.matchedOnPositionInText <= s.currentLineInfo.lineEnd
                        && isLocalRootKeyword(ktd))
                        sPos = _tryIsNextLocalRootKeyword(s, LOGPREFIX, sPos, ktd.matchedOnPositionInText + 1, ktd, isSpaceNext);
                }
                //

            }

            // if no keywords found, we can use tryNewVertex/tryLink, that are:
            // - filling needed space
            // - endPos==0 => needed space not defined 

            if (examinedKeywords.Count == 0)
            {
                link = tryLink;

                newPos = tryNewPos;

                if (tryEmptyKeyword != null)
                {
                    keywordTryingData ktd = createSpecialKeyword(s, tryEmptyKeyword, tryNewPos - 1,  specialType);

                    ParsingStack copy = s;

                    //s = tryEmptyKeywordStack; // THIS DOES NOT WORK // GET THE STACK FROM COPY

                    s.lineNo = tryEmptyKeywordStack_LineNoMemory;
                    s.parseNextLine();

                    //

                    newPos = _tryIsNextLocalRootKeyword(s, LOGPREFIX, newPos, tryNewPos, ktd, isSpaceNext);

                    //

                    examinedKeywords.Add(ktd);
                }
            }else
                newPos = sPos;

            MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"END link:"+link+" keywordsCount:"+examinedKeywords.Count);

            log_keywords(examinedKeywords, 0, LOGPREFIX);
        }

        string getKewordFilterFromParamName(string s)
        {
            int p = s.LastIndexOf('_');

            if (p == -1 || p==s.Length-1)
                return "";

            return s.Substring(p + 1);
        }

        bool checkIfKtdContainsKtdAsAParent(keywordTryingData test, keywordTryingData child)
        {
            if (test == child)
                return true;

            foreach (List<object> l in test.parameters.Values)
                foreach(object o in l)
                if (o is keywordTryingData)
                    if (checkIfKtdContainsKtdAsAParent((keywordTryingData)o, child))
                        return true;

            return false;
        }

        bool isLocalRootKeyword(keywordTryingData ktd)
        {
            if (ktd.keywordVertex.Get(@"\$StartInLocalRoot:") != null)
                return true;
            else
                return false;
        }

        private int _tryIsNextLocalRootKeyword(ParsingStack s, string LOGPREFIX, int newPos, int sPos, keywordTryingData ktd, bool isSpaceNext)
        {
            if(!isSpaceNext)
                while (text[sPos - 1] == ' ') // spaces handling
                    sPos++; // NO WAY !!!!!!!!!!!!!!!!!!! this will stuck @b in function "A" @b, so thats why I added !isSpaceNext

            //
            List<keywordTryingData> _examinedKeywords = new List<keywordTryingData>();

            string _link;

            int _tryPos = 0;

            // NEW STACK 

            // DO NO NEED THIS NOW !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            ParsingStack newStack = new ParsingStack(s);

            //s = newStack;

            //s.can_initialize_memory_tabCount = true;

            //

            //_tryIsKeyword(s, LOGPREFIX + "    ", sPos - 1, -1, 0, text.Length - 1, text.Length - 1, false, out _examinedKeywords, out _link, true, ref _tryPos, true, null, null, getKewordFilterFromParamName(ktd.currentlyProcessedParameterName));

            _tryIsKeyword(s, LOGPREFIX + "    ", sPos - 1, -1, 0, text.Length - 1, text.Length - 1, false, out _examinedKeywords, out _link, true, ref _tryPos, true, null, null, "", isSpaceNext);

            // BACK TO OLD STACK

            //s = s.parentStack;

            //

            if (_examinedKeywords.Count > 0)
            {
                ktd.LocalRootNext = _examinedKeywords[0];

                if(_examinedKeywords.Count > 1)
                {
                    int x = 0;
                }

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

        IVertex AddKeywordVertex(ParsingStack s, IVertex parent, keywordTryingData ktd)
        {
            return _AddKeywordVertex(s, parent, ktd, ktd.keywordVertex, null, 0, null);
        }

        void AddKeywordVertex_AddVertex(ParsingStack s, IVertex baseVertex, IEdge metaEdge, IVertex meta, object val, ref IVertex nv, keywordTryingData ktd, IEdge parentMetaEdge)
        {
            if (GeneralUtil.CompareStrings("$LocalRoot", metaEdge.Meta.Value)
            || GeneralUtil.CompareStrings("$StartInLocalRoot", metaEdge.Meta.Value)
            || GeneralUtil.CompareStrings("$KeywordGroup", metaEdge.Meta.Value))
                return;

            if (parentMetaEdge!=null
                && parentMetaEdge.To.Get("$LocalRoot:") != null 
                && GeneralUtil.CompareStrings("(?<ANY>)", meta))
            {
                if(val!=null && !GeneralUtil.CompareStrings("",val))
                    baseVertex.Value = val;

                nv = baseVertex;
            }else
                nv = AddVertex(s, baseVertex, meta, val);

            if (metaEdge.To.Get("$LocalRoot:") != null && ktd.LocalRootNext != null)
                _AddKeywordVertex(s, nv, ktd.LocalRootNext, ktd.LocalRootNext.keywordVertex, null, 0, metaEdge);
        }

        IEdge AddKeywordVertex_AddEdge(ParsingStack s, IVertex baseVertex, IEdge edgeForMeta, IVertex meta, IVertex to)
        {
            return AddEdge(s, baseVertex, meta, to);
        }

        IVertex _AddKeywordVertex(ParsingStack s, IVertex parent, keywordTryingData ktd, IVertex keywordAddingVertex, IVertex useMetaWhenANY, int subCount, IEdge parentMetaEdge)
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
                                AddKeywordVertex_AddVertex(s, parent, e, meta, sub, ref nv, ktd, parentMetaEdge); // was marked: ERROR. why?? 

                            if (sub is ToVertexMock)
                                nv = AddKeywordVertex_AddEdge(s, parent, e, meta, (IVertex)sub).To;

                            if (sub is keywordTryingData)
                                nv = _AddKeywordVertex(s, parent, (keywordTryingData)sub, ((keywordTryingData)sub).keywordVertex, meta, 0, null); // cnt_subCount);
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
                                AddKeywordVertex_AddVertex(s, parent, e, meta, sub, ref nv, ktd, parentMetaEdge);

                            if (sub is ToVertexMock)
                                nv = AddKeywordVertex_AddEdge(s, parent, e, meta, (IVertex)sub).To;

                            if (sub is keywordTryingData)
                                nv = _AddKeywordVertex(s, parent, (keywordTryingData)sub, ((keywordTryingData)sub).keywordVertex, meta, 0, null);// cnt_subCount);
                        }
                        else
                            AddKeywordVertex_AddVertex(s, parent, e, meta, e.To, ref nv, ktd, parentMetaEdge);

                      

                        if (s.subTextRanges.ContainsKey(ktd))
                        {
                            TextRange subText = s.subTextRanges[ktd];

                            //if (subText.isNonParameterRange) // do not need this, but who knows
                            //ProcessTextPart(nv, subText.begLine, subText.endLine);
                            //else
                            if (nv != null)
                            {
                                ProcessTextPart(nv, subText.begLine, subText.endLine);

                                s.subTextRanges.Remove(ktd);
                            }
                        }

                        _AddKeywordVertex(s, nv, ktd, e.To, null, cnt_subCount, null);

                    }
                }
            }

            return nv;
        }

        int getDoubleColonPos(string s)
        {
            return s.IndexOf("::");            
        }

        private bool isSpecialKeyword(string keyword)
        {
            if (GeneralUtil.CompareStrings("(?<value>)", keyword)
                    || GeneralUtil.CompareStrings("\"(?<value>)\"", keyword))
                return true;

            return false;
        }

        private void prepareSpecialKeywordsGroups()
        {
            specialKeywordGroups = new List<string>();

            foreach (IEdge e in MinusZero.Instance.newValueKeywordVertex.GetAll("$KeywordGroup:"))
            {
                string group = (string)e.To.Value;

                specialKeywordGroups.Add(group);

                if (!examinedKeywords_All.ContainsKey(group))
                    examinedKeywords_All.Add(group, new List<keywordTryingData>());

                if (!examinedKeywords_LocalRootOnly.ContainsKey(group))
                    examinedKeywords_LocalRootOnly.Add(group, new List<keywordTryingData>());
            }
        }

        private void prepareDictionaries()
        {
            examinedKeywords_All = new Dictionary<string, List<keywordTryingData>>();

            examinedKeywords_All.Add("", new List<keywordTryingData>());

            examinedKeywords_LocalRootOnly = new Dictionary<string, List<keywordTryingData>>();

            examinedKeywords_LocalRootOnly.Add("", new List<keywordTryingData>());


            prepareSpecialKeywordsGroups();


            allKeywordsSubstringsDictionary = new Dictionary<char, List<string>>();

            foreach (IEdge keyword in MinusZero.Instance.Root.GetAll(@"User\CurrentUser:\CodeSettings:\Keyword:\$Keyword:"))
            {
                if(isSpecialKeyword((string)keyword.To.Value))
                    continue;

                // examinedKeywords_All
         
                keywordTryingData ktd = new keywordTryingData(keyword.To, this);

                //if (!isSpecialKeyword(ktd.keyword))
                    examinedKeywords_All[""].Add(ktd);

                foreach(IEdge v in ktd.keywordVertex.GetAll("$KeywordGroup:"))
                {
                    string group = (string)v.To.Value;

                    if (!examinedKeywords_All.ContainsKey(group))
                        examinedKeywords_All.Add(group, new List<keywordTryingData>());

                    examinedKeywords_All[group].Add(ktd);
                }

                // examinedKeywords_LocalRootOnly

                if (keyword.To.Get(@"\$StartInLocalRoot:") != null)
                {
                    keywordTryingData ktd2 = new keywordTryingData(keyword.To, this);

                  //  if (!isSpecialKeyword(ktd.keyword))
                        examinedKeywords_LocalRootOnly[""].Add(ktd2);

                    foreach (IEdge v in ktd.keywordVertex.GetAll("$KeywordGroup:"))
                    {
                        string group = (string)v.To.Value;

                        if (!examinedKeywords_LocalRootOnly.ContainsKey(group))
                            examinedKeywords_LocalRootOnly.Add(group, new List<keywordTryingData>());

                        examinedKeywords_LocalRootOnly[group].Add(ktd);
                    }
                }

                string keywordString = keyword.To.Value.ToString();

                if (keywordString.Length > 0)
                    addKeywordsSubstrings(keywordString);

                // add space to allKeywordsSubstringsDictionary

                if (!allKeywordsSubstringsDictionary.ContainsKey(' '))
                {
                    List<string> l = new List<string>();

                    l.Add(" ");

                    allKeywordsSubstringsDictionary.Add(' ', l);
                }
            }
                
        }

        private void addKeywordsSubstrings(string keywordString)
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
            subString = subString.Trim();

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

        ///

        class LineInfo
        {
            public int tabCount;
            public bool startsWithLineContinuation;
            public int lineBeg;
            public int lineEnd;

            public int lineBeg_NoTrim;
            public int lineEnd_NoTrim;
            public bool isEmpty;

            public int getPosWithParentTabsTrimmed(ParsingStack stack)
            {
                return lineBeg_NoTrim + stack.memory_tabCount;
            }
            public bool IsLineEnd(int pos)
            {
                if (pos == lineEnd + 1 /*|| pos == lineEnd + 2*/
                    || pos == lineEnd_NoTrim + 1 /*|| pos == lineEnd_NoTrim + 2*/)
                    return true;

                return false;
            }
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
                    li.lineBeg_NoTrim = p;
                    li.lineEnd_NoTrim = p;
                    li.tabCount = 0;
                    li.startsWithLineContinuation = false;
                    li.isEmpty = true;

                    next = p;
                }
                else
                {
                    lineEndWithoutTrim = ZeroCodeUtil.getNextCRLF(text, p) - 1;

                    if (lineEndWithoutTrim == -1)
                        return;

                    li.tabCount = 0;

                    li.lineBeg_NoTrim = p;

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
                        li.startsWithLineContinuation = true;

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
                    if(s.lastAddedVertexParent!=null)
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

    
            List<keywordTryingData> examinedKeywords = TryIfIsKeywordLine(s);

            if (examinedKeywords != null)
            {
                if (examinedKeywords.Count > 1)
                {
                    int x = 0; // HOW IS THAT
                }

                keywordTryingData chosenKeyword = examinedKeywords[0];

                return AddKeywordVertex(s, _baseVertex, chosenKeyword);
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

                    return AddEdge(s, _baseVertex, null, processLink(ZeroCodeCommon.stringFromLinkString(afterColon, true))).To;
                }
                else
                {
                    string beforeColon = currentLineInner.Substring(0, doubleColonPos).Trim();

                    string afterColon = currentLineInner.Substring(doubleColonPos + 2, currentLineInner.Length - doubleColonPos - 2).Trim();

                    IVertex meta = processLink(beforeColon);

                    if (afterColon[0] == ZeroCodeCommon.NewVertexPrefix) // if is new value
                        return AddVertex(s, _baseVertex, meta, ZeroCodeCommon.stringFromNewVertexString(afterColon));

                    return AddEdge(s, _baseVertex, meta, processLink(ZeroCodeCommon.stringFromLinkString(afterColon, true))).To;
                }
            }
        }

        IVertex Process_reccurent(ParsingStack s, IVertex _baseVertex)
        {
            IVertex errors = null;

            IVertex prevVertex = ProcessLine(s, _baseVertex);

            while (s.parseNextLine())
            {
                if (s.parseRecurrentReturnNo > 0)
                {
                    s.parseRecurrentReturnNo--;
                    s.skipParse = true;
                    return errors;
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

                    return errors;
                }
            }

            return errors;
        }

        private IVertex ProcessTextPart(IVertex baseVertex, int begLine, int endLine)
        {
            ParsingStack stack = new ParsingStack(this, null, begLine, endLine);

            stack.parseNextLine();

            ////

            foreach (LineInfo i in lineInfoList)
            {
                MinusZero.Instance.Log(-1, "TryIfIsKeywordLine", i.startsWithLineContinuation + " [" + text.Substring(i.lineBeg, i.lineEnd - i.lineBeg + 1)+"]");
            }

            ////


            IVertex errors=Process_reccurent(stack, baseVertex);

            AddNewLines(stack);

            return errors;
        }

        public IVertex ParserAutoTestProcess(IVertex _baseVertex, string _text, int _l1089, int _1149_dict, int _1149_parent, int _l1009_left, int _l1009_right)
        {
            this.l1089 = _l1089;
            this.l1149_dict = _1149_dict;
            this.l1149_parent = _1149_parent;

            this.L1009_left = _l1009_left;
            this.L1009_right = _l1009_right;

            return Process(_baseVertex, _text);
        }

        public IVertex Process(IVertex _baseVertex, string _text)
        {
            baseVertex = _baseVertex;

            text = _text + "\r\n"; // for regexpes

            prepareLineInfoList();

            prepareImportList();

            IVertex parseRoot = _baseVertex.AddVertex(null, "ParseRoot");

            ProcessTextPart(parseRoot, 0, lineInfoList.Count - 1);


            return null;
        }

        public String2ZeroCodeGraphProcessing()
        {
            setupHelpVariables();

            prepareDictionaries();

            emptyKeywordVertex = MinusZero.Instance.Root.Get(@"User\CurrentUser:\CodeSettings:\Keyword:\$Keyword:(?<value>)");

            newValueKeywordVertex = MinusZero.Instance.newValueKeywordVertex;

        }
    }
}