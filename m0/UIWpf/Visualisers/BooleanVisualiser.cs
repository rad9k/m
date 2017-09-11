using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.UML;
using m0.ZeroTypes;
using m0.Graph;
using m0.Util;
using System.Windows.Input;
using m0.UIWpf.Foundation;
using m0.UIWpf.Controls;
using System.Windows;
using m0.UIWpf.Commands;

namespace m0.UIWpf.Visualisers
{
    public class BooleanVisualiser : CheckBox, IPlatformClass, IDisposable, IHasLocalizableEdges
    {

        bool IsNull { get; set; }

        public BooleanVisualiser()
        {
            MinusZero mz = MinusZero.Instance;

            if (mz != null && mz.IsInitialized)
            {
                Vertex = mz.CreateTempVertex();
                
                Vertex.Value="BooleanVisualiser" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(@"System\Meta\Visualiser\Boolean"));

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get("BaseEdge:"), mz.Root.Get(@"System\Meta\ZeroTypes\Edge"));

                this.Loaded += new RoutedEventHandler(OnLoad);

                this.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
                this.PreviewMouseMove += dndPreviewMouseMove;
                this.Drop += dndDrop;
                this.AllowDrop = true;

                this.MouseEnter += dndMouseEnter;
            }            
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            if (!UIWpf.HasParentsGotContextMenu(this))
                this.ContextMenu = new m0ContextMenu(this);
        }

        protected override void OnToggle()
        {
            base.OnToggle();

            if (IsNull)
            {
                IVertex r = MinusZero.Instance.Root;

                IVertex from = Vertex.Get(@"BaseEdge:\From:");
                IVertex meta = Vertex.Get(@"BaseEdge:\Meta:");
                IVertex toMeta = r.Get(@"System\Meta\ZeroTypes\Edge\To");

                if (from != null && meta != null)
                {
                    //GraphUtil.ReplaceEdge(Vertex.Get("BaseEdge:"), "To", GraphUtil.SetVertexValue(from, meta, "True")); // NOT
                    //GraphUtil.SetVertexValue(Vertex.Get("BaseEdge:"), toMeta, GraphUtil.SetVertexValue(from, meta, "True")); // NOT!!!!

                    GraphUtil.CreateOrReplaceEdge(Vertex.Get("BaseEdge:"), toMeta, GraphUtil.SetVertexValue(from, meta, "True"));

                    IsNull = false;
                }
            }

            if (Vertex.Get(@"BaseEdge:\To:") != null)
            {
                if (this.IsChecked == true)
                    Vertex.Get(@"BaseEdge:\To:").Value = "True";
                else
                    Vertex.Get(@"BaseEdge:\To:").Value = "False";
            }
        }

        private void UpdateBaseEdge(){
            IVertex bv = Vertex.Get(@"BaseEdge:\To:");

            if (bv != null && bv.Value != null)
            {
                if (GeneralUtil.CompareStrings(bv.Value, "True"))
                    this.IsChecked = true;
                else
                    this.IsChecked = false;

                IsNull = false;
            }
            else
                IsNull = true;
        }

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge"))
                 || (sender == Vertex.Get("BaseEdge:") && e.Type == VertexChangeType.ValueChanged)
                || ((sender == Vertex.Get("BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && ((GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To"))))
                || (sender == Vertex.Get(@"BaseEdge:\To:") && e.Type == VertexChangeType.ValueChanged))
            {
                UpdateBaseEdge();
            }
        }

        private IVertex _Vertex;

        public IVertex Vertex
        {
            get { return _Vertex; }
            set
            {
                if (_Vertex != null)
                    PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                _Vertex = value;

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });

                UpdateBaseEdge();
            }
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;
                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                if (Vertex is IDisposable)
                    ((IDisposable)Vertex).Dispose();
            }
        }

        public IVertex GetEdgeByLocation(System.Windows.Point point)
        {
            return Vertex.Get("BaseEdge:");
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
                if (Vertex.Get(@"BaseEdge:\To:") != null)
                {
                    isDraggin = true;

                    IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                    dndVertex.AddEdge(null, Vertex.Get(@"BaseEdge:"));

                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", this);

                    Dnd.DoDragDrop(this, dragData);

                    isDraggin = false;
                }
            }
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            Dnd.DoDrop(this, Vertex.Get(@"BaseEdge:\To:"), e);
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }
    }
    
}
