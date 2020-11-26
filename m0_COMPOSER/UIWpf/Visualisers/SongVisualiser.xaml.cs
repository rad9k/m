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
    public partial class SongVisualiser : ZoomScrollViewBasedVisualiserBase
    {
        enum PlayRecordStateEnum { Stop, Play, Record}

        PlayRecordStateEnum PlayRecordState;

        bool isRepeat = false;

        //
        void InitXAMLInstances()
        {
            PenButton = PenButton_Instance;
            ArrowButton = ArrowButton_Instance;
            EraseButton = EraseButton_Instance;
            TruncateButton = TruncateButton_Instance;
            ExtendButton = ExtendButton_Instance;

            ZoomScrollView = ZoomScrollView_Instance;
        }

        void InitSongState()
        {
            SetPlayRecordState(PlayRecordStateEnum.Stop);
        }

        void SetPlayRecordState(PlayRecordStateEnum toBeState)
        {
            switch (toBeState)
            {
                case PlayRecordStateEnum.Stop:
                    PlayRecordState = PlayRecordStateEnum.Stop;
                    PlayButton.IsChecked = false;
                    RecordButton.IsChecked = false;
                    break;

                case PlayRecordStateEnum.Play:
                    PlayRecordState = PlayRecordStateEnum.Play;
                    PlayButton.IsChecked = true;
                    RecordButton.IsChecked = false;
                    break;

                case PlayRecordStateEnum.Record:
                    PlayRecordState = PlayRecordStateEnum.Record;
                    PlayButton.IsChecked = false;
                    RecordButton.IsChecked = true;
                    break;
            }
        }

        public SongVisualiser()
        {
            InitializeComponent();

            InitXAMLInstances();            

            ZoomScrollViewBasedVisualiserBase_Init();

            //

            this.HasDown = false;

            //

            InitSongState();
        }

        private void RewindButton_Click(object sender, RoutedEventArgs e)
        {
            SetPlayRecordState(PlayRecordStateEnum.Stop);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            SetPlayRecordState(PlayRecordStateEnum.Play);
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            SetPlayRecordState(PlayRecordStateEnum.Record);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            SetPlayRecordState(PlayRecordStateEnum.Stop);
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (RepeatButton.IsChecked == true)
                isRepeat = true;
            else
                isRepeat = false;
        }

        //
        // overrides on ZoomScrollViewBasedVisualiserBase
        //
        //

        protected override void UpdateVertexValues()
        {
            IVertex r = MinusZero.Instance.root;

            bool dummy = false;

            ShowLabel = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowLabel:"), ref dummy);          
            ShowArowLines = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowArrowLines:"), ref dummy);
            ShowSnapLines = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowSnapLines:"), ref dummy);
          

            if (Vertex.Get(false, "SnapToGrid:") == null || Vertex.Get(false, "SnapToGrid:").Value.ToString() == "")
                GraphUtil.ReplaceEdge(Vertex, r.Get(false, @"System\Meta\Visualiser\Sequence\SnapToGrid"), r.Get(false, @"System\Meta\Visualiser\SnapToGridEnum\'1 bar'"));

            SnapToGridComboBox_SelectionChange();
        }
    }
}

