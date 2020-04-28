using m0.Foundation;
using m0.Graph;
using m0.Lib;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Lib
{
    public class Music
    {
        public static INoInEdgeInOutVertexVertex NoteOn(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.stack;

            IVertex noteV = GraphUtil.GetQueryOutFirst(stack, "note", null);

            if (noteV == null)
                return stack;

            bool isNull = false;

            int channel = LibUtil.GetIntFromVertex(stack, "Channel", ref isNull);

            IVertex device = GraphUtil.GetQueryOutFirst(stack, "Device", null);

            if (device == null)
                return stack;

            int deviceNumber = LibUtil.GetIntFromVertex(device, "DeviceNumber", ref isNull);

            int octave = LibUtil.GetIntFromVertex(noteV, "Octave", ref isNull);
            int note = LibUtil.GetIntFromVertex(noteV, "Note", ref isNull);
            int velocity = LibUtil.GetIntFromVertex(noteV, "Velocity", ref isNull);

            if (isNull)
                return stack;

            int noteFinal = 12 + note + (octave * 12);

            Midi.WinmmMidiLib.NoteOn(deviceNumber, channel, noteFinal, velocity);



            return stack;
        }
    }
}
