using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Util;
using System;
using System.Collections.Generic;

namespace m0.UIWpf.Commands
{
    public class SellectedSelectedSynchronisedHelper
    {
        IVertex masterVisualiserVertex;
        IVertex detailVisualiserVertex;

        IEdge selectedEdgesVertex_Listener;
        IEdge detailVisuliserVertex_Listener;

        public SellectedSelectedSynchronisedHelper(IVertex masterVisualiserVertex, IVertex detailVisualiserVertex)
        {
            this.masterVisualiserVertex = masterVisualiserVertex;
            this.detailVisualiserVertex = detailVisualiserVertex;

            IVertex selectedEdgesVertex = GraphUtil.GetQueryOutFirst(masterVisualiserVertex, "SelectedEdges", null);

            AddGraphChangeTrigger(selectedEdgesVertex);
        }

        void AddGraphChangeTrigger(IVertex selectedEdgesVertex)
        {
            selectedEdgesVertex_Listener = ExecutionFlowHelper.AddTriggerAndListener(selectedEdgesVertex, synchroniseMasterVisualiser_VertexChange);

            IEdge detailVisualiserVertexIncomingEdge = detailVisualiserVertex.InEdges[0];

            detailVisuliserVertex_Listener = ExecutionFlowHelper.AddTriggerAndListener(detailVisualiserVertexIncomingEdge.From,
                new List<string> { },
                new List<GraphChangeFilterEnum> {
                        GraphChangeFilterEnum.OutputEdgeRemoved,
                        GraphChangeFilterEnum.OutputEdgeDisposed
                },
                "SimpleDirectDisposeTrigger",
                detailVisualiser_Dispose);
        }

        protected INoInEdgeInOutVertexVertex synchroniseMasterVisualiser_VertexChange(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            IVertex selectedEdgesVertex = GraphUtil.GetQueryOutFirst(masterVisualiserVertex, "SelectedEdges", null);

            if (ExecutionFlowHelper.IsEdgeAddedRemovedDiscardedFrom(stack, selectedEdgesVertex))
                Synchronise(selectedEdgesVertex);

            return exe.Stack;
        }

        public void DoSynchronise()
        {
            IVertex selectedEdgesVertex = GraphUtil.GetQueryOutFirst(masterVisualiserVertex, "SelectedEdges", null);

            Synchronise(selectedEdgesVertex);
        }

        void Synchronise(IVertex selectedEdgesVertex)
        {
            if (selectedEdgesVertex != null)
            {
                IVertex firstSelectedEdgeEdgeVertex = GraphUtil.GetQueryOutFirst(selectedEdgesVertex, "Edge", null);

                if (firstSelectedEdgeEdgeVertex != null)
                    GraphUtil.ReplaceEdge(detailVisualiserVertex, "BaseEdge", firstSelectedEdgeEdgeVertex);
            }
        }

        protected INoInEdgeInOutVertexVertex detailVisualiser_Dispose(IExecution exe)
        {
            ExecutionFlowHelper.RemoveGraphChangeListener(selectedEdgesVertex_Listener);

            return exe.Stack;
        }

    }
}
