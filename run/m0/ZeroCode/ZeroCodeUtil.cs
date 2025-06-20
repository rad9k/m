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
using System.Runtime.CompilerServices;

namespace m0.ZeroCode
{
    public class ZComparer<Type> : IEqualityComparer<Type>
    {
        public bool Equals(Type x, Type y)
        {
            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(Type obj)
        {
            return obj.GetHashCode();
        }
    }


    public class TryStringMatch_params
    {
        public zstring s;
        public int pos;
        public zstring toMatch;

        public TryStringMatch_params(zstring _s, int _pos, zstring _toMatch)
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

            result += pos;

            result *= 397;
            
            result += toMatch.GetHashCode();

            return result;
        }
    }

    class TabRemove_tryStringMatch_params
    {
        public zstring s;
        public int pos;
        public zstring toMatch;
        public int toRemoveTabs;

        public TabRemove_tryStringMatch_params(zstring s, int pos, zstring toMatch, int toRemoveTabs)
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

            result += pos;

            result *= 397;

            result += toMatch.GetHashCode();

            result *= 397;

            result += toRemoveTabs;

            return result;
        }
    }

    class DoTextRangeContainString_params
    {
        public zstring s;
        public int beg;
        public int end;
        public zstring toMatch;

        public DoTextRangeContainString_params(zstring s, int beg, int end, zstring toMatch)
        {
            this.s = s;
            this.beg = beg;
            this.end = end;
            this.toMatch = toMatch;
        }

        public override int GetHashCode()
        {
            int result = 37;

            result *= 397;

            result += s.GetHashCode();

            result *= 397;

            result += beg;

            result *= 397;

            result += end;

            result *= 397;

            result += toMatch.GetHashCode();

            return result;
        }
    }

    class TryStringEndMatch_params
    {
        public zstring s;
        public zstring toMatch;

        public TryStringEndMatch_params(zstring s, zstring toMatch)
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
        public zstring s;
        public int startFrom;
        public zstring toMatch;

        public GetNextMatch_params(zstring s, int startFrom, zstring toMatch)
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

            result += startFrom;

            result *= 397;

            result += toMatch.GetHashCode();            

            return result;
        }
    }

    class GetNextMatch_twoAtOnce_params
    {
        public zstring s;
        public int startFrom;
        public zstring toMatch1;
        public zstring toMatch2;

        public GetNextMatch_twoAtOnce_params(zstring s, int startFrom, zstring toMatch1, zstring toMatch2)
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

            result += startFrom;

            result *= 397;

            result += toMatch1.GetHashCode();

            result *= 397;

            result += toMatch2.GetHashCode();

            return result;
        }
    }

    class GetNextCharacterPartFromKeyword_startingFromNonParameter_params
    {
        public zstring keyword;
        public int startFrom;

        public GetNextCharacterPartFromKeyword_startingFromNonParameter_params(zstring keyword, int startFrom)
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

            result += startFrom;

            return result;
        }
    }

    class StringMatchingDictionary
    {
        /*public Dictionary<TryStringMatch_params, bool> TryStringMatch = 
            new Dictionary<TryStringMatch_params, bool>(new ZComparer<TryStringMatch_params>());

        public Dictionary<TabRemove_tryStringMatch_params, bool> TabRemove_tryStringMatch = 
            new Dictionary<TabRemove_tryStringMatch_params, bool>(new ZComparer<TabRemove_tryStringMatch_params>());

        public Dictionary<DoTextRangeContainString_params, bool> DoTextRangeContainString = 
            new Dictionary<DoTextRangeContainString_params, bool>(new ZComparer<DoTextRangeContainString_params>());

        public Dictionary<TryStringEndMatch_params, bool> TryStringEndMatch = 
            new Dictionary<TryStringEndMatch_params, bool>(new ZComparer<TryStringEndMatch_params>());

        public Dictionary<GetNextMatch_params, int> GetNextMatch = 
            new Dictionary<GetNextMatch_params, int>(new ZComparer<GetNextMatch_params>());

        public Dictionary<GetNextMatch_twoAtOnce_params, int> GetNextMatch_twoAtOnce = 
            new Dictionary<GetNextMatch_twoAtOnce_params, int>(new ZComparer<GetNextMatch_twoAtOnce_params>());

        public Dictionary<GetNextCharacterPartFromKeyword_startingFromNonParameter_params, string> GetNextCharacterPartFromKeyword_startingFromNonParameter = 
            new Dictionary<GetNextCharacterPartFromKeyword_startingFromNonParameter_params, string>(new ZComparer<GetNextCharacterPartFromKeyword_startingFromNonParameter_params>());
        */


        public Dictionary<int, bool> TryStringMatch =
         new Dictionary<int, bool>();

        public Dictionary<int, bool> TabRemove_tryStringMatch =
            new Dictionary<int, bool>();

        public Dictionary<int, bool> DoTextRangeContainString =
            new Dictionary<int, bool>();

        public Dictionary<int, bool> TryStringEndMatch =
            new Dictionary<int, bool>();

        public Dictionary<int, int> GetNextMatch =
            new Dictionary<int, int>();

        public Dictionary<int, int> GetNextMatch_twoAtOnce =
            new Dictionary<int, int>();

        public Dictionary<int, string> GetNextCharacterPartFromKeyword_startingFromNonParameter =
            new Dictionary<int, string>();

        public Dictionary<int, Dictionary<int, int>> GetNextMatch_not_found_dictionary = 
            new Dictionary<int, Dictionary<int, int>>();
    }

    public class ZeroCodeUtil
    {
        static StringMatchingDictionary smdict = new StringMatchingDictionary();

        public static string CRLF = "\r\n";

        public static void ClearZeroCodeUtilDicionaries()
        {
            smdict.DoTextRangeContainString.Clear();
            smdict.GetNextCharacterPartFromKeyword_startingFromNonParameter.Clear();
            smdict.GetNextMatch.Clear();
            smdict.GetNextMatch_twoAtOnce.Clear();
            smdict.TabRemove_tryStringMatch.Clear();
            smdict.TryStringEndMatch.Clear();
            smdict.TryStringMatch.Clear();

            smdict.GetNextMatch_not_found_dictionary.Clear();
        }

        // Z-version
        public static bool TryStringMatch(zstring s, int pos, zstring toMatch)
        {
            /* TryStringMatch_params p = new TryStringMatch_params(s, pos, toMatch);

             int h = p.GetHashCode();

             if (smdict.TryStringMatch.ContainsKey(h))
             {
                 TryStringMatch_match++;
                 return smdict.TryStringMatch[h];
             }
             else
                 TryStringMatch_nomatch++;
            */
            int toMatchLength = toMatch.Length;

            if (s.Length < pos + toMatchLength)
            {
                //smdict.TryStringMatch.Add(h, false);
                return false;
            }

            for (int x = 0; x < toMatchLength; x++)
                if (s[pos + x] != toMatch[x])
                {
                    //    smdict.TryStringMatch.Add(h, false);
                    return false;
                }

            // smdict.TryStringMatch.Add(h, true);
            return true;
        }

        public static int GetNextMatch(zstring s, int startFrom, zstring toMatch)
        {
            GetNextMatch_params p = new GetNextMatch_params(s, startFrom, toMatch);

            int h = p.GetHashCode();

            if (smdict.GetNextMatch.ContainsKey(h))
                return smdict.GetNextMatch[h];

            // NOT FOUND DICT START
            Dictionary<int, int> dict_for_s = null;

            if (smdict.GetNextMatch_not_found_dictionary.ContainsKey(s.GetHashCode()))
            {
                dict_for_s = smdict.GetNextMatch_not_found_dictionary[s.GetHashCode()];

                if (dict_for_s.ContainsKey(toMatch.GetHashCode()))
                {
                    int lastNotSeen = dict_for_s[toMatch.GetHashCode()];

                    if (lastNotSeen <= startFrom)
                        return -1;
                }

            }
            // NOT FOUND DICT STOP

            int pos = startFrom;

            while ((pos + toMatch.Length) <= s.Length)
            {
                if (TryStringMatch(s, pos, toMatch))
                {
                    smdict.GetNextMatch.Add(h, pos);
                    return pos;
                }

                pos++;
            }

            smdict.GetNextMatch.Add(h, -1);

            // NOT FOUND DICT START
            dict_for_s = null;

            if (smdict.GetNextMatch_not_found_dictionary.ContainsKey(s.GetHashCode()))
                dict_for_s = smdict.GetNextMatch_not_found_dictionary[s.GetHashCode()];
            else
            {
                dict_for_s = new Dictionary<int, int>();
                smdict.GetNextMatch_not_found_dictionary.Add(s.GetHashCode(), dict_for_s);
            }

            if (dict_for_s.ContainsKey(toMatch.GetHashCode()))
                dict_for_s[toMatch.GetHashCode()] = startFrom;
            else
                dict_for_s.Add(toMatch.GetHashCode(), startFrom);
            // NOT FOUND DICT STOP

            return -1;
        }

        public static bool TryStringEndMatch(zstring s, zstring toMatch)
        {
            TryStringEndMatch_params p = new TryStringEndMatch_params(s, toMatch);

            int h = p.GetHashCode();

            if (smdict.TryStringEndMatch.ContainsKey(h))
                return smdict.TryStringEndMatch[h];

            int sLength = s.Length;

            int toMatchLength = toMatch.Length;

            if (s.Length < toMatchLength)
            {
                smdict.TryStringEndMatch.Add(h, false);
                return false;
            }

            for (int x = 1; x <= toMatch.Length; x++)
                if (s[sLength - x] != toMatch[toMatchLength - x])
                {
                    smdict.TryStringEndMatch.Add(h, false);
                    return false;
                }

            smdict.TryStringEndMatch.Add(h, true);
            return true;
        }

        public static bool DoTextRangeContainString(zstring s, int beg, int end, zstring toMatch)
        {
            DoTextRangeContainString_params p = new DoTextRangeContainString_params(s, beg, end, toMatch);

            int h = p.GetHashCode();

            if (smdict.DoTextRangeContainString.ContainsKey(h))
                return smdict.DoTextRangeContainString[h];


            int cnt;

            for (cnt = beg; cnt + toMatch.Length - 1 <= end; cnt++)
                if (TryStringMatch(s, cnt, toMatch))
                {
                    smdict.DoTextRangeContainString.Add(h, true);
                    return true;
                }

            smdict.DoTextRangeContainString.Add(h, false);
            return false;
        }

        public static bool TabRemove_tryStringMatch(zstring s, int pos, zstring toMatch, int toRemoveTabs)
        {
            TabRemove_tryStringMatch_params p = new TabRemove_tryStringMatch_params(s, pos, toMatch, toRemoveTabs);

            int h = p.GetHashCode();

            if (smdict.TabRemove_tryStringMatch.ContainsKey(h))
                return smdict.TabRemove_tryStringMatch[h];

            int toMatchLength = toMatch.Length;

            if (s.Length < pos + toMatchLength)
            {
                smdict.TabRemove_tryStringMatch.Add(h, false);
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
                    smdict.TabRemove_tryStringMatch.Add(h, false);
                    return false;
                }

                if (s[pos + x + tabPhase] != toMatch[x])
                {
                    smdict.TabRemove_tryStringMatch.Add(h, false);
                    return false;
                }
            }

            smdict.TabRemove_tryStringMatch.Add(h, true);
            return true;
        }

        public static int GetNextMatch_twoAtOnce(zstring s, int startFrom, zstring toMatch1, zstring toMatch2)
        {
            GetNextMatch_twoAtOnce_params p = new GetNextMatch_twoAtOnce_params(s, startFrom, toMatch1, toMatch2);

            int h = p.GetHashCode();

            if (smdict.GetNextMatch_twoAtOnce.ContainsKey(h))
                return smdict.GetNextMatch_twoAtOnce[h];

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
                        smdict.GetNextMatch_twoAtOnce.Add(h, 1);
                        return 1;
                    }

                    if (canCheck2 && TryStringMatch(s, pos, toMatch2) && toMatch2.Length > 0)
                    {
                        smdict.GetNextMatch_twoAtOnce.Add(h, 2);
                        return 2;
                    }
                }

                pos++;
            }

            smdict.GetNextMatch_twoAtOnce.Add(h, 0);
            return 0;
        }

        static zstring bracket_question_anglebracket = new zstring("(?<");

        static zstring bracket_star = new zstring("(*");

        public static string GetNextCharacterPartFromKeyword_startingFromNonParameter(zstring keyword, int startFrom)
        {
            GetNextCharacterPartFromKeyword_startingFromNonParameter_params p = new GetNextCharacterPartFromKeyword_startingFromNonParameter_params(keyword, startFrom);

            int h = p.GetHashCode();

            if (smdict.GetNextCharacterPartFromKeyword_startingFromNonParameter.ContainsKey(h))
                return smdict.GetNextCharacterPartFromKeyword_startingFromNonParameter[h];

            for (int x = startFrom; x < keyword.Length; x++)
            {
                if (ZeroCodeUtil.TryStringMatch(keyword, x, bracket_question_anglebracket))
                {
                    string ret = keyword.Substring(startFrom, x - startFrom).ToString();

                    smdict.GetNextCharacterPartFromKeyword_startingFromNonParameter.Add(h, ret);

                    return ret;
                }

                if (ZeroCodeUtil.TryStringMatch(keyword, x, bracket_star)) // needs some clever tests ideas, if this is valid???? I do not know 2024-03-16 but it workx, althoght I did not checked this line :)
                {
                    string ret2 = keyword.Substring(startFrom, x - startFrom).ToString();

                    smdict.GetNextCharacterPartFromKeyword_startingFromNonParameter.Add(h, ret2);

                    return ret2;
                }
            }

            string ret3 = keyword.Substring(startFrom).ToString();

            smdict.GetNextCharacterPartFromKeyword_startingFromNonParameter.Add(h, ret3);

            return ret3;
        }

        // STANDARD versions

        public static bool TryStringMatch(string s, int pos, string toMatch)
        {
            int toMatchLength = toMatch.Length;

            if (s.Length < pos + toMatchLength)
                return false;

            for (int x = 0; x < toMatchLength; x++)
                if (s[pos + x] != toMatch[x])
                    return false;

            return true;
        }

        /////        

        public static bool FilterEdgeForGraph2TextProcessing(IEdge toFilterEdge)
        {
            if (GeneralUtil.CompareStrings(toFilterEdge.Meta, "$GraphChangeTrigger"))
                return false;

            return true;
        }

        public static IDictionary<string, IList<IVertex>> GetFilteredKeywordListByGroup(IVertex FormalTextLanguage, string metaFilter)
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
            bool shallProcess = true;

            while (shallProcess)
            {
                if (pos >= s.Length)
                    return -1;

                if (IsCRorLF(s[pos]))
                    return pos;

                pos++;
            }

            return -1; // no hit @here
        }

        public static bool IsCRorLF(char c)
        {
            if (c == '\r' || c == '\n')
                return true;

            return false;
        }

        public static bool IsCRLF(string s, int pos)
        {
            if ((pos + 1) < s.Length && s[pos] == '\r' && s[pos + 1] == '\n')
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
            while (s[pos] == ' ' || s[pos] == '\t')
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


            secondPart = query.Substring(slashPos + 1, query.Length - slashPos - 1);

            firstPart = query.Substring(0, slashPos);
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
