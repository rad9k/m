using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using m0.Foundation;
using m0.Util;

namespace m0.ZeroCode
{
    public class ZeroCodeUtil
    {       
        public static IDictionary<string, IList<IVertex>> getFilteredKeywordListByGroup(IVertex FormalTextLanguage,string Filter)
        {
            Dictionary<string, IList<IVertex>> list = new Dictionary<string, IList<IVertex>>();

            foreach(IEdge e in FormalTextLanguage.GetAll(false, @"Keywords:\$Keyword:{" + Filter+"}"))           
                {
                IVertex groups = e.To.GetAll(false, @"$$KeywordGroup:");

                string groupName;

                foreach (IEdge group in groups)
                {
                    groupName = (string)group.To.Value;

                    if (!list.ContainsKey(groupName))
                        list.Add(groupName, new List<IVertex>());

                    list[groupName].Add(e.To);
                }

                groupName = ""; // THIS CAUSES PROBLEM
                // but we leave it becouse if (c1089 || ((possible_emptyKeywordByKeywordsFilter!=null || possible_newVertexKeywordByKeywordsFilter!=null) && keywordsFilter!="") << keywordsFilter!=""

                if (!list.ContainsKey(groupName))
                    list.Add(groupName, new List<IVertex>());

                list[groupName].Add(e.To);

            }

            return list;
        }

        public static IList<IVertex> getFilteredKeywordList(IVertex FormalTextLanguage, string Filter)
        {
            IList<IVertex> list = new List<IVertex>();

            foreach (IEdge e in FormalTextLanguage.GetAll(false, @"Keywords:\$Keyword:{" + Filter + "}"))                            
                    list.Add(e.To);
            
            return list;
        }

        public static bool IsDolarMeta(IEdge e)
        {
            if (!(e.Meta.Value is string))
                return false;

            string metaValue = (string)e.Meta.Value;

            if (metaValue == "$Empty")
                return false;

            if (metaValue.Length >= 1 && metaValue[0] == '$')
                return true;

            return false;
        }

        public static bool IsDoubleDolarMeta(IEdge e)
        {
            if (!(e.Meta.Value is string))
                return false;

            string metaValue = (string)e.Meta.Value;

            if (metaValue.Length >= 2 && metaValue[0] == '$' && metaValue[1] == '$')
                return true;

            return false;
        }

        public static int getNextCRLF(string s, int pos)
        {
            bool shallProcess=true;

            while(shallProcess)
            {
                if (pos >= s.Length)
                    return -1;

                if (isCRLF(s[pos]))
                    return pos;

                pos++;
            }

            return -1; // no hit @here
        }

        public static bool isCRLF(char c)
        {
            if (c == '\r' || c == '\n')
                return true;

            return false;
        }

        public static int trimRight(string s, int pos)
        {
            while (s[pos] == ' ')
                pos++;

            return pos;
        }

        public static int trimLeft(string s, int pos)
        {
            while (s[pos] == ' ')
                pos--;

            return pos;
        }
    
        public static void getQueryFirstAndSecondPart(string query, out string firstPart, out string secondPart)
        {
            firstPart = null;
            secondPart = null;

            int slashPos=query.IndexOf('\\');

            if (slashPos == -1)
            {
                firstPart = query;
                return;
            }
                

            secondPart = query.Substring(slashPos+1, query.Length - slashPos -1);

            firstPart = query.Substring(0, slashPos);
        }

        public static bool tryStringMatch(string s, int pos, string toMatch)
        {
            int toMatchLength = toMatch.Length;

            if (s.Length < pos + toMatchLength)
                return false;

            for (int x = 0; x < toMatchLength; x++)
                if (s[pos + x] != toMatch[x])
                    return false;

            return true;
        }

        public static bool tabRemove_tryStringMatch(string s, int pos, string toMatch, int toRemoveTabs)
        {
            int toMatchLength = toMatch.Length;

            if (s.Length < pos + toMatchLength)
                return false;

            int tabPhase = 0;

            for (int x = 0; x < toMatchLength; x++)
            {
                while (s[pos + x + tabPhase] == '\t' && toRemoveTabs > 0)
                {
                    toRemoveTabs--;
                    tabPhase++;
                }

                if (s.Length < pos + toMatchLength + tabPhase)
                    return false;

                if (s[pos + x + tabPhase] != toMatch[x])
                    return false;
            }

            return true;
        }

        public static bool doTextRangeContainString(string s, int beg, int end, string toMatch)
        {
            int cnt;

            for (cnt = beg; cnt + toMatch.Length -1 <= end;cnt++)
                if (tryStringMatch(s, cnt, toMatch))
                    return true;

            return false;
        }

        public static bool tryStringEndMatch(string s, string toMatch)
        {
            int sLength = s.Length;

            int toMatchLength = toMatch.Length;

            if (s.Length < toMatchLength)
                return false;

            for (int x = 1; x <= toMatch.Length; x++)
                if (s[sLength - x] != toMatch[toMatchLength - x])
                    return false;

            return true;
        }

