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
using System.Linq;

namespace m0_COMPOSER.UIWpf.Visualisers
{
    public class AxisEdgeComparer : IComparer<IEdge>
    {
        string AxisMetaString;        
        
        public int Compare(IEdge x, IEdge y)
        {
            if (x == null || y == null)
                return 0;

            IVertex xv = GraphUtil.GetQueryOutFirst(x.To, AxisMetaString, null);
            IVertex yv = GraphUtil.GetQueryOutFirst(y.To, AxisMetaString, null);

            if (xv == null || yv == null)
                return 0;

            
            bool isXnull = false, isYnull = false;

            double xValue = GraphUtil.GetDoubleValue(xv, ref isXnull);
            double yValue = GraphUtil.GetDoubleValue(yv, ref isYnull);

            if (isXnull || isYnull)
                return 0;

            if (xValue == yValue)
                return 0;

            if (yValue < xValue)
                return 1;

            return -1;            
        }

        public AxisEdgeComparer(string _AxisMetaString)
        {
            AxisMetaString = _AxisMetaString;            
        }
    }

    /// <summary>
    /// Interaction logic for SequenceVisualiser.xaml
    /// </summary>
    public partial class Set2DVisualiser : ZoomScrollViewBasedVisualiserBase, INoDownVisualiser
    {
        // Set2D beg

        IList<Line> PointLinesList = new List<Line>();

        protected bool ShowToolbarNames;
        protected bool ConnectPoints;
        protected bool CanEdit;

        public double ScaleLinesDensity;

        IVertex SetItemsDefiningMeta;
        IVertex SetItemsDefiningMetaIs;
        string SetItemsDefiningMetaString;

        IVertex SetItemHorizontalAxisMetaVertex;
        IVertex SetItemVerticalAxisMetaVertex;

        public string SetItemHorizontalAxisMetaString;
        public string SetItemVerticalAxisMetaString;

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

        static string[] _MetaTriggeringUpdateVertex = new string[] { "ShowArrowLines", "CanEdit", "ConnectPoints", "ShowToolbarNames", "ScaleLinesDensity" };
        public override string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { };
        public override string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public void InitXAMLInstances()
        {
            PenButton = PenButton_Instance;
            ArrowButton = ArrowButton_Instance;
            EraseButton = EraseButton_Instance;
            
            CopyButton = CopyButton_Instance;
            PasteButton = PasteButton_Instance;

            ZoomScrollView = ZoomScrollView_Instance;
        }

        public Set2DVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
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

            PositionMarkEnabled = false;

            ZoomSliderZero = true;

            IsCurrentPenItemCenter = true;

            //            

            ZoomScrollViewBasedVisualiserBase_Init(baseEdgeVertex, parentVisualiser);

            ZoomScrollView.ScrollViewer.Loaded += ScrollViewer_Loaded;
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
            Dictionary<IVertex, int> metaCount = new Dictionary<IVertex, int>();

            foreach (IEdge e in VisualizedVertex)
                if (VisualiserUtil.FilterEdge(e, this.Vertex)) {
                    if (metaCount.ContainsKey(e.Meta))
                        metaCount[e.Meta]++;
                    else
                        metaCount.Add(e.Meta, 1);
                }

            SetItemsDefiningMetaComboBox.Items.Clear();

            bool isFirst = true;


            foreach (KeyValuePair<IVertex,int> kvp in metaCount)
                if(kvp.Value > 1)                    
                    {
                        ComboBoxItem i = new ComboBoxItem();
                        i.Content = kvp.Key.Value;
                        i.Tag = kvp.Key;
                        SetItemsDefiningMetaComboBox.Items.Add(i);

                        if (isFirst)
                        {
                            isFirst = false;
                            i.IsSelected = true;
                        }
                    }
        }

        bool VisualizedVertexToUpdated_executed = false;

