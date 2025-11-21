using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Lib.StdView
{
    public class Html
    {
        static IVertex FormalTextLanguages_Vertex = MinusZero.Instance.Root.Get(false, @"System\FormalTextLanguage");

        static string DequoteText(string _textValue)
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

        public static INoInEdgeInOutVertexVertex AddColorsToCode(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex FormalTextLanguage_Vertex = GraphUtil.GetQueryOutFirst(stack, "FormalTextLanguage", null);
            IVertex text_Vertex = GraphUtil.GetQueryOutFirst(stack, "text", null);

            string FormalTextLanguage = GraphUtil.GetStringValue(FormalTextLanguage_Vertex);
            string text = DequoteText(GraphUtil.GetStringValue(text_Vertex));

            IVertex ftl = GraphUtil.GetQueryOutFirst(FormalTextLanguages_Vertex, null, FormalTextLanguage);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            if (ftl == null)
                newStack.AddVertex(null, text);
            else
                newStack.AddVertex(null, AddColorsToCode_Process(ftl, text));

            return newStack;
        }

        private static string AddColorsToCode_Process(IVertex ftl, string text)
        {
            FormalTextLanguageDictinaries dict = DictionariesForFormalTextLanguageFactory.Get(ftl);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (dict.viewTokensDictionary.ContainsKey(c))
                {
                    List<ViewToken> vtl = dict.viewTokensDictionary[c];

                    ViewToken matchToken = ViewTokenMatch(vtl, text, i);

                    if (matchToken == null)
                        sb.Append(c);
                    else
                    {
                        sb.Append("<span style=\"color:");
                        sb.Append(GetColor(matchToken.colorVertex));
                        sb.Append("\">");
                        sb.Append(matchToken.tokenString);
                        sb.Append("</span>");

                        i+=matchToken.tokenString.Length - 1;
                    }
                }
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }

        private static string GetColor(IVertex colorVertex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("#");
            object val;
           
            GraphUtil.GetNumberValue(GraphUtil.GetQueryOutFirst(colorVertex, "Red", null), out val);
            sb.Append(Convert.ToInt16(val).ToString("2X"));

            GraphUtil.GetNumberValue(GraphUtil.GetQueryOutFirst(colorVertex, "Green", null), out val);
            sb.Append(Convert.ToInt16(val).ToString("2X"));

            GraphUtil.GetNumberValue(GraphUtil.GetQueryOutFirst(colorVertex, "Blue", null), out val);
            sb.Append(Convert.ToInt16(val).ToString("2X"));

            return sb.ToString();
        }

        private static ViewToken ViewTokenMatch(List<ViewToken> vtl_in, string text, int text_pos)
        {
            int token_pos = 1;
            text_pos++;

            List<ViewToken> vtl = vtl_in;

            while (vtl.Count > 0)
            {
                List<ViewToken> vtl_next = new List<ViewToken>();

                foreach (ViewToken vt in vtl)
                {
                    if (vt.tokenString.Length > token_pos && vt.tokenString[token_pos] == text[text_pos])
                    {
                        if (token_pos + 1 == vt.tokenString.Length)
                            return vt;
                        else
                            vtl_next.Add(vt);
                    }
                }

                text_pos++;
                token_pos++;

                vtl = vtl_next;
            }

            return null;
        }
    }
}
