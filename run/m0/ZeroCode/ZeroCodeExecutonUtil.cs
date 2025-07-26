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

        public static void MethodCall(IExecution exe, IVertex endPoint, IVertex theObject, IVertex paramtersStack)
        {
            exe.AddStackFrame(theObject); // ENTER NEW STACK
            exe.AddStackFrame(paramtersStack);
            exe.Stack.AddEdge(this_meta, theObject);

            endPoint.Execute(exe);

            exe.RemoveStackFrame();
            exe.RemoveStackFrame(); // LEAVE NEW STACK
        }

        public static INoInEdgeInOutVertexVertex FuncionCall(IVertex endPoint, IVertex paramtersStack)
        {
            IExecution exe = new ZeroCodeExecution();

            exe.AddStackFrame(paramtersStack);

            return endPoint.Execute(exe);
        }

        public static INoInEdgeInOutVertexVertex FuncionCall(IExecution exe, IVertex endPoint, IVertex paramtersStack)
        { 
            exe.AddStackFrame(paramtersStack); // ENTER NEW STACK

            INoInEdgeInOutVertexVertex ret = endPoint.Execute(exe);

            exe.RemoveStackFrame(); // LEAVE NEW STACK

            return ret;
        }


        public static INoInEdgeInOutVertexVertex SequentiallyExecuteInstructions(IExecution exe, INoInEdgeInOutVertexVertex inStack, IVertex baseVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            bool local_isStackFrameReturn;

            INoInEdgeInOutVertexVertex possibleToReturnStack;

            foreach (IEdge e in baseVertex.OutEdgesRaw)
                if (e.Meta != NextAtom_meta && !ZeroCodeUtil.ShouldNotExecute(e)) // EXECUTE BLOCK BEG
                    {                        
                        possibleToReturnStack = exe.ExecuteInstruction(inStack, e.To, out local_isStackFrameReturn);

                        if (local_isStackFrameReturn)
                        {
                            isStackFrameReturn = true;

                            return possibleToReturnStack;
                        }
                    } // EXECUTE BLOCK END

            foreach (IEdge e in baseVertex.OutEdgesRaw)
                if (e.Meta != NextAtom_meta)
                {
                    possibleToReturnStack = SequentiallyExecuteInstructions_NextEdges(exe, inStack, e.To, out local_isStackFrameReturn);

                    if (local_isStackFrameReturn)
                    {
                        isStackFrameReturn = true;

                        return possibleToReturnStack;
                    }
                }

            return inStack;
        }

        public static INoInEdgeInOutVertexVertex SequentiallyExecuteInstructions_NextEdges(IExecution exe, INoInEdgeInOutVertexVertex inStack, IVertex baseVertex, out bool isStackFrameReturn)
        {
            isStackFrameReturn = false;

            foreach (IEdge e in baseVertex)
                if (e.Meta == NextAtom_meta && !ZeroCodeUtil.ShouldNotExecute(e)) // EXECUTE BLOCK BEG
                {
                    bool local_isStackFrameReturn;

                    INoInEdgeInOutVertexVertex possibleToReturnStack = exe.ExecuteInstruction(inStack, e.To, out local_isStackFrameReturn);

                    if (local_isStackFrameReturn)
                    {
                        isStackFrameReturn = true;

                        return possibleToReturnStack;
                    }
                    // EXECUTE BLOCK END

                    possibleToReturnStack = SequentiallyExecuteInstructions_NextEdges(exe, inStack, e.To, out local_isStackFrameReturn);

                    if (local_isStackFrameReturn)
                    {
                        isStackFrameReturn = true;

                        return possibleToReturnStack;
                    }
                } 

            return inStack;
        }
    }
}
