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

namespace m0_COMPOSER.UIWpf.Visualisers
{
    /// <summary>
    /// Interaction logic for SequenceVisualiser.xaml
    /// </summary>
    public partial class SequenceVisualiser : ZoomScrollViewBasedVisualiserBase
    {
        static string[] _MetaTriggeringUpdateVertex = new string[] { "ShowArrowLines:", "ShowSnapLines:", "ShowLabel:", "ShowVelocity:", "DefaultVelocity:", "SnapToGrid" };
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

        public SequenceVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser)
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

            ExecutionFlowHelper.RunAddRemoveDisposeHandlers(exe.Stack, new List<EdgeAddRemoveDisposeHandlers>()
            { new EdgeAddRemoveDisposeHandlers(
                baseEdgeTo,
                EdgeAdded,
                EdgeRemoved,
                EdgeDisposed)
            });

            return exe.Stack;
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

                HorizontalAD.SetLength(Length);
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

        protected override void AddItem(IEdge itemEdge, List<IVertex> selectedVertexes)
        {
            IVertex itemEventVertex = itemEdge.To;

            bool dummy = false;

            int triggerTime = GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "TriggerTime:"), ref dummy);

            int length = GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Length:"), ref dummy);

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(verticalSpanVertex,
                GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Octave:")),
                GraphUtil.GetIntegerValue(itemEventVertex.Get(false, "Note:")));

            string label = pitchVertex.Value.ToString();

            FrameworkElement newElement;

            if (IsDrum)
                newElement = new DrumItem(itemEdge, this, ShowVelocity);
            else
                newElement = new NoteItem(itemEdge, label, this, ShowLabel, ShowVelocity);

            IItem newItem = (IItem)newElement;

            if (selectedVertexes != null && selectedVertexes.Contains(itemEventVertex))
            {
                newItem.SelectHighlight();
                PreviousSelectedItemContext = MainDownEnum.Main;
            }

            AxisSegment itemSegment = GetVerticalSegment(pitchVertex);


            double startPosition = triggerTime * HorizontalAD.BaseUnitSize;

            double endPosition = startPosition + (length * HorizontalAD.BaseUnitSize);


            if (IsDrum)
            {
                newItem.HorizontalCenter = startPosition;
                newItem.Top = itemSegment.StartPosition;
                newItem.Bottom = itemSegment.EndPosition;
            }
            else
            {
                newItem.Left = startPosition;
                newItem.Top = itemSegment.StartPosition;
                newItem.Right = endPosition;
                newItem.Bottom = itemSegment.EndPosition;
            }

            ItemsAdd(newItem);

            if (MainItemsSyncedWithDown)
                AddItem_Down(itemEdge, selectedVertexes, false, true);
        }

        protected override IEdge AddItemVertex(AxisSegment itemSegment, double startPosition, double lengthPosition)
        {
            IVertex r = MinusZero.Instance.Root;

            //IVertex Event = r.Get(false, @"System\Lib\Music\Event");
            IVertex Event = r.Get(false, @"System\Lib\Music\Sequence\Event");
            IVertex noteEvent = r.Get(false, @"System\Lib\Music\NoteEvent");

            IEdge noteEventEdge = VisualizedVertex.AddVertexAndReturnEdge(Event, null);            

            IVertex noteEventVertex = noteEventEdge.To;

            noteEventVertex.AddEdge(MinusZero.Instance.Is, noteEvent);

            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:TriggerTime"), (int)((startPosition / HorizontalAD.BaseUnitSize) + 0.01));
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Length"), (int)((lengthPosition / HorizontalAD.BaseUnitSize) + 0.01));            
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Octave"), itemSegment.BaseVertex.Get(false, "Octave:").Value);
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Note"), itemSegment.BaseVertex.Get(false, "Note:").Value);
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Velocity"), DefaultVelocity);
                       

            return noteEventEdge;
        }

        protected override void DrawItems()
        {
            ItemDictionary.RemoveAllByHost(this);

            List<IVertex> selectedVertexes = GetSelectedVertexes();

            foreach (IEdge e in VisualizedVertex.GetAll(false, "Event:"))
                if (GraphUtil.ExistQueryOut(e.To, "$Is", "NoteEvent"))
                    AddItem(e, selectedVertexes);
        }

        protected override void UpdateItem_VerticalPosition(IItem item)
        {
            IVertex r = MinusZero.Instance.root;
            IVertex metaOctave = r.Get(false, @"System\Lib\Music\Pitch\Octave");
            IVertex metaNote = r.Get(false, @"System\Lib\Music\Pitch\Note");

            FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            IVertex noteEventVertex = item.BaseEdge.To;

            AxisSegment segment = FindVerticalSegment(item.Top + 1);

            IVertex octaveVertex = segment.BaseVertex.Get(false, "Octave:");
            IVertex noteVertex = segment.BaseVertex.Get(false, "Note:");

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            bool ForceVertexChangeOff_history = VisualiserHelper.ForceVertexChangeOff;
            VisualiserHelper.ForceVertexChangeOff = true;

            GraphUtil.CreateOrReplaceEdge(noteEventVertex, metaOctave, octaveVertex);
            GraphUtil.CreateOrReplaceEdge(noteEventVertex, metaNote, noteVertex);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
            
            VisualiserHelper.ForceVertexChangeOff = ForceVertexChangeOff_history;

            int? octave = GraphUtil.GetIntegerValue(octaveVertex);
            int? note = GraphUtil.GetIntegerValue(noteVertex);

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(verticalSpanVertex, octave, note);

            string label = pitchVertex.Value.ToString();

            item.Label = label;

            item.Update();
        }

        protected override void UpdateItem_HorizontalPosition(IItem item)
        {
            IVertex r = MinusZero.Instance.root;
            IVertex metaTriggerTime = r.Get(false, @"System\Lib\Music\Event\TriggerTime");
            IVertex metaLength = r.Get(false, @"System\Lib\Music\HasLength\Length");

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
                TriggerTime = (int)(item.Left / HorizontalAD.BaseUnitSize);

                Length = (int)(itemWidth / HorizontalAD.BaseUnitSize);
            }

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            bool ForceVertexChangeOff_history = VisualiserHelper.ForceVertexChangeOff;
            VisualiserHelper.ForceVertexChangeOff = true;

            GraphUtil.SetVertexValue(itemVertex, metaTriggerTime, TriggerTime);

            if (Length != 0)
                GraphUtil.SetVertexValue(itemVertex, metaLength, Length);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
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
            IVertex r = MinusZero.Instance.Root;

            IVertex Event = r.Get(false, @"System\Lib\Music\Event");
            IVertex noteEvent = r.Get(false, @"System\Lib\Music\NoteEvent");

            IEdge tempNoteEventEdge = VisualizedVertex.AddVertexAndReturnEdge(null, null);

            IVertex noteEventVertex = tempNoteEventEdge.To;

            noteEventVertex.AddEdge(MinusZero.Instance.Is, noteEvent);

            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:TriggerTime"), triggerTime);
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Length"), length);
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Octave"), octave.Value);
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Note"), note.Value);            
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Velocity"), velocity);

            IEdge finalEdge = VisualizedVertex.AddEdge(Event, noteEventVertex);

            VisualizedVertex.DeleteEdge(tempNoteEventEdge);

            return finalEdge;
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

        protected IEdge AddCCVertex(int triggerTime, int number, int value)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex Event = r.Get(false, @"System\Lib\Music\Event");
            IVertex controlChangeEvent = r.Get(false, @"System\Lib\Music\ControlChangeEvent");

            IEdge tempNoteEventEdge = VisualizedVertex.AddVertexAndReturnEdge(null, null);

            IVertex noteEventVertex = tempNoteEventEdge.To;

            noteEventVertex.AddEdge(MinusZero.Instance.Is, controlChangeEvent);

            noteEventVertex.AddVertex(controlChangeEvent.Get(false, @"Attribute:Number"), number);
            noteEventVertex.AddVertex(controlChangeEvent.Get(false, @"Attribute:Value"), value);
            noteEventVertex.AddVertex(controlChangeEvent.Get(false, @"Attribute:TriggerTime"), triggerTime);

            IEdge finalEdge = VisualizedVertex.AddEdge(Event, noteEventVertex);

            VisualizedVertex.DeleteEdge(tempNoteEventEdge);

            return finalEdge;
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
            IVertex r = MinusZero.Instance.Root;

            IVertex Event = r.Get(false, @"System\Lib\Music\Event");
            IVertex noteEvent = r.Get(false, @"System\Lib\Music\NoteEvent");            

            IVertex noteEventVertex = noteEventEdge.To;            

            GraphUtil.SetVertexValue(noteEventVertex, noteEvent.Get(false, @"Attribute:TriggerTime"), triggerTime);
            GraphUtil.SetVertexValue(noteEventVertex, noteEvent.Get(false, @"Attribute:Length"), length);
            GraphUtil.SetVertexValue(noteEventVertex, noteEvent.Get(false, @"Attribute:Octave"), octave);
            GraphUtil.SetVertexValue(noteEventVertex, noteEvent.Get(false, @"Attribute:Note"), note);
            GraphUtil.SetVertexValue(noteEventVertex, noteEvent.Get(false, @"Attribute:Velocity"), velocity);                        
        }

        private void UpdateCCVertex(IEdge ccEventEdge, int triggerTime, int number, int value)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex Event = r.Get(false, @"System\Lib\Music\Event");
            IVertex noteEvent = r.Get(false, @"System\Lib\Music\ControlChangeEvent");

            IVertex noteEventVertex = ccEventEdge.To;            

            GraphUtil.SetVertexValue(noteEventVertex, noteEvent.Get(false, @"Attribute:Number"), number);
            GraphUtil.SetVertexValue(noteEventVertex, noteEvent.Get(false, @"Attribute:Value"), value);
            GraphUtil.SetVertexValue(noteEventVertex, noteEvent.Get(false, @"Attribute:TriggerTime"), triggerTime);
        }

        protected override void DrawItems_Down()
        {
            List<IVertex> selectedVertexes = GetSelectedVertexes();

            if (CurrentControlChangeNumber == -1)
            {
                foreach (IEdge e in VisualizedVertex.GetAll(false, "Event:"))
                    if (GraphUtil.ExistQueryOut(e.To, "$Is", "NoteEvent"))
                        if (ApplyFilter_Down(e.To))
                            AddItem_Down(e, selectedVertexes, false, true);
            }
            else
            {
                foreach (IEdge e in VisualizedVertex.GetAll(false, "Event:"))
                    if (GraphUtil.ExistQueryOut(e.To, "$Is", "ControlChangeEvent")
                        && GraphUtil.GetIntegerValue(e.To.Get(false, @"Number:")) == CurrentControlChangeNumber)
                        AddItem_Down(e, selectedVertexes, false, false);
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
