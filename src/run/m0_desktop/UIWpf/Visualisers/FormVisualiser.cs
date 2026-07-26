using m0.Foundation;
using m0.Graph;
using m0.UIWpf.Commands;
using m0.ZeroUML;
using m0.Util;
using m0.ZeroTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using m0.UIWpf.Visualisers.Method;
using m0.UIWpf.Visualisers.Helper;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Foundation;
using m0.UIWpf.Controls;
using System.Threading;

namespace m0.UIWpf.Visualisers
{
    public class ControlInfo
    {
        public FrameworkElement GapControl;
        public FrameworkElement MetaControl;
        public FrameworkElement DataControl;
        public IEdge BaseEdge;
        public int Column;
        public int Order;
    }

    public class SectionInfo
    {
        public Panel Panel;
        public int Column;
    }

    public class TabInfo
    {
        public int TotalNumberOfControls;
        public int CurrentNumberOfControls;
        public IDictionary<string, SectionInfo> Sections;
        public IDictionary<IVertex, ControlInfo> ControlInfos;
        public TabItem TabItem;
        public bool WidthCorrectionDone;

        public TabInfo()
        {
            Sections=new Dictionary<string,SectionInfo>();
            ControlInfos = new Dictionary<IVertex, ControlInfo>();
            TotalNumberOfControls=0;
            CurrentNumberOfControls = 0;
        }
    }

    public class FormVisualiser : ContentControl, IListVisualiser, ITypedEdge, IKeyboardHighlight
    {
        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        public bool SelectionProhibited { get; set; }

        bool DisplayBaseVertex = true; /////////////////////////////////////////

        bool SectionsAsTabs;
        bool MetaOnLeft;
        bool MetaAlignRight;
        bool ExpertMode;

        bool HasTabs { get; set; }
        int ColumnNumber { get; set; }
        IDictionary<string, TabInfo> TabList { get; set; }

        TabControl TabControl;

        double marginOnRight = 1;
        double marginBetweenColumns = 5;
        double sectionControlBorderWidth = 17;
        double metaVsDataSeparator = 4;
        double controlLineVsControlLineSeparator = 4;

        double lastCorrectedWidth = 0;
        bool widthCorrectionScheduled = false;
        bool widthCorrectionInProgress = false;
        private ControlInfo keyboardHighlightedControlInfo;
        private bool isBeforeFirstKeyboardPosition;
        private bool isAfterLastKeyboardPosition;
        private bool suppressNestedSelectedEdgesChange;
        private readonly IList<IEdge> ownSelectedEdges = new List<IEdge>();
        private ControlInfo pendingMouseDownControlInfo;
        private bool pendingMouseDownIsCtrl;
        private bool pendingWasInSelectionAtMouseDown;
        private bool pendingMouseDownHasNestedKeyboardHighlight;
        private int pendingMouseDownClickCount;
        private Point pendingMouseDownPoint;
        private bool suppressNextMouseUpSelection;
        private bool isFormDndDragging;
        private bool pendingMouseDownStartedOnSelfDraggingControl;
        private bool isBaseEdgeToUpdateInProgress;
        private bool isBaseEdgeToInitialized;
        private IVertex initializedBaseEdgeFrom;
        private IVertex initializedBaseEdgeMeta;
        private IVertex initializedBaseEdgeTo;


        TabItem TabControlSelectedItem;

        static string[] _MetaTriggeringUpdateVertex = new string[] { "ExpertMode", "ColumnNumber", "MetaOnLeft", "MetaAlignRight", "SectionsAsTabs" };
        
        public string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { };
        public string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public void ViewAttributesUpdated() { }

        public int CurrentHighlightPosition
        {
            get
            {
                List<ControlInfo> controls = GetKeyboardHighlightControlInfos(getActiveTabInfo());

                if (keyboardHighlightedControlInfo == null)
                    return -1;

                return controls.IndexOf(keyboardHighlightedControlInfo);
            }
        }

        public bool IsBeforeFirstPosition { get { return isBeforeFirstKeyboardPosition; } }

        public bool IsAfterLastPosition { get { return isAfterLastKeyboardPosition; } }

        public bool IsFirstPosition
        {
            get { return CurrentHighlightPosition == 0 && !isBeforeFirstKeyboardPosition && !isAfterLastKeyboardPosition; }
            set { if (value) SetKeyboardHighlightToFirst(); }
        }

        public bool IsLastPosition
        {
            get
            {
                List<ControlInfo> controls = GetKeyboardHighlightControlInfos(getActiveTabInfo());
                return controls.Count > 0 && CurrentHighlightPosition == controls.Count - 1 && !isBeforeFirstKeyboardPosition && !isAfterLastKeyboardPosition;
            }
            set { if (value) SetKeyboardHighlightToLast(); }
        }

        public bool CanGoBeforeFirstPosition { get { return true; } }

        public bool CanGoAfterLastPosition { get { return true; } }

        public bool HasKeyboardHighlightItems
        {
            get { return GetKeyboardHighlightControlInfos(getActiveTabInfo()).Count > 0; }
        }

        public bool IsVertexCommanderKeyboardHighlightEnabled { get; set; }

        public IEdge KeyboardHighlightedEdge
        {
            get
            {
                if (keyboardHighlightedControlInfo == null)
                    return null;

                IKeyboardHighlight nestedKeyboardHighlight = GetNestedKeyboardHighlight(keyboardHighlightedControlInfo);

                if (nestedKeyboardHighlight != null && nestedKeyboardHighlight.KeyboardHighlightedEdge != null)
                    return nestedKeyboardHighlight.KeyboardHighlightedEdge;

                return keyboardHighlightedControlInfo.BaseEdge;
            }
        }

        public event EventHandler KeyboardHighlightActivated;

        public event EventHandler KeyboardHighlightEnterPressed;

        public event EventHandler GoneBeforeFirstPosition;

        public event EventHandler GoneAfterLastPosition;

        public void UnselectAllSelectedEdges()
        {
            suppressNestedSelectedEdgesChange = true;

            try
            {
                ownSelectedEdges.Clear();
                ClearOwnSelectedEdges();
                ClearNestedSelectedEdges(null);
            }
            finally
            {
                suppressNestedSelectedEdgesChange = false;
            }

            RefreshSelectedControlsVisualState();
        }

        // TypedEdge START

        public FormVisualiser(IEdge _edge)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        public FormVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            ListVisualiserHelper visualiserHelper = new ListVisualiserHelper(parentVisualiser,
                isVolatile,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Form"),
                this, 
                "FormVisualiser", 
                this, 
                false, 
                new List<string>
                {
                    @"BaseEdge:",
                    @"BaseEdge:\Meta:",
                    @"BaseEdge:\Meta:\",
                    @"BaseEdge:\To:",
                    @"Scale:",
                    @"SelectedEdges:",
                    @"ExpertMode:",
                    @"ColumnNumber:",
                    @"MetaOnLeft:",
                    @"MetaAlignRight:",
                    @"SectionsAsTabs:"
                },
                "AtomVisualiser",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitFirst);

            visualiserHelper.CustomVertexChangeEvent += FormVertexChange;

            SetVertexDefaultValues();
            BaseEdgeToUpdated();

            this.BorderBrush = new SolidColorBrush(Colors.Red);
            this.BorderThickness = new Thickness(10);

            this.Foreground = new SolidColorBrush(Colors.Purple);
        }

        private INoInEdgeInOutVertexVertex FormVertexChange(IExecution exe)
        {
            if (isBaseEdgeToUpdateInProgress)
                return exe.Stack;

            IVertex baseEdge = Vertex.Get(false, @"BaseEdge:");
            IVertex baseEdgeTo = Vertex.Get(false, @"BaseEdge:\To:");

            if (baseEdgeTo != null && ContainsOnlyValueChangesOfVertex(exe.Stack, baseEdgeTo))
                return exe.Stack;

            ListVisualiserHelper helper = (ListVisualiserHelper)VisualiserHelper;
            bool baseEdgeMatch = ExecutionFlowHelper.IsVertexChageOrEdgeAddedRemovedDisposedFromTo(exe.Stack, baseEdge);
            bool baseEdgeToMatch = ExecutionFlowHelper.IsVertexChageOrEdgeAddedRemovedDisposedFromTo(exe.Stack, baseEdgeTo);
            List<string> triggeringMetas = GetTriggeringUpdateMetas(exe.Stack);
            bool layoutConfigurationChanged =
                HasFormLayoutConfigurationChanged(triggeringMetas);
            bool baseEdgeDefinitionChanged =
                baseEdgeMatch
                && HasBaseEdgeDefinitionChanged(baseEdge);
            bool suppressTypedBaseEdgeToRebuild = helper.BaseEdgeToEventTriggeringUpdateVertex
                && !baseEdgeDefinitionChanged
                && baseEdgeToMatch
                && isBaseEdgeToInitialized
                && BaseVertexEdge != null;
            bool willRebuild = layoutConfigurationChanged
                || (helper.BaseEdgeToEventTriggeringUpdateVertex
                    && (baseEdgeDefinitionChanged
                        || (baseEdgeToMatch
                            && !suppressTypedBaseEdgeToRebuild)));

            if (willRebuild)
                return helper.VertexChangeLogic(exe);

            if (ExecutionFlowHelper.IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(
                exe.Stack,
                Vertex,
                "Scale"))
                ScaleChange();

            IVertex selectedEdges = Vertex.Get(false, @"SelectedEdges:");
            if (ExecutionFlowHelper.IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(
                    exe.Stack,
                    Vertex,
                    "SelectedEdges")
                || ExecutionFlowHelper.IsEdgeAddedRemovedDiscardedFrom(
                    exe.Stack,
                    selectedEdges))
            {
                SelectedVerticesUpdated();
            }

            return exe.Stack;
        }

