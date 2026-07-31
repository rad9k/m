using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.ZeroUML;
using m0.ZeroTypes;
using m0.Graph;
using m0.Util;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using m0.UIWpf.Controls;
using m0.UIWpf.Foundation;
using m0.UIWpf.Commands;
using m0.Graph.ExecutionFlow;
using m0.User.Process.UX;
using System.Windows.Controls.Primitives;
using m0.UIWpf.Visualisers;
using m0.UIWpf.Visualisers.Helper;
using static m0.Graph.ExecutionFlow.ExecutionFlowHelper;

namespace m0.UIWpf.Visualisers.Helper
{
    public class ListVisualiserHelper : AtomVisualiserHelper
    {
        public bool BaseEdgeToEventTriggeringUpdateVertex = true;

        IListVisualiser listVisualiser;

        public ListVisualiserHelper(
            IVertex parentVisualiser,
            bool isVolatile,
            IVertex _visualiserMetaVertex,
            IVisualiser _visualiser,
            string _visualiserName,
            FrameworkElement _visualiserAsFrameworkElement,
            IVertex baseEdgeVertex)
            : this(parentVisualiser,
                  isVolatile,
                  _visualiserMetaVertex,
                  _visualiser,
                  _visualiserName,
                  _visualiserAsFrameworkElement,
                  true,
                  new List<string> { @"", @"BaseEdge:\", @"BaseEdge:\To:\" },
                  "ListVisualiser",
                  baseEdgeVertex,
                  UpdateBaseEdgeCallSchemeEnum.OmmitFirst,
                  false)
        {

        }

        public ListVisualiserHelper(
            IVertex parentVisualiser,
            bool isVolatile,
            IVertex visualiserMetaVertex,
            IVisualiser _visualiser,
            string _visualiserName,
            FrameworkElement _visualiserAsFrameworkElement,
            bool _dndSupport,
            IList<string> _scopeQueries,
            string _scopeQueriesName,
            IVertex baseEdgeVertex,
            UpdateBaseEdgeCallSchemeEnum _updateBaseEdgeCallScheme            
            )
            :this(
                  parentVisualiser,
                  isVolatile,
                  visualiserMetaVertex,
                  _visualiser,
                  _visualiserName,
                  _visualiserAsFrameworkElement,
                  _dndSupport,
                  _scopeQueries,
                  _scopeQueriesName,
                  baseEdgeVertex,
                  _updateBaseEdgeCallScheme,
                  false)
        {
        }

            public ListVisualiserHelper(
                IVertex parentVisualiser,
                bool isVolatile,
                IVertex visualiserMetaVertex,
                IVisualiser _visualiser,
                string _visualiserName,
                FrameworkElement _visualiserAsFrameworkElement,
                bool _dndSupport,
                IList<string> _scopeQueries,
                string _scopeQueriesName,
                IVertex baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum _updateBaseEdgeCallScheme,
                bool _visualiserAsBaseEdge
            )
            : base(parentVisualiser,
                  isVolatile,
                  visualiserMetaVertex,
                  _visualiser,
                  _visualiserName,
                  _visualiserAsFrameworkElement,
                  _dndSupport,
                  _scopeQueries,
                  _scopeQueriesName,
                  baseEdgeVertex,
                  _updateBaseEdgeCallScheme,                  
                  _visualiserAsBaseEdge)
        {
            listVisualiser = (IListVisualiser)_visualiser;
        }

        public event CustomVertexChangeHandler CustomVertexChangeEvent;

        bool firstVertexChangeExecuted = false;

        public ISet<IVertex> GetSelectedVertexes()
        {
            ISet<IVertex> selectedVertexes = new HashSet<IVertex>();

            foreach (IEdge e in Vertex.GetAll(false, @"SelectedEdges:\\To:"))
                selectedVertexes.Add(e.To);

            return selectedVertexes;
        }

        protected override INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            if (ForceVertexChangeOff)
            {
                if (Visualiser is TreeVisualiser)
                    MinusZero.Instance.Log(1, "TreeVisualiser.RootRefresh.ListHelper.VertexChange",
                        "skip=ForceVertexChangeOff " + TreeVisualiser.FormatEventStackForLog(exe));
                return exe.Stack;
            }

            if (!firstVertexChangeExecuted && updateBaseEdgeCallSchema == UpdateBaseEdgeCallSchemeEnum.OmmitSecond)
            {
                firstVertexChangeExecuted = true;
                if (Visualiser is TreeVisualiser)
                    MinusZero.Instance.Log(1, "TreeVisualiser.RootRefresh.ListHelper.VertexChange",
                        "skip=OmmitSecondFirstCall " + TreeVisualiser.FormatEventStackForLog(exe));
                return exe.Stack;
            }

            if (CustomVertexChangeEvent != null)
            {
                if (Visualiser is TreeVisualiser)
                    MinusZero.Instance.Log(1, "TreeVisualiser.RootRefresh.ListHelper.VertexChange",
                        "dispatch=CustomVertexChangeEvent " + TreeVisualiser.FormatEventStackForLog(exe));
                return CustomVertexChangeEvent(exe);
            }

            return VertexChangeLogic(exe);
        }

