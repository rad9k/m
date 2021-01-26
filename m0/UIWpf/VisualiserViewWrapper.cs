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

namespace m0.UIWpf
{
    public class VisualiserViewWrapper : ContentControl, IDisposable
    {
        public VisualiserViewWrapper()        
        {
            //this.VerticalContentAlignment = VerticalAlignment.Center;
            this.VerticalAlignment = VerticalAlignment.Center;            
        }

        public IEdge BaseEdge
        {
            get { return (IEdge)GetValue(BaseEdgeProperty); }
            set { SetValue(BaseEdgeProperty, value); }
        }

        public static readonly DependencyProperty BaseEdgeProperty =
            DependencyProperty.Register("BaseEdge", typeof(IEdge), typeof(VisualiserViewWrapper), new UIPropertyMetadata(BaseEdgeChangedCallback));

        public static void BaseEdgeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs _e)
        {
            //int counter = (int)MinusZero.Instance.Root.Get(false, @"TEST\Counter:").Value;
            //counter++;
            //MinusZero.Instance.Root.Get(false, @"TEST\Counter:").Value = counter;

            VisualiserViewWrapper _this = (VisualiserViewWrapper)d;
            IEdge e = (IEdge)_e.NewValue;

            IPlatformClass pc;

            IVertex defvis = e.Meta.Get(false, @"$DefaultViewVisualiser:");

            if (defvis == null)
                defvis = e.Meta.Get(false, @"$EdgeTarget:\$DefaultViewVisualiser:");

            if (defvis == null)
                defvis = e.Meta.Get(false, @"$VertexTarget:\$DefaultViewVisualiser:");

            if (defvis == null)
                defvis = e.Meta.Get(false, @"$EdgeTarget:\$Is:\$DefaultEditVisualiser:");

            if (defvis == null)
                defvis = e.Meta.Get(false, @"$VertexTarget:\$Is:\$DefaultEditVisualiser:");

            if (defvis == null && e.To != null)
                defvis = e.To.Get(false, @"$Is:\$DefaultViewVisualiser:");

            if (defvis != null)
            {
                pc = (IPlatformClass)PlatformClass.CreatePlatformObject(defvis);

                if (defvis.Get(false, "$Inherits:HasBaseEdge") != null)
                    Edge.ReplaceEdgeVertexEdges(pc.Vertex.Get(false, "BaseEdge:"), e);
            }
            else
            {
                pc = new StringViewVisualiser();
                Edge.ReplaceEdgeVertexEdges(pc.Vertex.Get(false, "BaseEdge:"), e);
            }
            
            _this.Content = pc;

        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;
                if (this.Content != null && this.Content is IDisposable)
                    ((IDisposable)this.Content).Dispose();
            }
        }
    }
}
