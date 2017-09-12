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

        string currentLine;
        string currentLineNoTabs;

        int currentLineFirstCharacterPos;
        int prevFirstCharacterPos;

        //

        IVertex r = m0.MinusZero.Instance.Root;

        bool ParseLine()
        {
            if (skipParse)
            {
                skipParse = false;
                return true;
            }

            prevFirstCharacterPos = currentLineFirstCharacterPos;
            currentLineFirstCharacterPos = 0;

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
                    currentLineFirstCharacterPos = pos-begPos;
                }

                pos++;
                c = text[pos];
            }

            if(pos == (text.Length - 1))
                currentLine = text.Substring(begPos,pos - begPos + 1);
            else
                currentLine = text.Substring(begPos, pos - begPos);

            currentLineNoTabs = currentLine.Substring(currentLineFirstCharacterPos).Trim(); // can try witchout Trim

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
            lineNo = 0;
        }

        string processAsQuoted(string s)
        {
            s=s.Substring(1, s.Length - 2);

            s=s.Replace("\\\"","\"");

            return s;
        }

        string removeLinkPrefix(string s)
        {
            return s.Substring(1);
        }

        IVertex query(IVertex baseVertex, string query)
        {
            return baseVertex.Get(query);
        }

        IVertex queryMetaMode(IVertex baseVertex, string query)
        {
            return baseVertex.Get(query);
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

            }
            else {

                string currentLineInner = currentLineNoTabs.Substring(ZeroCodeCommon.CodeGraphVertexPrefix.Length, 
                   currentLineNoTabs.Length - ZeroCodeCommon.CodeGraphVertexPrefix.Length - ZeroCodeCommon.CodeGraphVertexSuffix.Length);
                    
                int doubleColonPos = getDoubleColonPos(currentLineInner);

                if (doubleColonPos == -1) // no meta (before ::)
                {
                    string afterColon = currentLineInner.Trim();

                    if (afterColon[0] == '"') // if is new value
                        return _baseVertex.AddVertex(null, processAsQuoted(afterColon));

                    if (afterColon[0] == '@')
                        return _baseVertex.AddEdge(null, processLink(removeLinkPrefix(afterColon))).To;

                    return _baseVertex.AddVertex(null, "SYNTAX ERROR");
                }
                else
                {
                    string beforeColon = currentLineInner.Substring(0, doubleColonPos).Trim();

                    string afterColon = currentLineInner.Substring(doubleColonPos + 2, currentLineInner.Length - doubleColonPos-2).Trim();

                    IVertex meta = processLink(beforeColon);

                    if (afterColon[0] == '"') // if is new value
                        return _baseVertex.AddVertex(meta, processAsQuoted(afterColon));
                    else
                        return _baseVertex.AddEdge(meta, processLink(afterColon)).To;
                }
            }

            return null;
        }

        List<IVertex> examinedKeywords_All;
        List<IVertex> examinedKeywords;

        bool TryIsKeyword(string s)
        {
            if (!ZeroCodeUtil.tryStringMatch(s, 0, ZeroCodeCommon.CodeGraphVertexPrefix) 
                && !ZeroCodeUtil.tryStringEndMatch(s, ZeroCodeCommon.CodeGraphVertexSuffix))
            {
                examinedKeywords = examinedKeywords_All;

                _tryKeyword(s, 0, 0);

                return true;
            }
            else
                return false;
        }

        void _tryKeyword(string s, int sPos, int keywordPos)
        {
            List<IVertex> newExaminedKeywords = new List<IVertex>();

            foreach(IVertex v in examinedKeywords)
            {
                //if (ZeroCodeUtil.tryStringMatch(((String)v.Value),keywordPos,"(?"))
                {
                //    int x = 0;
                }

                String keyword = (String)v.Value;

                if (keyword.Length<=keywordPos+1 && s[sPos] == keyword[keywordPos])
                    newExaminedKeywords.Add(v);
            }

            examinedKeywords = newExaminedKeywords;

            if (s.Length < sPos)
                return;

            _tryKeyword(s, sPos + 1, keywordPos + 1);
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
                if (currentLineFirstCharacterPos > prevFirstCharacterPos)
                {
                    int prevFirstCharacterPos_memory = prevFirstCharacterPos;
                    //int currentLineFirstCharacterPos_memory = currentLineFirstCharacterPos;

                    Process_reccurent(prevVertex);

                    prevFirstCharacterPos = prevFirstCharacterPos_memory;
                    // currentLineFirstCharacterPos = currentLineFirstCharacterPos_memory;

                    continue;
                }

                if (currentLineFirstCharacterPos == prevFirstCharacterPos)
                    prevVertex = ProcessLine(_baseVertex);

                if (currentLineFirstCharacterPos < prevFirstCharacterPos)
                {
                    skipParse = true;

                    return;
                }
            }
        }

        public String2ZeroCodeGraphProcessing()
        {
            setupHelpVariables();

            examinedKeywords_All = new List<IVertex>();

            foreach (IEdge keyword in MinusZero.Instance.Root.GetAll(@"User\CurrentUser:\CodeSettings:\Keyword:\$Keyword:"))
                examinedKeywords_All.Add(keyword.To);
        }
    }
}


