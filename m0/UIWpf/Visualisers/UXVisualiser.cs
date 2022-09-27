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
using System.Windows;
using System.Windows.Media;
using m0.UIWpf.Controls;
using m0.UIWpf.Foundation;
using m0.UIWpf.Commands;
using m0.Graph.ExecutionFlow;
using m0.User.Process.UX;

using m0.ZeroTypes.UX;

using m0.UIWpf.Visualisers.Helper;
using m0.UIWpf.UX;

namespace m0.UIWpf.Visualisers
{
    public class UXVisualiser : Canvas, IVisualiser, IUX
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        public UXVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser)
        {
            new AtomVisualiserHelper(parentVisualiser,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\UX"), 
                this, 
                "UXVisualiser", 
                this,
                false,
                new List<string> { "", @"BaseEdge:\To:" },
                "AtomVisualiser",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitFirst,
                true);

            CreateUX();
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ScaleChange() { }

        protected override void OnDragEnter(DragEventArgs e) { } // Do not want standard base implemention, that prevents allow drop

        protected override void OnDragOver(DragEventArgs e) { } // Do not want standard base implemention, that prevents allow drop        

        protected bool CanProceedUIUpdateEvent = true;

      

        bool _IsNull;

        bool IsNull
        {
            get { return _IsNull; }
            set
            {
                _IsNull = value;

                if (IsNull)
                    this.Background = (Brush)FindResource("0VeryLightGrayBrush");
                else
                    this.Background = (Brush)FindResource("0BackgroundBrush");
            }
        }

        public void UpdateVertex()
        {
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            if (bv != null && bv.Value != null && bv != MinusZero.Instance.Empty /*&& ((String)bv.Value) != "$Empty"*/)
            {

                IsNull = false;
            }
            else
                IsNull = true;
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

        public FrameworkElement GetVisualElementByEdge(IVertex vertex)
        {
            throw new NotImplementedException();
        }

        ////////////////////////// UX

        public UXItem uxItem { get; set; }
        public UXAggregator uxAggregator { get; set; }



        void CreateUX()
        {
            if (uxItem == null)
                return;

            foreach (Item i in uxItem.Items)
                AddItem(i, uxItem.Vertex);
        }

        public void AddItem(Item i, IVertex parent)
        {
            if (!(i is UXItem))
                return;

            UXItem ui = (UXItem)i;

            IVertex visEdge = EdgeHelper.CreateTempEdgeVertex(i.Edge);

            visEdge.AddVertex(null, "helo");

            IPlatformClass pc = PlatformClass.CreatePlatformObject(ui.Vertex, visEdge, parent);

            if (!(pc is FrameworkElement))
                return;

            FrameworkElement f = (FrameworkElement)pc;

            VisualiserHelper.UpdateControl(f, ui);

            Children.Add(f);
        }

        
    }
}
