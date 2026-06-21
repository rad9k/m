using System.Collections.Generic;
using System.Linq;
using m0.Foundation;
using m0.Graph;
using m0.Util;
using m0.User.Process.UX;
using m0.ZeroTypes;

namespace m0.UIWpf.Visualisers.Helper
{
    public enum SelectedEdgeMatchMode
    {
        /// <summary>Match by From + Meta + To (Tree, List, Icon, Form, InEdgesList, ...).</summary>
        FullIEdge,

        /// <summary>Match by To vertex only (GraphVisualiser, GraphVisualiser3D).</summary>
        ToVertexOnly
    }

    public struct PendingEdgeMouseGesture
    {
        public IEdge ClickedEdge;
        public bool IsCtrl;
        public bool WasInSelectionAtMouseDown;
    }

    /// <summary>Graph / Graph3D: selection identity is the displayed To vertex only.</summary>
    public struct PendingToVertexMouseGesture
    {
        public IVertex ClickedToVertex;
        public bool IsCtrl;
        public bool WasInSelectionAtMouseDown;
    }

    public static class SelectedEdgesInteractionHelper
    {
        public static bool IsEdgeInSelectedEdges(
            IVertex visualiserVertex,
            IEdge edge,
            SelectedEdgeMatchMode matchMode)
        {
            if (visualiserVertex == null || edge == null)
                return false;

            IVertex selectedEdges = visualiserVertex.Get(false, @"SelectedEdges:");

            if (selectedEdges == null)
                return false;

            if (matchMode == SelectedEdgeMatchMode.ToVertexOnly)
                return EdgeHelper.FindEdgeVertexByToVertex(selectedEdges, edge.To) != null;

            return EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, edge) != null;
        }

        public static void RemoveEdgeFromSelectedEdges(
            IVertex selectedEdges,
            IEdge edge,
            SelectedEdgeMatchMode matchMode)
        {
            if (selectedEdges == null || edge == null)
                return;

            if (matchMode == SelectedEdgeMatchMode.ToVertexOnly)
                EdgeHelper.DeleteVertexByEdgeTo(selectedEdges, edge.To);
            else
                EdgeHelper.DeleteVertexByEdge(selectedEdges, edge);
        }

        public static bool WasEdgeInSelectedEdges(IVertex visualiserVertex, IEdge edge)
        {
            return IsEdgeInSelectedEdges(visualiserVertex, edge, SelectedEdgeMatchMode.FullIEdge);
        }

        public static bool WasToVertexInSelectedEdges(IVertex visualiserVertex, IVertex toVertex)
        {
            if (visualiserVertex == null || toVertex == null)
                return false;

            IVertex selectedEdges = visualiserVertex.Get(false, @"SelectedEdges:");

            return IsToVertexInSelectedEdges(selectedEdges, toVertex);
        }

        private static bool IsToVertexInSelectedEdges(IVertex selectedEdges, IVertex toVertex)
        {
            if (selectedEdges == null || toVertex == null)
                return false;

            return EdgeHelper.FindEdgeVertexByToVertex(selectedEdges, toVertex) != null;
        }

