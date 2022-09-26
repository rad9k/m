using m0.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.Graph.ExecutionFlow
{
    public enum AtomGraphChangeTypeEnum { ValueChange, EdgeAdded, EdgeRemoved, OutputEdgeDisposed};

    public enum EdgeDirectionEnum { In, Out, Meta};

    public class GraphChangeTransactionAtom : TransacionAtom
    {
        static IVertex r = m0.MinusZero.Instance.root;

        static IVertex GraphChangeEvent_Trigger_meta;
        static IVertex GraphChangeEvent_Source_meta;
        static IVertex GraphChangeEvent_ChangedVertex_meta;
        public static IVertex GraphChangeEvent_Type_meta;
        static IVertex GraphChangeEvent_OldValue_meta;
        static IVertex GraphChangeEvent_NewValue_meta;
        public static IVertex GraphChangeEvent_Edge_meta;

        static IVertex GraphChangeEnum_ValueChange_meta;

        static IVertex GraphChangeEnum_OutputEdgeAdded_meta;
        static IVertex GraphChangeEnum_OutputEdgeRemoved_meta;

        static IVertex GraphChangeEnum_InputEdgeAdded_meta;
        static IVertex GraphChangeEnum_InputEdgeRemoved_meta;

        static IVertex GraphChangeEnum_MetaEdgeAdded_meta;
        public static IVertex GraphChangeEnum_MetaEdgeRemoved_meta;

        static IVertex GraphChangeEnum_OutputEdgeDisposed_meta;

        public IVertex ChangedVertex;
        public AtomGraphChangeTypeEnum Type;
        public object OldValue;
        public object NewValue;
        public IEdge Edge;
        
        public GraphChangeTransactionAtom(
            IVertex _ChangedVertex,
            AtomGraphChangeTypeEnum _Type,
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

        public GraphChangeTransactionAtom(GraphChangeTransactionAtom a)
        {
            ChangedVertex = a.ChangedVertex;
            Type = a.Type;
            OldValue = a.OldValue;
            NewValue = a.NewValue;
            Edge = a.Edge;
        }

        public static void Initialize()
        {
            IVertex r = m0.MinusZero.Instance.root;

            GraphChangeEvent_Trigger_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\Trigger");
            GraphChangeEvent_Source_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\Source");
            GraphChangeEvent_ChangedVertex_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\ChangedVertex");
            GraphChangeEvent_Type_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\Type");
            GraphChangeEvent_OldValue_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\OldValue");
            GraphChangeEvent_NewValue_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\NewValue");
            GraphChangeEvent_Edge_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEvent\Edge");


            GraphChangeEnum_ValueChange_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\ValueChange");

            GraphChangeEnum_OutputEdgeAdded_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\OutputEdgeAdded");
            GraphChangeEnum_OutputEdgeRemoved_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\OutputEdgeRemoved");

            GraphChangeEnum_InputEdgeAdded_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\InputEdgeAdded");
            GraphChangeEnum_InputEdgeRemoved_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\InputEdgeRemoved");

            GraphChangeEnum_MetaEdgeAdded_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\MetaEdgeAdded");
            GraphChangeEnum_MetaEdgeRemoved_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\MetaEdgeRemoved");

            GraphChangeEnum_OutputEdgeDisposed_meta = r.Get(false, @"System\Meta\ZeroTypes\ExecutionFlow\GraphChangeEnum\OutputEdgeDisposed");            
        }

        public override void Commit()
        {
            
        }

        public override void Rollback()
        {
            switch (Type)
            {
                case AtomGraphChangeTypeEnum.EdgeAdded:
                    Rollback_EdgeAdded();
                    break;

                case AtomGraphChangeTypeEnum.EdgeRemoved:
                    Rollback_EdgeRemoved();
                    break;

                case AtomGraphChangeTypeEnum.ValueChange:
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

        public IVertex CreateEventVertex_GraphChange(IVertex triggerVertex, IVertex sourceVertex, EdgeDirectionEnum edgeDirection)
        {            
            IVertex eventVertex = MinusZero.Instance.CreateTempVertex();

            eventVertex.AddEdge(GraphChangeEvent_Trigger_meta, triggerVertex);
            eventVertex.AddEdge(GraphChangeEvent_Source_meta, sourceVertex);

            switch (Type)
            {
                case AtomGraphChangeTypeEnum.ValueChange:
                    eventVertex.AddEdge(GraphChangeEvent_ChangedVertex_meta, ChangedVertex);
                    eventVertex.AddEdge(GraphChangeEvent_Type_meta, GraphChangeEnum_ValueChange_meta);
                    eventVertex.AddVertex(GraphChangeEvent_OldValue_meta, OldValue);
                    eventVertex.AddVertex(GraphChangeEvent_NewValue_meta, NewValue);
                    break;

                case AtomGraphChangeTypeEnum.EdgeAdded:
                    IVertex edgeVertex = ZeroTypes.EdgeHelper.CreateTempEdgeVertex(Edge);

                    switch (edgeDirection)
                    {
                        case EdgeDirectionEnum.In:
                            eventVertex.AddEdge(GraphChangeEvent_ChangedVertex_meta, Edge.To);
                            eventVertex.AddEdge(GraphChangeEvent_Type_meta, GraphChangeEnum_InputEdgeAdded_meta);
                            eventVertex.AddEdge(GraphChangeEvent_Edge_meta, edgeVertex);
                            break;

                        case EdgeDirectionEnum.Out:
                            eventVertex.AddEdge(GraphChangeEvent_ChangedVertex_meta, ChangedVertex);
                            eventVertex.AddEdge(GraphChangeEvent_Type_meta, GraphChangeEnum_OutputEdgeAdded_meta);
                            eventVertex.AddEdge(GraphChangeEvent_Edge_meta, edgeVertex);
                            break;

                        case EdgeDirectionEnum.Meta:
                            //eventVertex.AddEdge(GraphChangeEvent_ChangedVertex_meta, Edge.Meta);
                            eventVertex.AddEdge(GraphChangeEvent_Type_meta, GraphChangeEnum_MetaEdgeAdded_meta);
                            eventVertex.AddEdge(GraphChangeEvent_Edge_meta, edgeVertex);
                            break;
                    }
                    break;

                case AtomGraphChangeTypeEnum.EdgeRemoved:
                    IVertex edgeVertex2 = ZeroTypes.EdgeHelper.CreateTempEdgeVertex(Edge);

                    switch (edgeDirection)
                    {
                        case EdgeDirectionEnum.In:
                            eventVertex.AddEdge(GraphChangeEvent_ChangedVertex_meta, Edge.To);
                            eventVertex.AddEdge(GraphChangeEvent_Type_meta, GraphChangeEnum_InputEdgeRemoved_meta);
                            eventVertex.AddEdge(GraphChangeEvent_Edge_meta, edgeVertex2);
                            break;

                        case EdgeDirectionEnum.Out:
                            eventVertex.AddEdge(GraphChangeEvent_ChangedVertex_meta, ChangedVertex);
                            eventVertex.AddEdge(GraphChangeEvent_Type_meta, GraphChangeEnum_OutputEdgeRemoved_meta);
                            eventVertex.AddEdge(GraphChangeEvent_Edge_meta, edgeVertex2);
                            break;

                        case EdgeDirectionEnum.Meta:
                            //eventVertex.AddEdge(GraphChangeEvent_ChangedVertex_meta, Edge.Meta);
                            eventVertex.AddEdge(GraphChangeEvent_Type_meta, GraphChangeEnum_MetaEdgeRemoved_meta);
                            eventVertex.AddEdge(GraphChangeEvent_Edge_meta, edgeVertex2);
                            break;
                    }                    
                    break;

                case AtomGraphChangeTypeEnum.OutputEdgeDisposed:
                    IVertex edgeVertex3 = ZeroTypes.EdgeHelper.CreateTempEdgeVertex(Edge.From, Edge.Meta, null);
                    
                    eventVertex.AddEdge(GraphChangeEvent_ChangedVertex_meta, ChangedVertex);
                    eventVertex.AddEdge(GraphChangeEvent_Type_meta, GraphChangeEnum_OutputEdgeDisposed_meta);
                    eventVertex.AddEdge(GraphChangeEvent_Edge_meta, edgeVertex3);                    

                    break;
            }

            return eventVertex;
        }
    }
}
