using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using m0.ZeroUML.Instructions;
using m0.ZeroCode.Helpers;

namespace m0.ZeroCode
{
    class ZeroCodeExecuter
    {
        IVertex baseVertex;
        IVertex inputVertex;

        const string colon = "|";
        const string slash = @"\ ";

        static IList<IEdge> dummy = new List<IEdge>();

        public IVertex Execute(IVertex baseVertex, IVertex expression)
        {
            throw new NotImplementedException();
        }

        public IVertex Get(IVertex baseVertex, IVertex expression)
        {
            throw new NotImplementedException();
        }

        public IVertex GetAll(IVertex baseVertex, IVertex expression)
        {
            ZeroCodeExecution exe = new ZeroCodeExecution();

            IVertex qs = InstructionHelpers.CreateQueryStack();            

            if (InstructionHelpers.CheckIs(expression, colon))
            {
                IVertex left = InstructionHelpers.GetLeft(expression);
                IVertex right = InstructionHelpers.GetRight(expression);

                object meta=null;
                object value = null;

                if (left != null)
                    meta = left.Value;

                if (right != null)
                    value = right.Value;

                BaseInstructions.AddResults(baseVertex, true, meta, value, qs);

                IVertex nextExpression=null;
                
                if(right!=null)
                    nextExpression = InstructionHelpers.GetNextExpression(right);

                if (nextExpression != null)
                {
                    if (InstructionHelpers.CheckIs(nextExpression, slash))
                    {
                        IVertex newExpression = InstructionHelpers.GetNextExpression(nextExpression);

                        qs = BaseInstructions.stepIntoAllEdges(exe,qs,nextExpression);

                        return GetAll(qs, newExpression);
                    }
                }
            }

            return qs;
        }

      

      


        public ZeroCodeExecuter()
        {

        }

    }
}
