using System;
using System.Collections;
using System.Collections.Generic;
using m0.Foundation;

namespace m0.Graph
{
    public readonly struct EdgeQueryResult :
        IReadOnlyList<IEdge>
    {
        private readonly IEdge singleEdge;
        private readonly IList<IEdge> multipleEdges;

        internal EdgeQueryResult(
            IEdge singleEdge,
            IList<IEdge> multipleEdges)
        {
            this.singleEdge = singleEdge;
            this.multipleEdges =
                singleEdge == null
                    ? multipleEdges
                    : null;
        }

        internal EdgeQueryResult(
            IList<IEdge> multipleEdges)
            : this(null, multipleEdges)
        {
        }

        public int Count =>
            singleEdge != null
                ? 1
                : multipleEdges?.Count ?? 0;

        public bool IsEmpty => Count == 0;

        public IEdge FirstOrDefault =>
            singleEdge ??
            (multipleEdges != null &&
             multipleEdges.Count > 0
                ? multipleEdges[0]
                : null);

        public IEdge this[int index]
        {
            get
            {
                if (singleEdge != null)
                {
                    if (index == 0)
                        return singleEdge;

                    throw new ArgumentOutOfRangeException(
                        nameof(index));
                }

                if (multipleEdges == null)
                    throw new ArgumentOutOfRangeException(
                        nameof(index));

                return multipleEdges[index];
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<IEdge>
            IEnumerable<IEdge>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<IEdge>
        {
            private readonly IEdge singleEdge;
            private readonly List<IEdge> listEdges;
            private readonly IList<IEdge> otherEdges;
            private readonly int count;
            private int index;

            internal Enumerator(
                EdgeQueryResult result)
            {
                singleEdge = result.singleEdge;
                listEdges =
                    result.multipleEdges
                        as List<IEdge>;
                otherEdges =
                    listEdges == null
                        ? result.multipleEdges
                        : null;
                count = result.Count;
                index = -1;
            }

            public IEdge Current
            {
                get
                {
                    if (listEdges != null)
                        return listEdges[index];

                    if (singleEdge != null)
                        return singleEdge;

                    return otherEdges[index];
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                int nextIndex = index + 1;

                if (nextIndex >= count)
                    return false;

                index = nextIndex;
                return true;
            }

            public void Reset()
            {
                index = -1;
            }

            public void Dispose()
            {
            }
        }
    }
}
