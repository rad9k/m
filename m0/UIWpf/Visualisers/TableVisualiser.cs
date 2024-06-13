using System;
using System.Collections.Generic;
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

namespace m0.UIWpf.Visualisers
{
    public class TableVisualiser : ListVisualiser, ITypedEdge
    {
        static IVertex ToShowEdgesMeta_meta = MinusZero.Instance.root.Get(false, @"System\Meta\Visualiser\Table\ToShowEdgesMeta");
        static IVertex FilterQuery_meta = MinusZero.Instance.root.Get(false, @"System\Meta\Visualiser\Table\FilterQuery");

        static string[] _MetaTriggeringUpdateVertex = new string[] { "ExpertMode" };
        public override string[] MetaTriggeringUpdateVertex { get { return _MetaTriggeringUpdateVertex; } }

        static string[] _MetaTriggeringUpdateView = new string[] {"IsAllVisualisersEdit", "ShowHeader", "GridStyle", "AlternatingRows" };
        public override string[] MetaTriggeringUpdateView { get { return _MetaTriggeringUpdateView; } }

        public override void ViewAttributesUpdated() { ResetView(); }

        // TypedEdge START

        public TableVisualiser(IEdge _edge) : base(_edge)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        public TableVisualiser(IVertex baseEdgeVertex, IVertex _parentVisualiser, bool isVolatile) : base(baseEdgeVertex, _parentVisualiser, isVolatile)
        {
            parentVisualiser = _parentVisualiser;
        }

        protected override void PlatformClassInitialize(IVertex baseEdgeVertex)
        {
            new ListVisualiserHelper(parentVisualiser,
                         false,
                         MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Table"),
                         this,
                         "TableVisualiser",
                         this,
                         false,
                         new List<string> { @"", @"BaseEdge:\To:" },
                         "AtomVisualiserFull",
                         baseEdgeVertex,
                         UpdateBaseEdgeCallSchemeEnum.OmmitFirst);

            if (Vertex.Get(false, @"ToShowEdgesMeta:") == null)
                Vertex.AddVertex(ToShowEdgesMeta_meta, null);
        }

        protected override void AddFooter()
        {
            m0.UIWpf.Visualisers.Controls.NewButton button = new m0.UIWpf.Visualisers.Controls.NewButton();

            button.HorizontalAlignment = HorizontalAlignment.Left;

            this.Children.Add(button);
        }

        bool ExpertMode;

