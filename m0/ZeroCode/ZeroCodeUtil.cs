using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace m0.ZeroCode
{
    public class ZeroCodeUtil
    {

        public static string getRegexp(string s, string r)
        {
            Regex rgx = new Regex(r);

            foreach (Match match in rgx.Matches(s))
            {
                return match.Groups["EXTRACT"].Value;
            }

            return null;
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

        public static string getNextCharacterPartFromKeyword(string keyword, int startFrom)
        {
            int firstTryPos = getNextMatch(keyword, startFrom, "(*(+");

            int secondTryPos = getNextMatch(keyword, startFrom, "(?<");

            if(firstTryPos==-1 && secondTryPos==-1)
                return keyword.Substring(startFrom);

            if(secondTryPos == -1)
                return keyword.Substring(startFrom, firstTryPos - startFrom);

            if(firstTryPos == -1)
                return keyword.Substring(startFrom, secondTryPos - startFrom);

            return keyword.Substring(startFrom, Math.Min(firstTryPos,secondTryPos) - startFrom);
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
    }
}
