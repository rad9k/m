using m0.Foundation;
using m0.Graph;
using m0.Graph.Internal;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Store.FileSystem
{
    [Serializable]
    public class AbstractFileSystemVertex : EasyVertex
    {
        protected EasyVertex FileSystemVertex;

        bool FileSystemVertexFilled = false;

        protected override IVertex CreateVertexInstance()
        {
            return MinusZero.Instance.CreateTempVertex();
        }

        public override IList<IEdge> OutEdges
        {
            get
            {
                if (!FileSystemVertexFilled)
                {
                    UpdateFileSystemVertex();
                    FileSystemVertexFilled = true;
                }

                if (OutEdgesDictionariesNeedsRebuild_Edges)
                {
                    OutEdgesDictionariesRebuild_Edges();
                    return _OutEdges;
                }
                else
                    return _OutEdges;
            }
        }

        protected virtual void UpdateFileSystemVertex() { }

        public override IVertex AddVertex(IVertex metaVertex, object val)
        {
            return AddVertexAndReturnEdge(metaVertex, val).To;
        }

        protected void AddVertexToFileSystemVertex(IVertex metaVertex, string value)
        {
            FileSystemVertex.AddVertex(metaVertex, value);
        }

        protected void AddVertexToFileSystemVertex(IVertex metaVertex, IVertex vertex)
        {
            FileSystemVertex.AddEdge(metaVertex, vertex);
        }

        protected override void OutEdgesDictionariesRebuild_Edges()
        {
            if (HasInheritance && AllowInheritance)
            {
                List<IEdge> FullEdges = OutEdgesRaw.ToList();

                HashSet<IVertex> parents = VertexHelper.GetInheritParents(this);

                foreach (IVertex v in parents)
                    FullEdges.AddRange(v.OutEdgesRaw);

                _OutEdges = FullEdges;
            }
            else
                _OutEdges = OutEdgesRaw;

            List<IEdge> FileSystemExtendedOutEdges = new List<IEdge>();

            FileSystemExtendedOutEdges.AddRange(_OutEdges);
            FileSystemExtendedOutEdges.AddRange(FileSystemVertex.OutEdges);

            _OutEdges = FileSystemExtendedOutEdges;

            OutEdgesDictionariesNeedsRebuild_Edges = false;
        }

        public AbstractFileSystemVertex(IStore store, string identifier)
            : base(store, identifier)
        {
            FileSystemVertex = new EasyVertex(MinusZero.Instance.TempStore);
        }
    }
}
