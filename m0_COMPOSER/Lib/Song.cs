using m0;
using m0.Foundation;
using m0.Graph;
using m0.ZeroTypes;
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

        static IVertex r = MinusZero.Instance.Root;

        static IVertex lengthMeta = r.Get(false, @"System\Lib\Music\Sequence\Length");
        static IVertex eventMeta = r.Get(false, @"System\Lib\Music\Sequence\Event");

        static IVertex sequenceEventAttributeMeta = r.Get(false, @"System\Lib\Music\Track\SequenceEvent");

        static IVertex sequenceEventMeta = r.Get(false, @"System\Lib\Music\SequenceEvent");

        static IVertex sequenceMeta = r.Get(false, @"System\Lib\Music\Sequence");

        static IVertex sequenceIsDrumMeta = r.Get(false, @"System\Lib\Music\Sequence\IsDrum");

        public static IEdge AddSequenceEventVertex(IVertex trackVertex, double startPosition, double lengthPosition)
        {            
            IEdge tempSequenceEventEdge = trackVertex.AddVertexAndReturnEdge(null, null);

            IVertex sequenceEventVertex = tempSequenceEventEdge.To;


            sequenceEventVertex.AddEdge(MinusZero.Instance.Is, sequenceEventMeta);

           
            sequenceEventVertex.AddVertex(sequenceEventMeta.Get(false, @"Attribute:TriggerTime"), startPosition);

            IVertex sequenceVertex = VertexOperations.AddInstance(sequenceEventVertex, sequenceMeta);

            sequenceVertex.AddVertex(sequenceMeta.Get(false, @"Attribute:Length"), lengthPosition);

            bool isDrum = false;

            bool isNull = false;

            if (GraphUtil.GetBooleanValue(trackVertex.Get(false, "IsDrum:"), ref isNull))
                sequenceVertex.AddVertex(sequenceIsDrumMeta, "True");

            IEdge finalEdge = trackVertex.AddEdge(sequenceEventAttributeMeta, sequenceEventVertex);

            trackVertex.DeleteEdge(tempSequenceEventEdge);


            return finalEdge;
        }

        public static void RazorCut(IVertex songVertex, IVertex sequenceEventVertex, int cutPoint)
        {
            IVertex r = MinusZero.Instance.Root;
            bool isNull = false;

            IVertex trackVertex = GetTrackVertexFromSequenceEventVertex(sequenceEventVertex);

            IVertex firstSequenceEventVertex = sequenceEventVertex;
            IVertex firstSequenceVertex = firstSequenceEventVertex.Get(false, @"Sequence:");            

            int beforeTriggerTime = GraphUtil.GetIntegerValue(firstSequenceEventVertex.Get(false, "TriggerTime:"), ref isNull);
            int beforeLength = GraphUtil.GetIntegerValue(firstSequenceVertex.Get(false, "Length:"), ref isNull);

            int firstTriggerTime = beforeTriggerTime;
            int firstLength = cutPoint - beforeTriggerTime;

            int secondTriggerTime = cutPoint;
            int secondLength = beforeLength - firstLength;            

            GraphUtil.SetVertexValue(firstSequenceVertex, lengthMeta, firstLength);

            IVertex secondSequenceEventVertex = AddSequenceEventVertex(trackVertex, secondTriggerTime, secondLength).To;
            IVertex secondSequenceVertex = secondSequenceEventVertex.Get(false, @"Sequence:");

            foreach(IEdge e in firstSequenceVertex.GetAll(false, "Event:"))
            {
                int positionInFirst = GraphUtil.GetIntegerValue(e.To.Get(false, "TriggerTime:"), ref isNull);

                IVertex eventVertex = e.To;

                if(positionInFirst > firstLength)
                {
                    secondSequenceVertex.AddEdge(eventMeta, eventVertex);

                    firstSequenceEventVertex.DeleteEdge(e);

                    GraphUtil.SetVertexValue(eventVertex, triggerTimeMeta, positionInFirst - firstLength);
                }
            }


        }
    }
}
