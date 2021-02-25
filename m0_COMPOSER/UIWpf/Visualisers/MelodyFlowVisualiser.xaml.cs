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
    /// <summary>
    /// Interaction logic for SequenceVisualiser.xaml
    /// </summary>
    public partial class MelodyFlowVisualiser : ZoomScrollViewBasedVisualiserBase
    {
        MelodyFlow MelodyFlow;

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

        public MelodyFlowVisualiser()
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

            ZoomScrollViewBasedVisualiserBase_Init();

            ShowCCList = false;

            NewItemWidthOneSnapLimit = true;
        }

        protected override void UpdateVertexValues()
        {
            IVertex r = MinusZero.Instance.root;            

            bool dummy = false;

            ShowLabel = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowLabel:"), ref dummy);
            ShowVelocity = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowVelocity:"), ref dummy);
            ShowArowLines = GraphUtil.GetBooleanValue(Vertex.Get(false, "ShowArrowLines:"), ref dummy);            
            DefaultVelocity = GraphUtil.GetIntegerValue(Vertex.Get(false, "DefaultVelocity:"), ref dummy);

            VisualiserDraw();
        }        

        protected override void UpdateVariablesFromBaseVertex()
        {
            baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (baseVertex == null)
                return;            

            if (baseVertex.Get(false, "$Is:MelodyFlow") == null)
            {
                baseVertex = null;
                return;
            }

            MelodyFlow = new MelodyFlow(baseVertex);

            Length = MelodyFlow.GetNumberOfSteps();

            IVertex r = MinusZero.Instance.Root;

            
            if (IsDrum)
                verticalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultDrumPitchSet:");
            else
                verticalSpanVertex = r.Get(false, @"System\Lib\Music\Generator\Data\FlowPitchSet");

            horizontalSpanVertex = baseVertex.Get(false, "TimeSpan:");

            if (horizontalSpanVertex == null)
                horizontalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultNumberSpanLevel:");
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
                NumberSpanAxisDecorator TimeSpanAD = new NumberSpanAxisDecorator(this);

                TimeSpanAD.BoldLineCount = 5;

                HorizontalAD = TimeSpanAD;

                HorizontalAD.SetBaseVertex(horizontalSpanVertex);

                HorizontalAD.SetLength(Length);
            }

            ZoomScrollView.SetVerticalAxisDecorator(VerticalAD);

            ZoomScrollView.SetHorizontalAxisDecorator(HorizontalAD);
        }        

        protected override void SetupLocalVariablesFromBaseVertexVertexes()
        {            
            IsDrum = GraphUtil.GetBooleanValueOrFalse(baseVertex.Get(false, "IsDrum:"));

            if (IsDrum)
                IsCurrentPenItemCenter = true;
        }        

        protected override void AddItem(IEdge itemEdge, List<IVertex> selectedVertexes)
        {
            IVertex quantVertex = itemEdge.To;

            MelodyFlowStep step;
            int stepCount;
            MelodyFlowQuant quant = MelodyFlow.GetQuantAndStepFromQuantVertex(quantVertex, out stepCount, out step);
            

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(verticalSpanVertex,
                quant.Octave,
                quant.Note);

            string label = pitchVertex.Value.ToString();

            FrameworkElement newElement;

            if (IsDrum)
                newElement = new DrumItem(itemEdge, this, ShowVelocity);
            else
                newElement = new NoteItem(itemEdge, label, this, ShowLabel, ShowVelocity);

            IItem newItem = (IItem)newElement;

            if (selectedVertexes != null && selectedVertexes.Contains(quantVertex))
            {
                newItem.Select();
                PreviousSelectedItemContext = MainDownEnum.Main;
            }

            AxisSegment itemSegment = GetVerticalSegment(pitchVertex);


            double startPosition = MusicTimeToScreenPosition(stepCount, true);

            double endPosition = MusicTimeToScreenPosition(stepCount + 1, true);


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

        protected override IEdge AddItemEdge(AxisSegment itemSegment, double startPosition, double lengthPosition)
        {
            int note = GraphUtil.GetIntegerValueOr0(itemSegment.BaseVertex.Get(false, "Note:"));
            int octave = GraphUtil.GetIntegerValueOr0(itemSegment.BaseVertex.Get(false, "Octave:"));            

            int step = ScreenPositionToMusicTime(startPosition, true);
            
            return AddQuantEdge(note, octave, step, -1);
        }

        IEdge AddQuantEdge(int note, int octave, int step, int velocity)
        {
            MelodyFlowQuant quant = new MelodyFlowQuant(MelodyFlow);

            quant.Note = note;
            quant.Octave = octave;

            if(velocity == -1)
                quant.Velocity = DefaultVelocity;            
            else
                quant.Velocity = velocity;            

            IEdge newEdge = quant.PutOrMoveToStep(step);
            
            return newEdge;
        }

        protected override void DrawItems()
        {
            List<IVertex> selectedVertexes = GetSelectedVertexes();

            for (int stepCnt = 0; stepCnt < MelodyFlow.GetNumberOfSteps(); stepCnt++)
            {
                MelodyFlowStep step = MelodyFlow.GetStep(stepCnt);

                foreach (MelodyFlowQuant quant in step.Quants)
                    AddItem(quant.QuantEdge, selectedVertexes);
            }
        }

        protected override void UpdateItem_VerticalPosition(IItem item)
        {            
            FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            MelodyFlowQuant quant = null;

            if (element.Tag is MelodyFlowQuant)
                quant = (MelodyFlowQuant)element.Tag;

            if (quant == null)
                return;

            //
            
            AxisSegment segment = FindVerticalSegment(item.Top + 1);

            IVertex octaveVertex = segment.BaseVertex.Get(false, "Octave:");
            IVertex noteVertex = segment.BaseVertex.Get(false, "Note:");                        

            int octave = GraphUtil.GetIntegerValueOr0(octaveVertex);
            int note = GraphUtil.GetIntegerValueOr0(noteVertex);

            quant.Octave = octave;
            quant.Note = note;
            

            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(verticalSpanVertex, octave, note);

            string label = pitchVertex.Value.ToString();

            item.Label = label;

            item.Update();
        }

        protected override void UpdateItem_HorizontalPosition(IItem item)
        {
            FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            MelodyFlowQuant quant = null;

            if (element.Tag is MelodyFlowQuant)
                quant = (MelodyFlowQuant)element.Tag;

            if (quant == null)
                return;

            //

            int step = ScreenPositionToMusicTime(item.Left, true);

            quant.PutOrMoveToStep(step);            
        }

        protected override int ScreenPositionToMusicTime(double position, bool performSnapCorrection)
        {
            if (HorizontalAD == null)
                return 0;

            int musicTime = (int)(position / HorizontalAD.BaseUnitSize);

            return musicTime;
        }

        protected override double MusicTimeToScreenPosition(int musicTime, bool performSnapCorrection)
        {            
            if (HorizontalAD == null)
                return 0;

            return musicTime * HorizontalAD.BaseUnitSize;
        }

        int GetStepFromVertex(IVertex v)
        {
            int step;

            MelodyFlowStep so;

            MelodyFlow.GetQuantAndStepFromQuantVertex(v, out step, out so);

            return step;
        }

        protected override int FindLastPosition(IEnumerable<IEdge> edges)
        {
            int last = 0;

            foreach(IEdge e in edges)
            {
                IVertex v = e.To.Get(false, "To:");

                if(v.Get(false, "$Is:Quant") != null)
                {
                    int step = GetStepFromVertex(v);

                    if (last < step)
                        last = step;
                }
            }

            return last;
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

            onlyCopy = true;

            foreach(IEdge e in edges)
            {
                if (GeneralUtil.CompareStrings(e.Meta, "ClipboardCut"))
                    onlyCopy = false;

                IEdge edge = Edge.GetIEdgeByEdgeVertex(e.To);

                IVertex v = edge.To;                
                
                if (v.Get(false, "$Is:Quant") != null)
                {
                    int step = GetStepFromVertex(v);

                    if (step > maxPosition)
                        maxPosition = step;

                    if (step < minPosition)
                        minPosition = step;
                }
            }

            return WhatIsInEdgesEnum.Mix;
        }        

        protected override void PasteEdgesFromClipboard(IEnumerable<IEdge> edges)
        {
            bool o = false;            

            int minPosition, maxPosition;

            bool onlyCopy;

            WhatIsInEdgesEnum whatIsClipboard = GetWhatIsInEdges(edges, out minPosition, out maxPosition, out onlyCopy);
        
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

                    if (v.Get(false, "$Is:Quant") != null) // NOTE
                    {
                        int step = GetStepFromVertex(v);

                        int newStep = step - minPosition + PositionMark;                        

                        if (newStep > maxPosition)
                            maxPosition = newStep;                        

                        if (isClipboardCopy)
                            newEdge = AddQuantEdge(GraphUtil.GetIntegerValueOr0(v.Get(false, "Note:")),
                                GraphUtil.GetIntegerValueOr0(v.Get(false, "Octave:")),
                                newStep,                                
                                GraphUtil.GetIntegerValueOr0(v.Get(false, "Velocity:")));

                        if (isClipboardCut)
                        {
                            newEdge = edge;

                            UpdateQuantVertex(edge,
                                GraphUtil.GetIntegerValueOr0(v.Get(false, "Octave:")),
                                GraphUtil.GetIntegerValueOr0(v.Get(false, "Note:")),
                                newStep,                               
                                GraphUtil.GetIntegerValueOr0(v.Get(false, "Velocity:")));
                        }                       
                        
                       AddToSelectedEdges(newEdge);
                    }
                }                
            }

            PositionMark = MusicTimeSnapCorrect_Up(maxPosition);

            PreviousSelectedItemContext = MainDownEnum.Main;
        }

        private void UpdateQuantVertex(IEdge quantEdge, int octave, int note, int triggerTime, int velocity)
        {
            int step;

            MelodyFlowStep so;

            MelodyFlowQuant quant = MelodyFlow.GetQuantAndStepFromQuantVertex(quantEdge.To, out step, out so);
            
            quant.Note = note;
            quant.Octave = octave;
            
            quant.Velocity = velocity;

            int numberOfSteps = MelodyFlow.GetNumberOfSteps();

            IEdge newEdge = quant.PutOrMoveToStep(step);

            int newNumberOfSteps = MelodyFlow.GetNumberOfSteps();

            if (numberOfSteps != newNumberOfSteps)
                HorizontalAD.SetLength(newNumberOfSteps + 1);                        
        }        

        protected override double GetSnappedPosition(double position)
        {
            double CurrentSnapToGridValue_corrected = CurrentSnapToGridValue * 16;

            if (CurrentSnapToGrid == SnapToGridEnum.No_Snap)
                return position;

            double positionInBars = (position / HorizontalAD.BaseUnitSize) / HorizontalAD.SegmentLength;

            double reminder = positionInBars % CurrentSnapToGridValue_corrected;

            return (positionInBars - reminder) * HorizontalAD.SegmentLength * HorizontalAD.BaseUnitSize;
        }

        protected override void RebuildItemsDictionary_Down()
        {
            ItemsDictinaryHolder_Down.Clear();

            ItemsDictinaryHolder_Number_TriggerTime_Down.Clear();

            foreach (IItem i in Items_Down)
            {
                IVertex v = i.BaseEdge.To;
                ItemsDictinaryHolder_Down.Add(v, i);

                //

                int step = GetStepFromVertex(v);
                int number = -1;
                
                Dictionary<int, List<IItem>> itemsDictinaryHolder_TriggerTime_Down;

                if (ItemsDictinaryHolder_Number_TriggerTime_Down.ContainsKey(number))
                    itemsDictinaryHolder_TriggerTime_Down = ItemsDictinaryHolder_Number_TriggerTime_Down[number];
                else
                {
                    itemsDictinaryHolder_TriggerTime_Down = new Dictionary<int, List<IItem>>();
                    ItemsDictinaryHolder_Number_TriggerTime_Down.Add(number, itemsDictinaryHolder_TriggerTime_Down);
                }

                if (itemsDictinaryHolder_TriggerTime_Down.ContainsKey(step))
                    itemsDictinaryHolder_TriggerTime_Down[step].Add(i);
                else
                {
                    List<IItem> list = new List<IItem>();
                    list.Add(i);
                    itemsDictinaryHolder_TriggerTime_Down.Add(step, list);
                }
            }

            NeedToRebuildItemsDictionary_Down = false;
        }

        protected override bool IsVelocityHavingVertex(IVertex v)
        {
            if (v.Get(false, @"$Is:Quant") != null)
                return true;

            return false;
        }

        int numberOfStepsBefore;

        void GetNumberOfStepsBefore()
        {
            numberOfStepsBefore = MelodyFlow.GetNumberOfSteps();
        }

        void CheckNumberOfStepsAfter()
        {
            int numberOfStepsAfter = MelodyFlow.GetNumberOfSteps();

            if (numberOfStepsBefore != numberOfStepsAfter || numberOfStepsBefore == 1)
            {
                HorizontalAD.SetLength(numberOfStepsAfter + 1);

                VisualiserDraw();
            }
        }

        protected override void PerformPenUp_part1()
        {
            Main.Children.Remove(NewItemShape);

            GetNumberOfStepsBefore();
        }

        protected override void PerformPenUp_part2()
        {
            SetCursorMode(CursorStateEnum.PenUp);

            CheckNumberOfStepsAfter();
        }

        protected override void AddItem_Down(IEdge itemEdge, List<IVertex> selectedVertexes, bool isUpdate, bool isNoteEvent)
        {
            if (Height_Down == 0)
                return;

            IVertex itemEventVertex = itemEdge.To;            



            ControlChangeItem item = null;

            if (isUpdate)
                item = (ControlChangeItem)GetItemsDictionary_Down()[itemEdge.To];
            else
                item = new ControlChangeItem(itemEdge, this);

            IVertex itemVertex = item.BaseEdge.To;

            int step = GetStepFromVertex(itemVertex);
            
            int value;
            
            value = GraphUtil.GetIntegerValueOr0(itemVertex.Get(false, "Velocity:"));            

            if (selectedVertexes != null && selectedVertexes.Contains(itemEventVertex) && !isNoteEvent)
            {
                item.Select();
                PreviousSelectedItemContext = MainDownEnum.Down;
            }

            double startPosition = MusicTimeToScreenPosition(step, true);

            if (!isUpdate)
                ItemsAdd_Down(item); // need this as item.Canvas needs to be set for the cc top mark

            item.HorizontalCenter = startPosition;
            item.VerticalCenter = Height_Down - (((double)value / 127) * Height_Down);
        }

        protected override void DrawItems_Down()
        {
            List<IVertex> selectedVertexes = GetSelectedVertexes();

            for (int stepCnt = 0; stepCnt < MelodyFlow.GetNumberOfSteps(); stepCnt++)
            {
                MelodyFlowStep step = MelodyFlow.GetStep(stepCnt);

                foreach (MelodyFlowQuant quant in step.Quants)                    
                    AddItem_Down(quant.QuantEdge, selectedVertexes, false, false);
            }
        }

        protected override void ArrowMove_ArrowUp(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = GetMainContentMousePosition(e);

            FrameworkElement element = WpfUtil.GetElementAtFromList_StartFromEnd(Items, currentMousePosition);

            if (element != null && element is IItem)
            {
                IItem item = (IItem)element;

                ArrowMove_ArrowUp_SetMouseCurrentItem(item, CursorStateEnum.ArrowUp_MoveOnItem);
                return;
            }

            SetCursorMode(CursorStateEnum.ArrowUp);
            UpdateCursorShape();
        }
    }
}
