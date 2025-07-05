using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace m0_console.console
{
    public class ConsoleUserInteraction : IUserInteraction
    {
        private void WriteLine(string line)
        {
            System.Console.WriteLine(line);
        }

        private string ReadLine()
        {
            return System.Console.ReadLine() ?? "";
        }


        public void CloseWindowByContent(object obj)
        {
            WriteLine("[SYSTEM] CloseWindowByContent called with object: " + obj?.ToString() ?? "null");
        }

        public void EditEdge(IVertex baseVertex)
        {
            WriteLine("[SYSTEM] EditEdge called with baseVertex: " + baseVertex.Value?.ToString() ?? "null");
        }

        public string InteractionInput(string question)
        {
            WriteLine(question);

            return ReadLine();
        }

        public void InteractionOutput(string info)
        {
            WriteLine(info);
        }

        private void ConsoleWriteIfMetaEdgeExist(IVertex vertex, string metaEdgeName, string prefix)
        {
            IVertex v = GraphUtil.GetQueryOutFirst(vertex, metaEdgeName, null);

            if (v != null)
            {
                WriteLine(prefix + v.Value?.ToString() ?? "null");
            }            
        }

        public void InteractionOutputException(IVertex exception)
        {
            System.Console.WriteLine("[EXCEPTION]");

            ConsoleWriteIfMetaEdgeExist(exception, "Type", "   Type : ");
            ConsoleWriteIfMetaEdgeExist(exception, "Where", "   Where : ");            
            ConsoleWriteIfMetaEdgeExist(exception, "What", "   What : ");
            ConsoleWriteIfMetaEdgeExist(exception, "CodeEdge", "   CodeEdge : ");
            ConsoleWriteIfMetaEdgeExist(exception, "DataEdge", "   DataEdge : ");            
        }

        public IVertex InteractionSelect(IVertex info, IList<IEdge> options, bool firstSelected)
        {
            WriteLine(GraphUtil.GetStringValue(info));  
            
            if (options.Count == 0)
            {
                WriteLine("[No options available]");
                return null;
            }

            while (true)
            {
                int cnt = 1;

                foreach (IEdge option in options)
                {
                    WriteLine("   [" + cnt + "] " + GraphUtil.GetStringValue(option.To));
                    cnt++;
                }

                WriteLine("[Type number in 1 - " + cnt + " range and press enter]");

                string input = ReadLine();

                if (int.TryParse(input, out int selectedIndex) && selectedIndex > 0 && selectedIndex <= options.Count)                
                    return options[selectedIndex - 1].To;                    
                else 
                    WriteLine("[Invalid selection. Please try again]");                
            }

            return null;
        }

        public IVertex InteractionSelectButton(IVertex info, IList<IEdge> options)
        {
            return InteractionSelect(info, options, false);
        }

        public void OpenCodeVisualiser(IVertex baseVertex, bool isFloating)
        {
            WriteLine("[SYSTEM] OpenCodeVisualiser called with baseVertex: " + baseVertex.Value?.ToString() ?? "null");
        }

        public void OpenDefaultVisualiser(IVertex baseVertex, bool isFloating)
        {
            WriteLine("[SYSTEM] OpenDefaultVisualiser called with baseVertex: " + baseVertex.Value?.ToString() ?? "null");
        }

        public void OpenFormVisualiser(IVertex baseVertex, bool isFloating)
        {
            WriteLine("[SYSTEM] OpenFormVisualiser called with baseVertex: " + baseVertex.Value?.ToString() ?? "null");
        }

        public void OpenVisualiser(IVertex baseVertex, IVertex inputVertex, bool isFloating)
        {
            WriteLine("[SYSTEM] OpenVisualiser called with baseVertex: " + baseVertex.Value?.ToString() ?? "null" +
                ", inputVertex: " + inputVertex.Value?.ToString() ?? "null");
        }

        public void ShowContent(object obj)
        {
            WriteLine("[SYSTEM] ShowContent called with object: " + obj?.ToString() ?? "null");
        }

        public void ShowContentFloating(object obj, FloatingWindowSize size)
        {
            WriteLine("[SYSTEM] ShowContentFloating called with object: " + obj?.ToString() ?? "null" +
                ", size: " + size.ToString());
        }

        //

        public void UserInteractionInitialize()
        {
            WriteLine("[SYSTEM] UserInteractionInitialize called. ConsoleUserInteraction initialized.");
        }

        public void UserInteractionFinalize()
        {
            WriteLine("[SYSTEM] UserInteractionFinalize called. ConsoleUserInteraction finalized.");
        }

        //

        public bool TypedEdge_Get_Test(Type[] interfacesInToCreateType)
        {
            return false;
        }
    }
}