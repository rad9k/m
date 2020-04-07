using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// string                       NewVertexString
//      stringToNewVertexString
//      stringFromNewVertexString
//      tryStringFromNewVertexString
//      isNewVertexString
//                              Escaped
//      stringToPossiblyEscapedString
//      stringFromEscapedString
//                              Link
//      stringToLinkString
//      stringFromLinkString
//      tryStringFromLinkString
//      isLinkString

namespace m0.ZeroCode
{
    public class ZeroCodeCommon
    {

        ///////////////////////
        // core code style
        //////////////////////

        public static string CRLFoperator = "{";

        public static string MetaSeparator = ":";

        public static string CodeGraphVertexPrefix = "<";

        public static string CodeGraphVertexSuffix = ">";

        public static char LineContinuationPrefix = '^';

        public static char CodeGraphLinkPrefix = '@';

        public static string CodeGraphLinkKeywordPrefix = "@@"; // we store it here and in the textlanguage

        public static char NewVertexPrefix = '\"';

        public static char NewVertexSuffix = '\"';

        public static char EscapedSequencePrefix = '\'';

        public static char EscapedSequenceSuffix = '\'';

        public static char EscapeCharacter = '\\';

        public static string SetIndexPrefix = "<<";

        public static string SetIndexPostfix = ">>";

        public static HashSet<string> CodeViewTimeLinkKeywordParts = new HashSet<string>(new string[] { "\\", "{", "}", ":", "::", SetIndexPrefix, SetIndexPostfix, "," }); 
    // we store it here and in the textlanguage, but in general this is XXX. big question remins: how do you do cvtq while the code is in some different language?

        // Link
        ///////

        public static string stringToLinkString(string s, bool hideLinkPrefix)
        {
            if (hideLinkPrefix)
                return s;
            else
                return ZeroCodeCommon.CodeGraphLinkPrefix + s;
        }

        public static string stringFromLinkString(string s, bool hideLinkPrefix)
        {
            if (hideLinkPrefix)
                return s;
            else
                return s.Substring(ZeroCodeCommon.CodeGraphLinkPrefix.ToString().Length);
        }

        public static string stringFromLinkKeywordString(string s)
        {
            return s.Substring(ZeroCodeCommon.CodeGraphLinkKeywordPrefix.ToString().Length).TrimEnd();
        }

        // to be used only in ZeroCodeCommon.stringFromLinkString( , FALSE) scenario
        // and that means that TO BE USED ONLY IN KEYWORDS
        public static string tryStringFromLinkString(string text, int startPos, ref int pos, int endPos, IDictionary<char, List<string>> allKeywordsSubstringsDictionary)
        {
            string newVertex = null;

            int sPos = startPos;

            bool shallProceed = true;

            if (ZeroCodeUtil.tryStringMatch(text, startPos, ZeroCodeCommon.CodeGraphLinkPrefix.ToString()))
            {
                bool isInEscape = false;

                while (shallProceed)
                {
                    sPos++;

                    if (testIfIsKeywordSubstring(sPos, text, allKeywordsSubstringsDictionary, null))
                        shallProceed = false;

                    if (sPos == endPos)
                        shallProceed = false;

                    if (text[sPos] == '\n' || text[sPos] == '\r')
                        shallProceed = false;

                    if (text[sPos] == ' ' && !isInEscape)
                        shallProceed = false;

                    if (text[sPos] == EscapedSequencePrefix && !isInEscape)
                        isInEscape = true;

                    if (text[sPos] == EscapedSequenceSuffix && isInEscape
                        && sPos > 0 && text[sPos - 1] != EscapeCharacter) // if is no \'
                        isInEscape = false;
                }

                pos = sPos;

                newVertex = ZeroCodeCommon.stringFromLinkString(text.Substring(startPos, sPos - startPos), false);
            }

            return newVertex;
        }

        public static bool testIfIsKeywordSubstring(int startPos, string text, IDictionary<char, List<string>> keywordsSubstringsPositiveDictionary, IDictionary<char, List<string>> keywordsSubstringsNegativeDictionary)
        {
            char charAtPos = text[startPos];

            if (!keywordsSubstringsPositiveDictionary.ContainsKey(charAtPos))
                return false;

            List<string> l = keywordsSubstringsPositiveDictionary[charAtPos];

            foreach (string s in l)
                if (ZeroCodeUtil.tryStringMatch(text, startPos, s))
                {
                    if (keywordsSubstringsNegativeDictionary == null)
                        return true;
                    else
                    {
                        if (keywordsSubstringsNegativeDictionary.ContainsKey(charAtPos))
                        {
                            List<string> negList = keywordsSubstringsNegativeDictionary[charAtPos];

                            bool notFound = true;

                            foreach (string ss in negList)
                                for (int back = 0; back < ss.Length; back++)
                                {
                                    if (startPos-back > 0 && ZeroCodeUtil.tryStringMatch(text, startPos - back, ss))
                                        notFound = false;
                                }
                            

                            if (notFound)
                                return true;
                        }
                        else
                            return true;
                    }
                }

            return false;
        }

        public static bool isLinkString(string s, int beg)
        {
            if (s[beg] == CodeGraphLinkPrefix && s.Length>=beg && s[beg + 1] != CodeGraphLinkPrefix) // @@ support
                return true;

            return false;
        }