        public static void ApplyForClickByToVertex(IVertex visualiserVertex, PendingToVertexMouseGesture pending)
        {
            if (visualiserVertex == null || pending.ClickedToVertex == null)
                return;

            IVertex selectedEdges = visualiserVertex.Get(false, @"SelectedEdges:");

            if (selectedEdges == null)
                return;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            if (pending.IsCtrl)
            {
                if (pending.WasInSelectionAtMouseDown)
                    EdgeHelper.DeleteVertexByEdgeTo(selectedEdges, pending.ClickedToVertex);
                else
                    EdgeHelper.AddEdgeVertexByToVertex(selectedEdges, pending.ClickedToVertex);
            }
            else
            {
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);
                EdgeHelper.AddEdgeVertexByToVertex(selectedEdges, pending.ClickedToVertex);
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static void ApplyForDragByToVertex(IVertex visualiserVertex, PendingToVertexMouseGesture pending)
        {
            if (visualiserVertex == null || pending.ClickedToVertex == null)
                return;

            IVertex selectedEdges = visualiserVertex.Get(false, @"SelectedEdges:");

            if (selectedEdges == null)
                return;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            if (pending.IsCtrl)
            {
                if (!IsToVertexInSelectedEdges(selectedEdges, pending.ClickedToVertex))
                    EdgeHelper.AddEdgeVertexByToVertex(selectedEdges, pending.ClickedToVertex);
            }
            else if (!pending.WasInSelectionAtMouseDown)
            {
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);
                EdgeHelper.AddEdgeVertexByToVertex(selectedEdges, pending.ClickedToVertex);
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        /// <summary>
        /// Context menu (RMB down): keep SelectedEdges when clicking a selected To vertex; otherwise select only clicked vertex.
        /// </summary>
        public static void ApplyForContextMenuByToVertex(IVertex visualiserVertex, IVertex clickedToVertex)
        {
            if (visualiserVertex == null || clickedToVertex == null)
                return;

            if (WasToVertexInSelectedEdges(visualiserVertex, clickedToVertex))
                return;

            IVertex selectedEdges = visualiserVertex.Get(false, @"SelectedEdges:");

            if (selectedEdges == null)
                return;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);
            EdgeHelper.AddEdgeVertexByToVertex(selectedEdges, clickedToVertex);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static void ApplyForClick(IVertex visualiserVertex, PendingEdgeMouseGesture pending)
        {
            if (visualiserVertex == null || pending.ClickedEdge == null)
                return;

            IVertex selectedEdges = visualiserVertex.Get(false, @"SelectedEdges:");

            if (selectedEdges == null)
                return;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            if (pending.IsCtrl)
            {
                if (pending.WasInSelectionAtMouseDown)
                {
                    IEdge selectedEdgeVertexEdge = EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, pending.ClickedEdge);

                    if (selectedEdgeVertexEdge != null)
                        selectedEdges.DeleteEdge(selectedEdgeVertexEdge);
                }
                else
                    EdgeHelper.AddEdgeVertex(selectedEdges, pending.ClickedEdge);
            }
            else
            {
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);
                EdgeHelper.AddEdgeVertex(selectedEdges, pending.ClickedEdge);
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static void ApplyForDrag(IVertex visualiserVertex, PendingEdgeMouseGesture pending)
        {
            if (visualiserVertex == null || pending.ClickedEdge == null)
                return;

            IVertex selectedEdges = visualiserVertex.Get(false, @"SelectedEdges:");

            if (selectedEdges == null)
                return;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            if (pending.IsCtrl)
            {
                if (EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, pending.ClickedEdge) == null)
                    EdgeHelper.AddEdgeVertex(selectedEdges, pending.ClickedEdge);
            }
            else if (!pending.WasInSelectionAtMouseDown)
            {
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);
                EdgeHelper.AddEdgeVertex(selectedEdges, pending.ClickedEdge);
            }

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        /// <summary>
        /// Context menu (RMB down): keep SelectedEdges when clicking a selected edge; otherwise select only clicked edge.
        /// </summary>
        public static void ApplyForContextMenu(IVertex visualiserVertex, IEdge clickedEdge)
        {
            if (visualiserVertex == null || clickedEdge == null)
                return;

            if (WasEdgeInSelectedEdges(visualiserVertex, clickedEdge))
                return;

            IVertex selectedEdges = visualiserVertex.Get(false, @"SelectedEdges:");

            if (selectedEdges == null)
                return;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);
            EdgeHelper.AddEdgeVertex(selectedEdges, clickedEdge);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        public static IVertex BuildDndVertexFromSelectedEdges(IVertex visualiserVertex, IVertex fallbackEdgeVertex)
        {
            IVertex dndVertex = MinusZero.Instance.CreateTempVertex();
            IVertex selectedEdgeVertices = visualiserVertex == null
                ? null
                : visualiserVertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            if (selectedEdgeVertices != null && selectedEdgeVertices.Count() > 0)
            {
                foreach (IEdge selectedEdgeVertexEdge in selectedEdgeVertices)
                    dndVertex.AddEdge(null, selectedEdgeVertexEdge.To);

                return dndVertex;
            }

            if (fallbackEdgeVertex != null)
                dndVertex.AddEdge(null, fallbackEdgeVertex);

            return dndVertex;
        }
    }
}
