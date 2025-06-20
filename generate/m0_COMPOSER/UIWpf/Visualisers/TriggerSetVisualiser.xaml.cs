using m0;
using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf;
using m0.UIWpf.Controls;
using m0.UIWpf.Visualisers;
using m0.UIWpf.Visualisers.Helper;
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
    public partial class TriggerSetVisualiser : ZoomScrollViewBasedVisualiserBase, INoDownVisualiser
    {
        static string[] _MetaTriggeringUpdateVertex = new string[] { "ShowSnapLines", "ShowLabel", "ShowVelocity:", "DefaultVelocity", "SnapToGrid" };
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

        public TriggerSetVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            InitializeComponent();

            //

            MinusZero mz = MinusZero.Instance;

            VisualiserName = "TriggerSetVisuliser";

            BaseEdgeToMetaVertex = mz.root.Get(false, @"System\Lib\Music\Generator\Class:TriggerSet");
            VisualiserMetaVertex = mz.root.Get(false, @"System\Meta\Visualiser\TriggerSet");

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
                new string[] { },
                new string[] {"TriggerTime", "Length", "Velocity"},
                new string[] {"Trigger" },
                AddEdgeByMetaOrValueChangeHandler
                )
            });

            return exe.Stack;
        }

        protected void AddEdgeByMetaOrValueChangeHandler(IEdge eventEdge)
        {
            VisualiserDraw();
        }

        protected override void UpdateVertexValues()
        {
            IVertex r = MinusZero.Instance.root;

            bool dummy = false;

            ShowLabel = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowLabel:"), ref dummy);
            ShowVelocity = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowVelocity:"), ref dummy);
            ShowSnapLines = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowSnapLines:"), ref dummy);
            DefaultVelocity = GraphUtil.GetIntegerValue(Vertex.Get(false, "DefaultVelocity:"), ref dummy);

            if (Vertex.Get(false, "SnapToGrid:") == null || Vertex.Get(false, "SnapToGrid:").Value.ToString() == "")
                GraphUtil.ReplaceEdge(Vertex, r.Get(false, @"System\Meta\Visualiser\TriggerSet\SnapToGrid"), r.Get(false, @"System\Meta\Visualiser\SnapToGridEnum\'1/16 bar'"));

            SnapToGridComboBox_SelectionChange();
        }

        protected override void UpdateVariablesFromBaseVertex()
        {
            VisualizedVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (VisualizedVertex == null)
                return;

            if (VisualizedVertex.Get(false, "$Is:TriggerSet") == null)
            {
                VisualizedVertex = null;
                return;
            }

            IVertex r = MinusZero.Instance.Root;

            horizontalSpanVertex = VisualizedVertex.Get(false, "TimeSpan:");

            if (horizontalSpanVertex == null)
                horizontalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultMusicTimeSpanLevel:");
        }

        protected override void SetAxisDecorators()
        {
            if (VerticalAD == null)
            {
                VerticalAD = new OneSegmentAxisDecorator();
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


            FrameworkElement newElement = (FrameworkElement)item;

            AxisSegment itemSegment = VerticalAD.Segments[0];


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
        static IVertex triggerMeta = r.Get(false, @"System\Lib\Music\Generator\Trigger");
        static IVertex triggerTimeMeta = triggerMeta.Get(false, @"Attribute:TriggerTime");
        static IVertex lengthMeta = triggerMeta.Get(false, @"Attribute:Length");
        static IVertex velocityMeta = triggerMeta.Get(false, @"Attribute:Velocity");

        protected override IEdge AddItemVertex(AxisSegment itemSegment, double startPosition, double lengthPosition)
        {            
            IEdge eventEdge = VisualizedVertex.AddVertexAndReturnEdge(triggerMeta, null);

            IVertex noteEventVertex = eventEdge.To;

            noteEventVertex.AddEdge(MinusZero.Instance.Is, triggerMeta);

            noteEventVertex.AddVertex(triggerTimeMeta, (int)((startPosition / HorizontalAD.BaseUnitSize) + 0.01));
            noteEventVertex.AddVertex(lengthMeta, (int)((lengthPosition / HorizontalAD.BaseUnitSize) + 0.01));
            noteEventVertex.AddVertex(velocityMeta, DefaultVelocity);
            
            return eventEdge;
        }

        protected override void DrawItems()
        {
            ISet<IVertex> selectedVertexes = ((ListVisualiserHelper)VisualiserHelper).GetSelectedVertexes();

            foreach (IEdge e in VisualizedVertex.GetAll(false, "Trigger:"))
                //if (GraphUtil.ExistQueryOut(e.To, "$Is", "Trigger"))
                AddItemByEdge(e, selectedVertexes);
        }

        protected override void UpdateItem_VerticalPosition(IItem item)
        {

        }        

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
                TriggerTime = (int)(item.Left / HorizontalAD.BaseUnitSize);

                Length = (int)(itemWidth / HorizontalAD.BaseUnitSize);
            }

            GraphUtil.SetVertexValue(itemVertex, triggerTimeMeta, TriggerTime);

            if (Length != 0)
                GraphUtil.SetVertexValue(itemVertex, lengthMeta, Length);
        }

        protected override int ScreenPositionToMusicTime(double position, bool performSnapCorrection)
        {
            if (HorizontalAD == null)
                return 0;

            int musicTime = (int)(position / HorizontalAD.BaseUnitSize);

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

            foreach (IEdge e in edges)
            {
                IVertex v = e.To.Get(false, "To:");

                if (v.Get(false, "$Is:Trigger") != null)
                {
                    bool o = false;

                    int trigger = GraphUtil.GetIntegerValue(v.Get(false, "TriggerTime:"), ref o);

                    int length = GraphUtil.GetIntegerValue(v.Get(false, "Length:"), ref o);

                    int max = trigger + length;

                    if (last < max)
                        last = max;
                }
            }

            return last;
        }

        protected IEdge AddTriggerVertex(int triggerTime, int length, int velocity)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex triggerMeta = r.Get(false, @"System\Lib\Music\Generator\Trigger");

            IEdge noteEventEdge = VisualizedVertex.AddVertexAndReturnEdge(triggerMeta, null);

            IVertex noteEventVertex = noteEventEdge.To;

            noteEventVertex.AddEdge(MinusZero.Instance.Is, triggerMeta);

            noteEventVertex.AddVertex(triggerMeta.Get(false, @"Attribute:TriggerTime"), triggerTime);
            noteEventVertex.AddVertex(triggerMeta.Get(false, @"Attribute:Length"), length);
            noteEventVertex.AddVertex(triggerMeta.Get(false, @"Attribute:Velocity"), velocity);            

            return noteEventEdge;
        }

        // copy & paste rules for SequenceVisualiser
        //
        // what is selected before cut / paste | what is copied | what is selected after paste
        // ------------------------------------+----------------+---------------
        //                               notes | notes + cc     | notes
        //                     note velocities | notes + cc     | notes
        //                                  cc | cc             | cc

        protected enum WhatIsInEdgesEnum { OnlyNotes, OnlyCC, Mix }

        protected WhatIsInEdgesEnum GetWhatIsInEdges(IEnumerable<IEdge> edges, out int minPosition, out int maxPosition, out bool onlyCopy)
        {
            minPosition = Int32.MaxValue;
            maxPosition = Int32.MinValue;

            bool notes = false;

            bool cc = false;

            onlyCopy = true;

            foreach (IEdge e in edges)
            {
                if (GeneralUtil.CompareStrings(e.Meta, "ClipboardCut"))
                    onlyCopy = false;

                IEdge edge = EdgeHelper.GetIEdgeByEdgeVertex(e.To);

                IVertex v = edge.To;

                bool isNull = false;

                if (v.Get(false, "$Is:Trigger") != null)
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
            }

            return WhatIsInEdgesEnum.OnlyNotes;
        }

        protected override void PasteEdgesFromClipboard(IEnumerable<IEdge> edges)
        {
            bool o = false;

            int minPosition, maxPosition;

            bool onlyCopy;

            WhatIsInEdgesEnum whatIsClipboard = GetWhatIsInEdges(edges, out minPosition, out maxPosition, out onlyCopy);

            if (whatIsClipboard == WhatIsInEdgesEnum.Mix)
                return;

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

                    if (v.Get(false, "$Is:Trigger") != null)
                    {
                        int triggerTime = GraphUtil.GetIntegerValue(v.Get(false, "TriggerTime:"), ref o) - minPosition + PositionMark;
                        int length = GraphUtil.GetIntegerValue(v.Get(false, "Length:"), ref o);

                        if (triggerTime > maxPosition)
                            maxPosition = triggerTime;

                        if ((length + triggerTime) > maxPosition)
                            maxPosition = length + triggerTime;

                        if (isClipboardCopy)
                            newEdge = AddTriggerVertex(triggerTime,
                                length,
                                GraphUtil.GetIntegerValue(v.Get(false, "Velocity:"), ref o));

                        if (isClipboardCut)
                        {
                            newEdge = edge;

                            UpdateTriggerVertex(edge,
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

        private void UpdateTriggerVertex(IEdge noteEventEdge, int triggerTime, int length, int velocity)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex triggerMeta = r.Get(false, @"System\Lib\Music\Generator\Trigger");

            IVertex noteEventVertex = noteEventEdge.To;

            noteEventVertex.AddEdge(MinusZero.Instance.Is, triggerMeta);

            GraphUtil.SetVertexValue(noteEventVertex, triggerMeta.Get(false, @"Attribute:TriggerTime"), triggerTime);
            GraphUtil.SetVertexValue(noteEventVertex, triggerMeta.Get(false, @"Attribute:Length"), length);
            GraphUtil.SetVertexValue(noteEventVertex, triggerMeta.Get(false, @"Attribute:Velocity"), velocity);
        }

        //// TRIGGERSET SPECYFIC

        protected override IList<FrameworkElement> GetElementsAtFromListByArea(List<FrameworkElement> Items, double left, double top, double right, double bottom)
        {
            return WpfUtil.GetElementsAtFromListByArea_OnlyHorizontal(Items, left, top, right, bottom);
        }

        protected override void DrawItems_Down()
        {
            ISet<IVertex> selectedVertexes = ((ListVisualiserHelper)VisualiserHelper).GetSelectedVertexes();

            if (CurrentControlChangeNumber == -1)
            {
                foreach (IEdge e in VisualizedVertex.GetAll(false, "Trigger:"))
                    //if (GraphUtil.ExistQueryOut(e.To, "$Is", "NoteEvent"))
                    //if (ApplyFilter_Down(e.To))
                    AddItemByEdge_Down(e, selectedVertexes, false, true);
            }
        }

        static IVertex EventMeta = r.Get(false, @"System\Lib\Music\Event");
        static IVertex ControlChangeEventMeta = r.Get(false, @"System\Lib\Music\ControlChangeEvent");
        static IVertex NoteEventMeta = r.Get(false, @"System\Lib\Music\NoteEvent");

        protected override IEdge AddItemVertex_Down(double mouseY, double startPosition, out bool isUpdate, out bool isNoteEvent)
        {
            isUpdate = false;

            int triggerTime = (int)((startPosition / HorizontalAD.BaseUnitSize) + 0.01);

            IEdge eventEdge = null;

            isNoteEvent = false;

            List<IItem> existingItems = GetDownItemFromNumberTriggerTimeDictionary(CurrentControlChangeNumber, triggerTime);

            if (existingItems == null && MainItemsSyncedWithDown)
                return null;

            if (existingItems != null)
            {
                IItem item = existingItems[0];

                eventEdge = item.BaseEdge;

                isUpdate = true;

                //if (tempEventEdge.To.Get(false, @"$Is:NoteEvent") != null)
                isNoteEvent = true;
            }
            else
                eventEdge = VisualizedVertex.AddVertexAndReturnEdge(EventMeta, null);

            IVertex eventVertex = eventEdge.To;

            if (!isUpdate)
                eventVertex.AddEdge(MinusZero.Instance.Is, ControlChangeEventMeta);

            if (isNoteEvent)
                GraphUtil.SetVertexValue(eventVertex, NoteEventMeta.Get(false, @"Attribute:Velocity"), ControlChangeItem.getValueFromMouseY_Down(mouseY, Height_Down));
            else
            {
                GraphUtil.SetVertexValue(eventVertex, ControlChangeEventMeta.Get(false, @"Attribute:Number"), CurrentControlChangeNumber);
                GraphUtil.SetVertexValue(eventVertex, ControlChangeEventMeta.Get(false, @"Attribute:Value"), ControlChangeItem.getValueFromMouseY_Down(mouseY, Height_Down));
                GraphUtil.SetVertexValue(eventVertex, ControlChangeEventMeta.Get(false, @"Attribute:TriggerTime"), triggerTime);
            }            

            return eventEdge;
        }
    }
}
