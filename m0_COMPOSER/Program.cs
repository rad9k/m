using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using m0_COMPOSER.Midi;
using System.Runtime.InteropServices;

namespace m0_COMPOSER
{
    class Program
    {
        static void Main(string[] args)
        {
            int numDevs = WinmmMidiLib.midiOutGetNumDevs();
            Console.WriteLine("You have {0} midi output devices", numDevs);

            //

            for (int x = 0; x < numDevs; x++)
            {
                MidiOutCaps myCaps = new MidiOutCaps();
                var res2 = WinmmMidiLib.midiOutGetDevCaps(x, ref myCaps, (UInt32)Marshal.SizeOf(myCaps));

                Console.WriteLine(myCaps.szPname + " " + myCaps.vDriverVersion);
            }

            //

            int handle = 0;
            int deviceNumber = 0;
            var res = WinmmMidiLib.midiOutOpen(ref handle, deviceNumber, null, 0, 0);


            for (int x = 0; x < 127; x++)
            {
                byte command = 0x90;
                byte note = (byte)x;
                byte velocity = 0x7F;
                int message = (velocity << 16) + (note << 8) + command;

                res = WinmmMidiLib.midiOutShortMsg(handle, message);

                Thread.Sleep(100);

                command = 0x80;

                message = (velocity << 16) + (note << 8) + command;

                //res = MidiLib.midiOutShortMsg(handle, message);
            }

            res = WinmmMidiLib.midiOutClose(handle);

            System.Console.In.Read();
        }
    }
}
