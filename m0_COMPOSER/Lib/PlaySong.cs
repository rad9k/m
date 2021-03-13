using m0;
using m0.Foundation;
using m0.Graph;
using m0.Lib;
using m0.ZeroCode;
using m0.ZeroCode.Helpers;
using m0.ZeroTypes;
using m0_COMPOSER.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Lib
{
    public class SongPlay
    {
        public IExecution exe;
        public IVertex SongVertex;
        public SongEventsDictionary SongDictionary;
        public IDictionary<int, IList<SongEvent>> EventDictionary;
        public IList<KeyValuePair<int, IList<SongEvent>>> EventList;
        public IList<IVertex> OutputDictionary;

        public double TicksPerMilisecond;
        
        public MultimediaTimer Timer;

        public Stopwatch Watch;

        int prevEventIndex = 0;

        //

        static IVertex r = MinusZero.Instance.Root;

        static IVertex songPositionMeta = r.Get(false, @"System\Lib\Music\Song\Position");
        static IVertex noteOnNoteMeta = r.Get(false, @"System\Lib\Music\NoteOutput\NoteOn\note");
        static IVertex noteOffNoteMeta = r.Get(false, @"System\Lib\Music\NoteOutput\NoteOff\note");
        static IVertex controlChangeControlChangeMeta = r.Get(false, @"System\Lib\Music\NoteOutput\ControlChange\controlChange");

        protected double GetMidiTicksPerMilisecond(int tempo)
        {
            double ticksInMinute = tempo * Midi.Standard.MidiTicksPerBeat;

            double ticksPerMilisecond = ticksInMinute / (60 * 1000);

            return ticksPerMilisecond;
        }

        public SongPlay(IExecution _exe, IVertex songVertex, int Tempo)
        {
            exe = _exe;

            SongVertex = songVertex;

            TicksPerMilisecond = GetMidiTicksPerMilisecond(Tempo);

            SongDictionary = new SongEventsDictionary(songVertex);

            OutputDictionary = SongDictionary.GetOutputDicionary();

            EventDictionary = SongDictionary.GetEventDicionary();

            EventList = EventDictionary.ToList();

            Watch = new Stopwatch();

            //

            //Timer = new MultimediaTimer() { Interval = 1, Resolution = 0 };

            Timer = new MultimediaTimer() { Interval = 1000};

            Timer.Elapsed += Tick;
        }

        public void Start()
        {
            Watch.Restart();            

            Timer.Start();
        }

        public void Destroy()
        {
            if (Timer.IsRunning)
            {                
                Watch.Stop();
                Timer.Stop();
                Timer.Dispose();
            }
        }

        public void Tick(object sender, EventArgs e)
        {
            return;

            if (EventList.Count == 0)
            {
                PositionStop();
                return;
            }

            long now = Watch.ElapsedMilliseconds;

            long nowInTicks = (long)(now * TicksPerMilisecond);

            int currentEventIndex = prevEventIndex;

            bool shouldContinue = true;

            while (shouldContinue)
            {
                if (currentEventIndex >= EventList.Count
                    || EventList[currentEventIndex].Key > nowInTicks)
                    shouldContinue = false;
                else
                {
                    MidiOut(EventList[currentEventIndex].Value);

                    currentEventIndex++;
                }
            }

            if (currentEventIndex >= EventList.Count) // stop            
                PositionStop();            
            else
            {
                prevEventIndex = currentEventIndex;

                PositionUpdate((int)nowInTicks);
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
        
        void PositionUpdate(int nowInTicks)
        {
            int nowInTicksReduced = nowInTicks / 100;

            if (nowInTicksReduced > prevNowInTicksReduced)
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

        public void NoteOnEvent(NoteOnEvent e)
        {
            IVertex outputVertex = OutputDictionary[e.trackNumber];

            if (outputVertex != null)
            {
                IVertex playMethod = outputVertex.Get(false, @"$Is:\Method:NoteOn");

                IVertex parameters = InstructionHelpers.CreateStack();

                parameters.AddEdge(noteOnNoteMeta, e.eventVertex);

                ZeroCodeExecutonUtil.MethodCallFromHost(exe, playMethod, outputVertex, parameters);
            }
        }

        public void NoteOffEvent(NoteOffEvent e)
        {
            IVertex outputVertex = OutputDictionary[e.trackNumber];

            if (outputVertex != null)
            {
                IVertex playMethod = outputVertex.Get(false, @"$Is:\Method:NoteOff");

                IVertex parameters = InstructionHelpers.CreateStack();

                parameters.AddEdge(noteOnNoteMeta, e.eventVertex);

                ZeroCodeExecutonUtil.MethodCallFromHost(exe, playMethod, outputVertex, parameters);
            }
        }

        public void ControlChangeEvent(ControlChangeEvent e)
        {
            IVertex outputVertex = OutputDictionary[e.trackNumber];

            if (outputVertex != null)
            {
                IVertex playMethod = outputVertex.Get(false, @"$Is:\Method:ControlChange");

                IVertex parameters = InstructionHelpers.CreateStack();

                parameters.AddEdge(noteOnNoteMeta, e.eventVertex);

                ZeroCodeExecutonUtil.MethodCallFromHost(exe, playMethod, outputVertex, parameters);
            }
        }
    }
}
