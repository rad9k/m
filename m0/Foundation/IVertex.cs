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
    public interface IVertex : IEnumerable<IEdge>
    {
        event VertexChange Change;

        Delegate[] GetChangeDelegateInvocationList();

        string Identifier { get; }

        object Value {get; set;}

        IList<IEdge> OutEdgesRaw { get;}

        IEnumerable<IEdge> InEdges { get; }

        IList<IEdge> InEdgesRaw { get; }

        IEnumerable<IEdge> OutEdges { get; }

        IVertex AddVertex(IVertex metaVertex, object val);

        void AddInEdge(IEdge edge);

        void DeleteInEdge(IEdge edge);
        
        IEdge AddEdge(IVertex metaVertex, IVertex destVertex);

        void DeleteEdge(IEdge edge);


        IVertex Execute(IVertex inputVertex, IVertex expression);


        IVertex Get(string query);

        IVertex GetAll(string query);

        IEdge this[string meta] { get; } // for databinding
                        
        IStore Store { get; }

        IList<AccessLevelEnum> AccessLevel { get; }
    }
}
