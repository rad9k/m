using System.Collections.Generic;
using m0.Foundation;

namespace m0.Graph.Internal
{
    internal enum EdgeBucketRemovalResult
    {
        NotFound,
        Removed,
        CollapsedToSingle,
        Emptied
    }

    internal struct EdgeBucket
    {
        private IEdge singleEdge;
        private List_VertexBase multipleEdges;

        internal readonly bool IsEmpty =>
            singleEdge == null &&
            multipleEdges == null;

        internal void Add(
            IEdge edge,
            out bool promotedToMany)
        {
            promotedToMany = false;

            if (singleEdge == null &&
                multipleEdges == null)
            {
                singleEdge = edge;
                return;
            }

            if (multipleEdges != null)
            {
                multipleEdges.Add(edge);
                return;
            }

            multipleEdges =
                new List_VertexBase
                {
                    singleEdge,
                    edge
                };
            singleEdge = null;
            promotedToMany = true;
        }

        internal EdgeBucketRemovalResult Remove(
            IEdge edge)
        {
            if (multipleEdges == null)
            {
                if (!ReferenceEquals(
                    singleEdge,
                    edge))
                    return
                        EdgeBucketRemovalResult
                            .NotFound;

                singleEdge = null;
                return
                    EdgeBucketRemovalResult
                        .Emptied;
            }

            int removeIndex = -1;

            if (multipleEdges.Count > 0 &&
                ReferenceEquals(
                    multipleEdges[
                        multipleEdges.Count - 1],
                    edge))
                removeIndex =
                    multipleEdges.Count - 1;
            else
                for (int index = 0;
                    index < multipleEdges.Count;
                    index++)
                    if (ReferenceEquals(
                        multipleEdges[index],
                        edge))
                    {
                        removeIndex = index;
                        break;
                    }

            if (removeIndex < 0)
                return
                    EdgeBucketRemovalResult
                        .NotFound;

            multipleEdges.RemoveAt(removeIndex);

            if (multipleEdges.Count == 1)
            {
                singleEdge = multipleEdges[0];
                multipleEdges = null;
                return
                    EdgeBucketRemovalResult
                        .CollapsedToSingle;
            }

            if (multipleEdges.Count == 0)
            {
                multipleEdges = null;
                return
                    EdgeBucketRemovalResult
                        .Emptied;
            }

            return
                EdgeBucketRemovalResult.Removed;
        }

        internal readonly void GetQueryResult(
            out IEdge result,
            out IList<IEdge> results)
        {
            result = singleEdge;
            results = multipleEdges;
        }
    }
}
