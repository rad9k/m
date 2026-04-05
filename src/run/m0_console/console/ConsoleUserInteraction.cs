using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace m0_console.console
{
    public class ConsoleUserInteraction : IUserInteraction
    {
        private bool IsAnsiSupported = false;

        public PlatformTypeEnum GetPlatformType()
        {
            return PlatformTypeEnum.Console;
        }

        private void WriteLine(string line)
        {
            if (IsAnsiSupported)
                WriteLine_Ansi(line);
            else
                WriteLine_Colors(line);
        }

        private void WriteLine_Colors(string line)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            try
            {
                if (line.Contains("[EXCEPTION]"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(line);
                }
                else if (line.Contains("[ERROR]"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(line);
                }
                else if (line.Contains("[USER]"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow; // Closest to orange
                    Console.WriteLine(line);
                }
                else if (line.Contains("[INFO]"))
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(line);
                }
                else if (line.Contains("[SYSTEM]"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(line);
                }
                else if (line.Contains("[LINK]"))
                {
                    // Handle links with regex - color the URLs differently
                    string pattern = @"(https?://[^\s]+)";
                    if (Regex.IsMatch(line, pattern))
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        
                        string[] parts = Regex.Split(line, pattern);
                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (Regex.IsMatch(parts[i], pattern))
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.Write(parts[i]);
                                Console.ForegroundColor = ConsoleColor.Blue;
                            }
                            else
                            {
                                Console.Write(parts[i]);
                            }
                        }
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(line);
                    }
                }
                else
                {
                    Console.WriteLine(line);
                }
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }

        private void WriteLine_Ansi(string line)
        {
            const string Reset = "\u001b[0m";
            string coloredLine;

            if (line.Contains("[EXCEPTION]") || line.Contains("[ERROR]"))
                coloredLine = "\u001b[31m\u001b[1m" + line + Reset; // Red
            else if (line.Contains("[USER]"))
                coloredLine = "\u001b[38;5;214m\u001b[1m" + line + Reset; // Orange
            else if (line.Contains("[INFO]"))
                coloredLine = "\u001b[38;5;75m" + line + Reset; // Cyan
            else if (line.Contains("[SYSTEM]"))
                coloredLine = "\u001b[92m" + line + Reset; // Green
            else if (line.Contains("[LINK]"))
            {
                string pattern = @"(https?://[^\s]+)";                
                string Purple = "\u001b[38;5;99m";
                string Underline = "\u001b[4m";

                coloredLine = Purple + Regex.Replace(line, pattern, match =>
                    Underline + match.Value + Reset + Purple) + Reset;
            }
            else
                coloredLine = line;

            System.Console.WriteLine(coloredLine);
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
            WriteLine("[USER] " + question);

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
            ConsoleWriteIfMetaEdgeExist(exception, "Type", "[EXCEPTION]   Type : ");
            ConsoleWriteIfMetaEdgeExist(exception, "Where", "[EXCEPTION]   Where : ");            
            ConsoleWriteIfMetaEdgeExist(exception, "What", "[EXCEPTION]   What : ");
            ConsoleWriteIfMetaEdgeExist(exception, "CodeEdge", "[EXCEPTION]   CodeEdge : ");
            ConsoleWriteIfMetaEdgeExist(exception, "DataEdge", "[EXCEPTION]   DataEdge : ");            
        }

        public IVertex InteractionSelect(IVertex info, IList<IEdge> options, bool firstSelected)
        {
            WriteLine("[INFO] " + GraphUtil.GetStringValue(info));  
            
            if (options.Count == 0)
            {
                WriteLine("[INFO] No options available.");
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

                WriteLine("[USER] Type number in 1 - " + cnt + " range and press enter");

                string input = ReadLine();

                if (int.TryParse(input, out int selectedIndex) && selectedIndex > 0 && selectedIndex <= options.Count)
                    return options[selectedIndex - 1].To;
                else
                {
                    WriteLine("[INFO] " + GraphUtil.GetStringValue(info));
                    WriteLine("[INFO] Invalid selection. Please try again");
                }
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
            AnsiConsole.EnableAnsiSupport();
            IsAnsiSupported = AnsiConsole.IsAnsiSupported();

            WriteLine("[SYSTEM] -zero, version 0.99");
            WriteLine("[SYSTEM] public domain software by radek@tereszczuk.com");
            WriteLine("[LINK]   http://tereszczuk.com");
            //WriteLine("[SYSTEM] UserInteractionInitialize called. ConsoleUserInteraction initialized.");
            WriteLine("[SYSTEM] you will die. someday. remember");
        }

        public void UserInteractionFinalize()
        {
            //WriteLine("[SYSTEM] UserInteractionFinalize called. ConsoleUserInteraction finalized.");
            WriteLine("[SYSTEM] -zero sucesfull exit");
            WriteLine("[SYSTEM] be gracefull");
        }

        //

        public bool TypedEdge_Get_Test(Type[] interfacesInToCreateType)
        {
            return false;
        }
    }
}