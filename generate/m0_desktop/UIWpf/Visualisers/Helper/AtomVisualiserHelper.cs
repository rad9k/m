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
using m0.UIWpf.UX;
using m0.ZeroTypes.UX;
using m0.ZeroCode.Helpers;

namespace m0.UIWpf.Visualisers.Helper
{
    public enum UpdateBaseEdgeCallSchemeEnum { OmmitFirst, OmmitSecond, NoOmmit};

    public class AtomVisualiserHelper
    {
        public bool ForceVertexChangeOff = false;

        protected bool IsUX = false;

        protected IVisualiser Visualiser;
        protected FrameworkElement VisualiserAsFrameworkElement;

        protected IList<string> ScopeQueries;
        protected IList<GraphChangeFilterEnum> ChangeTypeFilter;
        protected string ScopeQueriesName;

        public string VisualiserName;

        protected bool dndSupport;

        protected UpdateBaseEdgeCallSchemeEnum updateBaseEdgeCallSchema;

        public IEdge graphChangeListenerEdge;

        public IVertex Vertex;

        public IVertex baseEdgeVertex;

        static IVertex r = m0.MinusZero.Instance.root;        

        static IVertex baseEdge_meta = r.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");

        public delegate INoInEdgeInOutVertexVertex CustomVertexChangeHandler(IExecution exe);

        public event CustomVertexChangeHandler CustomVertexChangeEvent;

        public static void Initialize()
        {
            IVertex r = m0.MinusZero.Instance.root;

            baseEdge_meta = r.Get(false, @"System\Meta\ZeroTypes\HasBaseEdge\BaseEdge");
        }

        public AtomVisualiserHelper(
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
                 new List<string> { "", @"BaseEdge:\To:" },
                 "AtomVisualiser",
                 baseEdgeVertex,
                 UpdateBaseEdgeCallSchemeEnum.OmmitSecond,                 
                 false
                 )
        {

        }

        public AtomVisualiserHelper(
            IVertex parentVisualiser,
            bool isVolatile,
            IVertex _visualiserMetaVertex,
            IVisualiser _visualiser,
            string _visualiserName,
            FrameworkElement _visualiserAsFrameworkElement,
            bool _dndSupport,
            IList<string> _scopeQueries,
            string _scopeQueriesName,
            IVertex baseEdgeVertex,
            UpdateBaseEdgeCallSchemeEnum _updateBaseEdgeCallSchema):
            this(parentVisualiser,
                isVolatile,
                 _visualiserMetaVertex,
                 _visualiser,
                 _visualiserName,
                 _visualiserAsFrameworkElement,
                 _dndSupport,
                 _scopeQueries,
                 _scopeQueriesName,
                 baseEdgeVertex,
                 _updateBaseEdgeCallSchema,                 
                 false
                 )
        {

        }

        public AtomVisualiserHelper(
            IVertex parentVisualiser,
            bool isVolatile,
            IVertex visualiserMetaVertex, 
            IVisualiser _visualiser, 
            string _visualiserName, 
            FrameworkElement _visualiserAsFrameworkElement, 
            bool _dndSupport, 
            IList<string> _scopeQueries,
            string _scopeQueriesName,
            IVertex _baseEdgeVertex,
            UpdateBaseEdgeCallSchemeEnum _updateBaseEdgeCallSchema,            
            bool _visualiserAsBaseEdge)
        {
            baseEdgeVertex = _baseEdgeVertex;

            Visualiser = _visualiser;

            VisualiserAsFrameworkElement = _visualiserAsFrameworkElement;

            IsUX = _visualiserAsBaseEdge;

            dndSupport = _dndSupport;

            ScopeQueries = _scopeQueries;

            updateBaseEdgeCallSchema = _updateBaseEdgeCallSchema;

            ChangeTypeFilter = new List<GraphChangeFilterEnum> {GraphChangeFilterEnum.ValueChange,
                     GraphChangeFilterEnum.OutputEdgeAdded,
                     GraphChangeFilterEnum.OutputEdgeRemoved,
                     GraphChangeFilterEnum.OutputEdgeDisposed};

            ScopeQueriesName = _scopeQueriesName;

            Visualiser.VisualiserHelper = this;

            MinusZero mz = MinusZero.Instance;

            VisualiserName = _visualiserName + Visualiser.GetHashCode();

            if (mz != null && mz.IsInitialized)
            {
                IVertex vVertex;

                if (IsUX)
                {
                    IVertex baseEdgeVertexTo = GraphUtil.GetQueryOutFirst(baseEdgeVertex, "To", null);

                    if (baseEdgeVertexTo != null)
                        vVertex = baseEdgeVertexTo;
                    else
                        vVertex = baseEdgeVertex;

                    Visualiser.Vertex = vVertex;

                    VisualisersList.AddVisualiser(Visualiser, parentVisualiser, false, isVolatile);
                }
                else
                {
                    vVertex = mz.CreateTempVertex();

                    ClassVertex.AddIsClassAndAllAttributesAndAssociations(vVertex, visualiserMetaVertex);

                    if (baseEdgeVertex != null)
                        GraphUtil.ReplaceEdge(vVertex, baseEdge_meta, baseEdgeVertex);
                    else
                        ClassVertex.AddIsClassAndAllAttributesAndAssociations(vVertex.Get(false, "BaseEdge:"),
                            mz.Root.Get(false, @"System\Meta\ZeroTypes\Edge"));

                    vVertex.AddExternalReference();

                    Visualiser.Vertex = vVertex;

                    Visualiser.Vertex.Value = VisualiserName;

                    VisualisersList.AddVisualiser(Visualiser, parentVisualiser, true, isVolatile);
                }               

                VisualiserAsFrameworkElement.Loaded += new RoutedEventHandler(Visualiser.OnLoad);

                if (dndSupport)
                {
                    VisualiserAsFrameworkElement.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
                    VisualiserAsFrameworkElement.PreviewMouseMove += dndPreviewMouseMove;
                    VisualiserAsFrameworkElement.Drop += dndDrop;
                    VisualiserAsFrameworkElement.AllowDrop = true;

                    VisualiserAsFrameworkElement.MouseEnter += dndMouseEnter;
                } else
                    VisualiserAsFrameworkElement.AllowDrop = false;
            }

        }

