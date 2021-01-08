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
    public partial class SequenceVisualiser : ZoomScrollViewBasedVisualiserBase
    {        
        public void InitXAMLInstances()
        {
            PenButton = PenButton_Instance;
            ArrowButton = ArrowButton_Instance;
            EraseButton = EraseButton_Instance;
            TruncateButton = TruncateButton_Instance;
            ExtendButton = ExtendButton_Instance;

            ZoomScrollView = ZoomScrollView_Instance;
        }

        public SequenceVisualiser()
        {
            InitializeComponent();

            //

            MinusZero mz = MinusZero.Instance;

            VisualiserName = "SequenceVisuliser";

            BaseEdgeToMetaVertex = mz.root.Get(false, @"System\Lib\Music\Class:Sequence");
            VisualiserMetaVertex = mz.root.Get(false, @"System\Meta\Visualiser\Sequence");

            //

            InitXAMLInstances();

            //

            HasDown = true;

            //

            ZoomScrollViewBasedVisualiserBase_Init();
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
            baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (baseVertex == null)
                return;            

            if (baseVertex.Get(false, "$Is:Sequence") == null)
            {
                baseVertex = null;
                return;
            }

            IVertex r = MinusZero.Instance.Root;

            verticalSpanVertex = baseVertex.Get(false, "PitchSet:");

            if (verticalSpanVertex == null)
                if (IsDrum)
                    verticalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultDrumPitchSet:");
                else
                    verticalSpanVertex = r.Get(false, @"System\Lib\Music\Data\DefaultPitchSet:");

            horizontalSpanVertex = baseVertex.Get(false, "TimeSpan:");

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
                MusicTimeSpanAxisDecorator TimeSpanAD = new MusicTimeSpanAxisDecorator();
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
            if (baseVertex.Get(false, "ExtendTimeLength:") != null)
                ExtendTimeLength = (int)GraphUtil.GetIntegerValue(baseVertex.Get(false, "ExtendTimeLength:"));
            else
                ExtendTimeLength = Midi.Standard.MidiTicksPerSixteen * 16; // default

            if (baseVertex.Get(false, "Length:") != null)
                Length = (int)GraphUtil.GetIntegerValue(baseVertex.Get(false, "Length:"));
            else
                Length = ExtendTimeLength;

            SaveLength();


            bool dummy = false;

            IsDrum = GraphUtil.GetBooleanValue(baseVertex.Get(false, "IsDrum:"), ref dummy);

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
                newItem.Select();
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

        protected override IEdge AddItemEdge(AxisSegment itemSegment, double startPosition, double lengthPosition)
        {
            IVertex r = MinusZero.Instance.Root;

            IVertex Event = r.Get(false, @"System\Lib\Music\Event");
            IVertex noteEvent = r.Get(false, @"System\Lib\Music\NoteEvent");

            IEdge tempNoteEventEdge = baseVertex.AddVertexAndReturnEdge(null, null);

            IVertex noteEventVertex = tempNoteEventEdge.To;

            noteEventVertex.AddEdge(MinusZero.Instance.Is, noteEvent);
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Velocity"), DefaultVelocity);
            noteEventVertex.AddEdge(noteEvent.Get(false, @"Attribute:Octave"), itemSegment.BaseVertex.Get(false, "Octave:"));
            noteEventVertex.AddEdge(noteEvent.Get(false, @"Attribute:Note"), itemSegment.BaseVertex.Get(false, "Note:"));
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:TriggerTime"), (int)((startPosition / HorizontalAD.BaseUnitSize) + 0.01));
            noteEventVertex.AddVertex(noteEvent.Get(false, @"Attribute:Length"), (int)((lengthPosition / HorizontalAD.BaseUnitSize) + 0.01));

            IEdge finalEdge = baseVertex.AddEdge(Event, noteEventVertex);

            baseVertex.DeleteEdge(tempNoteEventEdge);

            return finalEdge;
        }

        protected override void DrawItems()
        {
            List<IVertex> selectedVertexes = GetSelectedVertexes();

            foreach (IEdge e in baseVertex.GetAll(false, "Event:"))
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

            GraphUtil.CreateOrReplaceEdge(noteEventVertex, metaOctave, octaveVertex);
            GraphUtil.CreateOrReplaceEdge(noteEventVertex, metaNote, noteVertex);

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

            GraphUtil.SetVertexValue(itemVertex, metaTriggerTime, TriggerTime);

            if (Length != 0)
                GraphUtil.SetVertexValue(itemVertex, metaLength, Length);
        }
    }
}
