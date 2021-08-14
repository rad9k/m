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

namespace m0.UIWpf.Visualisers
{
    public class StringVisualiser : TextBox, IVisualiser
    {
        public GenericVisualiserHelper VisualiserHelper { get; set; }

        public StringVisualiser()
        {
            this.AcceptsReturn = true;

            new GenericVisualiserHelper(this, "TestVisualiser", this);
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        protected override void OnDragEnter(DragEventArgs e) { } // Do not want standard base implemention, that prevents allow drop

        protected override void OnDragOver(DragEventArgs e) { } // Do not want standard base implemention, that prevents allow drop        

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            ////////////////////////////////////////
            Interaction.BeginInteractionWithGraph();
            ////////////////////////////////////////

            if (bv == null)
            {
                IVertex from = Vertex.Get(false, @"BaseEdge:\From:");
                IVertex meta = Vertex.Get(false, @"BaseEdge:\Meta:");

                GraphUtil.SetVertexValue(from, meta, this.Text);

                IsNull = false;
            }
            else
                bv.Value = this.Text;

            //////////////////////////////////////
            Interaction.EndInteractionWithGraph();
            //////////////////////////////////////
        }

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

        public void UpdateBaseEdge()
        {
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            if (bv != null && bv.Value != null /*&& ((String)bv.Value) != "$Empty"*/)
            {
                this.Text = bv.Value.ToString();
                IsNull = false;
            }
            else
                IsNull = true;
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper._Vertex; }
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
    }
}