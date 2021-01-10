using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Midi
{
    public class Standard
    {
        public static int MidiTicksPerSixteen = 96;

        public static int MidiTicksPerBar = MidiTicksPerSixteen * 16;

        public static int MidiTicksPerBeat = MidiTicksPerSixteen * 4;
    }
}
