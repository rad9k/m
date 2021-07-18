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

namespace m0.UIWpf.Visualisers
{
    public class ListVisualiser : StackPanel,  IPlatformClass, IDisposable, IHasLocalizableEdges, IHasSelectableEdges
    {
        protected DataGrid ThisDataGrid;

        protected bool TurnOffSelectedItemsUpdate = false;

        protected bool TurnOffSelectedVerticesUpdate = false;

        public void UnselectAllSelectedEdges(){
            IVertex sv = Vertex.Get(false, "SelectedEdges:");

            GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);
        }

        protected void _OnSelectionChanged(object sender, SelectionChangedEventArgs e){
            if (!TurnOffSelectedVerticesUpdate)
            {
                TurnOffSelectedItemsUpdate = true;
                
                IVertex sv = Vertex.Get(false, "SelectedEdges:");

                UnselectAllSelectedEdges();

                IVertex baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

                foreach (IEdge ee in ThisDataGrid.SelectedItems)
                    Edge.AddEdgeVertex(sv, baseVertex, ee.Meta, ee.To); // becouse of possible FilterQuery
                   // Edge.AddEdge(sv, ee);

                TurnOffSelectedItemsUpdate = false;
            }            
        }

        bool ShowMeta;

        protected virtual void CreateView(){
            ThisDataGrid.Columns.Clear();

            DataGridTextColumn metaColumn = new DataGridTextColumn();

            if( GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "ShowHeader:"), "True"))
                metaColumn.Header = "meta";

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "ShowMeta:"), "False"))
                ShowMeta = false;
            else
                ShowMeta = true;

            if (ShowMeta)
            {

                Binding mb = new Binding("Meta.Value");
                mb.Mode = BindingMode.OneWay;
                metaColumn.Binding = mb;

                if (GeneralUtil.CompareStrings(Vertex.Get(false, "IsMetaRightAlign:").Value, "True"))
                    metaColumn.CellStyle = (Style)FindResource("0ListMetaColumnRight");
                else
                    metaColumn.CellStyle = (Style)FindResource("0ListMetaColumnLeft");

                //metaColumn.Foreground = (Brush)FindResource("0GrayBrush");            

                ThisDataGrid.Columns.Add(metaColumn);
            }
            //DataGridTextColumn valueColumn = new DataGridTextColumn();
            //valueColumn.Binding = new Binding("To.Value");
            
            DataGridTemplateColumn valueColumn = new DataGridTemplateColumn();

            valueColumn.CellStyle = (Style)FindResource("0ListValueColumn");

            //
            // CELL TEMPLATE
            //

            if (GeneralUtil.CompareStrings(Vertex.Get(false, "IsAllVisualisersEdit:").Value, "True"))
            {
                valueColumn.CellTemplate = new DataTemplate();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(VisualiserEditWrapper));
                factory.SetBinding(VisualiserEditWrapper.BaseEdgeProperty, new Binding(""));                
                valueColumn.CellTemplate.VisualTree = factory;
            }
            else
            {
                valueColumn.CellTemplate = new DataTemplate();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(VisualiserViewWrapper));
                factory.SetBinding(VisualiserViewWrapper.BaseEdgeProperty, new Binding(""));
                valueColumn.CellTemplate.VisualTree = factory;
            }

            //
            // EDIT TEMPLATE
            //
            valueColumn.CellEditingTemplate = new DataTemplate();
            FrameworkElementFactory EditFactory = new FrameworkElementFactory(typeof(VisualiserEditWrapper));
            EditFactory.SetBinding(VisualiserEditWrapper.BaseEdgeProperty, new Binding(""));
            valueColumn.CellEditingTemplate.VisualTree = EditFactory;

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "ShowHeader:"), "True"))
                 valueColumn.Header = "value";

            ThisDataGrid.Columns.Add(valueColumn); 
        }

        protected void ResetView()
        {
            CreateView();

            ThisDataGrid.HorizontalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");
            ThisDataGrid.VerticalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "Vertical"))
            {
                ThisDataGrid.BorderThickness = new System.Windows.Thickness(0);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.Vertical;                
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "Horizontal"))
            {
                ThisDataGrid.BorderThickness = new System.Windows.Thickness(0);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;                
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "All"))
            {
                ThisDataGrid.BorderThickness = new System.Windows.Thickness(0);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.All;                
            }
            else if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "GridStyle:"), "AllAndRound"))
            {
                ThisDataGrid.BorderThickness = new System.Windows.Thickness(1);
                ThisDataGrid.GridLinesVisibility = DataGridGridLinesVisibility.All;
                ThisDataGrid.BorderBrush = (Brush)FindResource("0ForegroundBrush");
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
        }

        protected void ChangeZoomVisualiserContent()
        {
            double scale = ((double)GraphUtil.GetIntegerValue(Vertex.Get(false, "ZoomVisualiserContent:")))/100;

            if (scale != 1.0)
                this.LayoutTransform = new ScaleTransform(scale, scale);
            else
                this.LayoutTransform = null;
        }        

        protected void SelectedVerticesUpdated(){
            if (TurnOffSelectedItemsUpdate)
                return;

            TurnOffSelectedVerticesUpdate = true;

            ThisDataGrid.SelectedItems.Clear();

            IVertex b=Vertex.Get(false, @"BaseEdge:\To:");

            if(b!=null)
            foreach(IEdge e in Vertex.Get(false, "SelectedEdges:")){
                IEdge ee = GraphUtil.FindEdgeByToVertex(b, e.To.Get(false, "To:"));
                if (ee != null)
                    ThisDataGrid.SelectedItems.Add(ee);
            }

            TurnOffSelectedVerticesUpdate = false;
        }

        protected virtual void SetVertexDefaultValues(){
            Vertex.Get(false, "IsMetaRightAlign:").Value = "False";
            Vertex.Get(false, "IsAllVisualisersEdit:").Value = "False";
            Vertex.Get(false, "ShowMeta:").Value = "True";
            Vertex.Get(false, "ZoomVisualiserContent:").Value = 100;

            GraphUtil.ReplaceEdge(Vertex, "GridStyle", MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\GridStyleEnum\None"));
        }

        protected virtual void AddFooter() { }

        protected virtual void PlatformClassInitialize()
        {
            MinusZero mz = MinusZero.Instance;

            //Vertex = mz.Root.Get(false, @"System\Session\Visualisers").AddVertex(null, "ListVisualiser" + this.GetHashCode());

            Vertex = mz.CreateTempVertex();
            Vertex.Value = "ListVisualiser" + this.GetHashCode();

            ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(false, @"System\Meta\Visualiser\List"));

            ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get(false, "BaseEdge:"), mz.Root.Get(false, @"System\Meta\ZeroTypes\Edge"));
           
        
        }

        
        public ListVisualiser()
        {
            ThisDataGrid = new DataGrid();

            this.Children.Add(ThisDataGrid); 
            
            AddFooter();

            

            ThisDataGrid.AllowDrop = true;

            ThisDataGrid.AutoGenerateColumns = false;

            ThisDataGrid.RowBackground = (Brush)FindResource("0BackgroundBrush");
            ThisDataGrid.Background = (Brush)FindResource("0BackgroundBrush");
            ThisDataGrid.HorizontalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");
            ThisDataGrid.VerticalGridLinesBrush = (Brush)FindResource("0ForegroundBrush");

            ThisDataGrid.HeadersVisibility = DataGridHeadersVisibility.Column;

            ThisDataGrid.SelectedValuePath = "To";
            VirtualizingStackPanel.SetIsVirtualizing(ThisDataGrid, false); 
            MinusZero mz=MinusZero.Instance;

            if (mz != null&&mz.IsInitialized)
            {
                PlatformClassInitialize();

                SetVertexDefaultValues();

                CreateView();

                ThisDataGrid.ContextMenu = new m0ContextMenu(this);

                ThisDataGrid.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
                ThisDataGrid.MouseMove += dndPreviewMouseMove; // !!!!!!!!!!!!!!!!!! otherwise sliders do not work
                ThisDataGrid.Drop += dndDrop;

                ThisDataGrid.MouseEnter += dndMouseEnter;

                ThisDataGrid.SelectionChanged += _OnSelectionChanged;
            }
        }

        protected virtual void UpdateBaseEdge(){
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
                    if (GraphUtil.GetQueryOutCount(e.To, "$Hide", null) == 0)
                        ItemsSourceValueNoHide.Add(e);

                ThisDataGrid.ItemsSource = ItemsSourceValueNoHide;
            }           
        }


        protected virtual void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge"))
                 || (sender == Vertex.Get(false, "BaseEdge:") && e.Type == VertexChangeType.ValueChanged)
                || ((sender == Vertex.Get(false, "BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && ((GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To")))))
                UpdateBaseEdge();

            if (sender == Vertex.Get(false, @"BaseEdge:\To:") && (e.Type == VertexChangeType.EdgeAdded || e.Type == VertexChangeType.EdgeRemoved))
                UpdateBaseEdge();

            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "SelectedEdges")))
                SelectedVerticesUpdated();

            if ((sender == Vertex.Get(false, "SelectedEdges:")) && ((e.Type == VertexChangeType.EdgeAdded)||(e.Type == VertexChangeType.EdgeRemoved)))
                SelectedVerticesUpdated();

            if (sender is IVertex && GraphUtil.FindEdgeByToVertex(Vertex.GetAll(false, @"SelectedEdges:\"), (IVertex)sender) != null)
                SelectedVerticesUpdated();

            if (sender == Vertex.Get(false, "IsMetaRightAlign:") && e.Type == VertexChangeType.ValueChanged) 
                ResetView();

            if (sender == Vertex.Get(false, "IsAllVisualisersEdit:") && e.Type == VertexChangeType.ValueChanged)
                ResetView();

            if (sender == Vertex.Get(false, "ZoomVisualiserContent:") && e.Type == VertexChangeType.ValueChanged)
                ChangeZoomVisualiserContent();

            if (sender == Vertex.Get(false, "FilterQuery:") && e.Type == VertexChangeType.ValueChanged)
                UpdateBaseEdge();
      
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "FilterQuery")))
                UpdateBaseEdge();

            if (sender == Vertex.Get(false, "ShowMeta:") && e.Type == VertexChangeType.ValueChanged)
                ResetView();

            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "ShowMeta")))
                ResetView();

            if (sender == Vertex.Get(false, "ShowHeader:") && e.Type == VertexChangeType.ValueChanged)
                ResetView();

            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "ShowHeader")))
                ResetView();

            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "GridStyle")))
                ResetView();
        }       

        private IVertex _Vertex;

        public IVertex Vertex { 
            get { return _Vertex;}
            set{
                if (_Vertex != null)
                    PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                _Vertex = value;

                PlatformClass.RegisterVertexChangeListeners(this.Vertex, new VertexChange(VertexChange), new string[] { "BaseEdge", "SelectedEdges" });

                UpdateBaseEdge();
            }
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;
                MinusZero mz = MinusZero.Instance;

                //GraphUtil.DeleteEdgeByToVertex(mz.Root.Get(false, @"System\Session\Visualisers"), Vertex);

                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                if (Vertex is IDisposable)
                    ((IDisposable)Vertex).Dispose();
            }
        }

        public IVertex GetEdgeByLocation(Point point)
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
                            Edge.AddEdgeVertexEdges(v, (IEdge)roww.Item);
                            return v;
                        }
                }
            }

            // DO WANT THIS FEATURE ?
            //
            if (GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"User\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "StartAndEnd"))
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

        ///// DRAG AND DROP

        IVertex tempSelectedVertices;

        protected void CopySelectedVerticesToTemp()
        {
            tempSelectedVertices = MinusZero.Instance.CreateTempVertex();

            GraphUtil.CopyEdges(Vertex.Get(false, "SelectedEdges:"), tempSelectedVertices);
        }

        protected void RestoreSelectedVertices()
        {
            IVertex sv = Vertex.Get(false, "SelectedEdges:");

            if (tempSelectedVertices != null)
            {
                GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);

                GraphUtil.CopyEdges(tempSelectedVertices, sv);
            }
        }

       //
        
        Point dndStartPoint;
        bool hasButtonBeenDown;

        private void dndPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dndStartPoint = e.GetPosition(this);
            hasButtonBeenDown = true;

            CopySelectedVerticesToTemp();

            MinusZero.Instance.IsGUIDragging = false;
        }

        bool isDraggin = false;

        private void dndPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            Vector diff = dndStartPoint - mousePos;

            var headersPresenter = WpfUtil.FindVisualChild<DataGridColumnHeadersPresenter>(this);
            double headerActualHeight = headersPresenter.ActualHeight;

            if (mousePos.Y <= headerActualHeight) // if header
            {
                e.Handled = false;
                return;
            }

            if (hasButtonBeenDown && isDraggin==false &&
                !WpfUtil.IsMouseOverScrollbar(sender, dndStartPoint) &&
                (e.LeftButton == MouseButtonState.Pressed) && (
                (Math.Abs(diff.X) > Dnd.MinimumHorizontalDragDistance) ||
                (Math.Abs(diff.Y) > Dnd.MinimumVerticalDragDistance)))
            {
                isDraggin = true;

                RestoreSelectedVertices();

                IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                if (Vertex.Get(false, @"SelectedEdges:\") != null)
                    foreach (IEdge ee in Vertex.GetAll(false, @"SelectedEdges:\"))
                        dndVertex.AddEdge(null, ee.To);
                else
                {
                    IVertex v = GetEdgeByLocation(dndStartPoint);
                    if (v != null)
                        dndVertex.AddEdge(null, v);
                }

                if (dndVertex.Count() > 0)
                {
                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", this);

                    Dnd.DoDragDrop(this, dragData);

                    e.Handled = true;
                }

                isDraggin = false;
            }

           // e.Handled = true;
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            IVertex v = GetEdgeByLocation(e.GetPosition(this));

            if (v == null && GeneralUtil.CompareStrings(MinusZero.Instance.Root.Get(false, @"User\CurrentUser:\Settings:\AllowBlankAreaDragAndDrop:").Value, "OnlyEnd"))
                v = Vertex.Get(false, "BaseEdge:");

            if (v != null)
                Dnd.DoDrop(null, v.Get(false, "To:"), e);

            e.Handled = true;
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }

    }
}

