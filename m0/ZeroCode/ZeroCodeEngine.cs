using m0.TextLanguage;
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
    }
}