        public static bool isLinkKeywordString(string s, int beg)
        {
            for (int x = 0; x < CodeGraphLinkKeywordPrefix.Length; x++)
                if (s[beg + x] != CodeGraphLinkKeywordPrefix[x])
                    return false;

            return true;
        }        

        //  NewVertexString
        ///////////////////

        public static string stringToNewVertexString(object o)
        {
            if (o == null)
                return "";

            string s=o.ToString();

            if (s.IndexOf(EscapeCharacter) != -1)
                s = s.Replace(EscapeCharacter.ToString(), EscapeCharacter.ToString() + EscapeCharacter.ToString());

            
            s = s.Replace(NewVertexPrefix.ToString(), EscapeCharacter.ToString() + NewVertexPrefix);

            if(NewVertexPrefix!=NewVertexSuffix)
                s = s.Replace(NewVertexSuffix.ToString(), EscapeCharacter.ToString() + NewVertexSuffix);

            return NewVertexPrefix + s + NewVertexSuffix;
        }

        public static string stringFromNewVertexString(string s)
        {
            s = s.Substring(1, s.Length - 2);

            //s = s.Replace("\\\\", "\\");

            s = s.Replace(String.Concat(ZeroCodeCommon.EscapeCharacter, ZeroCodeCommon.EscapeCharacter), ZeroCodeCommon.EscapeCharacter.ToString());

            // s = s.Replace("\\\"", "\"");

            s = s.Replace(String.Concat(ZeroCodeCommon.EscapeCharacter,ZeroCodeCommon.NewVertexPrefix), ZeroCodeCommon.NewVertexPrefix.ToString());
            s = s.Replace(String.Concat(ZeroCodeCommon.EscapeCharacter, ZeroCodeCommon.NewVertexSuffix), ZeroCodeCommon.NewVertexSuffix.ToString());

            return s;
        }

        public static string tryStringFromNewVertexString(string text, int startPos, ref int pos)
        {
            string newVertex = null;

            int sPos = startPos;

            bool shallProceed = true;

            if (ZeroCodeUtil.tryStringMatch(text, sPos, ZeroCodeCommon.NewVertexPrefix.ToString()))
            {
                while (shallProceed)
                {
                    sPos++;

                    if (text[sPos] == ZeroCodeCommon.NewVertexSuffix
                        && sPos > 0 && text[sPos - 1] != ZeroCodeCommon.EscapeCharacter) // if is no \"
                        shallProceed = false;
                }

                pos = sPos + 1;

                newVertex = ZeroCodeCommon.stringFromNewVertexString(text.Substring(startPos, sPos - startPos + 1));
            }

            return newVertex;
        }

        public static bool isNewVertexString(string s, int beg, int end)
        {
            if (s[beg] == NewVertexPrefix && s[end] == NewVertexSuffix)
                return true;

            return false;
        }
        public static bool isNewVertexString(string s)
        {
            if (s.Length > 0
                && s[0] == ZeroCodeCommon.NewVertexPrefix
                && s[s.Length - 1] == ZeroCodeCommon.NewVertexSuffix)
                return true;

            return false;
        }

        // Escaped
        //////////

        public static string surroundWithEscape(string s)
        {
            return EscapedSequencePrefix + s + EscapedSequenceSuffix;
        }

        public static string stringToPossiblyEscapedString(object o)
        {
            if (o == null)
                return "";

            string s=o.ToString();

            bool needToSurroundWithEscape = false;

            if (s.IndexOf(' ') != -1)
                needToSurroundWithEscape = true;

            // XXX need to reference dict.allKeywordsSubstringsDictionary

            //if (s.IndexOf('\\') != -1)
            if (s.IndexOf(EscapeCharacter) != -1)
            {
                //s = s.Replace("\\", "\\\\");
                s = s.Replace(EscapeCharacter.ToString(), String.Concat(EscapeCharacter, EscapeCharacter));
                needToSurroundWithEscape = true;
            }

            //if (s.IndexOf('\'') != -1)
            if (s.IndexOf(EscapedSequencePrefix) != -1)
            {
                //s = s.Replace("'", "\\'");
                s = s.Replace(EscapedSequencePrefix.ToString(),String.Concat(EscapeCharacter,EscapedSequencePrefix));
                needToSurroundWithEscape = true;
            }

        //    if (s.IndexOf('<') != -1 || s.IndexOf('>') != -1) // XXX
           //         needToSurroundWithEscape = true;

            if (needToSurroundWithEscape)
                return surroundWithEscape(s);
            else
                return s;

        }

        internal static string tryEscapedLinkString(string text, ref int sPos)
        {
            int begSpos = sPos;

            if (text[sPos] != ZeroCodeCommon.EscapedSequencePrefix)
                return null;

            bool canProceed=false;
            do
            {
                sPos++;

                canProceed = true;

                if (text.Length > sPos && text[sPos - 1] == ZeroCodeCommon.EscapeCharacter && text[sPos] == ZeroCodeCommon.EscapedSequenceSuffix)
                {

                }
                else if (text.Length <= sPos || text[sPos] == ZeroCodeCommon.EscapedSequenceSuffix || text[sPos] == '\r' || text[sPos] == '\n')
                    canProceed = false;

            } while (canProceed);

            if (text[sPos] == ZeroCodeCommon.EscapedSequenceSuffix)
            {
                sPos++;
                return text.Substring(begSpos + 1, sPos - begSpos - 2);
            }

            return null;
        }

        //////////////////////
    }
}
