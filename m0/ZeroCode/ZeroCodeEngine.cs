using m0.FormalTextLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m0.Foundation;

namespace m0.ZeroCode
{
    public class ZeroCodeEngine : ICodeGenerator, IParser, IExecuter
    {
        String2ZeroCodeGraphProcessing String2ZeroCodeGraphProcessing_Instance;
        ZeroCodeExecuter ZeroCodeExecuter_Instance;
        ZeroCodeGraph2StringProcessing ZeroCodeGraph2StringProcessing_Instance;

        public IVertex Execute(IVertex baseVertex, IVertex expression)
        {
            ZeroCodeExecuter executer = new ZeroCodeExecuter();

            return executer.Execute(baseVertex, expression);
        }

        public IVertex Get(IVertex baseVertex, IVertex expression)
        {
            ZeroCodeExecuter executer = new ZeroCodeExecuter();

            return executer.Get(baseVertex, expression);
        }

        public IVertex GetAll(IVertex baseVertex, IVertex expression)
        {
            ZeroCodeExecuter executer = new ZeroCodeExecuter();

            return executer.GetAll(baseVertex, expression);
        }

        public IVertex Parse(bool metaMode, IVertex rootVertex, string text)
        {             
            String2ZeroCodeGraphProcessing p = new String2ZeroCodeGraphProcessing();

            return p.Process(rootVertex, text); 
        }

        public string ZeroCodeGraph2String(IEdge graphBaseEdge)
        {
            ZeroCodeGraph2StringProcessing p = new ZeroCodeGraph2StringProcessing();

            return p.Process(graphBaseEdge);

        }

        public ZeroCodeEngine()
        {
            IVertex DefaultFormalTextLanguage = MinusZero.Instance.Root.Get(false, @"User\CurrentUser:\DefaultFormalTextLanguage:");

            String2ZeroCodeGraphProcessing_Instance = new String2ZeroCodeGraphProcessing(DefaultFormalTextLanguage);
            ZeroCodeExecuter_Instance = new ZeroCodeExecuter();
            ZeroCodeGraph2StringProcessing_Instance = new ZeroCodeGraph2StringProcessing(DefaultFormalTextLanguage);
        }
    }
}
