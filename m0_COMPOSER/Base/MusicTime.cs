using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Base
{
    public class MusicTime
    {   
        public Dictionary<IVertex, int> GetByTimeSpan(IVertex timeSpan)
        {
            return null;
        }

        public int Combined { get; set; }

        static int TicksPerBeat = Midi.Standard.MidiTicksPerSixteen * 4;

        public RealTime GetRealTime(double bpm)
        {
            double beats = ((double)Combined) / TicksPerBeat;

            double minutes = beats / bpm;

            RealTime rt = new RealTime();

            rt.Minutes = minutes;

            return rt;
        }

        public void SetRealTime(double bpm, RealTime realTime)
        {
            int beats = (int) (realTime.Minutes * bpm);

            Combined = beats * TicksPerBeat;
        }
    }
}
