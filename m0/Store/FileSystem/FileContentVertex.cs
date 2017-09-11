using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Graph;
using System.IO;

namespace m0.Store.FileSystem
{
    public class FileContentVertex : EasyVertex
    {
        public override object Value
        {
            get
            {
                try
                {
                    return System.IO.File.ReadAllText(Identifier);
                }
                catch (Exception e) { }
                return "";
            }
            set
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(Identifier);
                file.WriteLine(value);

                file.Close();
            }
        }

        public override void AddInEdge(IEdge edge)
        {

        }

        public override void DeleteInEdge(IEdge edge)
        {

        }

        

        public FileContentVertex(string identifier, IStore store)
            : base(store)
        {
            _Identifier = identifier;

            UsageCounter++; // identified vertex are used for volatile stores            
        }
    }
}

