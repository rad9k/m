using m0.FormalTextLanguage;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    public class ZeroCodeEngine : IFormalTextGenerator, IFormalTextParser, IExecuter
    {             
        ZeroCodeExecuter ZeroCodeExecuter_Instance;
        static Graph2TextProcessing ZeroCodeGraph2StringProcessing_Instance;
        
        static Dictionary<IVertex, Graph2TextProcessing> ZeroCodeGraph2StringProcessing_InstanceDictionary = new Dictionary<IVertex, Graph2TextProcessing>();

        public IVertex Execute(IVertex baseVertex, IVertex expression)
        {            
            return ZeroCodeExecuter_Instance.Execute(baseVertex, expression);
        }

        public IVertex Get(bool metaMode, IVertex baseVertex, IVertex expression)
        {            
            return ZeroCodeExecuter_Instance.Get(metaMode, baseVertex, expression);
        }

        public IVertex GetAll(bool metaMode, IVertex baseVertex, IVertex expression)
        {            
            return ZeroCodeExecuter_Instance.GetAll(metaMode, baseVertex, expression);
        }

        public static IVertex Parse_Processing(IVertex formalTextLanguageProcessing, IEdge rootEdge, string text, out IEdge rootEdge_new)
        {
            rootEdge_new = null;

            IVertex formalTextLanguage_Vertex = GraphUtil.GetQueryOutFirst(formalTextLanguageProcessing, "LanguageParameterFirst", null);
            IVertex codeRepresentation_Vertex = GraphUtil.GetQueryOutFirst(formalTextLanguageProcessing, "LanguageParameterSecond", null);

            CodeRepresentationEnum codeRepresentation = CodeRepresentationEnumHelper.GetEnum(codeRepresentation_Vertex);

            Text2GraphProcessing instance = new Text2GraphProcessing(formalTextLanguage_Vertex);

            return instance.Process(rootEdge, text, codeRepresentation, out rootEdge_new);
        }

        public static string Generate_Processing(IVertex formalTextLanguageProcessing, IEdge graphBaseEdge)
        {
            IVertex formalTextLanguage_Vertex = GraphUtil.GetQueryOutFirst(formalTextLanguageProcessing, "LanguageParameterFirst", null);
            IVertex codeRepresentation_Vertex = GraphUtil.GetQueryOutFirst(formalTextLanguageProcessing, "LanguageParameterSecond", null);

            CodeRepresentationEnum codeRepresentation = CodeRepresentationEnumHelper.GetEnum(codeRepresentation_Vertex);

            Graph2TextProcessing instance;

            if (ZeroCodeGraph2StringProcessing_InstanceDictionary.ContainsKey(formalTextLanguage_Vertex))
                instance = ZeroCodeGraph2StringProcessing_InstanceDictionary[formalTextLanguage_Vertex];
            else
            {
                instance = new Graph2TextProcessing(formalTextLanguage_Vertex);

                ZeroCodeGraph2StringProcessing_InstanceDictionary.Add(formalTextLanguage_Vertex, instance);
            }

            return instance.Process(graphBaseEdge, codeRepresentation);
        }

        public IVertex Parse(IEdge rootEdge, string text, CodeRepresentationEnum codeRepresentation, out IEdge rootEdge_new)
        {
            return Parse(MinusZero.Instance.DefaultFormalTextLanguage, rootEdge, text, codeRepresentation, out rootEdge_new);
        }


        public IVertex Parse(IVertex formalTextLanguage, IEdge rootEdge, string text, CodeRepresentationEnum codeRepresentation, out IEdge rootEdge_new)
        {
            Text2GraphProcessing instance = new Text2GraphProcessing(formalTextLanguage);            

            return instance.Process(rootEdge, text, codeRepresentation, out rootEdge_new);
        }

        public string Generate(IEdge graphBaseEdge, CodeRepresentationEnum codeRepresentation)
        {
            return ZeroCodeGraph2StringProcessing_Instance.Process(graphBaseEdge, codeRepresentation);
        }

        public string Generate(IVertex formalTextLanguage, IEdge graphBaseEdge, CodeRepresentationEnum codeRepresentation)
        {
            Graph2TextProcessing instance;

            if (ZeroCodeGraph2StringProcessing_InstanceDictionary.ContainsKey(formalTextLanguage))
                instance = ZeroCodeGraph2StringProcessing_InstanceDictionary[formalTextLanguage];
            else
            {
                instance = new Graph2TextProcessing(formalTextLanguage);

                ZeroCodeGraph2StringProcessing_InstanceDictionary.Add(formalTextLanguage, instance);
            }

            return instance.Process(graphBaseEdge, codeRepresentation);
        }

        public ZeroCodeEngine()
        {            
            ZeroCodeExecuter_Instance = new ZeroCodeExecuter();
            ZeroCodeGraph2StringProcessing_Instance = new Graph2TextProcessing(MinusZero.Instance.DefaultFormalTextLanguage);
        }
    }
}
