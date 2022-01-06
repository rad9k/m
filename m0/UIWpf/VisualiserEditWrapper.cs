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
    public class VisualiserEditWrapper:ContentControl, IDisposable
    {
        public bool TriggerNewTransaction = false;

        IVertex parentVisualiser;

        public VisualiserEditWrapper(IVertex _parentVisualiser)        
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
            DependencyProperty.Register("BaseEdge", typeof(IEdge), typeof(VisualiserEditWrapper), new UIPropertyMetadata(BaseEdgeChangedCallback));

        public static void BaseEdgeChangedCallback(DependencyObject d,DependencyPropertyChangedEventArgs _e){
            VisualiserEditWrapper _this = (VisualiserEditWrapper)d;
            IEdge e = (IEdge)_e.NewValue;

            IPlatformClass pc;

            IVertex defvis = e.Meta.Get(false, @"$DefaultEditVisualiser:");
           

            if (defvis == null)
                defvis = e.Meta.Get(false, @"$EdgeTarget:\$DefaultEditVisualiser:");

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

            ///////////////////////////////////////
            //if(_this.TriggerNewTransaction)
            //    ExecutionFlowHelper.StartTransaction();
            ///////////////////////////////////////

            if (defvis != null)
            {
                pc = (IPlatformClass)PlatformClass.CreatePlatformObject(defvis, e);
                
               // if (defvis.Get(false, "$Inherits:HasBaseEdge") != null)                
               //     Edge.ReplaceEdgeVertexEdges(pc.Vertex.Get(false, "BaseEdge:"), e);                                    
            }
            else
            {
                pc = new StringVisualiser(Edge.CreateTempEdgeVertex(e));
                //Edge.ReplaceEdgeVertexEdges(pc.Vertex.Get(false, "BaseEdge:"), e);                                                    
            }

            ////////////////////////////////////////
            //if (_this.TriggerNewTransaction)
            //    ExecutionFlowHelper.CommitTransaction();
            ////////////////////////////////////////

            _this.Content = pc;

            if (pc is FrameworkElement)
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
