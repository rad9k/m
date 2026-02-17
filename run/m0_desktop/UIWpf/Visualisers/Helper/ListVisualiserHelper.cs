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
                return exe.Stack;

            if (!firstVertexChangeExecuted && updateBaseEdgeCallSchema == UpdateBaseEdgeCallSchemeEnum.OmmitSecond)
            {
                firstVertexChangeExecuted = true;
                return exe.Stack;
            }

            if (CustomVertexChangeEvent != null)
                return CustomVertexChangeEvent(exe);

            return VertexChangeLogic(exe);
        }

        public INoInEdgeInOutVertexVertex VertexChangeLogic(IExecution exe)
        {            
            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Visualiser.Vertex, "Scale"))
                listVisualiser.ScaleChange();

            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Visualiser.Vertex, "SelectedEdges")
                || IsEdgeAddedRemovedDiscardedFrom(exe.Stack, Vertex.Get(false, @"SelectedEdges:")))
                listVisualiser.SelectedVerticesUpdated();

            if (BaseEdgeToEventTriggeringUpdateVertex && (
                IsVertexChageOrEdgeAddedRemovedDisposedFromTo(exe.Stack, Vertex.Get(false, @"BaseEdge:"))
                || IsVertexChageOrEdgeAddedRemovedDisposedFromTo(exe.Stack, Vertex.Get(false, @"BaseEdge:\To:"))))
                listVisualiser.BaseEdgeToUpdated();
            else
            {
                bool needToUpdateBaseEdge = false;

                foreach (string meta in listVisualiser.MetaTriggeringUpdateVertex)
                    if (IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(exe.Stack, Visualiser.Vertex, meta))
                        needToUpdateBaseEdge = true;

                if (needToUpdateBaseEdge)
                    listVisualiser.BaseEdgeToUpdated();

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

        IVertex tempSelectedVertices;

        protected void CopySelectedVerticesToTemp()
        {
            tempSelectedVertices = MinusZero.Instance.CreateTempVertex();

            tempSelectedVertices.AddExternalReference();

            GraphUtil.CopyShallow(Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}"), tempSelectedVertices);
        }

        protected void RestoreSelectedVertices()
        {            
            if (tempSelectedVertices != null)
            {
                VisualiserUtil.RemoveAllSelectedEdges(Visualiser);

                IVertex sv = Vertex.Get(false, "SelectedEdges:");
                GraphUtil.CopyShallow(tempSelectedVertices, sv);

                tempSelectedVertices.RemoveExternalReference();
            }
        }

        // Should be corrected as uncommeted makes dnd from tree to UXContainer not working

        /*private override void dndPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dndStartPoint = e.GetPosition(VisualiserAsFrameworkElement);
            hasButtonBeenDown = true;

            CopySelectedVerticesToTemp();

            MinusZero.Instance.IsGUIDragging = false;
        }*/

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

                RestoreSelectedVertices();

                IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                if (Vertex.Get(false, @"SelectedEdges:\") != null)
                    foreach (IEdge ee in Vertex.GetAll(false, @"SelectedEdges:\"))
                        dndVertex.AddEdge(null, ee.To);
                else
                {
                    IVertex v = Visualiser.GetEdgeByPoint(dndStartPoint);
                    if (v != null)
                        dndVertex.AddEdge(null, v);
                }

                if (dndVertex.Count() > 0)
                {
                    dndVertex.AddExternalReference();

                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", VisualiserAsFrameworkElement);

                    Dnd.DoDragDrop(VisualiserAsFrameworkElement, dragData);

                    e.Handled = true;
                }

                isDraggin = false;
            }
        }

        // Should be corrected as uncommeted makes dnd from tree to UXContainer not working
        // QQQ below two methods were commented out and now 2026.02.17 theyy are working and we are checking what is going on
        protected override void dndDrop(object sender, DragEventArgs e)
        {
            IVertex v = Visualiser.GetEdgeByPoint(e.GetPosition(VisualiserAsFrameworkElement));

            if (v == null && GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"User\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "OnlyEnd"))
                v = Vertex.Get(false, "BaseEdge:");

            if (v != null)
                Dnd.DoDrop(null, v.Get(false, "To:"), e);

            e.Handled = true;
        }

        protected override void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }
    }
}
