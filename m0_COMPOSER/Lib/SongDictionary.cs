using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Lib
{
    class SongEvent {
        int trackNumber;
    }

    class NoteOnEvent : SongEvent
    {        
        int octave;
        int note;
        int velocity;
    }

    class NoteOffEvent : SongEvent
    {     
        int octave;
        int note;
        int velocity;
    }

    class ControlChangeEvent : SongEvent
    {
        int number;
        int value;
    }

    class SongDictionary
    {
        public IVertex baseVertex;

        public bool NeedToRebuildOutputDictionary;
        public bool NeedToRebuildEventDictionary;

        IList<IVertex> outputDictionary;
        IDictionary<int, SongEvent> eventDictionary;

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

        void AddSequenceEvent(IVertex sequenceEventVertex)
        {

        }

        void AddTrack(IVertex trackVertex)
        {

        }

        void BuildEventDicionary()
        {
            IDictionary<int, SongEvent> dict = new Dictionary<int, SongEvent>();

            eventDictionary = new Dictionary<int, SongEvent>();
        }

        public IDictionary<int, SongEvent> GetEventDicionary()
        {
            if (NeedToRebuildEventDictionary)
                BuildEventDicionary();

            return eventDictionary;
        }
    }
}
