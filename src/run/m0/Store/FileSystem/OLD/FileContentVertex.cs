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
        string fileName;

        object _value;

        public override object Value
        {
            get
            {
                if (fileName == null)
                {
                    // this is a "normal" vertex, not identified by file name
                    return _value;
                }

                try
                {
                    return System.IO.File.ReadAllText(fileName);
                }
                catch (Exception e) { }
                return "";
            }
            set
            {
                if (fileName == null)
                {
                    // this is a "normal" vertex, not identified by file name
                    _value = value;
                    return;
                }

                // for now we do not want this

                //System.IO.StreamWriter file = new System.IO.StreamWriter(Identifier.ToString());
                //file.WriteLine(value);

                //file.Close();
            }
        }

        public FileContentVertex(string _fileName, IStore store)
            : base(store) 
        {            
            fileName = _fileName; // identified vertex are used for volatile stores         
        }

        public FileContentVertex(IStore store)
            : base(store)
        {
            fileName = null; // "normal" Vertex mode   
        }
    }
}

