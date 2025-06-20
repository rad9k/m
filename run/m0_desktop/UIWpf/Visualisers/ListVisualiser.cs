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

namespace m0.UIWpf.Visualisers
{
    public class ListVisualiser : StackPanel,  IListVisualiser, ITypedEdge
    {
        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        protected System.Windows.Controls.DataGrid ThisDataGrid;

        protected bool TurnOffSelectedItemsUpdate = false;

        protected bool TurnOffSelectedVerticesUpdate = false;

        static string[] _MetaTriggeringUpdateVertex = new string[] { };
        public virtual string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] { "IsMetaRightAlign", "IsAllVisualisersEdit", "ShowMeta", "GridStyle", "FilterQuery" };
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

            ThisDataGrid.SelectedValuePath = "To";
            VirtualizingStackPanel.SetIsVirtualizing(ThisDataGrid, false);
            MinusZero mz = MinusZero.Instance;

            if (mz != null && mz.IsInitialized)
            {
                PlatformClassInitialize(baseEdgeVertex);

                SetVertexDefaultValues();

                CreateView();               

                ThisDataGrid.SelectionChanged += _OnSelectionChanged;
            }
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

            GraphUtil.RemoveAllEdges_WhereEdgeIsEdge(sv);

            ////////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////// 
        }

        protected void _OnSelectionChanged(object sender, SelectionChangedEventArgs e){
            if (!TurnOffSelectedVerticesUpdate)
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
            }            
        }

        bool ShowMeta;

        protected virtual void CreateView(){
            ThisDataGrid.Columns.Clear();

            DataGridTextColumn metaColumn = new DataGridTextColumn();

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "ShowMeta:"), "False"))
                ShowMeta = false;
            else
                ShowMeta = true;

            if (ShowMeta)
            {
                System.Windows.Data.Binding mb = new System.Windows.Data.Binding("Meta.Value");
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
                factory.SetBinding(VisualiserEditWrapper.BaseEdgeProperty, new System.Windows.Data.Binding(""));                
                valueColumn.CellTemplate.VisualTree = factory;
            }
            else
            {
                valueColumn.CellTemplate = new DataTemplate();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(VisualiserViewWrapper));
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

            if (TurnOffSelectedItemsUpdate)
                return;

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
            }           
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
    }
}

