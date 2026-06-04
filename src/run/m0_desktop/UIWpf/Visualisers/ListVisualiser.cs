using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using m0.Foundation;
using System.Windows.Data;
using m0.Graph;
using m0.ZeroUML;
using m0.ZeroTypes;
using m0.Util;
using System.Windows.Media;
using System.Windows;
using m0.UIWpf.Controls;
using m0.UIWpf.Foundation;
using System.Windows.Input;
using m0.UIWpf.Commands;
using System.Windows.Controls.Primitives;
using m0.UIWpf.Visualisers.Helper;
using m0.Graph.ExecutionFlow;
using m0.User.Process.UX;
using System.Windows.Forms;
using System.Globalization;

namespace m0.UIWpf.Visualisers
{
    public class ListVisualiser : StackPanel,  IListVisualiser, ITypedEdge, IKeyboardHighlight
    {
        private static readonly IValueConverter EdgeIconSourceConverter = new ListVisualiserEdgeIconSourceConverter();
        private static readonly IValueConverter EdgeIconVisibilityConverter = new ListVisualiserEdgeIconVisibilityConverter();

        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        public bool SelectionProphibited { get; set; }

        protected System.Windows.Controls.DataGrid ThisDataGrid;

        protected bool TurnOffSelectedItemsUpdate = false;

        protected bool TurnOffSelectedVerticesUpdate = false;

        private bool selectedItemsGraphSyncPendingUntilMouseUp;
        private Point dataGridDndStartPoint;
        private Point dataGridDndStartPointInDataGrid;
        private bool dataGridDndHasButtonBeenDown;
        private bool isDataGridDndDragging;
        private bool preserveSelectedEdgesVisualStateUntilMouseUp;
        private object dataGridMouseDownFullRowItem;

        private int currentHighlightPosition = -1;
        private bool isBeforeFirstPosition;
        private bool isAfterLastPosition;

        static string[] _MetaTriggeringUpdateVertex = new string[] { };
        public virtual string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { "IsMetaRightAlign", "IsAllVisualisersEdit", "ShowMeta", "ShowIcons", "GridStyle", "FilterQuery", "ShowHeader" };
        public virtual string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public virtual void ViewAttributesUpdated() { ResetView(); }

        protected IVertex parentVisualiser;

        // TypedEdge START
        
        public ListVisualiser(IEdge _edge)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);            
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        protected bool isVolatile;