        protected override INoInEdgeInOutVertexVertex CheckBaseEdgeChange(IExecution exe)
        {
            IVertex baseEdgeTo = VisualiserHelper.Vertex.Get(false, @"BaseEdge:\To:");

            if (baseEdgeTo != null && !VisualizedVertexToUpdated_executed)
            {
                ComboBoxesUpdate();
                VisualizedVertexToUpdated_executed = true;
            }

            WasThereEdgeAddedRemovedDisposed = false;

            ExecutionFlowHelper.DoAddRemoveDisposeAddEdgeByMetaOrValueChangeHandlers(exe.Stack, new List<EventHandlers>()
            { new EventHandlers(
                baseEdgeTo,
                EdgeAdded,
                EdgeRemoved,
                EdgeDisposed,                
                new string[] {SetItemVerticalAxisMetaString, SetItemHorizontalAxisMetaString},
                new string[] {SetItemVerticalAxisMetaString, SetItemHorizontalAxisMetaString},
                new string[] {SetItemsDefiningMetaString },
                AddEdgeByMetaOrValueChangeHandler
                )
            });

            if (WasThereEdgeAddedRemovedDisposed)
                LinesUpdateIfNeeded();
            
            return exe.Stack;
        }

        public override void BaseEdgeToUpdated()
        {
            IVertex bas = Vertex.Get(false, @"BaseEdge:\To:");

            if (bas != null)
            {
                UpdateVertexValues();

                UpdateVariablesFromBaseVertex();

                // SnapToGridComboBox_SelectionChange(); in UpdateVertexValues() does this;
                VisualiserDraw();
            }
        }

        protected override void EdgeAdded(IEdge edge)
        {
            AddItemByEdge(edge, SelectedVertexes);

            WasThereEdgeAddedRemovedDisposed = true;
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

            ScaleLinesDensity = GraphUtil.GetDoubleValue(Vertex.Get(false, "ScaleLinesDensity:"), ref dummy);

            VisualiserDraw();

            ShowToolbarNames_SelectionChange();

            if (CanEdit)
                PenButton.IsEnabled = true;
            else
                PenButton.IsEnabled = false;
        }

