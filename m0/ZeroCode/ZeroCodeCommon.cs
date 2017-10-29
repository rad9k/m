using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static string stringToLinkString(string s)
        {
            return ZeroCodeCommon.CodeGraphLinkPrefix + s;
        }
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

        public static string getEscapedString(string s)
        {
            return '\'' + s + '\'';
        }

        public static string tryEscape(object o)
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
                return getEscapedString(s);
            else
                return s;

        }

        //////////////////////
    }
}
