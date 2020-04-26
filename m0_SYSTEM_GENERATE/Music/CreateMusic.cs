using m0.Foundation;
using m0.Graph;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static m0_SYSTEM_GENERATE.Util.GenerateUtil;

using static m0_SYSTEM_GENERATE.Program;

namespace m0_SYSTEM_GENERATE.Music
{
    public class CreateMusic
    {
        public static void Save(List<IVertex> systemSubGraphWithLinks, Dictionary<string, StoreId> storeOverride)
        {            
            print("* saving Lib\\Music");

            GeneralUtil.CreateM0AndMoveEdgesIntoIt_IncludeEverythingBesidesList("lib_music.m0", music, systemSubGraphWithLinks, storeOverride);                        
        }

        static IVertex music;

        public static void Create()
        {
            print("* creating Lib\\Music");

            IVertex r = m0.MinusZero.Instance.root;

            IVertex lib = r.Get(false, @"System\Lib");

            music = lib.AddVertex(null, "Music");

            string type = "m0_COMPOSER.Lib.Music, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            IVertex String = r.Get(false, @"System\Meta\ZeroTypes\String");
            IVertex Integer = r.Get(false, @"System\Meta\ZeroTypes\Integer");

            //

            IVertex midiDevice = GraphUtil.AddClass(music, "MidiDevice");

            GraphUtil.AddAttribute(midiDevice, "Name", String, 1, 1);

            GraphUtil.AddAttribute(midiDevice, "Mid", String, 0, 1);
            GraphUtil.AddAttribute(midiDevice, "Pid", String, 0, 1);
            GraphUtil.AddAttribute(midiDevice, "DriverVersion", String, 0, 1);
            GraphUtil.AddAttribute(midiDevice, "Technology", String, 0, 1);
            GraphUtil.AddAttribute(midiDevice, "Voices", String, 0, 1);
            GraphUtil.AddAttribute(midiDevice, "Notes", String, 0, 1);
            GraphUtil.AddAttribute(midiDevice, "ChannelMask", String, 0, 1);
            GraphUtil.AddAttribute(midiDevice, "Support", String, 0, 1);

            GraphUtil.AddAttribute(midiDevice, "DeviceNumber", Integer, 0, 1);

            AddMethod(midiDevice, "Reset", type, "Reset", null, new TypeName[] { });
            AddMethod(midiDevice, "TimingClock", type, "TimingClock", null, new TypeName[] { });
            AddMethod(midiDevice, "Start", type, "Start", null, new TypeName[] { });
            AddMethod(midiDevice, "Continue", type, "Continue", null, new TypeName[] { });
            AddMethod(midiDevice, "Stop", type, "Stop", null, new TypeName[] { });

            //

            IVertex midiOutput = GraphUtil.AddClass(music, "MidiOutput");
            GraphUtil.AddAttribute(midiOutput, "Device", midiDevice, 1, 1);
            GraphUtil.AddAttribute(midiDevice, "Channel", Integer, 1, 1);

            /*public static void NoteOn(int deviceNumber, int channel, int note, int velocity)
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
                int low = value & 0b0000000001111111;

                midiOut(deviceNumber, channel, 0b1110, low, high >> 7);
            }

            public static void Silent(int deviceNumber, int channel)
            {
                midiOut(deviceNumber, channel, 0b1011, 120, 0); // sound off
                midiOut(deviceNumber, channel, 0b1011, 123, 0); // all notes off
            }*/
        }
    }
}
