using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using m0.Foundation;
using m0.Graph;
using m0.Graph.Internal;
using System.IO;

namespace m0.Store.FileSystem
{
    public class FileContentVertex
        : EasyVertex,
        IExplicitQueryValueVertex
    {
        string fileName;

        bool IExplicitQueryValueVertex
            .RequiresExplicitQueryValueEvaluation =>
                fileName != null;

        public override object Value
        {
            get
            {
                if (fileName == null)
                {
                    // this is a "normal" vertex, not identified by file name
                    return base.Value;
                }

                try
                {
                    string content =
                        System.IO.File.ReadAllText(
                            fileName);
                    return content;
                }
                catch (Exception e) { }
                return "";
            }
            set
            {
                if (fileName == null)
                {
                    // this is a "normal" vertex, not identified by file name
                    base.Value = value;
                    return;
                }

                throw new NotSupportedException(
                    "File-backed content vertices are read-only.");
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

