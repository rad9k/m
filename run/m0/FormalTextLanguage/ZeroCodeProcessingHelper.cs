using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace m0.FormalTextLanguage
{
    public class ZeroCodeProcessingHelper
    {
        static IVertex DefaultFormalTextLanguageProcessing = MinusZero.Instance.Root.Get(false, @"System\FormalTextLanguage\DefaultFormalTextLanguageProcessing:");

        public static string Generate(IEdge graphBaseEdge)
        {
            return Generate(DefaultFormalTextLanguageProcessing, graphBaseEdge);
        }

        public static IVertex Parse(IEdge rootEdge, string text, out IEdge rootEdge_new)
        {
            return Parse(DefaultFormalTextLanguageProcessing, rootEdge, text, out rootEdge_new);
        }

        public static string Generate(IVertex formalTextLanguageProcessing, IEdge graphBaseEdge)
        {
            IVertex GeneratorHandler = GraphUtil.GetQueryOutFirst(formalTextLanguageProcessing, "GeneratorHandler", null);
            
            //

            IVertex GeneratorExecutableEndPoint = GraphUtil.GetQueryOutFirst(GeneratorHandler, "$ExecutableEndPoint", null);

            string generator_type = GraphUtil.GetStringValueOrNull(GraphUtil.GetQueryOutFirst(GeneratorExecutableEndPoint, "DotNetTypeName", null));
            string generator_methodName = GraphUtil.GetStringValueOrNull(GraphUtil.GetQueryOutFirst(GeneratorExecutableEndPoint, "DotNetMethodName", null));

            // Call the generate method
            Type generatorType = Type.GetType(generator_type);
            if (generatorType != null)
            {
                var generatorMethod = generatorType.GetMethod(generator_methodName);
                if (generatorMethod != null && generatorMethod.IsStatic)
                {
                    object[] parameters = new object[] { formalTextLanguageProcessing, graphBaseEdge };
                    return (string) generatorMethod.Invoke(null, parameters);
                }
            }

            return null;
        }

        public static IVertex Parse(IVertex formalTextLanguageProcessing, IEdge rootEdge, string text, out IEdge rootEdge_new)
        {
            rootEdge_new = null;

            IVertex ParserHandler = GraphUtil.GetQueryOutFirst(formalTextLanguageProcessing, "ParserHandler", null);
        
            //

            IVertex ParserExecutableEndPoint = GraphUtil.GetQueryOutFirst(ParserHandler, "$ExecutableEndPoint", null);

            string parser_type = GraphUtil.GetStringValueOrNull(GraphUtil.GetQueryOutFirst(ParserExecutableEndPoint, "DotNetTypeName", null));
            string parser_methodName = GraphUtil.GetStringValueOrNull(GraphUtil.GetQueryOutFirst(ParserExecutableEndPoint, "DotNetMethodName", null));

            // Call the parser method
            Type parserType = Type.GetType(parser_type);
            if (parserType != null)
            {
                var parserMethod = parserType.GetMethod(parser_methodName);
                if (parserMethod != null && parserMethod.IsStatic)
                {
                    object[] parameters = new object[] { formalTextLanguageProcessing, rootEdge, text, null };
                    object result = parserMethod.Invoke(null, parameters);
                    rootEdge_new = parameters[3] as IEdge;
                    return result as IVertex;
                }
            }

            return null;
        }
    }
}
