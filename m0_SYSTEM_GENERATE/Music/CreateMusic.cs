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

            IVertex Pitch = GraphUtil.AddClass(music, "Pitch");

            GraphUtil.AddAttribute(Pitch, "Octave", Integer, 1, 1);
            GraphUtil.AddAttribute(Pitch, "Note", Integer, 1, 1);

            //

            IVertex Note = GraphUtil.AddClass(music, "Note");

            GraphUtil.AddInherits(Note, Pitch);

            GraphUtil.AddAttribute(Note, "Velocity", Integer, 1, 1);

            //

            IVertex NoteEvent = GraphUtil.AddClass(music, "NoteEvent");

            GraphUtil.AddInherits(NoteEvent, Note);

            GraphUtil.AddAttribute(NoteEvent, "OnTime", Integer, 1, 1);
            GraphUtil.AddAttribute(NoteEvent, "OffTime", Integer, 1, 1);

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
            GraphUtil.AddAttribute(midiOutput, "Channel", Integer, 1, 1);

            AddMethod(midiOutput, "NoteOn", type, "NoteOn", null, new TypeName[] { new TypeName("note", Note, 1, 1) });
            AddMethod(midiOutput, "NoteOff", type, "NoteOff", null, new TypeName[] { new TypeName("note", Note, 1, 1) });
            AddMethod(midiOutput, "ControlChange", type, "ControlChange", null, new TypeName[] { new TypeName("ccNumber", "Integer", 1, 1), new TypeName("ccValue", "Integer", 1, 1) });
            AddMethod(midiOutput, "ProgramChange", type, "ProgramChange", null, new TypeName[] { new TypeName("programNumber", "Integer", 1, 1) });
            AddMethod(midiOutput, "PitchBend", type, "PitchBend", null, new TypeName[] { new TypeName("value", "Integer", 1, 1) });
            AddMethod(midiOutput, "Silent", type, "Silent", null, new TypeName[] { });            
        }
    }
}
