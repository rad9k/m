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
        public static int MidiTicksPerSixteen = 96;

        public static void Save(IEnumerable<IVertex> systemSubGraphWithLinks, Dictionary<string, StoreId> storeOverride)
        {            
            print("* saving Lib\\Music");

            GeneralUtil.CreateM0JAndMoveEdgesIntoIt_IncludeEverythingBesidesList("lib_music.m0j", Music, new HashSet<IVertex>(systemSubGraphWithLinks), storeOverride);                        
        }

        static IVertex Music;

        static IVertex MusicGenerator;

        static IVertex Data;

        static IVertex MusicGeneratorData;

        static IVertex PitchSet;

        static IVertex VisualisedPitch;

        static IVertex ControlChangeDescription;

        static IVertex ControlChangeDescriptionSet;

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

        public static void CreateLibMusic()
        {
            print("* creating Lib\\Music");

            IVertex r = m0.MinusZero.Instance.root;

            IVertex lib = r.Get(false, @"System\Lib");

            Music = lib.AddVertex(null, "Music");

            AddMusicBasicClasses();

            AddGenerator();

            AddMetaEdges();

            AddData();

            AddMusicSpace();

            AddFromFiles();

            AddUserCommands();
        }

        private static void AddData()
        {
            IVertex r = m0.MinusZero.Instance.root;

            Data = Music.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Package"), "Data");

            AddBasePitchSet();

            AddBaseDrumSet();

            AddBaseOneOctavePitchSet();

            AddMusicTimeSpanStructure();

            AddRealTimeSpanStructure();

            AddNumberSpanStructure();

            AddDefaultControlChangeDescription();
        }        

        private static void AddBasePitchSet()
        {
            IVertex r = m0.MinusZero.Instance.root;

            IVertex b=VertexOperations.AddInstance(Data, PitchSet);

            Data.AddEdge(Music.Get(false, "DefaultPitchSet"), b);

            b.Value = "BasePitchSet";

            IVertex white = r.Get(false, @"System\Data\UX\Colors\White");
            IVertex black = r.Get(false, @"System\Data\UX\Colors\Black");
            IVertex gray = r.Get(false, @"System\Data\UX\Colors\VeryVeryLightGray");

            for (int x = 9; x >= -1; x--) {                
                AddPitch(b, x, 11, "B " + x.ToString(), white, null);
                AddPitch(b, x, 10, "A# " + x.ToString(), black, gray);
                AddPitch(b, x, 9, "A " + x.ToString(), white, null);
                AddPitch(b, x, 8, "G# " + x.ToString(), black, gray);
                AddPitch(b, x, 7, "G " + x.ToString(), white, null);
                AddPitch(b, x, 6, "F# " + x.ToString(), black, gray);
                AddPitch(b, x, 5, "F " + x.ToString(), white, null);
                AddPitch(b, x, 4, "E " + x.ToString(), white, null);
                AddPitch(b, x, 3, "D# " + x.ToString(), black, gray);
                AddPitch(b, x, 2, "D " + x.ToString(), white, null);
                AddPitch(b, x, 1, "C# " + x.ToString(), black, gray);
                AddPitch(b, x, 0, "C " + x.ToString(), white, null);
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

            for (int x = 9; x >= -1; x--)
            {                
                AddDrumPitch(b, x, 11, "B " + x.ToString(), white);
                AddDrumPitch(b, x, 10, "A# " + x.ToString(), white);
                AddDrumPitch(b, x, 9, "A " + x.ToString(), white);
                AddDrumPitch(b, x, 8, "G# " + x.ToString(), white);
                AddDrumPitch(b, x, 7, "G " + x.ToString(), white);
                AddDrumPitch(b, x, 6, "F# " + x.ToString(), white);
                AddDrumPitch(b, x, 5, "F " + x.ToString(), white);
                AddDrumPitch(b, x, 4, "E " + x.ToString(), white);
                AddDrumPitch(b, x, 3, "D# " + x.ToString(), white);
                AddDrumPitch(b, x, 2, "D " + x.ToString(), white);
                AddDrumPitch(b, x, 1, "C# " + x.ToString(), white);
                AddDrumPitch(b, x, 0, "C " + x.ToString(), white);
            }
        }

        
        private static void AddBaseOneOctavePitchSet()
        {
            IVertex r = m0.MinusZero.Instance.root;

            IVertex b = VertexOperations.AddInstance(Data, PitchSet);

            Data.AddEdge(Music.Get(false, "DefaultOneOctavePitchSet"), b);

            b.Value = "BaseOneOctavePitchSet";

            IVertex white = r.Get(false, @"System\Data\UX\Colors\White");
            IVertex black = r.Get(false, @"System\Data\UX\Colors\Black");
            IVertex gray = r.Get(false, @"System\Data\UX\Colors\VeryVeryLightGray");

            int x = 0;
            
            AddPitch(b, x, 11, "B", white, null);
            AddPitch(b, x, 10, "A#", black, gray);
            AddPitch(b, x, 9, "A", white, null);
            AddPitch(b, x, 8, "G#", black, gray);
            AddPitch(b, x, 7, "G", white, null);
            AddPitch(b, x, 6, "F#", black, gray);
            AddPitch(b, x, 5, "F", white, null);
            AddPitch(b, x, 4, "E", white, null);
            AddPitch(b, x, 3, "D#", black, gray);
            AddPitch(b, x, 2, "D", white, null);
            AddPitch(b, x, 1, "C#", black, gray);
            AddPitch(b, x, 0, "C", white, null);
        }

        private static void AddPitch(IVertex basePitch, int octave, int note, string name, IVertex color, IVertex noteBackgroundColor)
        {
            octave = octave + 1;

            IVertex p = VertexOperations.AddInstance(basePitch, VisualisedPitch);

            p.Value = name;

            GraphUtil.SetVertexValue(p, VisualisedPitch.Get(false, "Name"), name);
            GraphUtil.SetVertexValue(p, VisualisedPitch.Get(false, "Octave"), octave);
            GraphUtil.SetVertexValue(p, VisualisedPitch.Get(false, "Note"), note);
            GraphUtil.CreateOrReplaceEdge(p, VisualisedPitch.Get(false, "PitchColor"), color);

            if(noteBackgroundColor != null)
                GraphUtil.CreateOrReplaceEdge(p, VisualisedPitch.Get(false, "NoteBackgroundColor"), noteBackgroundColor);
        }

        private static void AddDrumPitch(IVertex basePitch, int octave, int note, string name, IVertex color)
        {
            octave = octave + 1;

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

        private static void AddMusicTimeSpanStructure()
        {
            IVertex MidiTick = AddTimeSpan(Data, "MidiTick", 1, Music.Get(false, "TimeSpanLevel"));

            Data.AddEdge(Music.Get(false, "BaseMusicTimeSpanLevel"), MidiTick);

            IVertex tact = AddTimeSpan(Data, "Tact", 16, Music.Get(false, @"TimeSpanLevel"));

            Data.AddEdge(Music.Get(false, "DefaultMusicTimeSpanLevel"), tact);

            IVertex sixteen = AddTimeSpan(tact, "Sixteen", MidiTicksPerSixteen, Music.Get(false, @"TimeSpanLevel\SubLevel"));

            sixteen.AddEdge(Music.Get(false, @"TimeSpanLevel\SubLevel"), MidiTick);            
        }

        private static void AddRealTimeSpanStructure()
        {
            IVertex minute = AddTimeSpan(Data, "Minute", 60, Music.Get(false, @"TimeSpanLevel"));

            IVertex second = AddTimeSpan(minute, "Second", 100, Music.Get(false, @"TimeSpanLevel\SubLevel"));

            IVertex milisecond = AddTimeSpan(second, "MiliSecond", 1, Music.Get(false, @"TimeSpanLevel\SubLevel"));

            Data.AddEdge(Music.Get(false, "BaseRealTimeSpanLevel"), milisecond);

            Data.AddEdge(Music.Get(false, "DefaultRealTimeSpanLevel"), minute);
        }

        private static void AddNumberSpanStructure()
        {
            IVertex ten = AddTimeSpan(Data, "Ten", 10, Music.Get(false, @"TimeSpanLevel"));

            IVertex one = AddTimeSpan(ten, "One", 1, Music.Get(false, @"TimeSpanLevel\SubLevel"));

            IVertex _base = AddTimeSpan(one, "Base", 1, Music.Get(false, @"TimeSpanLevel\SubLevel"));

            Data.AddEdge(Music.Get(false, "DefaultNumberSpanLevel"), ten);

            Data.AddEdge(Music.Get(false, "BaseNumberSpanLevel"), _base);
        }

        static void AddDefaultControlChangeDescription()
        {
            IVertex b = VertexOperations.AddInstance(Data, ControlChangeDescriptionSet);

            Data.AddEdge(Music.Get(false, "DefaultControlChangeDescriptionSet"), b);

            b.Value = "BaseControlChangeDescriptionSet";

            IVertex eDefault = Music.Get(false, @"ControlChangeDescriptionTypeEnum\Default");
            IVertex eOnOff = Music.Get(false, @"ControlChangeDescriptionTypeEnum\OnOff");
            IVertex eMSB = Music.Get(false, @"ControlChangeDescriptionTypeEnum\MSB");
            IVertex eLSB = Music.Get(false, @"ControlChangeDescriptionTypeEnum\LSB");

            foreach (CCDescription d in DefaultControlChangeDescription.DefaultControlChangeDescriptionSet){
                IVertex desc = VertexOperations.AddInstance(b, ControlChangeDescription);

                GraphUtil.SetVertexValue(desc, ControlChangeDescription.Get(false, "Number"), d.Number);
                GraphUtil.SetVertexValue(desc, ControlChangeDescription.Get(false, "Description"), d.Description);


                switch (d.Type)
                {
                    case "Default":
                        GraphUtil.CreateOrReplaceEdge(desc, ControlChangeDescription.Get(false, "Type"), eDefault);
                        break;

                    case "OnOff":
                        GraphUtil.CreateOrReplaceEdge(desc, ControlChangeDescription.Get(false, "Type"), eOnOff);
                        break;

                    case "MSB":
                        GraphUtil.CreateOrReplaceEdge(desc, ControlChangeDescription.Get(false, "Type"), eMSB);
                        break;

                    case "LSB":
                        GraphUtil.CreateOrReplaceEdge(desc, ControlChangeDescription.Get(false, "Type"), eLSB);
                        break;
                }
            }
        }

        private static void AddMetaEdges()
        {
            GraphUtil.AddMetaEdge(Music, "DefaultPitchSet", Music.Get(false, "PitchSet"));

            GraphUtil.AddMetaEdge(Music, "DefaultDrumPitchSet", Music.Get(false, "PitchSet"));

            GraphUtil.AddMetaEdge(Music, "DefaultOneOctavePitchSet", Music.Get(false, "PitchSet"));

            GraphUtil.AddMetaEdge(Music, "BaseMusicTimeSpanLevel", Music.Get(false, "TimeSpanLevel"));

            GraphUtil.AddMetaEdge(Music, "DefaultMusicTimeSpanLevel", Music.Get(false, "TimeSpanLevel"));

            GraphUtil.AddMetaEdge(Music, "DefaultRealTimeSpanLevel", Music.Get(false, "TimeSpanLevel"));

            GraphUtil.AddMetaEdge(Music, "BaseRealTimeSpanLevel", Music.Get(false, "TimeSpanLevel"));

            GraphUtil.AddMetaEdge(Music, "DefaultNumberSpanLevel", Music.Get(false, "TimeSpanLevel"));

            GraphUtil.AddMetaEdge(Music, "BaseNumberSpanLevel", Music.Get(false, "TimeSpanLevel"));

            GraphUtil.AddMetaEdge(Music, "DefaultControlChangeDescriptionSet", Music.Get(false, "ControlChangeDescriptionSet"));
        }

        static IVertex r = m0.MinusZero.Instance.root;

        static string NoteOutoutTypeString = "m0_COMPOSER.Lib.NoteOutput, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        static string MidiDeviceTypeString = "m0_COMPOSER.Lib.MidiDevice, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        static string SongTypeString = "m0_COMPOSER.Lib.Song, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

        static IVertex StringMeta = r.Get(false, @"System\Meta\ZeroTypes\String");
        static IVertex IntegerMeta = r.Get(false, @"System\Meta\ZeroTypes\Integer");
        static IVertex BooleanMeta = r.Get(false, @"System\Meta\ZeroTypes\Boolean");
        static IVertex FloatMeta = r.Get(false, @"System\Meta\ZeroTypes\Float");
        static IVertex ColorMeta = r.Get(false, @"System\Meta\ZeroTypes\UX\Color");

        static IVertex Note;
        static IVertex Pitch;
        static IVertex ControlChange;
        static IVertex Event;
        static IVertex HasLength;

        static IVertex Song;
        static IVertex Track;
        static IVertex SequenceEvent;
        static IVertex Sequence;

        static IVertex MelodyFlow;
        static IVertex TriggerSet;
        static IVertex ChordProgression;

        private static void AddMusicBasicClasses() {            
            // CCDescription

            IVertex ControlChangeDescriptionTypeEnum = GraphUtil.AddEnum(Music, "ControlChangeDescriptionTypeEnum", new String[] { "Default", "On/Off", "MSB", "LSB" });

            // vertex stubs

            IVertex NoteOutput = GraphUtil.AddClass(Music, "NoteOutput");
            IVertex NoteInput = GraphUtil.AddClass(Music, "NoteInput");

            IVertex TimeSpanLevel = GraphUtil.AddClass(Music, "TimeSpanLevel");

            // HAS LENGTH

            HasLength = GraphUtil.AddClass(Music, "HasLength");

            GraphUtil.AddAttribute(HasLength, "Length", IntegerMeta, 0, 1);
            GraphUtil.AddAssociation(HasLength, "TimeSpan", TimeSpanLevel, 0, 1);

            // EVENT

            Event = GraphUtil.AddClass(Music, "Event");

            GraphUtil.AddAttribute(Event, "TriggerTime", IntegerMeta, 1, 1);

            // HISTORY

            IVertex History = GraphUtil.AddClass(Music, "History");

            IVertex History_Event = GraphUtil.AddAttribute(History, "Event", Event, 0, -1);

            History_Event.AddVertex(m0.MinusZero.Instance.root.Get(false, @"System\Meta\Presentation\$Hide"), null);

            // CONTROLCHANGE

            ControlChange = GraphUtil.AddClass(Music, "ControlChange");

            GraphUtil.AddAttribute(ControlChange, "Number", IntegerMeta, 1, 1);
            GraphUtil.AddAttribute(ControlChange, "Value", IntegerMeta, 1, 1);

            // CONTROLCHANGEEVENT

            IVertex ControlChangeEvent = GraphUtil.AddClass(Music, "ControlChangeEvent");

            GraphUtil.AddInherits(ControlChangeEvent, ControlChange);
            GraphUtil.AddInherits(ControlChangeEvent, Event);

            // CONTROLCHANGEDESCRIPTION

            ControlChangeDescription = GraphUtil.AddClass(Music, "ControlChangeDescription");

            GraphUtil.AddAttribute(ControlChangeDescription, "Number", IntegerMeta, 1, 1);
            GraphUtil.AddAttribute(ControlChangeDescription, "Description", StringMeta, 1, 1);
            GraphUtil.AddAssociation(ControlChangeDescription, "Type", ControlChangeDescriptionTypeEnum, 0, 1);

            // CONTROLCHANGEDESCRIPTIONSET

            ControlChangeDescriptionSet = GraphUtil.AddClass(Music, "ControlChangeDescriptionSet");

            GraphUtil.AddAggregation(ControlChangeDescriptionSet, "ControlChangeDescription", ControlChangeDescription, 0, -1);

            // PICH

            Pitch = GraphUtil.AddClass(Music, "Pitch");

            GraphUtil.AddAttribute(Pitch, "Octave", IntegerMeta, 1, 1);
            GraphUtil.AddAttribute(Pitch, "Note", IntegerMeta, 1, 1);

            // VISULISEDPICH

            VisualisedPitch = GraphUtil.AddClass(Music, "VisualisedPitch");

            GraphUtil.AddInherits(VisualisedPitch, Pitch);
            GraphUtil.AddAttribute(VisualisedPitch, "Name", StringMeta, 1, 1);
            GraphUtil.AddAssociation(VisualisedPitch, "PitchColor", ColorMeta, 1, 1);
            GraphUtil.AddAssociation(VisualisedPitch, "NoteBackgroundColor", ColorMeta, 0, 1);

            // PICHSET

            PitchSet = GraphUtil.AddClass(Music, "PitchSet");
            GraphUtil.AddAssociation(PitchSet, "BasedOn", PitchSet, 0, 1);
            GraphUtil.AddAggregation(PitchSet, "Pitch", Pitch, 0, -1);

          //  PitchSet.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$DefaultOpenVisualiser"), r.Get(false, @"System\Meta\Visualiser\PitchSet"));

            // TIMESPANLEVEL            

            GraphUtil.AddInherits(TimeSpanLevel, HasLength);
            GraphUtil.AddAggregation(TimeSpanLevel, "SubLevel", TimeSpanLevel, 0, 1);


            // NOTE

            Note = GraphUtil.AddClass(Music, "Note");

            GraphUtil.AddInherits(Note, Pitch);

            GraphUtil.AddAttribute(Note, "Velocity", IntegerMeta, 1, 1);

            // NOTEEVENT

            IVertex NoteEvent = GraphUtil.AddClass(Music, "NoteEvent");

            GraphUtil.AddInherits(NoteEvent, Note);
            GraphUtil.AddInherits(NoteEvent, Event);
            GraphUtil.AddInherits(NoteEvent, HasLength);

            // SEQUENCE

            Sequence = GraphUtil.AddClass(Music, "Sequence");

            GraphUtil.AddInherits(Sequence, HasLength);
            GraphUtil.AddInherits(Sequence, History);

            GraphUtil.AddAttribute(Sequence, "IsDrum", BooleanMeta, 0, 1);
            GraphUtil.AddAttribute(Sequence, "ExtendTimeLength", IntegerMeta, 1, 1, 16 * MidiTicksPerSixteen);
            GraphUtil.AddAssociation(Sequence, "PitchSet", PitchSet, 0, 1);
            GraphUtil.AddAssociation(Sequence, "ControlChangeDescriptionSet", ControlChangeDescriptionSet, 0, 1);

            Sequence.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$DefaultOpenVisualiser"), r.Get(false, @"System\Meta\Visualiser\Sequence"));
           

            // SEQUENCEEVENT

            SequenceEvent = GraphUtil.AddClass(Music, "SequenceEvent");

            GraphUtil.AddInherits(SequenceEvent, Event);
            //GraphUtil.AddAssociation(SequenceEvent, "Sequence", Sequence, 1, 1);
            GraphUtil.AddAggregation(SequenceEvent, "Sequence", Sequence, 1, 1); // TEMP

            // TRACK

            Track = GraphUtil.AddClass(Music, "Track");

            GraphUtil.AddInherits(Track, r.Get(false, @"System\Meta\ZeroTypes\UX\HasColor"));

            //GraphUtil.AddAttribute(Track, "Name", String, 0, 1);
            //GraphUtil.AddAttribute(Track, "Color", Color, 0, 1);
            GraphUtil.AddAssociation(Track, "Output", NoteOutput, 0, 1);
            GraphUtil.AddAttribute(Track, "IsDrum", BooleanMeta, 0, 1);
            GraphUtil.AddAttribute(Track, "IsMuted", BooleanMeta, 0, 1);
            GraphUtil.AddAttribute(Track, "IsSolo", BooleanMeta, 0, 1);
            GraphUtil.AddAttribute(Track, "ProgramChange", IntegerMeta, 0, 1, 0, 0, 127);
            GraphUtil.AddAttribute(Track, "BankSelect", IntegerMeta, 0, 1, 0, 0, 127);
            GraphUtil.AddAggregation(Track, "SequenceEvent", SequenceEvent, 0, -1);

            // SONG

            Song = GraphUtil.AddClass(Music, "Song");

            GraphUtil.AddInherits(Song, HasLength);

            GraphUtil.AddAttribute(Song, "ExtendTimeLength", FloatMeta, 1, 1, 1.0);
            //GraphUtil.AddAttribute(Song, "Name", String, 0, 1);
            GraphUtil.AddAggregation(Song, "Track", Track, 0, -1);
            GraphUtil.AddAttribute(Song, "Input", NoteInput, 0, 1);
            GraphUtil.AddAssociation(Song, "RecordingTrack", Track, 0, 1);
            GraphUtil.AddAttribute(Song, "Tempo", FloatMeta, 1, 1, (double)125.0, (double)10.0, (double)250.0);
            GraphUtil.AddAttribute(Song, "Position", IntegerMeta, 1, 1, 0);
            GraphUtil.AddAttribute(Song, "LoopBeg", IntegerMeta, 0, 1);
            GraphUtil.AddAttribute(Song, "LoopEnd", IntegerMeta, 0, 1);
            GraphUtil.AddAttribute(Song, "IsRepeat", BooleanMeta, 0, 1);

            AddMethod(Song, "Record", SongTypeString, "Record", null, new TypeName[] { });
            AddMethod(Song, "Play", SongTypeString, "Play", null, new TypeName[] { });
            AddMethod(Song, "Stop", SongTypeString, "Stop", null, new TypeName[] { });                        

            Song.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$DefaultOpenVisualiser"), r.Get(false, @"System\Meta\Visualiser\Song"));


            // NOTEOUTPUT continuation

            GraphUtil.AddAssociation(NoteOutput, "PitchSet", PitchSet, 0, 1);
            GraphUtil.AddAssociation(NoteOutput, "ControlChangeDescriptionSet", ControlChangeDescriptionSet, 0, 1);

            AddMethod(NoteOutput, "NoteOn", NoteOutoutTypeString, "NoteOn", null, new TypeName[] { new TypeName("note", Note, 1, 1) });
            AddMethod(NoteOutput, "NoteOff", NoteOutoutTypeString, "NoteOff", null, new TypeName[] { new TypeName("note", Note, 1, 1) });
            AddMethod(NoteOutput, "ControlChange", NoteOutoutTypeString, "ControlChange", null, new TypeName[] { new TypeName("controlChange", ControlChange, 1, 1) });
            AddMethod(NoteOutput, "ProgramChange", NoteOutoutTypeString, "ProgramChange", null, new TypeName[] { new TypeName("programNumber", "Integer", 1, 1) });
            AddMethod(NoteOutput, "PitchBend", NoteOutoutTypeString, "PitchBend", null, new TypeName[] { new TypeName("value", "Integer", 1, 1) });
            AddMethod(NoteOutput, "Silent", NoteOutoutTypeString, "Silent", null, new TypeName[] { });

            IVertex MidiOutput = GraphUtil.AddClass(Music, "MidiOutput");
            IVertex MidiInput = GraphUtil.AddClass(Music, "MidiInput");

            //

            IVertex MidiDevice = GraphUtil.AddClass(Music, "MidiDevice");

            GraphUtil.AddAttribute(MidiDevice, "Name", StringMeta, 1, 1);

            GraphUtil.AddAggregation(MidiDevice, "Output", MidiOutput, 0, -1);            

            GraphUtil.AddAttribute(MidiDevice, "DeviceNumber", IntegerMeta, 0, 1);

            GraphUtil.AddAttribute(MidiDevice, "Mid", StringMeta, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "Pid", StringMeta, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "DriverVersion", StringMeta, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "Technology", StringMeta, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "Voices", StringMeta, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "Notes", StringMeta, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "ChannelMask", StringMeta, 0, 1);
            GraphUtil.AddAttribute(MidiDevice, "Support", StringMeta, 0, 1);            

            AddMethod(MidiDevice, "Reset", MidiDeviceTypeString, "Reset", null, new TypeName[] { });
            AddMethod(MidiDevice, "TimingClock", MidiDeviceTypeString, "TimingClock", null, new TypeName[] { });
            AddMethod(MidiDevice, "Start", MidiDeviceTypeString, "Start", null, new TypeName[] { });
            AddMethod(MidiDevice, "Continue", MidiDeviceTypeString, "Continue", null, new TypeName[] { });
            AddMethod(MidiDevice, "Stop", MidiDeviceTypeString, "Stop", null, new TypeName[] { });

            // MIDI OUT

            GraphUtil.AddInherits(MidiOutput, NoteOutput);

            GraphUtil.AddAssociation(MidiOutput, "Device", MidiDevice, 1, 1);
            GraphUtil.AddAttribute(MidiOutput, "Name", StringMeta, 1, 1);
            GraphUtil.AddAttribute(MidiOutput, "Channel", IntegerMeta, 1, 1);

            // MIDI IN

            GraphUtil.AddInherits(MidiInput, NoteInput);

            GraphUtil.AddAssociation(MidiInput, "Device", MidiDevice, 1, 1);
            GraphUtil.AddAttribute(MidiInput, "Name", StringMeta, 1, 1);
            GraphUtil.AddAttribute(MidiInput, "Channel", IntegerMeta, 1, 1);
        }

        public static void AddGenerator()
        {
            MusicGenerator = Music.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Package"), "Generator");

            MusicGeneratorData = MusicGenerator.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Package"), "Data");

            AddMusicGeneratorClasses();

            AddGeneratorFlowPitchSet();
        }

        public static void AddMusicGeneratorClasses()
        {
            IVertex MelodyFlowQuantTypeEnum = GraphUtil.AddEnum(MusicGenerator, "MelodyFlowQuantTypeEnum", new String[] { "Note", "ChordIndex" });


            // MELODYFLOWQUANT

            IVertex MelodyFlowQuant = GraphUtil.AddClass(MusicGenerator, "MelodyFlowQuant");
            GraphUtil.AddInherits(MelodyFlowQuant, Note);            
            GraphUtil.AddAttribute(MelodyFlowQuant, "Velocity", IntegerMeta, 0, 1);
            GraphUtil.AddAttribute(MelodyFlowQuant, "QuantType", MelodyFlowQuantTypeEnum, 1, 1);


            // MELODYFLOWSTEP

            IVertex MelodyFlowStep = GraphUtil.AddClass(MusicGenerator, "MelodyFlowStep");
            GraphUtil.AddAggregation(MelodyFlowStep, "Quant", MelodyFlowQuant, 0, -1);
            GraphUtil.AddAggregation(MelodyFlowStep, "ControlChange", ControlChange, 0, -1);

            // MELODYFLOW

            MelodyFlow = GraphUtil.AddClass(MusicGenerator, "MelodyFlow");
            GraphUtil.AddAttribute(MelodyFlow, "IsDrum", BooleanMeta, 0, 1);
            GraphUtil.AddAssociation(MelodyFlow, "PitchSet", PitchSet, 0, 1);
            GraphUtil.AddAggregation(MelodyFlow, "Step", MelodyFlowStep, 0, -1);            

            MelodyFlow.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$DefaultOpenVisualiser"), r.Get(false, @"System\Meta\Visualiser\MelodyFlow"));                        

            // TRIGGER

            IVertex Trigger = GraphUtil.AddClass(MusicGenerator, "Trigger");
            GraphUtil.AddInherits(Trigger, Event);
            GraphUtil.AddInherits(Trigger, HasLength);
            GraphUtil.AddAttribute(Trigger, "Velocity", IntegerMeta, 0, 1);
            GraphUtil.AddAggregation(Trigger, "ControlChange", ControlChange, 0, -1);

            // TRIGGERSET

            TriggerSet = GraphUtil.AddClass(MusicGenerator, "TriggerSet");
            GraphUtil.AddInherits(TriggerSet, HasLength);
            GraphUtil.AddAttribute(TriggerSet, "IsDrum", BooleanMeta, 0, 1);
            GraphUtil.AddAggregation(TriggerSet, "Trigger", Trigger, 0, -1);

            TriggerSet.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$DefaultOpenVisualiser"), r.Get(false, @"System\Meta\Visualiser\TriggerSet"));

            // CHORDPROGRESSION

            ChordProgression = GraphUtil.AddClass(MusicGenerator, "ChordProgression");
            GraphUtil.AddAggregation(ChordProgression, "Chord", PitchSet, 0, -1);

            ChordProgression.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\$DefaultOpenVisualiser"), r.Get(false, @"System\Meta\Visualiser\ChordProgression"));

            // SEQUENCETRANSFORMER

            IVertex SequenceTransformer = GraphUtil.AddClass(MusicGenerator, "SequenceTransformer");
            GraphUtil.AddAssociation(SequenceTransformer, "Input", Sequence, 1, 1);
            GraphUtil.AddAssociation(SequenceTransformer, "Output", Sequence, 1, 1);
            AddMethod(SequenceTransformer, "Process", null, null, null, new TypeName[] { });
        }

        private static void AddGeneratorFlowPitchSet()
        {
            IVertex r = m0.MinusZero.Instance.root;

            IVertex b = VertexOperations.AddInstance(MusicGeneratorData, PitchSet);            

            b.Value = "FlowPitchSet";
            
            for (int x = 11; x >= -11; x--)
            {
                IVertex color = r.Get(false, @"System\Data\UX\Colors\Gray"+(11 - Math.Abs(x)));

                AddPitch(b, 0, x, x.ToString(), color, null);                
            }
        }

        public static void AddMusicSpace()
        {
            IVertex MusicSpace = GraphUtil.AddClass(Music, "MusicSpace");

            GraphUtil.AddAggregation(MusicSpace, "Song", Song, 0, -1);
            GraphUtil.AddAggregation(MusicSpace, "Track", Track, 0, -1);
            GraphUtil.AddAggregation(MusicSpace, "SequenceEvent", SequenceEvent, 0, -1);
            GraphUtil.AddAggregation(MusicSpace, "Sequence", Sequence, 0, -1);
            GraphUtil.AddAggregation(MusicSpace, "MelodyFlow", MelodyFlow, 0, -1);
            GraphUtil.AddAggregation(MusicSpace, "TriggerSet", TriggerSet, 0, -1);
            GraphUtil.AddAggregation(MusicSpace, "ChordProgression", ChordProgression, 0, -1);
        }

        public static void AddFromFiles()
        {
            // Generator
            
            GraphUtil.LoadTXTParseAndMove(@"_RES\Lib\Music\Generator\HarmonyMelodyTimeGenerator.txt", MusicGenerator, "'HarmonyMelodyTimeGenerator'");
            GraphUtil.LoadTXTParseAndMove(@"_RES\Lib\Music\Generator\SimpleTransformer.txt", MusicGenerator, "'SimpleTransformer'");

            // Instrument

            IVertex Instrument = Music.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Package"), "Instrument");

            IVertex X09 = Instrument.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Package"), "XBase09");

            GraphUtil.LoadTXTParseAndMove(@"_RES\Lib\Music\Instrument\X09\X09_CC.txt", X09, "'XBase09_ControlChangeDescriptionSet'");
            GraphUtil.LoadTXTParseAndMove(@"_RES\Lib\Music\Instrument\X09\X09_mode_1_PitchSet.txt", X09, "'XBase09_mode_1_PitchSet'");
            GraphUtil.LoadTXTParseAndMove(@"_RES\Lib\Music\Instrument\X09\X09_mode_2_PitchSet.txt", X09, "'XBase09_mode_2_PitchSet'");

            IVertex TR8S = Instrument.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Package"), "TR8S");

            GraphUtil.LoadTXTParseAndMove(@"_RES\Lib\Music\Instrument\TR8S\TR8S_PitchSet.txt", TR8S, "'TR8S_PitchSet'");

            IVertex MFB522 = Instrument.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Package"), "MFB522");

            GraphUtil.LoadTXTParseAndMove(@"_RES\Lib\Music\Instrument\MFB522\MFB522_PitchSet.txt", MFB522, "'MFB522_PitchSet'");

            // Chord

            IVertex Chord = Music.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Package"), "Chord");

            GraphUtil.LoadTXTAndParse(@"_RES\Lib\Music\Chord\BasicChords.txt", Chord);

            IEdge toDelete1 = Chord.OutEdges[0];
            IEdge toDelete2 = Chord.OutEdges[1];

            Chord.DeleteEdge(toDelete1);
            Chord.DeleteEdge(toDelete2);
        }

        public static void AddUserCommands()
        {
            IVertex commands = Music.AddVertex(r.Get(false, @"System\Meta\ZeroUML\Package"), "UserCommands");

            string type = "m0_COMPOSER.UserCommands.ComposerUserCommands, m0_COMPOSER, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            IVertex onNewMusicSpaceStore_Vertex = AddFunction(commands, "OnNewMusicSpaceStore", type, "OnNewMusicSpaceStore", null, new TypeName[] { new TypeName("baseVertex", "VertexType", 1, 1) });

            onNewMusicSpaceStore_Vertex.AddVertex(r.Get(false, @"System\Meta\Base\Vertex\$Name"), "New music space store");

            IVertex directory_Vertex = r.Get(false, @"System\Meta\Store\FileSystem\Directory");

            directory_Vertex.AddEdge(r.Get(false, @"System\Meta\Base\Vertex\UserCommand"), onNewMusicSpaceStore_Vertex);
        }
    }
}