        public void AddContextMenu()
        {
            if (!WpfUtil.HasParentsGotContextMenu(VisualiserAsFrameworkElement))
                VisualiserAsFrameworkElement.ContextMenu = new m0ContextMenu(Visualiser);
        }        

        bool firstVertexChangeExecuted = false;

        protected virtual INoInEdgeInOutVertexVertex VertexChange(IExecution exe)
        {
            if (ForceVertexChangeOff)
                return exe.Stack;

            if (!firstVertexChangeExecuted && updateBaseEdgeCallSchema == UpdateBaseEdgeCallSchemeEnum.OmmitSecond)
            {
                firstVertexChangeExecuted = true;
                return exe.Stack;
            }
          
            Visualiser.BaseEdgeToUpdated();          

            return exe.Stack;
        }        

        public void SetVertex(IVertex value)
        {
            if (Vertex != null)
                GraphChangeTrigger.RemoveListener(graphChangeListenerEdge);            

            Vertex = value;

            IEdge graphChangeTriggerEdge = GraphChangeTrigger.AddTrigger(Vertex, 
                ScopeQueries, 
                ChangeTypeFilter,
                ScopeQueriesName);

            graphChangeListenerEdge = ExecutionFlowHelper.AddListener_DotNetDelegate(graphChangeTriggerEdge.To, VertexChange, VisualiserName);            

            if(updateBaseEdgeCallSchema != UpdateBaseEdgeCallSchemeEnum.OmmitFirst)
                Visualiser.BaseEdgeToUpdated();
        }

        public bool IsDisposed = false;

        public void DisposeAllChildVisualisers()
        {
            foreach (IEdge e in Visualiser.Vertex.GetAll(false, "Item:"))
                VisualisersList.GetVisualiser(e.To).Dispose();
        }

        public void DisposeAllChildVisualisersExceptWrap()
        {
            foreach (IEdge e in Visualiser.Vertex.GetAll(false, "Item:"))
                if(!GraphUtil.ExistQueryOut(e.To, "$Is", "Wrap"))
                    VisualisersList.GetVisualiser(e.To).Dispose();
        }        

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;

                VisualisersList.RemoveVisualiser(Visualiser);

                GraphChangeTrigger.RemoveListener(graphChangeListenerEdge);

                DisposeAllChildVisualisers();

                if (Vertex is IDisposable)
                    ((IDisposable)Vertex).Dispose();                
            }
        }

        public void Dispose_UX() // dispose variant for UX visualisers
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;

                VisualisersList.RemoveVisualiser(Visualiser);

                GraphChangeTrigger.RemoveListener(graphChangeListenerEdge);

                DisposeAllChildVisualisers();

               // if (Vertex is IDisposable) NO NO !
                 //   ((IDisposable)Vertex).Dispose();
            }
        }

        ///// DRAG AND DROP

        protected Point dndStartPoint;

        protected virtual void dndPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dndStartPoint = e.GetPosition(VisualiserAsFrameworkElement);

            MinusZero.Instance.IsGUIDragging = false;

            hasButtonBeenDown = true;
        }

        protected bool isDraggin = false;
        protected bool hasButtonBeenDown;

        protected virtual void dndPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(VisualiserAsFrameworkElement);
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

                    dndVertex.AddExternalReference();

                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", Visualiser);

                    Dnd.DoDragDrop(VisualiserAsFrameworkElement, dragData);

                    isDraggin = false;
                }
            }
        }

        protected virtual void dndDrop(object sender, System.Windows.DragEventArgs e)
        {
            Dnd.DoDrop(Visualiser, Vertex.Get(false, @"BaseEdge:\To:"), e);
        }

        protected virtual void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }

    }
}
