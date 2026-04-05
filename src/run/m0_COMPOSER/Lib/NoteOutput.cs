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
    public class NoteOutput
    {
        static int ToNoteNumber(int octave, int note)
        {
            return 12 + note + (octave * 12);
        }

        public static INoInEdgeInOutVertexVertex NoteOn(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

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

            int noteNumber = ToNoteNumber(octave, note);

            if (channel == 1)
                Midi.WinmmMidiLib.ProgramChange(deviceNumber, 1, 10);

            Midi.WinmmMidiLib.NoteOn(deviceNumber, channel, noteNumber, velocity);

            return stack;
        }

        public static INoInEdgeInOutVertexVertex NoteOff(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

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

            int noteNumber = ToNoteNumber(octave, note);

            Midi.WinmmMidiLib.NoteOff(deviceNumber, channel, noteNumber, velocity);

            return stack;
        }

        public static INoInEdgeInOutVertexVertex ControlChange(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex controlChange = GraphUtil.GetQueryOutFirst(stack, "controlChange", null);

            if (controlChange == null)
                return stack;

            bool isNull = false;

            int number = LibUtil.GetIntFromVertex(controlChange, "Number", ref isNull);
            int value = LibUtil.GetIntFromVertex(controlChange, "Value", ref isNull);

            int channel = LibUtil.GetIntFromVertex(stack, "Channel", ref isNull);

            IVertex device = GraphUtil.GetQueryOutFirst(stack, "Device", null);

            if (device == null)
                return stack;

            int deviceNumber = LibUtil.GetIntFromVertex(device, "DeviceNumber", ref isNull);

            if (isNull)
                return stack;

            Midi.WinmmMidiLib.ControlChange(deviceNumber, channel, number, value);

            return stack;
        }

        public static INoInEdgeInOutVertexVertex ProgramChange(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;            

            bool isNull = false;

            int programNumber = LibUtil.GetIntFromVertex(stack, "programNumber", ref isNull);

            int channel = LibUtil.GetIntFromVertex(stack, "Channel", ref isNull);

            IVertex device = GraphUtil.GetQueryOutFirst(stack, "Device", null);

            if (device == null)
                return stack;

            int deviceNumber = LibUtil.GetIntFromVertex(device, "DeviceNumber", ref isNull);            

            if (isNull)
                return stack;            

            Midi.WinmmMidiLib.ProgramChange(deviceNumber, channel, programNumber);

            return stack;
        }

        public static INoInEdgeInOutVertexVertex PitchBend(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;            

            bool isNull = false;

            int value = LibUtil.GetIntFromVertex(stack, "value", ref isNull);

            int channel = LibUtil.GetIntFromVertex(stack, "Channel", ref isNull);

            IVertex device = GraphUtil.GetQueryOutFirst(stack, "Device", null);

            if (device == null)
                return stack;

            int deviceNumber = LibUtil.GetIntFromVertex(device, "DeviceNumber", ref isNull);            

            if (isNull)
                return stack;

            Midi.WinmmMidiLib.PitchBend(deviceNumber, channel, value);

            return stack;
        }

        public static INoInEdgeInOutVertexVertex Silent(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;            

            bool isNull = false;

            int channel = LibUtil.GetIntFromVertex(stack, "Channel", ref isNull);

            IVertex device = GraphUtil.GetQueryOutFirst(stack, "Device", null);

            if (device == null)
                return stack;

            int deviceNumber = LibUtil.GetIntFromVertex(device, "DeviceNumber", ref isNull);            

            if (isNull)
                return stack;


            Midi.WinmmMidiLib.Silent(deviceNumber, channel);            

            return stack;
        }
    }
}
