using m0.FormalTextLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;

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

        public IVertex Parse(IVertex rootVertex, string text)
        {
            return Parse(MinusZero.Instance.DefaultFormalTextLanguage, rootVertex, text);                
        }

        public IVertex Parse(IVertex formalTextLanguage, IVertex rootVertex, string text)
        {
            Text2GraphProcessing instance = new Text2GraphProcessing(formalTextLanguage);            

            return instance.Process(rootVertex, text);
        }

        public string Generate(IEdge graphBaseEdge)
        {
            return ZeroCodeGraph2StringProcessing_Instance.Process(graphBaseEdge);
        }

        public string Generate(IVertex formalTextLanguage, IEdge graphBaseEdge)
        {
            Graph2TextProcessing instance;

            if (ZeroCodeGraph2StringProcessing_InstanceDictionary.ContainsKey(formalTextLanguage))
                instance = ZeroCodeGraph2StringProcessing_InstanceDictionary[formalTextLanguage];
            else
            {
                instance = new Graph2TextProcessing(formalTextLanguage);

                ZeroCodeGraph2StringProcessing_InstanceDictionary.Add(formalTextLanguage, instance);
            }

            return instance.Process(graphBaseEdge);
        }

        public ZeroCodeEngine()
        {            
            ZeroCodeExecuter_Instance = new ZeroCodeExecuter();
            ZeroCodeGraph2StringProcessing_Instance = new Graph2TextProcessing(MinusZero.Instance.DefaultFormalTextLanguage);
        }
    }
}
