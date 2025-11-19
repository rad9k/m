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
        static IVertex FormalTextLanguages_Vertex = MinusZero.Instance.Root.Get(false, @"System\FormalTextLangueges");

        public static INoInEdgeInOutVertexVertex AddColorsToCode(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex FormalTextLanguage_Vertex = GraphUtil.GetQueryOutFirst(stack, "FormalTextLanguage", null);
            IVertex text_Vertex = GraphUtil.GetQueryOutFirst(stack, "text", null);

            string FormalTextLanguage = GraphUtil.GetStringValue(FormalTextLanguage_Vertex);
            string text = GraphUtil.GetStringValue(text_Vertex);

            IVertex ftl = GraphUtil.GetQueryOutFirst(FormalTextLanguages_Vertex, FormalTextLanguage, null);

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

                    string match = ViewTokenMatch(vtl, text, i);

                    if (match == null)
                        sb.Append(c);
                    else
                    {

                    }
                }
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }

        private static string ViewTokenMatch(List<ViewToken> vtl, string text, int i)
        {
            throw new NotImplementedException();
        }
    }
}