        protected override void CreateView(){
            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "AlternatingRows:"), "True"))
                this.ThisDataGrid.AlternatingRowBackground = (Brush)FindResource("0AlternatingBackgroundBrush");
            else
                this.ThisDataGrid.AlternatingRowBackground = (Brush)FindResource("0BackgroundBrush");

            ThisDataGrid.Columns.Clear();


            AddInfoTemplateButton();
            AddDeleteTemplateButton();

            AddColumn("", ""); // Vertex level column

            IVertex childs=null;

            if (ToShowEdgesMeta != null)
            {
                childs = VertexOperations.GetChildEdges(ToShowEdgesMeta);
               
                foreach (IEdge e in childs)
                    if (e.To.Get(false, "$Hide:") == null) // need refactor to VisualiserUtil.Filter
                        AddColumn((string)e.To.Value, "To[" + (string)e.To.Value + "]");

                if (ExpertMode)
                {
                    foreach (IEdge e in MinusZero.Instance.Root.Get(false, @"System\Meta\Base\Vertex"))
                    {
                        bool contains = false;

                        foreach(IEdge ee in childs)
                            if (GeneralUtil.CompareStrings(ee.To, e.To))
                                contains = true;

                        if (contains==false && e.To.Get(false, "$Hide:") == null)
                            AddColumn((string)e.To.Value, "To[" + (string)e.To.Value + "]");
                    }
                }
            }
        }

        protected virtual void AddDeleteTemplateButton()
        {
            DataGridTemplateColumn valueColumn = new DataGridTemplateColumn(); // DELETE

            valueColumn.CellStyle = (Style)FindResource("0ListValueColumn");

            valueColumn.CellTemplate = new DataTemplate();
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(Controls.DeleteButton));
            factory.SetBinding(Controls.DeleteButton.BaseEdgeProperty, new Binding(""));
            valueColumn.CellTemplate.VisualTree = factory;

            ThisDataGrid.Columns.Add(valueColumn);
        }

        protected virtual void AddInfoTemplateButton()
        {
            DataGridTemplateColumn valueColumn = new DataGridTemplateColumn(); //INFO

            valueColumn.CellStyle = (Style)FindResource("0ListValueColumn");

            valueColumn.CellTemplate = new DataTemplate();
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(Controls.InfoButton));
            factory.SetBinding(Controls.InfoButton.BaseEdgeProperty, new Binding(""));
            valueColumn.CellTemplate.VisualTree = factory;

            ThisDataGrid.Columns.Add(valueColumn);
        }

       protected virtual void AddColumn(string columnName, string bindingString)
        {
            DataGridTemplateColumn valueColumn = new DataGridTemplateColumn();

            valueColumn.CellStyle = (Style)FindResource("0ListValueColumn");

            //
            // CELL TEMPLATE
            //

            if (GeneralUtil.CompareStrings(Vertex.Get(false, "IsAllVisualisersEdit:").Value, "True"))
            {
                valueColumn.CellTemplate = new DataTemplate();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(VisualiserEditWrapper));
                //FrameworkElementFactory factory = new FrameworkElementFactory(typeof(VisualiserTransactedEditWrapper));
                factory.SetBinding(VisualiserEditWrapper.BaseEdgeProperty, new Binding(bindingString));
                valueColumn.CellTemplate.VisualTree = factory;
            }
            else
            {
                valueColumn.CellTemplate = new DataTemplate();
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(VisualiserViewWrapper));
                //FrameworkElementFactory factory = new FrameworkElementFactory(typeof(VisualiserTransactedViewWrapper));
                factory.SetBinding(VisualiserViewWrapper.BaseEdgeProperty, new Binding(bindingString));
                valueColumn.CellTemplate.VisualTree = factory;
            }
            
            //
            // EDIT TEMPLATE
            //
            valueColumn.CellEditingTemplate = new DataTemplate();
            FrameworkElementFactory EditFactory = new FrameworkElementFactory(typeof(VisualiserEditWrapper));
            //FrameworkElementFactory EditFactory = new FrameworkElementFactory(typeof(VisualiserTransactedEditWrapper));
            EditFactory.SetBinding(VisualiserEditWrapper.BaseEdgeProperty, new Binding(bindingString));
            valueColumn.CellEditingTemplate.VisualTree = EditFactory;

            if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "ShowHeader:"), "True"))
                valueColumn.Header = columnName + " ";

            ThisDataGrid.Columns.Add(valueColumn);
        }

        protected override void SetVertexDefaultValues()
        {            
            Vertex.Get(false, "IsAllVisualisersEdit:").Value = "False";
            Vertex.Get(false, "ShowHeader:").Value = "True";
            Vertex.Get(false, "ExpertMode:").Value = "False";
            Vertex.Get(false, "AlternatingRows:").Value = "True";
            Vertex.Get(false, "Scale:").Value = 100;

            GraphUtil.ReplaceEdge(Vertex, "GridStyle", MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\GridStyleEnum\Round"));
        }        

        IVertex ToShowEdgesMeta;

        public override void BaseEdgeToUpdated(){
            IVertex bas = Vertex.Get(false, @"BaseEdge:\To:");

            if (Vertex.Get(false, @"ToShowEdgesMeta:\Meta:") == null) // check if we are in the middle of ToShowEdgesMeta sub Vertices switching
                if (Vertex.Get(false, @"ToShowEdgesMeta:\To:") != null)
                    return;

            if (bas != null)
            {                
                ToShowEdgesMeta = null;

                if (Vertex.Get(false, @"ToShowEdgesMeta:\Meta:") != null)
                    ToShowEdgesMeta = Vertex.Get(false, @"ToShowEdgesMeta:\Meta:");

                if (ToShowEdgesMeta == null) // take first edge from BaseEdge\To, to have Meta as ToShowEdesMeta:\Meta:==null
                {
                    IEdge e=bas.FirstOrDefault();

                    if (e != null)
                    {
                        ToShowEdgesMeta = e.Meta;

                        ExecutionFlowHelper.GraphChangeWatchOff();

                        if (Vertex.Get(false, @"ToShowEdgesMeta:") == null)
                            Vertex.AddVertex(ToShowEdgesMeta_meta , null);

                        EdgeHelper.AddEdgeVertexEdges(Vertex.Get(false, @"ToShowEdgesMeta:"), e);

                        ExecutionFlowHelper.GraphChangeWatchOn();
                    }
                }

                if (ToShowEdgesMeta != null)
                {
                    ExecutionFlowHelper.GraphChangeWatchOff();

                    GraphUtil.SetVertexValue(Vertex, FilterQuery_meta, ToShowEdgesMeta.Value + ":");                    

                    ExecutionFlowHelper.GraphChangeWatchOn();
                }


                if (Vertex.Get(false, @"FilterQuery:") != null && Vertex.Get(false, @"FilterQuery:").Value != null) // do the filtering
                {
                    IVertex data = VertexOperations.DoFilter(bas, Vertex.Get(false, @"FilterQuery:"));

                    if (data != null)
                        ThisDataGrid.ItemsSource = data.ToList();
                    else
                        ThisDataGrid.ItemsSource = null;
                }
                else
                    ThisDataGrid.ItemsSource = bas.ToList(); // if there is no .ToList DataGrid can not edit

                if (GraphUtil.GetValueAndCompareStrings(Vertex.Get(false, "ExpertMode:"), "True"))
                    ExpertMode = true;
                else
                    ExpertMode = false;

                ResetView();
            }           
        }
    }
}
