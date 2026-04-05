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
        // InEdges
        // from == who inherits from me
        // meta == $Inherits
        // to == this

        public static HashSet<IVertex> GetInheritChilds(IVertex baseVertex)
        {
            HashSet<IVertex> inheritsSet = new HashSet<IVertex>();

            GetInheritChilds_RawEnumerate_recurrent(baseVertex, inheritsSet);

            return inheritsSet;
        }

        private static void GetInheritChilds_RawEnumerate_recurrent(IVertex baseVertex, HashSet<IVertex> inheritedSet)
        {
           if (baseVertex is EasyVertex) // FAST
            {
                EasyVertex baseVertex_EasyVertex = (EasyVertex)baseVertex;

                foreach (IEdge e in baseVertex_EasyVertex.InheritsInEdges)
                    if (!inheritedSet.Contains(e.From))
                    {
                        inheritedSet.Add(e.From);
                        GetInheritChilds_RawEnumerate_recurrent(e.From, inheritedSet);
                    }
            }
            else // SLOW
                foreach (IEdge e in baseVertex.InEdgesRaw)
                    if (GeneralUtil.CompareStrings(e.Meta, "$Inherits") && !inheritedSet.Contains(e.From))
                    {
                        inheritedSet.Add(e.From);
                        GetInheritChilds_RawEnumerate_recurrent(e.From, inheritedSet);
                    }
        }

        // OutEdgesRaw
        // from == this
        // meta == $Inherits
        // to == who I inherit from
        public static HashSet<IVertex> GetInheritParents(IVertex baseVertex)
        {
            HashSet<IVertex> inheritsSet = new HashSet<IVertex>();

            GetInheritParents_RawEnumerate_recurrent(baseVertex, inheritsSet);

            return inheritsSet;
        }

        private static void GetInheritParents_RawEnumerate_recurrent(IVertex baseVertex, HashSet<IVertex> inheritedSet)
        {
            if (baseVertex is EasyVertex) // FAST
            {
                EasyVertex baseVertex_EasyVertex = (EasyVertex)baseVertex;

                foreach (IEdge e in baseVertex_EasyVertex.InheritsOutEdges)
                    if (!inheritedSet.Contains(e.To))
                    {
                        inheritedSet.Add(e.To);
                        GetInheritParents_RawEnumerate_recurrent(e.To, inheritedSet);
                    }
            } 
            else // SLOW
                foreach (IEdge e in baseVertex.OutEdgesRaw)
                    if (GeneralUtil.CompareStrings(e.Meta, "$Inherits") && !inheritedSet.Contains(e.To))
                    {
                        inheritedSet.Add(e.To);
                        GetInheritParents_RawEnumerate_recurrent(e.To, inheritedSet);
                    }
        }
    }
}
