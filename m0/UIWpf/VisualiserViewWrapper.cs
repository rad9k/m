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

namespace m0.UIWpf
{
    public class VisualiserViewWrapper : ContentControl, IDisposable
    {        
        IVertex parentVisualiser;

        public VisualiserViewWrapper(): this(null) { }

        public VisualiserViewWrapper(IVertex _parentVisualiser)        
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
            DependencyProperty.Register("BaseEdge", typeof(IEdge), typeof(VisualiserViewWrapper), new UIPropertyMetadata(BaseEdgeChangedCallback));

        public static void BaseEdgeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs _e)
        {            
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
                pc = (IPlatformClass)PlatformClass.CreatePlatformObject(defvis, e, _this.parentVisualiser);                           
            else            
                pc = new StringViewVisualiser(Edge.CreateTempEdgeVertex(e), _this.parentVisualiser);                           
            
            _this.Content = pc;

            if(pc is FrameworkElement)
            {
                IVisualiser vis = WpfUtil.GetParentVisualiser((FrameworkElement)pc);

                if (vis != null)
                    vis.SubVisualisers.Add((IDisposable)pc);
            }
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
