using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.ZeroUML;
using m0.ZeroTypes;
using m0.Graph;
using m0.Util;
using System.Windows.Input;
using System.Windows.Media;
using m0.UIWpf.Foundation;
using m0.UIWpf.Controls;
using m0.UIWpf.Commands;
using System.Windows;
using m0.ZeroCode;

namespace m0.UIWpf.Visualisers.Method
{
    public class VoidVoidMethodVisualiser : Button, IPlatformClass, IDisposable, IHasLocalizableEdges
    {
        public VoidVoidMethodVisualiser()
        {
            MinusZero mz = MinusZero.Instance;            

            if (mz != null && mz.IsInitialized)
            {
                Vertex = mz.CreateTempVertex();

                Vertex.Value = "VoidVoidMethod" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(false, @"System\Meta\Visualiser\Method\VoidVoidMethod"));

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get(false, "BaseEdge:"), mz.Root.Get(false, @"System\Meta\ZeroTypes\Edge"));         


                this.AllowDrop = false;

                this.Content = "run";

                this.Click += VoidVoidMethodVisualiser_Click;
            }
        }

        private void VoidVoidMethodVisualiser_Click(object sender, RoutedEventArgs e)
        {
            IVertex baseVertex = Vertex.Get(false, @"BaseEdge:\To:");

            IVertex methodVertex = Vertex.Get(false, @"ExecutableVertex:");

            this.IsEnabled = false;

            if(baseVertex != null && methodVertex != null)
                ZeroCodeExecutonUtil.CreateExecutionAndVertexMethodExecute(methodVertex, baseVertex);

            this.IsEnabled = true;
        }

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
        
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
            }
        }

        bool IsDisposed = false;

        public void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;
                PlatformClass.RemoveVertexChangeListeners(this.Vertex, new VertexChange(VertexChange));

                if (Vertex is IDisposable)
                    ((IDisposable)Vertex).Dispose();
            }
        }

        public IVertex GetEdgeByLocation(System.Windows.Point point)
        {
            return Vertex.Get(false, @"BaseEdge:");
        }

        public IVertex GetEdgeByVisualElement(System.Windows.FrameworkElement visualElement)
        {
            throw new NotImplementedException();
        }

        public System.Windows.FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }
    }
}
