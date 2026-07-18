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
                    Refresh();
                    FileSystemVertexFilled = true;
                }

                EnsureInheritedLogicalOutEdgesCurrent();

                if (OutEdgesDictionariesNeedsRebuild_Edges)
                {
                    OutEdgesDictionariesRebuild_Edges();
                    return _OutEdges;
                }
                else
                    return _OutEdges;
            }
        }

        public virtual void Refresh() { }

        protected override bool
            CanIncrementallyUpdateLocalOutIndexes(
                IEdge edge)
        {
            return false;
        }

        protected override bool
            CanShareOutEdgesRebuild()
        {
            return false;
        }

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
            HashSet<IVertex> parents = null;
            List<IEdge> fullEdges =
                OutEdgesRaw.ToList();

            if (HasInheritance && AllowInheritance)
            {
                parents = VertexHelper.GetInheritParents(this);

                foreach (IVertex v in parents)
                    GraphUtil.AddRange_NoNoInherit(
                        fullEdges,
                        v.OutEdgesRaw);
            }

            fullEdges.AddRange(
                FileSystemVertex.OutEdges);
            _OutEdges = fullEdges;

            OutEdgesDictionariesNeedsRebuild_Edges = false;
            CompleteLogicalOutEdgesRebuild(parents);
        }

        public AbstractFileSystemVertex(IStore store, string identifier)
            : base(store, identifier)
        {
            FileSystemVertex = new EasyVertex(MinusZero.Instance.TempStore);
        }
    }
}