        public static int getNextMatch(string s, int startFrom, string toMatch)
        {
            int pos = startFrom;

            while ( (pos+toMatch.Length) <= s.Length)
            {
                if (tryStringMatch(s, pos, toMatch))
                    return pos;

                pos++;
            }

            return -1;
        }

        public static int getNextMatch_twoAtOnce(string s, int startFrom, string toMatch1, string toMatch2, out int whatMatch)
        {
            whatMatch = 0;

            int pos = startFrom;

            bool shallProcess = true;

            while (shallProcess)
            {
                bool canCheck1 = (pos + toMatch1.Length) <= s.Length;
                bool canCheck2 = (pos + toMatch2.Length) <= s.Length;

                if (!canCheck1 && !canCheck2)
                    shallProcess = false;
                else
                {
                    if (canCheck1 && tryStringMatch(s, pos, toMatch1) && toMatch1.Length > 0)
                    {
                        whatMatch = 1;
                        return pos;
                    }

                    if (canCheck2 && tryStringMatch(s, pos, toMatch2) && toMatch2.Length > 0)
                    {
                        whatMatch = 2;
                        return pos;
                    }
                }

                pos++;
            }

            return -1;
        }

        public static char getFirstCharacterFromKeyword(string keyword)
        {
            if (ZeroCodeUtil.tryStringMatch(keyword, 0, "(?<"))
            {
                int pos = ZeroCodeUtil.getNextMatch(keyword, 3, ">)");

                return keyword[pos + 2];
            }
            else
                return keyword[0];
        }

        public static string getNextCharacterPartFromKeyword_startingFromNonParameter(string keyword, int startFrom)
        {
            for (int x = startFrom; x < keyword.Length; x++)
            {
                if (ZeroCodeUtil.tryStringMatch(keyword, x, "(?<"))
                    return keyword.Substring(startFrom, x - startFrom);

                if (ZeroCodeUtil.tryStringMatch(keyword, x, "(*")) // needs some clever tests ideas, if this is valid????
                    return keyword.Substring(startFrom, x - startFrom);
            }

            return keyword.Substring(startFrom);
            /*
            int firstTryPos = getNextMatch(keyword, startFrom, "(*(+");

            int secondTryPos = getNextMatch(keyword, startFrom, "(?<");

            if(firstTryPos==-1 && secondTryPos==-1)
                return keyword.Substring(startFrom);

            if(secondTryPos == -1)
                return keyword.Substring(startFrom, firstTryPos - startFrom);

            if(firstTryPos == -1)
                return keyword.Substring(startFrom, secondTryPos - startFrom);

            return keyword.Substring(startFrom, Math.Min(firstTryPos,secondTryPos) - startFrom);*/
        }

        public static bool isStringOnlyWhiteSpaces(string s)
        {
            bool onlyWhite = true;

            foreach (char c in s)
                if (c != ' ' && c != '\t')
                {
                    onlyWhite = false;
                    break;
                }

            return onlyWhite;

        }

        public static List<string> tokenizeKeyword(string k, bool doNotCareAboutSub)
        {
            List<string> l = new List<string>();

            string current = "";

            for (int x = 0; x < k.Length; x++)
            {
                if (tryStringMatch(k, x, "(*"))
                {
                    x += 1;

                    if (current != "")
                    {
                        l.Add(current);
                        current = "";
                    }

                    l.Add("(*");
                }
                else
                if (tryStringMatch(k, x, "*)"))
                {
                    x += 1;

                    if (current != "")
                    {
                        l.Add(current);
                        current = "";
                    }

                    l.Add("*)");
                }
                else
                if (tryStringMatch(k, x, "(+"))
                {
                    x += 1;

                    if (current != "")
                    {
                        l.Add(current);
                        current = "";
                    }

                    l.Add("(+");
                }
                else
                if (tryStringMatch(k, x, "+)"))
                {
                    x += 1;

                    if (current != "")
                    {
                        l.Add(current);
                        current = "";
                    }

                    l.Add("+)");
                }
                else
                if (tryStringMatch(k, x, "(?<SUB>)"))
                {
                    x += 7;

                    current += "(?<SUB>)";
                }
                else
                if (tryStringMatch(k, x, "(?<"))
                {
                    x += 2;

                    if (current != "")
                    {
                        l.Add(current);
                        current = "";
                    }

                    l.Add("(?<");
                }
                else
                if (tryStringMatch(k, x, ">)"))
                {
                    x += 1;

                    if (current != "")
                    {
                        l.Add(current);
                        current = "";
                    }

                    l.Add(">)");
                }
                else
                if (x < k.Length)
                    current += k[x];
            }

            if (current != "")
                l.Add(current);

            return l;
        }
    }
}
