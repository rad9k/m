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
        IListVisualiser listVisualiser;

        public ListVisualiserHelper(IVertex _visualiserMetaVertex,
            IVisualiser _visualiser,
            string _visualiserName,
            FrameworkElement _visualiserAsFrameworkElement,
            IVertex baseEdgeVertex)
            : this(_visualiserMetaVertex,
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

        public ListVisualiserHelper(IVertex visualiserMetaVertex,
            IVisualiser _visualiser,
            string _visualiserName,
            FrameworkElement _visualiserAsFrameworkElement,
            bool _dndSupport,
            IList<string> _scopeQueries,
            string _scopeQueriesName,
            IVertex baseEdgeVertex,
            UpdateBaseEdgeCallSchemeEnum _updateBaseEdgeCallScheme            
            )
            :this(visualiserMetaVertex,
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

            public ListVisualiserHelper(IVertex visualiserMetaVertex,
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
            : base(visualiserMetaVertex,
                  _visualiser,
                  _visualiserName,
                  _visualiserAsFrameworkElement,
                  _dndSupport,
                  _scopeQueries,
                  _scopeQueriesName,
                  baseEdgeVertex,
                  _updateBaseEdgeCallScheme,
                  null,
                  _visualiserAsBaseEdge)
        {
            listVisualiser = (IListVisualiser)_visualiser;
        }

        public delegate INoInEdgeInOutVertexVertex CustomVertexChangeHandler(IExecution exe);

        public event CustomVertexChangeHandler CustomVertexChangeEvent;

        bool firstVertexChangeExecuted = false;

        protected override INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {            
            if (!firstVertexChangeExecuted && updateBaseEdgeCallSchema == UpdateBaseEdgeCallSchemeEnum.OmmitSecond)
            {
                firstVertexChangeExecuted = true;
                return exe.Stack;
            }

            if (CustomVertexChangeEvent != null)
                return CustomVertexChangeEvent(exe);            

            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMeta(exe.Stack, "ZoomVisualiserContent"))            
                listVisualiser.ZoomVisualiserContentChange();
                
            if (IsVertexChangeOrEdgeAddedRemovedDisposedByMeta(exe.Stack, "SelectedEdges")
                || IsEdgeAddedRemovedDiscardedFrom(exe.Stack, Vertex.Get(false, @"SelectedEdges:")))            
                listVisualiser.SelectedVerticesUpdated();                

            if(IsVertexChageOrEdgeAddedRemovedDisposedFromTo(exe.Stack, Vertex.Get(false, @"BaseEdge:"))
                || IsVertexChageOrEdgeAddedRemovedDisposedFromTo(exe.Stack, Vertex.Get(false, @"BaseEdge:\To:")))
                listVisualiser.UpdateBaseEdge();
            else
            {
                bool needToUpdateBaseEdge = false;

                foreach (string meta in listVisualiser.MetaTriggeringUpdateBaseEdge)
                    if (IsVertexChangeOrEdgeAddedRemovedDisposedByMeta(exe.Stack, meta))
                        needToUpdateBaseEdge = true;

                if(needToUpdateBaseEdge)
                    listVisualiser.UpdateBaseEdge();

                bool needToUpdateView = false;

                foreach (string meta in listVisualiser.MetaTriggeringUpdateView)
                    if (IsVertexChangeOrEdgeAddedRemovedDisposedByMeta(exe.Stack, meta))
                        needToUpdateView = true;

                if (needToUpdateView)
                    listVisualiser.UpdateView();
            }

            return exe.Stack;
        }

        // DRAG AND DROP

        IVertex tempSelectedVertices;

        protected void CopySelectedVerticesToTemp()
        {
            tempSelectedVertices = MinusZero.Instance.CreateTempVertex();

            GraphUtil.CopyEdges(Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}"), tempSelectedVertices);
        }

        protected void RestoreSelectedVertices()
        {            
            if (tempSelectedVertices != null)
            {
                VisualiserUtil.RemoveAllSelectedEdges(visualiser);

                IVertex sv = Vertex.Get(false, "SelectedEdges:");
                GraphUtil.CopyEdges(tempSelectedVertices, sv);
            }
        }

        private void dndPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dndStartPoint = e.GetPosition(visualiserAsFrameworkElement);
            hasButtonBeenDown = true;

            CopySelectedVerticesToTemp();

            MinusZero.Instance.IsGUIDragging = false;
        }

        protected override void dndPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(visualiserAsFrameworkElement);
            Vector diff = dndStartPoint - mousePos;

            var headersPresenter = m0.UIWpf.WpfUtil.FindVisualChild<DataGridColumnHeadersPresenter>(visualiserAsFrameworkElement);

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
                    IVertex v = visualiser.GetEdgeByLocation(dndStartPoint);
                    if (v != null)
                        dndVertex.AddEdge(null, v);
                }

                if (dndVertex.Count() > 0)
                {
                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", visualiserAsFrameworkElement);

                    Dnd.DoDragDrop(visualiserAsFrameworkElement, dragData);

                    e.Handled = true;
                }

                isDraggin = false;
            }
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            IVertex v = visualiser.GetEdgeByLocation(e.GetPosition(visualiserAsFrameworkElement));

            if (v == null && GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"User\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "OnlyEnd"))
                v = Vertex.Get(false, "BaseEdge:");

            if (v != null)
                Dnd.DoDrop(null, v.Get(false, "To:"), e);

            e.Handled = true;
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }
    }
}
