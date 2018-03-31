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
        string[,] testCases = new string[3, 2]{
        {"c0", "(a + b) + c" },
{ "c1","(a|aa + b) + c" },
        { "c2", "((a|aa + b) + c) + d" }};
        /*
{c3 "c3: (((a|aa + b) + c) + d) + e" },
c4: (a + ((x)) + b)
c5: (((((a|aa + b|bb) + c) + d) + e) + f) + g
c6: (a + b / c* (d - (e - (f))) + g)
c7: (a \ b \ c + (d \ e - e \ f \ g))
c8: (a + b / c* (d - (e + (f / (ff* (fff + fff2 - (x / v)))))) + g)
*/

        public static void ParserTest()
        {
            String2ZeroCodeGraphProcessing parser = new String2ZeroCodeGraphProcessing();

    }
}
