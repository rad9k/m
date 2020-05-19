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

        B0 Acoustic Bass Drum
    C1 Bass Drum 1
    C#1     Side Stick
    D1      Acoustic Snare
    Eb1 Hand Clap
E1      Electric Snare
    F1 Low Floor Tom
    F#1     Closed Hi Hat
    G1      High Floor Tom
    Ab1     Pedal Hi-Hat
    A1      Low Tom
    Bb1 Open Hi-Hat
B1      Low-Mid Tom
    C2 Hi Mid Tom
    C#2     Crash Cymbal 1
    D2      High Tom
    Eb2 Ride Cymbal 1
    E2 Chinese Cymbal
F2      Ride Bell
    F#2     Tambourine
    G2      Splash Cymbal
    Ab2 Cowbell
    A2 Crash Cymbal 2
    Bb2 Vibraslap
    B2 Ride Cymbal 2 
    C3 Hi Bongo
C#3     Low Bongo 
    D3 Mute Hi Conga
    Eb3 Open Hi Conga
    E3 Low Conga
F3      High Timbale
    F#3     Low Timbale 
    G3      High Agogo
    Ab3 Low Agogo
A3      Cabasa
Bb3     Maracas
B3      Short Whistle
    C4 Long Whistle
C#4     Short Guiro 
    D4 Long Guiro
Eb4     Claves
E4      Hi Wood Block
F4      Low Wood Block
F#4     Mute Cuica 
    G4 Open Cuica
Ab4     Mute Triangle
    A4 Open Triangle

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

            Data = Music.AddVertex(null, "Data");

            AddBasePitchSet();

            AddBaseDrumSet();

            AddBaseTimeSpanStructure();

        }        

        private static void AddBasePitchSet()
        {
            IVertex r = m0.MinusZero.Instance.root;

            IVertex b=VertexOperations.AddInstance(Data, PitchSet);

            Data.AddEdge(Music.Get(false, "DefaultPitchSet"), b);

            b.Value = "Base";

            IVertex white = r.Get(false, @"System\Data\UX\Colors\White");
            IVertex black = r.Get(false, @"System\Data\UX\Colors\Black");

            for (int x = -1; x <= 9; x++) {
                AddPitch(b, x, 0, "C" + x.ToString(), white);
                AddPitch(b, x, 1, "C#" + x.ToString(), black);
                AddPitch(b, x, 2, "D" + x.ToString(), white);
                AddPitch(b, x, 3, "D#" + x.ToString(), black);
                AddPitch(b, x, 4, "E" + x.ToString(), white);
                AddPitch(b, x, 5, "F" + x.ToString(), white);
                AddPitch(b, x, 6, "F#" + x.ToString(), black);
                AddPitch(b, x, 7, "G" + x.ToString(), white);
                AddPitch(b, x, 8, "G#" + x.ToString(), black);
                AddPitch(b, x, 9, "A" + x.ToString(), white);
                AddPitch(b, x, 10, "A#" + x.ToString(), black);
                AddPitch(b, x, 11, "B" + x.ToString(), white);
            }
        }

        private static void AddBaseDrumSet()
        {
            IVertex r = m0.MinusZero.Instance.root;

            IVertex b = VertexOperations.AddInstance(Data, PitchSet);

            Data.AddEdge(Music.Get(false, "DefaultDrumSet"), b);

            b.Value = "Base";

            IVertex white = r.Get(false, @"System\Data\UX\Colors\White");
            IVertex black = r.Get(false, @"System\Data\UX\Colors\Black");

            for (int x = -1; x <= 9; x++)
            {
                AddPitch(b, x, 0, "C" + x.ToString(), white);
                AddPitch(b, x, 1, "C#" + x.ToString(), black);
                AddPitch(b, x, 2, "D" + x.ToString(), white);
                AddPitch(b, x, 3, "D#" + x.ToString(), black);
                AddPitch(b, x, 4, "E" + x.ToString(), white);
                AddPitch(b, x, 5, "F" + x.ToString(), white);
                AddPitch(b, x, 6, "F#" + x.ToString(), black);
                AddPitch(b, x, 7, "G" + x.ToString(), white);
                AddPitch(b, x, 8, "G#" + x.ToString(), black);
                AddPitch(b, x, 9, "A" + x.ToString(), white);
                AddPitch(b, x, 10, "A#" + x.ToString(), black);
                AddPitch(b, x, 11, "B" + x.ToString(), white);
            }
        }

        private static void AddPitch(IVertex basePitch, int octave, int note, string name, IVertex color)
        {
            IVertex p = VertexOperations.AddInstance(basePitch, VisualisedPitch);

            p.Value = name;

            GraphUtil.SetVertexValue(p, VisualisedPitch.Get(false, "Octave"), octave);
            GraphUtil.SetVertexValue(p, VisualisedPitch.Get(false, "Note"), note);
            GraphUtil.CreateOrReplaceEdge(p, VisualisedPitch.Get(false, "Color"), color);
        }

        private static void AddBaseTimeSpanStructure()
        {
            IVertex MidiTick = VertexOperations.AddInstance(Data, Music.Get(false, "TimeSpanLevel"));

            MidiTick.Value = "MidiTick";
        }

        private static void AddMetaEdges()
        {
            GraphUtil.AddMetaEdge(Music, "DefaultPitchSet", Music.Get(false, "PitchSet"));

            GraphUtil.AddMetaEdge(Music, "DefaultDrumSet", Music.Get(false, "PitchSet"));

            GraphUtil.AddMetaEdge(Music, "BaseTimeSpanLevel", Music.Get(false, "TimeSpanLevel"));
        }

        private static void AddClasses() {
            IVertex r = m0.MinusZero.Instance.root;

            string type = "m0_COMPOSER.Lib.Music, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            IVertex String = r.Get(false, @"System\Meta\ZeroTypes\String");
            IVertex Integer = r.Get(false, @"System\Meta\ZeroTypes\Integer");
            IVertex Boolean = r.Get(false, @"System\Meta\ZeroTypes\Boolean");
            IVertex Color = r.Get(false, @"System\Meta\ZeroTypes\Color");

            //

            IVertex NoteOutput = GraphUtil.AddClass(Music, "NoteOutput");
            IVertex NoteInput = GraphUtil.AddClass(Music, "NoteInput");

            // HAS LENGTH

            IVertex HasLenth = GraphUtil.AddClass(Music, "HasLength");

            GraphUtil.AddAttribute(HasLenth, "Length", Integer, 1, 1);

            // EVENT

            IVertex Event = GraphUtil.AddClass(Music, "Event");

            GraphUtil.AddAggregation(Event, "TriggerTime", Integer, 1, 1);

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

            // PICHSET

            PitchSet = GraphUtil.AddClass(Music, "PitchSet");

            GraphUtil.AddAggregation(PitchSet, "Pitch", Pitch, 0, -1);

            // TIMESPANLEVEL

            IVertex TimeSpanLevel = GraphUtil.AddClass(Music, "TimeSpanLevel");

            GraphUtil.AddInherits(TimeSpanLevel, HasLenth);            
            GraphUtil.AddAggregation(TimeSpanLevel, "SubLevel", TimeSpanLevel, 0, -1);


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

            IVertex Sequence = GraphUtil.AddClass(Music, "Sequnce");

            GraphUtil.AddInherits(Sequence, HasLenth);
            GraphUtil.AddInherits(Sequence, History);

            GraphUtil.AddAttribute(Sequence, "IsDrum", Boolean, 0, 1);
            GraphUtil.AddAssociation(Sequence, "PitchSet", PitchSet, 0, 1);

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
