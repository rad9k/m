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

namespace m0.UIWpf.Visualisers
{
    public class TestVisualiser : TextBox, IPlatformClass, IDisposable, IHasLocalizableEdges
    {
        public TestVisualiser()
        {
            MinusZero mz = MinusZero.Instance;            

            if (mz != null && mz.IsInitialized)
            {                
                this.AcceptsReturn = true;

                ///////////////////////////////////////
                ExecutionFlowHelper.StartTransaction();
                ///////////////////////////////////////

                Vertex = mz.CreateTempVertex();
                
                Vertex.Value= "TestVisualiser" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(false, @"System\Meta\Visualiser\String"));

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get(false, "BaseEdge:"), mz.Root.Get(false, @"System\Meta\ZeroTypes\Edge"));

                ////////////////////////////////////////
                ExecutionFlowHelper.CommitTransaction();
                ////////////////////////////////////////

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
            if (!WpfUtil.HasParentsGotContextMenu(this))
                this.ContextMenu = new m0ContextMenu(this);
        }

        protected override void OnDragEnter(DragEventArgs e) // Do not want standard base implemention, that prevents allow drop
        {
            
        }

        protected override void OnDragOver(DragEventArgs e) // Do not want standard base implemention, that prevents allow drop
        {
            
        }

        protected override void OnTextChanged(TextChangedEventArgs e)        
        {        
            base.OnTextChanged(e);    

            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            if (bv == null)
            {
                IVertex from = Vertex.Get(false, @"BaseEdge:\From:");
                IVertex meta = Vertex.Get(false, @"BaseEdge:\Meta:");

                GraphUtil.SetVertexValue(from, meta, this.Text);

                IsNull = false;
            }else            
                bv.Value = this.Text;

            //////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////
        }

        bool _IsNull;

        bool IsNull
        {
            get { return _IsNull; }
            set
            {
                _IsNull = value;

                if(IsNull)
                    this.Background = (Brush)FindResource("0VeryLightGrayBrush");
                else
                    this.Background = (Brush)FindResource("0BackgroundBrush");                
            }
        }

        private void UpdateBaseEdge(){
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            if (bv != null && bv.Value != null /*&& ((String)bv.Value) != "$Empty"*/)
            {
                this.Text = bv.Value.ToString();
                IsNull = false;
            }
            else
                IsNull = true;
        }

        protected INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            UpdateBaseEdge();

            return exe.Stack;
        }

        /*protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge")))
                UpdateBaseEdge();                        

            if ( (sender == Vertex.Get(false, "BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To"))
                || (sender == Vertex.Get(false, @"BaseEdge:\To:") && e.Type==VertexChangeType.ValueChanged) )            
                UpdateBaseEdge();
            
        }*/

        IEdge graphChangeTriggerEdge;

        private IVertex _Vertex;

        public IVertex Vertex
        {
            get { return _Vertex; }
            set
            {
                if (_Vertex != null)
                    ExecutionFlowHelper.RemoveGraphChangeTrigger(graphChangeTriggerEdge);
                
                //PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                _Vertex = value;

                //graphChangeTriggerEdge = ExecutionFlowHelper.AddGraphChangeTrigger(_Vertex, new List<string> {});

                graphChangeTriggerEdge = ExecutionFlowHelper.AddGraphChangeTrigger(_Vertex, new List<string> { "", "BaseEdge:", "SelectedEdges:" });
                ExecutionFlowHelper.AddListener_DotNetDelegate(graphChangeTriggerEdge.To, VertexChange);

                //PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });

                UpdateBaseEdge();
            }
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;

                ExecutionFlowHelper.RemoveGraphChangeTrigger(graphChangeTriggerEdge);

                //PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                if (Vertex is IDisposable)
                    ((IDisposable)Vertex).Dispose();
            }
        }

        public IVertex GetEdgeByLocation(Point point)
        {
            return Vertex.Get(false, @"BaseEdge:");
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex vertex)
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

                    dndVertex.AddEdge(null, Vertex.Get(false, @"BaseEdge:"));

                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", this);

                    Dnd.DoDragDrop(this, dragData);                   

                    isDraggin =false;
                }
            }

            
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            Dnd.DoDrop(this,Vertex.Get(false, @"BaseEdge:\To:"), e);
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }

    }
}
