using m0;
using m0.Foundation;
using m0.Graph;
using m0.Lib;
using m0.Util;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using m0_COMPOSER.Midi;
using m0_COMPOSER.UIWpf.Visualisers;
using m0_COMPOSER.UIWpf.Visualisers.Control.Item;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Lib
{
    class HitHighlight
    {
        public IVertex eventVertex;
        public int ticksLeft;
    }

    class PlayState
    {
        public int prevEventIndex = 0;
        public int loopBeg = 0;
        public int loopBegEventIndex = 0;
        public int loopEnd = 0;

        public IList<KeyValuePair<int, IList<SongEvent>>> EventList;
        public IList<IVertex> OutputDictionary;
    }

    public class SongPlay
    {
        public int hitHighlightTicks = 200;

        IList<HitHighlight> hitHighlightList = new List<HitHighlight>();

        public IExecution exe;
        public IVertex SongVertex;
        public SongEventsDictionary SongDictionary;        
        

        public double TicksPerMilisecond;
        
        public MultimediaTimer Timer;

        public Stopwatch Watch;
        public long WatchAddElapsedMiliseconds = 0;

        PlayState CurrentPlayState = new PlayState();
        PlayState NewPlayState = new PlayState();

        //

        static IVertex r = MinusZero.Instance.Root;

        static IVertex songPositionMeta = r.Get(false, @"System\Lib\Music\Song\Position");
        static IVertex noteOnNoteMeta = r.Get(false, @"System\Lib\Music\NoteOutput\NoteOn\note");
        static IVertex noteOffNoteMeta = r.Get(false, @"System\Lib\Music\NoteOutput\NoteOff\note");        

        static IVertex controlChangeControlChangeMeta = r.Get(false, @"System\Lib\Music\NoteOutput\ControlChange\controlChange");
        static IVertex controlChangeEventValueMeta = r.Get(false, @"System\Lib\Music\ControlChangeEvent\Value");
        static IVertex controlChangeEventNumberMeta = r.Get(false, @"System\Lib\Music\ControlChangeEvent\Number");

        static IVertex programChangeProgramNumberMeta = r.Get(false, @"System\Lib\Music\NoteOutput\ProgramChange\programNumber");



        protected double GetMidiTicksPerMilisecond(int tempo)
        {
            double ticksInMinute = tempo * Midi.Standard.MidiTicksPerBeat;

            double ticksPerMilisecond = ticksInMinute / (60 * 1000);

            return ticksPerMilisecond;
        }

        public SongPlay(IExecution _exe, IVertex songVertex, int Tempo)
        {            
            exe = _exe;

            SongVertex = SongVertexDictionary.GetRealSongVertex(songVertex);

            TicksPerMilisecond = GetMidiTicksPerMilisecond(Tempo);

            SongDictionary = new SongEventsDictionary(songVertex);
            
            Watch = new Stopwatch();

            //

            Timer = new MultimediaTimer() { Interval = 1, Resolution = 0 };            

            Timer.Elapsed += Tick;

            int position = GraphUtil.GetIntegerValueOr0(SongVertex.Get(false, "Position:"));

            FillPositionRelated(CurrentPlayState, position);
        }

        void FillPositionRelated(PlayState stateToFill, int positionToUse)
        {
            SongDictionary.NeedToRebuildEventDictionary = true;
            SongDictionary.NeedToRebuildOutputDictionary = true;

            stateToFill.EventList = SongDictionary.GetEventDicionary();

            stateToFill.OutputDictionary = SongDictionary.GetOutputDicionary();


            if (stateToFill.EventList.Count == 0)
                return;            

            if (positionToUse > 0)
                WatchAddElapsedMiliseconds = (long)(positionToUse / TicksPerMilisecond);

            stateToFill.prevEventIndex = SearchForEventIndex(stateToFill.EventList, positionToUse);

            //

            SongVisualiser sv = SongVertexDictionary.GetSongVisualiser(SongVertex);

            if (sv.IsRepeat)
            {
                stateToFill.loopBeg = GraphUtil.GetIntegerValueOr0(SongVertex.Get(false, "LoopBeg:"));
                stateToFill.loopEnd = GraphUtil.GetIntegerValueOr0(SongVertex.Get(false, "LoopEnd:"));

                stateToFill.loopBegEventIndex = SearchForEventIndex(stateToFill.EventList, stateToFill.loopBeg);
            }
        }

        void EmitBankProgramChanges()
        {
            int cnt = 0;
            foreach(IEdge e in SongVertex.GetAll(false, "Track:"))
            {
                IVertex trackVertex = e.To;

                bool isProgramChangeNull = false, isBankSelectNull = false;

                int ProgramChangeValue = GraphUtil.GetIntegerValue(trackVertex.Get(false, "ProgramChange:"), ref isProgramChangeNull);

                int BankSelectValue = GraphUtil.GetIntegerValue(trackVertex.Get(false, "BankSelect:"), ref isBankSelectNull);

                if (!isProgramChangeNull)
                    ProgramChange(cnt, ProgramChangeValue);

                if (!isBankSelectNull)
                    BankSelect(cnt, BankSelectValue);

                cnt++;
            }
        }

        void ProgramChange(int track, int value)
        {
            IVertex outputVertex = CurrentPlayState.OutputDictionary[track];

            if (outputVertex != null)
            {
                IVertex playMethod = outputVertex.Get(false, @"$Is:\Method:ProgramChange");

                IVertex parameters = InstructionHelpers.CreateStack();                

                parameters.AddVertex(programChangeProgramNumberMeta, value);

                ZeroCodeExecutonUtil.MethodCallFromHost(exe, playMethod, outputVertex, parameters);
            }
        }

        void BankSelect(int track, int value)
        {
            IVertex outputVertex = CurrentPlayState.OutputDictionary[track];

            if (outputVertex != null)
            {
                IVertex playMethod = outputVertex.Get(false, @"$Is:\Method:ControlChange");

                IVertex parameters = InstructionHelpers.CreateStack();

                IVertex controlChangeVertex = MinusZero.Instance.CreateTempVertex();

                controlChangeVertex.AddVertex(controlChangeEventNumberMeta, 0);

                controlChangeVertex.AddVertex(controlChangeEventValueMeta, value);

                parameters.AddEdge(controlChangeControlChangeMeta, controlChangeVertex);

                ZeroCodeExecutonUtil.MethodCallFromHost(exe, playMethod, outputVertex, parameters);
            }
        }        

        private int SearchForEventIndex(IList<KeyValuePair<int, IList<SongEvent>>> EventListToUse, int position)
        {
            int currentEventIndex = 0;

            int indexFound = 0;

            bool shouldContinue = true;

            while (shouldContinue)
            {
                if (currentEventIndex >= EventListToUse.Count)
                { // stop            
                    shouldContinue = false;
                    break;
                }

                if (EventListToUse[currentEventIndex].Key >= position)
                {
                    indexFound = currentEventIndex;

                    if (indexFound > 0)
                        indexFound--;

                    shouldContinue = false;
                }                

                currentEventIndex++;
            }

            return indexFound;
        }

        public void Start()
        {
            StartSongVertexChangeTracking();

            EmitBankProgramChanges();

            Watch.Restart();            

            Timer.Start();
        }

        void StartSongVertexChangeTracking()
        {
            PlatformClass.RegisterVertexChangeListeners_byGenericVertex(SongVertex, new VertexChange(SongVertexChange), new string[] {});

          /*  foreach(IEdge trackEdge in SongVertex.GetAll(false, "Track:"))
            {
                PlatformClass.RegisterVertexChangeListeners_byGenericVertex(trackEdge.To, new VertexChange(TrackSequenceNoteVertexChange), new string[] { });
                //PlatformClass.RegisterVertexChangeListeners_byGenericVertex(trackEdge.To, new VertexChange(TrackSequenceNoteVertexChange), new string[] { "Output" });

                foreach (IEdge sequenceEdge in trackEdge.To.GetAll(false, @"SequenceEvent:\Sequence:"))
                    PlatformClass.RegisterVertexChangeListeners_byGenericVertex(sequenceEdge.To, new VertexChange(TrackSequenceNoteVertexChange), new string[] { });
            }*/

        }

        public void Destroy()
        {
            if (Timer.IsRunning)
            {                
                Watch.Stop();
                Timer.Stop();
                Timer.Dispose();
            }

            StopSongVertexChangeTracking();
        }

        void StopSongVertexChangeTracking()
        {
            PlatformClass.RemoveVertexChangeListeners_byGenericVertex(SongVertex, new VertexChange(SongVertexChange));

            foreach (IEdge trackEdge in SongVertex.GetAll(false, "Track:"))
                AddTrackListeners(trackEdge.To);
        }

        void AddTrackListeners(IVertex trackVertex)
        {
            PlatformClass.RemoveVertexChangeListeners_byGenericVertex(trackVertex, new VertexChange(TrackVertexChange));

            foreach (IEdge sequenceEdge in trackVertex.GetAll(false, @"SequenceEvent:\Sequence:"))
                AddSequenceListener(sequenceEdge.To);
        }

        void AddSequenceListener(IVertex sequenceEventVertex)
        {
            PlatformClass.RemoveVertexChangeListeners_byGenericVertex(sequenceEventVertex, new VertexChange(SequenceNoteVertexChange));
        }

        protected void SongVertexChange(object sender, VertexChangeEventArgs e)
        {
            if (sender == SongVertex.Get(false, "Position:"))
                return;

            IVertex loopBeg = SongVertex.Get(false, "LoopBeg:");
            IVertex loopEnd = SongVertex.Get(false, "LoopEnd:");

            if (loopBeg != null && sender == loopBeg)
                UpdateEventDictionaries();

            if (loopEnd != null && sender == loopEnd)
                UpdateEventDictionaries();

            if (e.Type == VertexChangeType.EdgeAdded && GeneralUtil.CompareStrings(e.Edge.Meta, "Track")){
                AddTrackListeners(e.Edge.To);
                UpdateEventDictionaries();
            }
        }

        protected void TrackVertexChange(object sender, VertexChangeEventArgs e)
        {
            if (e.Type == VertexChangeType.EdgeAdded && GeneralUtil.CompareStrings(e.Edge.Meta, "SequenceEvent"))
            {
                AddSequenceListener(e.Edge.To);
                UpdateEventDictionaries();
            }            
        }

        protected void SequenceNoteVertexChange(object sender, VertexChangeEventArgs e)
        {
            UpdateEventDictionaries();
        }

        protected void UpdateEventDictionaries()
        {
            //StopSongVertexChangeTracking();
            //StartSongVertexChangeTracking(); // add new sub vertexes to listen to

            long now = Watch.ElapsedMilliseconds + WatchAddElapsedMiliseconds;

            long nowInTicks = (long)(now * TicksPerMilisecond);
            

            FillPositionRelated(NewPlayState, (int)nowInTicks);

            //

            CurrentPlayState = NewPlayState;
        }

        public void Tick(object sender, EventArgs e)
        {            
            if (CurrentPlayState.EventList.Count == 0)
            {
                PositionStop();
                return;
            }

            long now = Watch.ElapsedMilliseconds + WatchAddElapsedMiliseconds;

            long nowInTicks = (long)(now * TicksPerMilisecond);

            //

            if(CurrentPlayState.loopEnd > 0 && nowInTicks > CurrentPlayState.loopEnd)
            {
                PositionUpdate(CurrentPlayState.loopBeg, false);

                Watch.Restart();

                WatchAddElapsedMiliseconds = (long)(CurrentPlayState.loopBeg / TicksPerMilisecond);

                now = Watch.ElapsedMilliseconds + WatchAddElapsedMiliseconds;

                nowInTicks = (long)(now * TicksPerMilisecond);

                CurrentPlayState.prevEventIndex = CurrentPlayState.loopBegEventIndex;
            }

            //

            int currentEventIndex = CurrentPlayState.prevEventIndex;

            bool shouldContinue = true;

            while (shouldContinue)
            {
                if (currentEventIndex >= CurrentPlayState.EventList.Count
                    || CurrentPlayState.EventList[currentEventIndex].Key > nowInTicks)
                    shouldContinue = false;
                else
                {
                    MidiOut(CurrentPlayState.EventList[currentEventIndex].Value);

                    currentEventIndex++;
                }
            }

            if (currentEventIndex >= CurrentPlayState.EventList.Count && CurrentPlayState.loopEnd == 0) // stop            
                PositionStop();            
            else
            {
                CurrentPlayState.prevEventIndex = currentEventIndex;

                PositionUpdate((int)nowInTicks, true);
            }

            DrumHitTicks();
        }

        void DrumHitTicks()
        {
            foreach(HitHighlight dh in hitHighlightList.ToList())
            {
                dh.ticksLeft--;

                if(dh.ticksLeft == 0)
                {
                    hitHighlightList.Remove(dh);
                    DoItemHighlight(dh.eventVertex, HighlightType.Stop);
                }

            }
        }

        int prevNowInTicksReduced = 0;

        public void PositionStop()
        {           
            Destroy();

            m0Main.Instance.Dispatcher.Invoke(() => {                
                GraphUtil.SetVertexValue(SongVertex, songPositionMeta, -1);                
            });            
        }
        
        void PositionUpdate(int nowInTicks, bool doReduce)
        {
            int nowInTicksReduced = nowInTicks / 100;

            if (!doReduce || nowInTicksReduced > prevNowInTicksReduced)
            {
                m0Main.Instance.Dispatcher.Invoke(() => {
                    GraphUtil.SetVertexValue(SongVertex, songPositionMeta, nowInTicks);
                });
                prevNowInTicksReduced = nowInTicksReduced;
            }
        }

        public void MidiOut(IList<SongEvent> el)
        {
            foreach (SongEvent e in el)
            {
                if (e is NoteOnEvent)
                    NoteOnEvent((NoteOnEvent)e);

                if (e is NoteOffEvent)
                    NoteOffEvent((NoteOffEvent)e);

                if (e is ControlChangeEvent)
                    ControlChangeEvent((ControlChangeEvent)e);
            }
        }

        enum HighlightType { Play, Stop, Hit}

        void DoItemHighlight(IVertex eventVertex, HighlightType type)
        {
            IList<IItem> items = ItemDictionary.Get(eventVertex);

            if (items == null)
                return;

            foreach (IItem i in items.ToList())
                m0Main.Instance.Dispatcher.Invoke(() => {
                    switch (type)
                    {
                        case HighlightType.Play:
                            i.PlayHighlight();
                            break;

                        case HighlightType.Stop:
                            i.StopHighlight();
                            break;

                        case HighlightType.Hit:
                            i.PlayHighlight();

                            HitHighlight dh = new HitHighlight();
                            dh.ticksLeft = hitHighlightTicks;
                            dh.eventVertex = eventVertex;

                            hitHighlightList.Add(dh);
                            break;
                    }                    
                });
        }

        public void NoteOnEvent(NoteOnEvent e)
        {
            IVertex outputVertex = CurrentPlayState.OutputDictionary[e.trackNumber];

            if (outputVertex != null)
            {
                IVertex playMethod = outputVertex.Get(false, @"$Is:\Method:NoteOn");

                IVertex parameters = InstructionHelpers.CreateStack();

                parameters.AddEdge(noteOnNoteMeta, e.eventVertex);

                ZeroCodeExecutonUtil.MethodCallFromHost(exe, playMethod, outputVertex, parameters);                
            }

            if(e.isDrum)
                DoItemHighlight(e.eventVertex, HighlightType.Hit);
            else
                DoItemHighlight(e.eventVertex, HighlightType.Play);
        }

        public void NoteOffEvent(NoteOffEvent e)
        {
            IVertex outputVertex = CurrentPlayState.OutputDictionary[e.trackNumber];

            if (outputVertex != null)
            {
                IVertex playMethod = outputVertex.Get(false, @"$Is:\Method:NoteOff");

                IVertex parameters = InstructionHelpers.CreateStack();

                parameters.AddEdge(noteOffNoteMeta, e.eventVertex);

                ZeroCodeExecutonUtil.MethodCallFromHost(exe, playMethod, outputVertex, parameters);                
            }

            DoItemHighlight(e.eventVertex, HighlightType.Stop);
        }

        public void ControlChangeEvent(ControlChangeEvent e)
        {
            IVertex outputVertex = CurrentPlayState.OutputDictionary[e.trackNumber];

            if (outputVertex != null)
            {
                IVertex playMethod = outputVertex.Get(false, @"$Is:\Method:ControlChange");

                IVertex parameters = InstructionHelpers.CreateStack();

                parameters.AddEdge(controlChangeControlChangeMeta, e.eventVertex);

                ZeroCodeExecutonUtil.MethodCallFromHost(exe, playMethod, outputVertex, parameters);
            }

            DoItemHighlight(e.eventVertex, HighlightType.Hit);
        }
    }
}
