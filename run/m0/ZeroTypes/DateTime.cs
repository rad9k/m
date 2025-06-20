using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        
    }
}
