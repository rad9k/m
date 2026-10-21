using m0.Foundation;
using m0.Graph;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;

namespace m0.ZeroTypes
{
    public class DateTime
    {
        public static void FillDateTime(IVertex basevertex, System.DateTime datetime)
        {
            basevertex.AddVertex(Year, datetime.Year);            
            basevertex.AddVertex(Month, datetime.Month);
            basevertex.AddVertex(Day, datetime.Day);
            basevertex.AddVertex(Hour, datetime.Hour);
            basevertex.AddVertex(Minute, datetime.Minute);
            basevertex.AddVertex(Second, datetime.Second);
            basevertex.AddVertex(Millisecond, datetime.Millisecond);
        }

        static IVertex Year;
        static IVertex Month;
        static IVertex Day;
        static IVertex Hour;
        static IVertex Minute;
        static IVertex Second;
        static IVertex Millisecond;

        public DateTime()
        {
            IVertex sm = MinusZero.Instance.Root.Get(false, @"System\Meta");

            Year = sm.Get(false, @"ZeroTypes\DateTime\Year");
            Month = sm.Get(false, @"ZeroTypes\DateTime\Month");
            Day = sm.Get(false, @"ZeroTypes\DateTime\Day");
            Hour = sm.Get(false, @"ZeroTypes\DateTime\Hour");
            Minute = sm.Get(false, @"ZeroTypes\DateTime\Minute");
            Second = sm.Get(false, @"ZeroTypes\DateTime\Second");
            Millisecond = sm.Get(false, @"ZeroTypes\DateTime\Millisecond"); 
        }        

        public static INoInEdgeInOutVertexVertex DateTime_constructor(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;            

            IVertex yearVertex = GraphUtil.GetQueryOutFirst(stack, "year", null);
            IVertex monthVertex = GraphUtil.GetQueryOutFirst(stack, "month", null);
            IVertex dayVertex = GraphUtil.GetQueryOutFirst(stack, "day", null);
            IVertex hourVertex = GraphUtil.GetQueryOutFirst(stack, "hour", null);
            IVertex minuteVertex = GraphUtil.GetQueryOutFirst(stack, "minute", null);
            IVertex secondVertex = GraphUtil.GetQueryOutFirst(stack, "second", null);
            IVertex millisecondVertex = GraphUtil.GetQueryOutFirst(stack, "millisecond", null);

            int yearValue = GraphUtil.GetIntegerValueOr0(yearVertex);
            int monthValue = GraphUtil.GetIntegerValueOr0(monthVertex);
            int dayValue = GraphUtil.GetIntegerValueOr0(dayVertex);
            int hourValue = GraphUtil.GetIntegerValueOr0(hourVertex);
            int minuteValue = GraphUtil.GetIntegerValueOr0(minuteVertex);
            int secondValue = GraphUtil.GetIntegerValueOr0(secondVertex);
            int millisecondValue = GraphUtil.GetIntegerValueOr0(millisecondVertex);

            IVertex YearVertex = GraphUtil.GetQueryOutFirst(stack, "Year", null);
            IVertex MonthVertex = GraphUtil.GetQueryOutFirst(stack, "Month", null);
            IVertex DayVertex = GraphUtil.GetQueryOutFirst(stack, "Day", null);
            IVertex HourVertex = GraphUtil.GetQueryOutFirst(stack, "Hour", null);
            IVertex MinuteVertex = GraphUtil.GetQueryOutFirst(stack, "Minute", null);
            IVertex SecondVertex = GraphUtil.GetQueryOutFirst(stack, "Second", null);
            IVertex MillisecondVertex = GraphUtil.GetQueryOutFirst(stack, "Millisecond", null);

            YearVertex.Value = yearValue;
            MonthVertex.Value = monthValue;
            DayVertex.Value = dayValue;
            HourVertex.Value = hourValue;
            MinuteVertex.Value = minuteValue;
            SecondVertex.Value = secondValue;
            MillisecondVertex.Value = millisecondValue;

            return stack;
        }

        public static INoInEdgeInOutVertexVertex IsLaterThan(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            // date

            IVertex dateVertex = GraphUtil.GetQueryOutFirst(stack, "date", null);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            if (dateVertex == null)
                return newStack;

            System.DateTime 



            return stack;
        }

        public static INoInEdgeInOutVertexVertex IsEarlierThan(IExecution exe)
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

        public static INoInEdgeInOutVertexVertex Add(IExecution exe)
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

        public static INoInEdgeInOutVertexVertex Substract(IExecution exe)
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
