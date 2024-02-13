using m0.Foundation;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode
{
    public class ZeroCodeExecutonUtil
    {
        static IVertex r = MinusZero.Instance.Root;

        static IVertex this_meta = r.Get(false, @"System\Meta\ZeroUML\this");
        static IVertex NextAtom_meta = r.Get(false, @"System\FormalTextLanguage\ZeroCode\NextAtomEdge:");


        public static void CreateExecutionAndVertexMethodExecute(IVertex endPoint, IVertex theObject)
        {
            IExecution exe = new ZeroCodeExecution();

            exe.AddStackFrame(theObject);
            
            exe.AddStackFrame();

            exe.Stack.AddEdge(this_meta, theObject);
            
            endPoint.Execute(exe);            
        }

        public static void CreateExecutionAndVertexExecute(IVertex endPoint, IVertex toBeStackVertex)
        {
            IExecution exe = new ZeroCodeExecution(toBeStackVertex);

            endPoint.Execute(exe);            
        }

        public static void MethodCall(IExecution exe, IVertex endPoint, IVertex theObject, IVertex paramtersStack)
        {
            exe.AddStackFrame(theObject); // ENTER NEW STACK
            exe.AddStackFrame(paramtersStack);
            exe.Stack.AddEdge(this_meta, theObject);

            endPoint.Execute(exe);

            exe.RemoveStackFrame();
            exe.RemoveStackFrame(); // LEAVE NEW STACK
        }

        public static void FuncionCall(IExecution exe, IVertex endPoint, IVertex paramtersStack)
        { 
            exe.AddStackFrame(paramtersStack); // ENTER NEW STACK

            endPoint.Execute(exe);

            exe.RemoveStackFrame(); // LEAVE NEW STACK
        }


        public static INoInEdgeInOutVertexVertex SequentiallyExecuteInstructions(IExecution exe, INoInEdgeInOutVertexVertex inStack, IVertex baseVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            INoInEdgeInOutVertexVertex stack = inStack;

            bool local_isStackFrameReturn;

            INoInEdgeInOutVertexVertex possibleToReturnStack;

            /*    foreach (IEdge e in baseVertex.OutEdgesRaw)
                    if (e.Meta != dict.NextAtomMeta)
                        linearizedList.Add(e);*/


            foreach (IEdge e in ZeroCodeView.LinearizeVertex(baseVertex))
                if (!ZeroCodeUtil.ShouldNotExecute(e))
                {



                    possibleToReturnStack = exe.ExecuteInstruction(stack, e.To, out local_isStackFrameReturn);

                    if (local_isStackFrameReturn)
                    {
                        isStackFrameReturn = true;

                        stack = possibleToReturnStack;

                        break;
                    }
                }

            return stack;
        }





        /*
         
           static public IList<IEdge> LinearizeVertex(IVertex v)
        {            
            IList<IEdge> linearizedList = new List<IEdge>();

            foreach (IEdge e in v.OutEdgesRaw)
                if (e.Meta != dict.NextAtomMeta)
                    linearizedList.Add(e);

            foreach (IEdge e in v.OutEdgesRaw)
                if (e.Meta != dict.NextAtomMeta)
                    AddNextEdges(linearizedList, e.To);

            return linearizedList;
        }

        static void AddNextEdges(IList<IEdge> linearizedList, IVertex v)
        {
            foreach(IEdge e in v)
                if(e.Meta == dict.NextAtomMeta)
                {
                    //IEdge ee = new EasyEdge(e.From, MinusZero.Instance.Empty, e.To);

                    linearizedList.Add(e);                    

                    AddNextEdges(linearizedList, e.To);
                }
        }


        */
    }
}
