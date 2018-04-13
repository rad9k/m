using m0.Foundation;
using m0.ZeroCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0
{
    public class AutoTest
    {
        static string[,] testCases = new string[10, 3]{
        {"c0", "(a + b) + c","+" },
        {"c1", "(aaaa + b) + c","+" },
        {"c2", "((aaaa + b) + c) + d","+"},
        {"c3", "(((aaaa + b) + c) + d) + e","+"},
        {"c4", "((((aaaa + b) + c) + d) + e) + f","+"},
        {"c5", "(((((aaaa + bbbb) + c) + d) + e) + f) + g", "+" },
        {"c6", "(a + ((x)) + b)","()"},
        {"c7", "(a + b / c * (d - (e - (f))) + g)","()"},
        {"c8", @"(a \ b \ c + (d \ e - e \ f \ g))","?"},
        {"c9", "(a + b / c * (d - (e + (f / (ff * (fff + fff2 - (x / v)))))) + g)","?"}};


        public static void ParserTest()
        {
            String2ZeroCodeGraphProcessing parser = new String2ZeroCodeGraphProcessing();

            System.IO.StreamWriter logFile = new System.IO.StreamWriter(@"AUTO_TEST.xls");
            logFile.AutoFlush = true;



            for(int l1009_left = 0; l1009_left <=0; l1009_left++)
            for (int l1009_right = 1; l1009_right <= 1; l1009_right++)
            for (int l1089 = -1; l1089 <= 2; l1089++)
                for (int l1149_dict = -1; l1149_dict <= 4; l1149_dict++)
                    for (int l1149_parent = 1; l1149_parent <= 1; l1149_parent++) {
                        //string line = l1089 + "\t" + l1149_dict + "\t" + l1149_parent;

                        string line = l1009_left + "\t" + l1009_right + "\t" +  l1089 + "\t" + l1149_dict + "\t" + l1149_parent;

                        if (!(l1149_dict==-1 && l1149_parent==-1) && l1009_left!=l1009_right)
                        for (int c = 7; c <= 7; c++)
                        {
                            //line += "\t" + testCases[c, 1];

                            IVertex b = MinusZero.Instance.Root.AddVertex(null, l1089 + " " + l1149_dict + " " + l1149_parent + " " + testCases[c,1]);

                            try
                            {
                                IVertex r = parser.ParserAutoTestProcess(b, testCases[c, 1], l1089, l1149_dict, l1149_parent, l1009_left, l1009_right);

                                    if (b.Get("SYNTAX ERROR") != null)
                                        line += "\tSYNTAX";
                                    else
                                    {
                                        if (b.Get(@"\" + testCases[c, 2]) != null)
                                            line += "\tO";
                                    }
                            }
                            catch (Exception e) {
                                line += "\t" + e.ToString();
                            }
                        }

                        logFile.WriteLine(line);
                    }

            logFile.Close();
        }
    }
}
