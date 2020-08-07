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
using m0.ZeroTypes;

namespace m0_SYSTEM_GENERATE.Music
{
    public class CreateMusic
    {
        public static void Save(List<IVertex> systemSubGraphWithLinks, Dictionary<string, StoreId> storeOverride)
        {            
            print("* saving Lib\\Music");

            GeneralUtil.CreateM0AndMoveEdgesIntoIt_IncludeEverythingBesidesList("lib_music.m0", Music, systemSubGraphWithLinks, storeOverride);                        
        }

        static IVertex Music;

        static IVertex Data;

        static IVertex PitchSet;

        static IVertex VisualisedPitch;

        class _Note
        {
            public string Name;
            public string Instrument;

            public _Note(String _Name, string _Instrument)
            {
                Name = _Name;
                Instrument = _Instrument;
            }
        }

        static _Note[] DrumInstruments = new _Note[] {
        new _Note("B 0", "Acoustic Bass Drum"),
        new _Note("C 1", "Bass Drum 1"),
        new _Note("C# 1", "Side Stick"),
        new _Note("D 1", "Acoustic Snare"),
        new _Note("D# 1", "Hand Clap"),
        new _Note("E 1", "Electric Snare"),
        new _Note("F 1", "Low Floor Tom"),
        new _Note("F# 1", "Closed Hi Hat"),
        new _Note("G 1", "High Floor Tom"),
        new _Note("G# 1", "Pedal Hi-Hat"),
        new _Note("A 1", "Low Tom"),
        new _Note("A# 1", "Open Hi-Hat"),
        new _Note("B 1", "Low-Mid Tom"),
        new _Note("C 2", "Hi Mid Tom"),
        new _Note("C# 2", "Crash Cymbal 1"),
        new _Note("D 2", "High Tom"),
        new _Note("D#2", "Ride Cymbal 1"),
        new _Note("E 2", "Chinese Cymbal"),
        new _Note("F 2", "Ride Bell"),
        new _Note("F# 2", "Tambourine"),
        new _Note("G 2", "Splash Cymbal"),
        new _Note("G# 2", "Cowbell"),
        new _Note("A 2", "Crash Cymbal 2"),
        new _Note("A# 2", "Vibraslap"),
        new _Note("B 2", "Ride Cymbal 2"),
        new _Note("C 3", "Hi Bongo"),
        new _Note("C# 3", "Low Bongo"),
        new _Note("D 3", "Mute Hi Conga"),
        new _Note("D# 3", "Open Hi Conga"),
        new _Note("E 3", "Low Conga"),
        new _Note("F 3", "High Timbale"),
        new _Note("F# 3", "Low Timbale"),
        new _Note("G 3", "High Agogo"),
        new _Note("G# 3", "Low Agogo"),
        new _Note("A 3", "Cabasa"),
        new _Note("A# 3", "Maracas"),
        new _Note("B 3", "Short Whistle"),
        new _Note("C 4", "Long Whistle"),
        new _Note("C# 4", "Short Guiro"),
        new _Note("D 4", "Long Guiro"),
        new _Note("D# 4", "Claves"),
        new _Note("E 4", "Hi Wood Block"),
        new _Note("F 4", "Low Wood Block"),
        new _Note("F# 4", "Mute Cuica"),
        new _Note("G 4", "Open Cuica"),
        new _Note("G# 4", "Mute Triangle"),
        new _Note("A 4", "Open Triangle") };

        public static void Create()
        {
            print("* creating Lib\\Music");

            IVertex r = m0.MinusZero.Instance.root;

            IVertex lib = r.Get(false, @"System\Lib");

            Music = lib.AddVertex(null, "Music");

            AddClasses();

            AddMetaEdges();

            AddData();
        }

        private static void AddData()
        {
            IVertex r = m0.MinusZero.Instance.root;

            Data = Music.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Package"), "Data");

            AddBasePitchSet();

            AddBaseDrumSet();

            AddBaseTimeSpanStructure();
        }        

