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
using m0.UIWpf.Foundation;
using System.Windows;
using m0.UIWpf.Commands;
using m0.UIWpf.Controls;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;

namespace m0.UIWpf.Visualisers
{
    public class EnumVisualiser : ComboBox, IVisualiser
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        public EnumVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser)
        {
            new AtomVisualiserHelper(parentVisualiser,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Enum"),
                this, 
                "EnumVisualiser", 
                this,
                baseEdgeVertex);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ZoomVisualiserContentChange() { }

        bool DoingSelectionChanged = false;

        protected bool CanProceedUIUpdateEvent = true;

        protected override void OnSelectionChanged(SelectionChangedEventArgs _e)
        {
            if (!CanProceedUIUpdateEvent)
                return;

            if (DoingSelectionChanged == false)
            {
                DoingSelectionChanged = true;

                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////

                if (this.SelectedItem != null && ((ComboBoxItem)this.SelectedItem).Tag is IVertex)
                {
                    IVertex tag = (IVertex)((ComboBoxItem)this.SelectedItem).Tag;

                    IVertex bev = Vertex.Get(false, "BaseEdge:");

                    if (bev != null)
                    {
                        IVertex fromv = bev.Get(false, "From:");
                        IVertex metav = bev.Get(false, "Meta:");
                        IVertex tov = bev.Get(false, "To:");

                        if (tov != tag) // is there any change ?
                        {
                            //GraphUtil.ReplaceEdge(fromv, metav, tag);

                            GraphUtil.CreateOrReplaceEdge(fromv, metav, tag);

                            GraphUtil.CreateOrReplaceEdge(bev, MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge\To"), tag);                            
                        }
                    }                    
                }

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////

                DoingSelectionChanged = false;
            }

            base.OnSelectionChanged(_e);
        }

        public void UpdateVertex()
        {
            IVertex bev = Vertex.Get(false, "BaseEdge:");

            if (bev == null)
                return;

            IVertex fromv = bev.Get(false, "From:");
            IVertex metav = bev.Get(false, "Meta:");
            IVertex tov = bev.Get(false, "To:");

            if(fromv!=null && metav!=null && tov!=null){                
                this.Items.Clear();

                ComboBoxItem SelectedItem=null;

                foreach (IEdge e in metav.GetAll(false, @"$EdgeTarget:\EnumValue:"))
                {
                    ComboBoxItem i = new ComboBoxItem();
                    i.Content = e.To.Value;
                    i.Tag = e.To;
                    this.Items.Add(i);

                    if(tov.Value==e.To.Value)
                        SelectedItem = i;
                }

                CanProceedUIUpdateEvent = false;

                this.SelectedItem = SelectedItem;

                CanProceedUIUpdateEvent = true;

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
            return Vertex.Get(false, @"BaseEdge:");
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
