using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using m0.Foundation;
using m0.Util;
using m0.Graph;
using m0.ZeroCode.Helpers;

namespace m0.ZeroCode
{
    public class TryStringMatch_params
    {
        public string s;
        public int pos;
        public string toMatch;

        public TryStringMatch_params(string _s, int _pos, string _toMatch)
        {
            this.s = _s;
            this.pos = _pos;
            this.toMatch = _toMatch;
        }

        public override int GetHashCode()
        {
            int result = 37; 

            result *= 397; 
                        
            result += s.GetHashCode();

            result *= 397;

            result += pos.GetHashCode();

            result *= 397;

            result += toMatch.GetHashCode();

            return result;
        }
    }

    class TabRemove_tryStringMatch_params
    {
        public string s;
        public int pos;
        public string toMatch;
        public int toRemoveTabs;

        public TabRemove_tryStringMatch_params(string s, int pos, string toMatch, int toRemoveTabs)
        {
            this.s = s;
            this.pos = pos;
            this.toMatch = toMatch;
            this.toRemoveTabs = toRemoveTabs;
        }

        public override int GetHashCode()
        {            
            int result = 37;

            result *= 397;

            result += s.GetHashCode();

            result *= 397;

            result += pos.GetHashCode();

            result *= 397;

            result += toMatch.GetHashCode();

            result *= 397;

            result += toRemoveTabs.GetHashCode();

            return result;
        }
    }

    class TryStringEndMatch_params
    {
        public string s;
        public string toMatch;

        public TryStringEndMatch_params(string s, string toMatch)
        {
            this.s = s;
            this.toMatch = toMatch;
        }

        public override int GetHashCode()
        {            
            int result = 37;

            result *= 397;

            result += s.GetHashCode();

            result *= 397;

            result += toMatch.GetHashCode();

            return result;
        }
    }

    class GetNextMatch_params
    {
        public string s;
        public int startFrom;
        public string toMatch;

        public GetNextMatch_params(string s, int startFrom, string toMatch)
        {
            this.s = s;
            this.startFrom = startFrom;
            this.toMatch = toMatch;
        }

        public override int GetHashCode()
        {            
            int result = 37;

            result *= 397;

            result += s.GetHashCode();

            result *= 397;

            result += startFrom.GetHashCode();

            result *= 397;

            result += toMatch.GetHashCode();            

            return result;
        }
    }

    class GetNextMatch_twoAtOnce_params
    {
        public string s;
        public int startFrom;
        public string toMatch1;
        public string toMatch2;

        public GetNextMatch_twoAtOnce_params(string s, int startFrom, string toMatch1, string toMatch2)
        {
            this.s = s;
            this.startFrom = startFrom;
            this.toMatch1 = toMatch1;
            this.toMatch2 = toMatch2;
        }

        public override int GetHashCode()
        {
            int result = 37;

            result *= 397;

            result += s.GetHashCode();

            result *= 397;

            result += toMatch1.GetHashCode();

            result *= 397;

            result += toMatch2.GetHashCode();

            return result;
        }
    }

    class GetNextCharacterPartFromKeyword_startingFromNonParameter_params
    {
        public string keyword;
        public int startFrom;

        public GetNextCharacterPartFromKeyword_startingFromNonParameter_params(string keyword, int startFrom)
        {
            this.keyword = keyword;
            this.startFrom = startFrom;
        }

        public override int GetHashCode()
        {            
            int result = 37;

            result *= 397;

            result += keyword.GetHashCode();

            result *= 397;

            result += startFrom.GetHashCode();

            return result;
        }
    }

