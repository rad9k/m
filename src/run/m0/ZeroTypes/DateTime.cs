using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
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
        static IVertex sm = MinusZero.Instance.Root.Get(false, @"System\Meta");

        static IVertex Year = sm.Get(false, @"ZeroTypes\DateTime\Year");
        static IVertex Month = sm.Get(false, @"ZeroTypes\DateTime\Month");
        static IVertex Day = sm.Get(false, @"ZeroTypes\DateTime\Day");
        static IVertex Hour = sm.Get(false, @"ZeroTypes\DateTime\Hour");
        static IVertex Minute = sm.Get(false, @"ZeroTypes\DateTime\Minute");
        static IVertex Second = sm.Get(false, @"ZeroTypes\DateTime\Second");
        static IVertex Millisecond = sm.Get(false, @"ZeroTypes\DateTime\Millisecond");

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
                
        public static void CopyDtToVertex(System.DateTime dt, IVertex dtVertex)
        {
            IVertex YearVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Year", null);
            IVertex MonthVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Month", null);
            IVertex DayVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Day", null);
            IVertex HourVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Hour", null);
            IVertex MinuteVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Minute", null);
            IVertex SecondVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Second", null);
            IVertex MillisecondVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Millisecond", null);

            YearVertex.Value = dt.Year;
            MonthVertex.Value = dt.Month;
            DayVertex.Value = dt.Day;
            HourVertex.Value = dt.Hour;
            MinuteVertex.Value = dt.Minute;
            SecondVertex.Value = dt.Second;
            MillisecondVertex.Value = dt.Millisecond;
        }

        public static System.DateTime VertexToDateTime(IVertex dtVertex)
        {
            IVertex YearVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Year", null);
            IVertex MonthVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Month", null);
            IVertex DayVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Day", null);
            IVertex HourVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Hour", null);
            IVertex MinuteVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Minute", null);
            IVertex SecondVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Second", null);
            IVertex MillisecondVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Millisecond", null);
            int yearValue = GraphUtil.GetIntegerValueOr0(YearVertex);
            int monthValue = GraphUtil.GetIntegerValueOr0(MonthVertex);
            int dayValue = GraphUtil.GetIntegerValueOr0(DayVertex);
            int hourValue = GraphUtil.GetIntegerValueOr0(HourVertex);
            int minuteValue = GraphUtil.GetIntegerValueOr0(MinuteVertex);
            int secondValue = GraphUtil.GetIntegerValueOr0(SecondVertex);
            int millisecondValue = GraphUtil.GetIntegerValueOr0(MillisecondVertex);

            return new System.DateTime(yearValue, monthValue, dayValue, hourValue, minuteValue, secondValue, millisecondValue);
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

            IVertex dateTimeVertex = GraphUtil.GetQueryOutFirst(stack, "dateTime", null);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            if (dateTimeVertex == null)
                return newStack;

            System.DateTime thisDate = VertexToDateTime(stack);
            System.DateTime compareDate = VertexToDateTime(dateTimeVertex);

            if (thisDate > compareDate)
                newStack.AddVertex(null, "True");
            else
                newStack.AddVertex(null, "False");

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex IsEarlierThan(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            // date

            IVertex dateTimeVertex = GraphUtil.GetQueryOutFirst(stack, "dateTime", null);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            if (dateTimeVertex == null)
                return newStack;

            System.DateTime thisDate = VertexToDateTime(stack);
            System.DateTime compareDate = VertexToDateTime(dateTimeVertex);

            if (thisDate < compareDate)
                newStack.AddVertex(null, "True");
            else
                newStack.AddVertex(null, "False");

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex IsEqual(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            // date

            IVertex dateTimeVertex = GraphUtil.GetQueryOutFirst(stack, "dateTime", null);

            INoInEdgeInOutVertexVertex newStack = InstructionHelpers.CreateStack();

            if (dateTimeVertex == null)
                return newStack;

            System.DateTime thisDate = VertexToDateTime(stack);
            System.DateTime compareDate = VertexToDateTime(dateTimeVertex);

            if (thisDate == compareDate)
                newStack.AddVertex(null, "True");
            else
                newStack.AddVertex(null, "False");

            return newStack;
        }

        public static INoInEdgeInOutVertexVertex Add(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            // date

            IVertex dtVertex = GraphUtil.GetQueryOutFirst(stack, "dateTime", null);            

            if (dtVertex == null)
                return stack;

            System.DateTime thisDate = VertexToDateTime(stack);

            //

            IVertex YearVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Year", null);
            IVertex MonthVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Month", null);
            IVertex DayVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Day", null);
            IVertex HourVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Hour", null);
            IVertex MinuteVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Minute", null);
            IVertex SecondVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Second", null);
            IVertex MillisecondVertex = GraphUtil.GetQueryOutFirst(dtVertex, "Millisecond", null);
            int yearValue = GraphUtil.GetIntegerValueOr0(YearVertex);
            int monthValue = GraphUtil.GetIntegerValueOr0(MonthVertex);
            int dayValue = GraphUtil.GetIntegerValueOr0(DayVertex);
            int hourValue = GraphUtil.GetIntegerValueOr0(HourVertex);
            int minuteValue = GraphUtil.GetIntegerValueOr0(MinuteVertex);
            int secondValue = GraphUtil.GetIntegerValueOr0(SecondVertex);
            int millisecondValue = GraphUtil.GetIntegerValueOr0(MillisecondVertex);

            //

            //System.DateTime compareDate = VertexToDateTime(dateTimeVertex);

            //thisDate = thisDate.Add(new TimeSpan(compareDate.Ticks));

            CopyDtToVertex(thisDate, stack);

            return stack;
        }

        public static INoInEdgeInOutVertexVertex Substract(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            // date

            IVertex dateTimeVertex = GraphUtil.GetQueryOutFirst(stack, "dateTime", null);

            if (dateTimeVertex == null)
                return stack;

            System.DateTime thisDate = VertexToDateTime(stack);
            System.DateTime compareDate = VertexToDateTime(dateTimeVertex);

            //thisDate = thisDate.Subtract(new TimeSpan(compareDate.Ticks));

            CopyDtToVertex(thisDate, stack);

            return stack;
        }

    }
}
