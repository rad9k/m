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
using System.Windows.Markup;
using m0.UIWpf.Foundation;
using m0.UIWpf.Controls;
using m0.UIWpf.Visualisers.Helper;
using m0.Graph.ExecutionFlow;

namespace m0.UIWpf.Visualisers
{
    public class WrapVisualiser : WrapPanel, IListVisualiser, ITypedEdge
    {
        public event Notify SelectedEdgesChange;

        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        public bool SelectionProhibited { get; set; }

        public double Scale { get; set; } // do not want to expose those as PlatformClass.Vertex

        public double Margin { get; set; } // do not want to expose those as PlatformClass.Vertex 

        private const string UnsectionedSectionName = "Other";
        private const double SectionCornerInset = 4;
        private const double SectionGroupSpacing = 2;
        private static ControlTemplate sectionGroupTemplate;
        private readonly Dictionary<string, WrapPanel> sectionPanels =
            new Dictionary<string, WrapPanel>();

        //

        public void UnselectAllSelectedEdges() { }

        public void SelectedVerticesUpdated() { }

        public void ScaleChange() { }

        public void ViewAttributesUpdated() { }

        public string[] MetaTriggeringUpdateVertex { get; }

        public string[] MetaTriggeringUpdateView { get; }

        public WrapVisualiser(IVertex baseEdgeVertex, IVertex parentVertex, bool isVolatile) : this(baseEdgeVertex, 1.0, parentVertex, isVolatile) { }

        // TypedEdge START
        public WrapVisualiser(IEdge _edge)
        {
            Edge = _edge;            

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        public WrapVisualiser(IVertex baseEdgeVertex, double _scale, IVertex parentVertex, bool isVolatile)
        {
            Scale = _scale;

            Margin = 3;
        
            this.Background = (Brush)FindResource("0LightBackgroundBrush");

            this.Orientation = Orientation.Horizontal;            

            new ListVisualiserHelper(parentVertex,
                isVolatile,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Wrap"),
                this, 
                "WrapVisualiser", 
                this, 
                false, 
                new List<string> { @"BaseEdge:\To:" }, 
                "Visualiser",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitSecond);

            ((ListVisualiserHelper)VisualiserHelper).CustomVertexChangeEvent += CustomVertexChange;
        }        

        protected virtual INoInEdgeInOutVertexVertex CustomVertexChange(IExecution exe)
        {                     
            if (ExecutionFlowHelper.AllEventChildVisualiser(exe.Stack))
                return exe.Stack;

            BaseEdgeToUpdated();

            return exe.Stack;
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
          //  VisualiserHelper.AddContextMenu(); // no contex menu here
        }        

        private string GetSectionName(IEdge edge)
        {
            if (edge == null || edge.Meta == null)
                return UnsectionedSectionName;

            IVertex section = edge.Meta.Get(false, "$Section:");

            if (section == null)
                return UnsectionedSectionName;

            object sectionValue = GraphUtil.GetValue(section);
            string sectionName = sectionValue == null ? null : sectionValue.ToString();

            if (String.IsNullOrWhiteSpace(sectionName))
                return UnsectionedSectionName;

            return sectionName;
        }

        private static ControlTemplate GetSectionGroupTemplate()
        {
            if (sectionGroupTemplate != null)
                return sectionGroupTemplate;

            string cornerRadius = SectionCornerInset.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string xaml =
                "<ControlTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" TargetType=\"GroupBox\">" +
                "<Grid SnapsToDevicePixels=\"True\">" +
                "<Grid.RowDefinitions>" +
                "<RowDefinition Height=\"Auto\"/>" +
                "<RowDefinition Height=\"*\"/>" +
                "</Grid.RowDefinitions>" +
                "<Border Grid.RowSpan=\"2\" Background=\"{TemplateBinding Background}\" BorderBrush=\"{TemplateBinding BorderBrush}\" BorderThickness=\"{TemplateBinding BorderThickness}\" CornerRadius=\"" + cornerRadius + "\"/>" +
                "<Border x:Name=\"Header\" Grid.Row=\"0\" Margin=\"0\" HorizontalAlignment=\"Stretch\" VerticalAlignment=\"Stretch\" Background=\"{DynamicResource 0ForegroundBrush}\" CornerRadius=\"" + cornerRadius + "," + cornerRadius + ",0,0\" Padding=\"2,0,2,0\">" +
                "<ContentPresenter ContentSource=\"Header\" HorizontalAlignment=\"Stretch\" VerticalAlignment=\"Center\" RecognizesAccessKey=\"True\"/>" +
                "</Border>" +
                "<ContentPresenter Grid.Row=\"1\" Margin=\"{TemplateBinding Padding}\" HorizontalAlignment=\"{TemplateBinding HorizontalContentAlignment}\" VerticalAlignment=\"{TemplateBinding VerticalContentAlignment}\"/>" +
                "</Grid>" +
                "</ControlTemplate>";

            sectionGroupTemplate = (ControlTemplate)XamlReader.Parse(xaml);
            return sectionGroupTemplate;
        }

        private GroupBox CreateSectionGroup(string sectionName, out WrapPanel sectionPanel)
        {
            GroupBox sectionGroup = new GroupBox
            {
                Background = (Brush)FindResource("0BackgroundBrush"),
                BorderBrush = (Brush)FindResource("0VeryVeryLightForegroundBrush"),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(SectionGroupSpacing),
                Padding = new Thickness(1),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Template = GetSectionGroupTemplate()
            };

            TextBlock sectionHeader = new TextBlock
            {
                Text = sectionName,
                Foreground = (Brush)FindResource("0BackgroundBrush"),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, -1, 0, 0),
                Padding = new Thickness(0),
                LayoutTransform = new ScaleTransform(Scale, Scale)
            };

            sectionPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal
            };

