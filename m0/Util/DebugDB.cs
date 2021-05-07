using m0.Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Util
{
    public enum EntryType { Add, AddList, Remove, RemoveList}

    public class DebugEntry
    {
        public string FromStore;
        public EntryType Type;
        public string FromIdentifier;
        public string FromValue;
        public string MetaIdentifier;
        public string MetaValue;
        public string ToIdentifier;
        public string ToValue;
        public int EdgesCount;
    }

    public class DebugDB
    {
        public static List<DebugEntry> list = new List<DebugEntry>();

        public static void Add(EntryType type, IVertex fromVertex, IEdge edge)
        {
            Add(type, fromVertex, edge, 0);
        }

        public static void Add(EntryType type, IVertex fromVertex, IEdge edge, int count)
        {
            DebugEntry e = new DebugEntry();

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

            if (type == EntryType.Add || type == EntryType.Remove)
                e.EdgesCount = fromVertex.OutEdgesRaw.Count();
            else
                e.EdgesCount = count;

            list.Add(e);
        }

        public static void EmitDB()
        {
            string d = " /t ";

            using (StreamWriter outputFile = new StreamWriter("DebugDB.txt"))
            {
                foreach (DebugEntry e in list)
                    outputFile.WriteLine(e.FromStore + d + e.Type + d + e.FromIdentifier + d + e.FromValue + d + e.MetaIdentifier + d + e.MetaIdentifier + d + e.ToIdentifier + d + e.ToValue + d + e.EdgesCount);
            }
        }
    }
}
