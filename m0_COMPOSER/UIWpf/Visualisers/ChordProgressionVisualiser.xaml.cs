using m0;
using m0.Foundation;
using m0.Graph;
using m0.UIWpf;
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
    public partial class ChordProgressionVisualiser : MelodyFlowVisualiserBase
    {
        static string[] _MetaTriggeringUpdateVertex = new string[] { "ShowArrowLines:", "ShowSnapLines:", "ShowLabel:"};
        public override string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { };
        public override string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

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

        public ChordProgressionVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser)
        {
            InitializeComponent();

            //

            MinusZero mz = MinusZero.Instance;

            VisualiserName = "ChordProgressionVisuliser";

            BaseEdgeToMetaVertex = mz.root.Get(false, @"System\Lib\Music\Generator\Class:ChordProgression");
            VisualiserMetaVertex = mz.root.Get(false, @"System\Meta\Visualiser\ChordProgression");

            //

            InitXAMLInstances();

            PositionMarkEnabled = true;

            PositionMark = -1000;

            //

            HasDown = false;

            //

            ZoomScrollViewBasedVisualiserBase_Init(baseEdgeVertex, parentVisualiser);

            ShowCCList = false;

            NewItemWidthOneSnapLimit = true;
        }

        protected override void UpdateVertexValues()
        {
            IVertex r = MinusZero.Instance.root;            

            bool dummy = false;

            ShowLabel = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowLabel:"), ref dummy);            
            ShowArowLines = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowArrowLines:"), ref dummy);            
            

            VisualiserDraw();
        }        

        protected override void CreateFlow()
        {
            Flow = new ChordProgressionFlow(VisualizedVertex);
        }

        protected override void UpdateVariablesFromBaseVertex()
        {
            VisualizedVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (VisualizedVertex == null)
                return;

            if (VisualizedVertex.Get(false, "$Is:ChordProgression") == null)
            {
                VisualizedVertex = null;
                return;
            }

            CreateFlow();

            Length = Flow.GetNumberOfSteps();

            IVertex r = MinusZero.Instance.Root;

            verticalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultOneOctavePitchSet:");            
            
            horizontalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultNumberSpanLevel:");
        }
    }
}
