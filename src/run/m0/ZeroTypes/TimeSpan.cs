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
    public class TimeSpan
    {
        public static INoInEdgeInOutVertexVertex TimeSpan_constructor(IExecution exe)
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
        
    }
}
