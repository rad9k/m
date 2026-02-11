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
    public partial class SequenceVisualiser : ZoomScrollViewBasedVisualiserBase, INoDownVisualiser
    {
        static string[] _MetaTriggeringUpdateVertex = new string[] { "ShowArrowLines", "ShowSnapLines", "ShowLabel", "ShowVelocity", "DefaultVelocity", "SnapToGrid" };
        public override string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { };
        public override string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public void InitXAMLInstances()
        {
            PenButton = PenButton_Instance;
            ArrowButton = ArrowButton_Instance;
            EraseButton = EraseButton_Instance;
            TruncateButton = TruncateButton_Instance;
            ExtendButton = ExtendButton_Instance;

            CutButton = CutButton_Instance;
            CopyButton = CopyButton_Instance;
            PasteButton = PasteButton_Instance;

            ZoomScrollView = ZoomScrollView_Instance;
        }

        public SequenceVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            InitializeComponent();

            //

            MinusZero mz = MinusZero.Instance;

            VisualiserName = "SequenceVisuliser";

            BaseEdgeToMetaVertex = mz.root.Get(false, @"System\Lib\Music\Class:Sequence");
            VisualiserMetaVertex = mz.root.Get(false, @"System\Meta\Visualiser\Sequence");

            //

            InitXAMLInstances();

            PositionMarkEnabled = true;

            PositionMark = -1000;

            //

            HasDown = true;

            //

            ZoomScrollViewBasedVisualiserBase_Init(baseEdgeVertex, parentVisualiser);
        }

        protected override INoInEdgeInOutVertexVertex CheckBaseEdgeChange(IExecution exe)
        {
            IVertex baseEdgeTo = VisualiserHelper.Vertex.Get(false, @"BaseEdge:\To:");

            ExecutionFlowHelper.DoAddRemoveDisposeAddEdgeByMetaOrValueChangeHandlers(exe.Stack, new List<EventHandlers>()
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

            return exe.Stack;
        }        

        protected override void EdgeAdded(IEdge edge)
        {
            if (GraphUtil.ExistQueryOut(edge.To, "$Is", "NoteEvent"))
                AddItemByEdge(edge, SelectedVertexes);
            else
                if(GraphUtil.GetIntegerValue(edge.To.Get(false, @"Number:")) == CurrentControlChangeNumber)
                    AddItemByEdge_Down(edge, null, false, false);
        }

        protected void AddEdgeByMetaOrValueChangeHandler(IEdge eventEdge)
        {            
            Dictionary<IVertex, IItem> ItemsDictionary = GetItemsDictionary();

            IItem item = null;

            if(ItemsDictionary.ContainsKey(eventEdge.To))
                item = GetItemsDictionary()[eventEdge.To];

            if(item != null)
                UpdateItem(eventEdge, item);

            //

            Dictionary<IVertex, IItem> ItemsDictionary_Down = GetItemsDictionary_Down();

            IItem item_Down = null;

            if (ItemsDictionary_Down.ContainsKey(eventEdge.To))
                item_Down = GetItemsDictionary_Down()[eventEdge.To];
           
            if (item_Down != null)
                UpdateItem_Down(eventEdge, item_Down);            
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
                GraphUtil.ReplaceEdge(Vertex, r.Get(false, @"System\Meta\Visualiser\Sequence\SnapToGrid"), r.Get(false, @"System\Meta\Visualiser\SnapToGridEnum\'1/16 bar'"));

            SnapToGridComboBox_SelectionChange();
        }        

        protected override void UpdateVariablesFromBaseVertex()
        {
            VisualizedVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (VisualizedVertex == null)
                return;            

            if (VisualizedVertex.Get(false, "$Is:Sequence") == null)
            {
                VisualizedVertex = null;
                return;
            }

            IVertex r = MinusZero.Instance.Root;

            verticalSpanVertex = VisualizedVertex.Get(false, "PitchSet:");

            if (verticalSpanVertex == null)
                if (IsDrum)
                    verticalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultDrumPitchSet:");
                else
                    verticalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultPitchSet:");

            verticalSpanVertex_Down = VisualizedVertex.Get(false, "ControlChangeDescriptionSet:");

            horizontalSpanVertex = VisualizedVertex.Get(false, "TimeSpan:");

            if (horizontalSpanVertex == null)
                horizontalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultMusicTimeSpanLevel:");
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
                MusicTimeSpanAxisDecorator TimeSpanAD = new MusicTimeSpanAxisDecorator(this);
                TimeSpanAD.BoldLineCount = 4;

                HorizontalAD = TimeSpanAD;

                HorizontalAD.SetBaseVertex(horizontalSpanVertex);

                HorizontalAD.ValueSpaceMax = Length;
            }

            ZoomScrollView.SetVerticalAxisDecorator(VerticalAD);

            ZoomScrollView.SetHorizontalAxisDecorator(HorizontalAD);
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

            SaveLength();


            bool dummy = false;

            IsDrum = GraphUtil.GetBooleanValue(VisualizedVertex.Get(false, "IsDrum:"), ref dummy);

            if (IsDrum)
                IsCurrentPenItemCenter = true;
        }

        protected void UpdateItem(IEdge itemEdge, IItem item)
        {
            IVertex itemEventVertex = itemEdge.To;

            bool dummy = false;

            int triggerTime = GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "TriggerTime:"), ref dummy);

            int length = GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Length:"), ref dummy);

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(verticalSpanVertex,
                GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Octave:")),
                GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Note:")));

            string label = "x";

            if (pitchVertex != null)
                label = pitchVertex.Value.ToString();
            else
                return;

            FrameworkElement newElement = (FrameworkElement)item;                        

            AxisSegment itemSegment = GetVerticalSegment(pitchVertex);


            double startPosition = triggerTime * HorizontalAD.BaseUnitSize;

            double endPosition = startPosition + (length * HorizontalAD.BaseUnitSize);


            if (IsDrum)
            {
                item.HorizontalCenter = startPosition;
                item.Top = itemSegment.StartPosition;
                item.Bottom = itemSegment.EndPosition;
            }
            else
            {
                item.Left = startPosition;
                item.Top = itemSegment.StartPosition;
                item.Right = endPosition;
                item.Bottom = itemSegment.EndPosition;

                ((NoteItem)item).Label = label;
            }

            item.Update();
        }

        protected override void AddItemByEdge(IEdge itemEdge, ISet<IVertex> selectedVertexes)
        {            
            FrameworkElement newElement;

            if (IsDrum)
                newElement = new DrumItem(itemEdge, this, ShowVelocity);
            else
                newElement = new NoteItem(itemEdge, this, ShowLabel, ShowVelocity);

            IItem newItem = (IItem)newElement;

            UpdateItem(itemEdge, newItem);

            IVertex itemEventVertex = itemEdge.To;

            if (selectedVertexes != null && selectedVertexes.Contains(itemEventVertex))
            {
                newItem.SelectHighlight();
                PreviousSelectedItemContext = MainDownEnum.Main;
            }            

            ItemsAdd(newItem);

            if (MainItemsSyncedWithDown)
                AddItemByEdge_Down(itemEdge, selectedVertexes, false, true);
        }

  

        static IVertex r = MinusZero.Instance.Root;
        static IVertex musicSequenceEvent = r.Get(false, @"System\Lib\Music\Sequence\Event");
        static IVertex musicNoteEvent = r.Get(false, @"System\Lib\Music\NoteEvent");

        protected override IEdge AddItemVertex(AxisSegment itemSegment, double startPosition, double lengthPosition)
        {            
            IEdge noteEventEdge = VisualizedVertex.AddVertexAndReturnEdge(musicSequenceEvent, null);            

            IVertex noteEventVertex = noteEventEdge.To;

            noteEventVertex.AddEdge(MinusZero.Instance.Is, musicNoteEvent);

            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:TriggerTime"), (int)((startPosition / HorizontalAD.BaseUnitSize) + 0.01));
            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:Length"), (int)((lengthPosition / HorizontalAD.BaseUnitSize) + 0.01));            
            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:Octave"), itemSegment.BaseVertex.Get(false, "Octave:").Value);
            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:Note"), itemSegment.BaseVertex.Get(false, "Note:").Value);
            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:Velocity"), DefaultVelocity);
                       
            return noteEventEdge;
        }

        static IVertex musicEvent = r.Get(false, @"System\Lib\Music\Event");
        static IVertex musicControlChangeEvent = r.Get(false, @"System\Lib\Music\ControlChangeEvent");                

        protected override void DrawItems()
        {
            ItemDictionary.RemoveAllByHost(this);

            ISet<IVertex> selectedVertexes = ((ListVisualiserHelper)VisualiserHelper).GetSelectedVertexes();

            foreach (IEdge e in VisualizedVertex.GetAll(false, "Event:"))
                if (GraphUtil.ExistQueryOut(e.To, "$Is", "NoteEvent"))
                    AddItemByEdge(e, selectedVertexes);
        }

        static IVertex musicPitchOctave = r.Get(false, @"System\Lib\Music\Pitch\Octave");
        static IVertex musicPitchNote = r.Get(false, @"System\Lib\Music\Pitch\Note");

        protected override void UpdateItem_VerticalPosition(IItem item)
        {            
            FrameworkElement element;

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

            item.Update();
        }

        static IVertex musicEventTriggerTime = r.Get(false, @"System\Lib\Music\Event\TriggerTime");
        static IVertex musicHasLengthLength = r.Get(false, @"System\Lib\Music\HasLength\Length");

        protected override void UpdateItem_HorizontalPosition(IItem item)
        {                        
            FrameworkElement element;

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
                TriggerTime = (int)((item.Left / HorizontalAD.BaseUnitSize) + 0.01);

                Length = (int)((itemWidth / HorizontalAD.BaseUnitSize) + 0.01);
            }            
            
            GraphUtil.SetVertexValue(itemVertex, musicEventTriggerTime, TriggerTime);

            if (Length != 0)
                GraphUtil.SetVertexValue(itemVertex, musicHasLengthLength, Length);            
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

        protected override double MusicTimeToScreenPosition(int musicTime, bool performSnapCorrection)
        {
            if (performSnapCorrection)
                musicTime = MusicTimeSnapCorrect(musicTime);

            if (HorizontalAD == null)
                return 0;

            return musicTime * HorizontalAD.BaseUnitSize;
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
            IEdge noteEventEdge = VisualizedVertex.AddVertexAndReturnEdge(musicEvent, null);

            IVertex noteEventVertex = noteEventEdge.To;

            noteEventVertex.AddEdge(MinusZero.Instance.Is, musicNoteEvent);

            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:TriggerTime"), triggerTime);
            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:Length"), length);
            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:Octave"), octave.Value);
            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:Note"), note.Value);            
            noteEventVertex.AddVertex(musicNoteEvent.Get(false, @"Attribute:Velocity"), velocity);            

            return noteEventEdge;
        }

        // copy & paste rules for SequenceVisualiser
        //
        // what is selected before cut / paste | what is copied | what is selected after paste
        // ------------------------------------+----------------+---------------
        //                               notes | notes + cc     | notes
        //                     note velocities | notes + cc     | notes
        //                                  cc | cc             | cc

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

                IEdge edge = EdgeHelper.GetIEdgeByEdgeVertex(e.To);

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
                clipboardMeta = m0.MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\User\Session\ClipboardCopy");
            else
                clipboardMeta = m0.MinusZero.Instance.root.Get(false, @"System\Meta\ZeroTypes\User\Session\ClipboardCut");

            bool isNull = false;

            List<IEdge> edgesOut = new List<IEdge>();

            edgesOut.AddRange(edgesIn);

            foreach (IEdge e in VisualizedVertex.GetAll(false, @"Event:{$Is:ControlChangeEvent}"))
            {
                int triggerTime = GraphUtil.GetIntegerValue(e.To.Get(false, "TriggerTime:"), ref isNull);

                if (triggerTime >= minPosition && triggerTime <= maxPosition)
                {
                    IVertex edgeVertex = EdgeHelper.CreateTempEdgeVertex(e);

                    IEdge newEdge = new EasyEdge(null, clipboardMeta, edgeVertex);

                    edgesOut.Add(newEdge);
                }
            }

            return edgesOut;
        }        

        protected IEdge AddCCVertex(int triggerTime, int number, int value)
        {            
            IEdge noteEventEdge = VisualizedVertex.AddVertexAndReturnEdge(musicEvent, null);

            IVertex noteEventVertex = noteEventEdge.To;

            noteEventVertex.AddEdge(MinusZero.Instance.Is, musicControlChangeEvent);

            noteEventVertex.AddVertex(musicControlChangeEvent.Get(false, @"Attribute:Number"), number);
            noteEventVertex.AddVertex(musicControlChangeEvent.Get(false, @"Attribute:Value"), value);
            noteEventVertex.AddVertex(musicControlChangeEvent.Get(false, @"Attribute:TriggerTime"), triggerTime);            

            return noteEventEdge;
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
                IEdge edge = EdgeHelper.GetIEdgeByEdgeVertex(e.To);

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

                    if (v.Get(false, "$Is:ControlChangeEvent") != null) // CONTROLCHANGE
                    {
                        int triggerTime = GraphUtil.GetIntegerValue(v.Get(false, "TriggerTime:"), ref o) - minPosition + PositionMark;

                        if (isClipboardCopy)
                            newEdge = AddCCVertex(triggerTime,
                                GraphUtil.GetIntegerValue(v.Get(false, "Number:"), ref o),
                                GraphUtil.GetIntegerValue(v.Get(false, "Value:"), ref o));

                        if (isClipboardCut)
                        {
                            newEdge = edge;

                            UpdateCCVertex(edge,
                                triggerTime,
                                GraphUtil.GetIntegerValue(v.Get(false, "Number:"), ref o),
                                GraphUtil.GetIntegerValue(v.Get(false, "Value:"), ref o));
                        }                        

                        if (whatIsClipboard == WhatIsInEdgesEnum.OnlyCC)
                            AddToSelectedEdges(newEdge);
                    }
                }                
            }

            PositionMark = MusicTimeSnapCorrect_Up(maxPosition);

            PreviousSelectedItemContext = MainDownEnum.Main;
        }

        private void UpdateNoteVertex(IEdge noteEventEdge, object octave, object note, int triggerTime, int length, int velocity)
        {
            IVertex noteEventVertex = noteEventEdge.To;            

            GraphUtil.SetVertexValue(noteEventVertex, musicNoteEvent.Get(false, @"Attribute:TriggerTime"), triggerTime);
            GraphUtil.SetVertexValue(noteEventVertex, musicNoteEvent.Get(false, @"Attribute:Length"), length);
            GraphUtil.SetVertexValue(noteEventVertex, musicNoteEvent.Get(false, @"Attribute:Octave"), octave);
            GraphUtil.SetVertexValue(noteEventVertex, musicNoteEvent.Get(false, @"Attribute:Note"), note);
            GraphUtil.SetVertexValue(noteEventVertex, musicNoteEvent.Get(false, @"Attribute:Velocity"), velocity);                        
        }

        private void UpdateCCVertex(IEdge ccEventEdge, int triggerTime, int number, int value)
        {            
            IVertex noteEventVertex = ccEventEdge.To;            

            GraphUtil.SetVertexValue(noteEventVertex, musicControlChangeEvent.Get(false, @"Attribute:Number"), number);
            GraphUtil.SetVertexValue(noteEventVertex, musicControlChangeEvent.Get(false, @"Attribute:Value"), value);
            GraphUtil.SetVertexValue(noteEventVertex, musicControlChangeEvent.Get(false, @"Attribute:TriggerTime"), triggerTime);
        }

        protected override void DrawItems_Down()
        {
            ISet<IVertex> selectedVertexes = ((ListVisualiserHelper)VisualiserHelper).GetSelectedVertexes();

            if (CurrentControlChangeNumber == -1)
            {
                foreach (IEdge e in VisualizedVertex.GetAll(false, "Event:"))
                    if (GraphUtil.ExistQueryOut(e.To, "$Is", "NoteEvent"))
                        if (ApplyFilter_Down(e.To))
                            AddItemByEdge_Down(e, selectedVertexes, false, true);
            }
            else
            {
                foreach (IEdge e in VisualizedVertex.GetAll(false, "Event:"))
                    if (GraphUtil.ExistQueryOut(e.To, "$Is", "ControlChangeEvent")
                        && GraphUtil.GetIntegerValue(e.To.Get(false, @"Number:")) == CurrentControlChangeNumber)
                        AddItemByEdge_Down(e, selectedVertexes, false, false);
            }
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
        }
}
