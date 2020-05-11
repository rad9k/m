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
            IVertex Boolean = r.Get(false, @"System\Meta\ZeroTypes\Boolean");
            IVertex Color = r.Get(false, @"System\Meta\ZeroTypes\Color");

            //

            IVertex NoteOutput = GraphUtil.AddClass(music, "NoteOutput");
            IVertex NoteInput = GraphUtil.AddClass(music, "NoteInput");

            // HAS LENGTH

            IVertex HasLenth = GraphUtil.AddClass(music, "HasLength");

            GraphUtil.AddAttribute(HasLenth, "Length", Integer, 1, 1);

            // EVENT

            IVertex Event = GraphUtil.AddClass(music, "Event");

            GraphUtil.AddAttribute(Event, "TriggerTime", Integer, 1, 1);

            // HISTORY

            IVertex History = GraphUtil.AddClass(music, "History");

            GraphUtil.AddAttribute(History, "Event", Event, 0, -1);

            // CONTROLCHANGE

            IVertex ControlChange = GraphUtil.AddClass(music, "ControlChange");

            GraphUtil.AddAttribute(ControlChange, "Number", Integer, 1, 1);
            GraphUtil.AddAttribute(ControlChange, "Value", Integer, 1, 1);

            // CONTROLCHANGEEVENT

            IVertex ControlChangeEvent = GraphUtil.AddClass(music, "ControlChangeEvent");

            GraphUtil.AddInherits(ControlChangeEvent, ControlChange);
            GraphUtil.AddInherits(ControlChangeEvent, Event);

            // PICH

            IVertex Pitch = GraphUtil.AddClass(music, "Pitch");

            GraphUtil.AddAttribute(Pitch, "Octave", Integer, 1, 1);
            GraphUtil.AddAttribute(Pitch, "Note", Integer, 1, 1);

            // VISULISEDPICH

            IVertex VisualisedPitch = GraphUtil.AddClass(music, "VisualisedPitch");

            GraphUtil.AddInherits(VisualisedPitch, Pitch);
            GraphUtil.AddAttribute(VisualisedPitch, "Name", String, 1, 1);
            GraphUtil.AddAttribute(VisualisedPitch, "Color", Color, 1, 1);

            // PICHSET

            IVertex PitchSet = GraphUtil.AddClass(music, "PitchSet");

            GraphUtil.AddAttribute(PitchSet, "Pitch", Pitch, 0, -1);

            // TIMESPANLEVEL

            IVertex TimeSpanLevel = GraphUtil.AddClass(music, "TimeSpanLevel");

            GraphUtil.AddInherits(TimeSpanLevel, HasLenth);
            GraphUtil.AddAttribute(TimeSpanLevel, "Name", String, 1, 1);
            GraphUtil.AddAttribute(TimeSpanLevel, "SubLevel", TimeSpanLevel, 0, -1);


            // NOTE

            IVertex Note = GraphUtil.AddClass(music, "Note");

            GraphUtil.AddInherits(Note, Pitch);

            GraphUtil.AddAttribute(Note, "Velocity", Integer, 1, 1);

            // NOTEEVENT

            IVertex NoteEvent = GraphUtil.AddClass(music, "NoteEvent");

            GraphUtil.AddInherits(NoteEvent, Note);
            GraphUtil.AddInherits(NoteEvent, Event);
            GraphUtil.AddInherits(NoteEvent, HasLenth);

            // SEQUENCE

            IVertex Sequence = GraphUtil.AddClass(music, "Sequnce");

            GraphUtil.AddInherits(Sequence, HasLenth);
            GraphUtil.AddInherits(Sequence, History);

            GraphUtil.AddAttribute(Sequence, "IsDrum", Boolean, 0, 1);
            GraphUtil.AddAttribute(Sequence, "PitchSet", PitchSet, 0, 1);

            // SEQUENCEOPERATOR

            IVertex SequenceOperator = GraphUtil.AddClass(music, "SequnceOperator");

            GraphUtil.AddInherits(SequenceOperator, Sequence);

            // NOTEOUTPUTOPERATOR

            IVertex NoteOutputOperator = GraphUtil.AddClass(music, "NoteOutputOperator");

            GraphUtil.AddInherits(NoteOutputOperator, NoteOutput);

            // SEQUENCEEVENT

            IVertex SequenceEvent = GraphUtil.AddClass(music, "SequenceEvent");

            GraphUtil.AddInherits(SequenceEvent, Event);
            GraphUtil.AddAttribute(SequenceEvent, "Sequence", Sequence, 1, 1);

            // TRACK

            IVertex Track = GraphUtil.AddClass(music, "Track");

            GraphUtil.AddAttribute(Track, "Name", String, 0, 1);
            GraphUtil.AddAttribute(Track, "Output", NoteOutput, 0, 1);
            GraphUtil.AddAttribute(Track, "SequenceEvent", SequenceEvent, 0, -1);

            // SONG

            IVertex Song = GraphUtil.AddClass(music, "Song");

            GraphUtil.AddInherits(Song, HasLenth);

            GraphUtil.AddAttribute(Song, "Name", String, 0, 1);
            GraphUtil.AddAttribute(Song, "Track", Track, 0, -1);
            GraphUtil.AddAttribute(Song, "Input", NoteInput, 0, 1);
            GraphUtil.AddAttribute(Song, "RecordingTrack", Track, 0, 1);
            GraphUtil.AddAttribute(Song, "Tempo", Integer, 0, 1);

            AddMethod(Song, "Record", type, "Record", null, new TypeName[] { });
            AddMethod(Song, "Play", type, "Play", null, new TypeName[] { });
            AddMethod(Song, "Stop", type, "Stop", null, new TypeName[] { });
            AddMethod(Song, "Pause", type, "Pause", null, new TypeName[] { });
            AddMethod(Song, "MoveTo", type, "MoveTo", null, new TypeName[] { new TypeName("position", "Integer", 1, 1) });


            // NOTEOUTPUT continuation

            AddMethod(NoteOutput, "NoteOn", type, "NoteOn", null, new TypeName[] { new TypeName("note", Note, 1, 1) });
            AddMethod(NoteOutput, "NoteOff", type, "NoteOff", null, new TypeName[] { new TypeName("note", Note, 1, 1) });
            AddMethod(NoteOutput, "ControlChange", type, "ControlChange", null, new TypeName[] { new TypeName("controlChange", ControlChange, 1, 1) });
            AddMethod(NoteOutput, "ProgramChange", type, "ProgramChange", null, new TypeName[] { new TypeName("programNumber", "Integer", 1, 1) });
            AddMethod(NoteOutput, "PitchBend", type, "PitchBend", null, new TypeName[] { new TypeName("value", "Integer", 1, 1) });
            AddMethod(NoteOutput, "Silent", type, "Silent", null, new TypeName[] { });

            IVertex MidiOutput = GraphUtil.AddClass(music, "MidiOutput");
            IVertex MidiInput = GraphUtil.AddClass(music, "MidiInput");

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

            // MIDI OUT

            GraphUtil.AddInherits(MidiOutput, NoteOutput);

            GraphUtil.AddAssociation(MidiOutput, "Device", MidiDevice, 1, 1);
            GraphUtil.AddAttribute(MidiOutput, "Name", String, 1, 1);
            GraphUtil.AddAttribute(MidiOutput, "Channel", Integer, 1, 1);

            // MIDI IN

            GraphUtil.AddInherits(MidiInput, NoteInput);

            GraphUtil.AddAssociation(MidiInput, "Device", MidiDevice, 1, 1);
            GraphUtil.AddAttribute(MidiInput, "Name", String, 1, 1);
            GraphUtil.AddAttribute(MidiInput, "Channel", Integer, 1, 1);


            // MUSICSPACE

            IVertex MusicSpace = GraphUtil.AddClass(music, "MusicSpace");

            GraphUtil.AddAttribute(MusicSpace, "Sequence", Sequence, 0, -1);
            GraphUtil.AddAttribute(MusicSpace, "Song", Song, 0, -1);
        }
    }
}
