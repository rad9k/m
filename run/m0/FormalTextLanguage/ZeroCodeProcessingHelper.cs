using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.FormalTextLanguage
{
    public class ZeroCodeProcessingHelper
    {
        static IVertex defaultFormalTextLanguageProcessing = MinusZero.Instance.Root.Get(false, @"System\FormalTextLanguage\DefaultFormalTextLanguageProcessing:");

        public static string Generate(IEdge graphBaseEdge)
        {
            return Generate(defaultFormalTextLanguageProcessing, graphBaseEdge);
        }

        public static IVertex Parse(IEdge rootEdge, string text, out IEdge rootEdge_new)
        {
            return Parse(defaultFormalTextLanguageProcessing, rootEdge, text, out rootEdge_new);
        }

        public static string Generate(IVertex formalTextLanguageProcessing, IEdge graphBaseEdge)
        {
            return null;
        }

        public static IVertex Parse(IVertex formalTextLanguageProcessing, IEdge rootEdge, string text, out IEdge rootEdge_new)
        {
            rootEdge_new = null;

            IVertex GeneratorHandler = GraphUtil.GetQueryOutFirst(formalTextLanguageProcessing, "GeneratorHandler", null);
            IVertex ParserHandler = GraphUtil.GetQueryOutFirst(formalTextLanguageProcessing, "ParserHandler", null);
            IVertex LanguageParameterFirst = GraphUtil.GetQueryOutFirst(formalTextLanguageProcessing, "LanguageParameterFirst", null);
            IVertex LanguageParameterSecond = GraphUtil.GetQueryOutFirst(formalTextLanguageProcessing, "LanguageParameterSecond", null);

            //

            IVertex ParserExecutableEndPoint = GraphUtil.GetQueryOutFirst(ParserHandler, "$ExecutableEndpoint", null);

            string parser_type = GraphUtil.GetStringValueOrNull(GraphUtil.GetQueryOutFirst(ParserExecutableEndPoint, "DotNetTypename", null));
            string parser_methodName = GraphUtil.GetStringValueOrNull(GraphUtil.GetQueryOutFirst(ParserExecutableEndPoint, "DotNetMethodName", null));

            // Call the parser method
            Type parserType = Type.GetType(parser_type);
            if (parserType != null)
            {
                var parserMethod = parserType.GetMethod(parser_methodName);
                if (parserMethod != null && parserMethod.IsStatic)
                {
                    object[] parameters = new object[] { rootEdge, text, null };
                    object result = parserMethod.Invoke(null, parameters);
                    rootEdge_new = parameters[2] as IEdge;
                    return result as IVertex;
                }
            }

            //

            IVertex GeneratorExecutableEndPoint = GraphUtil.GetQueryOutFirst(GeneratorHandler, "$ExecutableEndpoint", null);

            string generator_type = GraphUtil.GetStringValueOrNull(GraphUtil.GetQueryOutFirst(GeneratorExecutableEndPoint, "DotNetTypename", null));
            string generator_methodName = GraphUtil.GetStringValueOrNull(GraphUtil.GetQueryOutFirst(GeneratorExecutableEndPoint, "DotNetMethodName", null));

            return null;
        }
    }
}
