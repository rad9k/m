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

        protected override INoInEdgeInOutVertexVertex CheckBaseEdgeChange(IExecution exe)
        {
            needToVisualiserDraw = false;

            IVertex baseEdgeTo = VisualiserHelper.Vertex.Get(false, @"BaseEdge:\To:");

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

            return exe.Stack;
        }

        protected void AddEdgeByMetaOrValueChangeHandler(IEdge eventEdge)
        {
            needToVisualiserDraw = true;            
        }
    }
}
