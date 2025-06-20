using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0.ZeroCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Lib
{
    public class SongEvent {
        public int trackNumber;
        public IVertex eventVertex;        
    }

    public class NoteOnEvent : SongEvent {
        public bool isDrum;
    }

    public class NoteOffEvent : SongEvent {}

    public class ControlChangeEvent : SongEvent {}

    public class EOSEvent : SongEvent { }

    public class SongEventsDictionary
    {
        public IVertex baseVertex;

        public bool NeedToRebuildOutputDictionary;
        public bool NeedToRebuildEventDictionary;

        IList<IVertex> outputDictionary;
        IDictionary<int, IList<SongEvent>> eventDictionary;

        public SongEventsDictionary(IVertex _baseVertex)
        {
            baseVertex = _baseVertex;

            NeedToRebuildEventDictionary = true;
            NeedToRebuildOutputDictionary = true;
        }

        void BuildOutputDicionary()
        {
            outputDictionary = new List<IVertex>();

            bool thereIsSolo = false;

            if (baseVertex.Get(false, @"Track:\IsSolo:True") != null)
                thereIsSolo = true;

            foreach (IEdge e in baseVertex.GetAll(false, @"Track:"))
            {
                IVertex noteOutputVeretx = e.To.Get(false, @"Output:");

                bool canPlay = true;

                bool isMuted = GraphUtil.GetBooleanValueOrFalse(e.To.Get(false, "IsMuted:"));

                bool isSolo = GraphUtil.GetBooleanValueOrFalse(e.To.Get(false, "IsSolo:"));

                if (thereIsSolo && !isSolo)
                    canPlay = false;
                else
                {
                    if (!isSolo && isMuted)
                        canPlay = false;
                }

                if (noteOutputVeretx != null && canPlay && InstructionHelpers.CheckIfIsOrInherits_WRONG(noteOutputVeretx, "NoteOutput"))
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

            one.eventVertex = eventVertex;            

            tempDictAdd(baseTriggerTime + triggerTime, one);

            if (length == 0)
                one.isDrum = true;
            else
            {                
                // NoteOff

                NoteOffEvent offe = new NoteOffEvent();

                offe.trackNumber = trackNumber;

                offe.eventVertex = eventVertex;

                tempDictAdd(baseTriggerTime + triggerTime + length, offe);
            }
        }

        void AddControlChangeEvent(int trackNumber, int baseTriggerTime, IVertex eventVertex)
        {
            int triggerTime = GraphUtil.GetIntegerValueOr0(eventVertex.Get(false, "TriggerTime:"));
            ControlChangeEvent cce = new ControlChangeEvent();

            cce.trackNumber = trackNumber;

            cce.eventVertex = eventVertex;            

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

            int maxPosition = 0;

            int cnt = 0;
            foreach (IEdge e in baseVertex.GetAll(false, @"Track:"))
            {
                foreach (IEdge ee in e.To.GetAll(false, @"SequenceEvent:")) {
                    IVertex sequenceEventVertex = ee.To;

                    AddSequenceEvent(cnt, sequenceEventVertex);

                    int sequenceEventTrigger = GraphUtil.GetIntegerValueOr0(sequenceEventVertex.Get(false, "TriggerTime:"));
                    int sequenceLength = GraphUtil.GetIntegerValueOr0(sequenceEventVertex.Get(false, @"Sequence:\Length:"));

                    int currentMax = sequenceEventTrigger + sequenceLength;

                    if (currentMax > maxPosition)
                        maxPosition = currentMax;
                }

                cnt++;
            }

            AddEOSEvent(maxPosition);

            eventDictionary = new SortedDictionary<int, IList<SongEvent>>(tempDict);
        }

        void AddEOSEvent(int triggerTime)
        {
            tempDictAdd(triggerTime, new EOSEvent());
        }

        public IList<KeyValuePair<int, IList<SongEvent>>> GetEventDicionary()
        {
            if (NeedToRebuildEventDictionary)
                BuildEventDicionary();

            return eventDictionary.ToList();
        }
    }
}
