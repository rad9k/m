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
    public class GenericVisualiserHelper
    {
        IVisualiser visualiser;
        FrameworkElement visualiserAsFrameworkElement;

        IEdge visualiserVertexEdge;

        IList<string> scopeQueries;
        string scopeQueriesName;

        public string visualiserName;

        bool dndSupport;

        public GenericVisualiserHelper(IVisualiser _visualiser, string _visualiserName, FrameworkElement _visualiserAsFrameworkElement)
            :this(_visualiser, _visualiserName, _visualiserAsFrameworkElement, true, new List<string> { @"BaseEdge:\To:" }, "AtomVisualiser")
        {
            
        }

        public GenericVisualiserHelper(IVisualiser _visualiser, string _visualiserName, FrameworkElement _visualiserAsFrameworkElement, bool _dndSupport, IList<string> _scopeQueries, string _scopeQueriesName)
        {
            visualiser = _visualiser;

            visualiserAsFrameworkElement = _visualiserAsFrameworkElement;

            dndSupport = _dndSupport;

            scopeQueries = _scopeQueries;

            scopeQueriesName = _scopeQueriesName;

            visualiser.VisualiserHelper = this;

            MinusZero mz = MinusZero.Instance;

            visualiserName = _visualiserName + this.GetHashCode();

            if (mz != null && mz.IsInitialized)
            {                
                visualiser.Vertex = mz.CreateTempVertex();

                visualiser.Vertex.Value = visualiserName;

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(visualiser.Vertex, mz.Root.Get(false, @"System\Meta\Visualiser\String"));

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(visualiser.Vertex.Get(false, "BaseEdge:"), 
                    mz.Root.Get(false, @"System\Meta\ZeroTypes\Edge"));

                visualiserVertexEdge = mz.Root.Get(false, @"User\CurrentUser:\Session:\Visualisers:").
                    AddEdge(mz.Root.Get(false, @"Meta\User\VisualiserList\Visualiser"), visualiser.Vertex);


                visualiserAsFrameworkElement.Loaded += new RoutedEventHandler(visualiser.OnLoad);

                if (dndSupport)
                {
                    visualiserAsFrameworkElement.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
                    visualiserAsFrameworkElement.PreviewMouseMove += dndPreviewMouseMove;
                    visualiserAsFrameworkElement.Drop += dndDrop;
                    visualiserAsFrameworkElement.AllowDrop = true;

                    visualiserAsFrameworkElement.MouseEnter += dndMouseEnter;
                }else
                    visualiserAsFrameworkElement.AllowDrop = false;
            }

        }

        public void AddContextMenu()
        {
            if (!WpfUtil.HasParentsGotContextMenu(visualiserAsFrameworkElement))
                visualiserAsFrameworkElement.ContextMenu = new m0ContextMenu(visualiser);
        }

        protected INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            visualiser.UpdateBaseEdge();

            return exe.Stack;
        }

        IEdge graphChangeListenerEdge;

        public IVertex _Vertex;

        public void SetVertex(IVertex value)
        {
            if (_Vertex != null)
                ExecutionFlowHelper.RemoveGraphChangeListener(graphChangeListenerEdge);            

            _Vertex = value;

            IEdge graphChangeTriggerEdge = ExecutionFlowHelper.AddGraphChangeTrigger(_Vertex, scopeQueries, scopeQueriesName);

            graphChangeListenerEdge = ExecutionFlowHelper.AddListener_DotNetDelegate(graphChangeTriggerEdge.To, VertexChange, visualiserName);            

            visualiser.UpdateBaseEdge();
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;

                visualiserVertexEdge.From.DeleteEdge(visualiserVertexEdge);

                ExecutionFlowHelper.RemoveGraphChangeListener(graphChangeListenerEdge);                

                if (_Vertex is IDisposable)
                    ((IDisposable)_Vertex).Dispose();
            }
        }

        ///// DRAG AND DROP

        Point dndStartPoint;

        private void dndPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dndStartPoint = e.GetPosition(visualiserAsFrameworkElement);

            MinusZero.Instance.IsGUIDragging = false;

            hasButtonBeenDown = true;
        }

        bool isDraggin = false;
        bool hasButtonBeenDown;

        private void dndPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(visualiserAsFrameworkElement);
            Vector diff = dndStartPoint - mousePos;

            if (hasButtonBeenDown && isDraggin == false && (e.LeftButton == MouseButtonState.Pressed) && (
                (Math.Abs(diff.X) > Dnd.MinimumHorizontalDragDistance) ||
                (Math.Abs(diff.Y) > Dnd.MinimumVerticalDragDistance)))
            {
                if (_Vertex.Get(false, @"BaseEdge:\To:") != null)
                {
                    isDraggin = true;

                    IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                    dndVertex.AddEdge(null, _Vertex.Get(false, @"BaseEdge:"));

                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", visualiser);

                    Dnd.DoDragDrop(visualiserAsFrameworkElement, dragData);

                    isDraggin = false;
                }
            }
        }

        private void dndDrop(object sender, System.Windows.DragEventArgs e)
        {
            Dnd.DoDrop(visualiser, _Vertex.Get(false, @"BaseEdge:\To:"), e);
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }
    }
}
