using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using m0.Foundation;
using m0.UIWpf.Visualisers;
using m0.Graph;
using m0.ZeroTypes;
using m0.Util;
using m0.Graph.ExecutionFlow;
using m0.UIWpf.Visualisers.Helper;
using m0.UIWpf.Controls;
using System.Windows.Input;

namespace m0.UIWpf
{
    public class VisualiserEditWrapper: ContentControl, IDisposable, IMouseWheelHandler
    {
        public bool TriggerNewTransaction = false;

        IVertex parentVisualiser;

        public VisualiserEditWrapper() : this(null) { }

        public VisualiserEditWrapper(IVertex _parentVisualiser)        
        {
            parentVisualiser = _parentVisualiser;

            //this.VerticalContentAlignment = VerticalAlignment.Center;
            this.VerticalAlignment = VerticalAlignment.Center;            
        }

        public IEdge BaseEdge
        {
            get { return (IEdge)GetValue(BaseEdgeProperty); }
            set { SetValue(BaseEdgeProperty, value); }
        }
        
        public static readonly DependencyProperty BaseEdgeProperty =
            DependencyProperty.Register("BaseEdge", typeof(IEdge), typeof(VisualiserEditWrapper), new UIPropertyMetadata(BaseEdgeChangedCallback));

        public static void BaseEdgeChangedCallback(DependencyObject d,DependencyPropertyChangedEventArgs _e){
            VisualiserEditWrapper _this = (VisualiserEditWrapper)d;
            IEdge e = (IEdge)_e.NewValue;

            IPlatformClass pc;

            IVertex defvis = e.Meta.Get(false, @"$DefaultEditVisualiser:");
           

            if (defvis == null)
                defvis = e.Meta.Get(false, @"$EdgeTarget:\$DefaultEditVisualiser:");

            if (defvis == null && e.Meta.Get(false, @"$TargetQuery:") != null)
                defvis = MinusZero.Instance.root.Get(false, @"System\Meta\Visualiser\ListAndEnum");
            
           // if (defvis == null)
             //   defvis = e.Meta.Get(false, @"$VertexTarget:\$DefaultEditVisualiser:");
             //
             // in TableVisualiser it makes Class\Association, Class\Aggregation not editable 

            if (defvis == null)
                defvis = e.Meta.Get(false, @"$EdgeTarget:\$Is:\$DefaultEditVisualiser:");

            if (defvis == null)
                defvis = e.Meta.Get(false, @"$VertexTarget:\$Is:\$DefaultEditVisualiser:");

            if (defvis == null && e.To!=null)
                defvis = e.To.Get(false, @"$Is:\$DefaultEditVisualiser:");

            IVertex parentVisualiser = _this.parentVisualiser;

            if (parentVisualiser == null)
            {
                IVisualiser parentIVisualiser = WpfUtil.GetParentVisualiser(_this);

                if (parentIVisualiser != null)
                    parentVisualiser = parentIVisualiser.Vertex;
            }                            

            if (defvis != null)            
                pc = (IPlatformClass)PlatformClass.CreatePlatformObject(defvis, e, parentVisualiser);
            else            
                pc = new StringVisualiser(EdgeHelper.CreateTempEdgeVertex(e), parentVisualiser, false);                

            _this.Content = pc;

            IVisualiser createdVisualiser = pc as IVisualiser;

            MinusZero.Instance.Log(1, "VisualiserEditWrapper.DisposeNesting",
                "BaseEdgeChanged contentType=" + (pc == null ? "null" : pc.GetType().Name)
                + " defvis=" + (defvis == null ? "null" : (defvis.Value == null ? "null" : defvis.Value.ToString()))
                + " parentVisualiser=" + DescribeVertex(parentVisualiser)
                + " contentVertex=" + DescribeVertex(createdVisualiser == null ? null : createdVisualiser.Vertex)
                + " meta=" + (e == null || e.Meta == null || e.Meta.Value == null ? "null" : e.Meta.Value.ToString())
                + " to=" + (e == null || e.To == null || e.To.Value == null ? "null" : e.To.Value.ToString()));
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;

                MinusZero.Instance.Log(1, "VisualiserEditWrapper.DisposeNesting",
                    "Dispose begin contentType=" + (Content == null ? "null" : Content.GetType().Name)
                    + " parentVisualiser=" + DescribeVertex(parentVisualiser)
                    + " contentVertex=" + DescribeIVisualiserVertex(Content));

                if (this.Content != null && this.Content is IDisposable)
                    ((IDisposable)this.Content).Dispose();

                MinusZero.Instance.Log(1, "VisualiserEditWrapper.DisposeNesting",
                    "Dispose end contentWasDisposed=" + (Content is IDisposable));
            }
            else
            {
                MinusZero.Instance.Log(1, "VisualiserEditWrapper.DisposeNesting",
                    "Dispose skipped alreadyDisposed contentType=" + (Content == null ? "null" : Content.GetType().Name));
            }
        }

        private static string DescribeVertex(IVertex vertex)
        {
            if (vertex == null)
                return "null";

            return "val=" + (vertex.Value == null ? "null" : vertex.Value.ToString())
                + " hash=" + vertex.GetHashCode();
        }

        private static string DescribeIVisualiserVertex(object content)
        {
            IVisualiser visualiser = content as IVisualiser;

            if (visualiser == null)
                return "n/a";

            return DescribeVertex(visualiser.Vertex);
        }

        public void MouseWheelAction(MouseWheelEventArgs e)
        {
            if (Content != null && Content is IMouseWheelHandler)
                ((IMouseWheelHandler)Content).MouseWheelAction(e);
        }
    }
}
