using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace m0.Lib.StdView
{
    public class Html
    {
        static IVertex FormalTextLanguages_Vertex = MinusZero.Instance.Root.Get(false, @"System\FormalTextLanguage");

        public static string DiagramQueryToDiagramId_internal(string query)
        {
            query = query.Replace(@"\", "-");
            query = query.Replace(@"{", "-");
            query = query.Replace(@"}", "-");
            query = query.Replace(@"'", "-");
            query = query.Replace(@":", "-");

            return query;
        }

        public static INoInEdgeInOutVertexVertex DiagramQueryToDiagramId(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex query_Vertex = GraphUtil.GetQueryOutFirst(stack, "Query", null);

            string query = GraphUtil.GetStringValue(query_Vertex);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            newStack.AddVertex(null, DiagramQueryToDiagramId_internal(query));

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex DequoteText(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;
            
            IVertex text_Vertex = GraphUtil.GetQueryOutFirst(stack, "text", null);            

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();
            
            newStack.AddVertex(null, HtmlUtil.DequoteString(GraphUtil.GetStringValue(text_Vertex)));

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex AddColorsToCode(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex FormalTextLanguage_Vertex = GraphUtil.GetQueryOutFirst(stack, "FormalTextLanguage", null);
            IVertex text_Vertex = GraphUtil.GetQueryOutFirst(stack, "text", null);

            string FormalTextLanguage = GraphUtil.GetStringValue(FormalTextLanguage_Vertex);
            string text = HtmlUtil.DequoteString(GraphUtil.GetStringValue(text_Vertex));

            IVertex ftl = GraphUtil.GetQueryOutFirst(FormalTextLanguages_Vertex, null, FormalTextLanguage);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            if (ftl == null)
                newStack.AddVertex(null, text);
            else
                newStack.AddVertex(null, AddColorsToCode_Process(ftl, text));

            return newStack;
        }

        static bool IsZeroCode;
        static bool IsInsideString;

        private static string AddColorsToCode_Process(IVertex ftl, string text)
        {
            if (GeneralUtil.CompareStrings(ftl, "ZeroCode"))
                IsZeroCode = true;
            else
                IsZeroCode = false;            

            FormalTextLanguageDictinaries dict = DictionariesForFormalTextLanguageFactory.Get(ftl);

            StringBuilder sb = new StringBuilder();

            sb.Append("<span style=\"color:#2B91AF\">");

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (IsZeroCode && c == '\"') // STRING
                {
                    //sb.Append("<span style=\"color:#6733D5\"><b>\"");                    
                    sb.Append("<span style=\"color:#9763E5\"><b>\"");

                    i++;

                    StringBuilder sb2 = new StringBuilder();

                    while (i < text.Length && text[i] != '\"') {
                        sb2.Append(text[i]);
                        i++;
                    }
                    
                    sb.Append(HtmlUtil.QuoteString(sb2.ToString()));

                    sb.Append("\"</b></span>");                        
                }
                else
                {
                    if (dict.viewTokensDictionary.ContainsKey(c))
                    {
                        List<ViewToken> vtl = dict.viewTokensDictionary[c];

                        ViewToken matchToken = ViewTokenMatch(vtl, text, i);

                        if (matchToken == null)
                            sb.Append(c);
                        else
                        {
                            bool IsBold = false;
                            if (GraphUtil.ExistQueryOut(matchToken.tokenVertex, "IsBold", "True"))
                                IsBold = true;

                            bool IsItalic = false;
                            if (GraphUtil.ExistQueryOut(matchToken.tokenVertex, "IsItalic", "True"))
                                IsItalic = true;

                            sb.Append("<span style=\"color:");
                            sb.Append(GetColor(matchToken.colorVertex));
                            sb.Append("\">");

                            if (IsBold)
                                sb.Append("<b>");

                            if (IsItalic)
                                sb.Append("<i>");

                            sb.Append(HtmlUtil.QuoteString(matchToken.tokenString));

                            if (IsBold)
                                sb.Append("</b>");

                            if (IsItalic)
                                sb.Append("</i>");

                            sb.Append("</span>");

                            i += matchToken.tokenString.Length - 1;
                        }
                    }
                    else
                        sb.Append(c);
                }
            }

            sb.Append("</span>");
            return sb.ToString();
        }

        private static string GetColor(IVertex colorVertex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("#");
            object val;
           
            GraphUtil.GetNumberValue(GraphUtil.GetQueryOutFirst(colorVertex, "Red", null), out val);
            sb.Append(Convert.ToInt16(val).ToString("X2"));

            GraphUtil.GetNumberValue(GraphUtil.GetQueryOutFirst(colorVertex, "Green", null), out val);
            sb.Append(Convert.ToInt16(val).ToString("X2"));

            GraphUtil.GetNumberValue(GraphUtil.GetQueryOutFirst(colorVertex, "Blue", null), out val);
            sb.Append(Convert.ToInt16(val).ToString("X2"));

            return sb.ToString();
        }

        private static ViewToken ViewTokenMatch(List<ViewToken> vtl_in, string text, int text_pos)
        {
            int token_pos = 0;            

            List<ViewToken> vtl = vtl_in;

            while (vtl.Count > 0)
            {
                List<ViewToken> vtl_next = new List<ViewToken>();

                foreach (ViewToken vt in vtl)                
                    if (token_pos + 1 == vt.tokenString.Length)
                        return vt;

                text_pos++;
                token_pos++;

                foreach (ViewToken vt in vtl)                
                    if (vt.tokenString.Length >= token_pos && vt.tokenString[token_pos] == text[text_pos])                    
                        vtl_next.Add(vt);                                    

                vtl = vtl_next;
            }

            return null;
        }
    }
}