    class StringMatchingDictionary
    {
        public Dictionary<int, bool> TryStringMatch = new Dictionary<int, bool>();
        public Dictionary<int, bool> TabRemove_tryStringMatch = new Dictionary<int, bool>();
        public Dictionary<int, bool> TryStringEndMatch = new Dictionary<int, bool>();
        public Dictionary<int, int> GetNextMatch = new Dictionary<int, int>();
        public Dictionary<int, int> GetNextMatch_twoAtOnce = new Dictionary<int, int>();
        public Dictionary<int, string> GetNextCharacterPartFromKeyword_startingFromNonParameter = new Dictionary<int, string>();
    }

    public class ZeroCodeUtil
    {
        static StringMatchingDictionary smdict = new StringMatchingDictionary();

        public static bool FilterEdgeForGraph2TextProcessing(IEdge toFilterEdge)
        {
            if (GeneralUtil.CompareStrings(toFilterEdge.Meta, "$GraphChangeTrigger")) 
                return false;            

            return true;
        }

        public static IDictionary<string, IList<IVertex>> GetFilteredKeywordListByGroup(IVertex FormalTextLanguage,string metaFilter)
        {
            IList<IEdge> keywordList = new List<IEdge>();

            IVertex keywords = GraphUtil.GetQueryOutFirst(FormalTextLanguage, "Keywords", null);

            foreach (IEdge e in GraphUtil.GetQueryOut(keywords, "$Keyword", null))
                if (GraphUtil.GetQueryOutCount(e.To, metaFilter, null) > 0)
                    keywordList.Add(e);

            Dictionary<string, IList<IVertex>> list = new Dictionary<string, IList<IVertex>>();
 
            foreach (IEdge e in keywordList)           
                {
                //IVertex groups = e.To.GetAll(false, @"$$KeywordGroup:");
                IList<IEdge> groups = GraphUtil.GetQueryOut(e.To, "$$KeywordGroup", null);

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

        public static IList<IVertex> GetFilteredKeywordList(IVertex FormalTextLanguage, string metaFilter)
        {
            IList<IVertex> list = new List<IVertex>();

            IVertex keywords = GraphUtil.GetQueryOutFirst(FormalTextLanguage, "Keywords", null);

            foreach (IEdge e in GraphUtil.GetQueryOut(keywords, "$Keyword", null))
                if (GraphUtil.GetQueryOutCount(e.To, metaFilter, null) > 0)
                    list.Add(e.To);
            
            return list;
        }

        public static bool ShouldNotExecute(IEdge e) // XXX in some cases it might not work - instruction with meta begginning with $ will not be executed. nor its children
        {
            if (!(e.Meta.Value is string))
                return false;

            string metaValue = (string)e.Meta.Value;

            if (metaValue == "$Empty")
                return false;            
                

            if (metaValue.Length >= 1 && metaValue[0] == '$')
                return true;

            if (GraphUtil.ExistQueryOut(e.Meta, "$$NoSequentialExecution", null))
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

        public static int GetNextCRLF(string s, int pos)
        {
            bool shallProcess=true;

            while(shallProcess)
            {
                if (pos >= s.Length)
                    return -1;

                if (IsCRLF(s[pos]))
                    return pos;

                pos++;
            }

            return -1; // no hit @here
        }

        public static bool IsCRLF(char c)
        {
            if (c == '\r' || c == '\n')
                return true;

            return false;
        }

        public static int TrimRight(string s, int pos)
        {
            while (s[pos] == ' ')
                pos++;

            return pos;
        }

        public static int TrimLeft(string s, int pos)
        {
            while (s[pos] == ' ')
                pos--;

            return pos;
        }
    
        public static void GetQueryFirstAndSecondPart(FormalTextLanguageDictinaries dict, string query, out string firstPart, out string secondPart)
        {
            firstPart = null;
            secondPart = null;

            bool isInEscape = false;

            int slashPos = -1;

            for (int x = 0; x < query.Length; x++)
            {
                if (!isInEscape && query[x] == dict.EscapedSequencePrefix)
                {
                    isInEscape = true;
                }
                else if (isInEscape && query[x] == dict.EscapedSequenceSuffix)
                    isInEscape = false;

                if (isInEscape == false && query[x] == dict.QuerySlash)
                    slashPos = x;
            }
            

            if (slashPos == -1)
            {
                firstPart = query;
                return;
            }
                

            secondPart = query.Substring(slashPos+1, query.Length - slashPos -1);

            firstPart = query.Substring(0, slashPos);
        }

        public static bool TryStringMatch(string s, int pos, string toMatch)
        {
            TryStringMatch_params p = new TryStringMatch_params(s, pos, toMatch);

            if (smdict.TryStringMatch.ContainsKey(p.GetHashCode()))
                return smdict.TryStringMatch[p.GetHashCode()];

            int toMatchLength = toMatch.Length;

            if (s.Length < pos + toMatchLength)
            {
                smdict.TryStringMatch.Add(p.GetHashCode(), false);
                return false;
            }

            for (int x = 0; x < toMatchLength; x++)
                if (s[pos + x] != toMatch[x])
                {
                    smdict.TryStringMatch.Add(p.GetHashCode(), false);
                    return false;
                }

            smdict.TryStringMatch.Add(p.GetHashCode(), true);
            return true;
        }

        public static bool TabRemove_tryStringMatch(string s, int pos, string toMatch, int toRemoveTabs)
        {
            TabRemove_tryStringMatch_params p = new TabRemove_tryStringMatch_params(s, pos, toMatch, toRemoveTabs);

            if (smdict.TabRemove_tryStringMatch.ContainsKey(p.GetHashCode()))
                return smdict.TabRemove_tryStringMatch[p.GetHashCode()];

            int toMatchLength = toMatch.Length;

            if (s.Length < pos + toMatchLength)
            {
                smdict.TabRemove_tryStringMatch.Add(p.GetHashCode(), false);
                return false;
            }

            int tabPhase = 0;

            for (int x = 0; x < toMatchLength; x++)
            {
                while (s[pos + x + tabPhase] == '\t' && toRemoveTabs > 0)
                {
                    toRemoveTabs--;
                    tabPhase++;
                }

                if (s.Length < pos + toMatchLength + tabPhase)
                {
                    smdict.TabRemove_tryStringMatch.Add(p.GetHashCode(), false);
                    return false;
                }

                if (s[pos + x + tabPhase] != toMatch[x])
                {
                    smdict.TabRemove_tryStringMatch.Add(p.GetHashCode(), false);
                    return false;
                }
            }

            smdict.TabRemove_tryStringMatch.Add(p.GetHashCode(), true);
            return true;
        }

        public static bool DoTextRangeContainString(string s, int beg, int end, string toMatch)
        {
            int cnt;

            for (cnt = beg; cnt + toMatch.Length -1 <= end;cnt++)
                if (TryStringMatch(s, cnt, toMatch))
                    return true;

            return false;
        }

        public static bool TryStringEndMatch(string s, string toMatch)
        {
            TryStringEndMatch_params p = new TryStringEndMatch_params(s, toMatch);

            if (smdict.TryStringEndMatch.ContainsKey(p.GetHashCode()))
                return smdict.TryStringEndMatch[p.GetHashCode()];

            int sLength = s.Length;

            int toMatchLength = toMatch.Length;

            if (s.Length < toMatchLength)
            {
                smdict.TryStringEndMatch.Add(p.GetHashCode(), false);
                return false;
            }

            for (int x = 1; x <= toMatch.Length; x++)
                if (s[sLength - x] != toMatch[toMatchLength - x])
                {
                    smdict.TryStringEndMatch.Add(p.GetHashCode(), false);
                    return false;
                }

            smdict.TryStringEndMatch.Add(p.GetHashCode(), true);
            return true;
        }

        public static int GetNextMatch(string s, int startFrom, string toMatch)
        {
            GetNextMatch_params p = new GetNextMatch_params(s, startFrom, toMatch);

            if (smdict.GetNextMatch.ContainsKey(p.GetHashCode()))
                return smdict.GetNextMatch[p.GetHashCode()];

            int pos = startFrom;

            while ( (pos+toMatch.Length) <= s.Length)
            {
                if (TryStringMatch(s, pos, toMatch))
                {
                    smdict.GetNextMatch.Add(p.GetHashCode(), pos);
                    return pos;
                }

                pos++;
            }

            smdict.GetNextMatch.Add(p.GetHashCode(), -1);
            return -1;
        }

        public static int GetNextMatch_twoAtOnce(string s, int startFrom, string toMatch1, string toMatch2)
        {
            GetNextMatch_twoAtOnce_params p = new GetNextMatch_twoAtOnce_params(s, startFrom, toMatch1, toMatch2);

            if (smdict.GetNextMatch_twoAtOnce.ContainsKey(p.GetHashCode()))
                return smdict.GetNextMatch_twoAtOnce[p.GetHashCode()];

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
                    if (canCheck1 && TryStringMatch(s, pos, toMatch1) && toMatch1.Length > 0)
                    {
                        smdict.GetNextMatch_twoAtOnce.Add(p.GetHashCode(), 1);
                        return 1;
                    }

                    if (canCheck2 && TryStringMatch(s, pos, toMatch2) && toMatch2.Length > 0)
                    {
                        smdict.GetNextMatch_twoAtOnce.Add(p.GetHashCode(), 2);
                        return 2;
                    }
                }

                pos++;
            }

            smdict.GetNextMatch_twoAtOnce.Add(p.GetHashCode(), 0);
            return 0;
        }

        public static string GetNextCharacterPartFromKeyword_startingFromNonParameter(string keyword, int startFrom)
        {
            GetNextCharacterPartFromKeyword_startingFromNonParameter_params p = new GetNextCharacterPartFromKeyword_startingFromNonParameter_params(keyword, startFrom);

            if (smdict.GetNextCharacterPartFromKeyword_startingFromNonParameter.ContainsKey(p.GetHashCode()))
                return smdict.GetNextCharacterPartFromKeyword_startingFromNonParameter[p.GetHashCode()];
                
            
            for (int x = startFrom; x < keyword.Length; x++)
            {
                if (ZeroCodeUtil.TryStringMatch(keyword, x, "(?<"))
                {
                    string ret = keyword.Substring(startFrom, x - startFrom);

                    smdict.GetNextCharacterPartFromKeyword_startingFromNonParameter.Add(p.GetHashCode(), ret);

                    return ret;
                }

                if (ZeroCodeUtil.TryStringMatch(keyword, x, "(*")) // needs some clever tests ideas, if this is valid????
                {
                    string ret2 = keyword.Substring(startFrom, x - startFrom);

                    smdict.GetNextCharacterPartFromKeyword_startingFromNonParameter.Add(p.GetHashCode(), ret2);

                    return ret2;
                }
            }

            string ret3 = keyword.Substring(startFrom);

            smdict.GetNextCharacterPartFromKeyword_startingFromNonParameter.Add(p.GetHashCode(), ret3);

            return ret3;
        }

        public static bool IsStringOnlyWhiteSpaces(string s)
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

        public static List<string> TokenizeKeyword(string k, bool doNotCareAboutSub)
        {
            List<string> l = new List<string>();

            string current = "";

            for (int x = 0; x < k.Length; x++)
            {
                if (TryStringMatch(k, x, "(*"))
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
                if (TryStringMatch(k, x, "*)"))
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
                if (TryStringMatch(k, x, "(+"))
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
                if (TryStringMatch(k, x, "+)"))
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
                if (TryStringMatch(k, x, "(?<SUB>)"))
                {
                    x += 7;

                    current += "(?<SUB>)";
                }
                else
                if (TryStringMatch(k, x, "(?<"))
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
                if (TryStringMatch(k, x, ">)"))
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