        protected void ShowToolbarNames_SelectionChange()
        {
            if (ShowToolbarNames)
            {
                SetButtonComponentName(PenButton, "New");
                SetButtonComponentName(ArrowButton, "Select");
                SetButtonComponentName(EraseButton, "Erase");
                                         
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

        Point GetPointFromVertex(IVertex itemVertex)
        {
            double horizontalValue = GetHorizontal(itemVertex);
            double horizontalPosition = HorizontalAD.ValueSpaceToScreen(horizontalValue);

            double verticalValue = GetVertical(itemVertex);
            double verticalPosition = VerticalAD.ValueSpaceToScreen(verticalValue);

            return new Point(horizontalPosition, verticalPosition);
        }

        protected void UpdateItem(IEdge itemEdge, IItem item)
        {
            IVertex itemVertex = itemEdge.To;

            Point point = GetPointFromVertex(itemVertex);                       

            FrameworkElement newElement = (FrameworkElement)item;                        
            
            item.HorizontalCenter = point.X;
            item.VerticalCenter = point.Y;
            
            item.Update();
        }

        protected override void AddItemByEdge(IEdge itemEdge, ISet<IVertex> selectedVertexes)
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

        protected IList<IEdge> GetItemEdges()
        {
            //return VisualizedVertex.GetAll(false, SetItemsDefiningMetaString + ":");

            return GraphUtil.GetQueryOut(VisualizedVertex, SetItemsDefiningMetaString, null);
        }

        //int cnt = 0;

        protected override void DrawItems()
        {
             if (!canDraw)
                return;

          //  cnt++;

            //if (cnt < 5)
              //  return;

            ItemDictionary.RemoveAllByHost(this);

            ISet<IVertex> selectedVertexes = ((ListVisualiserHelper)VisualiserHelper).GetSelectedVertexes();

            IList<IEdge> itemEdges = GetItemEdges();

            if (ConnectPoints)
                DrawPointLines(itemEdges);

            foreach (IEdge e in itemEdges)
                AddItemByEdge(e, selectedVertexes);            
        }

        void DeleteLines() // delete old lines
        {
            foreach (Line l in PointLinesList.ToList())
            {
                Main.Children.Remove(l);
                PointLinesList.Remove(l);
            }
        }

        protected void DrawPointLines(IList<IEdge> itemEdges)
        {
            DeleteLines();

            List<IEdge> sortedItemEdges = itemEdges.ToList();
            
            sortedItemEdges.Sort(new AxisEdgeComparer(SetItemHorizontalAxisMetaString));

            for(int cnt = 0; cnt < sortedItemEdges.Count -1; cnt++)
            {
                Point from = GetPointFromVertex(sortedItemEdges[cnt].To);
                Point to = GetPointFromVertex(sortedItemEdges[cnt + 1].To);

                Line l = new Line();

                WpfUtil.SetLinePosition(l, from.X, from.Y, to.X, to.Y);

                l.StrokeThickness = 1;

                l.Stroke = (Brush)WpfUtil.FindResource("0ForegroundBrush");

                Main.Children.Add(l);
                PointLinesList.Add(l);
            }
        }

        void LinesUpdateIfNeeded()
        {
            if (ConnectPoints)
            {
                IList<IEdge> itemEdges = GetItemEdges();

                DrawPointLines(itemEdges);
            }
        }

        protected override void After_ArrowUp_FromMove()
        {
            LinesUpdateIfNeeded();
        }

        protected override void UpdateItem_VerticalPosition(IItem item)
        {            
            FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            IVertex itemVertex = item.BaseEdge.To;

            double itemVerticalPosition_valueSpace = VerticalAD.ScreenToValueSpace(item.VerticalCenter);

            GraphUtil.SetVertexValue(itemVertex, SetItemVerticalAxisMetaVertex, itemVerticalPosition_valueSpace);

            item.Update();
        }

        protected override void UpdateItem_HorizontalPosition(IItem item)
        {
            FrameworkElement element;

            if (!(item is FrameworkElement))
                return;

            element = (FrameworkElement)item;

            IVertex itemVertex = item.BaseEdge.To;

            double itemHorizontalPosition_valueSpace = HorizontalAD.ScreenToValueSpace(item.HorizontalCenter);

            GraphUtil.SetVertexValue(itemVertex, SetItemHorizontalAxisMetaVertex, itemHorizontalPosition_valueSpace);

            item.Update();
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
               
        protected override void PasteEdgesFromClipboard(IEnumerable<IEdge> edges)
        {            
            foreach (IEdge e in edges)
            {
                IEdge edge = EdgeHelper.GetIEdgeByEdgeVertex(e.To);

                IVertex v = edge.To;

                bool isClipboardCopy = false;

                if (GeneralUtil.CompareStrings(e.Meta, "ClipboardCopy"))
                    isClipboardCopy = true;
                
                if (isClipboardCopy)
                {
                    IEdge newEdge = GraphUtil.CopyEdgeIntoVertexOneLevel(edge, VisualizedVertex);
                        
                    AddToSelectedEdges(newEdge);   
                }                
            }

            PreviousSelectedItemContext = MainDownEnum.Main;
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
        bool ommit_SetItemHorizontalAxisMetaComboBox_SelectionChanged = false;

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
                        {
                            ommit_SetItemHorizontalAxisMetaComboBox_SelectionChanged = true;
                            i.IsSelected = true;
                        }

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


            if (ommit_SetItemHorizontalAxisMetaComboBox_SelectionChanged)                            
                ommit_SetItemHorizontalAxisMetaComboBox_SelectionChanged = false;
            else
                canDraw = true;

            UpdateAxisAndDraw();            
        }

        private void SetItemVerticalAxisMetaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SetItemVerticalAxisMetaComboBox.SelectedItem == null)
                return;

            SetItemVerticalAxisMetaVertex = (IVertex)((ComboBoxItem)SetItemVerticalAxisMetaComboBox.SelectedItem).Tag;

            SetItemVerticalAxisMetaString = SetItemVerticalAxisMetaVertex.Value.ToString();

            canDraw = true;

            UpdateAxisAndDraw();
        }

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

            foreach (IEdge e in GetItemEdges())
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
