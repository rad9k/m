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
using m0_COMPOSER.Midi;
using m0_COMPOSER.Base;
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

        int Position;

        double Tempo;

        protected double ExtendTimeLength_Song;

        IVertex postionAttribute;

        //

        bool ShowToolbarNames;

        //
        void InitXAMLInstances()
        {
            PenButton = PenButton_Instance;
            ArrowButton = ArrowButton_Instance;
            EraseButton = EraseButton_Instance;
            GlueButton = GlueButton_Instance;
            ScissorsButton = ScissorsButton_Instance;

            TruncateButton = TruncateButton_Instance;
            ExtendButton = ExtendButton_Instance;

            ZoomScrollView = ZoomScrollView_Instance;
        }

        void SetPosition(int newPosition)
        {
            GraphUtil.SetVertexValue(Vertex, postionAttribute, newPosition);            
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

        void SetupHelperVariables()
        {
            IVertex r = MinusZero.Instance.root;

            postionAttribute = r.Get(false, @"System\Lib\Music\Song\Position");            
        }

        public SongVisualiser()
        {
            InitializeComponent();

            //

            MinusZero mz = MinusZero.Instance;

            VisualiserName = "SongVisuliser";

            BaseEdgeToMetaVertex = mz.root.Get(false, @"System\Lib\Music\Class:Song");
            VisualiserMetaVertex = mz.root.Get(false, @"System\Meta\Visualiser\Song");

            //

            SetupHelperVariables();

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

        void InitializeSongVertex()
        {
            SetPosition(0);
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
            ShowToolbarNames = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowToolbarNames:"), ref dummy);

            if (Vertex.Get(false, "SnapToGrid:") == null || Vertex.Get(false, "SnapToGrid:").Value.ToString() == "")
                GraphUtil.ReplaceEdge(Vertex, r.Get(false, @"System\Meta\Visualiser\Song\SnapToGrid"), r.Get(false, @"System\Meta\Visualiser\SnapToGridEnum\'1 bar'"));

            SnapToGridComboBox_SelectionChange();

            ShowToolbarNames_SelectionChange();

            InitializeSongVertex();
        }

        protected void SetButtonComponentName(ContentControl c, string text)
        {
            StackPanel s = (StackPanel)c.Content;

            TextBlock t = (TextBlock)s.Children[1];

            t.Text = text;
        }

        protected void ShowToolbarNames_SelectionChange()
        {
            if (ShowToolbarNames)
            {
                SetButtonComponentName(PenButton, "New");
                SetButtonComponentName(ArrowButton, "Select");
                SetButtonComponentName(EraseButton, "Erase");
                SetButtonComponentName(GlueButton, "Merge");
                SetButtonComponentName(ScissorsButton, "Cut");

                SetButtonComponentName(RewindButton, "Rewind");
                SetButtonComponentName(PlayButton, "Play");
                SetButtonComponentName(RecordButton, "Record");
                SetButtonComponentName(StopButton, "Stop");
                SetButtonComponentName(RepeatButton, "Repeat");

                SetButtonComponentName(TruncateButton, "Truncate");
                SetButtonComponentName(ExtendButton, "Extend");                
            }
            else
            {
                SetButtonComponentName(PenButton, "");
                SetButtonComponentName(ArrowButton, "");
                SetButtonComponentName(EraseButton, "");
                SetButtonComponentName(GlueButton, "");
                SetButtonComponentName(ScissorsButton, "");

                SetButtonComponentName(RewindButton, "");
                SetButtonComponentName(PlayButton, "");
                SetButtonComponentName(RecordButton, "");
                SetButtonComponentName(StopButton, "");
                SetButtonComponentName(RepeatButton, "");

                SetButtonComponentName(TruncateButton, "");
                SetButtonComponentName(ExtendButton, "");
            }
        }

        protected override void UpdateVariablesFromBaseVertex()
        {
            baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (baseVertex == null)
                return;

            if (baseVertex.Get(false, "$Is:Song") == null)
            {
                baseVertex = null;
                return;
            }

            IVertex r = MinusZero.Instance.Root;

            verticalSpanVertex = baseVertex;            

            horizontalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultRealTimeSpanLevel:");            
        }

        protected override void SetAxisDecorators()
        {
            if (VerticalAD == null)
            {
                VerticalAD = new PitchSetAxisDecorator();

                VerticalAD.SetBaseVertex(verticalSpanVertex);
            }

            if (HorizontalAD == null)
            {
                RealTimeSpanAxisDecorator TimeSpanAD = new RealTimeSpanAxisDecorator();                

                HorizontalAD = TimeSpanAD;

                HorizontalAD.SetBaseVertex(horizontalSpanVertex);

                HorizontalAD.SetLength(Length);
            }

            ZoomScrollView.SetVerticalAxisDecorator(VerticalAD);

            ZoomScrollView.SetHorizontalAxisDecorator(HorizontalAD);
        }

        int GetMusicTimeFromRealTime(double minutes)
        {
            RealTime rt = new RealTime();
            rt.Minutes = minutes;

            return rt.GetMusicTime(Tempo).Combined;
        }

        protected override void SetupLocalVariablesFromBaseVertexVertexes()
        {
            bool dummy = false;

            Tempo = GraphUtil.GetDoubleValue(baseVertex.Get(false, "Tempo:"), ref dummy);

            if (baseVertex.Get(false, "ExtendTimeLength:") != null)
                ExtendTimeLength_Song = (int)GraphUtil.GetIntegerValue(baseVertex.Get(false, "ExtendTimeLength:"));
            else
                ExtendTimeLength_Song = 1; // default - 1 minute

            if (baseVertex.Get(false, "Length:") != null)
                Length = (int)GraphUtil.GetIntegerValue(baseVertex.Get(false, "Length:"));
            else
                Length = GetMusicTimeFromRealTime(ExtendTimeLength_Song);

            SaveLength();            

            IsDrum = GraphUtil.GetBooleanValue(baseVertex.Get(false, "IsDrum:"), ref dummy);

            if (IsDrum)
                IsCurrentPenItemCenter = true;
        }

        protected override void TruncateButton_Click(object sender, RoutedEventArgs e)
        {
            if ((Length - ExtendTimeLength) <= 0)
                return;

            Length -= GetMusicTimeFromRealTime(ExtendTimeLength_Song);

            SaveLength();

            HorizontalAD.SetLength(Length);

            VisualiserDraw();
        }

        protected override void ExtendButton_Click(object sender, RoutedEventArgs e)
        {
            Length += GetMusicTimeFromRealTime(ExtendTimeLength_Song);

            SaveLength();

            HorizontalAD.SetLength(Length);

            VisualiserDraw();
        }

        protected void GlueButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorStateEnum.Glue);
        }

        protected void ScissorsButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorStateEnum.Scissors);
        }

        protected override void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if (VertexChangeOff)
                return;

            if ((sender == Vertex.Get(false, "ShowToolbarNames:")) && (e.Type == VertexChangeType.ValueChanged))
                UpdateBaseEdge();

            base.VertexChange(sender, e);
        }
    }
}

