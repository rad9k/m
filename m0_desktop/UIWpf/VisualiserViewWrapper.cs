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
    public class VisualiserViewWrapper : ContentControl, IDisposable, IMouseWheelHandler
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
                pc = new StringViewVisualiser(EdgeHelper.CreateTempEdgeVertex(e), parentVisualiser, false);
            
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

        public void MouseWheelAction(MouseWheelEventArgs e)
        {
            if (Content != null && Content is IMouseWheelHandler)
                ((IMouseWheelHandler)Content).MouseWheelAction(e);
        }
    }
}