        private static void AddBasePitchSet()
        {
            IVertex r = m0.MinusZero.Instance.root;

            IVertex b=VertexOperations.AddInstance(Data, PitchSet);

            Data.AddEdge(Music.Get(false, "DefaultPitchSet"), b);

            b.Value = "BasePitchSet";

            IVertex white = r.Get(false, @"System\Data\UX\Colors\White");
            IVertex black = r.Get(false, @"System\Data\UX\Colors\Black");
            IVertex gray = r.Get(false, @"System\Data\UX\Colors\LightGray");

            for (int x = -1; x <= 9; x++) {
                AddPitch(b, x, 0, "C " + x.ToString(), white, null);
                AddPitch(b, x, 1, "C# " + x.ToString(), black, gray);
                AddPitch(b, x, 2, "D " + x.ToString(), white, null);
                AddPitch(b, x, 3, "D# " + x.ToString(), black, gray);
                AddPitch(b, x, 4, "E " + x.ToString(), white, null);
                AddPitch(b, x, 5, "F " + x.ToString(), white, null);
                AddPitch(b, x, 6, "F# " + x.ToString(), black, gray);
                AddPitch(b, x, 7, "G " + x.ToString(), white, null);
                AddPitch(b, x, 8, "G# " + x.ToString(), black, gray);
                AddPitch(b, x, 9, "A " + x.ToString(), white, null);
                AddPitch(b, x, 10, "A# " + x.ToString(), black, gray);
                AddPitch(b, x, 11, "B " + x.ToString(), white, null);               
            }
        }

        private static void AddBaseDrumSet()
        {
            IVertex r = m0.MinusZero.Instance.root;

            IVertex b = VertexOperations.AddInstance(Data, PitchSet);

            Data.AddEdge(Music.Get(false, "DefaultDrumPitchSet"), b);

            b.Value = "DrumBasePitchSet";

            IVertex white = r.Get(false, @"System\Data\UX\Colors\White");
            IVertex black = r.Get(false, @"System\Data\UX\Colors\Black");

            for (int x = -1; x <= 9; x++)
            {
                AddDrumPitch(b, x, 0, "C " + x.ToString(), white);
                AddDrumPitch(b, x, 1, "C# " + x.ToString(), white);
                AddDrumPitch(b, x, 2, "D " + x.ToString(), white);
                AddDrumPitch(b, x, 3, "D# " + x.ToString(), white);
                AddDrumPitch(b, x, 4, "E " + x.ToString(), white);
                AddDrumPitch(b, x, 5, "F " + x.ToString(), white);
                AddDrumPitch(b, x, 6, "F# " + x.ToString(), white);
                AddDrumPitch(b, x, 7, "G " + x.ToString(), white);
                AddDrumPitch(b, x, 8, "G# " + x.ToString(), white);
                AddDrumPitch(b, x, 9, "A " + x.ToString(), white);
                AddDrumPitch(b, x, 10, "A# " + x.ToString(), white);
                AddDrumPitch(b, x, 11, "B " + x.ToString(), white);
            }
        }

        private static void AddPitch(IVertex basePitch, int octave, int note, string name, IVertex color, IVertex noteBackgroundColor)
        {
            IVertex p = VertexOperations.AddInstance(basePitch, VisualisedPitch);

            p.Value = name;

            GraphUtil.SetVertexValue(p, VisualisedPitch.Get(false, "Name"), name);
            GraphUtil.SetVertexValue(p, VisualisedPitch.Get(false, "Octave"), octave);
            GraphUtil.SetVertexValue(p, VisualisedPitch.Get(false, "Note"), note);
            GraphUtil.CreateOrReplaceEdge(p, VisualisedPitch.Get(false, "Color"), color);

            if(noteBackgroundColor != null)
                GraphUtil.CreateOrReplaceEdge(p, VisualisedPitch.Get(false, "NoteBackgroundColor"), noteBackgroundColor);
        }

        private static void AddDrumPitch(IVertex basePitch, int octave, int note, string name, IVertex color)
        {
            _Note matched = null;

            foreach (_Note n in DrumInstruments)
                if (n.Name == name)
                    matched = n;

            if (matched == null)
                AddPitch(basePitch, octave, note, name, color, null);
            else
                AddPitch(basePitch, octave, note, name + " (" + matched.Instrument + ")", color, null);
                
        }

