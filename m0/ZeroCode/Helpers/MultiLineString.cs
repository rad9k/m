using m0.ZeroTypes.UX;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
            int lineCounter = 1;

            int prev_position = 0;

            bool wasCRLFLastChars = false;

            for (int position = 0; position < input.Length; position++)
            {
                if (ZeroCodeUtil.IsCRLF(input, position)) {
                    dict.Add(lineCounter, input.Substring(prev_position, position - prev_position + 2));

                    lineCounter++;

                    prev_position = position + 2;

                    if (position == input.Length - 1)
                        wasCRLFLastChars = true;
                }
            }

            if (!wasCRLFLastChars)
            {
                dict.Add(lineCounter, input.Substring(prev_position, input.Length - prev_position));
                lineCounter++;
            }

            NumberOfLines = lineCounter - 1;
        }

        public void RemoveLeftTab(int fromLine, int toLine)
        {
            for (int x = fromLine; x <= toLine; x++)
            {
                string line = dict[x];

                string newline = line;

                if (line[0]=='\t')
                    newline = line.Substring(1);

                dict.Remove(x);
                dict.Add(x, newline);
            }
        }

        public void RemoveLeftTab_TwoTimes(int fromLine, int toLine)
        {
            for (int x = fromLine; x <= toLine; x++)
            {
                string line = dict[x];

                string newline = line;

                if (line[0] == '\t' && line[1] == '\t')
                    newline = line.Substring(2);

                dict.Remove(x);
                dict.Add(x, newline);
            }
        }

        public void RemoveLeftTab()
        {
            RemoveLeftTab(1, NumberOfLines);
        }

        public void RemoveLeftTab_TwoTimes()
        {
            RemoveLeftTab_TwoTimes(1, NumberOfLines);
        }

        public override string ToString()
        {
            return ToString(1, NumberOfLines);
        }

        public string ToString(int fromLine, int toLine)
        {
            StringBuilder sb = new StringBuilder();

            for (int x = fromLine; x <= toLine; x++)            
                sb.Append(dict[x]);

            string toReturn = sb.ToString();

            if (toReturn.Length > 1 
                && toReturn[toReturn.Length - 2] == '\r'
                && toReturn[toReturn.Length - 1] == '\n')
                toReturn = toReturn.Substring(0, toReturn.Length - 2);

            return toReturn;
        }
    }
}