        public ListVisualiser(IVertex baseEdgeVertex, IVertex _parentVisualiser, bool _isVolatile)
        {            
            parentVisualiser = _parentVisualiser;

            isVolatile = _isVolatile;

            ThisDataGrid = new System.Windows.Controls.DataGrid();

            this.Children.Add(ThisDataGrid);

            AddFooter();

            ThisDataGrid.AllowDrop = true;

            ThisDataGrid.AutoGenerateColumns = false;

            ThisDataGrid.RowBackground = (Brush)FindResource("0BackgroundBrush");
            ThisDataGrid.Background = (Brush)FindResource("0BackgroundBrush");
            ThisDataGrid.HorizontalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");
            ThisDataGrid.VerticalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");

            ThisDataGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
            ThisDataGrid.ColumnHeaderStyle = CreateColumnHeaderStyle(false, false);
            ThisDataGrid.RowStyle = CreateHighlightedRowStyle();

            ThisDataGrid.SelectedValuePath = "To";
            VirtualizingStackPanel.SetIsVirtualizing(ThisDataGrid, false);
            MinusZero mz = MinusZero.Instance;

            if (mz != null && mz.IsInitialized)
            {
                PlatformClassInitialize(baseEdgeVertex);

                SetVertexDefaultValues();

                CreateView();               

                ThisDataGrid.SelectionChanged += _OnSelectionChanged;
                ThisDataGrid.PreviewMouseLeftButtonDown += OnDataGridPreviewMouseLeftButtonDown;
                ThisDataGrid.PreviewMouseMove += OnDataGridPreviewMouseMove;
                ThisDataGrid.PreviewMouseLeftButtonUp += OnDataGridPreviewMouseLeftButtonUp;
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
            get { return GetKeyboardHighlightedEdge(); }
        }

        protected virtual void PlatformClassInitialize(IVertex baseEdgeVertex)
        {
            new ListVisualiserHelper(parentVisualiser,
             isVolatile,
             MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\List"),
             this, 
             "ListVisualiser", 
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

            if (sv != null)
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        protected void _OnSelectionChanged(object sender, SelectionChangedEventArgs e){
            if (SelectionProphibited)
            {
                TurnOffSelectedVerticesUpdate = true;
                ThisDataGrid.SelectedItems.Clear();
                TurnOffSelectedVerticesUpdate = false;
                return;
            }

            if (!TurnOffSelectedVerticesUpdate)
            {
                if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    selectedItemsGraphSyncPendingUntilMouseUp = true;
                    preserveSelectedEdgesVisualStateUntilMouseUp = true;
                    RefreshSelectedRowsVisualState();
                    RefreshKeyboardHighlightAfterItemsChanged();

                    return;
                }

                SyncSelectedItemsToSelectedEdges();
            }            
        }

        private void OnDataGridPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dataGridDndHasButtonBeenDown = false;
            preserveSelectedEdgesVisualStateUntilMouseUp = false;

            object mouseUpFullRowItem = GetDataGridItemByFullRowPoint(e.GetPosition(this));

            if (!selectedItemsGraphSyncPendingUntilMouseUp
                && dataGridMouseDownFullRowItem != null
                && dataGridMouseDownFullRowItem == mouseUpFullRowItem)
            {
                ToggleDataGridSelectedItem(dataGridMouseDownFullRowItem);
                dataGridMouseDownFullRowItem = null;
                SyncSelectedItemsToSelectedEdges();
                e.Handled = true;
                return;
            }

            dataGridMouseDownFullRowItem = null;

            if (!selectedItemsGraphSyncPendingUntilMouseUp)
                return;

            selectedItemsGraphSyncPendingUntilMouseUp = false;
            SyncSelectedItemsToSelectedEdges();
        }

        private void OnDataGridPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dataGridDndStartPoint = e.GetPosition(this);
            dataGridDndStartPointInDataGrid = e.GetPosition(ThisDataGrid);
            dataGridMouseDownFullRowItem = GetDataGridItemByFullRowPoint(dataGridDndStartPoint);
            dataGridDndHasButtonBeenDown = true;
            isDataGridDndDragging = false;

            MinusZero.Instance.IsGUIDragging = false;

        }

