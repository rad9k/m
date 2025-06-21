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
        public Dictionary<int, string> Lines = new Dictionary<int, string>();

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
                    Lines.Add(lineCounter, input.Substring(prev_position, position - prev_position + 2));

                    lineCounter++;

                    prev_position = position + 2;

                    if (position == input.Length - 1)
                        wasCRLFLastChars = true;
                }
            }

            if (!wasCRLFLastChars)
            {
                Lines.Add(lineCounter, input.Substring(prev_position, input.Length - prev_position));
                lineCounter++;
            }

            NumberOfLines = lineCounter - 1;
        }

        public void Replace(string from, string to)
        {
            Replace(1, NumberOfLines, from, to);
        }

        public void Replace(int fromLine, int toLine, string from, string to)
        {
            for (int i = fromLine; i < toLine; i++)
                Lines[i] = Lines[i].Replace(from, to);
        }

        public void AddLeftTab(int fromLine, int toLine, int noOfTabs)
        {
            StringBuilder tabs = new StringBuilder();

            for (int x = 0; x < noOfTabs; x++)
                tabs.Append("\t");

            for (int x = fromLine; x <= toLine; x++)
            {
                string line = Lines[x];

                string newline = tabs + line;
                
                Lines.Remove(x);
                Lines.Add(x, newline);
            }
        }

        public void AddLeftTab(int noOfTabs)
        {
            AddLeftTab(1, NumberOfLines, noOfTabs);
        }

        public void RemoveLeftTab(int fromLine, int toLine)
        {
            for (int x = fromLine; x <= toLine; x++)
            {
                string line = Lines[x];

                string newline = line;

                if (line[0]=='\t')
                    newline = line.Substring(1);

                Lines.Remove(x);
                Lines.Add(x, newline);
            }
        }

        public void RemoveLeftTab_TwoTimes(int fromLine, int toLine)
        {
            for (int x = fromLine; x <= toLine; x++)
            {
                string line = Lines[x];

                string newline = line;

                if (line[0] == '\t' && line[1] == '\t')
                    newline = line.Substring(2);

                Lines.Remove(x);
                Lines.Add(x, newline);
            }
        }

        public void RemoveLeftTab_ThreeTimes(int fromLine, int toLine)
        {
            for (int x = fromLine; x <= toLine; x++)
            {
                string line = Lines[x];

                string newline = line;

                if (line[0] == '\t' && line[1] == '\t' && line[2] == '\t')
                    newline = line.Substring(3);

                Lines.Remove(x);
                Lines.Add(x, newline);
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

        public void RemoveLeftTab_ThreeTimes()
        {
            RemoveLeftTab_ThreeTimes(1, NumberOfLines);
        }

        public void InsertEmptyLineBeforeLineNo(int lineNo)
        {
            Dictionary<int, string> dict_new = new Dictionary<int, string>();

            foreach (KeyValuePair<int,string> kvp in Lines)            
                if (kvp.Key >= lineNo)                
                    dict_new.Add(kvp.Key + 1, kvp.Value);
                else
                    dict_new.Add(kvp.Key, kvp.Value);

            dict_new.Add(lineNo, "\r\n");

            Lines = dict_new;

            NumberOfLines++;
        }

        public override string ToString()
        {
            return ToString(1, NumberOfLines);
        }

        public string ToString(int fromLine, int toLine)
        {
            StringBuilder sb = new StringBuilder();

            for (int x = fromLine; x <= toLine; x++)            
                sb.Append(Lines[x]);

            string toReturn = sb.ToString();

            if (toReturn.Length > 1 
                && toReturn[toReturn.Length - 2] == '\r'
                && toReturn[toReturn.Length - 1] == '\n')
                toReturn = toReturn.Substring(0, toReturn.Length - 2);

            return toReturn;
        }
    }
}