        private List<string> GetTriggeringUpdateMetas(IVertex stack)
        {
            List<string> result = new List<string>();

            foreach (string meta in MetaTriggeringUpdateVertex)
                if (ExecutionFlowHelper.IsVertexChangeOrEdgeAddedRemovedDisposedByMetaAndFrom(stack, Vertex, meta))
                    result.Add(meta);

            return result;
        }

        private bool HasFormLayoutConfigurationChanged(
            IEnumerable<string> triggeringMetas)
        {
            foreach (string meta in triggeringMetas)
            {
                if (meta == "SectionsAsTabs"
                    && GraphUtil.GetBooleanValueOrFalse(
                        Vertex.Get(false, @"SectionsAsTabs:"))
                    != SectionsAsTabs)
                    return true;

                if (meta == "MetaOnLeft"
                    && GraphUtil.GetBooleanValueOrFalse(
                        Vertex.Get(false, @"MetaOnLeft:"))
                    != MetaOnLeft)
                    return true;

                if (meta == "MetaAlignRight"
                    && GraphUtil.GetBooleanValueOrFalse(
                        Vertex.Get(false, @"MetaAlignRight:"))
                    != MetaAlignRight)
                    return true;

                if (meta == "ExpertMode"
                    && GraphUtil.GetBooleanValueOrFalse(
                        Vertex.Get(false, @"ExpertMode:"))
                    != ExpertMode)
                    return true;

                if (meta == "ColumnNumber")
                {
                    int? columnNumber = GraphUtil.GetIntegerValue(
                        Vertex.Get(false, @"ColumnNumber:"));

                    if (columnNumber != null
                        && columnNumber.Value != ColumnNumber)
                        return true;
                }
            }

            return false;
        }

        private bool HasBaseEdgeDefinitionChanged(IVertex baseEdge)
        {
            if (!isBaseEdgeToInitialized)
                return true;

            if (baseEdge == null)
                return initializedBaseEdgeFrom != null
                    || initializedBaseEdgeMeta != null
                    || initializedBaseEdgeTo != null;

            return GraphUtil.GetQueryOutFirst(baseEdge, "From", null)
                    != initializedBaseEdgeFrom
                || GraphUtil.GetQueryOutFirst(baseEdge, "Meta", null)
                    != initializedBaseEdgeMeta
                || GraphUtil.GetQueryOutFirst(baseEdge, "To", null)
                    != initializedBaseEdgeTo;
        }

        private void CaptureBaseEdgeDefinition()
        {
            IVertex baseEdge = Vertex.Get(false, @"BaseEdge:");

            initializedBaseEdgeFrom = baseEdge == null
                ? null
                : GraphUtil.GetQueryOutFirst(baseEdge, "From", null);
            initializedBaseEdgeMeta = baseEdge == null
                ? null
                : GraphUtil.GetQueryOutFirst(baseEdge, "Meta", null);
            initializedBaseEdgeTo = baseEdge == null
                ? null
                : GraphUtil.GetQueryOutFirst(baseEdge, "To", null);
        }

        private static bool ContainsOnlyValueChangesOfVertex(IVertex stack, IVertex vertex)
        {
            bool containsEvent = false;

            foreach (IEdge eventEdge in GraphUtil.GetQueryOut(stack, "event", null))
            {
                containsEvent = true;

                IVertex eventType = GraphUtil.GetQueryOutFirst(eventEdge.To, "Type", null);
                IVertex changedVertex = GraphUtil.GetQueryOutFirst(eventEdge.To, "ChangedVertex", null);

                if (eventType == null
                    || !GraphUtil.GetValueAndCompareStrings(eventType, "ValueChange")
                    || changedVertex != vertex)
                    return false;
            }

            return containsEvent;
        }
        
        public void OnLoad(object sender, RoutedEventArgs e)
        {            
            // DO NOT WANT CONTEXTMENU HERE
        }

        public void SelectedVerticesUpdated() { }

        private IEdge GetKeyboardHighlightBaseEdgeForControl(IVertex meta, bool isSet)
        {
            IVertex baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

            if (baseVertex == null)
                return null;

            if (isSet || meta == null)
                return new EasyEdge(null, null, baseVertex);

            IEdge edge = Vertex.GetAll(false, @"BaseEdge:\To:\" + (string)meta.Value + ":").FirstOrDefault();

            if (edge != null)
                return edge;

            return new EasyEdge(baseVertex, meta, null);
        }

        public void ClearKeyboardHighlight()
        {
            if (keyboardHighlightedControlInfo != null)
            {
                IKeyboardHighlight nestedKeyboardHighlight = GetNestedKeyboardHighlight(keyboardHighlightedControlInfo);

                if (nestedKeyboardHighlight != null)
                    nestedKeyboardHighlight.ClearKeyboardHighlight();
                else
                    SetControlInfoKeyboardHighlight(keyboardHighlightedControlInfo, false);
            }

            ClearNestedKeyboardHighlights(null);

            keyboardHighlightedControlInfo = null;
            isBeforeFirstKeyboardPosition = false;
            isAfterLastKeyboardPosition = false;
        }

        public void MoveKeyboardHighlight(int positionDelta)
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

            TabInfo activeTabInfo = getActiveTabInfo();
            List<ControlInfo> controls = GetKeyboardHighlightControlInfos(activeTabInfo);

            if (controls.Count == 0)
            {
                if (positionDelta < 0)
                    MoveToPreviousTabOrBeforeFirst();
                else
                    MoveToNextTabOrAfterLast();

                return;
            }

            int currentIndex = CurrentHighlightPosition;

            if (keyboardHighlightedControlInfo != null && currentIndex < 0)
            {
                ClearKeyboardHighlight();
                currentIndex = -1;
            }

            IKeyboardHighlight nestedKeyboardHighlight = keyboardHighlightedControlInfo != null
                ? GetNestedKeyboardHighlight(keyboardHighlightedControlInfo)
                : null;

            if (nestedKeyboardHighlight != null && nestedKeyboardHighlight.CurrentHighlightPosition != -1)
            {
                nestedKeyboardHighlight.MoveKeyboardHighlight(positionDelta);

                if (nestedKeyboardHighlight.IsBeforeFirstPosition)
                    MoveKeyboardHighlightFromControlIndex(activeTabInfo, currentIndex, -1, false);
                else if (nestedKeyboardHighlight.IsAfterLastPosition)
                    MoveKeyboardHighlightFromControlIndex(activeTabInfo, currentIndex, 1, true);

                return;
            }

            if (currentIndex < 0)
                currentIndex = positionDelta < 0 ? controls.Count : -1;

            MoveKeyboardHighlightFromControlIndex(activeTabInfo, currentIndex, positionDelta, positionDelta > 0);
        }

        private void MoveKeyboardHighlightFromControlIndex(TabInfo tabInfo, int currentIndex, int positionDelta, bool nestedFirstPosition)
        {
            List<ControlInfo> controls = GetKeyboardHighlightControlInfos(tabInfo);
            int newIndex = currentIndex + positionDelta;

            if (newIndex < 0)
                MoveToPreviousTabOrBeforeFirst();
            else if (newIndex >= controls.Count)
                MoveToNextTabOrAfterLast();
            else
                SetKeyboardHighlightControlInfo(controls[newIndex], nestedFirstPosition);
        }

        public void MoveKeyboardHighlight(KeyboardHighlightMoveDirection direction)
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

            IKeyboardHighlight nestedKeyboardHighlight = keyboardHighlightedControlInfo != null
                ? GetNestedKeyboardHighlight(keyboardHighlightedControlInfo)
                : null;

            if (nestedKeyboardHighlight != null && nestedKeyboardHighlight.CurrentHighlightPosition != -1)
            {
                nestedKeyboardHighlight.MoveKeyboardHighlight(direction);

                if (nestedKeyboardHighlight.IsBeforeFirstPosition)
                    MoveKeyboardHighlightFromControlIndex(getActiveTabInfo(), CurrentHighlightPosition, -1, false);
                else if (nestedKeyboardHighlight.IsAfterLastPosition)
                    MoveKeyboardHighlightFromControlIndex(getActiveTabInfo(), CurrentHighlightPosition, 1, true);

                return;
            }

