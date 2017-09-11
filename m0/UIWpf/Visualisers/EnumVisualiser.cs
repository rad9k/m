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
using System.Windows;
using m0.UIWpf.Commands;
using m0.UIWpf.Controls;

namespace m0.UIWpf.Visualisers
{
    public class EnumVisualiser : ComboBox, IPlatformClass, IDisposable
    {
        public EnumVisualiser()
        {
            MinusZero mz = MinusZero.Instance;

            if (mz != null && mz.IsInitialized)
            {
                Vertex = mz.CreateTempVertex();

                Vertex.Value = "EnumVisualiser" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(@"System\Meta\Visualiser\Enum"));

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

        bool DoingSelectionChanged = false;

        protected override void OnSelectionChanged(SelectionChangedEventArgs _e)
        {
            if (DoingSelectionChanged == false)
            {
                DoingSelectionChanged = true;

                if (this.SelectedItem != null && ((ComboBoxItem)this.SelectedItem).Tag is IVertex)
                {
                    IVertex tag = (IVertex)((ComboBoxItem)this.SelectedItem).Tag;

                    IVertex bev = Vertex.Get("BaseEdge:");

                    if (bev != null)
                    {
                        IVertex fromv = bev.Get("From:");
                        IVertex metav = bev.Get("Meta:");
                        IVertex tov = bev.Get("To:");

                        if (tov != tag) // is there any change ?
                        {
                            //GraphUtil.ReplaceEdge(fromv, metav, tag);

                            GraphUtil.CreateOrReplaceEdge(fromv, metav, tag);

                            GraphUtil.CreateOrReplaceEdge(bev, MinusZero.Instance.Root.Get(@"System\Meta\ZeroTypes\Edge\To"), tag);                            
                        }
                    }                    
                }

                DoingSelectionChanged = false;
            }

            base.OnSelectionChanged(_e);
        }

        private void UpdateBaseEdge()
        {
            IVertex bev = Vertex.Get("BaseEdge:");

            if (bev == null)
                return;

            IVertex fromv = bev.Get("From:");
            IVertex metav = bev.Get("Meta:");
            IVertex tov = bev.Get("To:");

            if(fromv!=null && metav!=null && tov!=null){                
                this.Items.Clear();

                ComboBoxItem SelectedItem=null;

                foreach (IEdge e in metav.GetAll(@"$EdgeTarget:\EnumValue:"))
                {
                    ComboBoxItem i = new ComboBoxItem();
                    i.Content = e.To.Value;
                    i.Tag = e.To;
                    this.Items.Add(i);

                    if(tov.Value==e.To.Value)
                        SelectedItem = i;
                }

                this.SelectedItem = SelectedItem;

            }

        }

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge"))
                 || (sender == Vertex.Get("BaseEdge:") && e.Type == VertexChangeType.EdgeAdded)
                || (sender == Vertex.Get(@"BaseEdge:\To:") && e.Type == VertexChangeType.ValueChanged)
                || sender == Vertex.Get(@"BaseEdge:\Meta:"))
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

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge"/*, "SelectedEdges" */});

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
