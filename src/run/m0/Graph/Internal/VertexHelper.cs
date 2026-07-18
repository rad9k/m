using m0.Foundation;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.Internal
{
    internal class VertexHelper
    {
        // InEdgesRaw
        // from == who inherits from me
        // meta == $Inherits
        // to == this

        public static HashSet<IVertex> GetInheritChilds(IVertex baseVertex)
        {
            HashSet<IVertex> inheritsSet = new HashSet<IVertex>();
            Stack<IVertex> pending =
                new Stack<IVertex>();
            PushInheritChildrenInReverse(
                baseVertex,
                pending);

            while (pending.Count > 0)
            {
                IVertex child = pending.Pop();

                if (ReferenceEquals(
                        child,
                        baseVertex) ||
                    !inheritsSet.Add(child))
                    continue;

                PushInheritChildrenInReverse(
                    child,
                    pending);
            }

            return inheritsSet;
        }

        private static void PushInheritChildrenInReverse(
            IVertex baseVertex,
            Stack<IVertex> pending)
        {
            if (baseVertex is EasyVertex easyVertex)
            {
                IList<IEdge> edges =
                    easyVertex.InheritsInEdges;

                for (int index = edges.Count - 1;
                    index >= 0;
                    index--)
                    pending.Push(edges[index].From);

                return;
            }

            IList<IEdge> rawEdges =
                baseVertex.InEdgesRaw;

            for (int index = rawEdges.Count - 1;
                index >= 0;
                index--)
            {
                IEdge edge = rawEdges[index];

                if (GeneralUtil.CompareStrings(
                    edge.Meta,
                    "$Inherits"))
                    pending.Push(edge.From);
            }
        }

        // OutEdgesRaw
        // from == this
        // meta == $Inherits
        // to == who I inherit from
        public static HashSet<IVertex> GetInheritParents(IVertex baseVertex)
        {
            HashSet<IVertex> inheritsSet = new HashSet<IVertex>();
            Stack<IVertex> pending =
                new Stack<IVertex>();
            PushInheritParentsInReverse(
                baseVertex,
                pending);

            while (pending.Count > 0)
            {
                IVertex parent = pending.Pop();

                if (ReferenceEquals(
                        parent,
                        baseVertex) ||
                    !inheritsSet.Add(parent))
                    continue;

                PushInheritParentsInReverse(
                    parent,
                    pending);
            }

            return inheritsSet;
        }

        private static void PushInheritParentsInReverse(
            IVertex baseVertex,
            Stack<IVertex> pending)
        {
            if (baseVertex is EasyVertex easyVertex)
            {
                IList<IEdge> edges =
                    easyVertex.InheritsOutEdges;

                for (int index = edges.Count - 1;
                    index >= 0;
                    index--)
                    pending.Push(edges[index].To);

                return;
            }

            IList<IEdge> rawEdges =
                baseVertex.OutEdgesRaw;

            for (int index = rawEdges.Count - 1;
                index >= 0;
                index--)
            {
                IEdge edge = rawEdges[index];

                if (GeneralUtil.CompareStrings(
                    edge.Meta,
                    "$Inherits"))
                    pending.Push(edge.To);
            }
        }
    }
}
