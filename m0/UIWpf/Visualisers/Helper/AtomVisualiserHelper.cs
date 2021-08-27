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
    public class AtomVisualiserHelper
    {
        protected IVisualiser visualiser;
        protected FrameworkElement visualiserAsFrameworkElement;

        protected IEdge visualiserVertexEdge;

        protected IList<string> scopeQueries;
        protected IList<GraphChangeFilterEnum> changeTypeFilter;
        protected string scopeQueriesName;

        public string visualiserName;

        protected bool dndSupport;

        public AtomVisualiserHelper(IVertex _visualiserMetaVertex, 
            IVisualiser _visualiser, 
            string _visualiserName, 
            FrameworkElement _visualiserAsFrameworkElement)
            :this(_visualiserMetaVertex, 
                 _visualiser, 
                 _visualiserName, 
                 _visualiserAsFrameworkElement, 
                 true, 
                 new List<string> { @"BaseEdge:\To:" },
                 "AtomVisualiser")
        {
            
        }

        public AtomVisualiserHelper(IVertex visualiserMetaVertex, 
            IVisualiser _visualiser, 
            string _visualiserName, 
            FrameworkElement _visualiserAsFrameworkElement, 
            bool _dndSupport, 
            IList<string> _scopeQueries,
            string _scopeQueriesName)
        {
            visualiser = _visualiser;

            visualiserAsFrameworkElement = _visualiserAsFrameworkElement;

            dndSupport = _dndSupport;

            scopeQueries = _scopeQueries;

            changeTypeFilter = new List<GraphChangeFilterEnum> {GraphChangeFilterEnum.ValueChange,
                     GraphChangeFilterEnum.OutputEdgeAdded,
                     GraphChangeFilterEnum.OutputEdgeRemoved,
                     GraphChangeFilterEnum.OutputEdgeDisposed};

            scopeQueriesName = _scopeQueriesName;

            visualiser.VisualiserHelper = this;

            MinusZero mz = MinusZero.Instance;

            visualiserName = _visualiserName + visualiser.GetHashCode();

            if (mz != null && mz.IsInitialized)
            {                
                visualiser.Vertex = mz.CreateTempVertex();

                visualiser.Vertex.Value = visualiserName;

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(visualiser.Vertex, visualiserMetaVertex);

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

        //bool isBaseEdgeUpdating = false;

        protected virtual INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
          //  if (isBaseEdgeUpdating)
            //    return exe.Stack;

            //isBaseEdgeUpdating = true;

            visualiser.UpdateBaseEdge();

            //isBaseEdgeUpdating = false;

            return exe.Stack;
        }

        IEdge graphChangeListenerEdge;

        public IVertex _Vertex;

        public void SetVertex(IVertex value)
        {
            if (_Vertex != null)
                GraphChangeTrigger.RemoveGraphChangeListener(graphChangeListenerEdge);            

            _Vertex = value;

            IEdge graphChangeTriggerEdge = GraphChangeTrigger.AddGraphChangeTrigger(_Vertex, 
                scopeQueries, 
                changeTypeFilter,
                scopeQueriesName);

            graphChangeListenerEdge = ExecutionFlowHelper.AddListener_DotNetDelegate(graphChangeTriggerEdge.To, VertexChange, visualiserName);            

            visualiser.UpdateBaseEdge();
        }

        public bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;

                visualiserVertexEdge.From.DeleteEdge(visualiserVertexEdge);

                GraphChangeTrigger.RemoveGraphChangeListener(graphChangeListenerEdge);                

                if (_Vertex is IDisposable)
                    ((IDisposable)_Vertex).Dispose();

                if (visualiser.SubVisualisers != null)
                    foreach (IDisposable d in visualiser.SubVisualisers)
                        d.Dispose();
            }
        }

        ///// DRAG AND DROP

        protected Point dndStartPoint;

        protected virtual void dndPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dndStartPoint = e.GetPosition(visualiserAsFrameworkElement);

            MinusZero.Instance.IsGUIDragging = false;

            hasButtonBeenDown = true;
        }

        protected bool isDraggin = false;
        protected bool hasButtonBeenDown;

        protected virtual void dndPreviewMouseMove(object sender, MouseEventArgs e)
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

        protected virtual void dndDrop(object sender, System.Windows.DragEventArgs e)
        {
            Dnd.DoDrop(visualiser, _Vertex.Get(false, @"BaseEdge:\To:"), e);
        }

        protected virtual void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }
    }
}
