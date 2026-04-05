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
using System.Windows.Media.Imaging;
using m0.UIWpf.Visualisers.Helper;
using m0.User.Process.UX;

namespace m0.UIWpf.Visualisers
{
    public class VertexVisualiser : Grid, IVisualiser, ITypedEdge
    {
        public AtomVisualiserHelper VisualiserHelper { get; set; }        

        TextBlock TextBlock;

        Button Button;

        bool buttonStateIsNew;

        // TypedEdge START

        public VertexVisualiser(IEdge _edge)
        {
            Edge = _edge;

            TypedEdge.vertexDictionary.Add(Edge.To, this);
        }

        public IEdge Edge { get; set; }
        // TypedEdge END

        public VertexVisualiser(IVertex baseEdgeVertex, IVertex parentVisualiser, bool isVolatile)
        {
            SetUpGrid();

            MinusZero mz = MinusZero.Instance;

            TextBlock = new TextBlock();

            Grid.SetColumn(TextBlock, 0);

            this.Children.Add(TextBlock);

            ButtonSetUp();

            new AtomVisualiserHelper(parentVisualiser,
                isVolatile,
                MinusZero.Instance.Root.Get(false, @"System\Meta\Visualiser\Vertex"),
                this,
                "VertexVisualiser",
                this,
                false,
                new List<string> { @"BaseEdge:\To:" },
                "Visualiser",
                baseEdgeVertex,
                UpdateBaseEdgeCallSchemeEnum.OmmitSecond);

            TextBlock.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
            TextBlock.PreviewMouseMove += dndPreviewMouseMove;
            TextBlock.Drop += dndDrop;
            TextBlock.AllowDrop = true;

            TextBlock.MouseEnter += dndMouseEnter;

            TextBlock.Background = (Brush)FindResource("0LightGrayBrush");
        }

        void ButtonSetNew()
        {
            Button.Content = "+";
            buttonStateIsNew = true;
        }

        void ButtonSetOpen()
        {
            Button.Content = Image;
            buttonStateIsNew = false;
        }

        Image Image;

        void ButtonSetUp()
        {
            Button = new Button();

            Grid.SetColumn(Button, 1);

            Button.Style = (Style)Application.Current.FindResource("TransparentStyle");

            Button.BorderThickness = new Thickness(0);
            Button.Margin = new Thickness(2,0,0,0);
            Button.Padding = new Thickness(0);

            this.Children.Add(Button);

            Image = new Image();
            BitmapImage b = new BitmapImage(new Uri(@"pack://application:,,/m0_desktop;Component/_resources/basic/details.png", UriKind.RelativeOrAbsolute));
            int q = b.PixelHeight; // will not load without this
            Image.Source = b;

            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.HighQuality);

            Button.Foreground = (Brush)FindResource("0LightGrayBrush");

            Button.Click += Button_Click;
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            if (buttonStateIsNew) // new
            {
                ////////////////////////////////////////
                Interaction.BeginInteractionWithGraph();
                ////////////////////////////////////////
                
                IVertex baseVertex = Vertex.Get(false, @"BaseEdge:\From:");
                IVertex meta = Vertex.Get(false, @"BaseEdge:\Meta:");

                IVertex newVertex=VertexOperations.AddInstanceByEdgeVertex(baseVertex, meta);

                GraphUtil.CreateOrReplaceEdge(Vertex.Get(false, @"BaseEdge:"), MinusZero.Instance.Root.Get(false, @"System\Meta\ZeroTypes\Edge\To"), newVertex);

                if(newVertex!=null)
                    ButtonSetOpen();

                ////////////////////////////////////////
                Interaction.EndInteractionWithGraph();
                ////////////////////////////////////////

            }
            else // open
            {
                FormVisualiser v = (FormVisualiser)WpfUtil.GetParentFormVisualiser(this);

                if (v != null)
                {
                    ////////////////////////////////////////
                    Interaction.BeginInteractionWithGraph();
                    ////////////////////////////////////////
                    
                    EdgeHelper.CopyAndReplaceEdgeVertexByEdgeVertex(v.Vertex, "BaseEdge", Vertex.Get(false, "BaseEdge:"));

                    ////////////////////////////////////////
                    Interaction.EndInteractionWithGraph();
                    ////////////////////////////////////////
                }
                else
                    //BaseCommands.Open(Vertex.Get(false, "BaseEdge:"), null); // want Form visualiser
                    BaseCommands.OpenFormVisualiser(Vertex.Get(false, "BaseEdge:"), false);
                
            }
        }

        void SetUpGrid()
        {
            ColumnDefinition cdd = new ColumnDefinition();
            cdd.Width = new GridLength(1, GridUnitType.Star);
            this.ColumnDefinitions.Add(cdd);

            ColumnDefinition cdd2 = new ColumnDefinition();
            cdd2.Width = new GridLength(12, GridUnitType.Pixel);
            this.ColumnDefinitions.Add(cdd2);
        }        

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            VisualiserHelper.AddContextMenu();
        }

        public void ScaleChange() { }

        public void BaseEdgeToUpdated()
        {
            IVertex bv = Vertex.Get(false, @"BaseEdge:\To:");

            if (bv != null && bv.Value != null && bv != MinusZero.Instance.Empty)
            {
                TextBlock.Text = bv.Value.ToString();

                ButtonSetOpen();
            }
            else
                if (bv != null && bv != MinusZero.Instance.Empty)
                {
                    TextBlock.Text = "ØØØ";

                    ButtonSetOpen();
                }
                else
                {
                    TextBlock.Text = "Ø";

                    ButtonSetNew();
                }
        }

        public IVertex Vertex
        {
            get { return VisualiserHelper.Vertex; }
            set { VisualiserHelper.SetVertex(value); }
        }

        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                VisualiserHelper.Dispose();
            }
        }

        public IVertex GetEdgeByPoint(System.Windows.Point point)
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

        ///// DRAG AND DROP

        Point dndStartPoint;

        private void dndPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dndStartPoint = e.GetPosition(this);

            MinusZero.Instance.IsGUIDragging = false;

            hasButtonBeenDown = true;
        }

        bool isDraggin = false;
        bool hasButtonBeenDown;

        private void dndPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            Vector diff = dndStartPoint - mousePos;

            if (hasButtonBeenDown && isDraggin == false && (e.LeftButton == MouseButtonState.Pressed) && (
                (Math.Abs(diff.X) > Dnd.MinimumHorizontalDragDistance) ||
                (Math.Abs(diff.Y) > Dnd.MinimumVerticalDragDistance)))
            {
                if (Vertex.Get(false, @"BaseEdge:\To:") != null)
                {
                    isDraggin = true;

                    IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                    dndVertex.AddEdge(null, Vertex.Get(false, @"BaseEdge:"));

                    dndVertex.AddExternalReference();

                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", this);

                    Dnd.DoDragDrop(this, dragData);

                    isDraggin = false;
                }
            }
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            Dnd.DoDropForVertexVisualiser(this, Vertex.Get(false, @"BaseEdge:"), e);

            BaseEdgeToUpdated();
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }
    }
}

