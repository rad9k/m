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
        enum PlayRecordStateEnum { Stop, Play, Record }

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

        Button newTrackButton;

        private void CreateAddNewTrackControl()
        {
            newTrackButton = new Button();

            newTrackButton.Content = "+ new track";

            newTrackButton.Foreground = (Brush)FindResource("0ForegroundBrush");

            newTrackButton.Style = (Style)Application.Current.FindResource("TransparentStyle");

            newTrackButton.BorderThickness = new Thickness(0);
            newTrackButton.Margin = new Thickness(0);
            newTrackButton.Padding = new Thickness(0);

            newTrackButton.Click += NewTrackButton_Click;

            ZoomScrollView.SetLeftDownCornerControl(newTrackButton);
        }

        int trackCnt = 1;

        private void NewTrackButton_Click(object sender, RoutedEventArgs e)
        {
            IVertex r = MinusZero.Instance.root;

            IVertex v = VertexOperations.AddInstance(baseVertex, r.Get(false, @"System\Lib\Music\Track"));

            v.Value = "Track " + trackCnt;
            trackCnt++;

            //newTrackButton.Background = (Brush)FindResource("0ForegroundBrush"); // fix to some system bug?

            //MinusZero.Instance.DefaultUserInteraction.EditDialog(v, null);            
        }

        private void RedrawTracks()
        {
            VerticalAD.SetBaseVertex(baseVertex);

            DrawMain();
        }

        private void CreateSongControls()
        {
            CreateAddNewTrackControl();
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
                GraphUtil.ReplaceEdge(Vertex, r.Get(false, @"System\Meta\Visualiser\Song\SnapToGrid"), r.Get(false, @"System\Meta\Visualiser\SongSnapToGridEnum\'1 bar'"));

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

        protected void InitialiseBaseVertexBasedVisualiserControls()
        {
            TempoVisualiser.BaseEdge = GraphUtil.GetQueryOutFirstEdge(baseVertex, "Tempo", null);
        }

        IVertex previousBaseVertex;

        protected override void UpdateVariablesFromBaseVertex()
        {
            baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (baseVertex == null || baseVertex == previousBaseVertex)
                return;

            if (previousBaseVertex != null)
                PlatformClass.RemoveVertexChangeListeners_byGenericVertex(previousBaseVertex, new VertexChange(VertexChange_BaseEdge));

            if (baseVertex.Get(false, "$Is:Song") == null)
            {
                baseVertex = null;
                return;
            }

            previousBaseVertex = baseVertex;

            PlatformClass.RegisterVertexChangeListeners_byGenericVertex(baseVertex, new VertexChange(VertexChange_BaseEdge), new string[] { "Tempo", "Track" });


            IVertex r = MinusZero.Instance.Root;

            horizontalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultRealTimeSpanLevel:");

            InitialiseBaseVertexBasedVisualiserControls();

            AddChangeListenersToAllTracks();
        }

        void AddChangeListenersToAllTracks()
        {
            foreach (IEdge e in baseVertex.GetAll(false, "Track:"))
                AddChangeListenersToTrack(e.To);
        }

        void RemoveChangeListenersToAllTracks()
        {
            foreach (IEdge e in baseVertex.GetAll(false, "Track:"))
                PlatformClass.RemoveVertexChangeListeners_byGenericVertex(e.To, new VertexChange(VertexChange_Track));
        }

        void AddChangeListenersToTrack(IVertex v)
        {
            PlatformClass.RemoveVertexChangeListeners_byGenericVertex(v, new VertexChange(VertexChange_Track));
            PlatformClass.RegisterVertexChangeListeners_byGenericVertex(v, new VertexChange(VertexChange_Track), new string[] { "Sequence", "Color" });
        }

        protected override void SetAxisDecorators()
        {
            if (VerticalAD == null)
            {
                VerticalAD = new TrackAxisDecorator();

                VerticalAD.SetBaseVertex(baseVertex);
            }

            if (HorizontalAD == null)
            {
                RealTimeSpanAxisDecorator TimeSpanAD = new RealTimeSpanAxisDecorator();

                TimeSpanAD.BoldLineCount = 10;

                HorizontalAD = TimeSpanAD;

                HorizontalAD.SetBaseVertex(horizontalSpanVertex);

                UpdateHorizontalADLength();
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

        double GetRealTimeFromMusicTime(int length)
        {
            MusicTime mt = new MusicTime();

            mt.Combined = length;

            return mt.GetRealTime(Tempo).Minutes;
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

            UpdateHorizontalADLength();

            VisualiserDraw();
        }

        protected override void ExtendButton_Click(object sender, RoutedEventArgs e)
        {
            Length += GetMusicTimeFromRealTime(ExtendTimeLength_Song);

            SaveLength();

            UpdateHorizontalADLength();

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

        protected void VertexChange_Track(object sender, VertexChangeEventArgs e)
        {
            if (!(sender is IVertex))
                return;

            //if (GraphUtil.DoEdgeListContainsVertex(baseVertex.GetAll(false, "Track:"), (IVertex)sender))

            RedrawTracks();
        }

        protected void VertexChange_BaseEdge(object sender, VertexChangeEventArgs e)
        {
            if (!(sender is IVertex))
                return;

            IVertex senderVertex = (IVertex)sender;

            if ((sender == baseVertex.Get(false, "Tempo:")) && (e.Type == VertexChangeType.ValueChanged))
                UpdateTempo();

            if (GraphUtil.DoEdgeListContainsVertex(baseVertex.GetAll(false, "Track:"), senderVertex) && (e.Type == VertexChangeType.ValueChanged))
                RedrawTracks();

            if (GraphUtil.DoEdgeListContainsVertex(baseVertex.GetAll(false, "Track:"), senderVertex) && (e.Type == VertexChangeType.EdgeAdded))
                AddChangeListenersToTrack(senderVertex);
        }

        void UpdateHorizontalADLength()
        {
            double RealTimeLength = GetRealTimeFromMusicTime(Length);

            HorizontalAD.SetLength(RealTimeLength);
        }

        protected void UpdateTempo()
        {
            bool dummy = false;

            Tempo = GraphUtil.GetDoubleValue(baseVertex.Get(false, "Tempo:"), ref dummy);

            if (HorizontalAD != null)
            {
                UpdateHorizontalADLength();

                VisualiserDraw();
            }
        }

        public override void ChildControlsLoaded()
        {
            CreateSongControls();

            base.ChildControlsLoaded();
        }

        public override void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;

                DispachAllSubVisualisers();

                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                RemoveChangeListenersToAllTracks();

                // PlatformClass.RemoveVertexChangeListeners_byGenericVertex(baseVertex, new VertexChange(VertexChange_BaseEdge));

                if (Vertex is IDisposable)
                    ((IDisposable)Vertex).Dispose();
            }
        }

        protected override void SnapToGridComboBox_SelectionChange()
        {
            switch (Vertex.Get(false, "SnapToGrid:").Value.ToString())
            {
                case "1 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1;
                    CurrentSnapToGridValue = 1;
                    break;

                case "1/2 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_2;
                    CurrentSnapToGridValue = 1.0 / 2;
                    break;

                case "1/4 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_4;
                    CurrentSnapToGridValue = 1.0 / 4;
                    break;

                case "1/8 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_8;
                    CurrentSnapToGridValue = 1.0 / 8;
                    break;

                case "1/16 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_16;
                    CurrentSnapToGridValue = 1.0 / 16;
                    break;

                case "1/32 bar":
                    CurrentSnapToGrid = SnapToGridEnum.Bar1_32;
                    CurrentSnapToGridValue = 1.0 / 32;
                    break;

                case "no snap":
                    CurrentSnapToGrid = SnapToGridEnum.No_Snap;
                    CurrentSnapToGridValue = 0;
                    break;
            }

            VisualiserDraw();
        }

        protected override double GetSnappedPosition(double position)
        {
            if (CurrentSnapToGrid == SnapToGridEnum.No_Snap)
                return position;

            double snapMinimalWidth = GetSnapMinimalWidth();

            double numberOfSnaps = position / snapMinimalWidth;

            double numberOfSnapsFloor = Math.Floor(numberOfSnaps);

            double rest = position - (numberOfSnapsFloor * snapMinimalWidth);

            if (rest < (snapMinimalWidth / 2))
                return numberOfSnapsFloor * snapMinimalWidth;
            else
                return (numberOfSnapsFloor + 1) * snapMinimalWidth;
        }

        protected override double GetSnapMinimalWidth()
        {
            if (CurrentSnapToGridValue == 0)
                return 1;

            double minuteWidth = HorizontalAD.SegmentLength * HorizontalAD.BaseUnitSize;

            int snapSize_Music = (int) (CurrentSnapToGridValue * Midi.Standard.MidiTicksPerSixteen * 16);

            double snapSize_Real = GetRealTimeFromMusicTime(snapSize_Music);

            return snapSize_Real * minuteWidth;
        }

        int GetSequenceEventTriggerTime(IVertex sequenceEventVertex)
        {
            bool dummy = false;

            return GraphUtil.GetIntegerValue(sequenceEventVertex.Get(false, "TriggerTime:"), ref dummy);
        }

        int GetSequenceEventLength(IVertex sequenceEventVertex)
        {
            bool dummy = false;

            return GraphUtil.GetIntegerValue(sequenceEventVertex.Get(false, @"Sequence:\Length:"), ref dummy);
        }

        void SetSequenceEventTriggerTime(IVertex sequenceEventVertex, int value)
        {
            GraphUtil.SetVertexValue(sequenceEventVertex, MinusZero.Instance.root.Get(false, @"System\Lib\Music\SequenceEvent\TriggerTime"), value);            
        }

        void SetSequenceEventLength(IVertex sequenceEventVertex, int value)
        {
            IVertex sequenceVertex = sequenceEventVertex.Get(false, @"Sequence:");

            GraphUtil.SetVertexValue(sequenceVertex, MinusZero.Instance.root.Get(false, @"System\Lib\Music\Sequence\Length"), value);
        }

        public IVertex GetTrackVertexFromSequenceEventVertex(IVertex sequenceEventVertex)
        {
            foreach (IEdge e in baseVertex.GetAll(false, @"Track:"))
                foreach (IEdge ee in e.To)
                    if (ee.To == sequenceEventVertex)
                        return e.To;

            return null;
        }

        protected override void AddItem(IEdge itemEdge, List<IVertex> selectedVertexes)
        {
            IVertex itemEventVertex = itemEdge.To;

            bool dummy = false;

            int triggerTime = GetSequenceEventTriggerTime(itemEventVertex);

            int length = GetSequenceEventLength(itemEventVertex);

            SequenceEventItem newElement = new SequenceEventItem(itemEdge, this);            

            if (selectedVertexes != null && selectedVertexes.Contains(itemEventVertex))
            {
                newElement.Select();
                PreviousSelectedItemContext = MainDownEnum.Main;
            }

            IVertex trackVertex = GetTrackVertexFromSequenceEventVertex(itemEdge.To);

            AxisSegment itemSegment = GetVerticalSegment(trackVertex);


            bool performSnapCorrection = false;

            if (CurrentCursorState == CursorStateEnum.PenUp)
                performSnapCorrection = true;

            double startPosition = MusicTimeToScreenPosition(triggerTime, performSnapCorrection);

            double endPosition = startPosition + MusicTimeToScreenPosition(length, performSnapCorrection);

            
            newElement.Left = startPosition;
            newElement.Top = itemSegment.StartPosition;
            newElement.Right = endPosition;
            newElement.Bottom = itemSegment.EndPosition;            

            ItemsAdd(newElement);            
        }

        int ScreenPositionToMusicTime(double position)
        {
            double minuteWidth = HorizontalAD.SegmentLength * HorizontalAD.BaseUnitSize;

            double positionInMinutes = position / minuteWidth;

            int beforeCorrection =  GetMusicTimeFromRealTime(positionInMinutes);

            return MusicTimeSnapCorrect(beforeCorrection);
        }

        double MusicTimeToScreenPosition(int musicTime, bool performSnapCorrection)
        {
            if(performSnapCorrection)
                musicTime = MusicTimeSnapCorrect(musicTime);

            double realTime = GetRealTimeFromMusicTime(musicTime);

            double minuteWidth = HorizontalAD.SegmentLength * HorizontalAD.BaseUnitSize;            

            return realTime * minuteWidth;
        }

        protected override IEdge AddItemEdge(AxisSegment itemSegment, double startPosition, double lengthPosition)
        {
            IVertex r = MinusZero.Instance.Root;


            IVertex toAddVertex = itemSegment.BaseVertex;


            IVertex sequenceEventAttribute = r.Get(false, @"System\Lib\Music\Track\SequenceEvent");

            IVertex sequenceEvent = r.Get(false, @"System\Lib\Music\SequenceEvent");

            IVertex sequence = r.Get(false, @"System\Lib\Music\Sequence");


            IEdge tempSequenceEventEdge = toAddVertex.AddVertexAndReturnEdge(null, null);

            IVertex sequenceEventVertex = tempSequenceEventEdge.To;


            sequenceEventVertex.AddEdge(MinusZero.Instance.Is, sequenceEvent);
            
            sequenceEventVertex.AddVertex(sequenceEvent.Get(false, @"Attribute:TriggerTime"), ScreenPositionToMusicTime(startPosition));

            IVertex sequenceVertex = VertexOperations.AddInstance(sequenceEventVertex, sequence);

            sequenceVertex.AddVertex(sequence.Get(false, @"Attribute:Length"), ScreenPositionToMusicTime(lengthPosition));


            IEdge finalEdge = toAddVertex.AddEdge(sequenceEventAttribute, sequenceEventVertex);

            toAddVertex.DeleteEdge(tempSequenceEventEdge);   
            

            return finalEdge;
        }

        protected override void DrawItems()
        {
            List<IVertex> selectedVertexes = GetSelectedVertexes();

            foreach (IEdge e in baseVertex.GetAll(false, "Track:"))
                foreach (IEdge ee in e.To.GetAll(false, "SequenceEvent:"))                
                    AddItem(ee, selectedVertexes);
        }

        protected override void UpdateItem_VerticalPosition(IItem item)
        {            
            FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            IVertex itemVertex = item.BaseEdge.To;

            AxisSegment segment = FindVerticalSegment(item.Top + 1);

            IVertex newTrack = segment.BaseVertex;

            IVertex oldTrack = GetTrackVertexFromSequenceEventVertex(itemVertex);

            IVertex sequnceEventMeta = MinusZero.Instance.root.Get(false, @"System\Lib\Music\Track\SequenceEvent");

            newTrack.AddEdge(sequnceEventMeta, itemVertex);

            GraphUtil.DeleteEdge(oldTrack, sequnceEventMeta, itemVertex);
        }

        protected override void UpdateItem_HorizontalPosition(IItem item)
        {            
            FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            IVertex itemVertex = item.BaseEdge.To;

            double itemWidth = element.Width;

            int TriggerTime = ScreenPositionToMusicTime(item.Left);

            int Length = ScreenPositionToMusicTime(itemWidth);                        

            SetSequenceEventTriggerTime(itemVertex, TriggerTime);

            if (Length != 0)
                SetSequenceEventLength(itemVertex, Length);                
        }
    }
}

