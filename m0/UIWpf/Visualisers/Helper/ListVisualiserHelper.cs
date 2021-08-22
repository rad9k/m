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

namespace m0.UIWpf.Visualisers.Helper
{
    public class ListVisualiserHelper : AtomVisualiserHelper
    {
        IListVisualiser listVisualiser;

        public ListVisualiserHelper(IVertex _visualiserMetaVertex,
            IVisualiser _visualiser,
            string _visualiserName,
            FrameworkElement _visualiserAsFrameworkElement)
            : this(_visualiserMetaVertex,
                  _visualiser,
                  _visualiserName,
                  _visualiserAsFrameworkElement,
                  true,
                  new List<string> { @"", @"BaseEdge:\", @"BaseEdge:\To:\" },
                  "ListVisualiser")
        {

        }

        public ListVisualiserHelper(IVertex visualiserMetaVertex,
            IVisualiser _visualiser,
            string _visualiserName,
            FrameworkElement _visualiserAsFrameworkElement,
            bool _dndSupport,
            IList<string> _scopeQueries,
            string _scopeQueriesName)
            : base(visualiserMetaVertex,
                  _visualiser,
                  _visualiserName,
                  _visualiserAsFrameworkElement,
                  _dndSupport,
                  _scopeQueries,
                  _scopeQueriesName)
        {
            listVisualiser = (IListVisualiser)_visualiser;
        }

        //bool isBaseEdgeUpdating = false;

        protected override INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
          //  if (isBaseEdgeUpdating)
            //    return exe.Stack;

            //isBaseEdgeUpdating = true;

            IVertex changedVertex = exe.Stack.Get(false, @"event:\ChangedVertex:");

            if (changedVertex != null)
            {
                if (GraphUtil.ExistQueryIn(changedVertex, "ZoomVisualiserContent", null))
                {
                    listVisualiser.ZoomVisualiserContentChange();
                    return exe.Stack;
                }

                if (GraphUtil.ExistQueryIn(changedVertex, "SelectedEdges", null))
                {
                    listVisualiser.SelectedVerticesUpdated();

                    return exe.Stack;
                }
            }

            visualiser.UpdateBaseEdge();

            //isBaseEdgeUpdating = false;

            return exe.Stack;
        }

        // DRAG AND DROP

        IVertex tempSelectedVertices;

        protected void CopySelectedVerticesToTemp()
        {
            tempSelectedVertices = MinusZero.Instance.CreateTempVertex();

            GraphUtil.CopyEdges(_Vertex.Get(false, "SelectedEdges:"), tempSelectedVertices);
        }

        protected void RestoreSelectedVertices()
        {
            IVertex sv = _Vertex.Get(false, "SelectedEdges:");

            if (tempSelectedVertices != null)
            {
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);

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

            if (hasButtonBeenDown && isDraggin == false &&
                !WpfUtil.IsMouseOverScrollbar(sender, dndStartPoint) &&
                (e.LeftButton == MouseButtonState.Pressed) && (
                (Math.Abs(diff.X) > Dnd.MinimumHorizontalDragDistance) ||
                (Math.Abs(diff.Y) > Dnd.MinimumVerticalDragDistance)))
            {
                isDraggin = true;

                RestoreSelectedVertices();

                IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                if (_Vertex.Get(false, @"SelectedEdges:\") != null)
                    foreach (IEdge ee in _Vertex.GetAll(false, @"SelectedEdges:\"))
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
                v = _Vertex.Get(false, "BaseEdge:");

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
