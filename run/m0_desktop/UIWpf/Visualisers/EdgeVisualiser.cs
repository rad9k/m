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
using System.Windows.Media;
using m0.UIWpf.Foundation;
using m0.UIWpf.Controls;
using m0.UIWpf.Commands;
using System.Windows;
using m0.UIWpf.Visualisers.Helper;

namespace m0.UIWpf.Visualisers
{
    public class EdgeVisualiser : TextBlock, IVisualiser, ITypedEdge
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }

        // TypedEdge START

        public EdgeVisualiser(IEdge _edge)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        public EdgeVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            this.Background = (Brush)FindResource("0LightGrayBrush");

            new AtomVisualiserHelper(parentVisualiser,
                isVolatile,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Edge"), 
                this, 
                "EdgeVisualiser", 
                this, 
                false, 
                new List<string> { @"BaseEdge:\To:"}, 
                "AtomVisualiser",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitSecond);

            this.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
            this.PreviewMouseMove += dndPreviewMouseMove;
            this.Drop += dndDrop;
            this.AllowDrop = true;

            this.MouseEnter += dndMouseEnter;
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ScaleChange() { }

        public void BaseEdgeToUpdated()
        {
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:\To:");

            if (bv != null && bv.Value != null)
                this.Text = bv.Value.ToString();
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                VisualiserHelper.Dispose();
            }
        }

        public IVertex GetEdgeByPoint(System.Windows.Point point)
        {
            return Vertex.Get(false, @"BaseEdge:");
        }

        public IVertex GetEdgeByVisualElement(System.Windows.FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public System.Windows.FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }

        ///// DRAG AND DROP

        Point dndStartPoint;

        private void dndPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dndStartPoint = e.GetPosition(this);

            MinusZero.Instance.IsGUIDragging = false;

            hasButtonBeenDown = true;
        }

        bool isDraggin = false;
        bool hasButtonBeenDown;

        private void dndPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            Vector diff = dndStartPoint - mousePos;

            if (hasButtonBeenDown && isDraggin == false && (e.LeftButton == MouseButtonState.Pressed) && (
                (Math.Abs(diff.X) > Dnd.MinimumHorizontalDragDistance) ||
                (Math.Abs(diff.Y) > Dnd.MinimumVerticalDragDistance)))
            {
                if (Vertex.Get(false, @"BaseEdge:\To:") != null)
                {
                    isDraggin = true;

                    IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                    //dndVertex.AddEdge(null, Vertex.Get(false, @"BaseEdge:")); // this is

                    dndVertex.AddEdge(null, Vertex.Get(false, @"BaseEdge:\To:"));

                    dndVertex.AddExternalReference();

                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", this);

                    Dnd.DoDragDrop(this, dragData);

                    isDraggin = false;
                }
            }
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            Dnd.DoDropForEdgeVisualiser(this, Vertex.Get(false, @"BaseEdge:"), e);

            BaseEdgeToUpdated();
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }
    }
}