        public INoInEdgeInOutVertexVertex VertexChangeLogic(IExecution exe)
        {
            bool isTreeVisualiser = Visualiser is TreeVisualiser;

            bool scaleChanged = IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Visualiser.Vertex, "Scale");
            bool selectedEdgesChanged =
                IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Visualiser.Vertex, "SelectedEdges")
                || IsEdgeAddedRemovedDiscardedFrom(exe.Stack, Vertex.Get(false, @"SelectedEdges:"));
            bool baseEdgeOrToChanged = BaseEdgeToEventTriggeringUpdateVertex && (
                IsVertexChageOrEdgeAddedRemovedDisposedFromTo(exe.Stack, Vertex.Get(false, @"BaseEdge:"))
                || IsVertexChageOrEdgeAddedRemovedDisposedFromTo(exe.Stack, Vertex.Get(false, @"BaseEdge:\To:")));

            if (isTreeVisualiser)
                MinusZero.Instance.Log(1, "TreeVisualiser.RootRefresh.ListHelper.VertexChangeLogic",
                    "scaleChanged=" + scaleChanged
                    + " selectedEdgesChanged=" + selectedEdgesChanged
                    + " baseEdgeOrToChanged=" + baseEdgeOrToChanged
                    + " BaseEdgeToEventTriggeringUpdateVertex=" + BaseEdgeToEventTriggeringUpdateVertex
                    + " " + TreeVisualiser.FormatEventStackForLog(exe));

            if (scaleChanged)
                listVisualiser.ScaleChange();

            if (selectedEdgesChanged)
                listVisualiser.SelectedVerticesUpdated();