        private static IVertex AddTimeSpan(IVertex _base, String name, int length, IVertex meta)
        {
            IVertex v = VertexOperations.AddInstance(_base, Music.Get(false, "TimeSpanLevel"), meta);

            v.Value = name;

            GraphUtil.SetVertexValue(v, Music.Get(false, @"TimeSpanLevel\Length"), length);

            return v;
        }

        private static void AddBaseTimeSpanStructure()
        {
            IVertex MidiTick = AddTimeSpan(Data, "MidiTick", 1, Music.Get(false, "TimeSpanLevel"));

            Data.AddEdge(Music.Get(false, "BaseTimeSpanLevel"), MidiTick);

            IVertex tact = AddTimeSpan(Data, "Tact", 16, Music.Get(false, @"TimeSpanLevel"));

            Data.AddEdge(Music.Get(false, "DefaultTimeSpanLevel"), tact);

            IVertex sixteen = AddTimeSpan(tact, "Sixteen", 96, Music.Get(false, @"TimeSpanLevel\SubLevel"));

            sixteen.AddEdge(Music.Get(false, @"TimeSpanLevel\SubLevel"), MidiTick);            
        }

        private static void AddMetaEdges()
        {
            GraphUtil.AddMetaEdge(Music, "DefaultPitchSet", Music.Get(false, "PitchSet"));

            GraphUtil.AddMetaEdge(Music, "DefaultDrumPitchSet", Music.Get(false, "PitchSet"));

            GraphUtil.AddMetaEdge(Music, "BaseTimeSpanLevel", Music.Get(false, "TimeSpanLevel"));

            GraphUtil.AddMetaEdge(Music, "DefaultTimeSpanLevel", Music.Get(false, "TimeSpanLevel"));
        }

