using m0;
using m0.Foundation;
using m0.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0_COMPOSER.Lib
{
    public class Song
    {
        public static IVertex GetTrackVertexFromSequenceEventVertex(IVertex sequenceEventVertex)
        {
            return GraphUtil.GetQueryInFirst(sequenceEventVertex, "SequenceEvent", null);

            /*
            foreach (IEdge e in baseVertex.GetAll(false, @"Track:"))
                foreach (IEdge ee in e.To)
                    if (ee.To == sequenceEventVertex)
                        return e.To;

            return null;*/
        }

        public static IEdge InsertSequenceEvent(IVertex trackVertex, double startPosition, double lengthPosition)
        {
            IVertex r = MinusZero.Instance.Root;
            

            IVertex sequenceEventAttribute = r.Get(false, @"System\Lib\Music\Track\SequenceEvent");

            IVertex sequenceEvent = r.Get(false, @"System\Lib\Music\SequenceEvent");

            IVertex sequence = r.Get(false, @"System\Lib\Music\Sequence");

            IVertex sequenceIsDrum = r.Get(false, @"System\Lib\Music\Sequence\IsDrum");


            IEdge tempSequenceEventEdge = trackVertex.AddVertexAndReturnEdge(null, null);

            IVertex sequenceEventVertex = tempSequenceEventEdge.To;


            sequenceEventVertex.AddEdge(MinusZero.Instance.Is, sequenceEvent);

           
            sequenceEventVertex.AddVertex(sequenceEvent.Get(false, @"Attribute:TriggerTime"), ScreenPositionToMusicTime(startPosition, needsSnapCorrection));

            IVertex sequenceVertex = VertexOperations.AddInstance(sequenceEventVertex, sequence);

            sequenceVertex.AddVertex(sequence.Get(false, @"Attribute:Length"), ScreenPositionToMusicTime(lengthPosition, needsSnapCorrection));

            bool isDrum = false;

            bool isNull = false;

            if (GraphUtil.GetBooleanValue(toAddVertex.Get(false, "IsDrum:"), ref isNull))
                sequenceVertex.AddVertex(sequenceIsDrum, "True");

            IEdge finalEdge = toAddVertex.AddEdge(sequenceEventAttribute, sequenceEventVertex);

            toAddVertex.DeleteEdge(tempSequenceEventEdge);


            return finalEdge;
        }

        public static void RazorCut(IVertex songVertex, IVertex sequenceEventVertex, int cutPoint)
        {
            bool isNull = false;

            IVertex firstSequenceEventVertex = sequenceEventVertex;
            IVertex firstSequenceVertex = firstSequenceEventVertex.Get(false, @"Sequence:");

            int beforeTriggerTime = GraphUtil.GetIntegerValue(firstSequenceEventVertex.Get(false, "TriggerTime:"), ref isNull);
            int beforeLength = GraphUtil.GetIntegerValue(firstSequenceVertex.Get(false, "Length:"), ref isNull);

            int firstTriggerTime = beforeTriggerTime;
            int firestLength = cutPoint - beforeTriggerTime;

            int secondTriggerTime = cutPoint;
            int secondLength = beforeLength - cutPoint;



        }
    }
}
