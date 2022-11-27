using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Util;
using m0.Graph;

namespace m0.ZeroTypes
{
    class PlatformClassVertexChangeListener
    {
        public PlatformClassVertexChangeListener(string[] watchList)
        {
            foreach (string w in watchList)
                WatchList.Add(w);
        }

        public List<string> WatchList = new List<string>();

        public event VertexChange Change;

        public IVertex PlatformClassVertex;
    }

        public class PlatformClass
        {
            // static DictionaryList<string, Delegate> ListenerGroupDictionary = new DictionaryList<string, Delegate>();

            public static IPlatformClass CreatePlatformObject(IVertex Vertex, IEdge baseEdge)
            {
                return CreatePlatformObject(Vertex, baseEdge, null);
            }

            public static IPlatformClass CreatePlatformObject(IVertex Vertex, IEdge baseEdge, IVertex _parentVisualiser)
            {
                if (baseEdge == null)
                    return CreatePlatformObject(Vertex, null as IVertex, _parentVisualiser);
                else
                    return CreatePlatformObject(Vertex, EdgeHelper.CreateTempEdgeVertex(baseEdge), _parentVisualiser);
            }

            public static IPlatformClass CreatePlatformObject(IVertex Vertex, IVertex baseEdgeVertex)
            {
                return CreatePlatformObject(Vertex, baseEdgeVertex, null);
            }

            public static IPlatformClass CreatePlatformObject(IVertex Vertex, IVertex baseEdgeVertex, IVertex _parentVisualiser)
            {
                IPlatformClass pc;

                if (Vertex.Get(false, "$Is:Class") != null)
                {
                    String classname = (string)Vertex.Get(false, "$PlatformClassName:").Value;

                    pc = (IPlatformClass)Activator.CreateInstance(Type.GetType(classname), new object[] { baseEdgeVertex, _parentVisualiser });
                }
                else
                {
                    String classname = (string)Vertex.Get(false, @"$Is:\$PlatformClassName:").Value;

                    pc = (IPlatformClass)Activator.CreateInstance(Type.GetType(classname), new object[] { baseEdgeVertex, _parentVisualiser });

                    pc.Vertex = Vertex;
                }

                return pc;
            }
    }
}
