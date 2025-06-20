using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Midi
{
    public class MidiLib
    {
        public static void NoteOn(int deviceNumber, int channel, int note, int velocity) { }

        public static void NoteOff(int deviceNumber, int channel, int note, int velocity) { }

        public static void ControlChange(int deviceNumber, int channel, int ccNumber, int ccValue) { }

        public static void ProgramChange(int deviceNumber, int channel, int program) { }
        
        public static void PitchBend(int deviceNumber, int channel, int value) { }

        public static void Silent(int deviceNumber, int channel) { }

        public static void Reset(int deviceNumber) { }

        public static void TimingClock(int deviceNumber) { }

        public static void Start(int deviceNumber) { }

        public static void Continue(int deviceNumber) { }

        public static void Stop(int deviceNumber) { }
    }
}
