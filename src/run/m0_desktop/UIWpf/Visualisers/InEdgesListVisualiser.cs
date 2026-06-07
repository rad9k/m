using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using m0.Foundation;
using m0.Graph;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Foundation;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;
using m0.Util;
using m0.ZeroTypes;
using m0.ZeroUML;

namespace m0.UIWpf.Visualisers
{
    public class InEdgesListVisualiser : StackPanel, IListVisualiser, ITypedEdge, IKeyboardHighlight
    {
        private const double ColumnResizeCursorHotZone = 6.0;

        private static readonly IValueConverter FromIconSourceConverter = new InEdgesListVisualiserFromIconSourceConverter();
        private static readonly IValueConverter FromIconVisibilityConverter = new InEdgesListVisualiserFromIconVisibilityConverter();
        private static readonly IValueConverter InEdgePathConverter = new InEdgesListVisualiserPathConverter();

        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }

        public bool SelectionProphibited { get; set; }

        protected DataGrid ThisDataGrid;

        protected bool TurnOffSelectedItemsUpdate = false;

        protected bool TurnOffSelectedVerticesUpdate = false;

        private int currentHighlightPosition = -1;
        private bool isBeforeFirstPosition;
        private bool isAfterLastPosition;
        private IEdge currentKeyboardHighlightedEdge;

        static string[] _MetaTriggeringUpdateVertex = new string[] { };
        public virtual string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { "GridStyle", "ShowHeader" };
        public virtual string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public virtual void ViewAttributesUpdated() { ResetView(); }

        protected IVertex parentVisualiser;

        public InEdgesListVisualiser(IEdge edge)
        {
            Edge = edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }

        protected bool isVolatile;

        public InEdgesListVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            this.parentVisualiser = parentVisualiser;
            this.isVolatile = isVolatile;

            ThisDataGrid = new DataGrid();

            Children.Add(ThisDataGrid);

            ThisDataGrid.AllowDrop = false;
            ThisDataGrid.AutoGenerateColumns = false;
            ThisDataGrid.CanUserAddRows = false;
            ThisDataGrid.CanUserDeleteRows = false;
            ThisDataGrid.CanUserResizeColumns = true;
            ThisDataGrid.IsReadOnly = true;
            ThisDataGrid.RowBackground = (Brush)FindResource("0BackgroundBrush");
            ThisDataGrid.Background = (Brush)FindResource("0BackgroundBrush");
            ThisDataGrid.HorizontalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");
            ThisDataGrid.VerticalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");

            ThisDataGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
            ThisDataGrid.ColumnHeaderStyle = CreateResizableColumnHeaderStyle(false, false);
            ThisDataGrid.RowStyle = CreateHighlightedRowStyle();
            ThisDataGrid.SelectedValuePath = "From";
            VirtualizingStackPanel.SetIsVirtualizing(ThisDataGrid, false);

            MinusZero mz = MinusZero.Instance;

