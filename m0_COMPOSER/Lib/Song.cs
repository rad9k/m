using m0;
using m0.Foundation;
using m0.Graph;
using m0.Lib;
using m0.User.Process.UX;
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
    public class Song
    {
        public static INoInEdgeInOutVertexVertex Record(IExecution exe)
        {
            return null;
        }

        public static IDictionary<IVertex, SongPlay> SongPlaySongDictionary = new Dictionary<IVertex, SongPlay>();

        public static INoInEdgeInOutVertexVertex Play(IExecution exe)
        {            
            INoInEdgeInOutVertexVertex o = exe.Stack;                       

            bool isNull = false;       

            SongPlay sp = new SongPlay(exe, o);

            SongVertexDictionary.SetSongPlay(o, sp);

            sp.Start();

            return o;
        }

        public static INoInEdgeInOutVertexVertex Stop(IExecution exe)
        {            
            INoInEdgeInOutVertexVertex o = exe.Stack;            

            SongPlay sp = SongVertexDictionary.GetSongPlay(o);

            sp.Destroy();

            SongVertexDictionary.RemoveSongPlay(exe.Stack);

            return o;
        }        
             
        //

        public static IVertex GetTrackVertexFromSequenceEventVertex(IVertex sequenceEventVertex)
        {
            return GraphUtil.GetQueryInFirst(sequenceEventVertex, "SequenceEvent", null);         
        }

        static IVertex r = MinusZero.Instance.Root;

        static IVertex lengthMeta = r.Get(false, @"System\Lib\Music\Sequence\Length");
        static IVertex eventMeta = r.Get(false, @"System\Lib\Music\Sequence\Event");

        static IVertex sequenceEventAttributeMeta = r.Get(false, @"System\Lib\Music\Track\SequenceEvent");
        static IVertex sequenceEventMeta = r.Get(false, @"System\Lib\Music\SequenceEvent");
        static IVertex triggerTimeMeta = r.Get(false, @"System\Lib\Music\SequenceEvent\TriggerTime");
        static IVertex sequenceAttributeMeta = r.Get(false, @"System\Lib\Music\SequenceEvent\Sequence");
        static IVertex sequenceMeta = r.Get(false, @"System\Lib\Music\Sequence");
        static IVertex sequenceIsDrumMeta = r.Get(false, @"System\Lib\Music\Sequence\IsDrum");
        static IVertex pitchSetMeta = r.Get(false, @"System\Lib\Music\Sequence\PitchSet");
        static IVertex controlChangeDescriptionSetMeta = r.Get(false, @"System\Lib\Music\Sequence\ControlChangeDescriptionSet");

        public static string GetNameForNewSequenceEvent(IVertex trackVertex)
        {
            int max = 0;

            foreach(IEdge e in trackVertex.GetAll(false, @"SequenceEvent:\Sequence:"))
            {
                string seqName = e.To.Value.ToString();

                seqName = seqName.Split(' ')[0];

                int tryMax;

                if (Int32.TryParse(seqName, out tryMax))
                    if (tryMax > max)
                        max = tryMax;
            }

            max++;

            return max + " [" + trackVertex.Value+"]";
        }

        public static IEdge AddSequenceEventVertex(IVertex trackVertex, int startPosition, int lengthPosition)
        {            
            IEdge sequenceEventEdge = trackVertex.AddVertexAndReturnEdge(sequenceEventAttributeMeta, null);

            IVertex sequenceEventVertex = sequenceEventEdge.To;


            sequenceEventVertex.AddEdge(MinusZero.Instance.Is, sequenceEventMeta);

           
            sequenceEventVertex.AddVertex(sequenceEventMeta.Get(false, @"Attribute:TriggerTime"), startPosition);

            IVertex sequenceVertex = VertexOperations.AddInstance(sequenceEventVertex, sequenceMeta, sequenceAttributeMeta);

            sequenceVertex.Value = GetNameForNewSequenceEvent(trackVertex);
            
            sequenceVertex.AddVertex(sequenceMeta.Get(false, @"Attribute:Length"), lengthPosition);            

            bool isNull = false;

            if (GraphUtil.GetBooleanValue(trackVertex.Get(false, "IsDrum:"), ref isNull))
                sequenceVertex.AddVertex(sequenceIsDrumMeta, "True");            

            // pitchset and ControlChangeDescription

            IVertex trackOutput = trackVertex.Get(false, "Output:");

            if(trackOutput != null)
            {
                IVertex outputPitchSet = trackOutput.Get(false, "PitchSet:");

                if (outputPitchSet != null)
                    GraphUtil.CreateOrReplaceEdge(sequenceVertex, pitchSetMeta, outputPitchSet);

                IVertex controlChangeDescriptionSet = trackOutput.Get(false, "ControlChangeDescriptionSet:");

                if (controlChangeDescriptionSet != null)
                    GraphUtil.CreateOrReplaceEdge(sequenceVertex, controlChangeDescriptionSetMeta, controlChangeDescriptionSet);
            }

            return sequenceEventEdge;
        }

        public static void RazorCut(IVertex songVertex, IVertex sequenceEventVertex, int cutPoint)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
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
            
            foreach (IEdge e in firstSequenceVertex.GetAll(false, "Event:"))
            {
                int positionInFirst = GraphUtil.GetIntegerValue(e.To.Get(false, "TriggerTime:"), ref isNull);

                IVertex eventVertex = e.To;

                if(positionInFirst > firstLength)
                {
                    secondSequenceVertex.AddEdge(eventMeta, eventVertex);

                    firstSequenceVertex.DeleteEdge(e);

                    GraphUtil.SetVertexValue(eventVertex, triggerTimeMeta, positionInFirst - firstLength);
                }
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static int SequenceEventTriggerTimeCompare(IEdge sequenceEventEdgeA, IEdge sequenceEventEdgeB)
        {
            bool isNull = false;

            int triggerTimeA = GraphUtil.GetIntegerValue(sequenceEventEdgeA.To.Get(false, "TriggerTime:"), ref isNull);
            int triggerTimeB = GraphUtil.GetIntegerValue(sequenceEventEdgeB.To.Get(false, "TriggerTime:"), ref isNull);

            if (triggerTimeA < triggerTimeB)
                return -1;

            if (triggerTimeA == triggerTimeB)
                return 0;

            return 1;
        }

        public static IList<IEdge> SortSequenceEventsInTrack(IVertex trackVertex)
        {
            List<IEdge> list = new List<IEdge>();

            foreach (IEdge e in trackVertex.GetAll(false, "SequenceEvent:"))
                list.Add(e);

            list.Sort(SequenceEventTriggerTimeCompare);

            return list;
        }

        public static IEdge GetSequenceOntheLeftOrRight(IVertex sequenceEventVertex, bool isRight)
        {            
            IVertex trackVertex = GetTrackVertexFromSequenceEventVertex(sequenceEventVertex);

            IList<IEdge> sorted = SortSequenceEventsInTrack(trackVertex);

            IEdge sequenceEventEdge = GraphUtil.FindEdge(trackVertex, sequenceEventAttributeMeta, sequenceEventVertex);

            int indexOfSequence = sorted.IndexOf(sequenceEventEdge);

            if (isRight)
            {
                if (indexOfSequence == sorted.Count - 1)
                    return null;

                return sorted[indexOfSequence + 1];
            }
            else
            {
                if (indexOfSequence == 0)
                    return null;

                return sorted[indexOfSequence - 1];
            }
        }

        public static void Glue(IVertex songVertex, IEdge sequenceEventEdge, int gluePoint)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            IVertex r = MinusZero.Instance.Root;
            bool isNull = false;

            IVertex sequenceEventVertex = sequenceEventEdge.To;

            IVertex trackVertex = GetTrackVertexFromSequenceEventVertex(sequenceEventVertex);

            IVertex sequenceVertex = sequenceEventVertex.Get(false, @"Sequence:");

            int triggerTime = GraphUtil.GetIntegerValue(sequenceEventVertex.Get(false, "TriggerTime:"), ref isNull);
            int length = GraphUtil.GetIntegerValue(sequenceVertex.Get(false, "Length:"), ref isNull);

            int localPosition = gluePoint - triggerTime;

            if (localPosition > (length / 2))
            {
                IEdge sibilingSequenceEvent = GetSequenceOntheLeftOrRight(sequenceEventVertex, true);

                if (sibilingSequenceEvent != null)
                    Glue(sequenceEventEdge, sibilingSequenceEvent);
                else
                {
                    sibilingSequenceEvent = GetSequenceOntheLeftOrRight(sequenceEventVertex, false);

                    if (sibilingSequenceEvent != null)
                        Glue(sibilingSequenceEvent, sequenceEventEdge);
                }
            }
            else
            {
                IEdge sibilingSequenceEvent = GetSequenceOntheLeftOrRight(sequenceEventVertex, false);

                if (sibilingSequenceEvent != null)
                    Glue(sibilingSequenceEvent, sequenceEventEdge);
                else
                {
                    sibilingSequenceEvent = GetSequenceOntheLeftOrRight(sequenceEventVertex, true);

                    if (sibilingSequenceEvent != null)
                        Glue(sequenceEventEdge, sibilingSequenceEvent);
                }
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static void Glue(IEdge sequenceEventEdge_Final, IEdge sequenceEventEdge_Delete)
        {
            bool isNull = false;

            IVertex sequenceEventVertex_Final = sequenceEventEdge_Final.To;
            IVertex sequenceEventVertex_Delete = sequenceEventEdge_Delete.To;

            IVertex trackVertex = GetTrackVertexFromSequenceEventVertex(sequenceEventVertex_Final);

            IVertex sequenceVertex_Final = sequenceEventVertex_Final.Get(false, @"Sequence:");
            IVertex sequenceVertex_Delete = sequenceEventVertex_Delete.Get(false, @"Sequence:");

            int triggerTime_Final = GraphUtil.GetIntegerValue(sequenceEventVertex_Final.Get(false, "TriggerTime:"), ref isNull);
            int triggerTime_Delete = GraphUtil.GetIntegerValue(sequenceEventVertex_Delete.Get(false, "TriggerTime:"), ref isNull);

            int length_Delete = GraphUtil.GetIntegerValue(sequenceVertex_Delete.Get(false, "Length:"), ref isNull);

            int delta = triggerTime_Delete - triggerTime_Final;

            foreach(IEdge e in sequenceVertex_Delete.GetAll(false, "Event:"))
            {
                IVertex eventVertex = e.To;

                sequenceVertex_Final.AddEdge(eventMeta, eventVertex);

                int eventTriggerTime = GraphUtil.GetIntegerValue(eventVertex.Get(false, "TriggerTime:"), ref isNull);

                GraphUtil.SetVertexValue(eventVertex, triggerTimeMeta, eventTriggerTime + delta);

                sequenceEventVertex_Delete.DeleteEdge(e);
            }

            int newLength_Final = delta + length_Delete;

            GraphUtil.SetVertexValue(sequenceVertex_Final, lengthMeta, newLength_Final);

            trackVertex.DeleteEdge(sequenceEventEdge_Delete);
        }


    }
}