            if (direction == KeyboardHighlightMoveDirection.Up)
                MoveKeyboardHighlight(-1);
            else if (direction == KeyboardHighlightMoveDirection.Down)
                MoveKeyboardHighlight(1);
            else if (direction == KeyboardHighlightMoveDirection.Left)
                MoveKeyboardHighlightToAdjacentColumn(-1);
            else if (direction == KeyboardHighlightMoveDirection.Right)
                MoveKeyboardHighlightToAdjacentColumn(1);
        }

        public void ToggleKeyboardHighlightedEdgeSelection()
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled || SelectionProhibited)
                return;

            IKeyboardHighlight nestedKeyboardHighlight = keyboardHighlightedControlInfo != null
                ? GetNestedKeyboardHighlight(keyboardHighlightedControlInfo)
                : null;

            if (nestedKeyboardHighlight != null && nestedKeyboardHighlight.CurrentHighlightPosition != -1)
            {
                nestedKeyboardHighlight.ToggleKeyboardHighlightedEdgeSelection();
                return;
            }

            if (keyboardHighlightedControlInfo == null || keyboardHighlightedControlInfo.BaseEdge == null)
                return;

            bool wasInSelection = SelectedEdgesInteractionHelper.WasEdgeInSelectedEdges(
                Vertex,
                keyboardHighlightedControlInfo.BaseEdge);

            ApplyFormEdgeGesture(
                SelectedEdgesInteractionHelper.ApplyForClick,
                keyboardHighlightedControlInfo.BaseEdge,
                isCtrl: true,
                wasInSelection,
                isDrag: false);
        }

        private void SetKeyboardHighlightToFirst()
        {
            List<ControlInfo> controls = GetKeyboardHighlightControlInfos(getActiveTabInfo());

            if (controls.Count == 0)
                GoAfterLastKeyboardHighlightPosition();
            else
                SetKeyboardHighlightControlInfo(controls[0], true);
        }

        private void SetKeyboardHighlightToLast()
        {
            List<ControlInfo> controls = GetKeyboardHighlightControlInfos(getActiveTabInfo());

            if (controls.Count == 0)
                GoBeforeFirstKeyboardHighlightPosition();
            else
                SetKeyboardHighlightControlInfo(controls[controls.Count - 1], false);
        }

        private void SetKeyboardHighlightControlInfo(ControlInfo controlInfo, bool nestedFirstPosition = true)
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

            ClearKeyboardHighlight();

            keyboardHighlightedControlInfo = controlInfo;
            isBeforeFirstKeyboardPosition = false;
            isAfterLastKeyboardPosition = false;

            IKeyboardHighlight nestedKeyboardHighlight = GetNestedKeyboardHighlight(controlInfo);

            ClearNestedKeyboardHighlights(nestedKeyboardHighlight);

            if (nestedKeyboardHighlight != null)
            {
                if (nestedFirstPosition)
                    nestedKeyboardHighlight.IsFirstPosition = true;
                else
                    nestedKeyboardHighlight.IsLastPosition = true;
            }
            else
                SetControlInfoKeyboardHighlight(controlInfo, true);

            if (controlInfo.DataControl != null)
                controlInfo.DataControl.BringIntoView();
        }

        private void GoBeforeFirstKeyboardHighlightPosition()
        {
            ClearKeyboardHighlight();
            isBeforeFirstKeyboardPosition = true;

            if (GoneBeforeFirstPosition != null)
                GoneBeforeFirstPosition(this, EventArgs.Empty);
        }

        private void GoAfterLastKeyboardHighlightPosition()
        {
            ClearKeyboardHighlight();
            isAfterLastKeyboardPosition = true;

            if (GoneAfterLastPosition != null)
                GoneAfterLastPosition(this, EventArgs.Empty);
        }

        private void MoveKeyboardHighlightToAdjacentColumn(int columnDelta)
        {
            if (keyboardHighlightedControlInfo == null)
            {
                if (columnDelta > 0)
                    SetKeyboardHighlightToFirst();
                else
                    SetKeyboardHighlightToLast();

                return;
            }

            TabInfo activeTabInfo = getActiveTabInfo();
            List<ControlInfo> controls = GetKeyboardHighlightControlInfos(activeTabInfo);

            if (controls.Count == 0)
                return;

            int currentColumn = keyboardHighlightedControlInfo.Column;
            int targetColumn = currentColumn + columnDelta;
            int currentIndexInColumn = GetIndexInColumn(activeTabInfo, keyboardHighlightedControlInfo);
            List<ControlInfo> targetColumnControls = GetKeyboardHighlightControlInfosByColumn(activeTabInfo, targetColumn);

            if (targetColumnControls.Count > 0)
            {
                int index = Math.Min(currentIndexInColumn, targetColumnControls.Count - 1);
                SetKeyboardHighlightControlInfo(targetColumnControls[index], columnDelta > 0);
                return;
            }

            if (columnDelta < 0)
                MoveToPreviousTabFirstOrBeforeFirst();
            else
                MoveToNextTabOrAfterLast();
        }

        private int GetIndexInColumn(TabInfo tabInfo, ControlInfo controlInfo)
        {
            List<ControlInfo> controlsInColumn = GetKeyboardHighlightControlInfosByColumn(tabInfo, controlInfo.Column);

            int index = controlsInColumn.IndexOf(controlInfo);

            if (index < 0)
                return 0;

            return index;
        }

        private List<ControlInfo> GetKeyboardHighlightControlInfosByColumn(TabInfo tabInfo, int column)
        {
            return GetKeyboardHighlightControlInfos(tabInfo)
                .Where(controlInfo => controlInfo.Column == column)
                .OrderBy(controlInfo => GetControlScreenPosition(controlInfo).Y)
                .ThenBy(controlInfo => GetControlScreenPosition(controlInfo).X)
                .ToList();
        }

        private void MoveToPreviousTabFirstOrBeforeFirst()
        {
            TabInfo previousTabInfo = GetAdjacentTabInfo(-1);

            if (previousTabInfo == null)
            {
                GoBeforeFirstKeyboardHighlightPosition();
                return;
            }

            SetActiveTabInfo(previousTabInfo);

            List<ControlInfo> controls = GetKeyboardHighlightControlInfos(previousTabInfo);

            if (controls.Count == 0)
                GoBeforeFirstKeyboardHighlightPosition();
            else
                SetKeyboardHighlightControlInfo(controls[0], true);
        }

        private void MoveToPreviousTabOrBeforeFirst()
        {
            TabInfo previousTabInfo = GetAdjacentTabInfo(-1);

            if (previousTabInfo == null)
            {
                GoBeforeFirstKeyboardHighlightPosition();
                return;
            }

            SetActiveTabInfo(previousTabInfo);

            List<ControlInfo> controls = GetKeyboardHighlightControlInfos(previousTabInfo);

            if (controls.Count == 0)
                GoBeforeFirstKeyboardHighlightPosition();
            else
                SetKeyboardHighlightControlInfo(controls[controls.Count - 1], false);
        }

        private void MoveToNextTabOrAfterLast()
        {
            TabInfo nextTabInfo = GetAdjacentTabInfo(1);

            if (nextTabInfo == null)
            {
                GoAfterLastKeyboardHighlightPosition();
                return;
            }

            SetActiveTabInfo(nextTabInfo);

            List<ControlInfo> controls = GetKeyboardHighlightControlInfos(nextTabInfo);

            if (controls.Count == 0)
                GoAfterLastKeyboardHighlightPosition();
            else
                SetKeyboardHighlightControlInfo(controls[0], true);
        }

        private TabInfo GetAdjacentTabInfo(int tabDelta)
        {
            List<TabInfo> tabInfos = GetTabInfos();

            if (tabInfos.Count <= 1)
                return null;

            TabInfo activeTabInfo = getActiveTabInfo();
            int currentIndex = tabInfos.IndexOf(activeTabInfo);

            if (currentIndex < 0)
                currentIndex = 0;

            int newIndex = currentIndex + tabDelta;

            if (newIndex < 0 || newIndex >= tabInfos.Count)
                return null;

            return tabInfos[newIndex];
        }

        private List<TabInfo> GetTabInfos()
        {
            if (TabList == null)
                return new List<TabInfo>();

            return TabList.Values.ToList();
        }

        private void SetActiveTabInfo(TabInfo tabInfo)
        {
            if (tabInfo == null)
                return;

            if (TabControl != null && tabInfo.TabItem != null)
            {
                TabControl.SelectedItem = tabInfo.TabItem;
                TabControlSelectedItem = tabInfo.TabItem;
            }
        }

        private List<ControlInfo> GetKeyboardHighlightControlInfos(TabInfo tabInfo)
        {
            if (tabInfo == null)
                return new List<ControlInfo>();

            return tabInfo.ControlInfos.Values
                .Where(IsKeyboardHighlightControlInfoAvailable)
                .OrderBy(controlInfo => GetControlScreenPosition(controlInfo).Y)
                .ThenBy(controlInfo => GetControlScreenPosition(controlInfo).X)
                .ThenBy(controlInfo => controlInfo.Column)
                .ThenBy(controlInfo => controlInfo.Order)
                .ToList();
        }

        private static bool IsKeyboardHighlightControlInfoAvailable(ControlInfo controlInfo)
        {
            if (controlInfo == null || controlInfo.DataControl == null)
                return false;

            IKeyboardHighlight nestedKeyboardHighlight = GetNestedKeyboardHighlight(controlInfo);

            if (nestedKeyboardHighlight == null)
                return true;

            return nestedKeyboardHighlight.HasKeyboardHighlightItems;
        }

        private Point GetControlScreenPosition(ControlInfo controlInfo)
        {
            FrameworkElement element = controlInfo != null ? controlInfo.DataControl : null;

            if (element == null)
                return new Point(double.MaxValue, double.MaxValue);

            try
            {
                Point point = element.TranslatePoint(new Point(0, 0), this);

                if (double.IsNaN(point.X) || double.IsNaN(point.Y))
                    return new Point(double.MaxValue, double.MaxValue);

                return point;
            }
            catch (InvalidOperationException)
            {
                return new Point(double.MaxValue, double.MaxValue);
            }
        }

        private void SetControlInfoKeyboardHighlight(ControlInfo controlInfo, bool isHighlighted)
        {
            if (isHighlighted && !IsVertexCommanderKeyboardHighlightEnabled)
                isHighlighted = false;

            bool isSelected = IsOwnSelectedControlInfo(controlInfo);
            Brush dataBackground;
            Brush dataForeground;
            Brush metaBackground;
            Brush metaForeground;

            if (isHighlighted)
            {
                dataBackground = (Brush)FindResource("0HighlightBrush");
                dataForeground = isSelected
                    ? (Brush)FindResource("0ForegroundBrush")
                    : (Brush)FindResource("0HighlightForegroundBrush");
                metaBackground = dataBackground;
                metaForeground = dataForeground;
            }
            else if (isSelected)
            {
                dataBackground = (Brush)FindResource("0SelectionBrush");
                dataForeground = (Brush)FindResource("0BackgroundBrush");
                metaBackground = dataBackground;
                metaForeground = dataForeground;
            }
            else
            {
                dataBackground = (Brush)FindResource("0BackgroundBrush");
                dataForeground = (Brush)FindResource("0ForegroundBrush");
                metaBackground = Brushes.Transparent;
                metaForeground = (Brush)FindResource("0ForegroundBrush");
            }

            SetElementHighlight(controlInfo.MetaControl, metaBackground, metaForeground);
            SetElementHighlight(controlInfo.DataControl, dataBackground, dataForeground);
        }

        private bool IsOwnSelectedControlInfo(ControlInfo controlInfo)
        {
            if (controlInfo == null || controlInfo.BaseEdge == null)
                return false;

            return FindOwnSelectedEdge(controlInfo.BaseEdge) != null;
        }

        private void RefreshSelectedControlsVisualState()
        {
            if (TabList == null)
                return;

            foreach (TabInfo tabInfo in TabList.Values)
                foreach (ControlInfo controlInfo in tabInfo.ControlInfos.Values)
                {
                    bool isKeyboardHighlighted = controlInfo == keyboardHighlightedControlInfo
                        && GetNestedKeyboardHighlight(controlInfo) == null;

                    SetControlInfoKeyboardHighlight(controlInfo, isKeyboardHighlighted);
                }
        }

        private static IKeyboardHighlight GetNestedKeyboardHighlight(ControlInfo controlInfo)
        {
            if (controlInfo == null)
                return null;

            return controlInfo.DataControl as IKeyboardHighlight;
        }

        private IEnumerable<IKeyboardHighlight> GetNestedKeyboardHighlights()
        {
            if (TabList == null)
                yield break;

            foreach (TabInfo tabInfo in TabList.Values)
                foreach (ControlInfo controlInfo in tabInfo.ControlInfos.Values)
                {
                    IKeyboardHighlight nestedKeyboardHighlight = GetNestedKeyboardHighlight(controlInfo);

                    if (nestedKeyboardHighlight != null)
                        yield return nestedKeyboardHighlight;
                }
        }

        private void ClearNestedKeyboardHighlights(IKeyboardHighlight exceptKeyboardHighlight)
        {
            foreach (IKeyboardHighlight nestedKeyboardHighlight in GetNestedKeyboardHighlights())
                if (nestedKeyboardHighlight != exceptKeyboardHighlight)
                    nestedKeyboardHighlight.ClearKeyboardHighlight();
        }

        private void RegisterNestedKeyboardHighlightActivation(ControlInfo controlInfo)
        {
            IKeyboardHighlight nestedKeyboardHighlight = GetNestedKeyboardHighlight(controlInfo);

            if (nestedKeyboardHighlight == null)
                return;

            if (IsVertexCommanderKeyboardHighlightEnabled)
                nestedKeyboardHighlight.IsVertexCommanderKeyboardHighlightEnabled = true;

            nestedKeyboardHighlight.KeyboardHighlightActivated += delegate
            {
                ClearNestedKeyboardHighlights(nestedKeyboardHighlight);
                keyboardHighlightedControlInfo = controlInfo;
                isBeforeFirstKeyboardPosition = false;
                isAfterLastKeyboardPosition = false;
                RaiseKeyboardHighlightActivated();
            };

            IListVisualiser nestedListVisualiser = controlInfo.DataControl as IListVisualiser;

            if (nestedListVisualiser != null)
                nestedListVisualiser.SelectedEdgesChange += delegate { NestedListVisualiser_SelectedEdgesChange(nestedListVisualiser); };
        }

        private void NestedListVisualiser_SelectedEdgesChange(IListVisualiser sourceVisualiser)
        {
            if (suppressNestedSelectedEdgesChange)
                return;

            bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            suppressNestedSelectedEdgesChange = true;

            try
            {
                if (!isCtrl)
                {
                    ownSelectedEdges.Clear();
                    ClearNestedSelectedEdges(sourceVisualiser);
                }

                SyncOwnSelectedEdgesVertex();
                RefreshSelectedControlsVisualState();
            }
            finally
            {
                suppressNestedSelectedEdgesChange = false;
            }

            NotifySelectedEdgesChanged();
        }

        private void FormControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ControlInfo controlInfo = GetControlInfoByElement(sender as FrameworkElement);

            if (controlInfo == null)
                return;

            SetPendingMouseDownControlInfo(controlInfo, e);

            if (e.ClickCount == 1)
            {
                e.Handled = true;
                return;
            }

            if (e.ClickCount == 2)
            {
                e.Handled = true;
                return;
            }
        }

        private void FormControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ControlInfo controlInfo = GetControlInfoByElement(sender as FrameworkElement);

            if (controlInfo == null)
                return;

            SetPendingMouseDownControlInfo(controlInfo, e);
        }

        private void SetPendingMouseDownControlInfo(ControlInfo controlInfo, MouseButtonEventArgs e)
        {
            pendingMouseDownControlInfo = controlInfo;
            pendingMouseDownIsCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            pendingWasInSelectionAtMouseDown = controlInfo.BaseEdge != null
                && SelectedEdgesInteractionHelper.WasEdgeInSelectedEdges(Vertex, controlInfo.BaseEdge);
            pendingMouseDownHasNestedKeyboardHighlight = GetNestedKeyboardHighlight(controlInfo) != null;
            pendingMouseDownClickCount = e.ClickCount;
            pendingMouseDownPoint = e.GetPosition(this);
            pendingMouseDownStartedOnSelfDraggingControl = IsWithinSelfDraggingControl(e.OriginalSource as DependencyObject);
            suppressNextMouseUpSelection = false;
            MinusZero.Instance.IsGUIDragging = false;
        }

        // Some nested controls (e.g. the NumberVisualiser slider) handle their own drag gesture.
        // Form drag-and-drop must not hijack a gesture that starts on such a control, otherwise the
        // slider stutters because Form steals the mouse move and starts DnD instead.
        private static bool IsWithinSelfDraggingControl(DependencyObject element)
        {
            while (element != null)
            {
                if (element is System.Windows.Controls.Primitives.RangeBase
                    || element is System.Windows.Controls.Primitives.Thumb
                    || element is System.Windows.Controls.Primitives.Track
                    || element is System.Windows.Controls.Primitives.ScrollBar)
                    return true;

                DependencyObject parent = null;

                if (element is Visual || element is System.Windows.Media.Media3D.Visual3D)
                    parent = VisualTreeHelper.GetParent(element);

                if (parent == null)
                    parent = LogicalTreeHelper.GetParent(element);

                element = parent;
            }

            return false;
        }

        private void FormControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ControlInfo controlInfo = GetControlInfoByElement(sender as FrameworkElement);

            if (controlInfo == null || controlInfo != pendingMouseDownControlInfo)
                return;

            if (suppressNextMouseUpSelection)
            {
                ClearPendingMouseSelection();
                e.Handled = true;
                return;
            }

            if (pendingMouseDownClickCount == 1)
            {
                if (!pendingMouseDownHasNestedKeyboardHighlight)
                {
                    if (IsVertexCommanderMode())
                        SetKeyboardHighlightControlInfo(controlInfo);

                    TryApplyPendingMouseClick();
                }

                ClearPendingMouseSelection();
                e.Handled = true;
                return;
            }

            if (pendingMouseDownClickCount == 2)
            {
                if (!pendingMouseDownHasNestedKeyboardHighlight && IsVertexCommanderMode())
                    SetKeyboardHighlightControlInfo(controlInfo);

                RaiseKeyboardHighlightActivated();
                ClearPendingMouseSelection();
                e.Handled = true;
            }
        }

        private void FormControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (pendingMouseDownControlInfo == null || isFormDndDragging)
                return;

            if (pendingMouseDownHasNestedKeyboardHighlight)
                return;

            if (pendingMouseDownStartedOnSelfDraggingControl)
                return;

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            Point mousePosition = e.GetPosition(this);
            Vector diff = pendingMouseDownPoint - mousePosition;

            if (Math.Abs(diff.X) <= Dnd.MinimumHorizontalDragDistance
                && Math.Abs(diff.Y) <= Dnd.MinimumVerticalDragDistance)
                return;

            StartFormDndFromPendingSelection();
            e.Handled = true;
        }

        private void FormControl_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ControlInfo controlInfo = GetControlInfoByElement(sender as FrameworkElement);

            if (controlInfo == null || controlInfo.BaseEdge == null || SelectionProhibited)
                return;

            if (SelectedEdgesInteractionHelper.WasEdgeInSelectedEdges(Vertex, controlInfo.BaseEdge))
                return;

            suppressNestedSelectedEdgesChange = true;

            try
            {
                ownSelectedEdges.Clear();
                ClearNestedSelectedEdges(null);
                SelectedEdgesInteractionHelper.ApplyForContextMenu(Vertex, controlInfo.BaseEdge);
                ReloadOwnSelectedEdgesFromVertex();
            }
            finally
            {
                suppressNestedSelectedEdgesChange = false;
            }

            RefreshSelectedControlsVisualState();
            NotifySelectedEdgesChanged();
        }

        private void TryApplyPendingMouseClick()
        {
            if (SelectionProhibited || pendingMouseDownControlInfo == null || pendingMouseDownControlInfo.BaseEdge == null)
                return;

            ApplyFormEdgeGesture(
                SelectedEdgesInteractionHelper.ApplyForClick,
                pendingMouseDownControlInfo.BaseEdge,
                pendingMouseDownIsCtrl,
                pendingWasInSelectionAtMouseDown,
                isDrag: false);
        }

        private void StartFormDndFromPendingSelection()
        {
            IVertex dndVertex = TryPrepareDragAndBuildDndVertex();

            if (dndVertex != null && dndVertex.Count() > 0)
            {
                isFormDndDragging = true;
                suppressNextMouseUpSelection = true;
                ClearKeyboardHighlight();
                dndVertex.AddExternalReference();

                DataObject dragData = new DataObject("Vertex", dndVertex);
                dragData.SetData("DragSource", this);

                Dnd.DoDragDrop(this, dragData);

                ClearKeyboardHighlight();
                isFormDndDragging = false;
            }

            ClearPendingMouseSelection();
        }

        private IVertex TryPrepareDragAndBuildDndVertex()
        {
            if (pendingMouseDownControlInfo == null || pendingMouseDownControlInfo.BaseEdge == null)
                return null;

            IEdge clickedEdge = pendingMouseDownControlInfo.BaseEdge;
            IVertex fallbackEdgeVertex = GetEdgeByPoint(pendingMouseDownPoint);

            if (fallbackEdgeVertex == null)
            {
                fallbackEdgeVertex = MinusZero.Instance.CreateTempVertex();
                EdgeHelper.AddEdgeVertex(fallbackEdgeVertex, clickedEdge);
            }

            ApplyFormEdgeGesture(
                SelectedEdgesInteractionHelper.ApplyForDrag,
                clickedEdge,
                pendingMouseDownIsCtrl,
                pendingWasInSelectionAtMouseDown,
                isDrag: true);

            return SelectedEdgesInteractionHelper.BuildDndVertexFromSelectedEdges(Vertex, fallbackEdgeVertex);
        }

        private void ApplyFormEdgeGesture(
            Action<IVertex, PendingEdgeMouseGesture> applyHelper,
            IEdge edge,
            bool isCtrl,
            bool wasInSelectionAtMouseDown,
            bool isDrag)
        {
            if (SelectionProhibited || edge == null || Vertex == null)
                return;

            PendingEdgeMouseGesture gesture = new PendingEdgeMouseGesture
            {
                ClickedEdge = edge,
                IsCtrl = isCtrl,
                WasInSelectionAtMouseDown = wasInSelectionAtMouseDown
            };

            bool clearNestedBeforeApply = !isCtrl && (!wasInSelectionAtMouseDown || !isDrag);

            suppressNestedSelectedEdgesChange = true;

            try
            {
                if (clearNestedBeforeApply)
                {
                    ownSelectedEdges.Clear();
                    ClearNestedSelectedEdges(null);
                }

                applyHelper(Vertex, gesture);
                ReloadOwnSelectedEdgesFromVertex();

                if (isCtrl)
                    SyncOwnSelectedEdgesVertex();
            }
            finally
            {
                suppressNestedSelectedEdgesChange = false;
            }

            RefreshSelectedControlsVisualState();
            NotifySelectedEdgesChanged();
        }

        private void ReloadOwnSelectedEdgesFromVertex()
        {
            ownSelectedEdges.Clear();

            if (TabList == null || Vertex == null)
                return;

            foreach (TabInfo tabInfo in TabList.Values)
                foreach (ControlInfo controlInfo in tabInfo.ControlInfos.Values)
                {
                    if (controlInfo.BaseEdge == null)
                        continue;

                    if (SelectedEdgesInteractionHelper.WasEdgeInSelectedEdges(Vertex, controlInfo.BaseEdge))
                        ownSelectedEdges.Add(controlInfo.BaseEdge);
                }
        }

        private void ClearPendingMouseSelection()
        {
            pendingMouseDownControlInfo = null;
            pendingMouseDownIsCtrl = false;
            pendingWasInSelectionAtMouseDown = false;
            pendingMouseDownHasNestedKeyboardHighlight = false;
            pendingMouseDownClickCount = 0;
            pendingMouseDownStartedOnSelfDraggingControl = false;
        }

        private ControlInfo GetControlInfoByElement(FrameworkElement element)
        {
            if (element == null || TabList == null)
                return null;

            foreach (TabInfo tabInfo in TabList.Values)
                foreach (ControlInfo controlInfo in tabInfo.ControlInfos.Values)
                    if (controlInfo.MetaControl == element || controlInfo.DataControl == element)
                        return controlInfo;

            return null;
        }

        private static void SetElementHighlight(FrameworkElement element, Brush background, Brush foreground)
        {
            if (element == null)
                return;

            // Color only the Form's own control surface (the meta/value label). Do NOT descend into
            // nested visualisers (atomic or Table) - they manage their own background/foreground
            // (e.g. the empty-edge grey state via IsNull), so recursing here would overwrite it.
            Control control = element as Control;

            if (control != null)
            {
                control.Background = background;
                control.Foreground = foreground;
                return;
            }

            TextBlock textBlock = element as TextBlock;

            if (textBlock != null)
            {
                textBlock.Background = background;
                textBlock.Foreground = foreground;
            }
        }

        private IEdge FindOwnSelectedEdge(IEdge edge)
        {
            foreach (IEdge ownSelectedEdge in ownSelectedEdges)
                if (EdgeHelper.CompareIEdges(ownSelectedEdge, edge))
                    return ownSelectedEdge;

            return null;
        }

        private void ClearOwnSelectedEdges()
        {
            IVertex selectedEdges = GetOwnSelectedEdgesVertex();

            if (selectedEdges != null)
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(selectedEdges);
        }

        private IVertex GetOwnSelectedEdgesVertex()
        {
            if (Vertex == null)
                return null;

            return Vertex.Get(false, "SelectedEdges:");
        }

        private void ClearNestedSelectedEdges(IListVisualiser exceptVisualiser)
        {
            foreach (IListVisualiser nestedVisualiser in GetNestedListVisualisers())
            {
                if (nestedVisualiser == exceptVisualiser)
                    continue;

                nestedVisualiser.UnselectAllSelectedEdges();
            }
        }

        private IEnumerable<IListVisualiser> GetNestedListVisualisers()
        {
            if (TabList == null)
                yield break;

            foreach (TabInfo tabInfo in TabList.Values)
                foreach (ControlInfo controlInfo in tabInfo.ControlInfos.Values)
                {
                    IListVisualiser nestedVisualiser = controlInfo.DataControl as IListVisualiser;

                    if (nestedVisualiser != null)
                        yield return nestedVisualiser;
                }
        }

        private void SyncOwnSelectedEdgesVertex()
        {
            IVertex ownSelectedEdges = GetOwnSelectedEdgesVertex();

            if (ownSelectedEdges == null)
                return;

            GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(ownSelectedEdges);

            foreach (IEdge ownSelectedEdge in this.ownSelectedEdges)
                AddSelectedEdgeIfMissing(ownSelectedEdges, ownSelectedEdge);

            foreach (IListVisualiser nestedVisualiser in GetNestedListVisualisers())
            {
                IVertex nestedSelectedEdges = nestedVisualiser.Vertex.Get(false, "SelectedEdges:");

                if (nestedSelectedEdges == null)
                    continue;

                foreach (IEdge selectedEdgeVertexEdge in nestedSelectedEdges.GetAll(false, @"{$Is:Edge}"))
                {
                    IEdge selectedEdge = EdgeHelper.CreateIEdgeFromEdgeVertex(selectedEdgeVertexEdge.To);
                    AddSelectedEdgeIfMissing(ownSelectedEdges, selectedEdge);
                }
            }
        }

        private static void AddSelectedEdgeIfMissing(IVertex selectedEdges, IEdge edge)
        {
            if (selectedEdges == null || edge == null)
                return;

            if (EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, edge) == null)
                EdgeHelper.AddEdgeVertex(selectedEdges, edge);
        }

        private void NotifySelectedEdgesChanged()
        {
            if (SelectedEdgesChange != null)
                SelectedEdgesChange();
        }

        private void RaiseKeyboardHighlightActivated()
        {
            if (KeyboardHighlightedEdge == null)
                return;

            if (KeyboardHighlightActivated != null)
                KeyboardHighlightActivated(this, EventArgs.Empty);

            if (KeyboardHighlightEnterPressed != null)
                KeyboardHighlightEnterPressed(this, EventArgs.Empty);
        }

        private TabInfo getActiveTabInfo()
        {
            if (HasTabs)
            {
                TabItem i = TabControlSelectedItem;

                if (i == null && TabControl != null)
                    i = TabControl.SelectedItem as TabItem;

                if (i == null)
                    return TabList.Values.FirstOrDefault();

                foreach (TabInfo tie in TabList.Values)
                    if (tie.TabItem.Header == i.Header)
                        return tie;

                return null;
            }
            else
            {
                if (TabList == null)
                    return null;

                if (TabList.ContainsKey(""))
                    return TabList[""];

                return TabList.Values.FirstOrDefault();
            }
        }

        private IVertex getMetaForForm()
        {
            if (Vertex.Get(false, @"BaseEdge:\Meta:") == null/* || Vertex.Get(false, @"BaseEdge:\Meta:").Count() == 0*/)
                return null;

            IVertex v = GraphUtil.GetMostInheritedMeta(Vertex.Get(false, @"BaseEdge:\To:"), Vertex.Get(false, @"BaseEdge:\Meta:"));
            // XXX there is error in GetMostInheritedMeta - see it 

            if (v != null && v.Get(false, @"$EdgeTarget:") != null)
                return v.Get(false, @"$EdgeTarget:");
            else
                return v;
        }

        private string getGroup(IVertex meta)
        {
            if (SectionsAsTabs) {
                if (meta == null)
                    return " | ";

                string _section = (string)GraphUtil.GetValue(meta.Get(false, "$Section:"));
                string _group = (string)GraphUtil.GetValue(meta.Get(false, "$Group:"));

                if (_group == null && _section == null)
                    return "";

                if (_group == null)
                    return "| " + _section;

                if (_section == null)
                    return _group;

                return _group + " | " + _section;
            }
            else
            {
                if (meta == null)
                    return "";

                string _group = (string)GraphUtil.GetValue(meta.Get(false, "$Group:"));

                if (_group == null)
                    return "";
                else
                    return _group;
            }
        }

        private string getSection(IVertex meta)
        {
            if (meta == null)
                return null;

            if (SectionsAsTabs)
                return null;
            else
                return (string)GraphUtil.GetValue(meta.Get(false, "$Section:"));
        }

        bool BaseVertexEdgeAdded_PreFill = false;

        private void PreFillFormAnalyseEdge(IVertex meta, bool isSet)
        {
            if (DisplayBaseVertex && BaseVertexEdgeAdded_PreFill == false)
            {
                BaseVertexEdge = getMetaForForm();
                BaseVertexEdgeAdded_PreFill = true;
                PreFillFormAnalyseEdge(BaseVertexEdge, false);
            }

            string group = getGroup(meta);
            string section = getSection(meta);

            TabInfo t;

            if (group != null && group != "")
                HasTabs = true;

            if (TabList.ContainsKey(group))
                t = TabList[group];
            else
            {
                t = new TabInfo();
                TabList.Add(group, t);
            }

            //if(isSet==false)
            t.TotalNumberOfControls++;

        }

        private IList<(IVertex Meta, bool IsSet)> BuildFormFieldDescriptors(
            bool isTypedForm,
            IVertex baseEdgeTo,
            IList<IEdge> childEdges,
            IList<IEdge> expertEdges,
            IList<IEdge> executableEdges)
        {
            IList<(IVertex Meta, bool IsSet)> result =
                new List<(IVertex Meta, bool IsSet)>();

            if (!isTypedForm) // if Form is not typed
            {
                IList<IVertex> visited = new List<IVertex>();
                Dictionary<IVertex, int> edgeCountByMeta =
                    new Dictionary<IVertex, int>();

                foreach (IEdge e in childEdges)
                {
                    if (!visited.Contains(e.Meta) && e.Meta.Get(false, "$Hide:") == null)
                    {
                        if (!edgeCountByMeta.TryGetValue(
                            e.Meta,
                            out int edgeCount))
                        {
                            edgeCount =
                                baseEdgeTo.GetAll(
                                    false,
                                    e.Meta + ":").Count();
                            edgeCountByMeta.Add(e.Meta, edgeCount);
                        }

                        bool isSet = edgeCount > 1;
                        result.Add((e.Meta, isSet));

                        if (isSet)
                            visited.Add(e.Meta);
                    }
                }
            }
            else // Form is typed
            {
                foreach (IEdge e in childEdges)
                {
                    if (e.To.Get(false, "$Hide:") == null)
                    {
                        int? maxCardinality = GraphUtil.GetIntegerValue(
                            e.To.Get(false, "$MaxCardinality:"));
                        result.Add(
                            (e.To,
                             maxCardinality > 1
                                || maxCardinality == -1));
                    }
                }
            }

            foreach (IEdge e in expertEdges)
            {
                bool contains = false;

                foreach (IEdge ee in childEdges)
                    if (GeneralUtil.CompareStrings(ee.To, e.To))
                        contains = true;

                if (contains == false)
                    result.Add((e.To, false));
            }

            foreach (IEdge e in executableEdges)
                if (e.To.Get(false, "$Hide:") == null)
                    result.Add((e.To, false));

            return result;
        }

        private void PreFillForm(
            IEnumerable<(IVertex Meta, bool IsSet)> fieldDescriptors)
        {
            TabList = new Dictionary<string, TabInfo>();

            foreach ((IVertex Meta, bool IsSet) descriptor
                in fieldDescriptors)
            {
                PreFillFormAnalyseEdge(
                    descriptor.Meta,
                    descriptor.IsSet);
            }
        }

        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                this.SizeChanged -= FormVisualiser_SizeChanged;

                // Nested UX (and other edit visualisers) are hosted in TabList via VisualiserEditWrapper
                // with AddVertex=false, so they are not reachable through Item: edges.
                DisposeTabListDataControls();

                VisualiserHelper.Dispose();
            }
        }

        IVertex BaseVertexEdge = null;
        

        public void BaseEdgeToUpdated()
        {
            if (isBaseEdgeToUpdateInProgress)
                return;

            bool originalForceVertexChangeOff =
                VisualiserHelper.ForceVertexChangeOff;

            isBaseEdgeToUpdateInProgress = true;
            VisualiserHelper.ForceVertexChangeOff = true;

            try
            {
           // Content = new Button();

            this.SizeChanged -= FormVisualiser_SizeChanged;

            ClearKeyboardHighlight();
            ClearPendingMouseSelection();
            UnselectAllSelectedEdges();

            // Dispose TabList hosts first so nested UX (AddVertex=false) leaves VisualisersList
            // before Item:-based cleanup and UI rebuild.
            DisposeTabListDataControls();

            VisualiserHelper.DisposeAllChildVisualisersExceptWrap();

            BaseVertexEdgeAdded_PreFill = false;
            BaseVertexEdgeAdded = false;
            lastCorrectedWidth = 0;
            widthCorrectionScheduled = false;
            widthCorrectionInProgress = false;
            TabControlSelectedItem = null;
            TabControl = null;

            IVertex basTo = Vertex.Get(false, @"BaseEdge:\To:");            

            if (basTo != null)
            {
                SectionsAsTabs = GraphUtil.GetBooleanValueOrFalse(
                    Vertex.Get(false, @"SectionsAsTabs:"));
                MetaOnLeft = GraphUtil.GetBooleanValueOrFalse(
                    Vertex.Get(false, @"MetaOnLeft:"));
                MetaAlignRight = GraphUtil.GetBooleanValueOrFalse(
                    Vertex.Get(false, @"MetaAlignRight:"));
                ExpertMode = GraphUtil.GetBooleanValueOrFalse(
                    Vertex.Get(false, @"ExpertMode:"));

                int? _columnNumber = GraphUtil.GetIntegerValue(Vertex.Get(false, @"ColumnNumber:"));

                if (_columnNumber != null)
                    ColumnNumber = (int)_columnNumber;

                IVertex metaForForm = getMetaForForm();
                bool isTypedForm =
                    metaForForm != null && metaForForm.Count() != 0;
                IList<IEdge> childEdges = isTypedForm
                    ? VertexOperations.GetChildEdges(metaForForm).ToList()
                    : basTo.ToList();
                IList<IEdge> expertEdges = ExpertMode
                    ? MinusZero.Instance.Root
                        .Get(false, @"System\Meta\Base\Vertex")
                        .ToList()
                    : new List<IEdge>();
                IList<IEdge> executableEdges =
                    ExecutableVisualiserFactory.IsOfExecutableMeta(metaForForm)
                        ? ExecutableVisualiserFactory
                            .GetExecutableEdges(metaForForm)
                            .ToList()
                        : new List<IEdge>();

                IList<(IVertex Meta, bool IsSet)> fieldDescriptors =
                    BuildFormFieldDescriptors(
                    isTypedForm,
                    basTo,
                    childEdges,
                    expertEdges,
                    executableEdges);
                PreFillForm(fieldDescriptors);

                InitializeControlContent();

                foreach ((IVertex Meta, bool IsSet) descriptor
                    in fieldDescriptors)
                {
                    AddEdge(
                        descriptor.Meta,
                        descriptor.IsSet);
                }
                
                if (MetaOnLeft)
                    ScheduleWidthCorrection(true);
                
            }
            //return;
                CaptureBaseEdgeDefinition();
                isBaseEdgeToInitialized = true;
            }
            finally
            {
                VisualiserHelper.ForceVertexChangeOff =
                    originalForceVertexChangeOff;
                isBaseEdgeToUpdateInProgress = false;
            }
        }

        protected bool CorrectWidth(TabInfo i, bool allowUpdateLayout = true)
        {
            if (i.ControlInfos.Count() == 0)
                return true;

            if (allowUpdateLayout && !i.WidthCorrectionDone)
                this.UpdateLayout();

           if (i.ControlInfos.First().Value.MetaControl.ActualWidth == 0)
                return false;
            
            i.WidthCorrectionDone = true;
           

            double oneColumnWidth = Math.Floor(((this.ActualWidth - marginOnRight) / ColumnNumber) - marginBetweenColumns);
                      
                    double[] maxMetaWidthInColumn = new double[ColumnNumber];

                    foreach (ControlInfo ci in i.ControlInfos.Values)
                        if (ci.MetaControl.ActualWidth > maxMetaWidthInColumn[ci.Column])
                            maxMetaWidthInColumn[ci.Column] = Math.Floor(ci.MetaControl.ActualWidth);

                    for (int c = 0; c < ColumnNumber; c++)
                        if (maxMetaWidthInColumn[c] > oneColumnWidth - metaVsDataSeparator - 5)
                            maxMetaWidthInColumn[c] = Math.Floor(oneColumnWidth * 0.5);

            if(i.Sections.Count()==0)
            foreach (KeyValuePair<IVertex, ControlInfo> ci in i.ControlInfos) // if there are no sections
                    {                       
                        ci.Value.MetaControl.Width = maxMetaWidthInColumn[ci.Value.Column];
                        ci.Value.GapControl.Width = 0;

                    double ci_Value_DataControl_Width_to_be = Math.Floor(oneColumnWidth - maxMetaWidthInColumn[ci.Value.Column] - metaVsDataSeparator - 5);

                    if (ci_Value_DataControl_Width_to_be < 0)
                        ci_Value_DataControl_Width_to_be = 0;

                    ci.Value.DataControl.Width = ci_Value_DataControl_Width_to_be;
                    }
            else
                foreach (KeyValuePair<IVertex, ControlInfo> ci in i.ControlInfos) // if there are sections
                {
                    ci.Value.MetaControl.Width = maxMetaWidthInColumn[ci.Value.Column];

                    if (getSection(ci.Key) == null)
                    {
                        ci.Value.GapControl.Width = (sectionControlBorderWidth / 2) - 2;
                        ci.Value.DataControl.Width = Math.Max(0, Math.Floor(oneColumnWidth - maxMetaWidthInColumn[ci.Value.Column] - metaVsDataSeparator - 9 - sectionControlBorderWidth / 2));
                    }
                    else
                    {
                        ci.Value.GapControl.Width = 0;
                        ci.Value.DataControl.Width = Math.Max(0, Math.Floor(oneColumnWidth - maxMetaWidthInColumn[ci.Value.Column] - metaVsDataSeparator - sectionControlBorderWidth));
                    }
                }

            return true;
        }

        protected object CreateColumnedContent()
        {
            Grid g = new Grid();

            bool notFirstColumn = false;

            int columnCount = 0;

            for (int i = 0; i < ColumnNumber; i++)
            {
                if (notFirstColumn)
                {
                    ColumnDefinition cd = new ColumnDefinition();
                    cd.Width = new GridLength(marginBetweenColumns);
                    g.ColumnDefinitions.Add(cd);

                    columnCount++;
                }
                
                g.ColumnDefinitions.Add(new ColumnDefinition());
                StackPanel s = new StackPanel();
                Grid.SetColumn(s, columnCount);
                g.Children.Add(s);

                columnCount++;

                notFirstColumn = true;
            }

            ColumnDefinition cdd = new ColumnDefinition();
            cdd.Width = new GridLength(marginOnRight);
            g.ColumnDefinitions.Add(cdd);

            columnCount++;

            return g;
        }

        private void InitializeControlContent()
        {               
            if (HasTabs)
            {
                TabControl = new TabControl();

                TabControl.SelectionChanged += TabControl_SelectionChanged;

                Content = TabControl;

                foreach (KeyValuePair<string,TabInfo> t in TabList)
                {
                    TabItem i = new TabItem();
                    i.Header = t.Key ;
                    TabControl.Items.Add(i);
                    t.Value.TabItem = i;
                    i.Tag = t.Value;

                    i.Content = CreateColumnedContent();
                }

                if (TabControl.Items.Count > 0)
                    TabControl.SelectedIndex = 0;

                TabControlSelectedItem = TabControl.SelectedItem as TabItem;

            }
            else
                Content = CreateColumnedContent();

            if (MetaOnLeft)
                this.SizeChanged += FormVisualiser_SizeChanged;

           // Content = new Button();
        }

        private TabInfo GetTabInfoForWidthCorrection()
        {
            if (TabList == null)
                return null;

            if (HasTabs)
                return getActiveTabInfo();

            if (TabList.ContainsKey(""))
                return TabList[""];

            return null;
        }

        private void ScheduleWidthCorrection(bool allowUpdateLayout, int retryCount = 0)
        {
            if (!MetaOnLeft || isDisposed || TabList == null)
                return;

            if (retryCount == 0)
            {
                if (widthCorrectionScheduled)
                    return;

                widthCorrectionScheduled = true;
            }

            this.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    if (retryCount == 0)
                        widthCorrectionScheduled = false;

                    if (!MetaOnLeft || isDisposed || TabList == null)
                        return;

                    TabInfo tabInfo = GetTabInfoForWidthCorrection();
                    if (tabInfo == null)
                        return;

                    this.SizeChanged -= FormVisualiser_SizeChanged;
                    bool correctionDone = false;
                    widthCorrectionInProgress = true;
                    try
                    {
                        correctionDone = CorrectWidth(tabInfo, allowUpdateLayout);
                        if (correctionDone)
                            lastCorrectedWidth = this.ActualWidth;
                    }
                    finally
                    {
                        widthCorrectionInProgress = false;
                        this.SizeChanged += FormVisualiser_SizeChanged;
                    }

                    if (!correctionDone && retryCount < 5)
                        ScheduleWidthCorrection(true, retryCount + 1);
                }),
                retryCount == 0 ? System.Windows.Threading.DispatcherPriority.Loaded : System.Windows.Threading.DispatcherPriority.Render);
        }

        private void DataControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!MetaOnLeft || isDisposed || widthCorrectionInProgress)
                return;

            if (!e.WidthChanged)
                return;

            ScheduleWidthCorrection(false);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControlSelectedItem = (TabItem)TabControl.SelectedItem;

            if (e.Source == TabControl)
                SetKeyboardHighlightToFirst();

            if (MetaOnLeft && TabControlSelectedItem != null && TabControlSelectedItem.Tag is TabInfo tabInfo)
            {
                this.SizeChanged -= FormVisualiser_SizeChanged;
                bool correctionDone = CorrectWidth(tabInfo);
                if (correctionDone)
                    lastCorrectedWidth = this.ActualWidth;
                this.SizeChanged += FormVisualiser_SizeChanged;

                ScheduleWidthCorrection(true);
            }
        }

        private void FormVisualiser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged)
                return;

            if (TabList == null)
                return;

            if (this.ActualWidth < 1)
                return;

            if (Math.Abs(this.ActualWidth - lastCorrectedWidth) < 3)
                return;

            lastCorrectedWidth = this.ActualWidth;

            this.SizeChanged -= FormVisualiser_SizeChanged;
            bool correctionDone = false;
            try
            {
                if (HasTabs)
                {
                    TabInfo activeTab = getActiveTabInfo();
                    if (activeTab != null)
                        correctionDone = CorrectWidth(activeTab, false);
                }
                else
                {
                    if (TabList.ContainsKey(""))
                        correctionDone = CorrectWidth(TabList[""], false);
                }
            }
            finally
            {
                this.SizeChanged += FormVisualiser_SizeChanged;
            }

            if (!correctionDone)
                ScheduleWidthCorrection(false);
        }

        protected Panel GetUIPlace(string group,string section, ControlInfo ci)
        {
            TabInfo t = TabList[group];

            
            int targetColumn = (int)((double)t.CurrentNumberOfControls * (double)ColumnNumber / (double)t.TotalNumberOfControls);

            if (targetColumn >= ColumnNumber)
                targetColumn = ColumnNumber-1;

            t.CurrentNumberOfControls++;

            ci.Column = targetColumn;

            if (section != null)
            {
                if (t.Sections.ContainsKey(section))
                {
                    ci.Column = t.Sections[section].Column;

                    return ((Panel)t.Sections[section].Panel);
                }

                Panel toAdd;

                if (HasTabs)
                    toAdd=(Panel)((Grid)t.TabItem.Content).Children[targetColumn];
                else
                    toAdd=(Panel)((Grid)this.Content).Children[0];

                GroupBox g = new GroupBox();

                //Expander g = new Expander();

                g.BorderBrush = (Brush)FindResource("0ForegroundBrush");

                TextBlock Header = new TextBlock();
                Header.FontWeight = WpfUtil.BoldWeight;
                Header.Text = section;
                g.Header = Header;

                g.BorderThickness = new Thickness(2); // can be 1, but 2 is more separated

                toAdd.Children.Add(g);

                Border b = new Border(); // separator

                b.BorderThickness = new System.Windows.Thickness(0, controlLineVsControlLineSeparator, 0, 0);

                toAdd.Children.Add(b);

                StackPanel gp = new StackPanel();

                g.Content = gp;

                SectionInfo si = new SectionInfo();
                si.Panel = gp;
                si.Column = targetColumn;

                t.Sections.Add(section, si);

                return gp;
            }


            if(HasTabs)
                return (Panel)((Grid)t.TabItem.Content).Children[targetColumn];
            else
                return (Panel)((Grid)this.Content).Children[targetColumn];
        }

        bool BaseVertexEdgeAdded = false;

        protected void AddEdge(IVertex meta, bool isSet)
        {
            if (DisplayBaseVertex && BaseVertexEdgeAdded == false) { 
                BaseVertexEdge = getMetaForForm();
                BaseVertexEdgeAdded = true;
                AddEdge(BaseVertexEdge, false);
            }
            
            string group = getGroup(meta);
            string section = getSection(meta);  

            IVertex r = MinusZero.Instance.Root;

            TextBlock metaControl = new TextBlock();

            if (meta == null)
            {
                metaControl.Text = "XXX";
                metaControl.Height = 0;
            }else
                metaControl.Text = (string)meta.Value;

            metaControl.FontStyle = FontStyles.Italic;
            metaControl.FontWeight = WpfUtil.MetaWeight;
            metaControl.Foreground = (Brush)FindResource("0GrayBrush");
            metaControl.Background = Brushes.Transparent;

            if (!WpfUtil.HasParentsGotContextMenu(metaControl))
                metaControl.ContextMenu = new m0ContextMenu(this);

            System.Windows.FrameworkElement dataControl = null;
            
            if (isSet)
            {
                IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(null, null, Vertex.Get(false, @"BaseEdge:\To:"));

                TableVisualiser tableVisualiser = new TableVisualiser(baseEdgeVertex, Vertex, false);

                if (ExpertMode)
                    GraphUtil.SetVertexValue(tableVisualiser.Vertex, MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Table\ExpertMode"), "True");               

                // need to remove and add to have "transaction" // THIS DOES NOT WORK
                GraphUtil.CreateOrReplaceEdge(tableVisualiser.Vertex.Get(false, "ToShowEdgesMeta:"), r.Get(false, @"System\Meta\ZeroTypes\Edge\Meta"), meta);

               // IVertex v = tableVisualiser.Vertex.Get(false, "ToShowEdgesMeta:"); /////////////// this ToShowEdgesMeta is a trash bin XXX

                //tableVisualiser.Vertex.AddEdge(MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Table\ToShowEdgesMeta"), v);

                //GraphUtil.DeleteEdgeByMeta(tableVisualiser.Vertex, "ToShowEdgesMeta");                

                // no need for this

                dataControl = tableVisualiser;
            }
            else
            {
                if (meta == BaseVertexEdge)
                {
                    IVertex baseEdgeVertex = EdgeHelper.CreateTempEdgeVertex(Vertex.GetAll(false, @"BaseEdge:\To:").FirstOrDefault());

                    StringVisualiser sv = new StringVisualiser(baseEdgeVertex, this.Vertex, false);

                    //Edge.ReplaceEdgeVertexEdges(sv.Vertex.Get(false, "BaseEdge:"), Vertex.GetAll(false, @"BaseEdge:\To:").FirstOrDefault());

                    baseEdgeVertex.AddExternalReference();

                    dataControl = sv;
                }
                else
                if (ExecutableVisualiserFactory.IsExecutableVertex(meta))
                {
                    dataControl = ExecutableVisualiserFactory.CreateExecutableVisualiser(Vertex.GetAll(false, @"BaseEdge:\To:").FirstOrDefault(), meta);
                }
                else
                {
                    VisualiserEditWrapper w = new VisualiserEditWrapper(Vertex);

                    IEdge e;

                    e = Vertex.GetAll(false, @"BaseEdge:\To:\" + (string)meta.Value + ":").FirstOrDefault();

                    if (e == null) // no edge in data vertex
                    {
                        w.BaseEdge = new EasyEdge(Vertex.Get(false, @"BaseEdge:\To:"), meta, null);
                    }
                    else
                        w.BaseEdge = e;

                    dataControl = w;
                }
                
           
            }

            ControlInfo ci = new ControlInfo();

            ci.MetaControl = metaControl;
            ci.DataControl = dataControl;
            ci.BaseEdge = GetKeyboardHighlightBaseEdgeForControl(meta, isSet);
            ci.Order = TabList[group].ControlInfos.Count;
            ci.MetaControl.PreviewMouseLeftButtonDown += FormControl_PreviewMouseLeftButtonDown;
            ci.MetaControl.MouseLeftButtonDown += FormControl_MouseLeftButtonDown;
            ci.MetaControl.MouseLeftButtonUp += FormControl_MouseLeftButtonUp;
            ci.MetaControl.PreviewMouseMove += FormControl_PreviewMouseMove;
            ci.MetaControl.PreviewMouseRightButtonDown += FormControl_PreviewMouseRightButtonDown;
            ci.DataControl.PreviewMouseLeftButtonDown += FormControl_PreviewMouseLeftButtonDown;
            ci.DataControl.MouseLeftButtonDown += FormControl_MouseLeftButtonDown;
            ci.DataControl.MouseLeftButtonUp += FormControl_MouseLeftButtonUp;
            ci.DataControl.PreviewMouseMove += FormControl_PreviewMouseMove;
            ci.DataControl.PreviewMouseRightButtonDown += FormControl_PreviewMouseRightButtonDown;
            RegisterNestedKeyboardHighlightActivation(ci);
            ci.DataControl.SizeChanged += DataControl_SizeChanged;

            if (meta == null)
            { // BaseEdgeVertex
                IVertex metaVertex = MinusZero.Instance.CreateTempVertex();

                metaVertex.AddExternalReference();

                TabList[group].ControlInfos.Add(metaVertex, ci);
            }
            else
            {
                if (TabList[group].ControlInfos.ContainsKey(meta))
                {
                    int x = 0; // same meta sub vertex two times in meta vertex
                }
                else
                    TabList[group].ControlInfos.Add(meta, ci);
            }
            
            Panel place = GetUIPlace(group,section,ci);

            if (MetaAlignRight)
                metaControl.TextAlignment = TextAlignment.Right;
            else
                metaControl.TextAlignment = TextAlignment.Left;


            if (MetaOnLeft)
            {                
                StackPanel s=new StackPanel();
                s.Orientation=Orientation.Horizontal;

                ci.GapControl = new StackPanel();
                
                s.Children.Add(ci.GapControl);

                s.Children.Add(metaControl);

                Border b2 = new Border();

                b2.BorderThickness = new System.Windows.Thickness(metaVsDataSeparator, 0, 0, 0);

                s.Children.Add(b2);

                s.Children.Add(dataControl);

                place.Children.Add(s);
            }
            else
            {
                place.Children.Add(metaControl);
                
                place.Children.Add(dataControl);
            }


            Border b = new Border();

            b.BorderThickness = new System.Windows.Thickness(0, controlLineVsControlLineSeparator, 0, 0);

            place.Children.Add(b);
        }

        protected virtual void SetVertexDefaultValues()
        {
            IVertex scale = Vertex.Get(false, "Scale:");
            if (!object.Equals(scale.Value, 100))
                scale.Value = 100;

            IVertex columnNumber = Vertex.Get(false, "ColumnNumber:");
            if (!object.Equals(columnNumber.Value, 1))
                columnNumber.Value = 1;

            IVertex sectionsAsTabs =
                Vertex.Get(false, "SectionsAsTabs:");
            if (!object.Equals(sectionsAsTabs.Value, "False"))
                sectionsAsTabs.Value = "False";

            IVertex metaOnLeft = Vertex.Get(false, "MetaOnLeft:");
            if (!object.Equals(metaOnLeft.Value, "True"))
                metaOnLeft.Value = "True";
        }        

        public void ScaleChange()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get(false, "Scale:"))) / 100;

            if (scale != 1.0)
                this.LayoutTransform = new ScaleTransform(scale, scale);
            else
                this.LayoutTransform = null;
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        // LOCATION STUFF

        public IVertex GetEdgeByPoint(Point p)
        {
            TabInfo t = getActiveTabInfo();

            foreach(KeyValuePair<IVertex,ControlInfo> kvp in t.ControlInfos)
                if (VisualTreeHelper.HitTest(kvp.Value.MetaControl, TranslatePoint(p, kvp.Value.MetaControl)) != null)
                {
                    IVertex v = MinusZero.Instance.CreateTempVertex();
                    EdgeHelper.AddEdgeVertexEdgesOnlyTo(v, kvp.Key);
                    return(v);
                }
               
            return null;
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }

        private bool IsVertexCommanderMode()
        {
            return IsVertexCommanderKeyboardHighlightEnabled;
        }

        internal void EnableNestedVertexCommanderKeyboardHighlight()
        {
            foreach (IKeyboardHighlight nestedKeyboardHighlight in GetNestedKeyboardHighlights())
                nestedKeyboardHighlight.IsVertexCommanderKeyboardHighlightEnabled = true;
        }

        private void DisposeTabListDataControls()
        {
            if (TabList == null)
                return;

            foreach (TabInfo tabInfo in TabList.Values)
            {
                foreach (ControlInfo controlInfo in tabInfo.ControlInfos.Values)
                {
                    FrameworkElement dataControl = controlInfo.DataControl;

                    if (dataControl == null)
                        continue;

                    if (dataControl is IDisposable disposableDataControl)
                        disposableDataControl.Dispose();

                    controlInfo.DataControl = null;
                }

                tabInfo.ControlInfos.Clear();
            }

            TabList.Clear();
            TabList = null;
        }
    }
}
