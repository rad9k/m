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
        public static HashSet<IVertex> GetInheritChilds(IVertex baseVertex)
        {
            HashSet<IVertex> inheritsSet = new HashSet<IVertex>();

            GetInheritChilds_RawEnumerate_recurrent(baseVertex, inheritsSet);

            return inheritsSet;
        }

        private static void GetInheritChilds_RawEnumerate_recurrent(IVertex baseVertex, HashSet<IVertex> inheritedSet)
        {
            foreach (IEdge e in baseVertex.InEdgesRaw)
                if (GeneralUtil.CompareStrings(e.Meta, "$Inherits") && !inheritedSet.Contains(e.From))
                {
                    inheritedSet.Add(e.From);
                    GetInheritChilds_RawEnumerate_recurrent(e.From, inheritedSet);
                }
        }

        public static HashSet<IVertex> GetInheritParents(IVertex baseVertex)
        {
            HashSet<IVertex> inheritsSet = new HashSet<IVertex>();

            GetInheritParents_RawEnumerate_recurrent(baseVertex, inheritsSet);

            return inheritsSet;
        }

        private static void GetInheritParents_RawEnumerate_recurrent(IVertex baseVertex, HashSet<IVertex> inheritedSet)
        {
            foreach (IEdge e in baseVertex.OutEdgesRaw)
                if (GeneralUtil.CompareStrings(e.Meta, "$Inherits") && !inheritedSet.Contains(e.To))
                {
                    inheritedSet.Add(e.To);
                    GetInheritParents_RawEnumerate_recurrent(e.To, inheritedSet);
                }
        }

        public static HashSet<IVertex> GetInheritParents_IVertex(IVertex baseVertex)
        {
            HashSet<IVertex> inheritsSet = new HashSet<IVertex>();

            GetInheritParents_IVertex_recurrent(baseVertex, inheritsSet);

            return inheritsSet;
        }

        private static void GetInheritParents_IVertex_recurrent(IVertex baseVertex, HashSet<IVertex> inheritedSet)
        {
            foreach (IEdge e in baseVertex.OutEdgesRaw)
                if (GeneralUtil.CompareStrings(e.Meta, "$Inherits") && !inheritedSet.Contains(e.To))
                {
                    inheritedSet.Add(e.To);
                    GetInheritParents_RawEnumerate_recurrent(e.To, inheritedSet);
                }
        }
    }
}