            if (mz != null && mz.IsInitialized)
            {
                PlatformClassInitialize(baseEdgeVertex);

                SetVertexDefaultValues();
                CreateView();

                ThisDataGrid.SelectionChanged += OnSelectionChanged;
                ThisDataGrid.PreviewKeyDown += OnKeyboardHighlightPreviewKeyDown;
                ThisDataGrid.MouseDoubleClick += OnKeyboardHighlightMouseDoubleClick;
            }
        }

        public int CurrentHighlightPosition
        {
            get { return currentHighlightPosition; }
        }

        public bool IsBeforeFirstPosition
        {
            get { return isBeforeFirstPosition; }
        }

        public bool IsAfterLastPosition
        {
            get { return isAfterLastPosition; }
        }

        public bool IsFirstPosition
        {
            get { return currentHighlightPosition == 0 && !isBeforeFirstPosition && !isAfterLastPosition; }
            set
            {
                if (value)
                    SetFirstKeyboardHighlightPosition();
            }
        }

        public bool IsLastPosition
        {
            get { return currentHighlightPosition == GetKeyboardHighlightItemCount() - 1 && !isBeforeFirstPosition && !isAfterLastPosition; }
            set
            {
                if (value)
                    SetLastKeyboardHighlightPosition();
            }
        }

        public bool CanGoBeforeFirstPosition
        {
            get { return true; }
        }

        public bool CanGoAfterLastPosition
        {
            get { return true; }
        }

        public bool HasKeyboardHighlightItems
        {
            get { return GetKeyboardHighlightItemCount() > 0; }
        }

        public event EventHandler GoneBeforeFirstPosition;

        public event EventHandler GoneAfterLastPosition;

        public event EventHandler KeyboardHighlightEnterPressed;

        public event EventHandler KeyboardHighlightActivated;

        public IEdge KeyboardHighlightedEdge
        {
            get { return currentKeyboardHighlightedEdge; }
        }

        private Style CreateHighlightedRowStyle()
        {
            Style rowStyle = new Style(typeof(DataGridRow));

            rowStyle.Setters.Add(new Setter(Control.BackgroundProperty, (Brush)FindResource("0BackgroundBrush")));
            rowStyle.Setters.Add(new Setter(Control.ForegroundProperty, (Brush)FindResource("0ForegroundBrush")));
            rowStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

            Trigger mouseOverTrigger = new Trigger
            {
                Property = DataGridRow.IsMouseOverProperty,
                Value = true
            };
            mouseOverTrigger.Setters.Add(new Setter(Control.ForegroundProperty, (Brush)FindResource("0HighlightBrush")));
            rowStyle.Triggers.Add(mouseOverTrigger);

            Trigger selectedTrigger = new Trigger
            {
                Property = DataGridRow.IsSelectedProperty,
                Value = true
            };
            selectedTrigger.Setters.Add(new Setter(Control.BackgroundProperty, (Brush)FindResource("0SelectionBrush")));
            selectedTrigger.Setters.Add(new Setter(Control.BorderBrushProperty, (Brush)FindResource("0SelectionBrush")));
            rowStyle.Triggers.Add(selectedTrigger);

            return rowStyle;
        }

        private Style CreateHighlightedCellStyle()
        {
            Style baseCellStyle = (Style)FindResource("0ListValueColumn");
            Style cellStyle = new Style(typeof(DataGridCell), baseCellStyle);

            DataTrigger rowMouseOverTrigger = new DataTrigger
            {
                Binding = new Binding("IsMouseOver")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1)
                },
                Value = true
            };
            rowMouseOverTrigger.Setters.Add(new Setter(Control.ForegroundProperty, (Brush)FindResource("0HighlightBrush")));
            cellStyle.Triggers.Add(rowMouseOverTrigger);

            Trigger selectedTrigger = new Trigger
            {
                Property = DataGridCell.IsSelectedProperty,
                Value = true
            };
            selectedTrigger.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            selectedTrigger.Setters.Add(new Setter(Control.ForegroundProperty, (Brush)FindResource("0BackgroundBrush")));
            selectedTrigger.Setters.Add(new Setter(Control.BorderBrushProperty, Brushes.Transparent));
            cellStyle.Triggers.Add(selectedTrigger);

            return cellStyle;
        }

        private void OnKeyboardHighlightPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Up && e.Key != Key.Down && e.Key != Key.Space && e.Key != Key.Enter)
                return;

            e.Handled = true;

            if (currentHighlightPosition == -1)
                return;

            if (e.Key == Key.Up)
                MoveKeyboardHighlight(-1);
            else if (e.Key == Key.Down)
                MoveKeyboardHighlight(1);
            else if (e.Key == Key.Space)
                ToggleKeyboardHighlightedEdgeSelection();
            else
                RaiseKeyboardHighlightEnterPressed();
        }

        private void SetFirstKeyboardHighlightPosition()
        {
            int itemCount = GetKeyboardHighlightItemCount();

            if (itemCount == 0)
                GoAfterLastKeyboardHighlightPosition();
            else
                SetKeyboardHighlightPosition(0);
        }

        private void SetLastKeyboardHighlightPosition()
        {
            int itemCount = GetKeyboardHighlightItemCount();

            if (itemCount == 0)
                GoBeforeFirstKeyboardHighlightPosition();
            else
                SetKeyboardHighlightPosition(itemCount - 1);
        }

        public void MoveKeyboardHighlight(int positionDelta)
        {
            int itemCount = GetKeyboardHighlightItemCount();

            if (itemCount == 0)
            {
                if (positionDelta < 0)
                    GoBeforeFirstKeyboardHighlightPosition();
                else
                    GoAfterLastKeyboardHighlightPosition();

                return;
            }

            int newPosition = currentHighlightPosition + positionDelta;

            if (newPosition < 0)
                GoBeforeFirstKeyboardHighlightPosition();
            else if (newPosition >= itemCount)
                GoAfterLastKeyboardHighlightPosition();
            else
                SetKeyboardHighlightPosition(newPosition);
        }

        public void MoveKeyboardHighlight(KeyboardHighlightMoveDirection direction)
        {
            if (direction == KeyboardHighlightMoveDirection.Up)
                MoveKeyboardHighlight(-1);
            else if (direction == KeyboardHighlightMoveDirection.Down)
                MoveKeyboardHighlight(1);
        }

        private void SetKeyboardHighlightPosition(int position)
        {
            ClearKeyboardHighlight();

            currentHighlightPosition = position;
            isBeforeFirstPosition = false;
            isAfterLastPosition = false;
            currentKeyboardHighlightedEdge = GetKeyboardHighlightedEdge();

            ApplyKeyboardHighlight();
        }

        private void GoBeforeFirstKeyboardHighlightPosition()
        {
            ClearKeyboardHighlight();

            currentHighlightPosition = -1;
            isBeforeFirstPosition = true;
            isAfterLastPosition = false;

            if (GoneBeforeFirstPosition != null)
                GoneBeforeFirstPosition(this, EventArgs.Empty);
        }

        private void GoAfterLastKeyboardHighlightPosition()
        {
            ClearKeyboardHighlight();

            currentHighlightPosition = -1;
            isBeforeFirstPosition = false;
            isAfterLastPosition = true;

            if (GoneAfterLastPosition != null)
                GoneAfterLastPosition(this, EventArgs.Empty);
        }

        private void ApplyKeyboardHighlight()
        {
            DataGridRow row = GetKeyboardHighlightRow();

            if (row == null)
                return;

            Brush foregroundBrush = IsKeyboardHighlightedEdgeSelected()
                ? (Brush)FindResource("0ForegroundBrush")
                : (Brush)FindResource("0HighlightForegroundBrush");

            row.Background = (Brush)FindResource("0HighlightBrush");
            row.Foreground = foregroundBrush;
            row.BorderBrush = (Brush)FindResource("0HighlightBrush");

            foreach (DataGridCell cell in FindVisualChildren<DataGridCell>(row))
            {
                cell.Background = (Brush)FindResource("0HighlightBrush");
                cell.Foreground = foregroundBrush;
                cell.BorderBrush = (Brush)FindResource("0HighlightBrush");
            }

            row.BringIntoView();
        }

        public void ToggleKeyboardHighlightedEdgeSelection()
        {
            if (SelectionProphibited)
                return;

            IEdge keyboardHighlightedEdge = GetKeyboardHighlightedEdge();

            if (keyboardHighlightedEdge == null)
                return;

            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");
            IEdge selectedEdgeVertexEdge = EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, keyboardHighlightedEdge);

            Interaction.BeginInteractionWithGraph();

            if (selectedEdgeVertexEdge != null)
                selectedEdges.DeleteEdge(selectedEdgeVertexEdge);
            else
                EdgeHelper.AddEdgeVertex(selectedEdges, keyboardHighlightedEdge);

            Interaction.EndInteractionWithGraph();

            RefreshVisualStatesAfterItemsChanged();
        }

        private void RaiseKeyboardHighlightEnterPressed()
        {
            RaiseKeyboardHighlightActivated();

            if (KeyboardHighlightEnterPressed != null)
                KeyboardHighlightEnterPressed(this, EventArgs.Empty);
        }

        private void RaiseKeyboardHighlightActivated()
        {
            if (KeyboardHighlightedEdge == null)
                return;

            if (KeyboardHighlightActivated != null)
                KeyboardHighlightActivated(this, EventArgs.Empty);
        }

        private void OnKeyboardHighlightMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = GetDataGridRowFromEventSource(e.OriginalSource as DependencyObject);

            if (row == null)
                return;

            int position = GetKeyboardHighlightEdges().IndexOf(row.Item as IEdge);

            if (position < 0)
                return;

            SetKeyboardHighlightPosition(position);
            RaiseKeyboardHighlightEnterPressed();
            e.Handled = true;
        }

        private DataGridRow GetDataGridRowFromEventSource(DependencyObject dependencyObject)
        {
            while (dependencyObject != null && !(dependencyObject is DataGridRow))
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);

            return dependencyObject as DataGridRow;
        }

        private bool IsKeyboardHighlightedEdgeSelected()
        {
            IEdge keyboardHighlightedEdge = GetKeyboardHighlightedEdge();

            if (keyboardHighlightedEdge == null)
                return false;

            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");

            return selectedEdges != null && EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, keyboardHighlightedEdge) != null;
        }

        private IEdge GetKeyboardHighlightedEdge()
        {
            List<IEdge> edges = GetKeyboardHighlightEdges();

            if (currentHighlightPosition < 0 || currentHighlightPosition >= edges.Count)
                return null;

            return edges[currentHighlightPosition];
        }

        // Logical edge order taken straight from Items (no layout, no container realization).
        private List<IEdge> GetKeyboardHighlightEdges()
        {
            List<IEdge> edges = new List<IEdge>();

            if (ThisDataGrid == null || ThisDataGrid.Items == null)
                return edges;

            foreach (object item in ThisDataGrid.Items)
                if (item is IEdge edge)
                    edges.Add(edge);

            return edges;
        }

        public void ClearKeyboardHighlight()
        {
            DataGridRow row = GetKeyboardHighlightRow();

            if (row != null)
            {
                row.ClearValue(Control.BackgroundProperty);
                row.ClearValue(Control.ForegroundProperty);
                row.ClearValue(Control.BorderBrushProperty);

                foreach (DataGridCell cell in FindVisualChildren<DataGridCell>(row))
                {
                    cell.ClearValue(Control.BackgroundProperty);
                    cell.ClearValue(Control.ForegroundProperty);
                    cell.ClearValue(Control.BorderBrushProperty);
                }

                if (ThisDataGrid.SelectedItems.Contains(row.Item))
                    ApplySelectedRowVisualState(row);
            }

            currentHighlightPosition = -1;
            isBeforeFirstPosition = false;
            isAfterLastPosition = false;
            currentKeyboardHighlightedEdge = null;
        }

        private DataGridRow GetKeyboardHighlightRow()
        {
            List<IEdge> edges = GetKeyboardHighlightEdges();

            if (currentHighlightPosition < 0 || currentHighlightPosition >= edges.Count)
                return null;

            object item = edges[currentHighlightPosition];
            ThisDataGrid.ScrollIntoView(item);
            ThisDataGrid.UpdateLayout();

            return ThisDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
        }

        private int GetKeyboardHighlightItemCount()
        {
            if (ThisDataGrid == null || ThisDataGrid.Items == null)
                return 0;

            int count = 0;

            foreach (object item in ThisDataGrid.Items)
                if (item is IEdge)
                    count++;

            return count;
        }

        private Style CreateResizableColumnHeaderStyle(bool drawHorizontalHeaderLine, bool drawVerticalHeaderLine)
        {
            Style headerStyle = new Style(typeof(DataGridColumnHeader));
            Thickness headerBorderThickness = new Thickness(0, 0, 1, drawHorizontalHeaderLine ? 1 : 0);
            Brush verticalGripBrush = drawVerticalHeaderLine
                ? (Brush)FindResource("0ForegroundBrush")
                : (Brush)FindResource("0VeryVeryVeryLightForegroundBrush");

            headerStyle.Setters.Add(new Setter(Control.ForegroundProperty, (Brush)FindResource("0VeryLightForegroundBrush")));
            headerStyle.Setters.Add(new Setter(DataGridColumnHeader.SeparatorBrushProperty, verticalGripBrush));
            headerStyle.Setters.Add(new Setter(DataGridColumnHeader.SeparatorVisibilityProperty, Visibility.Visible));
            headerStyle.Setters.Add(new Setter(Control.BorderBrushProperty, verticalGripBrush));
            headerStyle.Setters.Add(new Setter(Control.BorderThicknessProperty, headerBorderThickness));
            headerStyle.Setters.Add(new EventSetter(FrameworkElement.LoadedEvent, new RoutedEventHandler(OnColumnHeaderLoaded)));
            headerStyle.Setters.Add(new EventSetter(PreviewMouseMoveEvent, new MouseEventHandler(OnColumnHeaderMouseMove)));
            headerStyle.Setters.Add(new EventSetter(MouseMoveEvent, new MouseEventHandler(OnColumnHeaderMouseMove)));
            headerStyle.Setters.Add(new EventSetter(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnColumnHeaderPreviewMouseLeftButtonDown)));
            headerStyle.Setters.Add(new EventSetter(PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(OnColumnHeaderPreviewMouseLeftButtonUp)));
            headerStyle.Setters.Add(new EventSetter(MouseLeaveEvent, new MouseEventHandler(OnColumnHeaderMouseLeave)));

            return headerStyle;
        }

        private void OnColumnHeaderLoaded(object sender, RoutedEventArgs e)
        {
            DataGridColumnHeader header = sender as DataGridColumnHeader;

            if (header == null)
                return;

            foreach (Thumb thumb in FindVisualChildren<Thumb>(header))
                thumb.Cursor = Cursors.SizeWE;
        }

        private void OnColumnHeaderMouseMove(object sender, MouseEventArgs e)
        {
            DataGridColumnHeader header = sender as DataGridColumnHeader;

            if (header == null || header.Column == null || !ThisDataGrid.CanUserResizeColumns || !header.Column.CanUserResize)
                return;

            Point point = e.GetPosition(header);
            bool isOnResizeHotZone =
                point.X <= ColumnResizeCursorHotZone ||
                point.X >= header.ActualWidth - ColumnResizeCursorHotZone;

            header.Cursor = isOnResizeHotZone ? Cursors.SizeWE : null;
        }

        private void OnColumnHeaderPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridColumnHeader header = sender as DataGridColumnHeader;

            if (header != null && header.Cursor == Cursors.SizeWE)
                Mouse.OverrideCursor = Cursors.SizeWE;
        }

        private void OnColumnHeaderPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void OnColumnHeaderMouseLeave(object sender, MouseEventArgs e)
        {
            DataGridColumnHeader header = sender as DataGridColumnHeader;

            if (header != null)
                header.Cursor = null;

            if (e.LeftButton != MouseButtonState.Pressed)
                Mouse.OverrideCursor = null;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null)
                yield break;

            for (int index = 0; index < VisualTreeHelper.GetChildrenCount(dependencyObject); index++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, index);

                if (child is T)
                    yield return (T)child;

                foreach (T descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }

        protected virtual void PlatformClassInitialize(IVertex baseEdgeVertex)
        {
            new ListVisualiserHelper(parentVisualiser,
                isVolatile,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\InEdgesList"),
                this,
                "InEdgesListVisualiser",
                this,
                false,
                new List<string> { @"", @"BaseEdge:\To:" },
                "AtomVisualiserFull",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitFirst);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void UnselectAllSelectedEdges()
        {
            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            IVertex sv = Vertex.Get(false, "SelectedEdges:");

            GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            ////////////////////////////////////////
        }

        protected void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionProphibited)
            {
                TurnOffSelectedVerticesUpdate = true;
                ThisDataGrid.SelectedItems.Clear();
                TurnOffSelectedVerticesUpdate = false;
                return;
            }

            if (!TurnOffSelectedVerticesUpdate)
            {
                TurnOffSelectedItemsUpdate = true;

                IVertex sv = Vertex.Get(false, "SelectedEdges:");

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                UnselectAllSelectedEdges();

                foreach (IEdge selectedEdge in ThisDataGrid.SelectedItems)
                    EdgeHelper.AddEdgeVertex(sv, selectedEdge);

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////

                TurnOffSelectedItemsUpdate = false;
                RefreshSelectedRowsVisualState();
                RefreshKeyboardHighlightAfterItemsChanged();
            }
        }

        protected virtual void CreateView()
        {
            ThisDataGrid.Columns.Clear();
            UpdateHeaderVisibility();

            ThisDataGrid.Columns.Add(CreateFromIconColumn());
            AddTextColumn("From", new Binding("From.Value"), true);
            AddTextColumn("Meta", new Binding("Meta.Value"));

            Binding pathBinding = new Binding("");
            pathBinding.Mode = BindingMode.OneWay;
            pathBinding.Converter = InEdgePathConverter;
            AddTextColumn("Path", pathBinding);
        }

        private void UpdateHeaderVisibility()
        {
            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "ShowHeader:"), "False"))
                ThisDataGrid.HeadersVisibility = DataGridHeadersVisibility.None;
            else
                ThisDataGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
        }

        private DataGridTemplateColumn CreateFromIconColumn()
        {
            DataGridTemplateColumn iconColumn = new DataGridTemplateColumn();
            iconColumn.CellStyle = CreateHighlightedCellStyle();
            iconColumn.CellTemplate = new DataTemplate();
            iconColumn.CanUserResize = true;
            iconColumn.Header = new TextBlock
            {
                Text = " ",
                FontStyle = FontStyles.Italic,
                Foreground = (Brush)FindResource("0VeryLightForegroundBrush"),
                Margin = new Thickness(4, 0, 4, 0)
            };

            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(Image));
            factory.SetValue(Image.WidthProperty, WpfUtil.IconSize);
            factory.SetValue(Image.HeightProperty, WpfUtil.IconSize);
            factory.SetValue(Image.MarginProperty, new Thickness(0, 0, 3, 0));
            factory.SetValue(Image.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.SetValue(Image.StretchProperty, Stretch.Uniform);
            factory.SetValue(Image.SnapsToDevicePixelsProperty, true);
            factory.SetValue(Image.UseLayoutRoundingProperty, true);
            factory.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.Fant);

            Binding sourceBinding = new Binding("");
            sourceBinding.Mode = BindingMode.OneWay;
            sourceBinding.Converter = FromIconSourceConverter;
            factory.SetBinding(Image.SourceProperty, sourceBinding);

            Binding visibilityBinding = new Binding("");
            visibilityBinding.Mode = BindingMode.OneWay;
            visibilityBinding.Converter = FromIconVisibilityConverter;
            factory.SetBinding(Image.VisibilityProperty, visibilityBinding);

            iconColumn.CellTemplate.VisualTree = factory;

            return iconColumn;
        }

        protected virtual void AddTextColumn(string header, Binding binding, bool isBold = false)
        {
            DataGridTextColumn column = new DataGridTextColumn();

            binding.Mode = BindingMode.OneWay;
            column.Binding = binding;
            column.CanUserResize = true;
            column.CellStyle = CreateHighlightedCellStyle();
            column.FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal;
            column.Header = new TextBlock
            {
                Text = header + " ",
                FontStyle = FontStyles.Italic,
                Foreground = (Brush)FindResource("0VeryLightForegroundBrush"),
                Margin = new Thickness(4, 0, 4, 0)
            };

            ThisDataGrid.Columns.Add(column);
        }

        protected void ResetView()
        {
            CreateView();

            ThisDataGrid.HorizontalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");
            ThisDataGrid.VerticalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");
            bool drawHorizontalHeaderLine = false;
            bool drawVerticalHeaderLine = false;

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "Vertical"))
            {
                ThisDataGrid.BorderThickness = new Thickness(0);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.Vertical;
                drawVerticalHeaderLine = true;
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "Horizontal"))
            {
                ThisDataGrid.BorderThickness = new Thickness(0);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
                drawHorizontalHeaderLine = true;
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "All"))
            {
                ThisDataGrid.BorderThickness = new Thickness(0);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.All;
                drawHorizontalHeaderLine = true;
                drawVerticalHeaderLine = true;
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "AllAndRound"))
            {
                ThisDataGrid.BorderThickness = new Thickness(1);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.All;
                ThisDataGrid.BorderBrush = (Brush)FindResource("0ForegroundBrush");
                drawHorizontalHeaderLine = true;
                drawVerticalHeaderLine = true;
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "Round"))
            {
                ThisDataGrid.BorderThickness = new Thickness(1);
                ThisDataGrid.BorderBrush = (Brush)FindResource("0LightGrayBrush");
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.None;
            }
            else
            {
                ThisDataGrid.BorderThickness = new Thickness(0);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.None;
            }

            ThisDataGrid.ColumnHeaderStyle = CreateResizableColumnHeaderStyle(drawHorizontalHeaderLine, drawVerticalHeaderLine);
            RefreshVisualStatesAfterItemsChanged();
        }

        public void ScaleChange()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get(false, "Scale:"))) / 100;

            if (scale != 1.0)
                LayoutTransform = new ScaleTransform(scale, scale);
            else
                LayoutTransform = null;
        }

        public void SelectedVerticesUpdated()
        {
            if (SelectedEdgesChange != null)
                SelectedEdgesChange();

            if (SelectionProphibited)
                return;

            if (TurnOffSelectedItemsUpdate)
                return;

            TurnOffSelectedVerticesUpdate = true;

            ThisDataGrid.SelectedItems.Clear();

            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");

            if (selectedEdges != null)
            {
                foreach (IEdge selectedEdgeVertexEdge in selectedEdges)
                {
                    if (selectedEdgeVertexEdge.Meta.Value.ToString() != "Edge")
                        continue;

                    IVertex edgeVertex = selectedEdgeVertexEdge.To;
                    IVertex from = edgeVertex.Get(false, "From:");
                    IVertex meta = edgeVertex.Get(false, "Meta:");
                    IVertex to = edgeVertex.Get(false, "To:");

                    foreach (object item in ThisDataGrid.Items)
                    {
                        IEdge inEdge = item as IEdge;

                        if (inEdge != null && inEdge.From == from && inEdge.Meta == meta && inEdge.To == to)
                            ThisDataGrid.SelectedItems.Add(inEdge);
                    }
                }
            }

            TurnOffSelectedVerticesUpdate = false;
        }

        protected virtual void SetVertexDefaultValues()
        {
            Vertex.Get(false, "ShowHeader:").Value = "True";
            Vertex.Get(false, "Scale:").Value = 100;

            GraphUtil.ReplaceEdge(Vertex, "GridStyle", MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\GridStyleEnum\None"));
        }

        public virtual void BaseEdgeToUpdated()
        {
            IVertex baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

            IEnumerable itemsSourceValue = null;

            if (baseVertex != null)
            {
                ResetView();

                itemsSourceValue = baseVertex.InEdges.ToList();

                IList<IEdge> visibleInEdges = new List<IEdge>();

                foreach (IEdge edge in itemsSourceValue)
                    if (GraphUtil.GetQueryOutCount(edge.Meta, "$Hide", null) == 0)
                        visibleInEdges.Add(edge);

                ThisDataGrid.ItemsSource = visibleInEdges;
                RefreshVisualStatesAfterItemsChanged();
            }
        }

        private void RefreshVisualStatesAfterItemsChanged()
        {
            SelectedVerticesUpdated();
            RefreshSelectedRowsVisualState();
            RefreshKeyboardHighlightAfterItemsChanged();
        }

        private void RefreshSelectedRowsVisualState()
        {
            if (ThisDataGrid == null || ThisDataGrid.Items == null)
                return;

            foreach (object item in ThisDataGrid.Items)
            {
                DataGridRow row = GetDataGridRow(item);

                if (row == null)
                    continue;

                ClearSelectedRowVisualState(row);

                if (ThisDataGrid.SelectedItems.Contains(item))
                    ApplySelectedRowVisualState(row);
            }
        }

        private DataGridRow GetDataGridRow(object item)
        {
            ThisDataGrid.ScrollIntoView(item);
            ThisDataGrid.UpdateLayout();

            return ThisDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
        }

        private void ApplySelectedRowVisualState(DataGridRow row)
        {
            row.Background = (Brush)FindResource("0SelectionBrush");
            row.Foreground = (Brush)FindResource("0BackgroundBrush");
            row.BorderBrush = (Brush)FindResource("0SelectionBrush");

            foreach (DataGridCell cell in FindVisualChildren<DataGridCell>(row))
            {
                cell.Background = Brushes.Transparent;
                cell.Foreground = (Brush)FindResource("0BackgroundBrush");
                cell.BorderBrush = Brushes.Transparent;
            }
        }

        private void ClearSelectedRowVisualState(DataGridRow row)
        {
            row.ClearValue(Control.BackgroundProperty);
            row.ClearValue(Control.ForegroundProperty);
            row.ClearValue(Control.BorderBrushProperty);

            foreach (DataGridCell cell in FindVisualChildren<DataGridCell>(row))
            {
                cell.ClearValue(Control.BackgroundProperty);
                cell.ClearValue(Control.ForegroundProperty);
                cell.ClearValue(Control.BorderBrushProperty);
            }
        }

        private void RefreshKeyboardHighlightAfterItemsChanged()
        {
            if (currentHighlightPosition == -1)
                return;

            int itemCount = GetKeyboardHighlightItemCount();

            if (itemCount == 0)
                return;

            if (currentHighlightPosition >= itemCount)
                currentHighlightPosition = itemCount - 1;

            ApplyKeyboardHighlight();
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                VisualiserHelper.Dispose();
            }
        }

        public IVertex GetEdgeByPoint(Point point)
        {
            var headersPresenter = WpfUtil.FindVisualChild<DataGridColumnHeadersPresenter>(ThisDataGrid);

            if (headersPresenter != null && point.Y <= headersPresenter.ActualHeight)
                return null;

            foreach (var item in ThisDataGrid.Items)
            {
                var row = ThisDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;

                if (row != null && VisualTreeHelper.HitTest(row, TranslatePoint(point, row)) != null)
                {
                    IVertex vertex = MinusZero.Instance.CreateTempVertex();
                    EdgeHelper.AddEdgeVertexEdges(vertex, (IEdge)row.Item);
                    return vertex;
                }
            }

            if (GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"Home:\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "StartAndEnd"))
                return Vertex.Get(false, "BaseEdge:");
            else
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

        private class InEdgesListVisualiserFromIconSourceConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                IEdge edge = value as IEdge;

                if (edge == null)
                    return null;

                return IconServer.GetIconByVertex(edge.From);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private class InEdgesListVisualiserFromIconVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                IEdge edge = value as IEdge;

                if (edge == null || IconServer.GetIconByVertex(edge.From) == null)
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private class InEdgesListVisualiserPathConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                IEdge edge = value as IEdge;

                if (edge == null || edge.From == null)
                    return "";

                string path = GraphUtil.GetQueryBetweenVertexes_byInEdges(edge.From, MinusZero.Instance.Root);

                return path ?? "";
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }
}
