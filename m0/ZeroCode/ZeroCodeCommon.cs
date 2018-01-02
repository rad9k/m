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

        public static string CodeGraphVertexPrefix = "<";

        public static string CodeGraphVertexSuffix = ">";

        public static char LineContinuationPrefix = '^';

        public static char CodeGraphLinkPrefix = '@';

        public static char NewVertexPrefix = '\"';

        public static char NewVertexSuffix = '\"';

        public static char EscapePrefix = '\'';

        public static char EscapeSuffix = '\'';

        public static char EscapeCharacter = '\\';

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
                return s.Substring(1);
        }

        // to be used only in ZeroCodeCommon.stringFromLinkString( , FALSE) scenario
        // and that means that TO BE USED ONLY IN KEYWORDS
        public static string tryStringFromLinkString(string text, int startPos, ref int pos, int endPos)
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

                    if (sPos == endPos)
                        shallProceed = false;

                    if (text[sPos] == '\n' || text[sPos] == '\r')
                        shallProceed = false;

                    //if (text[sPos] == ' ' && !isInEscape)
                      //  shallProceed = false;

                    if (text[sPos] == '\'' && !isInEscape)
                        isInEscape = true;

                    if (text[sPos] == '\'' && isInEscape
                        && sPos > 0 && text[sPos - 1] != '\\') // if is no \'
                        isInEscape = false;
                }

                pos = sPos;

                newVertex = ZeroCodeCommon.stringFromLinkString(text.Substring(startPos, sPos - startPos), false);
            }

            return newVertex;
        }

        public static bool isLinkString(string s, int beg, int end)
        {
            if (s[beg] == CodeGraphLinkPrefix)
                return true;

            return false;
        }

        public static bool isLinkString(string s)
        {
            if (s.Length > 0
                && s[0] == ZeroCodeCommon.CodeGraphLinkPrefix)
                return true;

            return false;
        }

        //  NewVertexString
        ///////////////////

        public static string stringToNewVertexString(object o)
        {
            if (o == null)
                return "";

            string s=o.ToString();

            if (s.IndexOf('\\') != -1)
                s = s.Replace("\\", "\\\\");

            if (s.IndexOf('\"') != -1)
                s = s.Replace("\"", "\"");

            return NewVertexPrefix + s + NewVertexSuffix;
        }

        public static string stringFromNewVertexString(string s)
        {
            s = s.Substring(1, s.Length - 2);

            s = s.Replace("\\\"", "\"");

            s = s.Replace("\\\\", "\\");

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
            return EscapePrefix + s + EscapeSuffix;
        }

        public static string stringToPossiblyEscapedString(object o)
        {
            if (o == null)
                return "";

            string s=o.ToString();

            bool wasThereReplace = false;

            if (s.IndexOf(' ') != -1)
                wasThereReplace = true;

            if (s.IndexOf('\\') != -1)
            {
                s = s.Replace("\\", "\\\\");
                wasThereReplace = true;
            }

            if (s.IndexOf('\'') != -1)
            {
                s = s.Replace("'", "\\'");
                wasThereReplace = true;
            }

            if (wasThereReplace)
                return surroundWithEscape(s);
            else
                return s;

        }

        public static string stringFromEscapedString(string s)
        {
            return "";
        }

        //////////////////////
    }
}