        private void OnDataGridPreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!dataGridDndHasButtonBeenDown || isDataGridDndDragging)
                return;

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (WpfUtil.IsMouseOverScrollbar(sender, dataGridDndStartPointInDataGrid))
                return;

            Point mousePosition = e.GetPosition(this);
            Vector diff = dataGridDndStartPoint - mousePosition;

            if (Math.Abs(diff.X) <= Dnd.MinimumHorizontalDragDistance
                && Math.Abs(diff.Y) <= Dnd.MinimumVerticalDragDistance)
                return;

            isDataGridDndDragging = true;
            selectedItemsGraphSyncPendingUntilMouseUp = false;
            preserveSelectedEdgesVisualStateUntilMouseUp = true;

            IVertex dndVertex = CreateDataGridDndVertex();

            if (dndVertex.Count() > 0)
            {
                dndVertex.AddExternalReference();

                System.Windows.DataObject dragData = new System.Windows.DataObject("Vertex", dndVertex);
                dragData.SetData("DragSource", this);

                Dnd.DoDragDrop(this, dragData);

                e.Handled = true;
            }

            isDataGridDndDragging = false;
            dataGridDndHasButtonBeenDown = false;
            dataGridMouseDownFullRowItem = null;
        }

        private object GetDataGridItemByFullRowPoint(Point point)
        {
            if (ThisDataGrid == null || ThisDataGrid.Items == null)
                return null;

            foreach (object item in ThisDataGrid.Items)
            {
                System.Windows.Controls.DataGridRow row = ThisDataGrid.ItemContainerGenerator.ContainerFromItem(item) as System.Windows.Controls.DataGridRow;

                if (row == null || !row.IsVisible)
                    continue;

                Point pointInRow = TranslatePoint(point, row);

                if (pointInRow.Y >= 0
                    && pointInRow.Y <= row.ActualHeight
                    && pointInRow.X >= 0
                    && pointInRow.X <= ThisDataGrid.ActualWidth)
                    return item;
            }

            return null;
        }

        private void ToggleDataGridSelectedItem(object item)
        {
            if (item == null)
                return;

            TurnOffSelectedVerticesUpdate = true;

            bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (isCtrl)
            {
                if (ThisDataGrid.SelectedItems.Contains(item))
                    ThisDataGrid.SelectedItems.Remove(item);
                else
                    ThisDataGrid.SelectedItems.Add(item);
            }
            else
            {
                ThisDataGrid.SelectedItems.Clear();
                ThisDataGrid.SelectedItems.Add(item);
            }

            TurnOffSelectedVerticesUpdate = false;
        }

        private IVertex CreateDataGridDndVertex()
        {
            IVertex dndVertex = MinusZero.Instance.CreateTempVertex();
            IVertex selectedEdges = Vertex.GetAll(false, @"SelectedEdges:\{$Is:Edge}");

            if (selectedEdges != null && selectedEdges.Count() > 0)
            {
                foreach (IEdge selectedEdge in selectedEdges)
                    dndVertex.AddEdge(null, selectedEdge.To);

                return dndVertex;
            }

            IVertex edgeByPoint = GetEdgeByPoint(dataGridDndStartPoint);

            if (edgeByPoint != null)
                dndVertex.AddEdge(null, edgeByPoint);

            return dndVertex;
        }

        private void SyncSelectedItemsToSelectedEdges()
        {
            TurnOffSelectedItemsUpdate = true;

            IVertex sv = Vertex.Get(false, "SelectedEdges:");

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            //////////////////////////////////////// 

            UnselectAllSelectedEdges();

            IVertex baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

            foreach (IEdge ee in ThisDataGrid.SelectedItems)
                EdgeHelper.AddEdgeVertex(sv, baseVertex, ee.Meta, ee.To); // becouse of possible FilterQuery
                                                                    // Edge.AddEdge(sv, ee);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 

            TurnOffSelectedItemsUpdate = false;
            RefreshSelectedRowsVisualState();
            RefreshKeyboardHighlightAfterItemsChanged();

        }

        bool ShowMeta;
        bool ShowIcons;

        protected virtual void CreateView(){
            ThisDataGrid.Columns.Clear();
            UpdateHeaderVisibility();

            DataGridTextColumn metaColumn = new DataGridTextColumn();

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "ShowMeta:"), "False"))
                ShowMeta = false;
            else
                ShowMeta = true;

            ShowIcons = GraphUtil.GetBooleanValueOrFalse(Vertex.Get(false, "ShowIcons:"));

            if (ShowIcons)
                ThisDataGrid.Columns.Add(CreateIconColumn());

            if (ShowMeta)
            {
                System.Windows.Data.Binding mb = new System.Windows.Data.Binding("Meta.Value");
                mb.Mode = BindingMode.OneWay;
                metaColumn.Binding = mb;
                metaColumn.Header = CreateColumnHeader("Meta");

                if (GeneralUtil.CompareStrings(Vertex.Get(false, "IsMetaRightAlign:").Value, "True"))
                    metaColumn.CellStyle = CreateHighlightedCellStyle("0ListMetaColumnRight");
                else
                    metaColumn.CellStyle = CreateHighlightedCellStyle("0ListMetaColumnLeft");

                //metaColumn.Foreground = (Brush)FindResource("0GrayBrush");            

                ThisDataGrid.Columns.Add(metaColumn);
            }
            //DataGridTextColumn valueColumn = new DataGridTextColumn();
            //valueColumn.Binding = new Binding("To.Value");
            
            DataGridTemplateColumn valueColumn = new DataGridTemplateColumn();

            valueColumn.CellStyle = CreateHighlightedCellStyle("0ListValueColumn");
            valueColumn.Header = CreateColumnHeader("To");

            //
            // CELL TEMPLATE
            //

            if (GeneralUtil.CompareStrings(Vertex.Get(false, "IsAllVisualisersEdit:").Value, "True"))
            {
                valueColumn.CellTemplate = new DataTemplate();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(VisualiserEditWrapper));
                factory.SetBinding(VisualiserEditWrapper.BaseEdgeProperty, new System.Windows.Data.Binding(""));                
                valueColumn.CellTemplate.VisualTree = factory;
            }
            else
            {
                valueColumn.CellTemplate = new DataTemplate();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(VisualiserViewWrapper));
                factory.SetValue(VisualiserViewWrapper.FontWeightProperty, FontWeights.Bold);
                factory.SetBinding(VisualiserViewWrapper.BaseEdgeProperty, new System.Windows.Data.Binding(""));
                valueColumn.CellTemplate.VisualTree = factory;
            }

            //
            // EDIT TEMPLATE
            //
            valueColumn.CellEditingTemplate = new DataTemplate();
            FrameworkElementFactory EditFactory = new FrameworkElementFactory(typeof(VisualiserEditWrapper));
            EditFactory.SetBinding(VisualiserEditWrapper.BaseEdgeProperty, new  System.Windows.Data.Binding(""));
            valueColumn.CellEditingTemplate.VisualTree = EditFactory;


            ThisDataGrid.Columns.Add(valueColumn); 
        }

        private DataGridTemplateColumn CreateIconColumn()
        {
            DataGridTemplateColumn iconColumn = new DataGridTemplateColumn();
            iconColumn.CellStyle = CreateHighlightedCellStyle("0ListValueColumn");
            iconColumn.CellTemplate = new DataTemplate();
            iconColumn.Header = CreateColumnHeader("");

            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(System.Windows.Controls.Image));
            factory.SetValue(System.Windows.Controls.Image.WidthProperty, WpfUtil.IconSize);
            factory.SetValue(System.Windows.Controls.Image.HeightProperty, WpfUtil.IconSize);
            factory.SetValue(System.Windows.Controls.Image.MarginProperty, new Thickness(0, 0, 3, 0));
            factory.SetValue(System.Windows.Controls.Image.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.SetValue(System.Windows.Controls.Image.StretchProperty, Stretch.Uniform);
            factory.SetValue(System.Windows.Controls.Image.SnapsToDevicePixelsProperty, true);
            factory.SetValue(System.Windows.Controls.Image.UseLayoutRoundingProperty, true);
            factory.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.Fant);

            System.Windows.Data.Binding sourceBinding = new System.Windows.Data.Binding("");
            sourceBinding.Mode = BindingMode.OneWay;
            sourceBinding.Converter = EdgeIconSourceConverter;
            factory.SetBinding(System.Windows.Controls.Image.SourceProperty, sourceBinding);

            System.Windows.Data.Binding visibilityBinding = new System.Windows.Data.Binding("");
            visibilityBinding.Mode = BindingMode.OneWay;
            visibilityBinding.Converter = EdgeIconVisibilityConverter;
            factory.SetBinding(System.Windows.Controls.Image.VisibilityProperty, visibilityBinding);

            iconColumn.CellTemplate.VisualTree = factory;

            return iconColumn;
        }

        private void UpdateHeaderVisibility()
        {
            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "ShowHeader:"), "False"))
                ThisDataGrid.HeadersVisibility = DataGridHeadersVisibility.None;
            else
                ThisDataGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
        }

        private Style CreateColumnHeaderStyle(bool drawHorizontalHeaderLine, bool drawVerticalHeaderLine)
        {
            Style headerStyle = new Style(typeof(DataGridColumnHeader));
            Thickness headerBorderThickness = new Thickness(0, 0, 1, drawHorizontalHeaderLine ? 1 : 0);
            Brush verticalGripBrush = drawVerticalHeaderLine
                ? (Brush)FindResource("0ForegroundBrush")
                : (Brush)FindResource("0VeryVeryVeryLightForegroundBrush");

            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, (Brush)FindResource("0VeryLightForegroundBrush")));
            headerStyle.Setters.Add(new Setter(DataGridColumnHeader.SeparatorBrushProperty, verticalGripBrush));
            headerStyle.Setters.Add(new Setter(DataGridColumnHeader.SeparatorVisibilityProperty, Visibility.Visible));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Control.BorderBrushProperty, verticalGripBrush));
            headerStyle.Setters.Add(new Setter(System.Windows.Controls.Control.BorderThicknessProperty, headerBorderThickness));

            return headerStyle;
        }

        private Style CreateHighlightedRowStyle()
        {
            Style rowStyle = new Style(typeof(DataGridRow));

            rowStyle.Setters.Add(new Setter(System.Windows.Controls.Control.BackgroundProperty, (Brush)FindResource("0BackgroundBrush")));
            rowStyle.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, (Brush)FindResource("0ForegroundBrush")));
            rowStyle.Setters.Add(new Setter(System.Windows.Controls.Control.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Stretch));

            Trigger mouseOverTrigger = new Trigger
            {
                Property = DataGridRow.IsMouseOverProperty,
                Value = true
            };
            mouseOverTrigger.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, (Brush)FindResource("0HighlightBrush")));
            rowStyle.Triggers.Add(mouseOverTrigger);

            Trigger selectedTrigger = new Trigger
            {
                Property = DataGridRow.IsSelectedProperty,
                Value = true
            };
            selectedTrigger.Setters.Add(new Setter(System.Windows.Controls.Control.BackgroundProperty, (Brush)FindResource("0SelectionBrush")));
            selectedTrigger.Setters.Add(new Setter(System.Windows.Controls.Control.BorderBrushProperty, (Brush)FindResource("0SelectionBrush")));
            rowStyle.Triggers.Add(selectedTrigger);

            return rowStyle;
        }

        private TextBlock CreateColumnHeader(string text)
        {
            return new TextBlock
            {
                Text = text + " ",
                FontStyle = FontStyles.Italic,
                Foreground = (Brush)FindResource("0VeryLightForegroundBrush"),
                Margin = new Thickness(4, 0, 4, 0)
            };
        }

        private Style CreateHighlightedCellStyle(string baseStyleResourceKey)
        {
            Style baseCellStyle = (Style)FindResource(baseStyleResourceKey);
            Style cellStyle = new Style(typeof(System.Windows.Controls.DataGridCell), baseCellStyle);

            DataTrigger rowMouseOverTrigger = new DataTrigger
            {
                Binding = new System.Windows.Data.Binding("IsMouseOver")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1)
                },
                Value = true
            };
            rowMouseOverTrigger.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, (Brush)FindResource("0HighlightBrush")));
            cellStyle.Triggers.Add(rowMouseOverTrigger);

            Trigger selectedTrigger = new Trigger
            {
                Property = System.Windows.Controls.DataGridCell.IsSelectedProperty,
                Value = true
            };
            selectedTrigger.Setters.Add(new Setter(System.Windows.Controls.Control.BackgroundProperty, Brushes.Transparent));
            selectedTrigger.Setters.Add(new Setter(System.Windows.Controls.Control.BorderBrushProperty, Brushes.Transparent));
            selectedTrigger.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, (Brush)FindResource("0BackgroundBrush")));
            cellStyle.Triggers.Add(selectedTrigger);

            return cellStyle;
        }

        private void OnKeyboardHighlightPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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

            foreach (System.Windows.Controls.DataGridCell cell in FindVisualChildren<System.Windows.Controls.DataGridCell>(row))
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
            {
                IVertex baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

                if (baseVertex != null)
                    EdgeHelper.AddEdgeVertex(selectedEdges, baseVertex, keyboardHighlightedEdge.Meta, keyboardHighlightedEdge.To);
            }

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

            int position = GetKeyboardHighlightRowsInScreenOrder().IndexOf(row);

            if (position < 0)
                return;

            SetKeyboardHighlightPosition(position);
            RaiseKeyboardHighlightActivated();
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
            List<DataGridRow> rows = GetKeyboardHighlightRowsInScreenOrder();

            if (currentHighlightPosition < 0 || currentHighlightPosition >= rows.Count)
                return null;

            return rows[currentHighlightPosition].Item as IEdge;
        }

        private bool IsItemSelectedForVisualState(object item)
        {
            if (!preserveSelectedEdgesVisualStateUntilMouseUp && !isDataGridDndDragging)
                return ThisDataGrid.SelectedItems.Contains(item);

            IEdge itemEdge = item as IEdge;

            if (itemEdge == null)
                return false;

            IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");

            return selectedEdges != null && EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, itemEdge) != null;
        }

        public void ClearKeyboardHighlight()
        {
            DataGridRow row = GetKeyboardHighlightRow();

            if (row == null)
                return;

            row.ClearValue(System.Windows.Controls.Control.BackgroundProperty);
            row.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
            row.ClearValue(System.Windows.Controls.Control.BorderBrushProperty);

            foreach (System.Windows.Controls.DataGridCell cell in FindVisualChildren<System.Windows.Controls.DataGridCell>(row))
            {
                cell.ClearValue(System.Windows.Controls.Control.BackgroundProperty);
                cell.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
                cell.ClearValue(System.Windows.Controls.Control.BorderBrushProperty);
            }

            if (IsItemSelectedForVisualState(row.Item))
                ApplySelectedRowVisualState(row);
        }

        private DataGridRow GetKeyboardHighlightRow()
        {
            List<DataGridRow> rows = GetKeyboardHighlightRowsInScreenOrder();

            if (currentHighlightPosition < 0 || currentHighlightPosition >= rows.Count)
                return null;

            object item = rows[currentHighlightPosition].Item;
            ThisDataGrid.ScrollIntoView(item);
            ThisDataGrid.UpdateLayout();

            return ThisDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
        }

        private int GetKeyboardHighlightItemCount()
        {
            return GetKeyboardHighlightRowsInScreenOrder().Count;
        }

        private List<DataGridRow> GetKeyboardHighlightRowsInScreenOrder()
        {
            List<DataGridRow> rows = new List<DataGridRow>();

            if (ThisDataGrid == null || ThisDataGrid.Items == null)
                return rows;

            ThisDataGrid.UpdateLayout();

            foreach (object item in ThisDataGrid.Items)
            {
                DataGridRow row = ThisDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;

                if (row != null && row.Item is IEdge)
                    rows.Add(row);
            }

            return rows
                .OrderBy(row => row.TranslatePoint(new Point(0, 0), ThisDataGrid).Y)
                .ThenBy(row => row.TranslatePoint(new Point(0, 0), ThisDataGrid).X)
                .ToList();
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

        protected void ResetView()
        {
            CreateView();

            ThisDataGrid.HorizontalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");
            ThisDataGrid.VerticalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");
            bool drawHorizontalHeaderLine = false;
            bool drawVerticalHeaderLine = false;

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "Vertical"))
            {
                ThisDataGrid.BorderThickness = new System.Windows.Thickness(0);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.Vertical;
                drawVerticalHeaderLine = true;
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "Horizontal"))
            {
                ThisDataGrid.BorderThickness = new System.Windows.Thickness(0);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
                drawHorizontalHeaderLine = true;
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "All"))
            {
                ThisDataGrid.BorderThickness = new System.Windows.Thickness(0);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.All;
                drawHorizontalHeaderLine = true;
                drawVerticalHeaderLine = true;
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "AllAndRound"))
            {
                ThisDataGrid.BorderThickness = new System.Windows.Thickness(1);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.All;
                ThisDataGrid.BorderBrush = (Brush)FindResource("0ForegroundBrush");
                drawHorizontalHeaderLine = true;
                drawVerticalHeaderLine = true;
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "Round"))
            {
                ThisDataGrid.BorderThickness = new System.Windows.Thickness(1);
                ThisDataGrid.BorderBrush = (Brush)FindResource("0LightGrayBrush");
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.None; 
            }
            else
            {
                ThisDataGrid.BorderThickness = new System.Windows.Thickness(0);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.None;                
            }

            ThisDataGrid.ColumnHeaderStyle = CreateColumnHeaderStyle(drawHorizontalHeaderLine, drawVerticalHeaderLine);
            RefreshVisualStatesAfterItemsChanged();
        }

        public void ScaleChange()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get(false, "Scale:")))/100;

            if (scale != 1.0)
                this.LayoutTransform = new ScaleTransform(scale, scale);
            else
                this.LayoutTransform = null;
        }        

        public void SelectedVerticesUpdated(){
            if (SelectedEdgesChange != null)
                SelectedEdgesChange();

            if (SelectionProphibited)
                return;

            if (TurnOffSelectedItemsUpdate)
                return;

            if (preserveSelectedEdgesVisualStateUntilMouseUp || isDataGridDndDragging)
            {
                RefreshSelectedRowsVisualState();
                return;
            }

            TurnOffSelectedVerticesUpdate = true;

            ThisDataGrid.SelectedItems.Clear();

            IVertex b = Vertex.Get(false, @"BaseEdge:\To:");

            if (b != null)
            foreach(IEdge e in Vertex.Get(false, "SelectedEdges:")){
                if (e.Meta.Value.ToString() != "Edge") // can have event trigger here
                    continue;

                IEdge ee = GraphUtil.FindEdgeByToVertex_fromVertex(b, e.To.Get(false, "To:"));
                if (ee != null)
                    ThisDataGrid.SelectedItems.Add(ee);
            }

            TurnOffSelectedVerticesUpdate = false;
        }

        protected virtual void SetVertexDefaultValues(){
            Vertex.Get(false, "IsMetaRightAlign:").Value = "False";
            Vertex.Get(false, "IsAllVisualisersEdit:").Value = "False";
            Vertex.Get(false, "ShowMeta:").Value = "True";
            Vertex.Get(false, "ShowIcons:").Value = "True";
            Vertex.Get(false, "ShowHeader:").Value = "True";
            Vertex.Get(false, "Scale:").Value = 100;

            GraphUtil.ReplaceEdge(Vertex, "GridStyle", MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\GridStyleEnum\None"));
        }

        protected virtual void AddFooter() { }       

        public virtual void BaseEdgeToUpdated(){            
            IVertex _bas = Vertex.Get(false, @"BaseEdge:\To:");

            IEnumerable ItemsSourceValue = null;

            if (_bas != null)
            {
                ResetView();

                if (Vertex.Get(false, @"FilterQuery:") != null && Vertex.Get(false, @"FilterQuery:").Value != null)
                {
                    IVertex data = VertexOperations.DoFilter(_bas, Vertex.Get(false, @"FilterQuery:"));

                    if (data != null)
                        ItemsSourceValue = data.ToList();                    
                }
                else
                    ItemsSourceValue = _bas.ToList(); // if there is no .ToList DataGrid can not edit
                

                IList<IEdge> ItemsSourceValueNoHide = new List<IEdge>();

                foreach (IEdge e in ItemsSourceValue)
                    if (GraphUtil.GetQueryOutCount(e.Meta, "$Hide", null) == 0)
                        ItemsSourceValueNoHide.Add(e);

                ThisDataGrid.ItemsSource = ItemsSourceValueNoHide;             
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
                System.Windows.Controls.DataGridRow row = GetDataGridRow(item);

                if (row == null)
                    continue;

                ClearSelectedRowVisualState(row);

                if (IsItemSelectedForVisualState(item))
                    ApplySelectedRowVisualState(row);
            }
        }

        private System.Windows.Controls.DataGridRow GetDataGridRow(object item)
        {
            ThisDataGrid.ScrollIntoView(item);
            ThisDataGrid.UpdateLayout();

            return ThisDataGrid.ItemContainerGenerator.ContainerFromItem(item) as System.Windows.Controls.DataGridRow;
        }

        private void ApplySelectedRowVisualState(System.Windows.Controls.DataGridRow row)
        {
            row.Background = (Brush)FindResource("0SelectionBrush");
            row.Foreground = (Brush)FindResource("0BackgroundBrush");
            row.BorderBrush = (Brush)FindResource("0SelectionBrush");

            foreach (System.Windows.Controls.DataGridCell cell in FindVisualChildren<System.Windows.Controls.DataGridCell>(row))
            {
                cell.Background = Brushes.Transparent;
                cell.Foreground = (Brush)FindResource("0BackgroundBrush");
                cell.BorderBrush = Brushes.Transparent;
            }
        }

        private void ClearSelectedRowVisualState(System.Windows.Controls.DataGridRow row)
        {
            row.ClearValue(System.Windows.Controls.Control.BackgroundProperty);
            row.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
            row.ClearValue(System.Windows.Controls.Control.BorderBrushProperty);

            foreach (System.Windows.Controls.DataGridCell cell in FindVisualChildren<System.Windows.Controls.DataGridCell>(row))
            {
                cell.ClearValue(System.Windows.Controls.Control.BackgroundProperty);
                cell.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
                cell.ClearValue(System.Windows.Controls.Control.BorderBrushProperty);
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
            double headerActualHeight = headersPresenter.ActualHeight;

            if (point.Y <= headerActualHeight) // if header
                return null;

            foreach (var item in ThisDataGrid.Items)
            {
                var row = ThisDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;

                if (row != null)
                {
                    DataGridRow roww = (DataGridRow)row;                    
                   
                    if (VisualTreeHelper.HitTest(roww, TranslatePoint(point, roww)) != null)
                        {
                            if (point.X >= ThisDataGrid.Columns.First().ActualWidth && roww.IsEditing)
                                return null;

                            IVertex v = MinusZero.Instance.CreateTempVertex();
                            EdgeHelper.AddEdgeVertexEdges(v, (IEdge)roww.Item);
                            return v;
                        }
                }
            }

            // DO WANT THIS FEATURE ?
            //
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

        private class ListVisualiserEdgeIconSourceConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return IconServer.GetIconByEdge(value as IEdge);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private class ListVisualiserEdgeIconVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return IconServer.GetIconByEdge(value as IEdge) == null ? Visibility.Collapsed : Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }
}

