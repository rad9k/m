using m0;
using m0.Foundation;
using m0.Graph;
using m0.UIWpf;
using m0.UIWpf.Controls;
using m0.UIWpf.Visualisers;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroUML;
using m0_COMPOSER.Lib;
using m0_COMPOSER.UIWpf.Visualisers.Control;
using m0_COMPOSER.UIWpf.Visualisers.Control.Item;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    /// <summary>
    /// Interaction logic for SequenceVisualiser.xaml
    /// </summary>
    public partial class SequenceVisualiser : ZoomScrollViewBasedVisualiserBase
    {
        public void InitXAMLInstances()
        {
            PenButton = PenButton_Instance;
            ArrowButton = ArrowButton_Instance;
            EraseButton = EraseButton_Instance;
            TruncateButton = TruncateButton_Instance;
            ExtendButton = ExtendButton_Instance;

            ZoomScrollView = ZoomScrollView_Instance;
        }

        public SequenceVisualiser()
        {
            InitializeComponent();

            InitXAMLInstances();

            //

            HasDown = true;

            //

            ZoomScrollViewBasedVisualiserBase_Init();
        }

        protected override void UpdateVertexValues()
        {
            IVertex r = MinusZero.Instance.root;            

            bool dummy = false;

            ShowLabel = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowLabel:"), ref dummy);
            ShowVelocity = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowVelocity:"), ref dummy);
            ShowArowLines = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowArrowLines:"), ref dummy);
            ShowSnapLines = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowSnapLines:"), ref dummy);
            DefaultVelocity = GraphUtil.GetIntegerValue(Vertex.Get(false, "DefaultVelocity:"), ref dummy);

            if (Vertex.Get(false, "SnapToGrid:") == null || Vertex.Get(false, "SnapToGrid:").Value.ToString() == "")
                GraphUtil.ReplaceEdge(Vertex, r.Get(false, @"System\Meta\Visualiser\Sequence\SnapToGrid"), r.Get(false, @"System\Meta\Visualiser\SnapToGridEnum\'1 bar'"));

            SnapToGridComboBox_SelectionChange();
        }

        protected override void UpdateVariablesFromBaseVertex()
        {
            baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (baseVertex == null)
                return;

            if (baseVertex.Get(false, "$Is:Sequence") == null)
            {
                baseVertex = null;
                return;
            }

            IVertex r = MinusZero.Instance.Root;

            verticalSpanVertex = baseVertex.Get(false, "PitchSet:");

            if (verticalSpanVertex == null)
                if (IsDrum)
                    verticalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultDrumPitchSet:");
                else
                    verticalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultPitchSet:");

            horizontalSpanVertex = baseVertex.Get(false, "TimeSpan:");

            if (horizontalSpanVertex == null)
                horizontalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultTimeSpanLevel:");
        }

    }
}