            sectionGroup.Header = sectionHeader;
            sectionGroup.Content = sectionPanel;

            return sectionGroup;
        }

        protected void AddEdge(IEdge e, Panel sectionPanel)
        {
            StackPanel p = new StackPanel();

            p.Margin = new Thickness(2, 0, 2, 0);
            
            if(!GeneralUtil.CompareStrings(e.Meta.Value,"$Empty")){
                TextBlock label=new TextBlock();
                label.Foreground = (Brush)FindResource("0ForegroundBrush");
                label.Text=e.Meta.Value.ToString();
                label.LayoutTransform = new ScaleTransform(Scale, Scale);
                
                p.Children.Add(label);
            }

            VisualiserEditWrapper w = new VisualiserEditWrapper(this.Vertex);

            w.LayoutTransform = new ScaleTransform(Scale, Scale);

            w.BaseEdge = e;

            p.Children.Add(w);            

            if (GraphUtil.GetQueryOutCount(e.Meta, "$DisplayLarger", null) > 0)
                p.Width = 100;

            sectionPanel.Children.Add(p);
        }

        public void BaseEdgeToUpdated()
        {
            IVertex baseEdgeTo = Vertex.Get(false, @"BaseEdge:\To:");

            IVertex meta = Vertex.Get(false, @"BaseEdge:\To:\$Is:");

  
            if (baseEdgeTo != null && meta != null)
            {
                Children.Clear();
                sectionPanels.Clear();

                VisualiserHelper.DisposeAllChildVisualisers();                

                foreach (IEdge e in VertexOperations.GetChildEdges(meta))
                {
                    IEdge ee = GraphUtil.GetQueryOutFirstEdge(baseEdgeTo, e.To.Value, null);
                    
                    if(ee != null)                        
                        if(VisualiserUtil.FilterEdge(ee, this.Vertex))
                        //if(ee.Meta.Get(false, "$Hide:") == null)
                        {
                            string sectionName = GetSectionName(ee);
                            WrapPanel sectionPanel;

                            if (!sectionPanels.TryGetValue(sectionName, out sectionPanel))
                            {
                                GroupBox sectionGroup = CreateSectionGroup(sectionName, out sectionPanel);
                                sectionPanels.Add(sectionName, sectionPanel);
                                Children.Add(sectionGroup);
                            }

                            AddEdge(ee, sectionPanel);
                        }
                }
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
            return null;
        }

        public IVertex GetEdgeByVisualElement(FrameworkElement visualElement)
        {
 	        throw new NotImplementedException();
        }

        public FrameworkElement GetVisualElementByEdge(IVertex edge)
        {
 	        throw new NotImplementedException();
        }

    }
}
