using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.ZeroCode.Helpers
{
    class MultiLineString
    {
        Dictionary<int, string> dict = new Dictionary<int, string>();

        string input;

        public int NumberOfLines;

        public MultiLineString(string _input)
        {
            input = _input;

            ProcessInputString();
        }        

        void ProcessInputString()
        {
            int lineCounter = 0;

            int prev_position = 0;

            for (int position = 0; position < input.Length; position++)
            {
                if (ZeroCodeUtil.IsCRLF(input, position)) {
                    dict.Add(lineCounter, input.Substring(prev_position, position - prev_position));

                    lineCounter++;

                    prev_position = position;
                }
            }

            NumberOfLines = lineCounter;
        }

        void RemoveLeftTab(int fromLine, int toLine)
        {
            for (int x = fromLine; x < toLine; x++)
            {
                string line = dict[x];

                string newline = line;

                if (line[0]=='\t')
                    newline = line.Substring(1);

                dict.Remove(x);
                dict.Add(x, newline);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int x = 0; x < NumberOfLines) {
                if (x != 0)
                    sb.Append(ZeroCodeUtil.CRLF);

                sb.Append(dict[x]);
            }

            return sb.ToString();
        }
    }
}
