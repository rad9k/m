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
    public class ListVisualiser : Grid,  IListVisualiser, ITypedEdge, IKeyboardHighlight, IOwnScrolling
    {
        private static readonly IValueConverter EdgeIconSourceConverter = new ListVisualiserEdgeIconSourceConverter();
        private static readonly IValueConverter EdgeIconVisibilityConverter = new ListVisualiserEdgeIconVisibilityConverter();

        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        public bool SelectionProhibited { get; set; }

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
        private bool pendingMouseDownIsCtrl;
        private bool pendingWasInSelectionAtMouseDown;
        private bool suppressNextMouseUpSelection;
        private bool isSyncingSelectedItemsFromGraph;

        private int currentHighlightPosition = -1;
        private bool isBeforeFirstPosition;
        private bool isAfterLastPosition;
        private IEdge currentKeyboardHighlightedEdge;
        private object keyboardHighlightedRowItem;
        private DataGridColumn editableValueColumn;
        private bool skipNextMouseUpSelectionForEditGesture;

        static string[] _MetaTriggeringUpdateVertex = new string[] { };
        public virtual string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { "IsMetaRightAlign", "IsAllVisualisersEdit", "ShowMeta", "ShowIcons", "GridStyle", "FilterQuery", "ShowHeader" };
        public virtual string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public virtual void ViewAttributesUpdated() { ResetView(); }

        protected virtual bool IsDataGridRowVirtualizationEnabled { get { return true; } }

        protected virtual VirtualizationMode DataGridVirtualizationMode { get { return VirtualizationMode.Standard; } }

        protected virtual bool IsDataGridColumnVirtualizationEnabled { get { return false; } }

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

            RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Star)
            });

            ThisDataGrid = new System.Windows.Controls.DataGrid();
            Children.Add(ThisDataGrid);
            Grid.SetRow(ThisDataGrid, 0);

            FrameworkElement footer = CreateFooter();

            if (footer != null)
            {
                RowDefinitions.Add(new RowDefinition
                {
                    Height = GridLength.Auto
                });
                Children.Add(footer);
                Grid.SetRow(footer, 1);
            }

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
            ConfigureDataGridVirtualization();
            MinusZero mz = MinusZero.Instance;

            if (mz != null && mz.IsInitialized)
            {
                PlatformClassInitialize(baseEdgeVertex);

                SetVertexDefaultValues();

                ResetView();

                ThisDataGrid.SelectionChanged += _OnSelectionChanged;
                ThisDataGrid.PreviewMouseLeftButtonDown += OnDataGridPreviewMouseLeftButtonDown;
                ThisDataGrid.PreviewMouseRightButtonDown += OnDataGridPreviewMouseRightButtonDown;
                ThisDataGrid.PreviewMouseMove += OnDataGridPreviewMouseMove;
                ThisDataGrid.PreviewMouseLeftButtonUp += OnDataGridPreviewMouseLeftButtonUp;
                ThisDataGrid.PreviewKeyDown += OnKeyboardHighlightPreviewKeyDown;
                ThisDataGrid.MouseDoubleClick += OnKeyboardHighlightMouseDoubleClick;
                ThisDataGrid.LoadingRow += OnDataGridLoadingRow;
                ThisDataGrid.BeginningEdit += OnDataGridBeginningEdit;
                ThisDataGrid.CellEditEnding += OnDataGridCellEditEnding;
            }
        }

        private DataGridRow GetDataGridRowFromPoint(Point point)
        {
            object item = GetDataGridItemByFullRowPoint(point);

            if (item == null)
                return null;

            return TryGetDataGridRow(item);
        }

        private void OnDataGridBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            ClearKeyboardHighlight();
        }

        private void OnDataGridCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel)
                ClearKeyboardHighlight();
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

        public bool IsVertexCommanderKeyboardHighlightEnabled { get; set; }

        public event EventHandler GoneBeforeFirstPosition;

        public event EventHandler GoneAfterLastPosition;

        public event EventHandler KeyboardHighlightEnterPressed;

        public event EventHandler KeyboardHighlightActivated;

        public IEdge KeyboardHighlightedEdge
        {
            get { return currentKeyboardHighlightedEdge; }
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
            if (SelectionProhibited)
            {
                TurnOffSelectedVerticesUpdate = true;
                ThisDataGrid.SelectedItems.Clear();
                TurnOffSelectedVerticesUpdate = false;
                return;
            }

            if (isSyncingSelectedItemsFromGraph)
                return;

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
            DataGridRow rowAtPoint = GetDataGridRowFromPoint(e.GetPosition(this));

            dataGridDndHasButtonBeenDown = false;
            preserveSelectedEdgesVisualStateUntilMouseUp = false;
            selectedItemsGraphSyncPendingUntilMouseUp = false;

            if (suppressNextMouseUpSelection)
            {
                ClearPendingMouseGesture();
                return;
            }

            if (skipNextMouseUpSelectionForEditGesture)
            {
                skipNextMouseUpSelectionForEditGesture = false;
                ClearPendingMouseGesture();
                return;
            }

            if (rowAtPoint != null && rowAtPoint.IsEditing)
            {
                ClearPendingMouseGesture();
                return;
            }

            object mouseUpFullRowItem = GetDataGridItemByFullRowPoint(e.GetPosition(this));

            if (SelectionProhibited
                || dataGridMouseDownFullRowItem == null
                || dataGridMouseDownFullRowItem != mouseUpFullRowItem)
            {
                ClearPendingMouseGesture();
                return;
            }

            IEdge clickedEdge = dataGridMouseDownFullRowItem as IEdge;

            if (clickedEdge != null)
            {
                PendingEdgeMouseGesture pendingGesture = new PendingEdgeMouseGesture
                {
                    ClickedEdge = clickedEdge,
                    IsCtrl = pendingMouseDownIsCtrl,
                    WasInSelectionAtMouseDown = pendingWasInSelectionAtMouseDown
                };

                SelectedEdgesInteractionHelper.ApplyForClick(Vertex, pendingGesture);
                SelectedVerticesUpdated();
                RefreshSelectedRowsVisualState();
            }

            ClearPendingMouseGesture();
            e.Handled = true;
        }

        private void OnDataGridPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dataGridDndStartPoint = e.GetPosition(this);
            dataGridDndStartPointInDataGrid = e.GetPosition(ThisDataGrid);
            dataGridMouseDownFullRowItem = GetDataGridItemByFullRowPoint(dataGridDndStartPoint);
            dataGridDndHasButtonBeenDown = true;
            isDataGridDndDragging = false;
            suppressNextMouseUpSelection = false;
            preserveSelectedEdgesVisualStateUntilMouseUp = true;

            IEdge clickedEdge = dataGridMouseDownFullRowItem as IEdge;

            pendingMouseDownIsCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            pendingWasInSelectionAtMouseDown = clickedEdge != null
                && SelectedEdgesInteractionHelper.WasEdgeInSelectedEdges(Vertex, clickedEdge);

            if (e.ClickCount >= 2)
                skipNextMouseUpSelectionForEditGesture = true;

            MinusZero.Instance.IsGUIDragging = false;
        }

        private void OnDataGridPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            object rowItem = GetDataGridItemByFullRowPoint(e.GetPosition(this));
            IEdge clickedEdge = rowItem as IEdge;

            if (clickedEdge == null || SelectionProhibited)
                return;

            SelectedEdgesInteractionHelper.ApplyForContextMenu(Vertex, clickedEdge);
            SelectedVerticesUpdated();
            RefreshSelectedRowsVisualState();
        }

        protected void ClearPendingMouseGesture()
        {
            dataGridMouseDownFullRowItem = null;
            pendingMouseDownIsCtrl = false;
            pendingWasInSelectionAtMouseDown = false;
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

            IEdge clickedEdge = dataGridMouseDownFullRowItem as IEdge;

            if (clickedEdge == null)
            {
                IVertex edgeByPoint = GetEdgeByPoint(dataGridDndStartPoint);

                if (edgeByPoint != null)
                    clickedEdge = EdgeHelper.GetIEdgeByEdgeVertex(edgeByPoint);
            }

            if (clickedEdge != null)
            {
                bool isCtrl = pendingMouseDownIsCtrl;
                bool wasInSelectionAtMouseDown = pendingWasInSelectionAtMouseDown;

                if (dataGridMouseDownFullRowItem == null || clickedEdge != dataGridMouseDownFullRowItem)
                {
                    isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                    wasInSelectionAtMouseDown = SelectedEdgesInteractionHelper.WasEdgeInSelectedEdges(Vertex, clickedEdge);
                }

                PendingEdgeMouseGesture pendingGesture = new PendingEdgeMouseGesture
                {
                    ClickedEdge = clickedEdge,
                    IsCtrl = isCtrl,
                    WasInSelectionAtMouseDown = wasInSelectionAtMouseDown
                };

                preserveSelectedEdgesVisualStateUntilMouseUp = false;
                SelectedEdgesInteractionHelper.ApplyForDrag(Vertex, pendingGesture);
                SelectedVerticesUpdated();
                RefreshSelectedRowsVisualState();
            }

            suppressNextMouseUpSelection = true;

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
            ClearPendingMouseGesture();
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

        private IVertex CreateDataGridDndVertex()
        {
            IVertex fallbackEdgeVertex = GetEdgeByPoint(dataGridDndStartPoint);

            return SelectedEdgesInteractionHelper.BuildDndVertexFromSelectedEdges(
                Vertex,
                fallbackEdgeVertex);
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
        private bool isViewInitialized;

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

            editableValueColumn = valueColumn;
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

            MultiTrigger selectedMouseOverTrigger = new MultiTrigger();
            selectedMouseOverTrigger.Conditions.Add(new Condition(DataGridRow.IsSelectedProperty, true));
            selectedMouseOverTrigger.Conditions.Add(new Condition(DataGridRow.IsMouseOverProperty, true));
            selectedMouseOverTrigger.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, (Brush)FindResource("0HighlightBrush")));
            rowStyle.Triggers.Add(selectedMouseOverTrigger);

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

        protected Style CreateHighlightedCellStyle(string baseStyleResourceKey)
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

            MultiDataTrigger selectedRowMouseOverTrigger = new MultiDataTrigger();
            selectedRowMouseOverTrigger.Conditions.Add(new Condition
            {
                Binding = new System.Windows.Data.Binding("IsSelected")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1)
                },
                Value = true
            });
            selectedRowMouseOverTrigger.Conditions.Add(new Condition
            {
                Binding = new System.Windows.Data.Binding("IsMouseOver")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1)
                },
                Value = true
            });
            selectedRowMouseOverTrigger.Setters.Add(new Setter(System.Windows.Controls.Control.ForegroundProperty, (Brush)FindResource("0HighlightBrush")));
            cellStyle.Triggers.Add(selectedRowMouseOverTrigger);

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

        private void OnDataGridLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseEnter -= OnDataGridRowMouseEnter;
            e.Row.MouseLeave -= OnDataGridRowMouseLeave;
            e.Row.MouseEnter += OnDataGridRowMouseEnter;
            e.Row.MouseLeave += OnDataGridRowMouseLeave;

            SyncRowVisualStateOnLoad(e.Row);
        }

        private void SyncRowVisualStateOnLoad(DataGridRow row)
        {
            if (row == null)
                return;

            if (IsRowKeyboardHighlighted(row))
                return;

            ClearSelectedRowVisualState(row);

            if (IsItemSelectedForVisualState(row.Item))
                ApplySelectedRowVisualState(row);
            else
                UpdateRowForegroundVisualState(row);
        }

        private void OnDataGridRowMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UpdateRowForegroundVisualState(sender as DataGridRow);
        }

        private void OnDataGridRowMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UpdateRowForegroundVisualState(sender as DataGridRow);
        }

        private bool IsRowKeyboardHighlighted(DataGridRow row)
        {
            if (row == null || currentHighlightPosition < 0 || keyboardHighlightedRowItem == null)
                return false;

            return row.Item == keyboardHighlightedRowItem;
        }

        private void UpdateRowForegroundVisualState(DataGridRow row)
        {
            if (row == null || IsRowKeyboardHighlighted(row))
                return;

            bool isSelected = IsItemSelectedForVisualState(row.Item);
            Brush foregroundBrush = ResolveRowForegroundBrush(isSelected, row.IsMouseOver);

            ApplyForegroundToRow(row, foregroundBrush);
        }

        private Brush ResolveRowForegroundBrush(bool isSelected, bool isMouseOver)
        {
            if (isSelected && isMouseOver)
                return (Brush)FindResource("0HighlightBrush");

            if (isSelected)
                return (Brush)FindResource("0BackgroundBrush");

            if (isMouseOver)
                return (Brush)FindResource("0HighlightBrush");

            return null;
        }

        private void ApplyForegroundToRow(DataGridRow row, Brush foregroundBrush)
        {
            if (foregroundBrush != null)
            {
                row.Foreground = foregroundBrush;

                foreach (System.Windows.Controls.DataGridCell cell in FindVisualChildren<System.Windows.Controls.DataGridCell>(row))
                    cell.Foreground = foregroundBrush;
            }
            else
            {
                row.ClearValue(System.Windows.Controls.Control.ForegroundProperty);

                foreach (System.Windows.Controls.DataGridCell cell in FindVisualChildren<System.Windows.Controls.DataGridCell>(row))
                    cell.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
            }
        }

        private void OnKeyboardHighlightPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

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
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

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
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

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
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

            DataGridRow row = GetKeyboardHighlightRow();

            if (row == null)
                return;

            keyboardHighlightedRowItem = row.Item;

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
            if (!IsVertexCommanderKeyboardHighlightEnabled || SelectionProhibited)
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
            if (!IsVertexCommanderKeyboardHighlightEnabled)
                return;

            DataGridRow row = GetDataGridRowFromEventSource(e.OriginalSource as DependencyObject);

            if (IsAnyDataGridRowEditing())
                return;

            if (IsEditableValueColumnClick(e.OriginalSource as DependencyObject))
                return;

            if (row == null)
                return;

            int position = GetKeyboardHighlightEdges().IndexOf(row.Item as IEdge);

            if (position < 0)
                return;

            SetKeyboardHighlightPosition(position);
            RaiseKeyboardHighlightEnterPressed();
            e.Handled = true;
        }

        private System.Windows.Controls.DataGridCell FindAncestorDataGridCell(DependencyObject source)
        {
            while (source != null)
            {
                System.Windows.Controls.DataGridCell cell = source as System.Windows.Controls.DataGridCell;

                if (cell != null)
                    return cell;

                source = VisualTreeHelper.GetParent(source);
            }

            return null;
        }

        private bool IsEditableValueColumnClick(DependencyObject source)
        {
            if (editableValueColumn == null || source == null)
                return false;

            System.Windows.Controls.DataGridCell cell = FindAncestorDataGridCell(source);

            if (cell != null)
                return cell.Column == editableValueColumn;

            while (source != null)
            {
                if (source is VisualiserEditWrapper
                    || source is VisualiserViewWrapper
                    || source is StringVisualiser
                    || source is StringViewVisualiser)
                    return true;

                source = VisualTreeHelper.GetParent(source);
            }

            return false;
        }

        private bool IsAnyDataGridRowEditing()
        {
            if (ThisDataGrid?.Items == null)
                return false;

            foreach (object item in ThisDataGrid.Items)
            {
                DataGridRow row = TryGetDataGridRow(item);

                if (row != null && row.IsEditing)
                    return true;
            }

            return false;
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
            ClearKeyboardHighlightVisualsOnRow(FindKeyboardHighlightedRow());

            currentHighlightPosition = -1;
            isBeforeFirstPosition = false;
            isAfterLastPosition = false;
            currentKeyboardHighlightedEdge = null;
            keyboardHighlightedRowItem = null;
        }

        private DataGridRow FindKeyboardHighlightedRow()
        {
            if (keyboardHighlightedRowItem != null)
            {
                DataGridRow rowByItem = TryGetDataGridRow(keyboardHighlightedRowItem);

                if (rowByItem != null)
                    return rowByItem;
            }

            return GetKeyboardHighlightRow();
        }

        private void ClearKeyboardHighlightVisualsOnRow(DataGridRow row)
        {
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
            else
                UpdateRowForegroundVisualState(row);
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
            isViewInitialized = true;
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

            if (SelectionProhibited)
                return;

            if (TurnOffSelectedItemsUpdate)
                return;

            if (preserveSelectedEdgesVisualStateUntilMouseUp || isDataGridDndDragging)
            {
                RefreshSelectedRowsVisualState();
                return;
            }

            isSyncingSelectedItemsFromGraph = true;

            try
            {
                TurnOffSelectedVerticesUpdate = true;

                ThisDataGrid.SelectedItems.Clear();

                IVertex selectedEdges = Vertex.Get(false, "SelectedEdges:");

                if (selectedEdges != null)
                {
                    foreach (object item in ThisDataGrid.Items)
                    {
                        IEdge itemEdge = item as IEdge;

                        if (itemEdge == null)
                            continue;

                        if (EdgeHelper.FindIEdgeVertexByIEdge(selectedEdges, itemEdge) != null)
                            ThisDataGrid.SelectedItems.Add(item);
                    }
                }

                TurnOffSelectedVerticesUpdate = false;
                RefreshSelectedRowsVisualState();
            }
            finally
            {
                isSyncingSelectedItemsFromGraph = false;
            }
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

        protected virtual FrameworkElement CreateFooter() { return null; }

        public virtual void BaseEdgeToUpdated(){            
            UnselectAllSelectedEdges();
            ClearPendingMouseGesture();

            IVertex _bas = Vertex.Get(false, @"BaseEdge:\To:");

            IEnumerable ItemsSourceValue = null;

            if (_bas != null)
            {
                if (!isViewInitialized)
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

                SetDataGridItemsSourceWithDiagnostics(ItemsSourceValueNoHide, "ListVisualiser.BaseEdgeToUpdated");
                RefreshVisualStatesAfterItemsChanged();
            }
        }

        protected void SetDataGridItemsSourceWithDiagnostics(IEnumerable itemsSource, string reason)
        {
            ThisDataGrid.ItemsSource = itemsSource;
        }

        private void ConfigureDataGridVirtualization()
        {
            VirtualizingStackPanel.SetIsVirtualizing(ThisDataGrid, IsDataGridRowVirtualizationEnabled);
            VirtualizingStackPanel.SetVirtualizationMode(ThisDataGrid, DataGridVirtualizationMode);
            ScrollViewer.SetCanContentScroll(ThisDataGrid, IsDataGridRowVirtualizationEnabled);
            ThisDataGrid.EnableRowVirtualization = IsDataGridRowVirtualizationEnabled;
            ThisDataGrid.EnableColumnVirtualization = IsDataGridColumnVirtualizationEnabled;
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
                System.Windows.Controls.DataGridRow row = TryGetDataGridRow(item);

                if (row == null)
                    continue;

                if (IsRowKeyboardHighlighted(row))
                    continue;

                ClearSelectedRowVisualState(row);

                if (IsItemSelectedForVisualState(item))
                    ApplySelectedRowVisualState(row);
            }

            if (currentHighlightPosition >= 0)
                RefreshKeyboardHighlightAfterItemsChanged();
        }

        private System.Windows.Controls.DataGridRow TryGetDataGridRow(object item)
        {
            return ThisDataGrid.ItemContainerGenerator.ContainerFromItem(item) as System.Windows.Controls.DataGridRow;
        }

        private System.Windows.Controls.DataGridRow GetDataGridRow(object item)
        {
            ThisDataGrid.ScrollIntoView(item);
            ThisDataGrid.UpdateLayout();

            return TryGetDataGridRow(item);
        }

        private void ApplySelectedRowVisualState(System.Windows.Controls.DataGridRow row)
        {
            row.Background = (Brush)FindResource("0SelectionBrush");
            row.BorderBrush = (Brush)FindResource("0SelectionBrush");

            foreach (System.Windows.Controls.DataGridCell cell in FindVisualChildren<System.Windows.Controls.DataGridCell>(row))
            {
                cell.Background = Brushes.Transparent;
                cell.BorderBrush = Brushes.Transparent;
            }

            UpdateRowForegroundVisualState(row);
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

