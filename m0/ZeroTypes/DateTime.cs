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
            IVertex sm = MinusZero.Instance.Root.Get(@"System\Meta");

            basevertex.AddVertex(sm.Get(@"ZeroTypes\DateTime\Year"),datetime.Year);            
            basevertex.AddVertex(sm.Get(@"ZeroTypes\DateTime\Month"),datetime.Month);
            basevertex.AddVertex(sm.Get(@"ZeroTypes\DateTime\Day"),datetime.Day);
            basevertex.AddVertex(sm.Get(@"ZeroTypes\DateTime\Hour"),datetime.Hour);
            basevertex.AddVertex(sm.Get(@"ZeroTypes\DateTime\Minute"),datetime.Minute);
            basevertex.AddVertex(sm.Get(@"ZeroTypes\DateTime\Second"),datetime.Second);
            basevertex.AddVertex(sm.Get(@"ZeroTypes\DateTime\Millisecond"), datetime.Millisecond);
        }
    }
}
