using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace m0.UIWpf.Visualisers.Diagram
{
    /// <summary>
    /// Interaction logic for DiagramImageItem.xaml
    /// </summary>
    public partial class DiagramImageItem : DiagramRectangleItemBase
    {
        public override void VisualiserUpdate()
        {
            base.VisualiserUpdate();
            
            BitmapImage b = new BitmapImage(new Uri("images\\"+Vertex.Get("Filename:"), UriKind.Relative));
            int q = b.PixelHeight; // will not load without this
            Image.Source = b;

            if (Vertex.Get(@"BaseEdge:\To:").Value != null)
                this.Title.Text = Vertex.Get(@"BaseEdge:\To:").Value.ToString();
            else
                this.Title.Text = "Ø";
        }

        public override void SetBackAndForeground()
        {
            this.Foreground = ForegroundColor;
            this.Background = null;
        }        

        public DiagramImageItem()
        {
            InitializeComponent();
        }

        

        public override void Select()
        {
            base.Select();
            
            this.Background = (Brush)FindResource("0SelectionBrush");

            this.Image.Cursor = Cursors.ScrollAll;

            this.Title.Foreground = BackgroundColor;
        }

        public override void Unselect()
        {
            base.Unselect();

            SetBackAndForeground();

            this.Image.Cursor = Cursors.Arrow;

            this.Title.Foreground = ForegroundColor;
        }

        public override void Highlight()
        {
            base.Highlight();
            
            this.Background = (Brush)FindResource("0HighlightBrush");
        }

        public override void Unhighlight()
        {
            SetBackAndForeground();

            base.Unhighlight();
        }
    }
}
