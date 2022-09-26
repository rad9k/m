using m0;
using m0.Foundation;
using m0.Graph;
using m0.UIWpf;
using m0.UIWpf.Visualisers.Helper;
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
    public partial class MelodyFlowVisualiserBase : ZoomScrollViewBasedVisualiserBase
    {
        protected IFlow Flow;
        
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

        protected virtual void CreateFlow()
        {
            Flow = new MelodyFlow(VisualizedVertex);
        }

        protected override void UpdateVariablesFromBaseVertex()
        {
            VisualizedVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (VisualizedVertex == null)
                return;

            if (VisualizedVertex.Get(false, "$Is:MelodyFlow") == null)
            {
                VisualizedVertex = null;
                return;
            }

            CreateFlow();

            Length = Flow.GetNumberOfSteps();

            IVertex r = MinusZero.Instance.Root;


            if (IsDrum)
            {
                IVertex pitchSetVertex = VisualizedVertex.Get(false, "PitchSet:");

                if (pitchSetVertex != null)
                    verticalSpanVertex = pitchSetVertex;
                else
                    verticalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultDrumPitchSet:");
            }
            else
                verticalSpanVertex = r.Get(false, @"System\Lib\Music\Generator\Data\FlowPitchSet");
         
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
                IntegerSpanAxisDecorator TimeSpanAD = new IntegerSpanAxisDecorator(this);

                TimeSpanAD.BoldLineCount = 5;

                HorizontalAD = TimeSpanAD;

                HorizontalAD.SetBaseVertex(horizontalSpanVertex);

                HorizontalAD.ValueSpaceMax = Length;
            }

            ZoomScrollView.SetVerticalAxisDecorator(VerticalAD);

            ZoomScrollView.SetHorizontalAxisDecorator(HorizontalAD);
        }

        protected override void SetupLocalVariablesFromBaseVertexVertexes()
        {
            IsDrum = GraphUtil.GetBooleanValueOrFalse(VisualizedVertex.Get(false, "IsDrum:"));

            if (IsDrum)
                IsCurrentPenItemCenter = true;
        }
        protected override void EdgeRemoved(IEdge edge)
        {
            IEdge quantEdge = GraphUtil.GetQueryOutFirstEdge(edge.To, Flow.StepToQuantMeta, null);

            if (quantEdge != null)
                RemoveItemByEdge(quantEdge);
        }

        protected void UpdateItem(IEdge itemEdge, IItem item)
        {
            IVertex quantVertex = itemEdge.To;

            IFlowStep step;
            int stepCount;
            IFlowQuant quant = Flow.GetQuantAndStepFromQuantVertex(quantVertex, out stepCount, out step);


            IVertex pitchVertex = MusicUtil.GetNoteFromPitchSet(verticalSpanVertex,
                quant.Octave,
                quant.Note);

            string label = pitchVertex.Value.ToString();

            FrameworkElement newElement = (FrameworkElement)item;

            newElement.Tag = quant;

            AxisSegment itemSegment = GetVerticalSegment(pitchVertex);


            double startPosition = MusicTimeToScreenPosition(stepCount, true);

            double endPosition = MusicTimeToScreenPosition(stepCount + 1, true);


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
            IVertex quantVertex = itemEdge.To;

            if (itemEdge.Meta.ToString() != Flow.StepToQuantMeta)
                return;
            
            FrameworkElement newElement;

            if (IsDrum)
                newElement = new DrumItem(itemEdge, this, ShowVelocity);
            else
                newElement = new NoteItem(itemEdge, this, ShowLabel, ShowVelocity);

            IItem newItem = (IItem)newElement;

            UpdateItem(itemEdge, newItem);

            if (selectedVertexes != null && selectedVertexes.Contains(quantVertex))
            {
                newItem.SelectHighlight();
                PreviousSelectedItemContext = MainDownEnum.Main;
            }

            ItemsAdd(newItem);

            if (MainItemsSyncedWithDown)
                AddItemByEdge_Down(itemEdge, selectedVertexes, false, true);
        }

        protected override IEdge AddItemVertex(AxisSegment itemSegment, double startPosition, double lengthPosition)
        {
            int note = GraphUtil.GetIntegerValueOr0(itemSegment.BaseVertex.Get(false, "Note:"));
            int octave = GraphUtil.GetIntegerValueOr0(itemSegment.BaseVertex.Get(false, "Octave:"));

            int step = ScreenPositionToMusicTime(startPosition, true);

            return AddQuantEdge(note, octave, step, -1, true);
        }

        IEdge AddQuantEdge(int note, int octave, int step, int velocity, bool insertAfter)
        {
            //IFlowQuant quant = new MelodyFlowQuant(Flow);
            IFlowQuant quant = Flow.CreateQuant();

            quant.Note = note;
            quant.Octave = octave;

            if (velocity == -1)
                quant.Velocity = DefaultVelocity;
            else
                quant.Velocity = velocity;

            IEdge newEdge = quant.PutOrMoveToStep(step);

            return newEdge;
        }

        protected override void DrawItems()
        {
            ISet<IVertex> selectedVertexes = ((ListVisualiserHelper)VisualiserHelper).GetSelectedVertexes();

            for (int stepCnt = 0; stepCnt < Flow.GetNumberOfSteps(); stepCnt++)
            {
                IFlowStep step = Flow.GetStep(stepCnt);

                foreach (IFlowQuant quant in step.Quants)
                    AddItemByEdge(quant.QuantEdge, selectedVertexes);
            }
        }

        protected override void UpdateItem_VerticalPosition(IItem item)
        {
            FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            IFlowQuant quant = null;

            if (element.Tag is IFlowQuant)
                quant = (IFlowQuant)element.Tag;

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

            IFlowQuant quant = null;

            if (element.Tag is IFlowQuant)
                quant = (IFlowQuant)element.Tag;

            if (quant == null)
                return;

            //             

            int oldStep = GetStepFromVertex(quant.QuantVertex);

            int newStep;

            if (item is NoteItem)
                newStep = ScreenPositionToMusicTime(item.Left, true);
            else
                newStep = ScreenPositionToMusicTime(item.HorizontalCenter, true);

            //VertexChangeOff = true;

            if (oldStep != newStep)
            {            
             //   if (newStep > oldStep)
             //       newStep++;

                quant.PutOrMoveToStep(newStep);

            //    if (newStep < oldStep)
           //         oldStep++; // corection for delete

              //  if (Flow.GetStep(oldStep).Quants.Count == 0)
                //    Flow.RemoveStep(oldStep);                
            }

            //DoCleanUpAndVisualiserDraw();

            //VertexChangeOff = false;
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

            IFlowStep so;

            Flow.GetQuantAndStepFromQuantVertex(v, out step, out so);

            return step;
        }

        protected override int FindLastPosition(IEnumerable<IEdge> edges)
        {
            int last = 0;

            foreach (IEdge e in edges)
            {
                IVertex v = e.To.Get(false, "To:");

                if (v.Get(false, Flow.IsQuantMeta) != null)
                {
                    int step = GetStepFromVertex(v);

                    if (last < step)
                        last = step;
                }
            }

            return last + 1;
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

            onlyCopy = true;

            foreach (IEdge e in edges)
            {
                if (GeneralUtil.CompareStrings(e.Meta, "ClipboardCut"))
                    onlyCopy = false;

                //IEdge edge = Edge.GetIEdgeByEdgeVertex(e.To);

                //IVertex v = edge.To;                

                IVertex v = e.To.Get(false, "To:");

                if (v.Get(false, Flow.IsQuantMeta) != null)
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
            int minPosition, maxPosition;

            bool onlyCopy;

            WhatIsInEdgesEnum whatIsClipboard = GetWhatIsInEdges(edges, out minPosition, out maxPosition, out onlyCopy);

            maxPosition = 0;

            foreach (IEdge e in edges)
            {                
                IVertex sourceQuantVertex = e.To.Get(false, "To:");

                bool isClipboardCopy = false;
                bool isClipboardCut = false;

                if (GeneralUtil.CompareStrings(e.Meta, "ClipboardCopy"))
                    isClipboardCopy = true;

                if (GeneralUtil.CompareStrings(e.Meta, "ClipboardCut"))
                    isClipboardCut = true;

                if (isClipboardCopy || isClipboardCut)
                {
                    if (sourceQuantVertex.Get(false, Flow.IsQuantMeta) != null)
                    {
                        IVertex newQuantVertex = null;
                        IEdge newStepToQuantEdge = null;

                        int step = GetStepFromVertex(sourceQuantVertex);

                        int newStep = step - minPosition + PositionMark;

                        if (isClipboardCopy)
                        {
                            newStepToQuantEdge = AddQuantEdge(GraphUtil.GetIntegerValueOr0(sourceQuantVertex.Get(false, "Note:")),
                                GraphUtil.GetIntegerValueOr0(sourceQuantVertex.Get(false, "Octave:")),
                                newStep,
                                GraphUtil.GetIntegerValueOr0(sourceQuantVertex.Get(false, "Velocity:")), false);

                            newStep = GetStepFromVertex(newStepToQuantEdge.To);
                        }

                        if (isClipboardCut)
                        {
                            newQuantVertex = sourceQuantVertex;

                            UpdateQuantVertex(sourceQuantVertex,
                                GraphUtil.GetIntegerValueOr0(sourceQuantVertex.Get(false, "Octave:")),
                                GraphUtil.GetIntegerValueOr0(sourceQuantVertex.Get(false, "Note:")),
                                newStep,
                                GraphUtil.GetIntegerValueOr0(sourceQuantVertex.Get(false, "Velocity:")));

                            newStepToQuantEdge = GetStepToQuantEdgeFromQuantVertes(newQuantVertex);

                            newStep = GetStepFromVertex(newStepToQuantEdge.To);
                        }

                        if (newStep > maxPosition)
                            maxPosition = newStep;

                        AddToSelectedEdges(newStepToQuantEdge);
                    }
                }
            }

            PositionMark = MusicTimeSnapCorrect_Up(maxPosition) + 1;

            PreviousSelectedItemContext = MainDownEnum.Main;

            DoStepsCleanUp();
        }

        private IEdge GetStepToQuantEdgeFromQuantVertes(IVertex quantVertex)
        {
            return GraphUtil.GetQueryInFirstEdge(quantVertex, Flow.StepToQuantMeta, null);
        }

        private void UpdateQuantVertex(IVertex quantVertex, int octave, int note, int newStep, int velocity)
        {
            int step;

            IFlowStep so;

            IFlowQuant quant = Flow.GetQuantAndStepFromQuantVertex(quantVertex, out step, out so);

            quant.Note = note;
            quant.Octave = octave;

            quant.Velocity = velocity;

            IEdge newEdge = quant.PutOrMoveToStep(newStep);
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
            if (v.Get(false, Flow.IsQuantMeta) != null)
                return true;

            return false;
        }

        void DoStepsCleanUp()
        {
            int newNumberOfSteps;

            Flow.GetNumberOfStepsAndCleanUp(out newNumberOfSteps);

            HorizontalAD.ValueSpaceMax = newNumberOfSteps;
        }

        protected override void After_ArrowUp_FromMove()
        {
            DoStepsCleanUpAndVisualiserDraw();
            //DoStepsCleanUp(); // need to draw. can check if StepsCleanUp has wasThereChange = true
        }

        protected void DoStepsCleanUpAndVisualiserDraw()
        {            
            DoStepsCleanUp();

            VisualiserDraw();
        }

        protected override void PerformPenUp_part2()
        {
            SetCursorMode(CursorStateEnum.PenUp);

            //DoCleanUpAndVisualiserDraw();
        }

        protected override void AddItemByEdge_Down(IEdge itemEdge, ISet<IVertex> selectedVertexes, bool isUpdate, bool isNoteEvent)
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
                item.SelectHighlight();
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
            ISet<IVertex> selectedVertexes = ((ListVisualiserHelper)VisualiserHelper).GetSelectedVertexes();            

            for (int stepCnt = 0; stepCnt < Flow.GetNumberOfSteps(); stepCnt++)
            {
                IFlowStep step = Flow.GetStep(stepCnt);

                foreach (MelodyFlowQuant quant in step.Quants)
                    if (ApplyFilter_Down(quant.QuantVertex))
                        AddItemByEdge_Down(quant.QuantEdge, selectedVertexes, false, false);
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

        public override double GetSnappedPosition(double position)
        {
            double CurrentSnapToGridValue_corrected = CurrentSnapToGridValue * 16;

            if (CurrentSnapToGrid == SnapToGridEnum.No_Snap)
                return position;

            double positionInBars = (position / HorizontalAD.BaseUnitSize) / HorizontalAD.SegmentLength;

            double reminder = positionInBars % CurrentSnapToGridValue_corrected;


            return (positionInBars - reminder) * HorizontalAD.SegmentLength * HorizontalAD.BaseUnitSize;
        }

        protected override int MusicTimeSnapCorrect_Up(int toCorrect)
        {
            return toCorrect;
        }

        protected override void RemoveItemVertex(IItem i)
        {
            IEdge eventEdge = i.BaseEdge;

            GraphUtil.DeleteEdgeByToVertex(VisualizedVertex, eventEdge.To);

            IFlowQuant quant = Flow.GetQuantFromVertex(eventEdge.To);

            quant.Remove();

            EdgeHelper.DeleteVertexByEdgeTo(Vertex.Get(false, "SelectedEdges:"), eventEdge.To);

            NeedToRebuildItemsDictionary = true;

            Items.Remove((FrameworkElement)i);

            i.Remove();
        }

        protected override List<IItem> GetSelectedAndMouseOverItems(MainDownEnum actualContext)
        {
            //if (actualContext != PreviousSelectedItemContext)
            //  UnselectAllSelectedItems();

            List<IItem> selectedItems = GetSelectedItems();

            if (!selectedItems.Contains(MouseOverItem))
                selectedItems.Add(MouseOverItem);

            return selectedItems;
        }
    }
}