        private static void AddClasses() {
            IVertex r = m0.MinusZero.Instance.root;

            string type = "m0_COMPOSER.Lib.Music, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            IVertex String = r.Get(false, @"System\Meta\ZeroTypes\String");
            IVertex Integer = r.Get(false, @"System\Meta\ZeroTypes\Integer");
            IVertex Boolean = r.Get(false, @"System\Meta\ZeroTypes\Boolean");
            IVertex Color = r.Get(false, @"System\Meta\ZeroTypes\Color");

            // vertex stubs

            IVertex NoteOutput = GraphUtil.AddClass(Music, "NoteOutput");
            IVertex NoteInput = GraphUtil.AddClass(Music, "NoteInput");

            IVertex TimeSpanLevel = GraphUtil.AddClass(Music, "TimeSpanLevel");

            // HAS LENGTH

            IVertex HasLenth = GraphUtil.AddClass(Music, "HasLength");

            GraphUtil.AddAttribute(HasLenth, "Length", Integer, 0, 1);
            GraphUtil.AddAssociation(HasLenth, "TimeSpan", TimeSpanLevel, 0, 1);

            // EVENT

            IVertex Event = GraphUtil.AddClass(Music, "Event");

            GraphUtil.AddAttribute(Event, "TriggerTime", Integer, 1, 1);

            // HISTORY

            IVertex History = GraphUtil.AddClass(Music, "History");

            GraphUtil.AddAttribute(History, "Event", Event, 0, -1);

            // CONTROLCHANGE

            IVertex ControlChange = GraphUtil.AddClass(Music, "ControlChange");

            GraphUtil.AddAttribute(ControlChange, "Number", Integer, 1, 1);
            GraphUtil.AddAttribute(ControlChange, "Value", Integer, 1, 1);

            // CONTROLCHANGEEVENT

            IVertex ControlChangeEvent = GraphUtil.AddClass(Music, "ControlChangeEvent");

            GraphUtil.AddInherits(ControlChangeEvent, ControlChange);
            GraphUtil.AddInherits(ControlChangeEvent, Event);

            // PICH

            IVertex Pitch = GraphUtil.AddClass(Music, "Pitch");

            GraphUtil.AddAttribute(Pitch, "Octave", Integer, 1, 1);
            GraphUtil.AddAttribute(Pitch, "Note", Integer, 1, 1);

            // VISULISEDPICH

            VisualisedPitch = GraphUtil.AddClass(Music, "VisualisedPitch");

            GraphUtil.AddInherits(VisualisedPitch, Pitch);
            GraphUtil.AddAttribute(VisualisedPitch, "Name", String, 1, 1);
            GraphUtil.AddAttribute(VisualisedPitch, "Color", Color, 1, 1);
            GraphUtil.AddAttribute(VisualisedPitch, "NoteBackgroundColor", Color, 0, 1);

            // PICHSET

            PitchSet = GraphUtil.AddClass(Music, "PitchSet");

            GraphUtil.AddAggregation(PitchSet, "Pitch", Pitch, 0, -1);

            // TIMESPANLEVEL            

            GraphUtil.AddInherits(TimeSpanLevel, HasLenth);
            GraphUtil.AddAggregation(TimeSpanLevel, "SubLevel", TimeSpanLevel, 0, 1);


            // NOTE

            IVertex Note = GraphUtil.AddClass(Music, "Note");

            GraphUtil.AddInherits(Note, Pitch);

            GraphUtil.AddAttribute(Note, "Velocity", Integer, 1, 1);

            // NOTEEVENT

            IVertex NoteEvent = GraphUtil.AddClass(Music, "NoteEvent");

            GraphUtil.AddInherits(NoteEvent, Note);
            GraphUtil.AddInherits(NoteEvent, Event);
            GraphUtil.AddInherits(NoteEvent, HasLenth);

            // SEQUENCE

            IVertex Sequence = GraphUtil.AddClass(Music, "Sequence");

            GraphUtil.AddInherits(Sequence, HasLenth);
            GraphUtil.AddInherits(Sequence, History);

            GraphUtil.AddAttribute(Sequence, "IsDrum", Boolean, 0, 1);
            GraphUtil.AddAttribute(Sequence, "ExtendTimeLength", Integer, 1, 1, 16*96);
            GraphUtil.AddAssociation(Sequence, "PitchSet", PitchSet, 0, 1);

            Sequence.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$DefaultOpenVisualiser"), r.Get(false, @"System\Meta\Visualiser\Sequence"));
           

            // SEQUENCEOPERATOR

            IVertex SequenceOperator = GraphUtil.AddClass(Music, "SequnceOperator");

            GraphUtil.AddInherits(SequenceOperator, Sequence);

            // NOTEOUTPUTOPERATOR

            IVertex NoteOutputOperator = GraphUtil.AddClass(Music, "NoteOutputOperator");

            GraphUtil.AddInherits(NoteOutputOperator, NoteOutput);

            // SEQUENCEEVENT

            IVertex SequenceEvent = GraphUtil.AddClass(Music, "SequenceEvent");

            GraphUtil.AddInherits(SequenceEvent, Event);
            GraphUtil.AddAttribute(SequenceEvent, "Sequence", Sequence, 1, 1);

            // TRACK

            IVertex Track = GraphUtil.AddClass(Music, "Track");

            //GraphUtil.AddAttribute(Track, "Name", String, 0, 1);
            GraphUtil.AddAttribute(Track, "Output", NoteOutput, 0, 1);
            GraphUtil.AddAssociation(Track, "SequenceEvent", SequenceEvent, 0, -1);

            // SONG

            IVertex Song = GraphUtil.AddClass(Music, "Song");

            GraphUtil.AddInherits(Song, HasLenth);

            //GraphUtil.AddAttribute(Song, "Name", String, 0, 1);
            GraphUtil.AddAggregation(Song, "Track", Track, 0, -1);
            GraphUtil.AddAttribute(Song, "Input", NoteInput, 0, 1);
            GraphUtil.AddAssociation(Song, "RecordingTrack", Track, 0, 1);
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

            IVertex MidiOutput = GraphUtil.AddClass(Music, "MidiOutput");
            IVertex MidiInput = GraphUtil.AddClass(Music, "MidiInput");

            //

            IVertex MidiDevice = GraphUtil.AddClass(Music, "MidiDevice");

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

            IVertex MusicSpace = GraphUtil.AddClass(Music, "MusicSpace");

            GraphUtil.AddAggregation(MusicSpace, "Sequence", Sequence, 0, -1);
            GraphUtil.AddAggregation(MusicSpace, "Song", Song, 0, -1);
        }
    }
}
