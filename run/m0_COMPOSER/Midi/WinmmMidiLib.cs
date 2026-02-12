// docs https://www.midi.org/specifications/item/table-1-summary-of-midi-message
//
// note table https://www.codeguru.com/columns/dotnet/making-music-with-midi-and-c.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Midi
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MidiOutCaps
    {
        public UInt16 wMid;
        public UInt16 wPid;
        public UInt32 vDriverVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public String szPname;

        public UInt16 wTechnology;
        public UInt16 wVoices;
        public UInt16 wNotes;
        public UInt16 wChannelMask;
        public UInt32 dwSupport;
    }
    public class WinmmMidiLib : MidiLib
    {
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder returnValue, int returnLength, IntPtr winHandle);

        [DllImport("winmm.dll")]
        public static extern int midiOutGetNumDevs();

        [DllImport("winmm.dll")]
        public static extern int midiOutGetDevCaps(Int32 uDeviceID, ref MidiOutCaps lpMidiOutCaps, UInt32 cbMidiOutCaps);

        [DllImport("winmm.dll", ExactSpelling = true)]
        private static extern int midiOutOpen(out IntPtr handle, int deviceID, IntPtr proc, IntPtr instance, uint flags);

        [DllImport("winmm.dll", ExactSpelling = true)]
        private static extern int midiOutShortMsg(IntPtr handle, uint message);

        [DllImport("winmm.dll", ExactSpelling = true)]
        private static extern int midiOutClose(IntPtr handle);

        public delegate void MidiCallBack(IntPtr handle, int msg, IntPtr instance, IntPtr param1, IntPtr param2);

        private static string Mci(string command)
        {
            int returnLength = 256;
            StringBuilder reply = new StringBuilder(returnLength);
            mciSendString(command, reply, returnLength, IntPtr.Zero);
            return reply.ToString();
        }

        static Dictionary<int, IntPtr> deviceHandles = new Dictionary<int, IntPtr>();

        static IntPtr getHandle(int deviceNumber)
        {
            if (deviceHandles.ContainsKey(deviceNumber))
                return deviceHandles[deviceNumber];

            IntPtr handle;
            int res = midiOutOpen(out handle, deviceNumber, IntPtr.Zero, IntPtr.Zero, 0);

            if (res != 0)
                throw new InvalidOperationException($"midiOutOpen failed for device {deviceNumber}, MMRESULT={res}");

            deviceHandles.Add(deviceNumber, handle);

            return handle;
        }

        public static void Close()
        {            
            foreach(IntPtr handle in deviceHandles.Values)
                midiOutClose(handle);

            deviceHandles.Clear();
        }
       
        static void midiOut(int deviceNumber, int message)
        {
            int res = midiOutShortMsg(getHandle(deviceNumber), unchecked((uint)message));
            if (res != 0)
                throw new InvalidOperationException($"midiOutShortMsg failed for device {deviceNumber}, MMRESULT={res}, msg=0x{message:X8}");
        }

        static void midiOut(int deviceNumber, int channel, int command, int note, int velocity)
        {
            byte _command = (byte) ( (command << 4) + channel);
            byte _note = (byte) note;
            byte _velocity = (byte) velocity;
            int message = (_velocity << 16) + (_note << 8) + _command;

            int res = midiOutShortMsg(getHandle(deviceNumber), unchecked((uint)message));
            if (res != 0)
                throw new InvalidOperationException($"midiOutShortMsg failed for device {deviceNumber}, MMRESULT={res}, msg=0x{message:X8}");
        }

        public static void NoteOn(int deviceNumber, int channel, int note, int velocity)
        {
            midiOut(deviceNumber, channel, 0b1001, note, velocity);
        }

        public static void NoteOff(int deviceNumber, int channel, int note, int velocity)
        {
            midiOut(deviceNumber, channel, 0b1000, note, velocity);
        }

        public static void ControlChange(int deviceNumber, int channel, int ccNumber, int ccValue)
        {
            midiOut(deviceNumber, channel, 0b1011, ccNumber, ccValue);
        }

        public static void ProgramChange(int deviceNumber, int channel, int program)
        {
            midiOut(deviceNumber, channel, 0b1100, program, 0);
        }

        // 2000H center
        public static void PitchBend(int deviceNumber, int channel, int value)
        {
            int high = value & 0b0011111110000000;
            int low = value &  0b0000000001111111;

            midiOut(deviceNumber, channel, 0b1110, low, high >> 7);
        }

        public static void Silent(int deviceNumber, int channel)
        {
            midiOut(deviceNumber, channel, 0b1011, 120, 0); // sound off
            midiOut(deviceNumber, channel, 0b1011, 123, 0); // all notes off
        }

        public static void Reset(int deviceNumber)
        {
            midiOut(deviceNumber, 0b11111111);
        }

        public static void TimingClock(int deviceNumber)
        {
            midiOut(deviceNumber, 0b11111000);
        }

        public static void Start(int deviceNumber)
        {
            midiOut(deviceNumber, 0b11111010);
        }

        public static void Continue(int deviceNumber)
        {
            midiOut(deviceNumber, 0b11111011);
        }

        public static void Stop(int deviceNumber)
        {
            midiOut(deviceNumber, 0b11111100);
        }
    }
}
