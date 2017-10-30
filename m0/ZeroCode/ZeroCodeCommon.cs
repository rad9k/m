using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// string                       NewVertexString
//      stringToNewVertexString
//      stringFromNewVertexString
//      isNewVertex
//                              Escaped
//      stringToPossiblyEscapedString
//      stringFromEscapedString
//                              Link
//      stringToLinkString
//      stringFromLinkString
//      isLink


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

        // Link
        ///////

        public static string stringToLinkString(string s)
        {
            return ZeroCodeCommon.CodeGraphLinkPrefix + s;
        }

        public static string stringFromLinkString(string s)
        {
            return s.Substring(1);
        }

        public static bool isLink(string s, int beg, int end)
        {
            if (s[beg] == CodeGraphLinkPrefix)
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

        public static bool isNewVertex(string s, int beg, int end)
        {
            if (s[beg] == NewVertexPrefix && s[end] == NewVertexSuffix)
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
