using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace m0.ZeroCode
{
    public class Text2GraphProcessing 
    {
        public class TextRange
        {
            public int begLine;
            public int endLine;
            public bool isNonParameterRange;
        }

        public class KeywordInfo
        {
            public bool HasLocalRoot;
            public string LocalRootKeywordsGroup;
            public bool NonSelfRecursiveParameters;
            public bool hasCRLF;
        }

        public class ParsingStack
        {
            public Dictionary<IVertex, int> sameStartPosKewords = new Dictionary<IVertex, int>();

            //

            Text2GraphProcessing processing;

            public ParsingStack parentStack;

            public int begLine;
            public int endLine;

            public int lineNo;
            public LineInfo currentLineInfo;

            public string currentLineNoTabs;
            public zstring zcurrentLineNoTabs;

            public IVertex LocalRoot;

            public IVertex lastAddedVertex;
            public IVertex lastAddedVertexParent;            

            public int newLineCount;

            public bool skipParse = false;
            public int parseRecurrentReturnNo = -1;

            public int memory_tabCount = -1;
            public bool can_initialize_memory_tabCount = true;

            public Dictionary<object, TextRange> subTextRanges;            


            public ParsingStack(Text2GraphProcessing _parent, ParsingStack _parentStack, int _begLine, int _endLine)
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
                zcurrentLineNoTabs = new zstring(currentLineNoTabs);

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

            public override int GetHashCode()
            {
                return sameStartPosKewords.GetHashCode()
                + processing.GetHashCode()
                + parentStack.GetHashCode()
                + begLine.GetHashCode()
                + endLine.GetHashCode()
                + lineNo.GetHashCode()
                + currentLineInfo.GetHashCode()
                + currentLineNoTabs.GetHashCode()
                + LocalRoot.GetHashCode()
                + lastAddedVertex.GetHashCode()
                + lastAddedVertexParent.GetHashCode()
                + newLineCount.GetHashCode()
                + skipParse.GetHashCode()
                + parseRecurrentReturnNo.GetHashCode()
                + memory_tabCount.GetHashCode()
                + can_initialize_memory_tabCount.GetHashCode()
                + subTextRanges.GetHashCode();
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
                zcurrentLineNoTabs = copyFrom.zcurrentLineNoTabs;

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
                int currentLine = lineNo - 1;

                while (true)
                {
                    if (currentLine == -1)
                        return 0;

                    if (!processing.lineInfoList[currentLine].isEmpty)
                        return processing.lineInfoList[currentLine].tabCount; // to be corrected // why?

                    currentLine--;
                }
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

            public int getNextLineWithLessTabCountIfNextLineHasMoreTabCount()
            {
                List<LineInfo> li = processing.lineInfoList;

                if (lineNo >= li.Count - 1)
                    return -1;

                if (li[lineNo + 1].tabCount <= currentLineInfo.tabCount)
                    return -1;

                int iterationLineNo = lineNo + 1;

                while (iterationLineNo < li.Count
                    && li[iterationLineNo].tabCount > li[lineNo].tabCount)
                    iterationLineNo++;

                if (iterationLineNo == li.Count)
                    return -1;

                return iterationLineNo;
            }

            HashSet<LineInfo> lineInfosThatResultedNewLine = new HashSet<LineInfo>();

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
                {
                    currentLineNoTabs = "";
                    zcurrentLineNoTabs = new zstring("");
                }
                else
                {
                    currentLineNoTabs = processing.text.Substring(currentLineInfo.lineBeg, currentLineInfo.lineEnd - currentLineInfo.lineBeg + 1);
                    zcurrentLineNoTabs = new zstring(currentLineNoTabs);
                }

                // and now check if there are only whitespaces

                //if (ZeroCodeUtil.IsStringOnlyWhiteSpaces(currentLineNoTabs))
                if (currentLineInfo.isEmpty)
                {
                    if (!lineInfosThatResultedNewLine.Contains(currentLineInfo))
                    {
                        newLineCount++;
                        lineInfosThatResultedNewLine.Add(currentLineInfo);
                    }

                    return parseNextLine();
                }

                return true;
            }

            public bool goToLine(int newLineNo)
            {
                if (newLineNo > endLine)
                    return false;
                
                currentLineInfo = processing.lineInfoList[newLineNo];
                zcurrentLineNoTabs = new zstring(currentLineNoTabs);

                lineNo = newLineNo;

                if (currentLineInfo.isEmpty)
                {
                    currentLineNoTabs = "";
                    zcurrentLineNoTabs = new zstring("");
                }
                else
                {
                    currentLineNoTabs = processing.text.Substring(currentLineInfo.lineBeg, currentLineInfo.lineEnd - currentLineInfo.lineBeg + 1);
                    zcurrentLineNoTabs = new zstring(currentLineNoTabs);
                }

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



        FormalTextLanguageDictinaries dict;        

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

        IVertex FormalTextLanguage;

        IVertex lastAddedVertex;

        //

        IVertex errorList;

        // PROCESS dependent

        public IVertex baseVertex;

        string text;
        zstring ztext;

        public List<LineInfo> lineInfoList;

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

            importList.AddExternalReference();
            importMetaList.AddExternalReference();
            importDirectList.AddExternalReference();
            importDirectMetaList.AddExternalReference();

            prepareImportList_FromString();
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
            //Regex rgx = new Regex("import[ ]+\"(?<name>.*)\"[ ]+@(?<link>.*[^ ])[ ]*\\r");
            //Regex rgx = new Regex("import[]+\"(?<name>.*)\"[ ]+@(?<link>[^ ]+)[ ]*\\r");
            Regex rgx = new Regex(dict.Import.regexpString);

            foreach (Match match in rgx.Matches(text))
            {
                string name = match.Groups["name"].Value;

                string link = match.Groups["link"].Value;

                IVertex namev = MinusZero.Instance.CreateTempVertex();

                namev.Value = name;

                IVertex target=r.Get(false, link); // YYY huston..... we assume that this get will not go into infinite reccursion as link variable is simple run time query

                if (target != null)
                    importList.AddEdge(namev, target);
                else
                    importList.AddEdge(namev, MinusZero.Instance.Empty);
            }
        }

        void prepareImportList_FromString_importMeta()
        {
            //Regex rgx = new Regex("import[ ]+meta[ ]+\"(?<name>.*)\"[ ]+@(?<link>.*[^ ])[ ]*\\r");
            
            Regex rgx = new Regex(dict.ImportMeta.regexpString);

            foreach (Match match in rgx.Matches(text))
            {
                string name = match.Groups["name"].Value;

                string link = match.Groups["link"].Value;


                IVertex namev = MinusZero.Instance.CreateTempVertex();

                namev.Value = name;


                IVertex target = r.Get(false, link); // YYY huston..... we assume that this get will not go into infinite reccursion as link variable is simple run time query

                if (target != null)
                    importMetaList.AddEdge(namev, target);
                else
                    importMetaList.AddEdge(namev, MinusZero.Instance.Empty);
            }
        }

        static IVertex smb;

        static IVertex Direct_meta;

        static IVertex DirectMeta_meta;

        static IVertex NewLine_meta;

        void setupHelpVariables_onlyOnce()
        {
            if (smb == null)
            {
                IVertex System = GraphUtil.GetQueryOutFirst(MinusZero.Instance.Root, null, "System");

                IVertex Meta = GraphUtil.GetQueryOutFirst(System, null, "Meta");

                smb = GraphUtil.GetQueryOutFirst(Meta, null, "Base");                

                Direct_meta = GraphUtil.GetQueryOutFirst(smb, null, "$ImportDirect");

                DirectMeta_meta = GraphUtil.GetQueryOutFirst(smb, null, "$ImportDirectMeta");

                NewLine_meta = GraphUtil.GetQueryOutFirst(smb, null, "$NewLine");
            }
        }

        void prepareImportList_FromString_importDirect()
        {
            //Regex rgx = new Regex("import[ ]+direct[ ]+@(?<link>.*[^ ])[ ]*\\r");

            Regex rgx = new Regex(dict.ImportDirect.regexpString);

            foreach (Match match in rgx.Matches(text))
            {
                string link = match.Groups["link"].Value;

                IVertex target = r.Get(false, link); // YYY huston..... we assume that this get will not go into infinite reccursion as link variable is simple run time query

                if (target != null)
                    importDirectList.AddEdge(Direct_meta, target);
                else
                    importDirectList.AddEdge(Direct_meta, MinusZero.Instance.Empty);
            }
        }

        void prepareImportList_FromString_importDirectMeta()
        {
            //Regex rgx = new Regex("import[ ]+direct[ ]+meta[ ]+@(?<link>.*[^ ])[ ]*\\r");

            Regex rgx = new Regex(dict.ImportDirectMeta.regexpString);

            foreach (Match match in rgx.Matches(text))
            {
                string link = match.Groups["link"].Value;

                IVertex target = r.Get(false, link); // YYY huston..... we assume that this get will not go into infinite reccursion as link variable is simple run time query

                if (target != null)
                    importDirectMetaList.AddEdge(DirectMeta_meta, target);
                else
                    importDirectMetaList.AddEdge(DirectMeta_meta, MinusZero.Instance.Empty);
            }
        }

        IVertex query(IVertex baseVertex, string query)
        {            
            return baseVertex.Get(true, query); // YYY huston..... we assume that this get will not go into infinite reccursion as query variable is simple run time query
        }

        IVertex queryMetaImport(IVertex baseVertex, string query)
        {
            return baseVertex.Get(false, query); // YYY huston..... we assume that this get will not go into infinite reccursion as query variable is simple run time query            
        }   

        IVertex ToVertexMock2VertexByLinkString(ToVertexMock mock)
        {
            string link = mock.mockData.ToString();

            // try named link

            string secondPart;

            string firstPart;

            ZeroCodeUtil.GetQueryFirstAndSecondPart(dict, link, out firstPart, out secondPart);

            IVertex tryIf;

            // named link
          
            if (secondPart != null)
            {
                // normal importList
                IVertex importLink = importList.Get(false, firstPart + ":"); // YYY huston fisttPart variable to be simple run time query

                if (importLink != null)
                {
                    tryIf = query(importLink, secondPart);

                    if (tryIf != null)
                        return tryIf;
                }

                // dict importList
                importLink = dict.importList.Get(false, firstPart + ":"); // YYY huston fisttPart variable to be simple run time query

                if (importLink != null)
                {
                    tryIf = query(importLink, secondPart);

                    if (tryIf != null)
                        return tryIf;
                }

                // normal importMetaList
                IVertex importMetaLink = importMetaList.Get(false, firstPart + ":"); // YYY huston fisttPart variable to be simple run time query

                if (importMetaLink != null)
                {
                    tryIf = queryMetaImport(importMetaLink, secondPart);

                    if (tryIf != null)
                        return tryIf;
                }

                // dict importMetaList
                importMetaLink = dict.importMetaList.Get(false, firstPart + ":"); // YYY huston fisttPart variable to be simple run time query

                if (importMetaLink != null)
                {
                    tryIf = queryMetaImport(importMetaLink, secondPart);

                    if (tryIf != null)
                        return tryIf;
                }
            }

            // try from local root

            tryIf = queryMetaImport(baseVertex, @"$ParseRoot" + dict.MetaSeparator + @"\\" + link);

            if (tryIf != null && !(tryIf is ToVertexMock))
                return tryIf;

            // normal direct link

            tryIf = query(importDirectList, @"\" + link);

            if (tryIf != null)
                return tryIf;

            // dict direct link

            tryIf = query(dict.importDirectList, @"\" + link);

            if (tryIf != null)
                return tryIf;

            // normal direct link meta

            tryIf = queryMetaImport(importDirectMetaList, @"\" + link);

            if (tryIf != null)
                return tryIf;

            // dict direct link meta

            tryIf = queryMetaImport(dict.importDirectMetaList, @"\" + link);
            
            if (tryIf != null)
                return tryIf;            

            if (mock.parentVertex != null)
            {
                foreach (IEdge inEdge in mock.parentVertex.InEdges)
                {
                    if (GeneralUtil.CompareStrings(inEdge.Meta.Value, link))
                        return inEdge.Meta;

                    IVertex found = inEdge.Meta.Get(false, link); // ??? for sure XXX I do not know why it works, but it should be there. perhaps
                                                                  // YYY huston fisttPart variable to be simple run time query


                    if (found != null)
                        return found;                    
                }
            }

            return MinusZero.Instance.Empty;
        }

        IVertex processLink(string link, IVertex parent)
        {
            return new ToVertexMock(link, parent);            
        }

        public enum keywordTryingState { keywordCharacter, parameter, waiting, matched}

        public class keywordTryingData
        {
            public Text2GraphProcessing parent;
            public IVertex keywordVertex;
            public String keyword;
            public zstring zkeyword;

            public keywordTryingState state;
            public int currentPositionInKeyword;
            
            public int waitingUntilPositionInText;
            public int matchedOnPositionInText;

            public string currentlyProcessedParameterName;
            public string afterParameterString;
            public zstring zafterParameterString;

            public int multiParameterCount = 0;

            public Dictionary<string, List<object>> parameters = new Dictionary<string, List<object>>();

            public keywordTryingData LocalRootNext;

            public object lastAddedParameter;
            public bool isCurrentlyProcessedSubParameter;            

            //

            string multiParameterSeparator;
            zstring zmultiParameterSeparator;
            string multiParameterString;
            zstring zmultiParameterString;
            string multiParamPlusSeparatorString;
            string multiParameterAfterParamBeforeSeparator;
            string multiParameterAfterSeparatorString;
            zstring zmultiParameterAfterSeparatorString;

            int currentPositionInMultiParamPlusSeparatorString = -1;

            int multiParameterStringBegPosition = -1;
            int multiParameterStringEndPosition = -1;

            bool lastCharWasSkippedSpace; // space support            

            public keywordTryingData(keywordTryingData source, Text2GraphProcessing _parent)
            {                
                parent = _parent;
                keywordVertex = source.keywordVertex;
                keyword = source.keyword;
                zkeyword = source.zkeyword;
                currentPositionInKeyword = source.currentPositionInKeyword;
                state = source.state;
                currentlyProcessedParameterName = source.currentlyProcessedParameterName;
                afterParameterString = source.afterParameterString;
                zafterParameterString = new zstring(afterParameterString);
            }

            static Dictionary<string, string> keywordNoSpacesDict = new Dictionary<string, string>();

            public string removeSpaces(string s)
            {
                if (keywordNoSpacesDict.ContainsKey(s))
                    return keywordNoSpacesDict[s];
                else
                {
                    string noSpaces = "";

                    List<string> l = ZeroCodeUtil.TokenizeKeyword(s, true);

                    foreach (string t in l)
                    {
                       // MinusZero.Instance.Log(-1, "T", t);
                        if (!ZeroCodeUtil.IsStringOnlyWhiteSpaces(t))
                            noSpaces += t.Trim(new Char[]{' '});
                        else
                            noSpaces += t;
                    }

                    keywordNoSpacesDict.Add(s, noSpaces);

                    return noSpaces;
                }
            }

            public keywordTryingData(IVertex k, Text2GraphProcessing _processing)
            {
                parent = _processing;
                keywordVertex = k;
                
                //keyword = (String)keywordVertex.Value;

                keyword = removeSpaces((String)keywordVertex.Value);

                zkeyword = new zstring(keyword);

               // MinusZero.Instance.Log(-1, "KTD", keyword);

                currentPositionInKeyword = 0;
                state = keywordTryingState.keywordCharacter; // that and rest of the fields will be updated in the _tryKeyword
            }

            public int getMatchedOnPositionInText_Reccurent()
            {
                if (LocalRootNext == null)
                    return this.matchedOnPositionInText;

                return LocalRootNext.getMatchedOnPositionInText_Reccurent();
            }

            static zstring bracket_star = new zstring("(*");
            static zstring bracket_plus = new zstring("(+");
            static zstring plus_bracket = new zstring("+)");
            static zstring star_bracket = new zstring("*)");
            static zstring anglebracket_bracket = new zstring(">)");

            public bool currentPositionInKeyword_isParameterMatch(ParsingStack s, int curSpos)
            {
                 if(ZeroCodeUtil.TryStringMatch(zkeyword, currentPositionInKeyword, bracket_star) &&!isInMultiParameter())
                  {
                      if(ZeroCodeUtil.TryStringMatch(zkeyword, currentPositionInKeyword+2, bracket_plus))
                      {
                        int multiParameterSeparatorEndPos = ZeroCodeUtil.GetNextMatch(zkeyword, currentPositionInKeyword + 4, plus_bracket);

                        multiParameterSeparator = keyword.Substring(currentPositionInKeyword + 4, multiParameterSeparatorEndPos - currentPositionInKeyword - 4);
                        zmultiParameterSeparator = new zstring(multiParameterSeparator);

                        multiParameterStringBegPosition = multiParameterSeparatorEndPos + 2;

                        multiParameterStringEndPosition = ZeroCodeUtil.GetNextMatch(zkeyword, multiParameterSeparatorEndPos, star_bracket) + 1;

                        multiParameterString = keyword.Substring(multiParameterStringBegPosition, multiParameterStringEndPosition - multiParameterStringBegPosition - 1);
                        zmultiParameterString = new zstring(multiParameterString);

                        multiParamPlusSeparatorString = multiParameterString + multiParameterSeparator;
                    }
                    else
                    {
                        multiParameterSeparator = "";
                        zmultiParameterSeparator = new zstring(multiParameterSeparator);

                        multiParameterStringBegPosition = currentPositionInKeyword + 2;

                        multiParameterStringEndPosition = ZeroCodeUtil.GetNextMatch(zkeyword, multiParameterStringBegPosition, star_bracket) + 1;

                        multiParameterString = keyword.Substring(multiParameterStringBegPosition, multiParameterStringEndPosition - multiParameterStringBegPosition - 1);
                        zmultiParameterString = new zstring(multiParameterString);

                        multiParamPlusSeparatorString = multiParameterString;
                    }

                    int multiParameterAfterParamBeg = ZeroCodeUtil.GetNextMatch(zmultiParameterString, 4, anglebracket_bracket) + 2;

                    multiParameterAfterParamBeforeSeparator = multiParameterString.Substring(multiParameterAfterParamBeg);

                    multiParameterAfterSeparatorString = ZeroCodeUtil.GetNextCharacterPartFromKeyword_startingFromNonParameter(
                        zkeyword, multiParameterStringEndPosition + 1);
                    zmultiParameterAfterSeparatorString = new zstring(multiParameterAfterSeparatorString);

                    if (ZeroCodeUtil.TabRemove_tryStringMatch(parent.ztext, curSpos, zkeyword.Substring(multiParameterStringEndPosition + 1), s.currentLineInfo.tabCount))
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
                    if (ZeroCodeUtil.TryStringMatch(multiParamPlusSeparatorString, currentPositionInMultiParamPlusSeparatorString, "(?<"))
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

                if (ZeroCodeUtil.TryStringMatch(keyword, currentPositionInKeyword, "(?<"))
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
                //MinusZero.Instance.Log(1, "isCharacterMatch", curPos + " ? "+ keyword[currentPositionInKeyword] + " | curPositionInKeyword:"+currentPositionInKeyword);

                lastCharWasSkippedSpace = false;

                if (isInMultiParameter())
                {
                    if ((currentPositionInMultiParamPlusSeparatorString == multiParameterString.Length // after multi param string
                       || (multiParameterCount > 1 && currentPositionInMultiParamPlusSeparatorString == 0)) // after multi param and no separator
                       // && keyword[multiParameterStringEndPosition + 1] == v) // we are going out of multi
                       && ZeroCodeUtil.TabRemove_tryStringMatch(parent.ztext,curPos,zkeyword.Substring(multiParameterStringEndPosition + 1), s.currentLineInfo.tabCount)) // this is the way
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
                zstring zstr;

                if (isInMultiParameter())
                {
                    //MinusZero.Instance.Log(1, "GetParameter", "MULTI: currentPositionInMultiParamPlusSeparatorString:"+ currentPositionInMultiParamPlusSeparatorString);
                    currentPosition = currentPositionInMultiParamPlusSeparatorString;
                    str = multiParamPlusSeparatorString;
                    zstr = new zstring(str);
                }
                else
                {
                    //MinusZero.Instance.Log(1, "GetParameter", "NORMAL");
                    currentPosition = currentPositionInKeyword;
                    str = keyword;
                    zstr = new zstring(str);
                }

                int begCurrentPosition = currentPosition;

                currentPosition = ZeroCodeUtil.GetNextMatch(zstr, currentPosition + 2, anglebracket_bracket) + 2;
                currentlyProcessedParameterName = str.Substring(begCurrentPosition + 3, currentPosition - begCurrentPosition - 5);

                if (isInMultiParameter())
                {
                    if (currentPosition == multiParameterString.Length)
                    {
                        multiParameterAfterSeparatorString = ZeroCodeUtil.GetNextCharacterPartFromKeyword_startingFromNonParameter(
                            zkeyword, multiParameterStringEndPosition + 1);
                        zmultiParameterAfterSeparatorString = new zstring(multiParameterAfterSeparatorString);

                        int whatMatch = ZeroCodeUtil.GetNextMatch_twoAtOnce(parent.ztext, curSpos,
                            zmultiParameterSeparator,
                            zmultiParameterAfterSeparatorString);

                        if (whatMatch == 0)
                        {
                            afterParameterString = "";
                            zafterParameterString = new zstring(afterParameterString);
                        }

                        if (whatMatch == 1)
                        {
                            afterParameterString = multiParameterSeparator;
                            zafterParameterString = new zstring(afterParameterString);
                        }

                        if (whatMatch == 2)
                        {
                            afterParameterString = multiParameterAfterSeparatorString;
                            zafterParameterString = new zstring(afterParameterString);
                        }
                    }
                    else
                    {
                        int whatMatch = ZeroCodeUtil.GetNextMatch_twoAtOnce(parent.ztext, curSpos,
                            new zstring(multiParameterAfterParamBeforeSeparator + multiParameterSeparator),
                            new zstring(multiParameterAfterParamBeforeSeparator + zmultiParameterAfterSeparatorString));

                        if (whatMatch == 0)
                            afterParameterString = "";

                        if (whatMatch == 1)
                            afterParameterString = multiParameterAfterParamBeforeSeparator + multiParameterSeparator;

                        if (whatMatch == 2)
                            afterParameterString = multiParameterAfterParamBeforeSeparator + multiParameterAfterSeparatorString;
                    }
                }
                else
                {
                    afterParameterString = ZeroCodeUtil.GetNextCharacterPartFromKeyword_startingFromNonParameter(zstr, currentPosition);
                    zafterParameterString = new zstring(afterParameterString);
                }

                //MinusZero.Instance.Log(1, "GetParameter", "currentlyProcessedParameterName:"+ currentlyProcessedParameterName+" curPosition:" + currentPosition+ " afterParameterString:"+ afterParameterString);

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
                if (ZeroCodeUtil.TryStringMatch(keyword, currentPositionInKeyword + 1, "(?<SUB>)") && parent.text[curSpos] != ' ')
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

                        //MinusZero.Instance.Log(1, "currentPositionInKeyword_Increase", "MULTI:" + currentPositionInMultiParamPlusSeparatorString);
                    }
                else
                {
                    if (!lastCharWasSkippedSpace)
                        currentPositionInKeyword++;
                    //MinusZero.Instance.Log(1, "currentPositionInKeyword_Increase", "NORMAL:" + currentPositionInKeyword);
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
                keywordTryingData _ktd = new keywordTryingData(ktd, this);
                
                target.Add(_ktd);
            }              
        }

        List<keywordTryingData> TryIfIsKeywordLine(ParsingStack s)
        {     
            List <keywordTryingData> examinedKeywords;

            if (s.currentLineInfo.lineBeg >= text.Length)
                return null;

            if (!ZeroCodeUtil.TryStringMatch(s.zcurrentLineNoTabs, 0, dict.zCodeGraphVertexPrefix) 
                || !ZeroCodeUtil.TryStringEndMatch(s.zcurrentLineNoTabs, dict.zCodeGraphVertexSuffix))
            {
                string link;

                int tryPos = 0;

                _tryIsKeyword(s, "", s.currentLineInfo.lineBeg, s.currentLineInfo.lineBeg, 0, text.Length - 1, text.Length - 1, false, false, out examinedKeywords, out link, true, ref tryPos, false, null, null, "", false);

                if (examinedKeywords.Count() > 0)
                    return examinedKeywords;                
                return null;
            }
            else
                return null;
        }

        enum SpecialKeywordType {EmptyKeyword, NewVertexKeyword, LinkKeyword}

        IList<keywordTryingData> createSpecialKeyword(ParsingStack s, object value, int matchedOnPositionInText, SpecialKeywordType type, IList<IVertex> possible_newVertexKeyword, IList<IVertex> possible_emptyKeyword, IList<IVertex> possible_linkKeyword)
        {
            List<keywordTryingData> ktdListToReturn = new List<keywordTryingData>();

            IList<IVertex> toUseVertexList=null;

            switch (type)
            {
                case SpecialKeywordType.EmptyKeyword:                    
                     toUseVertexList = possible_emptyKeyword;                     
                    break;

                case SpecialKeywordType.NewVertexKeyword:                    
                    toUseVertexList = possible_newVertexKeyword;                     
                    break;

                case SpecialKeywordType.LinkKeyword:
                    toUseVertexList = possible_linkKeyword;
                    value = new ToVertexMock(ZeroCodeCommon.stringFromLinkKeywordString(dict, value.ToString()),null);
                    break;
            }

            if (toUseVertexList == null)
                return ktdListToReturn;

            foreach(IVertex keywordVertex in toUseVertexList)
            {
                keywordTryingData ktd = new keywordTryingData(keywordVertex, this);

                ktd.matchedOnPositionInText = matchedOnPositionInText;

                List<object> l = new List<object>();
                l.Add(value);

                ktd.parameters.Add("value", l);

                ktdListToReturn.Add(ktd);
            }
            
            return ktdListToReturn;
        }

        class ParameterChache
        {
            public object Parameter = null;
            public int waitingUntilPositionInText = 0;
        }
       
        class tryIsKeyword_Parameters_IN
        {
            public ParsingStack s;
            public string LOGPREFIX;
            public int startPos;
            public int prev_startPos;
            public int isPrevStartPosSameAsStartPosParentCount;
            public int endPos;
            public int endPos_forAtomParts;
            public bool canStopByForAtomParts;
            public bool afterKeywordPartExist;
            public bool isTopLevelCall;            
            public bool lookForLocalRootOnly;
            public IVertex parentKeyword;
            public tryIsKeyword_Parameters_IN parentParams;
            public string keywordsFilter;
            public bool isSpaceNext;

            public tryIsKeyword_Parameters_IN(
                ParsingStack s,
                string _LOGPREFIX,
                int _startPos,
                int _prev_startPos,
                int _isPrevStartPosSameAsStartPosParentCount,
                int _endPos,
                int _endPos_forAtomParts,
                bool _canStopByForAtomParts,
                bool _afterKeywordPartExist,
                List<keywordTryingData> _examinedKeywords,
                string _link,
                bool _isTopLevelCall,
                int _newPos,
                bool _lookForLocalRootOnly,
                IVertex _parentKeyword,
                tryIsKeyword_Parameters_IN _parentParams,
                string _keywordsFilter,
                bool _isSpaceNext
                )
            {
                LOGPREFIX = _LOGPREFIX;
                startPos = _startPos;
                prev_startPos = _prev_startPos;
                isPrevStartPosSameAsStartPosParentCount = _isPrevStartPosSameAsStartPosParentCount;
                endPos = _endPos;
                endPos_forAtomParts = _endPos_forAtomParts;
                canStopByForAtomParts = _canStopByForAtomParts;
                afterKeywordPartExist = _afterKeywordPartExist;
                isTopLevelCall = _isTopLevelCall;
                lookForLocalRootOnly = _lookForLocalRootOnly;
                parentKeyword = _parentKeyword;
                parentParams = _parentParams;
                keywordsFilter = _keywordsFilter;
                isSpaceNext = _isSpaceNext;
            }

            public override int GetHashCode()
            {
                int result = 37;

                result *= 397;
                result += startPos;

                result *= 397;
                result += prev_startPos;

                result *= 397;
                result += isPrevStartPosSameAsStartPosParentCount;

                result *= 397;
                result += endPos;

                result *= 397;
                result += endPos_forAtomParts;

                result *= 397;
                result += canStopByForAtomParts.GetHashCode();

                result *= 397;
                result += afterKeywordPartExist.GetHashCode();

                result *= 397;
                result += isTopLevelCall.GetHashCode();

                result *= 397;
                result += lookForLocalRootOnly.GetHashCode();

                result *= 397;
                result += GeneralUtil.GetHashCode(parentKeyword);

               // result *= 397;
               // result += GeneralUtil.GetHashCode(parentParams);

                result *= 397;
                result += keywordsFilter.GetHashCode();

                result *= 397;
                result += isSpaceNext.GetHashCode();

                return result;
                
            }    
        }

        class tryIsKeyword_Parameters_OUT
        {
            public List<keywordTryingData> examinedKeywords;
            public string link;
            public int newPos;

            public tryIsKeyword_Parameters_OUT(
                List<keywordTryingData> _examinedKeywords,
                string _link,
                int _newPos
                )
            {                
                examinedKeywords = _examinedKeywords;
                link = _link;                
                newPos = _newPos;                
            }
        }

        Dictionary<int, tryIsKeyword_Parameters_OUT> params_IN_OUT_dictionary = new Dictionary<int, tryIsKeyword_Parameters_OUT> ();


        void _tryIsKeyword(ParsingStack s, 
            string LOGPREFIX, 
            int startPos, 
            int prev_startPos, 
            int isPrevStartPosSameAsStartPosParentCount, 
            int endPos, 
            int endPos_forAtomParts, 
            bool canStopByForAtomParts, 
            bool afterKeywordPartExist, 
            out List<keywordTryingData> examinedKeywords, 
            out string link, 
            bool isTopLevelCall, 
            ref int newPos, 
            bool lookForLocalRootOnly, 
            IVertex parentKeyword, 
            tryIsKeyword_Parameters_IN parentParams, 
            string keywordsFilter, 
            bool isSpaceNext)
        {
            //ZeroCodeCommon.testIfIsKeywordSubstring(0, "<<", dict.allKeywordsSubstringsPositiveDictionary_witchoutLinkKeywordParts, dict.allKeywordsSubstringsNegativeDictionary_witchoutLinkKeywordParts);
            tryIsKeyword_Parameters_IN parameters_IN = new tryIsKeyword_Parameters_IN(
                s,
                LOGPREFIX,
                startPos,
                prev_startPos,
                isPrevStartPosSameAsStartPosParentCount,
                endPos,
                endPos_forAtomParts,
                canStopByForAtomParts,
                afterKeywordPartExist,
                null,
                null,
                isTopLevelCall,
                newPos,
                lookForLocalRootOnly,
                parentKeyword,
                parentParams,
                keywordsFilter,
                isSpaceNext);

            tryIsKeyword_Parameters_OUT parameters_OUT;

            //MinusZero.Instance.Log(0, "_tryIfKeyword", LOGPREFIX + "RUN "+callParams.ToString());

            // PARAMS OPTIMISATION 
            if (params_IN_OUT_dictionary.ContainsKey(parameters_IN.GetHashCode()))
            {
                parameters_OUT = params_IN_OUT_dictionary[parameters_IN.GetHashCode()];

                examinedKeywords = parameters_OUT.examinedKeywords;
                link = parameters_OUT.link;
                newPos = parameters_OUT.newPos;

                return;
            }

            //

            examinedKeywords = new List<keywordTryingData>();

            link = null;

            //

            if (s.currentLineInfo.IsLineEnd(startPos))
            { // the + 2 might be not needed, but who knows....
              //if (text[startPos] == '\r' || text[startPos] == '\n')
                newPos = s.currentLineInfo.lineEnd_NoTrim + 1;

                parameters_OUT = new tryIsKeyword_Parameters_OUT(examinedKeywords, link, newPos);

                params_IN_OUT_dictionary.Add(parameters_IN.GetHashCode(), parameters_OUT);

                return;
            }

            if (startPos == endPos_forAtomParts)
            {
                parameters_OUT = new tryIsKeyword_Parameters_OUT(examinedKeywords, link, newPos);

                params_IN_OUT_dictionary.Add(parameters_IN.GetHashCode(), parameters_OUT);

                return;
            }

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

            //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"BEG startPos:" + startPos + " prevSpos:"+prev_startPos+" same:"+isPrevStartPosSameAsStartPos+" endPos:" + endPos);


            int sPos = startPos;

            if (sPos == endPos)
            {
                parameters_OUT = new tryIsKeyword_Parameters_OUT(examinedKeywords, link, newPos);

                params_IN_OUT_dictionary.Add(parameters_IN.GetHashCode(), parameters_OUT);

                return;
            }

            
            bool shallProceed = true;

            int tryNewPos = 0;
            string tryLink = null;
            string trySpecialKeyword = null;
            //ParsingStack tryEmptyKeywordStack=null;
            int tryEmptyKeywordStack_LineNoMemory=-1;

            //

            SpecialKeywordType specialType = SpecialKeywordType.NewVertexKeyword; // got to intialize

            IList<IVertex> possible_emptyKeyworsByKeywordsFilter = new List<IVertex>(); 
            IList<IVertex> possible_newVertexKeywordsByKeywordsFilter = new List<IVertex>();
            IList<IVertex> possible_linkKeywordsByKeywordsFilter = new List<IVertex>();

            //

            int sPos_copy;

            bool _isLink = ZeroCodeCommon.isLinkString(dict, text, sPos);


            if (!ZeroCodeCommon.testIfIsKeywordSubstring(sPos, text, dict.allKeywordsSubstringsDictionary, null) || _isLink)
            {
                if (!isTopLevelCall && _isLink) // @
                {
                    ZeroCodeCommon.tryStringFromLinkString(dict, text, sPos, ref sPos, endPos_forAtomParts, dict.allKeywordsSubstringsPositiveDictionary_witchoutLinkKeywordParts);

                    string foundString = text.Substring(startPos, sPos - startPos);

                    tryLink = ZeroCodeCommon.stringFromLinkString(dict, foundString, false);

                    sPos_copy = sPos;
                }
                else
                {
                    trySpecialKeyword = ZeroCodeCommon.tryStringFromNewVertexString(dict, text, startPos, ref sPos);                    

                    if (trySpecialKeyword != null)
                    {
                        specialType = SpecialKeywordType.NewVertexKeyword;

                        sPos_copy = sPos;

                        sPos++; // hmmm ????
                    }
                    else
                    {
                        string foundString;
                        bool isLinkKeyword = false;

                        foundString = ZeroCodeCommon.tryEscapedLinkStringAndDeescape(dict, text, ref sPos);

                        if (foundString != null)
                        {
                            sPos_copy = sPos;

                        }else
                        {
                            isLinkKeyword = ZeroCodeCommon.isLinkKeywordString(dict, text, sPos);

                            bool isInEscape = false;

                            while (shallProceed)
                            {
                                sPos++;

                                if (isLinkKeyword)
                                {
                                    if (isInEscape == false && text[sPos] == dict.EscapedSequencePrefix)
                                    {
                                        isInEscape = true;
                                    }
                                    else if (isInEscape && text[sPos] == dict.EscapedSequenceSuffix)
                                        isInEscape = false;
                                }                                    

                                if (!isLinkKeyword && ZeroCodeCommon.testIfIsKeywordSubstring(sPos, text, dict.allKeywordsSubstringsDictionary_witchoutAlpha, null))
                                    shallProceed = false;

                                if (isLinkKeyword && !isInEscape && ZeroCodeCommon.testIfIsKeywordSubstring(sPos, text, dict.allKeywordsSubstringsPositiveDictionary_witchoutLinkKeywordParts, dict.allKeywordsSubstringsNegativeDictionary_witchoutLinkKeywordParts))
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

                            foundString = text.Substring(startPos, sPos - startPos);

                        }
                    
                        trySpecialKeyword = foundString;

                        if(isLinkKeyword)
                            specialType = SpecialKeywordType.LinkKeyword;
                        else
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

                // !!!!!!!!!!!!!!!!!!!!!!! A or B ! YOU DECIDE. I do not know :)


                if (dict.emptyKeywordByGroups.ContainsKey(keywordsFilter))
                    possible_emptyKeyworsByKeywordsFilter = dict.emptyKeywordByGroups[keywordsFilter];                

                if (dict.newVertexKeywordByGroups.ContainsKey(keywordsFilter))
                    possible_newVertexKeywordsByKeywordsFilter = dict.newVertexKeywordByGroups[keywordsFilter];

                if (dict.linkKeywordByGroupsDictionary.ContainsKey(keywordsFilter))
                    possible_linkKeywordsByKeywordsFilter = dict.linkKeywordByGroupsDictionary[keywordsFilter];

                if (c1089 && ((possible_emptyKeyworsByKeywordsFilter!=null || possible_newVertexKeywordsByKeywordsFilter!=null) && keywordsFilter!="")
                    //_specialKeywordGroups_empty.Contains(keywordsFilter) // A
                    /*keywordsFilter=="Atom"*/) // B
                    //( (afterKeywordPartExist && sPos_copy == endPos_forAtomParts && isPrevStartPosSameAsStartPos)
                    //|| (!afterKeywordPartExist && (sPos == endPos_forAtomParts || isPrevStartPosSameAsStartPos)))
                    //( //(tryEmptyKeyword != null || lookForLocalRootOnly==false) &&
                    // ( sPos == endPos_forAtomParts || (isPrevStartPosSameAsStartPos /*&& isPrevStartPosSameAsStartPosThisCount > 1*/)) ) // !!!
                {             
                   // MinusZero.Instance.Log(0, "_tryIfKeyword", LOGPREFIX + "conditions 0: ENTER");
                    link = tryLink;

                    newPos = sPos;

                    if (trySpecialKeyword != null)
                    {
                        IList<keywordTryingData> ktdList = createSpecialKeyword(s, trySpecialKeyword, sPos - 1, specialType, possible_newVertexKeywordsByKeywordsFilter, possible_emptyKeyworsByKeywordsFilter, possible_linkKeywordsByKeywordsFilter);

                        keywordTryingData maxKtd = null;
                        int maxKtdNewPos = -1;

                        foreach (keywordTryingData ktd in ktdList)
                        {
                            // MIGHT BE NEEDED !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                            //sPos = CheckIfThereIsSubTextAndProcessIt(s, endPos, null, ktd, sPos);

                            newPos = _tryIsNextLocalRootKeyword(s, LOGPREFIX, newPos, sPos, ktd, isSpaceNext);

                            if (maxKtd == null || newPos > maxKtdNewPos)
                            {
                                maxKtd = ktd;
                                maxKtdNewPos = newPos;
                            }
                        }

                        if (maxKtd != null)
                            examinedKeywords.Add(maxKtd);

                        //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "RETURN:" + trySpecialKeyword + " newPos:" + newPos);
                    }

                    parameters_OUT = new tryIsKeyword_Parameters_OUT(examinedKeywords, link, newPos);

                    params_IN_OUT_dictionary.Add(parameters_IN.GetHashCode(), parameters_OUT);

                    return;
                }
                else
                {
                    tryNewPos = sPos;

                    sPos = startPos;

                    if (trySpecialKeyword != null)
                    {
                        // SAVE LINE NO MEMORY

                        //tryEmptyKeywordStack = new ParsingStack(s); // COPY STACK
                        tryEmptyKeywordStack_LineNoMemory = s.lineNo - 1;
                        //
                    }
                }
                
            }

            // no infinite reccursion

            //string _s = "";
            //foreach (KeyValuePair<IVertex,int> v in s.sameStartPosKewords)
                //if(v!=null)
              //      _s += ", " + v.Key.Value+"["+v.Value+"]";

           // MinusZero.Instance.Log(0, "_tryIsKeyword", LOGPREFIX + "REKURSION " + _s);

            bool containsCondition = false;

            if (parentKeyword!=null && s.sameStartPosKewords.ContainsKey(parentKeyword))
            {
                if (s.sameStartPosKewords[parentKeyword] > l1149_dict)
                    containsCondition = true;
            }

           // if (isPrevStartPosSameAsStartPos && isPrevStartPosSameAsStartPosParentCount == 1)
            if((l1149_dict>-1 && containsCondition)
              //  || (l1149_parent>-1 && isPrevStartPosSameAsStartPos && isPrevStartPosSameAsStartPosThisCount > l1149_parent)
              || (keywordsFilter == "" && l1149_parent > -1 && isPrevStartPosSameAsStartPos && isPrevStartPosSameAsStartPosThisCount > l1149_parent)
              || (keywordsFilter != "" && l1149_parent > -1 && isPrevStartPosSameAsStartPos && isPrevStartPosSameAsStartPosThisCount > 2) // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
              // MIGHT NEED TO MAKE THIS "2" BEING CONFIGURED BY keywordsFilter !!!!!!!!!!!!!!!!!!!!!!!!
                || keywordsFilter == "Atom") // here we also should use A ????? that is specialKeywordGroups_empty.Contains(keywordsFilter)
            { // do not want inifinite recursion
              // if(containsCondition)
              //     MinusZero.Instance.Log(0, "_tryIsKeyword", LOGPREFIX + "HARD RETURN PARENT "+ parentKeyword.Value);
              // else

                //MinusZero.Instance.Log(0, "_tryIsKeyword", LOGPREFIX + "HARD RETURN");

                parameters_OUT = new tryIsKeyword_Parameters_OUT(examinedKeywords, link, newPos);

                params_IN_OUT_dictionary.Add(parameters_IN.GetHashCode(), parameters_OUT);

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

            if (lookForLocalRootOnly)
            {
                if (dict.examinedKeywords_StartInLocalRootOnly.ContainsKey(keywordsFilter))
                    copyExaminedKeywords(dict.examinedKeywords_StartInLocalRootOnly[keywordsFilter], examinedKeywords);
            }
            else
            {
                if (dict.examinedKeywords_All.ContainsKey(keywordsFilter))
                    copyExaminedKeywords(dict.examinedKeywords_All[keywordsFilter], examinedKeywords);
            }

            shallProceed = true;

            while (shallProceed)
            {
                List<keywordTryingData> newExaminedKeywords = new List<keywordTryingData>();

                foreach (keywordTryingData ktd in examinedKeywords)
                {
                    //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"*** "+ktd.keyword+" sPos:" + sPos + " waitingUntilPositionInText:" + ktd.waitingUntilPositionInText + " currentPositionInKeyword:" + ktd.currentPositionInKeyword + " state:"+ktd.state);

                    String keyword = (String)ktd.keywordVertex.Value;
                    
                    if (ktd.state == keywordTryingState.matched)
                    {// matched => matched
                        //if (sPos <= ktd.waitingUntilPositionInText)
                        {
                            newExaminedKeywords.Add(ktd);
                            //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"matched => matched");
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
                                //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "(any) => matched");
                            }
                        }

                        // waiting => keywordCharacter
                        // waiting => waiting
                        if (ktd.state == keywordTryingState.waiting)
                        {
                            if (sPos == ktd.waitingUntilPositionInText)
                            {
                                //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"waiting => keywordCharacter sPos:" + sPos + " fiished waiting");
                                ktd.state = keywordTryingState.keywordCharacter; // waiting => keywordCharacter
                            }
                            else
                            {
                                //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"waiting => waiting sPos:" + sPos + " waiting "+text[sPos]);
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
                                    if (ktd.keyword[0] == dict.CodeGraphLinkPrefix && text.Length >= sPos && text[sPos + 1] != dict.CodeGraphLinkPrefix)
                                    {
                                        // XXX
                                        // this is @ but not @@
                                    }
                                    else
                                    {
                                        ktd.currentPositionInKeyword_Increase(sPos);

                                        newExaminedKeywords.Add(ktd);
                                    //    MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "sPos:" + sPos + " keywordCharacter -> keywordCharacter");
                                    }
                                }
                                else
                                {
                                  //  MinusZero.Instance.Log(0, "_tryIsKeyword", LOGPREFIX+"sPos:" + sPos + "***OUT OF*** keywordCharacter match");
                                }
                        }                    
                    }
                }

                Dictionary<string, ParameterChache> ParameterChache = new Dictionary<string, ParameterChache>();

                // check if anything fits info keyword parameters
                foreach (keywordTryingData ktd in examinedKeywords)
                    if (ktd.state == keywordTryingState.parameter)
                    {
                        int sPox_memory_if_parameterNotFound = sPos;

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

                        if (!chacheHit) 
                        {
                            int isTryKeyword_endPos = endPos_forAtomParts;

                            bool _afterKeywordPartExist = false;

                            //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"TRY for parameter:" +ktd.currentlyProcessedParameterName+" for keyword:" + ktd.keyword + " afterParameterString:" + ktd.afterParameterString + "| ("+sPos+","+endPos+")");

                            bool _isSpaceNext = false;

                            if (ktd.afterParameterString != "")
                            {
                                if (ktd.afterParameterString.Length > 0 && ktd.afterParameterString[0] == ' ')
                                    _isSpaceNext = true;

                                int sPosAfterParameter = ZeroCodeUtil.GetNextMatch(ztext, sPos, ktd.zafterParameterString);

                              //  MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"sPosAfterParameter:" + sPosAfterParameter+ " for afterParameterString:"+ktd.afterParameterString);

                                if (sPosAfterParameter != -1
                                    && ((sPosAfterParameter < endPos) || (endPos == 0)))
                                {
                                    isTryKeyword_endPos = sPosAfterParameter;
                                    _afterKeywordPartExist = true;
                                }
                                else
                                    isTryKeyword_endPos = -1; // do not search; this keyword does not fit in text
                            }

                            //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"isTryKeyword_endPos:" + isTryKeyword_endPos);

                            if (isTryKeyword_endPos != -1)
                            {
                                List<keywordTryingData> foundKeywords = null;
                                string foundLink = null;
                                int _newPos = 0;

                              //  MinusZero.Instance.Log(1, LOGPREFIX+"_tryIsKeyword", "will run _tryIs for:"+ ktd.currentlyProcessedParameterName);

                              
                                // NEW STACK

                                ParsingStack newStack = new ParsingStack(s);

                                s = newStack;

                                s.can_initialize_memory_tabCount = true;

                                //

                                //MinusZero.Instance.Log(0, "_tryIfKeyword", LOGPREFIX + "_tryIsCall / " + ktd.currentlyProcessedParameterName + " / " + ktd.keywordVertex.Value);

                                //MinusZero.Instance.Log(-1, "_tryIfKeyword", LOGPREFIX + sPos + " " + ktd.keywordVertex.Value + ktd.currentlyProcessedParameterName);
                                // int modified_isPrevStartPosSameAsStartPosThisCount = isPrevStartPosSameAsStartPosThisCount;

                                // if (sPos != startPos)
                                //  modified_isPrevStartPosSameAsStartPosThisCount = 0;

                                // XXX

                                bool _canStopByForAtomParts = false;
                                if (dict.keywordInfoDict[ktd.keywordVertex].NonSelfRecursiveParameters)
                                    _canStopByForAtomParts = true;

                                _tryIsKeyword(s, LOGPREFIX + "    ", sPos, startPos, isPrevStartPosSameAsStartPosThisCount, endPos, isTryKeyword_endPos, _canStopByForAtomParts, _afterKeywordPartExist, out foundKeywords, out foundLink, false, ref _newPos, false, ktd.keywordVertex, parameters_IN, paramFilterName, _isSpaceNext);

                                // BACK TO OLD STACK

                                s = s.parentStack;

                                //
                              

                                if (foundLink != null)
                                {
                                    ktd.waitingUntilPositionInText = _newPos;
                                    foundParameter = new ToVertexMock(foundLink, null);
     
                                  //  MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "FOUND PARAMETER LINK:" + foundParameter.ToString() + " waitUntil:" + ktd.waitingUntilPositionInText);
                                }

                                if (foundKeywords.Count() > 0)
                                {
                                    ktd.waitingUntilPositionInText = _newPos - 1;

                                    foundParameter = foundKeywords[0];

                                    //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "FOUND PARAMETER KEYWORDS:" + foundParameter.ToString() + " waitUntil:" + ktd.waitingUntilPositionInText);

                                    if (foundKeywords.Count() > 1)
                                    {
                                        int x = 0; // HOW IS THAT
                                    }
                                }
                            }
                        }

                        if (foundParameter != null)
                        {
                            if (!chacheHit)
                            {
                                ParameterChache p = new ParameterChache();

                                p.Parameter = foundParameter;
                                p.waitingUntilPositionInText = ktd.waitingUntilPositionInText;

                                ParameterChache.Add(paramFilterName, p);
                            }

                            ktd.AddParameter(ktd.currentlyProcessedParameterName, foundParameter);

                            //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "sub add:" + ktd.currentlyProcessedParameterName + " foundParameter:" + foundParameter);

                            //if (foundParameter is keywordTryingData)
                              //  MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + ((keywordTryingData)foundParameter).keyword);


                            newExaminedKeywords.Add(ktd);
                        }
                        else
                            sPos = sPox_memory_if_parameterNotFound;


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
                    //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "SHALPROCEED FALSE only matched");
                }

                if (canStopByForAtomParts && sPos == endPos_forAtomParts)
                {
                    shallProceed = false; // end of this part of text
                }

                if (sPos == endPos )
                {
                    shallProceed = false; // end of this part of text
                    //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"SHALPROCEED FALSE end of this part of text");
                }

                //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "%%% " + text[sPos] + " shallProceed:" + shallProceed + " t1:" + (sPos + 1 < endPos) + " t2: " + s.currentLineInfo.IsLineEnd(sPos + 1));

                if (shallProceed)
                {
                    sPos = SubAndChildCheckAndProcess(s, endPos, examinedKeywords, null, sPos, parentKeyword);

                    if (s.currentLineInfo.IsLineEnd(sPos)) // NEW LINE
                    //if (text[sPos] == '\r')
                    {
                        sPos = s.currentLineInfo.lineEnd_NoTrim + 1;

                        foreach (keywordTryingData ktd in examinedKeywords)
                            if (ktd.state == keywordTryingState.matched)
                            {
                                shallProceed = false; // end of line and one of keywords matched
                                //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "SHALPROCEED FALSE end of line and one of keywords matched");
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
                        //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "SHALPROCEED FALSE  no keyword found");
                    }
                }

                // ++

                sPos++;

                if (s.currentLineInfo.IsLineEnd(sPos))
                    sPos = s.currentLineInfo.lineEnd_NoTrim + 1;

                //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX + "sPos++ " + sPos);

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

                keywordTryingData maxKtd = null;
                int maxKtdSpos = -1;

                if (examinedKeywords.Count > 0)
                {
                    int savedSpos = sPos;

                    foreach(keywordTryingData ktd in examinedKeywords)
                    {
                        if (ktd.matchedOnPositionInText <= s.currentLineInfo.lineEnd)
                        {
                            // MIGHT BE NEEDED !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                            //sPos = CheckIfThereIsSubTextAndProcessIt(s, endPos, null, ktd, sPos);

                            sPos = _tryIsNextLocalRootKeyword(s, LOGPREFIX, savedSpos, ktd.matchedOnPositionInText + 1, ktd, isSpaceNext);
                        }

                        if(maxKtd == null || sPos > maxKtdSpos)
                        {
                            maxKtd = ktd;
                            maxKtdSpos = sPos;
                        }
                    }

                    List<keywordTryingData> c = examinedKeywords;

                    examinedKeywords = new List<keywordTryingData>();
                    examinedKeywords.Add(maxKtd);
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

                if (trySpecialKeyword != null)
                {
                    IList<keywordTryingData> ktdList = createSpecialKeyword(s, trySpecialKeyword, tryNewPos - 1,  specialType, possible_newVertexKeywordsByKeywordsFilter, possible_emptyKeyworsByKeywordsFilter, possible_linkKeywordsByKeywordsFilter);

                    keywordTryingData maxKtd = null;
                    int maxKtdNewPos = -1;

                    if (ktdList.Count > 0) // not sure if we have to, but to act as old code
                    {
                        // ParsingStack copy = s; // ?????

                        //s = tryEmptyKeywordStack; // THIS DOES NOT WORK // GET THE STACK FROM COPY

                        s.lineNo = tryEmptyKeywordStack_LineNoMemory;
                        s.parseNextLine();
                    }

                    foreach (keywordTryingData ktd in ktdList)
                    {                                                
                        int _newPos = SubAndChildCheckAndProcess(s, endPos, null, ktd, newPos - 2, parentKeyword);

                        if (_newPos > newPos - 2)
                            newPos = _newPos + 2;

                        newPos = _tryIsNextLocalRootKeyword(s, LOGPREFIX, newPos, newPos, ktd, isSpaceNext);

                        if (maxKtd == null || newPos > maxKtdNewPos)
                        {
                            maxKtd = ktd;
                            maxKtdNewPos = newPos;
                        }
                    }

                    if (maxKtd != null)
                        examinedKeywords.Add(maxKtd);                    
                }
            } else
                newPos = sPos;

            parameters_OUT = new tryIsKeyword_Parameters_OUT(examinedKeywords, link, newPos);

            if (!params_IN_OUT_dictionary.ContainsKey(parameters_IN.GetHashCode()))
                params_IN_OUT_dictionary.Add(parameters_IN.GetHashCode(), parameters_OUT);

            //MinusZero.Instance.Log(1, "_tryIsKeyword", LOGPREFIX+"END link:"+link+" keywordsCount:"+examinedKeywords.Count);

            //log_keywords(examinedKeywords, 0, LOGPREFIX);
        }

        private int SubAndChildCheckAndProcess(ParsingStack s, int endPos, List<keywordTryingData> examinedKeywords, keywordTryingData _ktd, int sPos, IVertex parentKeyword)
        {
            int outPos = CheckIfThereIsSubTextAndProcessIt(s, endPos, examinedKeywords, _ktd, sPos);

            if (outPos == sPos)
                return CheckIfInsideKeywordAndIfThereIsChildTextAndProcessIt(s, endPos, examinedKeywords, _ktd, sPos, parentKeyword);
            else
                return outPos;
        }

        private int CheckIfThereIsSubTextAndProcessIt(ParsingStack s, int endPos, List<keywordTryingData> examinedKeywords, keywordTryingData _ktd, int sPos)
        {
            if (examinedKeywords == null)
            {
                examinedKeywords = new List<keywordTryingData>();
                examinedKeywords.Add(_ktd);
            }

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

                        if (subText.begLine > subText.endLine)
                            canAddRange = false;

                        if (canAddRange)
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

            return sPos;
        }

        private int CheckIfInsideKeywordAndIfThereIsChildTextAndProcessIt(ParsingStack s, int endPos, List<keywordTryingData> examinedKeywords, keywordTryingData _ktd, int sPos, IVertex parentKeyword)
        {
            if (parentKeyword == null || !dict.keywordInfoDict[parentKeyword].hasCRLF)
                return sPos;

            if (ZeroCodeUtil.DoTextRangeContainString(ztext, lineInfoList[s.lineNo].lineBeg, lineInfoList[s.lineNo].lineEnd, dict.zCRLFoperator))
                return sPos; // XXX bit hacky but no better idea

            if (examinedKeywords == null)
            {
                examinedKeywords = new List<keywordTryingData>();
                examinedKeywords.Add(_ktd);
            }

            if (sPos + 1 < endPos && s.currentLineInfo.IsLineEnd(sPos + 1))
            {
                int nextLineLessTabCount = s.getNextLineWithLessTabCountIfNextLineHasMoreTabCount();

                if (nextLineLessTabCount != -1 && nextLineLessTabCount > s.lineNo + 1 && !lineInfoList[nextLineLessTabCount].startsWithLineContinuation)
                {
                    bool rangeAdded = false;

                    foreach (keywordTryingData ktd in examinedKeywords)
                    if(!dict.keywordInfoDict[ktd.keywordVertex].hasCRLF)
                    {
                        TextRange subText = new TextRange();
                        subText.begLine = s.lineNo + 1;
                        subText.endLine = nextLineLessTabCount - 1;

                        bool canAddRange = true;

                        foreach (KeyValuePair<object, TextRange> kvp in s.subTextRanges)
                            if (kvp.Key is keywordTryingData)
                                if (kvp.Value.begLine == subText.begLine && checkIfKtdContainsKtdAsAParent(ktd, (keywordTryingData)kvp.Key))
                                    canAddRange = false;

                        if (subText.begLine > subText.endLine)
                            canAddRange = false;

                        if (canAddRange)
                        {
                            rangeAdded = true;
                            s.subTextRanges.Add(ktd, subText);
                        }
                    }

                    if (rangeAdded)
                    {
                        s.goToLine(nextLineLessTabCount - 1);

                        sPos = s.currentLineInfo.lineEnd; // XXX migh need correction
                                                          //s.currentLineInfo.lineBeg - 4;
                    }
                }
            }

            return sPos;
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

        private int _tryIsNextLocalRootKeyword(ParsingStack s, string LOGPREFIX, int newPos, int sPos, keywordTryingData ktd, bool isSpaceNext)
        {
            if(!dict.keywordInfoDict[ktd.keywordVertex].HasLocalRoot)
                return newPos;

            string keywordsFilter = "";

            //MinusZero.Instance.Log(-1, "_tryIsKeyword", LOGPREFIX + "_tryIsNextLocalRootKeyword");

            KeywordInfo ki = dict.keywordInfoDict[ktd.keywordVertex];

            if (ki.LocalRootKeywordsGroup != null)
                keywordsFilter = ki.LocalRootKeywordsGroup;

            if (!isSpaceNext)
                while (text[sPos - 1] == ' ') // spaces handling
                    sPos++; // NO WAY !!!!!!!!!!!!!!!!!!! this will stuck @b in function "A" @b, so thats why I added !isSpaceNext

            //
            List<keywordTryingData> _examinedKeywords = new List<keywordTryingData>();

            string _link;

            int _tryPos = 0;

            // NEW STACK 

            // DO NO NEED THIS NOW !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            //ParsingStack newStack = new ParsingStack(s);

            //s = newStack;

            //s.can_initialize_memory_tabCount = true;

            //

            //_tryIsKeyword(s, LOGPREFIX + "    ", sPos - 1, -1, 0, text.Length - 1, text.Length - 1, false, out _examinedKeywords, out _link, true, ref _tryPos, true, null, null, getKewordFilterFromParamName(ktd.currentlyProcessedParameterName));

            _tryIsKeyword(s, LOGPREFIX + "    ", sPos - 1, -1, 0, text.Length - 1, text.Length - 1, false, false, out _examinedKeywords, out _link, true, ref _tryPos, true, null, null, keywordsFilter, isSpaceNext);

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
                        //log_keywords(l, pos + 1, LOGPREFIX);
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
            if (ZeroCodeUtil.IsDoubleDolarMeta(metaEdge))
                return;            

            //if (GeneralUtil.CompareStrings("$$LocalRoot", metaEdge.Meta.Value)
            //|| GeneralUtil.CompareStrings("$$StartInLocalRoot", metaEdge.Meta.Value)
            //|| GeneralUtil.CompareStrings("$$KeywordGroup", metaEdge.Meta.Value))
            //  return;

            if (parentMetaEdge != null
                //&& parentMetaEdge.To.Get(false, "$$LocalRoot:") != null
                && GraphUtil.GetQueryOutFirst(parentMetaEdge.To, "$$LocalRoot", null) != null
                && GeneralUtil.CompareStrings("(?<ANY>)", meta))
            {
                if (val != null && !GeneralUtil.CompareStrings("", val))
                    baseVertex.Value = val;

                nv = baseVertex;
            }
            else
            {

                bool doAdd = false;

                //if (metaEdge.To.Get(false, "$$LocalRoot:") != null) {
                if (GraphUtil.GetQueryOutFirst(metaEdge.To, "$$LocalRoot", null) != null)
                {
                    if (ktd.LocalRootNext != null)
                        doAdd = true;
                }
                else
                    doAdd = true;

                if (doAdd)
                {
                    if (val == MinusZero.Instance.Empty)
                    //if (val.ToString() == "$Empty") // XXX ???
                        nv = AddEdge(s, baseVertex, meta, MinusZero.Instance.Empty).To;
                    else
                    {
                        nv = AddVertex(s, baseVertex, meta, val);                        

                        if(val.ToString()!="$Empty")
                            lastAddedVertex = nv;
                    }
                }                
            }

            tryLocalRootAdd(s, metaEdge, nv, ktd);
        }

        private void tryLocalRootAdd(ParsingStack s, IEdge metaEdge, IVertex nv, keywordTryingData ktd)
        {
            //if (metaEdge.To.Get(false, "$$LocalRoot:") != null && ktd.LocalRootNext != null)
            if (GraphUtil.GetQueryOutFirst(metaEdge.To, "$$LocalRoot", null) != null && ktd.LocalRootNext != null)
            {
                _AddKeywordVertex(s, nv, ktd.LocalRootNext, ktd.LocalRootNext.keywordVertex, null, 0, metaEdge);

                ktd.LocalRootNext = null; // XXX this is hack not to add local root again
            }
        }

        IEdge AddKeywordVertex_AddEdge(ParsingStack s, IVertex baseVertex, IEdge edgeForMeta, IVertex meta, IVertex to)
        {
            return AddEdge(s, baseVertex, meta, to);
        }

        bool IsVertexKeywordManyRoot(IVertex v)
        {
            if (GraphUtil.GetQueryOutFirst(v, "$$KeywordManyRoot", null) != null)
                return true;

            return false;
        }

        bool IsVertexParentKeywordManyRoot(IVertex v)
        {
            foreach (IEdge e in v.InEdges)
                if (GraphUtil.GetQueryOutFirst(e.From, "$$KeywordManyRoot", null) != null)
                    return true;

            return false;
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

            foreach (IEdge _e in keywordAddingVertex) {
                IEdge e = _e;
                //if (e.To.Get(false, @"$$KeywordManyRoot:") != null)
                //if (GraphUtil.GetQueryOutFirst(e.To, "$$KeywordManyRoot", null) != null)
                if(IsVertexKeywordManyRoot(e.To))
                {
                    min_subCount = 0;
                    max_subCount = ktd.multiParameterCount - 1;
                }
                else if (IsVertexParentKeywordManyRoot(e.To))
                {
                    min_subCount = subCount; // XXX
                    max_subCount = subCount;
                }
                else
                {
                    min_subCount = 0; // XXX
                    max_subCount = 0;
                }

                for (int cnt_subCount = min_subCount; cnt_subCount <= max_subCount; cnt_subCount++)
                {
                    if (GeneralUtil.CompareStrings(e.Meta, "$$KeywordManyRoot"))
                        continue;

                    IVertex meta = e.Meta;

                    if ((string)e.Meta.Value == "(?<ANY>)" && useMetaWhenANY != null)
                    {
                        meta = useMetaWhenANY;
                        e = new EasyEdge(e.From, meta, e.To);                        
                    }

                    if ((string)e.Meta.Value == "(?<LAST>)")
                    {
                        meta = lastAddedVertex;
                        e = new EasyEdge(e.From, meta, e.To);
                    }

                    if (VertexOperations.IsLink(e))
                    {
                        if (ZeroCodeUtil.TryStringMatch((string)e.To.Value, 0, "(?<"))
                        {
                            string name = GeneralUtil.GetRegexpEXTRACT((string)e.To.Value, Regex.Escape("(?<") + "(?<EXTRACT>.*)" + Regex.Escape(">)"));

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
                        if (ZeroCodeUtil.TryStringMatch((string)e.To.Value, 0, "(?<"))
                        {
                            string name = GeneralUtil.GetRegexpEXTRACT((string)e.To.Value, Regex.Escape("(?<") + "(?<EXTRACT>.*)" + Regex.Escape(">)"));

                            List<object> subs;
                            object sub;

                            if (name == "ANY")
                                sub = "";
                            else
                            {
                                subs = ktd.parameters[name];
                                sub = subs[cnt_subCount];
                            }

                            if (sub is string)
                                AddKeywordVertex_AddVertex(s, parent, e, meta, sub, ref nv, ktd, parentMetaEdge);

                            if (sub is ToVertexMock)                            
                                nv = AddKeywordVertex_AddEdge(s, parent, e, meta, (IVertex)sub).To;                                

                            if (sub is keywordTryingData)             
                                nv = _AddKeywordVertex(s, parent, (keywordTryingData)sub, ((keywordTryingData)sub).keywordVertex, meta, 0, null);// cnt_subCount);                            

                            tryLocalRootAdd(s, e, nv, ktd);
                        }
                        else
                            //AddKeywordVertex_AddVertex(s, parent, e, meta, e.To, ref nv, ktd, parentMetaEdge);                            
                            AddKeywordVertex_AddVertex(s, parent, e, meta, e.To.Value, ref nv, ktd, parentMetaEdge);

                        TextRange subText = null;

                        if (s.subTextRanges.ContainsKey(ktd) && nv != null)
                        {
                            subText = s.subTextRanges[ktd];
                            s.subTextRanges.Remove(ktd);
                            //if (subText.isNonParameterRange) // do not need this, but who knows
                            //ProcessTextPart(nv, subText.begLine, subText.endLine);
                            //else                            
                        }

                        _AddKeywordVertex(s, nv, ktd, e.To, null, cnt_subCount, null); // XXX maybe this should go after textranges

                        if (subText != null)
                            specialAddingTextPartHack(nv, subText);
                    }
                }
            }

            return nv;
        }

        private void specialAddingTextPartHack(IVertex nv, TextRange subText)
        {
            ParsingStack stack;

            ProcessTextPart(nv, subText.begLine - 1, subText.endLine, out stack);

            int count = nv.OutEdges.Count;

            IEdge lastEdge = nv.OutEdges[count - 1];

            foreach (IEdge e in lastEdge.To)
                if((count--)<=1)
                    nv.AddEdge(e.Meta, e.To);

            nv.DeleteEdge(lastEdge);
        }
        
        int getDoubleColonPos(string s)
        {
            return s.IndexOf("::");            
        }        

        ///

        public class LineInfo
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

                if (ZeroCodeUtil.IsCRorLF(text[p]))
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
                    lineEndWithoutTrim = ZeroCodeUtil.GetNextCRLF(text, p) - 1;

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

                    li.lineBeg = ZeroCodeUtil.TrimRight(text, lineBegWithoutTrim);
                    li.lineEnd = ZeroCodeUtil.TrimLeft(text, lineEndWithoutTrim);

                    li.lineEnd_NoTrim = lineEndWithoutTrim;

                    if (text[li.lineBeg] == dict.LineContinuationPrefix)
                        li.startsWithLineContinuation = true;

                    li.isEmpty = true;
                    for (int x = li.lineBeg; x <= li.lineEnd; x++)
                        if (text[x] != ' ' || text[x] != '\t')
                        {
                            li.isEmpty = false;                            
                            break;
                        }

                    if (li.isEmpty == true)
                        li.tabCount = 0;

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

                if (ZeroCodeUtil.IsCRorLF(text[next]))
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
                IVertex toAdd = null;

                /*if (s.lastAddedVertexParent != null) // WAS
                    toAdd = s.lastAddedVertexParent;
                else if(s.lastAddedVertex != null)
                    toAdd = s.lastAddedVertex;*/

                if (s.lastAddedVertex != null)
                    toAdd = s.lastAddedVertex;
                else if (s.lastAddedVertexParent != null)
                    toAdd = s.lastAddedVertexParent;

                if (toAdd != null)
                    for (int x = 0; x < s.newLineCount; x++)
                        toAdd.AddEdge(NewLine_meta, MinusZero.Instance.Empty);                        

                s.newLineCount = 0;
            }  
        }

        IVertex AddVertex(ParsingStack s, IVertex baseVertex, IVertex meta, object val)
        {
            s.lastAddedVertexParent = baseVertex;

            if (meta != null && GeneralUtil.CompareStrings("(?<ANY>)", meta.Value))
                meta = MinusZero.Instance.Empty;

            if (baseVertex is ToVertexMock)
                return null;
            
            s.lastAddedVertex = baseVertex.AddVertex(meta, val);            

            return s.lastAddedVertex;
        }

        IEdge AddEdge(ParsingStack s, IVertex baseVertex, IVertex meta, IVertex to)
        {
            s.lastAddedVertexParent = baseVertex;
            s.lastAddedVertex = null;

            if (meta != null && GeneralUtil.CompareStrings("(?<ANY>)", meta.Value))
                meta = MinusZero.Instance.Empty;                        

            return baseVertex.AddEdge(meta, to);
        }        

        IVertex ProcessLine(ParsingStack s, IVertex _baseVertex)
        {
            AddNewLines(s);
            
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

                if (s.currentLineNoTabs[0] != dict.CodeGraphVertexPrefix[0]
                    || s.currentLineNoTabs[s.currentLineNoTabs.Length - 1] != dict.CodeGraphVertexSuffix[0])
                {
                    AddError(s.lineNo, "SYNTAX ERROR");

                    return AddVertex(s, _baseVertex, null, "SYNTAX ERROR");
                }                

                string currentLineInner = s.currentLineNoTabs.Substring(dict.CodeGraphVertexPrefix.Length,
                    s.currentLineNoTabs.Length - dict.CodeGraphVertexPrefix.Length - dict.CodeGraphVertexSuffix.Length);

                int doubleColonPos = getDoubleColonPos(currentLineInner);

                if (doubleColonPos == -1) // no meta (before ::)
                {
                    string afterColon = currentLineInner.Trim();

                    if (afterColon[0] == dict.NewVertexPrefix) // if is new value
                        return AddVertex(s, _baseVertex, null, ZeroCodeCommon.stringFromNewVertexString(dict, afterColon));

                    //return AddEdge(s, _baseVertex, null, processLink(ZeroCodeCommon.stringFromLinkString(afterColon, true), _baseVertex)).To;
                    return AddEdge(s, _baseVertex, null, processLink(ZeroCodeCommon.stringFromLinkString(dict, afterColon, false), _baseVertex)).To; // XXX we want cvtq linx in <> with @
                }
                else
                {
                    string beforeColon = currentLineInner.Substring(0, doubleColonPos).Trim();

                    string afterColon = currentLineInner.Substring(doubleColonPos + 2, currentLineInner.Length - doubleColonPos - 2).Trim();

                    IVertex meta = processLink(ZeroCodeCommon.stringFromLinkString(dict, beforeColon, false), _baseVertex);

                    if (afterColon.Length > 0 && afterColon[0] == dict.NewVertexPrefix) // if is new value
                        return AddVertex(s, _baseVertex, meta, ZeroCodeCommon.stringFromNewVertexString(dict, afterColon));

                    //return AddEdge(s, _baseVertex, meta, processLink(ZeroCodeCommon.stringFromLinkString(afterColon, true), _baseVertex)).To;
                    return AddEdge(s, _baseVertex, meta, processLink(ZeroCodeCommon.stringFromLinkString(dict, afterColon, false), _baseVertex)).To; // XXX we want cvtq linx in <> with @
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

        private IVertex ProcessTextPart(IVertex baseVertex, int begLine, int endLine, out ParsingStack stack)
        {
            stack = new ParsingStack(this, null, begLine, endLine);

            stack.parseNextLine();

            IVertex errors = Process_reccurent(stack, baseVertex);

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

            IEdge baseEdge_new;
            return Process(new EdgeBase(null, null, _baseVertex), _text, m0.ZeroTypes.UX.CodeRepresentationEnum.VertexAndManyLines, out baseEdge_new);
        }

        private bool ProcessToVertexMocksToLinks_Delegate(IEdge edge)
        {
            if(edge.Meta is ToVertexMock || edge.To is ToVertexMock)
            {
                IVertex metaVertex = edge.Meta;

                IVertex toVertex = edge.To;

                if(metaVertex is ToVertexMock)
                    metaVertex = ToVertexMock2VertexByLinkString((ToVertexMock)metaVertex);

                if (toVertex is ToVertexMock)
                    toVertex = ToVertexMock2VertexByLinkString((ToVertexMock)toVertex);

                edge.From.AddEdge(metaVertex, toVertex);

                edge.From.DeleteEdge(edge);
            }
            else
            {
                edge.From.AddEdge(edge.Meta, edge.To);

                edge.From.DeleteEdge(edge);
            }
            return false;            
        }

        private void ProcessToVertexMocksToLinks()
        {
            GraphUtil.DeepIterator(parseRoot, this.ProcessToVertexMocksToLinks_Delegate, false, true, false);
        }

        static bool NoCodeViewProcessReEnter = false; // in the parseRoot.AddVertexAndReturnEdge(codeViewMetaEdge, "view trigger"); processing there is 
        // reccurent String2ZeroCodeGraphProcessing call that we need to avoid. this is ok as the expressions are simple and do not need CodeViewProcess()

        private void CodeViewProcess()
        {
            if (NoCodeViewProcessReEnter)
                return;

            NoCodeViewProcessReEnter = true;           

            IVertex codeViewMetaEdge = GraphUtil.GetQueryOutFirst(FormalTextLanguage, "FormalTextLanguageView", "ZeroCodeView");

            if (codeViewMetaEdge != null)
            {
                IEdge codeViewEdge = parseRoot.AddVertexAndReturnEdge(codeViewMetaEdge, "view trigger");
                parseRoot.DeleteEdge(codeViewEdge);
            }

            NoCodeViewProcessReEnter = false;
        }

        IEnumerable<IVertex> SubGraphPreProcessing;

        IVertex parseRoot;

        void GestSubGraphPreProcessing()
        {
            SubGraphPreProcessing = GraphUtil.GetSubGraphWithoutLinksAsList(baseVertex);
        }

        void MoveInEdgesComingFromOutsideOfSubGraphToParseRoot()
        {
            List<IVertex> visited = new List<IVertex>();

            MoveInEdgesComingFromOutsideOfSubGraphToParseRoot_Reccurent(baseVertex, parseRoot.FirstOrDefault().To, visited);
        }

        void MoveInEdgesComingFromOutsideOfSubGraphToParseRoot_Reccurent(IVertex iterationRoot, IVertex parsedVertex, List<IVertex> visited)
        {
            IList<IEdge> OutEdgesRaw = iterationRoot.OutEdgesRaw.ToList();

            foreach (IEdge e in OutEdgesRaw)
                if (e.To != parseRoot && !visited.Contains(e.To) && !VertexOperations.IsLink(e))
                {
                    visited.Add(e.To);

                    IEdge foundInParsed = FindSimilarEdge(parsedVertex, e);

                    if (foundInParsed != null && foundInParsed.To != e.To)
                        {
                            MoveInEdgesComingFromOutsideOfSubGraphToParseVertex_forOneVertex(e.To, foundInParsed.To);

                            MoveInEdgesComingFromOutsideOfSubGraphToParseRoot_Reccurent(e.To, foundInParsed.To, visited);
                        }
                }
        }

        IEdge FindSimilarEdge(IVertex findHere, IEdge toFind)
        {
            IEdge found = null;

            foreach(IEdge e in findHere)
                if(GeneralUtil.CompareStrings(e.Meta, toFind.Meta) && GeneralUtil.CompareStrings(e.To, toFind.To))
                {
                    found = e;
                    break;
                }

            if(found == null)
            {
                // we can have some additonal logic in case we have not found the proper edge (for exmaple there might be some name change)
                // BUT
                // as for now the recommended way of changeing vertex value (atefact name) is string visualiser and not code generation / parsing
            }

            return found;
        }

        void MoveInEdgesComingFromOutsideOfSubGraphToParseVertex_forOneVertex(IVertex existing, IVertex parsedVertex)
        {
            IList<IEdge> InEdgesRaw = existing.InEdgesRaw.ToList();

            foreach (IEdge e in InEdgesRaw)
                if (!SubGraphPreProcessing.Contains(e.From))
                {
                    e.From.AddEdge(e.Meta, parsedVertex);
                    e.From.DeleteEdge(e);
                }

            IList<IEdge> MetaInEdgesRaw = existing.MetaInEdgesRaw.ToList();

            foreach (IEdge e in MetaInEdgesRaw)
                if (!SubGraphPreProcessing.Contains(e.From))
                {
                    foreach (IEdge ee in e.From.ToList())
                        if (ee.Meta == e.Meta && ee.To == e.To) // no to create non exising edge XXX
                        {
                            e.From.AddEdge(parsedVertex, e.To);
                            e.From.DeleteEdge(e);
                        }
                }
        }

        void MoveTriggersToParseRoot()
        {
            IVertex System = GraphUtil.GetQueryOutFirst(MinusZero.Instance.Root, null, "System");
            IVertex Meta = GraphUtil.GetQueryOutFirst(System, null, "Meta");
            IVertex Base = GraphUtil.GetQueryOutFirst(Meta, null, "Base");
            IVertex Vertex = GraphUtil.GetQueryOutFirst(Base, null, "Vertex");
            IVertex GraphChangeTriggerDollar = GraphUtil.GetQueryOutFirst(Vertex, null, "$GraphChangeTrigger");
             
            foreach (IEdge e in GraphUtil.GetQueryOut(baseVertex, "$GraphChangeTrigger", null))
            {
                IVertex newGraphChangeTrigger = parseRoot.OutEdges[0].To.AddVertex(GraphChangeTriggerDollar, e.To.Value);

                foreach (IEdge ee in e.To)
                    newGraphChangeTrigger.AddEdge(ee.Meta, ee.To);

                e.From.DeleteEdge(e);
            }
        }

        void AddError(int lineNumber, string value)
        {
            //IVertex smz = MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes");

            IVertex System = GraphUtil.GetQueryOutFirst(MinusZero.Instance.Root, null, "System");
            IVertex Meta = GraphUtil.GetQueryOutFirst(System, null, "Meta");
            IVertex smz = GraphUtil.GetQueryOutFirst(Meta, null, "ZeroTypes");

            //IVertex error = VertexOperations.AddInstance(errorList, smz.Get(false, "Exception"));
            IVertex Exception = GraphUtil.GetQueryOutFirst(smz, null, "Exception");

            IVertex error = VertexOperations.AddInstance(errorList, Exception);

            error.AddVertex(GraphUtil.GetQueryOutFirst(Exception, null, "Where"), lineNumber.ToString());

            IVertex ExceptionTypeEnum = GraphUtil.GetQueryOutFirst(smz, null, "ExceptionTypeEnum");

            error.AddEdge(GraphUtil.GetQueryOutFirst(Exception, null, "Type"), GraphUtil.GetQueryOutFirst(ExceptionTypeEnum, null, "Error"));

            GraphUtil.SetVertexValue(error, GraphUtil.GetQueryOutFirst(Exception, null, "What"), value);
        }

        void DeleteAllEdgesFromBaseVertex()
        {
           List<List<IEdge>> toDelete = new List<List<IEdge>>();

            foreach (IEdge e in baseVertex.ToList())
                if (!GeneralUtil.CompareStrings(e.Meta, "$ParseRoot") && !GeneralUtil.CompareStrings(e.Meta, "$GraphChangeTrigger"))
                    toDelete.Add(GraphUtil.GetSubGraphAsEdgesWithoutLinksAsList(e));
            
            foreach(List<IEdge> edgesList in toDelete)
                foreach(IEdge e in edgesList)                    
                    e.From.DeleteEdge(e);
        }

        void MoveAllParseRootEdgesToBaseVertex()
        {
            if (parseRoot.OutEdges.Count == 0)
                return;

            object firstValue = parseRoot.FirstOrDefault().To.Value;

            foreach(IEdge e in parseRoot.FirstOrDefault().To.ToList())
            {
                baseVertex.AddEdge(e.Meta, e.To);

                parseRoot.DeleteEdge(e);
            }

            GraphUtil.DeleteEdgeByMeta(baseVertex, "$ParseRoot");

            baseVertex.Value = firstValue;
        }

        //

        void MoveInEdgesFromOneVertexToAnother(IEdge fromEdge, IEdge toEdge)
        {
            IVertex fromVertex = fromEdge.To;
            IVertex toVertex = toEdge.To;

            IList<IEdge> InEdgesRaw = fromVertex.InEdgesRaw.ToList();

            foreach (IEdge e in InEdgesRaw)
            {
                if (e.From == fromEdge.From && e.Meta == fromEdge.Meta)
                    e.From.AddEdge(toEdge.Meta, toVertex);
                else
                    e.From.AddEdge(e.Meta, toVertex);
                
                e.From.DeleteEdge(e);
            }

            IList<IEdge> MetaInEdgesRaw = fromVertex.MetaInEdgesRaw.ToList();

            foreach (IEdge e in MetaInEdgesRaw)
            {
                foreach (IEdge ee in e.From.ToList())
                    if (ee.Meta == e.Meta && ee.To == e.To) // no to create non exising edge XXX
                    {
                        e.From.AddEdge(toVertex, e.To);
                        e.From.DeleteEdge(e);
                    }
            }
        }
        
        // used by ZeroUML diagram representation
        public IVertex Process_EdgeOneLine(IEdge _baseEdge, string _text, out IEdge rootEdge_new)
        {
            string vertexAndManyLines = MinusZero.Instance.DefaultFormalTextGenerator.Generate(_baseEdge, CodeRepresentationEnum.EdgeAndManyLines);

            MultiLineString mls = new MultiLineString(vertexAndManyLines);

            mls.Lines[1] = _text + "\r\n";

            return Process_EdgeAndManyLines(_baseEdge, mls.ToString(), out rootEdge_new);            
        }

        // used by ZeroUML diagram representation
        public IVertex Process_LinearizedManyLines(IEdge _baseEdge, string _text, out IEdge rootEdge_new)
        {
            IVertex _baseEdge_meta = _baseEdge.Meta;

            MultiLineString mls = new MultiLineString(_text);

            mls.AddLeftTab(1);
            mls.InsertEmptyLineBeforeLineNo(1);
            mls.Lines[1] = "\"\"\r\n";

            //

            IEdge _baseEdge_parentEdge = m0.MinusZero.Instance.CreateTempEdge();

            _baseEdge_parentEdge.To.AddEdge(_baseEdge.Meta, _baseEdge.To);

            //

            IVertex returnedVertex = Process_VertexAndManyLines(_baseEdge_parentEdge, mls.ToString());

            IEdge parsedRootEdge = _baseEdge_parentEdge.To.First();

            if (_baseEdge_meta == MinusZero.Instance.Empty)
                rootEdge_new = parsedRootEdge;
            else
                rootEdge_new = _baseEdge.From.AddEdge(_baseEdge_meta, parsedRootEdge.To);
            
            //

            _baseEdge.From.DeleteEdge(_baseEdge);

            //MoveInEdgesFromOneVertexToAnother(_baseEdge.To, rootEdge_new.To);

            //

            return returnedVertex;
        }

        // used by ZeroUML diagram representation
        public IVertex Process_EdgeAndManyLines(IEdge _baseEdge, string _text, out IEdge rootEdge_new)
        {
            MultiLineString mls = new MultiLineString(_text);

            mls.AddLeftTab(1);
            mls.InsertEmptyLineBeforeLineNo(1);
            mls.Lines[1] = "\"\"\r\n";

            //

            IEdge parentEdge = m0.MinusZero.Instance.CreateTempEdge();

            parentEdge.To.AddEdge(_baseEdge.Meta, _baseEdge.To);

            //

            IVertex returnedVertex = Process_VertexAndManyLines(parentEdge, mls.ToString());

            rootEdge_new = parentEdge.To.OutEdges.FirstOrDefault();

            MoveInEdgesFromOneVertexToAnother(_baseEdge, rootEdge_new);

            return returnedVertex;
        }

        public IVertex Process_VertexAndManyLines(IEdge _baseEdge, string _text)
        {            
            ZeroCodeUtil.ClearZeroCodeUtilDicionaries();

            baseVertex = _baseEdge.To;

            errorList = MinusZero.Instance.CreateTempVertex();

            GestSubGraphPreProcessing();

            text = _text + "\r\n"; // for regexpes
            ztext = new zstring(text);

            prepareLineInfoList();

            prepareImportList();

            GraphUtil.DeleteEdgeByMeta(baseVertex, "$ParseRoot");
            GraphUtil.DeleteEdgeByMeta(baseVertex, "$ParseArtefacts");

            //parseRoot = baseVertex.AddVertex(MinusZero.Instance.Root.Get(false, @"System\Meta\Base\$ParseRoot"),"");

            IVertex System = GraphUtil.GetQueryOutFirst(MinusZero.Instance.Root, null, "System");
            IVertex Meta = GraphUtil.GetQueryOutFirst(System, null, "Meta");
            IVertex Base = GraphUtil.GetQueryOutFirst(Meta, null, "Base");

            IVertex _parseRoot = GraphUtil.GetQueryOutFirst(Base, null, "$ParseRoot");

            parseRoot = baseVertex.AddVertex(_parseRoot, "");

            ParsingStack stack;

            ProcessTextPart(parseRoot, 0, lineInfoList.Count - 1, out stack);

            if (errorList.Count() == 0)
            {
                if (stack.lineNo > 0)
                    CodeViewProcess();

                ProcessToVertexMocksToLinks();
                MoveInEdgesComingFromOutsideOfSubGraphToParseRoot();
                MoveTriggersToParseRoot();                
                DeleteAllEdgesFromBaseVertex();
                MoveAllParseRootEdgesToBaseVertex();                
            }
            else
            {
                //baseVertex.AddEdge(MinusZero.Instance.Root.Get(false, @"System\Meta\Base\$ParseArtefacts"), errorList);

                IVertex ParseArtefacts = GraphUtil.GetQueryOutFirst(Base, null, "$ParseArtefacts");
                baseVertex.AddEdge(ParseArtefacts, errorList);
            }

            DisposeImportList();

            return errorList;
        }

        // used by ZeroUML diagram representation
        public IVertex Process_ManyLinesExcludingParent(IEdge _baseEdge, string _text)
        {
            IEdge nextEdge = GraphUtil.GetQueryOutFirstEdge(_baseEdge.To, "Next", null);

            if (nextEdge != null)
                _baseEdge.To.DeleteEdge(nextEdge);
                
            //

            string firstLine = MinusZero.Instance.DefaultFormalTextGenerator.Generate(
                _baseEdge,
                CodeRepresentationEnum.EdgeOneLine);


            MultiLineString mls = new MultiLineString(_text);

            mls.AddLeftTab(1);
            mls.InsertEmptyLineBeforeLineNo(1);
            mls.Lines[1] = firstLine + "\r\n";

            //

            IEdge _baseEdge_parentEdge = m0.MinusZero.Instance.CreateTempEdge();

            _baseEdge_parentEdge.To.AddEdge(null, _baseEdge.To);

            //

            IVertex returnedVertex = Process_VertexAndManyLines(_baseEdge, mls.ToString());

            if (nextEdge != null)
                _baseEdge.To.AddEdge(nextEdge.Meta, nextEdge.To);

            return returnedVertex;
        }

        public IVertex Process(IEdge _baseEdge, string _text, CodeRepresentationEnum codeRepresentation, out IEdge rootEdge_new)
        {
            rootEdge_new = null;

            switch (codeRepresentation)
            {
                case CodeRepresentationEnum.EdgeOneLine: return Process_EdgeOneLine(_baseEdge, _text, out rootEdge_new);

                case CodeRepresentationEnum.EdgeAndManyLines: return Process_EdgeAndManyLines(_baseEdge, _text, out rootEdge_new);

                case CodeRepresentationEnum.VertexAndManyLines: return Process_VertexAndManyLines(_baseEdge, _text);

                case CodeRepresentationEnum.LinearizedManyLines: return Process_LinearizedManyLines(_baseEdge, _text, out rootEdge_new);

                case CodeRepresentationEnum.ManyLinesExcludingParent: return Process_ManyLinesExcludingParent(_baseEdge, _text);
            }

            return null;
        }

        public void DisposeImportList()
        {
            importList.AddExternalReference ();
            importMetaList.AddExternalReference();
            importDirectList.AddExternalReference();
            importDirectMetaList.AddExternalReference();
        }

        public Text2GraphProcessing(IVertex formalTextLanguage)
        {
            FormalTextLanguage = formalTextLanguage;

            setupHelpVariables_onlyOnce();

            dict = DictionariesForFormalTextLanguageFactory.Get(formalTextLanguage);
        }
    }
}