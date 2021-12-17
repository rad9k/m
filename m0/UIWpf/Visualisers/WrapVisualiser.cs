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
using m0.UIWpf.Foundation;
using m0.UIWpf.Controls;
using m0.UIWpf.Visualisers.Helper;

namespace m0.UIWpf.Visualisers
{
    public class WrapVisualiser : WrapPanel, IVisualiser
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }

        public List<IDisposable> SubVisualisers { get; set; }

        public double Scale { get; set; } // do not want to expose those as PlatformClass.Vertex

        public double Margin { get; set; } // do not want to expose those as PlatformClass.Vertex 

        public WrapVisualiser(IVertex baseEdgeVertex) : this(baseEdgeVertex, 1.0) { }
         
        public WrapVisualiser(IVertex baseEdgeVertex, double _scale)
        {
            Scale = _scale;

            Margin = 5;
        
            this.Background = (Brush)FindResource("0BackgroundBrush");

            this.Orientation = Orientation.Horizontal;

            SubVisualisers = new List<IDisposable>();

            new AtomVisualiserHelper(MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Wrap"),
                this, 
                "WrapVisualiser", 
                this, 
                false, 
                new List<string> { @"BaseEdge:\To:" }, 
                "Visualiser",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitSecond);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ZoomVisualiserContentChange() { }

        protected void AddEdge(IEdge e)
        {
            StackPanel p = new StackPanel();

            p.Margin = new Thickness(Margin);   
            
            if(!GeneralUtil.CompareStrings(e.Meta.Value,"$Empty")){
                TextBlock label=new TextBlock();
                label.Foreground = (Brush)FindResource("0GrayBrush");
                label.Text=e.Meta.Value.ToString();
                label.LayoutTransform = new ScaleTransform(Scale, Scale);
                
                p.Children.Add(label);
            }

            VisualiserEditWrapper w = new VisualiserEditWrapper();

            w.LayoutTransform = new ScaleTransform(Scale, Scale);

            w.BaseEdge = e;

            p.Children.Add(w);

            SubVisualisers.Add(w);

            if (GraphUtil.GetQueryOutCount(e.Meta, "$DisplayLarger", null) > 0)
                p.Width = 100;

            Children.Add(p);
        }

        public void UpdateBaseEdge()
        {
            IVertex baseEdgeTo = Vertex.Get(false, @"BaseEdge:\To:");

            IVertex meta = Vertex.Get(false, @"BaseEdge:\To:\$Is:");

  
            if (baseEdgeTo != null && meta != null)
            {
                Children.Clear();

                SubVisualisers.Clear();

                foreach (IEdge e in VertexOperations.GetChildEdges(meta))
                {
                    IEdge ee = GraphUtil.GetQueryOutFirstEdge(baseEdgeTo, e.To.Value, null);
                    
                    if(ee != null)                        
                        if(VisualiserUtil.FilterEdge(ee, this.Vertex))
                        //if(ee.Meta.Get(false, "$Hide:") == null)
                            AddEdge(ee);
                }
            }           
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        public void Dispose()
        {
            VisualiserHelper.Dispose();
        }

        public IVertex GetEdgeByLocation(Point point)
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
