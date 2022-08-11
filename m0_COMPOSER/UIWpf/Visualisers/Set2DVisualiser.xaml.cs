using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf;
using m0.UIWpf.Controls;
using m0.UIWpf.Visualisers;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroUML;
using m0_COMPOSER.Lib;
using m0_COMPOSER.UIWpf.Visualisers.Control;
using m0_COMPOSER.UIWpf.Visualisers.Control.Item;
using m0.User.Process.UX;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using m0.UIWpf.Visualisers.Helper;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    /// <summary>
    /// Interaction logic for SequenceVisualiser.xaml
    /// </summary>
    public partial class Set2DVisualiser : ZoomScrollViewBasedVisualiserBase
    {
        // Set2D beg

        protected bool ShowToolbarNames;
        protected bool ConnectPoints;
        protected bool CanEdit;

        IVertex SetItemsDefiningMeta;
        IVertex SetItemsDefiningMetaIs;
        string SetItemsDefiningMetaString;

        IVertex SetItemHorizontalAxisMetaVertex;
        IVertex SetItemVerticalAxisMetaVertex;

        string SetItemHorizontalAxisMetaString;
        string SetItemVerticalAxisMetaString;

        double horizontalMin_fromData;
        double horizontalMax_fromData;

        double verticalMin_fromData;
        double verticalMax_fromData;

        double horizontalMin;
        double horizontalMax;

        double verticalMin;
        double verticalMax;

        bool canDraw = false;

        // Set2D end

        static string[] _MetaTriggeringUpdateVertex = new string[] { "ShowArrowLines", "CanEdit", "ConnectPoints", "ShowToolbarNames" };
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

        public Set2DVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser)
        {
            InitializeComponent();

            //

            MinusZero mz = MinusZero.Instance;

            VisualiserName = "Set2DVisualiser";

            BaseEdgeToMetaVertex = mz.root.Get(false, @"System\Meta\ZeroTypes\Vertex");
            VisualiserMetaVertex = mz.root.Get(false, @"System\Meta\Visualiser\Set2D");

            //

            InitXAMLInstances();

            PositionMarkEnabled = true;

            PositionMark = -1000;

            //

            HasDown = false;

            //            

            ZoomSliderZero = true;

            ZoomScrollViewBasedVisualiserBase_Init(baseEdgeVertex, parentVisualiser);

            ZoomScrollView.ScrollViewer.Loaded += ScrollViewer_Loaded;

            IsCurrentPenItemCenter = true;
        }

        protected override AxisSegment FindVerticalSegment(double position) { return null; }

        private void ScrollViewer_Loaded(object sender, EventArgs e)
        {
            base.ChildControlsLoaded();

            canDraw = true;

            UpdateAxisAndDraw();

        }

        void UpdateAxisAndDraw()
        {
            AxisUpdate();

            if (verticalMax_fromData == verticalMin_fromData || horizontalMax_fromData == horizontalMin_fromData)
                canDraw = false;

            AxisMinMaxValuesUpdate();

            VisualiserDraw();
        }

        protected void VisualizedVertexToUpdated()
        {
            //CanDoItemsUpdate();
            //UpdateAxisAndDraw();
            ComboBoxesUpdate();
        }

        public override double GetSnappedPosition(double position)
        {
            return position;
        }

        protected override void PenUp(object sender, MouseButtonEventArgs e)
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            PerformPenUp_part1();

            AddItemVertex(GetSnappedPosition(MouseDownPoint.X), GetSnappedPosition(MouseDownPoint.Y));
            
            PerformPenUp_part2();
            
            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        protected void ComboBoxesUpdate()
        {
            ISet<IVertex> metaDictionary = new HashSet<IVertex>();

            foreach (IEdge e in VisualizedVertex)
                if (!metaDictionary.Contains(e.Meta) && VisualiserUtil.FilterEdge(e, this.Vertex))
                    metaDictionary.Add(e.Meta);

            SetItemsDefiningMetaComboBox.Items.Clear();

            bool isFirst = true;

            foreach (IVertex v in metaDictionary)
            {
                ComboBoxItem i = new ComboBoxItem();
                i.Content = v.Value;
                i.Tag = v;
                SetItemsDefiningMetaComboBox.Items.Add(i);

                if (isFirst)
                {
                    isFirst = false;
                    i.IsSelected = true;
                }
            }
        }

        protected override INoInEdgeInOutVertexVertex CheckBaseEdgeChange(IExecution exe)
        {
            VisualizedVertex = VisualiserHelper.Vertex.Get(false, @"BaseEdge:\To:");

            if (VisualizedVertex != null)
                VisualizedVertexToUpdated();

/*            ExecutionFlowHelper.DoAddRemoveDisposeAddEdgeByMetaOrValueChangeHandlers(exe.Stack, new List<EventHandlers>()
            { new EventHandlers(
                baseEdgeTo,
                EdgeAdded,
                EdgeRemoved,
                EdgeDisposed,                
                new string[] {"Octave", "Note"},
                new string[] {"TriggerTime", "Length", "Velocity"},
                new string[] {"Event" },
                AddEdgeByMetaOrValueChangeHandler
                )
            });
            */
            return exe.Stack;
        }        

        protected override void EdgeAdded(IEdge edge)
        {
            // AddItemByEdge(edge, null);

            int x = 0;
        }

        protected void AddEdgeByMetaOrValueChangeHandler(IEdge eventEdge)
        {            
            Dictionary<IVertex, IItem> ItemsDictionary = GetItemsDictionary();

            IItem item = null;

            if(ItemsDictionary.ContainsKey(eventEdge.To))
                item = GetItemsDictionary()[eventEdge.To];

            if(item != null)
                UpdateItem(eventEdge, item);        
        }

        protected override void UpdateVertexValues()
        {
            IVertex r = MinusZero.Instance.root;

            bool dummy = false;

            
            ShowArowLines = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowArrowLines:"), ref dummy);
            ShowToolbarNames = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowToolbarNames:"), ref dummy);
            CanEdit = GraphUtil.GetBooleanValue(Vertex.Get(false, "CanEdit:"), ref dummy);
            ConnectPoints = GraphUtil.GetBooleanValue(Vertex.Get(false, "ConnectPoints:"), ref dummy);

            ShowToolbarNames_SelectionChange();
        }

        protected void ShowToolbarNames_SelectionChange()
        {
            if (ShowToolbarNames)
            {
                SetButtonComponentName(PenButton, "New");
                SetButtonComponentName(ArrowButton, "Select");
                SetButtonComponentName(EraseButton, "Erase");
         
                
                SetButtonComponentName(CutButton, "Cut");
                SetButtonComponentName(CopyButton, "Copy");
                SetButtonComponentName(PasteButton, "Paste");

                SetButtonComponentName(ExtendUpButton, "Extend Up");
                SetButtonComponentName(ExtendDownButton, "Extend Down");
                SetButtonComponentName(ExtendLeftButton, "Extend Left");
                SetButtonComponentName(ExtendRightButton, "Extend Right");
            }
            else
            {
                SetButtonComponentName(PenButton, "");
                SetButtonComponentName(ArrowButton, "");
                SetButtonComponentName(EraseButton, "");
                
                SetButtonComponentName(CutButton, "");
                SetButtonComponentName(CopyButton, "");
                SetButtonComponentName(PasteButton, "");

                SetButtonComponentName(ExtendUpButton, "");
                SetButtonComponentName(ExtendDownButton, "");
                SetButtonComponentName(ExtendLeftButton, "");
                SetButtonComponentName(ExtendRightButton, "");
            }
        }

        protected override void UpdateVariablesFromBaseVertex()
        {
            VisualizedVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (VisualizedVertex == null)
                return;                       
        }   

        protected override void SetAxisDecorators()
        {
            if (VerticalAD == null)
                VerticalAD = new FloatSpanAxisDecorator(this, false);

            if (HorizontalAD == null)
                HorizontalAD = new FloatSpanAxisDecorator(this, true);

            ZoomScrollView.SetVerticalAxisDecorator(VerticalAD);

            ZoomScrollView.SetHorizontalAxisDecorator(HorizontalAD);

            //CanDoItemsUpdate();
        }

        protected override void SetupLocalVariablesFromBaseVertexVertexes()
        {
            if (VisualizedVertex.Get(false, "ExtendTimeLength:") != null)
                ExtendTimeLength = (int)GraphUtil.GetIntegerValue(VisualizedVertex.Get(false, "ExtendTimeLength:"));
            else
                ExtendTimeLength = Midi.Standard.MidiTicksPerSixteen * 16; // default

            if (VisualizedVertex.Get(false, "Length:") != null)
                Length = (int)GraphUtil.GetIntegerValue(VisualizedVertex.Get(false, "Length:"));
            else
                Length = ExtendTimeLength;



            bool dummy = false;

            IsDrum = GraphUtil.GetBooleanValue(VisualizedVertex.Get(false, "IsDrum:"), ref dummy);

            if (IsDrum)
                IsCurrentPenItemCenter = true;
        }

        protected void UpdateItem(IEdge itemEdge, IItem item)
        {
            IVertex itemVertex = itemEdge.To;

            double horizontalValue = GetHorizontal(itemVertex);
            double horizontalPosition = HorizontalAD.ValueSpaceToScreen(horizontalValue);
            
            double verticalValue = GetVertical(itemVertex);
            double verticalPosition = VerticalAD.ValueSpaceToScreen(verticalValue);

            bool dummy = false;                        

            FrameworkElement newElement = (FrameworkElement)item;                        
            
            item.HorizontalCenter = horizontalPosition;
            item.Top = verticalPosition - 5;
            item.Bottom = verticalPosition + 5;

            item.Update();
        }

        protected override void AddItemByEdge(IEdge itemEdge, List<IVertex> selectedVertexes)
        {
            FrameworkElement newElement;
            
            newElement = new Set2DItem(itemEdge, this);

            IItem newItem = (IItem)newElement;

            UpdateItem(itemEdge, newItem);

            IVertex itemEventVertex = itemEdge.To;

            if (selectedVertexes != null && selectedVertexes.Contains(itemEventVertex))
            {
                newItem.SelectHighlight();
                PreviousSelectedItemContext = MainDownEnum.Main;
            }            

            ItemsAdd(newItem);
        }
  
        protected IEdge AddItemVertex(double x, double y)
        {
            IEdge dataEdge = VertexOperations.AddInstanceAndReturnEdge(VisualizedVertex, SetItemsDefiningMeta);

            IVertex dataVertex = dataEdge.To;

            dataVertex.AddVertex(SetItemHorizontalAxisMetaVertex, HorizontalAD.ScreenToValueSpace(x));
            dataVertex.AddVertex(SetItemVerticalAxisMetaVertex, VerticalAD.ScreenToValueSpace(y));
            
            return dataEdge;
        }

        protected override void DrawItems()
        {
            if (!canDraw)
                return;

            ItemDictionary.RemoveAllByHost(this);

            List<IVertex> selectedVertexes = GetSelectedVertexes();


            foreach (IEdge e in VisualizedVertex.GetAll(false, SetItemsDefiningMetaString + ":"))
                AddItemByEdge(e, selectedVertexes);            
        }
        
        protected override void UpdateItem_VerticalPosition(IItem item)
        {            
      /*      FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            IVertex noteEventVertex = item.BaseEdge.To;

            AxisSegment segment = FindVerticalSegment(item.Top + 1);

            IVertex octaveVertex = segment.BaseVertex.Get(false, "Octave:");
            IVertex noteVertex = segment.BaseVertex.Get(false, "Note:");             

            GraphUtil.CreateOrReplaceEdge(noteEventVertex, musicPitchOctave, octaveVertex);
            GraphUtil.CreateOrReplaceEdge(noteEventVertex, musicPitchNote, noteVertex);
                      

            int? octave = GraphUtil.GetIntegerValue(octaveVertex);
            int? note = GraphUtil.GetIntegerValue(noteVertex);

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(verticalSpanVertex, octave, note);

            string label = pitchVertex.Value.ToString();

            item.Label = label;

            item.Update();*/
        }

        protected override void UpdateItem_HorizontalPosition(IItem item)
        {                        
            /*FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            IVertex itemVertex = item.BaseEdge.To;

            double itemWidth = element.Width;

            int TriggerTime;

            int Length;

            if (item.IsCentered)
            {
                TriggerTime = (int)(item.HorizontalCenter / HorizontalAD.BaseUnitSize);

                Length = 0;
            }
            else
            {
                TriggerTime = (int)(item.Left / HorizontalAD.BaseUnitSize);

                Length = (int)(itemWidth / HorizontalAD.BaseUnitSize);
            }            
            
            GraphUtil.SetVertexValue(itemVertex, musicEventTriggerTime, TriggerTime);

            if (Length != 0)
                GraphUtil.SetVertexValue(itemVertex, musicHasLengthLength, Length);  */          
        }

        protected override int ScreenPositionToMusicTime(double position, bool performSnapCorrection)
        {
            if (HorizontalAD == null)
                return 0;

            int musicTime = (int) (position / HorizontalAD.BaseUnitSize);

            if (performSnapCorrection)
                return MusicTimeSnapCorrect(musicTime);
            else
                return musicTime;
        }


        protected override int FindLastPosition(IEnumerable<IEdge> edges)
        {
            int last = 0;

            foreach(IEdge e in edges)
            {
                IVertex v = e.To.Get(false, "To:");

                if(v.Get(false, "$Is:NoteEvent") != null)
                {
                    bool o = false;

                    int trigger = GraphUtil.GetIntegerValue(v.Get(false, "TriggerTime:"), ref o);

                    int length = GraphUtil.GetIntegerValue(v.Get(false, "Length:"), ref o);

                    int max = trigger + length;

                    if (last < max)
                        last = max;
                }

                if (v.Get(false, "$Is:ControlChangeEvent") != null)
                {
                    bool o = false;

                    int trigger = GraphUtil.GetIntegerValue(v.Get(false, "TriggerTime:"), ref o);

                    if (last < trigger)
                        last = trigger;
                }
            }

            return last;
        }
        
        protected IEdge AddNoteVertex(IVertex octave, IVertex note, int triggerTime, int length, int velocity)
        {
            /*IEdge noteEventEdge = VisualizedVertex.AddVertexAndReturnEdge(musicEvent, null);

            IVertex noteEventVertex = noteEventEdge.To;

            noteEventVertex.AddEdge(MinusZero.Instance.Is, musicNoteEvent);

            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:TriggerTime"), triggerTime);
            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:Length"), length);
            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:Octave"), octave.Value);
            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:Note"), note.Value);            
            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:Velocity"), velocity);            

            return noteEventEdge;*/

            return null;
        }

        protected enum WhatIsInEdgesEnum { OnlyNotes, OnlyCC, Mix}

        protected WhatIsInEdgesEnum GetWhatIsInEdges(IEnumerable<IEdge> edges, out int minPosition, out int maxPosition, out bool onlyCopy)
        {
            minPosition = Int32.MaxValue;
            maxPosition = Int32.MinValue;

            bool notes = false;

            bool cc = false;

            onlyCopy = true;

            foreach(IEdge e in edges)
            {
                if (GeneralUtil.CompareStrings(e.Meta, "ClipboardCut"))
                    onlyCopy = false;

                IEdge edge = Edge.GetIEdgeByEdgeVertex(e.To);

                IVertex v = edge.To;

                bool isNull = false;

                if (v.Get(false, "$Is:NoteEvent") != null)
                {
                    notes = true;

                    int triggerTime = GraphUtil.GetIntegerValue(v.Get(false, "TriggerTime:"), ref isNull);

                    if (triggerTime > maxPosition)
                        maxPosition = triggerTime;

                    if (triggerTime < minPosition)
                        minPosition = triggerTime;

                    int triggerTimePlusLength = triggerTime + GraphUtil.GetIntegerValue(v.Get(false, "Length:"), ref isNull);

                    if (triggerTimePlusLength > maxPosition)
                        maxPosition = triggerTimePlusLength;

                    if (triggerTimePlusLength < minPosition)
                        minPosition = triggerTimePlusLength;
                }

                if (v.Get(false, "$Is:ControlChangeEvent") != null)
                {
                    cc = true;

                    int triggerTime = GraphUtil.GetIntegerValue(v.Get(false, "TriggerTime:"), ref isNull);

                    if (triggerTime > maxPosition)
                        maxPosition = triggerTime;

                    if (triggerTime < minPosition)
                        minPosition = triggerTime;
                }
            }

            if (notes && !cc)
                return WhatIsInEdgesEnum.OnlyNotes;

            if (!notes && cc)
                return WhatIsInEdgesEnum.OnlyCC;

            return WhatIsInEdgesEnum.Mix;
        }

        protected IEnumerable<IEdge> GetCCEdges(IEnumerable<IEdge> edgesIn, int minPosition, int maxPosition, bool onlyCopy)
        {
            IVertex clipboardMeta;

            if (onlyCopy)
                clipboardMeta = m0.MinusZero.Instance.root.Get(false, @"System\Meta\User\Session\ClipboardCopy");
            else
                clipboardMeta = m0.MinusZero.Instance.root.Get(false, @"System\Meta\User\Session\ClipboardCut");

            bool isNull = false;

            List<IEdge> edgesOut = new List<IEdge>();

            edgesOut.AddRange(edgesIn);

            foreach (IEdge e in VisualizedVertex.GetAll(false, @"Event:{$Is:ControlChangeEvent}"))
            {
                int triggerTime = GraphUtil.GetIntegerValue(e.To.Get(false, "TriggerTime:"), ref isNull);

                if (triggerTime >= minPosition && triggerTime <= maxPosition)
                {
                    IVertex edgeVertex = Edge.CreateTempEdgeVertex(e);

                    IEdge newEdge = new EasyEdge(null, clipboardMeta, edgeVertex);

                    edgesOut.Add(newEdge);
                }
            }

            return edgesOut;
        }        

        protected override void PasteEdgesFromClipboard(IEnumerable<IEdge> edges)
        {
            bool o = false;            

            int minPosition, maxPosition;

            bool onlyCopy;

            WhatIsInEdgesEnum whatIsClipboard = GetWhatIsInEdges(edges, out minPosition, out maxPosition, out onlyCopy);

            if (whatIsClipboard == WhatIsInEdgesEnum.Mix)
                return;

            if (whatIsClipboard == WhatIsInEdgesEnum.OnlyNotes)
                edges = GetCCEdges(edges, minPosition, maxPosition, onlyCopy);

            maxPosition = 0;

            foreach (IEdge e in edges)
            {
                IEdge edge = Edge.GetIEdgeByEdgeVertex(e.To);

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

                    if (v.Get(false, "$Is:NoteEvent") != null) // NOTE
                    {
                        int triggerTime = GraphUtil.GetIntegerValue(v.Get(false, "TriggerTime:"), ref o) - minPosition + PositionMark;
                        int length = GraphUtil.GetIntegerValue(v.Get(false, "Length:"), ref o);

                        if (triggerTime > maxPosition)
                            maxPosition = triggerTime;

                        if ((length + triggerTime) > maxPosition)
                            maxPosition = length + triggerTime;

                        if (isClipboardCopy)
                            newEdge = AddNoteVertex(v.Get(false, "Octave:"),
                                v.Get(false, "Note:"),
                                triggerTime,
                                length,
                                GraphUtil.GetIntegerValue(v.Get(false, "Velocity:"), ref o));

                        if (isClipboardCut)
                        {
                            newEdge = edge;

                            UpdateNoteVertex(edge,
                                v.Get(false, "Octave:").Value,
                                v.Get(false, "Note:").Value,
                                triggerTime,
                                length,
                                GraphUtil.GetIntegerValue(v.Get(false, "Velocity:"), ref o));
                        }                       
                        
                       AddToSelectedEdges(newEdge);
                    }

        
                }                
            }

            PositionMark = MusicTimeSnapCorrect_Up(maxPosition);

            PreviousSelectedItemContext = MainDownEnum.Main;
        }

        private void UpdateNoteVertex(IEdge noteEventEdge, object octave, object note, int triggerTime, int length, int velocity)
        {/*
            IVertex noteEventVertex = noteEventEdge.To;            

            GraphUtil.SetVertexValue(noteEventVertex, musicNoteEvent.Get(false, @"Attribute:TriggerTime"), triggerTime);
            GraphUtil.SetVertexValue(noteEventVertex, musicNoteEvent.Get(false, @"Attribute:Length"), length);
            GraphUtil.SetVertexValue(noteEventVertex, musicNoteEvent.Get(false, @"Attribute:Octave"), octave);
            GraphUtil.SetVertexValue(noteEventVertex, musicNoteEvent.Get(false, @"Attribute:Note"), note);
            GraphUtil.SetVertexValue(noteEventVertex, musicNoteEvent.Get(false, @"Attribute:Velocity"), velocity);                        
            */
        } 

        public override void Dispose()
            {
                if (!IsDisposed)
                {
                    VisualiserHelper.Dispose();

                    DispachSubControls();

                    ItemDictionary.RemoveAllByHost(this);

                    IsDisposed = true;
                }            
            }

        private void ExtendUpButton_Click(object sender, RoutedEventArgs e)
        {
            verticalMax = verticalMax + (Math.Abs(verticalMax-verticalMin) * 0.1);

            AxisMinMaxValuesUpdate();

            VisualiserDraw();
        }

        private void ExtendDownButton_Click(object sender, RoutedEventArgs e)
        {
            verticalMin = verticalMin - (Math.Abs(verticalMax - verticalMin) * 0.1);

            AxisMinMaxValuesUpdate();

            VisualiserDraw();
        }

        private void ExtendLeftButton_Click(object sender, RoutedEventArgs e)
        {
            horizontalMin = horizontalMin - (Math.Abs(horizontalMax - horizontalMin) * 0.1);

            AxisMinMaxValuesUpdate();

            VisualiserDraw();
        }

        private void ExtendRightButton_Click(object sender, RoutedEventArgs e)
        {
            horizontalMax = horizontalMax + (Math.Abs(horizontalMax - horizontalMin) * 0.1);

            AxisMinMaxValuesUpdate();

            VisualiserDraw();
        }

        private bool IsMetaSuitableForAxis(IVertex v)
        {
            IVertex edgeTarget = v.Get(false, "$EdgeTarget:");

            if (edgeTarget == null)
                return false;

            string type = edgeTarget. Value.ToString();

            if (type == "Integer" 
                || type == "Float" 
                || type == "Decimal")
                return true;

            return false;
        }

        private void SetItemsDefiningMetaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SetItemsDefiningMetaComboBox.SelectedItem == null)
                return;

            SetItemsDefiningMeta = (IVertex) ((ComboBoxItem)SetItemsDefiningMetaComboBox.SelectedItem).Tag;

            SetItemsDefiningMetaString = SetItemsDefiningMeta.Value.ToString();

            SetItemHorizontalAxisMetaComboBox.Items.Clear();
            SetItemVerticalAxisMetaComboBox.Items.Clear();

            int cnt = 0;

            foreach(IEdge _e in SetItemsDefiningMeta.GetAll(false, @"$EdgeTarget:\"))
                if(IsMetaSuitableForAxis(_e.To))
                {
                    if (VisualiserUtil.FilterEdge(_e, this.Vertex))
                    {
                        ComboBoxItem i = new ComboBoxItem();
                        i.Content = _e.To.Value;
                        i.Tag = _e.To;

                        SetItemHorizontalAxisMetaComboBox.Items.Add(i);

                        if (cnt == 0)
                            i.IsSelected = true;

                        //

                        i = new ComboBoxItem();
                        i.Content = _e.To.Value;
                        i.Tag = _e.To;

                        SetItemVerticalAxisMetaComboBox.Items.Add(i);

                        if (cnt == 1)
                            i.IsSelected = true;

                        cnt++;
                    }
                }
        }

        private void SetItemHorizontalAxisMetaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SetItemHorizontalAxisMetaComboBox.SelectedItem == null)
                return;

            SetItemHorizontalAxisMetaVertex = (IVertex)((ComboBoxItem)SetItemHorizontalAxisMetaComboBox.SelectedItem).Tag;

            SetItemHorizontalAxisMetaString = SetItemHorizontalAxisMetaVertex.Value.ToString();

            UpdateAxisAndDraw();
            //CanDoItemsUpdate();
        }

        private void SetItemVerticalAxisMetaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SetItemVerticalAxisMetaComboBox.SelectedItem == null)
                return;

            SetItemVerticalAxisMetaVertex = (IVertex)((ComboBoxItem)SetItemVerticalAxisMetaComboBox.SelectedItem).Tag;

            SetItemVerticalAxisMetaString = SetItemVerticalAxisMetaVertex.Value.ToString();

            UpdateAxisAndDraw();
            //CanDoItemsUpdate();
        }

        /*void CanDoItemsUpdate()
        {
            AxisUpdate();

            if(canDraw)
                VisualiserDraw();
        }*/

        double GetHorizontal(IVertex item)
        {
            IVertex horizontalVertex = GraphUtil.GetQueryOutFirst(item, SetItemHorizontalAxisMetaString, null);
            return GraphUtil.GetDoubleValueOr0(horizontalVertex);
        }

        double GetVertical(IVertex item)
        {
            IVertex verticalVertex = GraphUtil.GetQueryOutFirst(item, SetItemVerticalAxisMetaString, null);
            return GraphUtil.GetDoubleValueOr0(verticalVertex);
        }

        void AxisUpdate()
        {
            if (SetItemsDefiningMetaString == null
                || HorizontalAD == null
                || VerticalAD == null)
                return;

            if (!canDraw)
                return;

            horizontalMin_fromData = double.PositiveInfinity;
            horizontalMax_fromData = double.NegativeInfinity;

            verticalMin_fromData = double.PositiveInfinity;
            verticalMax_fromData = double.NegativeInfinity;

            foreach (IEdge e in VisualizedVertex.GetAll(false, SetItemsDefiningMetaString + ":"))
            {
                double horizontalValue = GetHorizontal(e.To);

                double verticalValue = GetVertical(e.To);

                if (horizontalValue > horizontalMax_fromData)
                    horizontalMax_fromData = horizontalValue;

                if (horizontalValue < horizontalMin_fromData)
                    horizontalMin_fromData = horizontalValue;

                if (verticalValue > verticalMax_fromData)
                    verticalMax_fromData = verticalValue;

                if (verticalValue < verticalMin_fromData)
                    verticalMin_fromData = verticalValue;
            }

            verticalMin = verticalMin_fromData;
            verticalMax = verticalMax_fromData;
            horizontalMin = horizontalMin_fromData;
            horizontalMax = horizontalMax_fromData;
        }

        public void AxisMinMaxValuesUpdate()
        {
            if (VerticalAD != null && HorizontalAD != null)
            {
                VerticalAD.ValueSpaceMax = verticalMax;
                VerticalAD.ValueSpaceMin = verticalMin;

                HorizontalAD.ValueSpaceMax = horizontalMax;
                HorizontalAD.ValueSpaceMin = horizontalMin;
            }
        }
    }
}
