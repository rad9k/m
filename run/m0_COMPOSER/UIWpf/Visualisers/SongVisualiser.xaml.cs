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
using m0.ZeroCode;
using System.Windows.Threading;
using m0.Graph.ExecutionFlow;
using m0.User.Process.UX;
using m0.UIWpf.Visualisers.Helper;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    /// <summary>
    /// Interaction logic for SongVisualiser.xaml
    /// </summary>
    public partial class SongVisualiser : ZoomScrollViewBasedVisualiserBase, INoDownVisualiser
    {
        static string[] _MetaTriggeringUpdateVertex = new string[] { "SnapToGrid", "ShowLabel", "ShowArrowLines", "ShowSnapLines", "ShowToolbarNames"};
        public override string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { };
        public override string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        static IList<string> _listenerScopeQueries = new List<string> { @"", @"BaseEdge:\To:", @"BaseEdge:\To:\", @"BaseEdge:\To:\\", @"BaseEdge:\To:\\\" };
        protected override IList<string> listenerScopeQueries { get { return _listenerScopeQueries; } }


        public int AutoBackupMinutes = 1;
        DispatcherTimer AutoBackupTimer;

        static List<IVertex> AutoBackupVertexList = new List<IVertex>();

        enum PlayRecordStateEnum { Stop, Play, Record }

        PlayRecordStateEnum PlayRecordState;

        public bool IsRepeat = false;

        static IVertex r = MinusZero.Instance.Root;

        static IVertex positionAttributeMeta = r.Get(false, @"System\Lib\Music\Song\Position");

        static IVertex isRepeatMeta = r.Get(false, @"System\Lib\Music\Song\IsRepeat");
        

        double Tempo;

        protected double ExtendTimeLength_Song;

        //

        bool ShowToolbarNames;

        //

        protected Button MuteSpeakerButton;

        //

        void InitXAMLInstances()
        {
            PenButton = PenButton_Instance;
            ArrowButton = ArrowButton_Instance;
            EraseButton = EraseButton_Instance;

            CutButton = CutButton_Instance;
            CopyButton = CopyButton_Instance;
            PasteButton = PasteButton_Instance;

            GlueButton = GlueButton_Instance;
            RazorButton = RazorButton_Instance;

            TruncateButton = TruncateButton_Instance;
            ExtendButton = ExtendButton_Instance;

            ZoomScrollView = ZoomScrollView_Instance;

            MuteSpeakerButton = MuteSpeakerButton_Instance;
        }

        void SetPosition(int newPosition)
        {
            GraphUtil.SetVertexValue(VisualizedVertex, positionAttributeMeta, newPosition);
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

                    MuteAllOutput();

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

        public SongVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            InitializeComponent();

            //

            MinusZero mz = MinusZero.Instance;

            VisualiserName = "SongVisualiser";

            BaseEdgeToMetaVertex = mz.root.Get(false, @"System\Lib\Music\Class:Song");
            VisualiserMetaVertex = mz.root.Get(false, @"System\Meta\Visualiser\Song");

            //

            InitXAMLInstances();

            ZoomScrollViewBasedVisualiserBase_Init(baseEdgeVertex, parentVisualiser);

            PositionMarkEnabled = true;

            PositionMarkPrimEnabled = true;

            PositionMark = -1000;

            //

            this.HasDown = false;

            //

            InitSongState();            
        }

        // NEW START

        protected override INoInEdgeInOutVertexVertex CheckBaseEdgeChange(IExecution exe)
        {
            if (ExecutionFlowHelper.IsVertexChange(exe.Stack, "Tempo"))
                UpdateTempo(); // executes VisualiserDraw();
            else
                RedrawTracks();

            return exe.Stack;
        }

        /*
        protected override INoInEdgeInOutVertexVertex CheckBaseEdgeChange(IExecution exe)
        {
            IVertex baseEdgeTo = VisualiserHelper.Vertex.Get(false, @"BaseEdge:\To:");

            ExecutionFlowHelper.DoAddRemoveDisposeAddEdgeByMetaOrValueChangeHandlers(exe.Stack, new List<EventHandlers>()
            { new EventHandlers(
                baseEdgeTo,
                EdgeAdded,
                EdgeRemoved,
                EdgeDisposed,
                new string[] {"Track", "SequenceEvent"},
                new string[] {//"TriggerTime", "Length", "Velocity", 
                "Red", "Green", "Blue"},
                new string[] {//"SequenceEvent", "Sequence",
                "Color" },
                AddEdgeByMetaOrValueChangeHandler
                )
            });

            return exe.Stack;
        }

        protected override void EdgeAdded(IEdge edge)
        {         
        }

        protected void AddEdgeByMetaOrValueChangeHandler(IEdge eventEdge)
        {         
        }

        protected void VertexChange_Track(object sender, VertexChangeEventArgs e)
        {            
            if (!(sender is IVertex))
                return;

            if (GraphUtil.DoEdgeListContainsVertex(VisualizedVertex.GetAll(false, @"Track:\Color:"), (IVertex)sender)
                || GraphUtil.DoEdgeListContainsVertex(VisualizedVertex.GetAll(false, @"Track:\Color:\"), (IVertex)sender))
                RedrawTracks();

            if (GraphUtil.DoEdgeListContainsVertex(VisualizedVertex.GetAll(false, @"SequenceEvent:"), (IVertex)sender))
                RedrawTracks();
        }

        protected void VertexChange_BaseEdge(object sender, VertexChangeEventArgs e)
        {            
            if (!(sender is IVertex))
                return;

            IVertex senderVertex = (IVertex)sender;

            if ((sender == VisualizedVertex.Get(false, "Tempo:")) && (e.Type == VertexChangeType.ValueChanged))
                UpdateTempo();

            if (GraphUtil.DoEdgeListContainsVertex(VisualizedVertex.GetAll(false, "Track:"), senderVertex) && (e.Type == VertexChangeType.ValueChanged))
                RedrawTracks();

            if (GraphUtil.DoEdgeListContainsVertex(VisualizedVertex.GetAll(false, "Track:"), senderVertex) && (e.Type == VertexChangeType.EdgeAdded))
                AddChangeListenersToTrack(senderVertex);
        }
*/
        protected void UpdateItem(IEdge itemEdge, IItem item)
        {
            IVertex itemEventVertex = itemEdge.To;            

            int triggerTime = GetSequenceEventTriggerTime(itemEventVertex);

            int length = GetSequenceEventLength(itemEventVertex);

            bool performSnapCorrection = false;

            if (CurrentCursorState == CursorStateEnum.PenDown)
                performSnapCorrection = true;

            double startPosition = MusicTimeToScreenPosition(triggerTime, performSnapCorrection);

            double endPosition = startPosition + MusicTimeToScreenPosition(length, performSnapCorrection);

            IVertex trackVertex = Song.GetTrackVertexFromSequenceEventVertex(itemEdge.To);

            AxisSegment itemSegment = GetVerticalSegment(trackVertex);

            ((SequenceEventItem)item).TrackVertex = trackVertex;

            item.Left = startPosition;
            item.Top = itemSegment.StartPosition;
            item.Right = endPosition;
            item.Bottom = itemSegment.EndPosition;           

            item.Update();
        }

        protected override void AddItemByEdge(IEdge itemEdge, ISet<IVertex> selectedVertexes)
        {
            IVertex itemEventVertex = itemEdge.To;


            SequenceEventItem newElement = new SequenceEventItem(itemEdge, this, ShowLabel);

            UpdateItem(itemEdge, newElement);

            if (selectedVertexes != null && selectedVertexes.Contains(itemEventVertex))
            {
                newElement.SelectHighlight();
                PreviousSelectedItemContext = MainDownEnum.Main;
            }

            ItemsAdd(newElement);
        }

        protected override IEdge AddItemVertex(AxisSegment itemSegment, double startPosition_Screen, double lengthPosition_Screen)
        {
            IVertex trackVertex = itemSegment.BaseVertex;

            bool needsSnapCorrection = false;

            if (CurrentCursorState == CursorStateEnum.PenDown)
                needsSnapCorrection = true;

            int startPosition = ScreenPositionToMusicTime(startPosition_Screen, needsSnapCorrection);

            int lengthPosition = ScreenPositionToMusicTime(lengthPosition_Screen, needsSnapCorrection);

            return Song.AddSequenceEventVertex(trackVertex, startPosition, lengthPosition);
        }

        static IVertex sequnceEventMeta = MinusZero.Instance.root.Get(false, @"System\Lib\Music\Track\SequenceEvent");

        protected override void UpdateItem_VerticalPosition(IItem _item)
        {
            SequenceEventItem item;

            if (!(_item is SequenceEventItem))
                return;

            item = (SequenceEventItem)_item;

            IVertex itemVertex = _item.BaseEdge.To;

            AxisSegment segment = FindVerticalSegment(_item.Top + 1);

            IVertex newTrack = segment.BaseVertex;

            IVertex oldTrack = Song.GetTrackVertexFromSequenceEventVertex(itemVertex);

            if (newTrack != oldTrack)
            {
                newTrack.AddEdge(sequnceEventMeta, itemVertex);

                GraphUtil.DeleteEdge(oldTrack, sequnceEventMeta, itemVertex);

                item.TrackVertex = newTrack;
            }
        }

        protected override void UpdateItem_HorizontalPosition(IItem item)
        {
            FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            IVertex itemVertex = item.BaseEdge.To;

            double itemWidth = element.Width;

            int TriggerTime = ScreenPositionToMusicTime(item.Left, true);

            int Length = ScreenPositionToMusicTime(itemWidth, false);

            SetSequenceEventTriggerTime(itemVertex, TriggerTime);

            if (Length != 0)
                SetSequenceEventLength(itemVertex, Length);
        }

        protected override void UpdateVariablesFromBaseVertex()
        {
            VisualizedVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (VisualizedVertex == previousBaseVertex)
                return;

            if (VisualizedVertex == null)
            {
                baseVertexIsEmpty();
                return;
            }

            // if (previousBaseVertex != null)
            //    PlatformClass.RemoveVertexChangeListeners_byGenericVertex(previousBaseVertex, new VertexChange(VertexChange_BaseEdge), "SongVisualiser");

            if (VisualizedVertex.Get(false, "$Is:Song") == null)
            {
                VisualizedVertex = null;
                baseVertexIsEmpty();
                return;
            }

            previousBaseVertex = VisualizedVertex;

            // PlatformClass.RegisterVertexChangeListeners_byGenericVertex(VisualizedVertex, new VertexChange(VertexChange_BaseEdge), new string[] { "Tempo", "Track" }, "SongVisualiser");


            IVertex r = MinusZero.Instance.Root;

            horizontalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultRealTimeSpanLevel:");

            InitialiseBaseVertexBasedVisualiserControls();

            positionMarkPrim_Beg = GraphUtil.GetIntegerValueOr0(VisualizedVertex.Get(false, "LoopBeg:"));
            positionMarkPrim_End = GraphUtil.GetIntegerValueOr0(VisualizedVertex.Get(false, "LoopEnd:"));

            IsRepeat = GraphUtil.GetBooleanValueOrFalse(VisualizedVertex.Get(false, "IsRepeat:"));

            UpdateRepeatButton();            

            CheckOrStartAutoBackup();
        }        

        


        // NEW END

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

        public string GetNameForNewTrack()
        {
            int max = 0;

            foreach (IEdge e in VisualizedVertex.GetAll(false, @"Track:"))
            {
                string seqName = e.To.Value.ToString();

                string[] seqNameSplit = seqName.Split(' ');

                seqName = seqNameSplit[0];

                int tryMax;

                if (Int32.TryParse(seqName, out tryMax))
                    if (tryMax > max)
                        max = tryMax;

                if (seqNameSplit.Length > 1)
                {
                    seqName = seqNameSplit[1];

                    if (Int32.TryParse(seqName, out tryMax))
                        if (tryMax > max)
                            max = tryMax;
                }
            }

            max++;

            return "Track " + max;
        }

        private void NewTrackButton_Click(object sender, RoutedEventArgs e)
        {
            IVertex r = MinusZero.Instance.root;

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IVertex v = VertexOperations.AddInstance(VisualizedVertex, r.Get(false, @"System\Lib\Music\Track"), r.Get(false, @"System\Lib\Music\Song\Track"));

            v.Value = GetNameForNewTrack();

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            //newTrackButton.Background = (Brush)FindResource("0ForegroundBrush"); // fix to some system bug?

            MinusZero.Instance.UserInteraction.EditEdge(v);            
        }

        private void RedrawTracks()
        {
            if(VerticalAD != null)
                VerticalAD.SetBaseVertex(VisualizedVertex);

            UpdateMainSize();
            DrawMain();
        }

        private void RewindButton_Click(object sender, RoutedEventArgs e)
        {
            SetPosition(0);

            SetPlayRecordState(PlayRecordStateEnum.Stop);            
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (VisualizedVertex.Get(false, "Track:") != null)
            {
                SongVertexDictionary.SetSongVisualiser(VisualizedVertex, this);

                SetPlayRecordState(PlayRecordStateEnum.Play);

                IVertex playMethod = VisualizedVertex.Get(false, @"$Is:\Method:Play");

                ZeroCodeExecutonUtil.CreateExecutionAndVertexMethodExecute(playMethod, VisualizedVertex);
            }
            else
                StopButton_Click(null, null);
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            SetPlayRecordState(PlayRecordStateEnum.Record);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayRecordState == PlayRecordStateEnum.Play && VisualizedVertex.Get(false, "Track:") != null)
            {
                SongVertexDictionary.SetSongVisualiser(VisualizedVertex, this);

                SetPlayRecordState(PlayRecordStateEnum.Play);

                IVertex stopMethod = VisualizedVertex.Get(false, @"$Is:\Method:Stop");

                ZeroCodeExecutonUtil.CreateExecutionAndVertexMethodExecute(stopMethod, VisualizedVertex);
            }

            SetPlayRecordState(PlayRecordStateEnum.Stop);
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (RepeatButton.IsChecked == true)
                IsRepeat = true;
            else
                IsRepeat = false;

            GraphUtil.SetVertexValue(VisualizedVertex, isRepeatMeta, IsRepeat);
        }

        void UpdateRepeatButton()
        {
            if (IsRepeat)
                RepeatButton.IsChecked = true;
            else
                RepeatButton.IsChecked = false;
        }

        void MuteAllOutput()
        {
            if (VisualizedVertex != null)
            {
                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                foreach (IEdge outputEdge in VisualizedVertex.GetAll(false, @"Track:\Output:"))
                {
                    IVertex outputVertex = outputEdge.To;

                    IVertex silentMethod = outputVertex.Get(false, @"$Is:\Method:Silent");

                    ZeroCodeExecutonUtil.CreateExecutionAndVertexMethodExecute(silentMethod, outputVertex);
                }

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////
            }
        }

        private void MuteSpeakerButton_Click(object sender, RoutedEventArgs e)
        {
            MuteAllOutput();
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

        protected void ShowToolbarNames_SelectionChange()
        {
            if (ShowToolbarNames)
            {
                SetButtonComponentName(PenButton, "New");
                SetButtonComponentName(ArrowButton, "Select");
                SetButtonComponentName(EraseButton, "Erase");
                SetButtonComponentName(GlueButton, "Merge");
                SetButtonComponentName(RazorButton, "Razor");

                SetButtonComponentName(RewindButton, "Rewind");
                SetButtonComponentName(PlayButton, "Play");
                SetButtonComponentName(RecordButton, "Record");
                SetButtonComponentName(StopButton, "Stop");
                SetButtonComponentName(RepeatButton, "Repeat");

                SetButtonComponentName(CutButton, "Cut");
                SetButtonComponentName(CopyButton, "Copy");
                SetButtonComponentName(PasteButton, "Paste");

                SetButtonComponentName(TruncateButton, "Truncate");
                SetButtonComponentName(ExtendButton, "Extend");

                SetButtonComponentName(MuteSpeakerButton, "Silence");
            }
            else
            {
                SetButtonComponentName(PenButton, "");
                SetButtonComponentName(ArrowButton, "");
                SetButtonComponentName(EraseButton, "");
                SetButtonComponentName(GlueButton, "");
                SetButtonComponentName(RazorButton, "");

                SetButtonComponentName(RewindButton, "");
                SetButtonComponentName(PlayButton, "");
                SetButtonComponentName(RecordButton, "");
                SetButtonComponentName(StopButton, "");
                SetButtonComponentName(RepeatButton, "");

                SetButtonComponentName(CutButton, "");
                SetButtonComponentName(CopyButton, "");
                SetButtonComponentName(PasteButton, "");

                SetButtonComponentName(TruncateButton, "");
                SetButtonComponentName(ExtendButton, "");

                SetButtonComponentName(MuteSpeakerButton, "");
            }
        }

        protected void InitialiseBaseVertexBasedVisualiserControls()
        {
            TempoVisualiser.BaseEdge = GraphUtil.GetQueryOutFirstEdge(VisualizedVertex, "Tempo", null);
        }

        IVertex previousBaseVertex;

        void baseVertexIsEmpty()
        {
            StopAutoBackup();
        }        
        
        void CheckOrStartAutoBackup()
        {
            if (!AutoBackupVertexList.Contains(VisualizedVertex))
            {                
                if (AutoBackupTimer == null)
                {
                    AutoBackupTimer = new DispatcherTimer();
                    AutoBackupTimer.Tick += AutoBackupTimer_Elapsed;
                    AutoBackupTimer.Interval = new TimeSpan(0, AutoBackupMinutes, 0);
                }

                if (!AutoBackupTimer.IsEnabled)
                    AutoBackupTimer.Start();

                AutoBackupVertexList.Add(VisualizedVertex);
            }
        }

        private void AutoBackupTimer_Elapsed(object sender, EventArgs e)
        {
            if(VisualizedVertex != null)
            {
                IStore store = VisualizedVertex.Store;

                store.Backup();                
            }
        }

        void StopAutoBackup()
        {
            if (AutoBackupTimer != null && AutoBackupTimer.IsEnabled)
            {
                AutoBackupTimer.Stop();

                if (AutoBackupVertexList.Contains(VisualizedVertex))
                    AutoBackupVertexList.Remove(VisualizedVertex);
            }
        }        

        protected override void SetAxisDecorators()
        {
            if (VerticalAD == null)
            {
                VerticalAD = new TrackAxisDecorator();

                VerticalAD.SetBaseVertex(VisualizedVertex);
            }

            if (HorizontalAD == null)
            {
                RealTimeSpanAxisDecorator TimeSpanAD = new RealTimeSpanAxisDecorator(this);

                TimeSpanAD.BoldLineCount = 10;

                TimeSpanAD.PositionMarkPrimEnabled = PositionMarkPrimEnabled;

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

            Tempo = GraphUtil.GetDoubleValue(VisualizedVertex.Get(false, "Tempo:"), ref dummy);

            if (VisualizedVertex.Get(false, "ExtendTimeLength:") != null)
                ExtendTimeLength_Song = (int)GraphUtil.GetIntegerValue(VisualizedVertex.Get(false, "ExtendTimeLength:"));
            else
                ExtendTimeLength_Song = 1; // default - 1 minute

            if (VisualizedVertex.Get(false, "Length:") != null)
                Length = (int)GraphUtil.GetIntegerValue(VisualizedVertex.Get(false, "Length:"));
            else
                Length = GetMusicTimeFromRealTime(ExtendTimeLength_Song);

            SaveLength();

            IsDrum = GraphUtil.GetBooleanValue(VisualizedVertex.Get(false, "IsDrum:"), ref dummy);

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

        protected void RazorButton_Click(object sender, RoutedEventArgs e)
        {
            SetCursorMode(CursorStateEnum.Razor);
        }

        bool doNotUpdatePositionVertex = false;

        public void PositionUpdate()
        {
            if (VisualizedVertex != null)
            {
                int SongPosition = GraphUtil.GetIntegerValueOr0(VisualizedVertex.Get(false, "Position:"));

                if (SongPosition == -1)
                {
                    IVertex r = MinusZero.Instance.Root;

                    this.Dispatcher.Invoke(() =>
                    {
                        GraphUtil.SetVertexValue(VisualizedVertex, positionAttributeMeta, 0);

                        StopButton_Click(null, null);
                    });
                }

                this.Dispatcher.Invoke(() => {
                    doNotUpdatePositionVertex = true;
                    PositionMark = SongPosition;
                });
            }
        }

        void UpdateHorizontalADLength()
        {
            double RealTimeLength = GetRealTimeFromMusicTime(Length);

            HorizontalAD.ValueSpaceMax = RealTimeLength;
        }

        protected void UpdateTempo()
        {
            bool dummy = false;

            int PositionMark_Copy = PositionMark;

            Tempo = GraphUtil.GetDoubleValue(VisualizedVertex.Get(false, "Tempo:"), ref dummy);

            if (HorizontalAD != null)
            {
                UpdateHorizontalADLength();

                VisualiserDraw();

                PositionMark = PositionMark_Copy;
            }            
        }

        public override void ChildControlsLoaded()
        {
            CreateAddNewTrackControl();

            base.ChildControlsLoaded();
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                StopAutoBackup();                

                VisualiserHelper.Dispose();

                DispachSubControls();                
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

        public override double GetSnappedPosition(double position)
        {
            if (CurrentSnapToGrid == SnapToGridEnum.No_Snap)
                return position;

            double snapMinimalWidth = GetSnapMinimalWidth_Screen();

            double numberOfSnaps = position / snapMinimalWidth;

            double numberOfSnapsFloor = Math.Floor(numberOfSnaps);

            double rest = position - (numberOfSnapsFloor * snapMinimalWidth);

            if (rest < (snapMinimalWidth / 2))
                return numberOfSnapsFloor * snapMinimalWidth;
            else
                return (numberOfSnapsFloor + 1) * snapMinimalWidth;
        }

        protected override double GetSnapMinimalWidth_Screen()
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

        protected override int ScreenPositionToMusicTime(double position, bool performSnapCorrection)
        {
            if (HorizontalAD == null)
                return 0;

            double minuteWidth = HorizontalAD.SegmentLength * HorizontalAD.BaseUnitSize;

            double positionInMinutes = position / minuteWidth;
            
            int musicTime =  GetMusicTimeFromRealTime(positionInMinutes);

            if (performSnapCorrection)
                return MusicTimeSnapCorrect(musicTime);
            else
                return musicTime;
        }

        protected override double MusicTimeToScreenPosition(int musicTime, bool performSnapCorrection)
        {
            if (HorizontalAD == null)
                return 0;

            if(performSnapCorrection)
                musicTime = MusicTimeSnapCorrect(musicTime);

            double realTime = GetRealTimeFromMusicTime(musicTime);

            double minuteWidth = HorizontalAD.SegmentLength * HorizontalAD.BaseUnitSize;

            return realTime * minuteWidth;
        }        

        protected override void DrawItems()
        {
            ISet<IVertex> selectedVertexes = ((ListVisualiserHelper)VisualiserHelper).GetSelectedVertexes();

            foreach (IEdge e in VisualizedVertex.GetAll(false, "Track:"))
                foreach (IEdge ee in e.To.GetAll(false, "SequenceEvent:"))                
                    AddItemByEdge(ee, selectedVertexes);
        }        

        protected override void RemoveItemVertex(IItem i)
        {
            IEdge eventEdge = i.BaseEdge;

            IVertex trackVertex = Song.GetTrackVertexFromSequenceEventVertex(eventEdge.To);

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////
            
            GraphUtil.DeleteEdgeByToVertex(trackVertex, eventEdge.To);

            EdgeHelper.DeleteVertexByEdgeTo(Vertex.Get(false, "SelectedEdges:"), eventEdge.To);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////

            NeedToRebuildItemsDictionary = true;

            Items.Remove((FrameworkElement)i);

            i.Remove();
        }

        protected virtual void TimeUpdate()
        {
            if (PositionMark < 0)
            {
                Time.Text = "--:--:--";
                return;
            }

            RealTime rt = new RealTime();

            rt.Minutes = GetRealTimeFromMusicTime(PositionMark);

            string seconds = null;
            string miliseconds = null;

            if(rt.Second < 0)
            {
                Time.Text = "--:--:--";
                return;
            }

            if (rt.Second < 10)
                seconds = "0" + rt.Second;
            else
                seconds = rt.Second.ToString();

            if (rt.Milisecond < 10)
                miliseconds = "0" + rt.Milisecond;
            else
                miliseconds = rt.Milisecond.ToString();

            Time.Text = rt.Minute + ":" + seconds + ":" + miliseconds;
        }

        public override void UpdatePositionMark()
        {
            TimeUpdate();

            base.UpdatePositionMark();
        }

        protected void GetMinMaxPositionFromSequenceEdges(IEnumerable<IEdge> edges, out int minPosition, out int maxPosition)
        {
            minPosition = Int32.MaxValue;
            maxPosition = Int32.MinValue;

            foreach (IEdge e in edges)
            {
                IEdge edge = EdgeHelper.GetIEdgeByEdgeVertex(e.To);

                if (edge == null)
                    continue;

                IVertex v = edge.To;

                bool isNull = false;

                if (v.Get(false, "$Is:SequenceEvent") != null)
                {
                    int triggerTime = GraphUtil.GetIntegerValue(v.Get(false, "TriggerTime:"), ref isNull);

                    if (triggerTime > maxPosition)
                        maxPosition = triggerTime;

                    if (triggerTime < minPosition)
                        minPosition = triggerTime;

                    int triggerTimePlusLength = triggerTime + GraphUtil.GetIntegerValue(v.Get(false, @"Sequence:\Length:"), ref isNull);

                    if (triggerTimePlusLength > maxPosition)
                        maxPosition = triggerTimePlusLength;

                    if (triggerTimePlusLength < minPosition)
                        minPosition = triggerTimePlusLength;
                }
            }
        }

        protected override void PasteEdgesFromClipboard(IEnumerable<IEdge> edges)
        {
            bool o = false;            

            int minPosition, maxPosition;

            GetMinMaxPositionFromSequenceEdges(edges, out minPosition, out maxPosition);

            maxPosition = 0;

            foreach (IEdge e in edges)
            {
                IEdge edge = EdgeHelper.GetIEdgeByEdgeVertex(e.To);

                if (edge == null)
                    continue;

                IVertex v = edge.To;

                bool isClipboardCopy = false;
                bool isClipboardCut = false;

                if (GeneralUtil.CompareStrings(e.Meta, "ClipboardCopy"))
                    isClipboardCopy = true;

                if (GeneralUtil.CompareStrings(e.Meta, "ClipboardCut"))
                    isClipboardCut = true;

                if (isClipboardCopy || isClipboardCut)
                {
                    IEdge newEdge = null;

                    if (v.Get(false, "$Is:SequenceEvent") != null) // SequenceEvent
                    {
                        int triggerTime = GraphUtil.GetIntegerValue(v.Get(false, "TriggerTime:"), ref o) - minPosition + PositionMark;

                        int length = GraphUtil.GetIntegerValue(v.Get(false, @"Sequence:\Length:"), ref o);


                        if (triggerTime > maxPosition)
                            maxPosition = triggerTime;

                        if ((length + triggerTime) > maxPosition)
                            maxPosition = length + triggerTime;

                        if (isClipboardCopy)
                            newEdge = CopySequenceVertex(v, triggerTime);

                        if (isClipboardCut)
                        {
                            newEdge = edge;

                            UpdateSequenceVertex(edge, triggerTime);
                        }
                        

                        AddToSelectedEdges(newEdge);
                    }
                }
            }

            PositionMark = MusicTimeSnapCorrect_Up(maxPosition);

            PreviousSelectedItemContext = MainDownEnum.Main;
        }

        private void UpdateSequenceVertex(IEdge edge, int triggerTime)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex triggerTimeAttribute = r.Get(false, @"System\Lib\Music\SequenceEvent\TriggerTime");

            GraphUtil.SetVertexValue(edge.To, triggerTimeAttribute, triggerTime);

            RedrawTracks();
        }

        private IEdge CopySequenceVertex(IVertex v, int triggerTime)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex trackVertex = Song.GetTrackVertexFromSequenceEventVertex(v);            


            IVertex sequenceEventAttribute = r.Get(false, @"System\Lib\Music\Track\SequenceEvent");

            IVertex sequenceEvent = r.Get(false, @"System\Lib\Music\SequenceEvent");

            IVertex sequenceMeta = r.Get(false, @"System\Lib\Music\Sequence");

            IVertex sequenceEventSequenceMeta = r.Get(false, @"System\Lib\Music\SequenceEvent\Sequence");


            IEdge sequenceEventEdge = trackVertex.AddVertexAndReturnEdge(sequenceEventAttribute, null);

            IVertex sequenceEventVertex = sequenceEventEdge.To;


            sequenceEventVertex.AddEdge(MinusZero.Instance.Is, sequenceEvent);

            
            sequenceEventVertex.AddVertex(sequenceEvent.Get(false, @"Attribute:TriggerTime"), triggerTime);
            

            //

            IVertex sourceVertex = v.Get(false, "Sequence:");

            IEdge sourceEdge = GraphUtil.FindEdge(v, sequenceEventSequenceMeta, sourceVertex);

            GraphUtil.DeepCopy(sourceEdge, sequenceEventVertex);
            
            
            return sequenceEventEdge;
        }

        protected override int FindLastPosition(IEnumerable<IEdge> edges)
        {
            int last = 0;

            foreach (IEdge e in edges)
            {
                IVertex v = e.To.Get(false, "To:");

                if (v.Get(false, "$Is:SequenceEvent") != null)
                {
                    bool o = false;

                    int trigger = GraphUtil.GetIntegerValue(v.Get(false, "TriggerTime:"), ref o);

                    int length = GraphUtil.GetIntegerValue(v.Get(false, @"Sequence:\Length:"), ref o);

                    int max = trigger + length;

                    if (last < max)
                        last = max;
                }                
            }

            return last;
        }

        protected override void RazorDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(Items, currentMousePosition);

            if (element != null && element is IItem)
            {
                IItem item = (IItem)element;

                int cutPoint = ScreenPositionToMusicTime(currentMousePosition.X, true);

                Song.RazorCut(VisualizedVertex, item.BaseEdge.To, cutPoint);
            }

            RedrawTracks();
        }

        protected override void GlueDown(object sender, MouseButtonEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(Items, currentMousePosition);

            if (element != null && element is IItem)
            {
                IItem item = (IItem)element;

                int gluePoint = ScreenPositionToMusicTime(currentMousePosition.X, true);

                Song.Glue(VisualizedVertex, item.BaseEdge, gluePoint);
            }

            RedrawTracks();
        }

        public override double PositionMark_Screen
        {
            get
            {
                return positionMark_Screen;
            }
            set
            {
                positionMark = ScreenPositionToMusicTime(value, true);

                if(!doNotUpdatePositionVertex)
                    GraphUtil.SetVertexValue(VisualizedVertex, positionAttributeMeta, positionMark);

                doNotUpdatePositionVertex = false;

                positionMark_Screen = MusicTimeToScreenPosition(positionMark, true);

                UpdatePositionMark();
            }
        }

        public override int PositionMark
        {
            get
            {
                return positionMark;
            }

            set
            {
                positionMark_Screen = MusicTimeToScreenPosition(value, true);

                positionMark = ScreenPositionToMusicTime(PositionMark_Screen, true);

                if (!doNotUpdatePositionVertex)
                    GraphUtil.SetVertexValue(VisualizedVertex, positionAttributeMeta, positionMark);

                doNotUpdatePositionVertex = false;

                UpdatePositionMark();
            }
        }
    }
}

