using m0.Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Util
{
    public enum VertexDebugType { Add, AddList, Remove, RemoveList}

    public class VertexEntry
    {
        public string FromStore;
        public VertexDebugType Type;
        public string FromIdentifier;
        public string FromValue;
        public string MetaIdentifier;
        public string MetaValue;
        public string ToIdentifier;
        public string ToValue;
        public int EdgesCount;
    }

    public class VertexDebugDB
    {
        public static List<VertexEntry> list = new List<VertexEntry>();

        public static void Add(VertexDebugType type, IVertex fromVertex, IEdge edge)
        {
            Add(type, fromVertex, edge, 0);
        }

        public static void Add(VertexDebugType type, IVertex fromVertex, IEdge edge, int count)
        {
            VertexEntry e = new VertexEntry();

            e.Type = type;
            e.FromStore = fromVertex.Store.Identifier;

            e.FromIdentifier = fromVertex.Identifier.ToString();
            e.FromValue = fromVertex.Value.ToString();

            if (edge != null) {
                e.MetaIdentifier = edge.Meta.Identifier.ToString();
                e.MetaValue = edge.Meta.Value.ToString();

                e.ToIdentifier = edge.To.Identifier.ToString();
                e.ToValue = edge.To.Value.ToString();
            }

            if (type == VertexDebugType.Add || type == VertexDebugType.Remove)
                e.EdgesCount = fromVertex.OutEdgesRaw.Count();
            else
                e.EdgesCount = count;

            list.Add(e);
        }

        static void EmitAgregated(StreamWriter w, string store)
        {
            int AddCount = 0;
            int RemoveCount = 0;
            int AddListCount = 0;
            int AddListEdges = 0;
            int RemoveListCount = 0;
            int RemoveListEdges = 0;

            foreach(VertexEntry e in list)
                if(store == null || e.FromStore == store)
                {
                    switch (e.Type)
                    {
                        case VertexDebugType.Add:
                            AddCount++;
                            break;

                        case VertexDebugType.Remove:
                            RemoveCount++;
                            break;

                        case VertexDebugType.AddList:
                            AddListCount++;
                            AddListEdges += e.EdgesCount;
                            break;

                        case VertexDebugType.RemoveList:
                            RemoveListCount++;
                            RemoveListEdges += e.EdgesCount;
                            break;
                    }
                }

            w.WriteLine(store + " Add: " + AddCount + " Remove: " + RemoveCount + " AddListCount: " + AddListCount + " AddListEdges: " + AddListEdges + " AddListCount: " + RemoveListCount + " RemoveListEdges: " + RemoveListEdges);
        }

        public static void EmitDB()
        {
            string d = " \t ";

            List<string> sl = new List<string>();

            foreach (VertexEntry e in list)
            {
                string s = e.FromStore;

                if (!sl.Contains(s))
                    sl.Add(s);
            }

            using (StreamWriter outputFile = new StreamWriter("DebugDB.txt"))
            {
                EmitAgregated(outputFile, null);

                foreach(string s in sl)
                    EmitAgregated(outputFile, s);

                outputFile.WriteLine("");
                outputFile.WriteLine("");

                foreach (VertexEntry e in list)
                    outputFile.WriteLine(e.FromStore + d + e.Type + d + e.FromIdentifier + d 
                        +'\"' + e.FromValue + '\"' 
                        + d + "M[" + e.MetaIdentifier + "," 
                        + d + '\"' + e.MetaValue + '\"' + "]"
                        + d + "T[" + e.ToIdentifier +"," 
                        + d + '\"'+ e.ToValue + '\"'+ "]" 
                        + d + e.EdgesCount);
            }
        }
    }
}
