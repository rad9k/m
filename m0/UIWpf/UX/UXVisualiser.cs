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

namespace m0.UIWpf.UX
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
            VisualiserHelper.Dispose_UX();
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

        public UXItem UXItem { get; set; }
        public UXAggregator UXAggregator { get; set; }
        public Canvas Canvas { get { return this; } }

        void CreateUX()
        {
            if (UXItem == null)
                return;

            IterateItems(Canvas, UXItem);
        }

        void IterateItems(Canvas c, UXItem parentUXItem)
        {
            foreach (object i in parentUXItem.Items)
                if (i is UXItem)
                {
                    UXItem ux = (UXItem)i;

                    if (!GraphUtil.ExistQueryOut(ux.Vertex, "$Is", "Wrap"))
                    {
                        IUX iux = AddItem(c, ux, parentUXItem.Vertex);

                        if (iux != null)
                            IterateItems(iux.Canvas, ux);
                    }
                }
        }

        public IUX AddItem(Canvas c, UXItem ui, IVertex parent)
        {
            IVertex visEdge = EdgeHelper.CreateTempEdgeVertex(ui.Edge);

            IPlatformClass pc = PlatformClass.CreatePlatformObject(ui.Vertex, visEdge, parent);

            if (!(pc is FrameworkElement) || !(pc is IUX))
                return null;

            FrameworkElement f = (FrameworkElement)pc;

            VisualiserHelper.UpdateControl(f, ui);

            c.Children.Add(f);

            return (IUX)pc;
        }

        
    }
}
