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
        static IVertex r = m0.MinusZero.Instance.root;

        static IVertex GraphChangeEvent_Trigger_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\Trigger");
        static IVertex GraphChangeEvent_Source_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\Source");
        static IVertex GraphChangeEvent_ChangedVertex_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\ChangedVertex");
        static IVertex GraphChangeEvent_Type_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\Type");
        static IVertex GraphChangeEvent_OldValue_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\OldValue");
        static IVertex GraphChangeEvent_NewValue_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\NewValue");
        static IVertex GraphChangeEvent_Edge_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\Edge");

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

        public override IVertex CreateEventVertex(IVertex triggerVertex, IVertex sourceVertex)
        {
            throw new NotImplementedException();
        }

        public IVertex CreateEventVertex_GraphChange(IVertex triggerVertex, IVertex sourceVertex, bool isInEdge)
        {
            IVertex eventVertex = MinusZero.Instance.CreateTempVertex();

            eventVertex.AddEdge(GraphChangeEvent_Trigger_meta, triggerVertex);
            eventVertex.AddEdge(GraphChangeEvent_Source_meta, sourceVertex);

            switch (Type)
            {
                case GraphChangeEnum.EdgeAdded:
                    break;
            }

            return eventVertex;
        }
    }
}
