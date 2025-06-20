using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
 
string | NewVertexString
"      | \"
\      | \\

    where
    " dict.NewVertexPrefix or NewVertexPrefix
    \ dict.EscapeCharacter

string | Ecspaped
'      | \'
\      | \\

    where
    ' dict.EscapedSequencePrefix or EscapedSequencePrefix
    \ dict.EscapeCharacter


*/

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


        public static char forbidden = '\u0007'; // bell we can use NUL also
/* kept for the reference
        public static string CRLFoperator = "{";

        public static string MetaSeparator = ":";

        public static string CodeGraphVertexPrefix = "<";

        public static string CodeGraphVertexSuffix = ">";

        public static char LineContinuationPrefix = '^';

        public static char c = '@';

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
    */
        // Link
        ///////


        public static bool IsSpecialCharacter(FormalTextLanguageDictinaries dict, string s, int pos)
        {
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.CRLFoperator))
                return true;
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.MetaSeparator))
                return true;
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.CodeGraphVertexPrefix))
                return true; 
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.CodeGraphVertexSuffix))
                return true; 
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.LineContinuationPrefix.ToString()))
                return true; 
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.CodeGraphLinkPrefix.ToString()))
                return true; 
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.CodeGraphLinkKeywordPrefix))
                return true; 
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.NewVertexPrefix.ToString()))
                return true; 
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.NewVertexSuffix.ToString()))
                return true; 
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.EscapedSequencePrefix.ToString()))
                return true; 
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.EscapedSequenceSuffix.ToString()))
                return true; 
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.EscapeCharacter.ToString()))
                return true; 
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.SetIndexPrefix))
                return true; 
            if (ZeroCodeUtil.TryStringMatch(s, pos, dict.SetIndexPostfix))
                return true;

            char c = s[pos];

            if(dict.allKeywordsSubstringsDictionary_onlyFirstPart.ContainsKey(c)) // XXX TURNED ON was: // XXX TURNED OFF IN SAKE OF IN
                foreach (string cs in dict.allKeywordsSubstringsDictionary_onlyFirstPart[c])
                    if (ZeroCodeUtil.TryStringMatch(s, pos, cs))
                        return true;

            return false;
        }
        public static string stringToLinkString(FormalTextLanguageDictinaries dict, string s, bool hideLinkPrefix)
        {
            if (hideLinkPrefix)
                return s;
            else
                return dict.CodeGraphLinkPrefix + s;
        }

        public static string stringFromLinkString(FormalTextLanguageDictinaries dict, string s, bool hideLinkPrefix)
        {
            if (hideLinkPrefix)
                return s;
            else
                return s.Substring(dict.CodeGraphLinkPrefix.ToString().Length);
        }

        public static string stringFromLinkKeywordString(FormalTextLanguageDictinaries dict, string s)
        {
            return s.Substring(dict.CodeGraphLinkKeywordPrefix.ToString().Length).TrimEnd();
        }

        // to be used only in ZeroCodeCommon.stringFromLinkString( , FALSE) scenario
        // and that means that TO BE USED ONLY IN KEYWORDS
        public static string tryStringFromLinkString(FormalTextLanguageDictinaries dict, string text, int startPos, ref int pos, int endPos, IDictionary<char, List<string>> allKeywordsSubstringsDictionary)
        {
            string newVertex = null;

            int sPos = startPos;

            bool shallProceed = true;

            if (ZeroCodeUtil.TryStringMatch(text, startPos, dict.CodeGraphLinkPrefix.ToString()))
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

                    if (text[sPos] == dict.EscapedSequencePrefix && !isInEscape)
                        isInEscape = true;

                    if (text[sPos] == dict.EscapedSequenceSuffix && isInEscape
                        && sPos > 0 && text[sPos - 1] != dict.EscapeCharacter) // if is no \'
                        isInEscape = false;
                }

                pos = sPos;

                newVertex = ZeroCodeCommon.stringFromLinkString(dict, text.Substring(startPos, sPos - startPos), false);
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
                if (ZeroCodeUtil.TryStringMatch(text, startPos, s))
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
                                    if (startPos-back > 0 && ZeroCodeUtil.TryStringMatch(text, startPos - back, ss))
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

        public static bool isLinkString(FormalTextLanguageDictinaries dict, string s, int beg)
        {
            if (s[beg] == dict.CodeGraphLinkPrefix && s.Length>=beg && s[beg + 1] != dict.CodeGraphLinkPrefix) // @@ support
                return true;

            return false;
        }

        public static bool isLinkKeywordString(FormalTextLanguageDictinaries dict, string s, int beg)
        {
            for (int x = 0; x < dict.CodeGraphLinkKeywordPrefix.Length; x++)
                if (s[beg + x] != dict.CodeGraphLinkKeywordPrefix[x])
                    return false;

            return true;
        }        

        //  NewVertexString
        ///////////////////

        public static string stringToNewVertexString(FormalTextLanguageDictinaries dict, object o)
        {
            if (o == null)
                return "";

            string s=o.ToString();

            s = s.Replace(dict.EscapeCharacter.ToString(), dict.EscapeCharacter.ToString() + dict.EscapeCharacter.ToString());

            //s = s.Replace(dict.EscapeCharacter.ToString(), forbidden.ToString() + forbidden.ToString());

            s = s.Replace(dict.NewVertexPrefix.ToString(), dict.EscapeCharacter.ToString() + dict.NewVertexPrefix);            

            //s = s.Replace(dict.NewVertexPrefix.ToString(), forbidden.ToString() + dict.NewVertexPrefix);

            if (dict.NewVertexPrefix != dict.NewVertexSuffix)
                s = s.Replace(dict.NewVertexSuffix.ToString(), dict.EscapeCharacter.ToString() + dict.NewVertexSuffix);
                //s = s.Replace(dict.NewVertexSuffix.ToString(), forbidden.ToString() + dict.NewVertexSuffix);

            s = s.Replace(forbidden, dict.EscapeCharacter);

            return dict.NewVertexPrefix + s + dict.NewVertexSuffix;
        }

        public static string stringFromNewVertexString(FormalTextLanguageDictinaries dict, string s)
        {
            s = s.Substring(1, s.Length - 2);                       

            s = s.Replace(String.Concat(dict.EscapeCharacter, dict.NewVertexPrefix), dict.NewVertexPrefix.ToString());
            s = s.Replace(String.Concat(dict.EscapeCharacter, dict.NewVertexSuffix), dict.NewVertexSuffix.ToString());

            s = s.Replace(String.Concat(dict.EscapeCharacter, dict.EscapeCharacter), dict.EscapeCharacter.ToString());

            return s;
        }

        public static string tryStringFromNewVertexString(FormalTextLanguageDictinaries dict, string text, int startPos, ref int pos)
        {
            string newVertex = null;

            int sPos = startPos;

            bool shallProceed = true;            

            if (ZeroCodeUtil.TryStringMatch(text, sPos, dict.NewVertexPrefix.ToString()))
            {
                int escapeCharacterCount = 0;

                while (shallProceed)
                {                    
                    sPos++;

                    if (sPos>0 && text[sPos-1] == dict.EscapeCharacter)
                        escapeCharacterCount++;
                    else
                        escapeCharacterCount = 0;

                    if (escapeCharacterCount == 2)
                        escapeCharacterCount = 0;                    

                    if (text[sPos] == dict.NewVertexSuffix && escapeCharacterCount!=1) 
                        shallProceed = false;
                }

                pos = sPos + 1;

                newVertex = ZeroCodeCommon.stringFromNewVertexString(dict, text.Substring(startPos, sPos - startPos + 1));
            }

            return newVertex;
        }

        public static bool isNewVertexString(FormalTextLanguageDictinaries dict, string s, int beg, int end)
        {
            if (s[beg] == dict.NewVertexPrefix && s[end] == dict.NewVertexSuffix)
                return true;

            return false;
        }
        public static bool isNewVertexString(FormalTextLanguageDictinaries dict, string s)
        {
            if (s.Length > 0
                && s[0] == dict.NewVertexPrefix
                && s[s.Length - 1] == dict.NewVertexSuffix)
                return true;

            return false;
        }

        // Escaped
        //////////

        public static string surroundWithEscape(FormalTextLanguageDictinaries dict, string s)
        {
            return dict.EscapedSequencePrefix + s + dict.EscapedSequenceSuffix;
        }

        public static string stringToPossiblyEscapedString(FormalTextLanguageDictinaries dict, object o)
        {
            if (o == null)
                return "";

            string s = o.ToString();

            bool needToSurroundWithEscape = false;

            if (s.IndexOf(' ') != -1)
                needToSurroundWithEscape = true;

            // XXX need to reference dict.allKeywordsSubstringsDictionary

            //if (s.IndexOf('\\') != -1)
            if (s.IndexOf(dict.EscapeCharacter) != -1)
            {
                //s = s.Replace("\\", "\\\\");
                s = s.Replace(dict.EscapeCharacter.ToString(), String.Concat(dict.EscapeCharacter, dict.EscapeCharacter));
                
                needToSurroundWithEscape = true;
            }

            //if (s.IndexOf('\'') != -1)
            if (s.IndexOf(dict.EscapedSequencePrefix) != -1)
            {
                //s = s.Replace("'", "\\'");
                s = s.Replace(dict.EscapedSequencePrefix.ToString(),String.Concat(dict.EscapeCharacter, dict.EscapedSequencePrefix));
                needToSurroundWithEscape = true;
            }

            for (int x = 0; x < s.Length; x++)
                if (IsSpecialCharacter(dict, s, x))
                    needToSurroundWithEscape = true;

        //    if (s.IndexOf('<') != -1 || s.IndexOf('>') != -1) // XXX
           //         needToSurroundWithEscape = true;

            if (needToSurroundWithEscape)
                return surroundWithEscape(dict, s);
            else
                return s;

        }        

        public static string tryEscapedLinkStringAndDeescape(FormalTextLanguageDictinaries dict, string text, ref int sPos)
        {         
            int begSpos = sPos;

            if (text[sPos] != dict.EscapedSequencePrefix)
                return null;

            bool canProceed=false;
            do
            {
                sPos++;

                canProceed = true;

                if (text.Length > sPos && text[sPos - 1] == dict.EscapeCharacter && text[sPos] == dict.EscapedSequenceSuffix)
                {

                }
                else if (text.Length <= sPos || text[sPos] == dict.EscapedSequenceSuffix || text[sPos] == '\r' || text[sPos] == '\n')
                    canProceed = false;

            } while (canProceed);

            if (text[sPos] == dict.EscapedSequenceSuffix)
            {
                sPos++;

                string descaped = text.Substring(begSpos + 1, sPos - begSpos - 2);

                descaped = descaped.Replace(String.Concat(dict.EscapeCharacter, dict.EscapeCharacter), dict.EscapeCharacter.ToString());                

                descaped = descaped.Replace(String.Concat(dict.EscapeCharacter, dict.EscapedSequencePrefix), dict.EscapedSequencePrefix.ToString());

                return descaped;
            }

            return null;
        }

        //////////////////////
    }
}
