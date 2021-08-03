using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    public enum GraphChangeEnum { ValueChange, EdgeAdded, EdgeRemoved };

    public class GraphChangeTransactionAtom : TransacionAtom
    {
        public IVertex ChangedVertex;
        public GraphChangeEnum Type;
        public object OldValue;
        public object NewValue;
        public IEdge Edge;
        
        public GraphChangeTransactionAtom(
            IVertex _ChangedVertex,
            GraphChangeEnum _Type,
            object _OldValue,
            object _NewValue,
            IEdge _Edge)
        {
            ChangedVertex = _ChangedVertex;
            Type = _Type;
            OldValue = _OldValue;
            NewValue = _NewValue;
            Edge = _Edge;
        }

        public override void Commit()
        {
            
        }

        public override void Rollback()
        {
            switch (Type)
            {
                case GraphChangeEnum.EdgeAdded:
                    Rollback_EdgeAdded();
                    break;

                case GraphChangeEnum.EdgeRemoved:
                    Rollback_EdgeRemoved();
                    break;

                case GraphChangeEnum.ValueChange:
                    Rollback_ValueChange();
                    break;
            }
        }

        void Rollback_EdgeAdded()
        {
            Edge.From.DeleteEdge(Edge);
        }

        void Rollback_EdgeRemoved()
        {
            Edge.From.AddEdge(Edge.Meta, Edge.To);
        }
                      
        void Rollback_ValueChange()
        {
            ChangedVertex.Value = OldValue;
        }
    }
}
