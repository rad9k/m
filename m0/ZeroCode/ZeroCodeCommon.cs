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

        public static string LineContinuationPrefix = "^";

        public static string CodeGraphLinkPrefix = "@";

        public static string getLinkString(string s)
        {
            return ZeroCodeCommon.CodeGraphLinkPrefix + s;
        }
        public static string getNewString(object o)
        {
            if (o == null)
                return "";

            string s=o.ToString();

            if (s.IndexOf('\\') != -1)
                s = s.Replace("\\", "\\\\");

            if (s.IndexOf('\"') != -1)
                s = s.Replace("\"", "\"");

            return '\"' + s + '\"';
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
