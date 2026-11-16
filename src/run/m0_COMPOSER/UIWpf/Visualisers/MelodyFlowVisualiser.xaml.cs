using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf;
using m0.UIWpf.Visualisers;
using m0.Util;
using m0.ZeroTypes;
using m0_COMPOSER.Lib;
using m0_COMPOSER.UIWpf.Visualisers.Control;
using m0_COMPOSER.UIWpf.Visualisers.Control.Item;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    public partial class MelodyFlowVisualiser : MelodyFlowVisualiserBase, INoDownVisualiser
    {
        static string[] _MetaTriggeringUpdateVertex = new string[] { "ShowArrowLines", "ShowLabel", "ShowVelocity", "DefaultVelocity" };
        public override string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { };
        public override string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        protected static IList<string> _listenerScopeQueries = new List<string> { @"", @"BaseEdge:\To:", @"BaseEdge:\To:\", @"BaseEdge:\To:\\", @"BaseEdge:\To:\\\" };
        protected override IList<string> listenerScopeQueries { get { return _listenerScopeQueries; } }


        public void InitXAMLInstances()
        {
            PenButton = PenButton_Instance;
            ArrowButton = ArrowButton_Instance;
            EraseButton = EraseButton_Instance;            

            CutButton = CutButton_Instance;
            CopyButton = CopyButton_Instance;
            PasteButton = PasteButton_Instance;

            ZoomScrollView = ZoomScrollView_Instance;
        }

        public MelodyFlowVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            InitializeComponent();

            //

            MinusZero mz = MinusZero.Instance;

            VisualiserName = "MelodyFlowVisuliser";

            BaseEdgeToMetaVertex = mz.root.Get(false, @"System\Lib\Music\Generator\Class:MelodyFlow");
            VisualiserMetaVertex = mz.root.Get(false, @"System\Meta\Visualiser\MelodyFlow");

            //

            InitXAMLInstances();

            PositionMarkEnabled = true;

            PositionMark = -1000;

            //

            HasDown = true;

            //

            ZoomScrollViewBasedVisualiserBase_Init(baseEdgeVertex, parentVisualiser);

            ShowCCList = false;

            NewItemWidthOneSnapLimit = true;
        }

        bool needToVisualiserDraw = false;

        protected override IEdge AddItemVertex(AxisSegment itemSegment, double startPosition, double lengthPosition)
        {
            int step = ScreenPositionToMusicTime(startPosition, true);

            IEdge newEdge = base.AddItemVertex(itemSegment, startPosition, lengthPosition);

            string meta = newEdge == null || newEdge.Meta == null ? "null" : newEdge.Meta.ToString();

            LogPen("AddItemVertex step=" + step
                + " from=" + DescribeVertex(newEdge == null ? null : newEdge.From)
                + " meta=" + meta
                + " to=" + DescribeVertex(newEdge == null ? null : newEdge.To));

            return newEdge;
        }

        protected override void PerformPenUp_part2()
        {
            LogPen("PerformPenUp_part2 before commit " + CanvasStateText());

            base.PerformPenUp_part2();
        }

        protected override INoInEdgeInOutVertexVertex CheckBaseEdgeChange(IExecution exe)
        {
            needToVisualiserDraw = false;

            IVertex baseEdgeTo = VisualiserHelper.Vertex.Get(false, @"BaseEdge:\To:");

            LogCheckBaseEdgeChangeStack(exe, baseEdgeTo);

            ExecutionFlowHelper.DoAddRemoveDisposeAddEdgeByMetaOrValueChangeHandlers(exe.Stack, new List<EventHandlers>()
            { new EventHandlers(
                baseEdgeTo,
                EdgeAdded,
                EdgeRemoved,
                EdgeDisposed,
                new string[] { },
                new string[] {"Velocity", "Octave", "Note", "Number", "Value"},
                new string[] {"Step", "Quant", "ControlChange" },
                AddEdgeByMetaOrValueChangeHandler
                )
            });

            if (needToVisualiserDraw)            
                DoStepsCleanUpAndVisualiserDraw();

            LogPen("CheckBaseEdgeChange end needToVisualiserDraw=" + needToVisualiserDraw
                + " " + CanvasStateText());

            return exe.Stack;
        }

        protected override void EdgeAdded(IEdge edge)
        {
            string meta = edge == null || edge.Meta == null ? "null" : edge.Meta.ToString();
            bool isStepToQuant = Flow != null && meta == Flow.StepToQuantMeta;

            LogPen("EdgeAdded meta=" + meta + " isStepToQuant=" + isStepToQuant);

            base.EdgeAdded(edge);
        }

        protected void AddEdgeByMetaOrValueChangeHandler(IEdge eventEdge)
        {
            needToVisualiserDraw = true;

            string meta = eventEdge == null || eventEdge.Meta == null ? "null" : eventEdge.Meta.ToString();

            LogPen("AddEdgeByMetaOrValueChangeHandler meta=" + meta + " needToVisualiserDraw=true");
        }

        void LogCheckBaseEdgeChangeStack(IExecution exe, IVertex baseEdgeTo)
        {
            if (exe == null || exe.Stack == null)
            {
                LogPen("CheckBaseEdgeChange stack is null baseEdgeTo=" + DescribeVertex(baseEdgeTo));
                return;
            }

            int eventCount = 0;
            int fromBaseEdgeToCount = 0;
            int quantMetaCount = 0;
            string quantMeta = Flow == null ? "Quant" : Flow.StepToQuantMeta;

            foreach (IEdge stackEventEdge in GraphUtil.GetQueryOut(exe.Stack, "event", null))
            {
                IVertex eventVertex = stackEventEdge.To;
                string eventType = "null";
                IVertex edgeFrom = null;
                IVertex edgeMeta = null;
                IVertex edgeTo = null;

                if (eventVertex != null)
                {
                    eventType = DescribeVertex(eventVertex.Get(false, "Type:"));
                    edgeFrom = eventVertex.Get(false, @"Edge:\From:");
                    edgeMeta = eventVertex.Get(false, @"Edge:\Meta:");
                    edgeTo = eventVertex.Get(false, @"Edge:\To:");
                }

                bool fromIsBaseEdgeTo = edgeFrom != null && edgeFrom == baseEdgeTo;
                string metaValue = edgeMeta == null || edgeMeta.Value == null ? "null" : edgeMeta.Value.ToString();

                if (fromIsBaseEdgeTo)
                    fromBaseEdgeToCount++;

                if (metaValue == quantMeta)
                    quantMetaCount++;

                LogPen("event[" + eventCount + "]"
                    + " type=" + eventType
                    + " fromIsBaseEdgeTo=" + fromIsBaseEdgeTo
                    + " from=" + DescribeVertex(edgeFrom)
                    + " meta=" + metaValue
                    + " to=" + DescribeVertex(edgeTo));

                eventCount++;
            }

            LogPen("CheckBaseEdgeChange summary events=" + eventCount
                + " fromBaseEdgeTo=" + fromBaseEdgeToCount
                + " metaQuant=" + quantMetaCount
                + " baseEdgeTo=" + DescribeVertex(baseEdgeTo));
        }

        string CanvasStateText()
        {
            int itemsCount = Items == null ? -1 : Items.Count;
            int mainChildrenCount = Main == null ? -1 : Main.Children.Count;

            return "Items.Count=" + itemsCount + " Main.Children.Count=" + mainChildrenCount;
        }

        void LogPen(string what)
        {
            MinusZero.Instance.Log(1, "MelodyFlowPen", what);
        }

        static string DescribeVertex(IVertex vertex)
        {
            if (vertex == null)
                return "null";

            string identity = "hash=" + vertex.GetHashCode();

            if (vertex.Store != null && vertex.Store.Identifier != null && vertex.Identifier != null)
                identity = GraphUtil.GetVertexIdString(vertex) + " " + identity;

            object value = vertex.Value;

            return identity + " value=" + (value == null ? "null" : value.ToString());
        }
    }
}
