using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace m0.Lib
{
    public class QueryStringEdgeComparer : IComparer<IEdge>
    {
        string queryString;
        bool isAlphabetical;

        public int Compare(IEdge x, IEdge y) {
            if (x == null || y == null || x.To == null || y.To == null)
                return 0;

            IVertex xv = x.To.Get(false, queryString);
            IVertex yv = y.To.Get(false, queryString);

            if (xv == null || yv == null)
                return 0;

            if (isAlphabetical)
            {
                string xs = xv.Value.ToString();
                string ys = yv.Value.ToString();

                return xs.CompareTo(ys);
            }
            else
            {
                bool isXnull=false, isYnull=false;

                double xValue = GraphUtil.GetDoubleValue(xv, ref isXnull);
                double yValue = GraphUtil.GetDoubleValue(yv, ref isYnull);

                if (isXnull || isYnull)
                    return 0;

                if (xValue == yValue)
                    return 0;

                if (yValue < xValue)
                    return 1;

                return -1;
            }            
        }        

        public QueryStringEdgeComparer(string _queryString, bool _isAlphabetical)
        {
            queryString = _queryString;
            isAlphabetical = _isAlphabetical;
        }
    }

    public class EdgeComparer : IComparer<IEdge>
    {
        bool isAlphabetical;

        public int Compare(IEdge x, IEdge y)
        {
            if (x == null || y == null)
                return 0;

            IVertex xv = x.To;
            IVertex yv = y.To;

            if (xv == null || yv == null)
                return 0;

            if (isAlphabetical)
            {
                string xs = xv.Value.ToString();
                string ys = yv.Value.ToString();

                return xs.CompareTo(ys);
            }
            else
            {
                bool isXnull = false, isYnull = false;

                double xValue = GraphUtil.GetDoubleValue(xv, ref isXnull);
                double yValue = GraphUtil.GetDoubleValue(yv, ref isYnull);

                if (isXnull || isYnull)
                    return 0;

                if (xValue == yValue)
                    return 0;

                if (yValue < xValue)
                    return 1;

                return -1;
            }
        }

        public EdgeComparer(bool _isAlphabetical)
        {
            isAlphabetical = _isAlphabetical;
        }
    }

    public class Std
    {
        public static INoInEdgeInOutVertexVertex AlphabeticalSortByQuery(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> toSortList = GraphUtil.GetQueryOut(stack, "toSortVertex", null);

            IVertex sortVertexQueryString = GraphUtil.GetQueryOutFirst(stack, "sortVertexQueryString", null);

            if (toSortList == null || sortVertexQueryString == null)
                return exe.Stack;

            string queryString = sortVertexQueryString.Value.ToString();

            QueryStringEdgeComparer qsec = new QueryStringEdgeComparer(queryString, true);

            List<IEdge> edgesList = toSortList.ToList<IEdge>();
            edgesList.Sort(qsec);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in edgesList)            
                newStack.AddEdge(e.Meta, e.To);            

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex NumericSortByQuery(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> toSortList = GraphUtil.GetQueryOut(stack, "toSortVertex", null);

            IVertex sortVertexQueryString = GraphUtil.GetQueryOutFirst(stack, "sortVertexQueryString", null);

            if (toSortList == null || sortVertexQueryString == null)
                return exe.Stack;

            string queryString = sortVertexQueryString.Value.ToString();

            QueryStringEdgeComparer qsec = new QueryStringEdgeComparer(queryString, false);

            List<IEdge> edgesList = toSortList.ToList<IEdge>();
            edgesList.Sort(qsec);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in edgesList)
                newStack.AddEdge(e.Meta, e.To);

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex AlphabeticalSort(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> toSortList = GraphUtil.GetQueryOut(stack, "toSortVertex", null);

            if (toSortList == null)
                return exe.Stack;

            EdgeComparer ec = new EdgeComparer(true);

            List<IEdge> edgesList = toSortList.ToList<IEdge>();
            edgesList.Sort(ec);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in edgesList)
                newStack.AddEdge(e.Meta, e.To);

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex NumericSort(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> toSortList = GraphUtil.GetQueryOut(stack, "toSortVertex", null);

            if (toSortList == null)
                return exe.Stack;

            EdgeComparer ec = new EdgeComparer(false);

            List<IEdge> edgesList = toSortList.ToList<IEdge>();
            edgesList.Sort(ec);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in edgesList)
                newStack.AddEdge(e.Meta, e.To);

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Concatenate(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "input", null);

            StringBuilder sb = new StringBuilder();

            foreach (IEdge e in inputList)
                sb.Append(e.To.ToString());

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            newStack.AddVertex(null, sb.ToString());

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Split(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "input", null);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in inputList)
            {
                string inputString = e.To.Value.ToString();

                for (int x = 0; x < inputString.Length; x++)
                    newStack.AddVertex(null, inputString[x]);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex SplitBy(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> byList = GraphUtil.GetQueryOut(stack, "by", null);

            IList<string> splitByStringList = new List<string>();

            foreach (IEdge e in byList)
                splitByStringList.Add(e.To.Value.ToString());

            string[] slitByStringArray = splitByStringList.ToArray();

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "input", null);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in inputList)
            {
                string inputString = e.To.Value.ToString();

                string[] splitStrings = inputString.Split(slitByStringArray, StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in splitStrings)
                    newStack.AddVertex(null, s);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Replace(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "input", null);

            IVertex fromVertex = GraphUtil.GetQueryOutFirst(stack, "from", null);
            IVertex toVertex = GraphUtil.GetQueryOutFirst(stack, "to", null);

            if (fromVertex == null || toVertex == null)
                return exe.Stack;

            string from = fromVertex.Value.ToString();
            string to = toVertex.Value.ToString();

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in inputList)
            {
                string inputString = e.To.Value.ToString();

                string newString = inputString.Replace(from, to);

                newStack.AddVertex(null, newString);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex IndexOf(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "input", null);

            IVertex testVertex = GraphUtil.GetQueryOutFirst(stack, "test", null);

            if (testVertex == null)
                return exe.Stack;

            string test = testVertex.Value.ToString();

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in inputList)
            {
                string inputString = e.To.Value.ToString();

                int indexOf = inputString.IndexOf(test);

                newStack.AddVertex(null, indexOf);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Substring(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "input", null);

            /*IVertex fromVertex = GraphUtil.GetQueryOutFirst(stack, "from", null);
            IVertex toVertex = GraphUtil.GetQueryOutFirst(stack, "to", null);

            if (fromVertex == null || toVertex == null)
                return exe.stack;

            int? _from = GraphUtil.GetIntegerValue(fromVertex);
            int? _to = GraphUtil.GetIntegerValue(toVertex);

            if (_from == null || _to == null)
                return exe.stack;
                */
            bool isNull=false;

            int from = LibUtil.GetIntFromVertex(stack, "from", ref isNull);
            int to = LibUtil.GetIntFromVertex(stack, "to", ref isNull);

            if (isNull)
                return exe.Stack;

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (IEdge e in inputList)
            {
                string inputString = e.To.Value.ToString();

                int finalFrom = from - 1;
                int finalLength = to - from + 1;

                if (finalFrom < inputString.Length)
                {
                    if (finalFrom + finalLength > inputString.Length)
                        finalLength = inputString.Length - finalFrom;

                    string newString = inputString.Substring(finalFrom, finalLength);

                    newStack.AddVertex(null, newString);
                }
            }

            return newStack;
        }
        public static INoInEdgeInOutVertexVertex Sqrt(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, Math.Sqrt(doubleValue));                
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Pow(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex powerVertex = GraphUtil.GetQueryOutFirst(stack, "power", null);

            double? _powerValue = GraphUtil.GetDoubleValue(powerVertex);

            if (_powerValue == null)
                return null;

            double powerValue = (double)_powerValue;


            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);


            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, Math.Pow(doubleValue, powerValue));
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Abs(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, (int)Math.Abs(doubleValue));
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Truncate(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, Math.Truncate(doubleValue));
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Celling(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, Math.Ceiling(doubleValue));
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Floor(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, Math.Floor(doubleValue));
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Sin(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, Math.Sin(doubleValue));
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Cos(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, Math.Cos(doubleValue));
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Exp(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, Math.Exp(doubleValue));
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Log(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, Math.Log(doubleValue));
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Log10(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, Math.Log10(doubleValue));
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Max(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            double max = Double.MinValue;

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                if (doubleValue > max)
                    max = doubleValue;             
            }

            newStack.AddVertex(null, Math.Ceiling(max));

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Min(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            double min = Double.MaxValue;

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                if (doubleValue < min)
                    min = doubleValue;
            }

            newStack.AddVertex(null, Math.Ceiling(min));

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Sign(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, Math.Sign(doubleValue));
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Tan(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "value", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, Math.Tan(doubleValue));
            }

            return newStack;
        }

        static Random RandomObject = new System.Random();

        public static INoInEdgeInOutVertexVertex Randomize(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex valueVertex = GraphUtil.GetQueryOutFirst(stack, "value", null);

            int? _valueValue = GraphUtil.GetIntegerValue(valueVertex);

            if (_valueValue == null)
                return null;

            int valueValue = (int)_valueValue;

            RandomObject = new System.Random(valueValue);
           
            return stack;
        }

        public static INoInEdgeInOutVertexVertex Random(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IList<IEdge> inputList = GraphUtil.GetQueryOut(stack, "max", null);

            InstructionHelpers.NumericTypeEnum resType;

            IList<object> numericList = InstructionHelpers.GetNumberList(inputList, out resType);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            foreach (object o in numericList)
            {
                double doubleValue = Convert.ToDouble(o);

                newStack.AddVertex(null, RandomObject.NextDouble() * doubleValue);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Sequence(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            // min

            IVertex minVertex = GraphUtil.GetQueryOutFirst(stack, "min", null);

            int? _minValue = GraphUtil.GetIntegerValue(minVertex);

            if (_minValue == null)
                return null;

            int minValue = (int)_minValue;

            // max

            IVertex maxVertex = GraphUtil.GetQueryOutFirst(stack, "max", null);

            int? _maxValue = GraphUtil.GetIntegerValue(maxVertex);

            if (_maxValue == null)
                return null;

            int maxValue = (int)_maxValue;

            //

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            if (maxValue > minValue)
            {
                for (int x = minValue; x <= maxValue; x++)
                    newStack.AddVertex(null, x);
            }
            else
            {
                for (int x = minValue; x >= maxValue; x--)
                    newStack.AddVertex(null, x);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex StepSequence(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            // min

            IVertex minVertex = GraphUtil.GetQueryOutFirst(stack, "min", null);

            double? _minValue = GraphUtil.GetDoubleValue(minVertex);

            if (_minValue == null)
                return null;

            double minValue = (double)_minValue;

            // max

            IVertex maxVertex = GraphUtil.GetQueryOutFirst(stack, "max", null);

            double? _maxValue = GraphUtil.GetDoubleValue(maxVertex);

            if (_maxValue == null)
                return null;

            double maxValue = (double)_maxValue;

            // step

            IVertex stepVertex = GraphUtil.GetQueryOutFirst(stack, "step", null);

            double? _stepValue = GraphUtil.GetDoubleValue(stepVertex);

            if (_stepValue == null)
                return null;

            double stepValue = (double)_stepValue;

            //

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            if (maxValue > minValue)
            {
                for (double x = minValue; x <= maxValue; x += stepValue)
                    newStack.AddVertex(null, x);
            }
            else
            {
                for (double x = minValue; x >= maxValue; x -= stepValue)
                    newStack.AddVertex(null, x);
            }

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Sleep(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            // miliseconds

            IVertex milisecondsVertex = GraphUtil.GetQueryOutFirst(stack, "miliseconds", null);

            int? _milisecondsValue = GraphUtil.GetIntegerValue(milisecondsVertex);

            if (_milisecondsValue == null)
                return null;

            int milisecondsValue = (int)_milisecondsValue;

            Thread.Sleep(milisecondsValue);

            return stack;
        }
    }
}
