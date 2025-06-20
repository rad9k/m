using m0.FormalTextLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;
using m0.ZeroTypes.UX;

namespace m0.ZeroCode
{
    public class ZeroCodeEngine : IFormalTextGenerator, IFormalTextParser, IExecuter
    {             
        ZeroCodeExecuter ZeroCodeExecuter_Instance;
        Graph2TextProcessing ZeroCodeGraph2StringProcessing_Instance;
        
        Dictionary<IVertex, Graph2TextProcessing> ZeroCodeGraph2StringProcessing_InstanceDictionary = new Dictionary<IVertex, Graph2TextProcessing>();

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
            //Graph2TextProcessing ZeroCodeGraph2StringProcessing_Instance = new Graph2TextProcessing(MinusZero.Instance.DefaultFormalTextLanguage);

            return ZeroCodeGraph2StringProcessing_Instance.Process(graphBaseEdge, codeRepresentation);
        }

        public string Generate(IVertex formalTextLanguage, IEdge graphBaseEdge, CodeRepresentationEnum codeRepresentation)
        {
            //Graph2TextProcessing instance = new Graph2TextProcessing(formalTextLanguage);

            //return instance.Process(graphBaseEdge);

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
