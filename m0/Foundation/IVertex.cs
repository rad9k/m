using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Foundation
{
    public delegate void VertexChange(object sender, VertexChangeEventArgs args);

    public enum VertexChangeType { ValueChanged, EdgeAdded, EdgeRemoved, InEdgeAdded, InEdgeRemoved }

    public class VertexChangeEventArgs:EventArgs
    {
        public VertexChangeType Type;

        public IEdge Edge;

        public VertexChangeEventArgs(VertexChangeType _Type, IEdge _Edge){
            Type=_Type;
            Edge=_Edge;
        }
    }


    // IVertex has to have constructor with:
    // - IStore param
    public interface IVertex : IEnumerable<IEdge>, IDisposable
    {
        event VertexChange Change;

        Delegate[] GetChangeDelegateInvocationList();

        object Identifier { get; }

        object Value {get; set;}

        IList<IEdge> OutEdgesRaw { get;} // without $Inherits

        IList<IEdge> OutEdges { get; }        

        IList<IEdge> InEdgesRaw { get; } // without $Inherits

        IList<IEdge> InEdges { get; }        

        IList<IEdge> MetaInEdgesRaw { get; } // without $Inherits


        INoInEdgeInOutVertexVertex Execute(IExecution exe);

        // 2.0 BEG

        bool InEdgesDictionariesNeedsRebuild { get; set; }

        bool OutEdgesDictionariesNeedsRebuild { get; set; }


        void QueryOutEdges(object meta, object to, out IEdge result, out IList<IEdge> results);

        void QueryInEdges(object meta, object from, out IEdge result, out IList<IEdge> results);

        // 2.0 END

        IVertex AddVertex(IVertex metaVertex, object val);

        IEdge AddVertexAndReturnEdge(IVertex metaVertex, object val);

        void AddMetaInEdge(IEdge edge);

        void DeleteMetaInEdge(IEdge edge);

        void AddInEdge(IEdge edge);

        void DeleteInEdge(IEdge edge);

        void DeleteInEdgeOnlyIn(IEdge edge);

        IEdge AddEdge(IVertex metaVertex, IVertex destVertex);

        void AddEdgesList(IEnumerable<IEdge> edges);

        void DeleteEdge(IEdge edge);

        void DeleteEdgeOnlyOut(IEdge edge);

        void DeleteEdgesList(IEnumerable<IEdge> edges);

        IVertex Get(bool metaMode, string query);

        IVertex GetAll(bool metaMode, string query);

        IVertex Get(bool metaMode, IVertex expression);

        IVertex GetAll(bool metaMode, IVertex expression);


        IEdge this[string meta] { get; } // for databinding
                        
        IStore Store { get; }

        IList<AccessLevelEnum> AccessLevel { get; }
    }
}
