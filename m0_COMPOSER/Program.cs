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
            /*int numDevs = WinmmMidiLib.midiOutGetNumDevs();
            Console.WriteLine("You have {0} midi output devices", numDevs);

            //

            for (int x = 0; x < numDevs; x++)
            {
                MidiOutCaps myCaps = new MidiOutCaps();
                var res2 = WinmmMidiLib.midiOutGetDevCaps(x, ref myCaps, (UInt32)Marshal.SizeOf(myCaps));

                Console.WriteLine(myCaps.szPname + " " + myCaps.vDriverVersion);
            }
            */
            //


            /*for (int x = 0; x < 127; x++)
            {
              WinmmMidiLib.ProgramChange(0, 0, x);

              WinmmMidiLib.NoteOn(0, 0, 60, 127);

                Thread.Sleep(100);

              WinmmMidiLib.NoteOn(0, 0, 62, 127);

              Thread.Sleep(100);

              WinmmMidiLib.NoteOn(0, 0, 64, 127);

              Thread.Sleep(200);
          }   */

            WinmmMidiLib.ProgramChange(0, 0, 90);

            WinmmMidiLib.NoteOn(0, 0, 60, 127);

            //for (int x = 0x2000; x < 0x4000; x+=100)
            for (int x = 0; x < 127; x += 1)
            {
                WinmmMidiLib.ControlChange(0, 0, 77, x);

                WinmmMidiLib.NoteOn(0, 0, 60, 127);

                Thread.Sleep(30);
            }


            WinmmMidiLib.Close();

            System.Console.WriteLine("END!!!!!!!!!!!!!!!");
            System.Console.In.Read();
        }
    }
}
