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

            IVertex MidiOutput = GraphUtil.AddClass(music, "MidiOutput");

            //

            IVertex MidiDevice = GraphUtil.AddClass(music, "MidiDevice");

            GraphUtil.AddAttribute(MidiDevice, "Name", String, 1, 1);

            GraphUtil.AddAggregation(MidiDevice, "Output", MidiOutput, 0, -1);            

            GraphUtil.AddAttribute(MidiDevice, "DeviceNumber", Integer, 0, 1);

            GraphUtil.AddAttribute(MidiDevice, "Mid", String, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "Pid", String, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "DriverVersion", String, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "Technology", String, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "Voices", String, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "Notes", String, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "ChannelMask", String, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "Support", String, 0, 1);            

            AddMethod(MidiDevice, "Reset", type, "Reset", null, new TypeName[] { });
            AddMethod(MidiDevice, "TimingClock", type, "TimingClock", null, new TypeName[] { });
            AddMethod(MidiDevice, "Start", type, "Start", null, new TypeName[] { });
            AddMethod(MidiDevice, "Continue", type, "Continue", null, new TypeName[] { });
            AddMethod(MidiDevice, "Stop", type, "Stop", null, new TypeName[] { });

            //
            
            GraphUtil.AddAssociation(MidiOutput, "Device", MidiDevice, 1, 1);
            GraphUtil.AddAttribute(MidiOutput, "Name", String, 1, 1);
            GraphUtil.AddAttribute(MidiOutput, "Channel", Integer, 1, 1);

            AddMethod(MidiOutput, "NoteOn", type, "NoteOn", null, new TypeName[] { new TypeName("note", Note, 1, 1) });
            AddMethod(MidiOutput, "NoteOff", type, "NoteOff", null, new TypeName[] { new TypeName("note", Note, 1, 1) });
            AddMethod(MidiOutput, "ControlChange", type, "ControlChange", null, new TypeName[] { new TypeName("ccNumber", "Integer", 1, 1), new TypeName("ccValue", "Integer", 1, 1) });
            AddMethod(MidiOutput, "ProgramChange", type, "ProgramChange", null, new TypeName[] { new TypeName("programNumber", "Integer", 1, 1) });
            AddMethod(MidiOutput, "PitchBend", type, "PitchBend", null, new TypeName[] { new TypeName("value", "Integer", 1, 1) });
            AddMethod(MidiOutput, "Silent", type, "Silent", null, new TypeName[] { });            
        }
    }
}
