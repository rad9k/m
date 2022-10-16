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

using m0.UIWpf.Visualisers.Helper;
using m0.ZeroTypes.UX;
using m0.UIWpf.UX;

namespace m0.UIWpf.UX
{
    public class UXTestVisualiser : Border, IVisualiser, IUX
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        public UXTestVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser)
        {
            new AtomVisualiserHelper(parentVisualiser,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\UXTest"), 
                this, 
                "UXTestVisualiser", 
                this,
                false,
                new List<string> { "", @"BaseEdge:\To:" },
                "AtomVisualiser",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitSecond,
                true);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ScaleChange() { }

        protected override void OnDragEnter(DragEventArgs e) { } // Do not want standard base implemention, that prevents allow drop

        protected override void OnDragOver(DragEventArgs e) { } // Do not want standard base implemention, that prevents allow drop        

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

        // UX

        IVertex baseEdgeTo;
        Canvas canvas;

        public UXItem UXItem { get; set; }
        public UXAggregator UXAggregator { get; set; }
        public Canvas Canvas { get { return canvas; } }

        public void UpdateVertex()
        {
            baseEdgeTo = Vertex.Get(false, @"BaseEdge:\To:");

            if (baseEdgeTo != null && baseEdgeTo.Value != null && baseEdgeTo != MinusZero.Instance.Empty /*&& ((String)bv.Value) != "$Empty"*/)
            {
                IsNull = false;

                CreateUX();
            }
            else
                IsNull = true;
        }

        void CreateUX()
        {
            if (IsNull)
                return;

            canvas = new Canvas();

            this.Child = canvas;


            Label l = new Label();

            l.Content = baseEdgeTo.Value.ToString();

            if (UXItem.ForegroundColor != null)
                l.Foreground = new SolidColorBrush(UXItem.ForegroundColor.GetColor());

            canvas.Children.Add(l);

            VisualiserHelper.UpdateBorder(this, UXItem);
        }
    }
}
