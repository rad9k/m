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
    public partial class MelodyFlowVisualiser : MelodyFlowVisualiserBase
    {
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

        public MelodyFlowVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser)
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
    }
}