            if (baseEdgeOrToChanged)
                listVisualiser.BaseEdgeToUpdated();
            else
            {
                bool needToUpdateBaseEdge = false;

                foreach (string meta in listVisualiser.MetaTriggeringUpdateVertex)
                    if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Visualiser.Vertex, meta))
                        needToUpdateBaseEdge = true;

                if (needToUpdateBaseEdge)
                {
                    if (isTreeVisualiser)
                        MinusZero.Instance.Log(1, "TreeVisualiser.RootRefresh.ListHelper.VertexChangeLogic",
                            "needToUpdateBaseEdge=true via MetaTriggeringUpdateVertex");
                    listVisualiser.BaseEdgeToUpdated();
                }

                bool needToUpdateView = false;

                foreach (string meta in listVisualiser.MetaTriggeringUpdateView)
                    if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Visualiser.Vertex, meta))
                        needToUpdateView = true;

                if (needToUpdateView)
                    listVisualiser.ViewAttributesUpdated();
            }

            return exe.Stack;
        }

        // DRAG AND DROP

        protected override void dndPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(VisualiserAsFrameworkElement);
            Vector diff = dndStartPoint - mousePos;

            var headersPresenter = m0.UIWpf.WpfUtil.FindVisualChild<DataGridColumnHeadersPresenter>(VisualiserAsFrameworkElement);

            if (headersPresenter != null)
            {
                double headerActualHeight = headersPresenter.ActualHeight;

                if (mousePos.Y <= headerActualHeight) // if header
                {
                    e.Handled = false;
                    return;
                }
            }

            if (hasButtonBeenDown && isDraggin == false &&
                !WpfUtil.IsMouseOverScrollbar(sender, dndStartPoint) &&
                (e.LeftButton == MouseButtonState.Pressed) && (
                (Math.Abs(diff.X) > Dnd.MinimumHorizontalDragDistance) ||
                (Math.Abs(diff.Y) > Dnd.MinimumVerticalDragDistance)))
            {
                isDraggin = true;

                TreeVisualiser treeVisualiser = Visualiser as TreeVisualiser;

                IVertex dndVertex = treeVisualiser != null
                    ? treeVisualiser.TryPrepareDragAndBuildDndVertex(dndStartPoint)
                    : BuildLegacyDndVertex();

                if (dndVertex != null && dndVertex.Count() > 0)
                {
                    if (treeVisualiser != null)
                        MinusZero.Instance.Log(1, "TreeVisualiser.RootRefresh.ListHelper.dndPreviewMouseMove",
                            "DoDragDrop payloadCount=" + dndVertex.Count()
                            + " rootBase=" + TreeVisualiser.FormatVertexForLog(
                                Vertex == null ? null : Vertex.Get(false, @"BaseEdge:\To:")));

                    dndVertex.AddExternalReference();

                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", VisualiserAsFrameworkElement);

                    Dnd.DoDragDrop(VisualiserAsFrameworkElement, dragData);

                    e.Handled = true;
                }

                isDraggin = false;
                hasButtonBeenDown = false;
            }
        }

        private IVertex BuildLegacyDndVertex()
        {
            IVertex dndVertex = MinusZero.Instance.CreateTempVertex();
            IVertex selectedEdgeVertices = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            if (selectedEdgeVertices != null && selectedEdgeVertices.Count() > 0)
            {
                foreach (IEdge selectedEdgeVertexEdge in selectedEdgeVertices)
                    dndVertex.AddEdge(null, selectedEdgeVertexEdge.To);

                return dndVertex;
            }

            IVertex edgeByPoint = Visualiser.GetEdgeByPoint(dndStartPoint);

            if (edgeByPoint != null)
                dndVertex.AddEdge(null, edgeByPoint);

            return dndVertex;
        }

        // Should be corrected as uncommeted makes dnd from tree to UXContainer not working
        // QQQ below two methods were commented out and now 2026.02.17 theyy are working and we are checking what is going on
        protected override void dndDrop(object sender, DragEventArgs e)
        {
            Point dropPoint = e.GetPosition(VisualiserAsFrameworkElement);

            IVertex v = Visualiser.GetEdgeByPoint(dropPoint);

            if (v == null && GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"Home:\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "OnlyEnd"))
                v = Vertex.Get(false, "BaseEdge:");

            bool isTreeVisualiser = Visualiser is TreeVisualiser;
            IVertex dropTargetTo = v == null ? null : v.Get(false, "To:");

            if (isTreeVisualiser)
                MinusZero.Instance.Log(1, "TreeVisualiser.RootRefresh.ListHelper.dndDrop",
                    "dropPoint=" + dropPoint
                    + " edgeByPointNull=" + (v == null)
                    + " dropTargetTo=" + TreeVisualiser.FormatVertexForLog(dropTargetTo)
                    + " rootBase=" + TreeVisualiser.FormatVertexForLog(
                        Vertex == null ? null : Vertex.Get(false, @"BaseEdge:\To:"))
                    + " dropOntoRootBase=" + (dropTargetTo != null
                        && Vertex != null
                        && dropTargetTo == Vertex.Get(false, @"BaseEdge:\To:")));

            if (v != null)
                Dnd.DoDrop(null, dropTargetTo, e);

            e.Handled = true;
        }

        protected override void dndMouseEnter(object sender, MouseEventArgs e)
        {
            base.dndMouseEnter(sender, e);
        }
    }
}
