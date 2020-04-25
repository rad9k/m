using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Midi
{
    public class MidiLib
    {
        public static virtual void NoteOn(int deviceNumber, int channel, int note, int velocity) { }

        public static virtual void NoteOff(int deviceNumber, int channel, int note, int velocity)
        {
            midiOut(getHandle(deviceNumber), channel, 0b1000, note, velocity);
        }

        public static virtual void ControlChange(int deviceNumber, int channel, int ccNumber, int ccValue)
        {
            midiOut(getHandle(deviceNumber), channel, 0b1011, ccNumber, ccValue);
        }

        public static virtual void ProgramChange(int deviceNumber, int channel, int program)
        {
            midiOut(getHandle(deviceNumber), channel, 0b1100, program, 0);
        }

        // 2000H center
        public static virtual void PitchBend(int deviceNumber, int channel, int value)
        {
            int high = value & 0b0111111100000000;
            int low = value & 0b0000000001111111;

            midiOut(getHandle(deviceNumber), channel, 0b1110, low, high);
        }

        public static virtual void Silent(int deviceNumber, int channel)
        {
            midiOut(getHandle(deviceNumber), channel, 0b1011, 120, 0); // sound off
            midiOut(getHandle(deviceNumber), channel, 0b1011, 123, 0); // all notes off
        }

        public static virtual void Reset(int deviceNumber)
        {
            midiOut(deviceNumber, 0b11111111);
        }

        public static virtual void TimingClock(int deviceNumber)
        {
            midiOut(deviceNumber, 0b11111000);
        }

        public static virtual void Start(int deviceNumber)
        {
            midiOut(deviceNumber, 0b11111010);
        }

        public static virtual void Continue(int deviceNumber)
        {
            midiOut(deviceNumber, 0b11111011);
        }

        public static virtual void Stop(int deviceNumber)
        {
            midiOut(deviceNumber, 0b11111100);
        }
    }
}
