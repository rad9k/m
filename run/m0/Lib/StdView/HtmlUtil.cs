using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Lib.StdView
{
    public class HtmlUtil
    {
        public static string DequoteString(string _textValue)
        {
            StringBuilder textValue = new StringBuilder(_textValue);

            textValue.Replace("&amp;", "&");
            textValue.Replace("&lt;", "<");
            textValue.Replace("&gt;", ">");
            textValue.Replace("&quot;", "\"");
            textValue.Replace("&apos;", "\"");
            textValue.Replace("&grave;", "`");
            textValue.Replace("&tilde;", "~");
            textValue.Replace("&circ;", "^");
            textValue.Replace("&verbar;", "|");
            textValue.Replace("&bsol;", "\\");

            return textValue.ToString();
        }

        public static string QuoteString(string _textValue)
        {
            StringBuilder textValue = new StringBuilder(_textValue);

            textValue.Replace("&", "&amp;");
            textValue.Replace("<", "&lt;");
            textValue.Replace(">", "&gt;");
            textValue.Replace("\"", "&quot;");
            textValue.Replace("'", "&apos;");
            textValue.Replace("`", "&grave;");
            textValue.Replace("~", "&tilde;");
            textValue.Replace("^", "&circ;");
            textValue.Replace("|", "&verbar;");
            textValue.Replace("\\", "&bsol;");

            return textValue.ToString();
        }
    }
}
