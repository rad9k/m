using m0.Foundation;
using m0.Graph;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Lib
{
    public class SongEvent {
        public int trackNumber;
    }

    public class NoteOnEvent : SongEvent
    {
        public int octave;
        public int note;
        public int velocity;
    }

    public class NoteOffEvent : SongEvent
    {
        public int octave;
        public int note;
        public int velocity;
    }

    public class ControlChangeEvent : SongEvent
    {
        public int number;
        public int value;
    }

    public class SongDictionary
    {
        public IVertex baseVertex;

        public bool NeedToRebuildOutputDictionary;
        public bool NeedToRebuildEventDictionary;

        IList<IVertex> outputDictionary;
        IDictionary<int, IList<SongEvent>> eventDictionary;

        public SongDictionary(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;

            NeedToRebuildEventDictionary = true;
            NeedToRebuildOutputDictionary = true;
        }

        void BuildOutputDicionary()
        {
            outputDictionary = new List<IVertex>();

            foreach (IEdge e in baseVertex.GetAll(false, @"Track:"))
            {
                IVertex noteOutputVeretx = e.To.Get(false, "Output:");

                if (noteOutputVeretx != null)
                    outputDictionary.Add(noteOutputVeretx);
                else
                    outputDictionary.Add(null);
            }
        }

        public IList<IVertex> GetOutputDicionary()
        {
            if (NeedToRebuildOutputDictionary)
                BuildOutputDicionary();

            return outputDictionary;
        }

        void AddSequenceEvent(int trackNumber, IVertex sequenceEventVertex)
        {
            int triggerTime = GraphUtil.GetIntegerValueOr0(sequenceEventVertex.Get(false, "TriggerTime:"));

            foreach (IEdge e in sequenceEventVertex.GetAll(false, @"Sequence:\Event:"))
                AddEvent(trackNumber, triggerTime, e.To);
        }
        
        void AddEvent(int trackNumber, int triggerTime, IVertex eventVertex)
        {
            IVertex isVertex = eventVertex.Get(false, "$Is:");

            if (GeneralUtil.CompareStrings(isVertex.Value, "NoteEvent"))
                AddNoteEvent(trackNumber, triggerTime, eventVertex);

            if (GeneralUtil.CompareStrings(isVertex.Value, "ControlChangeEvent"))
                AddControlChangeEvent(trackNumber, triggerTime, eventVertex);
        }

        void AddNoteEvent(int trackNumber, int baseTriggerTime, IVertex eventVertex)
        {
            int triggerTime = GraphUtil.GetIntegerValueOr0(eventVertex.Get(false, "TriggerTime:"));
            int length = GraphUtil.GetIntegerValueOr0(eventVertex.Get(false, "Length:"));

            // NoteOn

            NoteOnEvent one = new NoteOnEvent();

            one.trackNumber = trackNumber;

            one.note = GraphUtil.GetIntegerValueOr0(eventVertex.Get(false, "Note:"));
            one.octave = GraphUtil.GetIntegerValueOr0(eventVertex.Get(false, "Octave:"));
            one.velocity = GraphUtil.GetIntegerValueOr0(eventVertex.Get(false, "Velocity:"));

            tempDictAdd(baseTriggerTime + triggerTime, one);

            // NoteOff

            NoteOffEvent offe = new NoteOffEvent();

            offe.trackNumber = trackNumber;

            offe.note = GraphUtil.GetIntegerValueOr0(eventVertex.Get(false, "Note:"));
            offe.octave = GraphUtil.GetIntegerValueOr0(eventVertex.Get(false, "Octave:"));
            offe.velocity = GraphUtil.GetIntegerValueOr0(eventVertex.Get(false, "Velocity:"));

            tempDictAdd(baseTriggerTime + triggerTime + length, offe);
        }

        void AddControlChangeEvent(int trackNumber, int baseTriggerTime, IVertex eventVertex)
        {
            int triggerTime = GraphUtil.GetIntegerValueOr0(eventVertex.Get(false, "TriggerTime:"));
            ControlChangeEvent cce = new ControlChangeEvent();

            cce.trackNumber = trackNumber;

            cce.number = GraphUtil.GetIntegerValueOr0(eventVertex.Get(false, "Number:"));
            cce.value = GraphUtil.GetIntegerValueOr0(eventVertex.Get(false, "Value:"));            

            tempDictAdd(baseTriggerTime + triggerTime, cce);
        }

        void tempDictAdd(int triggerTime, SongEvent e)
        {
            if (tempDict.ContainsKey(triggerTime)) {
                IList<SongEvent> l = tempDict[triggerTime];
                l.Add(e);
            }
            else
            {
                IList<SongEvent> l = new List<SongEvent>();
                tempDict.Add(triggerTime, l);
                l.Add(e);
            }
        }

        IDictionary<int, IList<SongEvent>> tempDict;

        void BuildEventDicionary()
        {
            tempDict = new Dictionary<int, IList<SongEvent>>();

            int cnt = 0;
            foreach (IEdge e in baseVertex.GetAll(false, @"Track:\SequenceEvent:"))
                AddSequenceEvent(cnt++, e.To);

            eventDictionary = new SortedDictionary<int, IList<SongEvent>>(tempDict);
        }

        public IDictionary<int, IList<SongEvent>> GetEventDicionary()
        {
            if (NeedToRebuildEventDictionary)
                BuildEventDicionary();

            return eventDictionary;
        }
    }
}
