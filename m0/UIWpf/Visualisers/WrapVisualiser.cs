using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using m0.Foundation;
using System.Windows.Data;
using m0.Graph;
using m0.UML;
using m0.ZeroTypes;
using m0.Util;
using System.Windows.Media;
using System.Windows;
using m0.UIWpf.Foundation;
using m0.UIWpf.Controls;

namespace m0.UIWpf.Visualisers
{
    public class WrapVisualiser : WrapPanel, IPlatformClass, IDisposable, IHasLocalizableEdges
    {
        public double Scale { get; set; } // do not want to expose those as PlatformClass.Vertex

        public double Margin { get; set; } // do not want to expose those as PlatformClass.Vertex 

        public WrapVisualiser()
        {
            Scale = 1.0;

            Margin = 5;
        
            this.Background = (Brush)FindResource("0BackgroundBrush");

            this.Orientation = Orientation.Horizontal;

            MinusZero mz = MinusZero.Instance;

            if (mz != null && mz.IsInitialized)
            {
                //Vertex = mz.Root.Get(@"System\Session\Visualisers").AddVertex(null, "WrapVisualiser" + this.GetHashCode());

                Vertex = mz.CreateTempVertex();

                Vertex.Value = "WrapVisualiser" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(@"System\Meta\Visualiser\Wrap"));

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get("BaseEdge:"), mz.Root.Get(@"System\Meta\ZeroTypes\Edge"));

               // DO NOT WANT CONTEXTMENU HERE
                // this.ContextMenu = new m0ContextMenu(this);
            }
        }

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

            Children.Add(p);
        }

        protected void UpdateBaseEdge()
        {
            IVertex bas = Vertex.Get(@"BaseEdge:\To:");

            if (bas != null)
            {
                Children.Clear();

                foreach (IEdge e in bas)
                {
                    if(e.Meta.Get("$Hide:")==null)
                        AddEdge(e);
                }
            }
            
        }

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {       
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge")))
                UpdateBaseEdge();                        

            if ((sender == Vertex.Get("BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To")))            
                UpdateBaseEdge();                        

            if (sender == Vertex.Get(@"BaseEdge:\To:") && (e.Type == VertexChangeType.EdgeAdded || e.Type == VertexChangeType.EdgeRemoved))
                UpdateBaseEdge();
        }

        private IVertex _Vertex;

        public IVertex Vertex
        {
            get { return _Vertex; }
            set
            {
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

                //GraphUtil.DeleteEdgeByToVertex(mz.Root.Get(@"System\Session\Visualisers"), Vertex);

                foreach (UIElement e in Children)
                {
                    if (e is StackPanel)
                        foreach (UIElement ee in ((StackPanel)e).Children)
                            if (ee is IDisposable)
                                ((IDisposable)ee).Dispose();
                }
            }
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
