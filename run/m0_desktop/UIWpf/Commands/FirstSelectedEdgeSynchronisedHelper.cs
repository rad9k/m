using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m0.UIWpf.Commands
{
    public class FirstSelectedEdgeSynchronisedHelper
    {
        IVertex synchroniseMasterVisualiserVertex;
        IVertex selectSynchronisedVisualiser;

        public FirstSelectedEdgeSynchronisedHelper(IVertex synchroniseMaserVisualiserVertex, IVertex SelectSynchronisedVisualiserVertex)
        {
            this.synchroniseMasterVisualiserVertex = synchroniseMaserVisualiserVertex;
            this.selectSynchronisedVisualiser = SelectSynchronisedVisualiserVertex;

            AddGraphChangeTrigger();
        }

        void AddGraphChangeTrigger()
        {
            ExecutionFlowHelper.AddTriggerAndListener(synchroniseMasterVisualiserVertex, synchroniseMasterVisualiser_VertexChange);
        }

        protected virtual INoInEdgeInOutVertexVertex synchroniseMasterVisualiser_VertexChange(IExecution exe)
        {
            INoInEdgeInOutVertexVertex stack = exe.Stack;

            if (ExecutionFlowHelper.IsEdgeAddedRemovedDiscardedFrom(stack, synchroniseMasterVisualiserVertex))
            {
                //IVertex base
            }

            return exe.Stack;
        }

        public void SynchronisedVisualiserChange(object sender, VertexChangeEventArgs e)
        {
            if (
                ((sender == selectSynchronisedVisualiser) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "SelectedEdges")))
            ||
            (sender is IVertex && GraphUtil.FindEdgeByToVertex(selectSynchronisedVisualiser.GetAll(false, @"SelectedEdges:\"), (IVertex)sender) != null && ((e.Type == VertexChangeType.EdgeAdded) || (e.Type == VertexChangeType.EdgeRemoved)))
             ||
            (sender is IVertex && selectSynchronisedVisualiser.Get(false, @"SelectedEdges:") == (IVertex)sender && ((e.Type == VertexChangeType.EdgeAdded) || (e.Type == VertexChangeType.EdgeRemoved)))
                )
            {
                if (synchroniseMasterVisualiserVertex.Get(false, @"BaseEdge:\To:") == null) // if Disposed
                {
                    //      PlatformClass.RemoveVertexChangeListeners(selectSynchronisedVisualiser, new VertexChange(this.SynchronisedVisualiserChange));
                }
                else
                {
                    IVertex selEdgesFirst = selectSynchronisedVisualiser.Get(false, @"SelectedEdges:\");

                    if (selEdgesFirst != null)
                    {
                        IVertex firstSelectedVertexEdgeTo = selEdgesFirst.Get(false, "To:");

                        if (firstSelectedVertexEdgeTo != null)
                            GraphUtil.ReplaceEdge(synchroniseMasterVisualiserVertex.Get(false, "BaseEdge:"), "To", firstSelectedVertexEdgeTo);

                        IVertex firstSelectedVertexEdgeMeta = selEdgesFirst.Get(false, "Meta:");

                        if (firstSelectedVertexEdgeMeta != null)
                            GraphUtil.ReplaceEdge(synchroniseMasterVisualiserVertex.Get(false, "BaseEdge:"), "Meta", firstSelectedVertexEdgeMeta);
                    }
                }
            }
        }
    }
}
