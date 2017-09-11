using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using m0.Foundation;
using m0.UML;
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

namespace m0.UIWpf.Visualisers
{
    public class VertexVisualiser : Grid, IPlatformClass, IDisposable, IHasLocalizableEdges
    {
        TextBlock TextBlock;

        Button Button;

        bool buttonStateIsNew;

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
            BitmapImage b = new BitmapImage(new Uri("mag.gif", UriKind.Relative));
            int q = b.PixelHeight; // will not load without this
            Image.Source = b;

            Button.Foreground = (Brush)FindResource("0LightGrayBrush");

            Button.Click += Button_Click;
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            if (buttonStateIsNew) // new
            {
                IVertex baseVertex = Vertex.Get(@"BaseEdge:\From:");
                IVertex meta = Vertex.Get(@"BaseEdge:\Meta:");

                IVertex newVertex=VertexOperations.AddInstanceByEdgeVertex(baseVertex, meta);

                GraphUtil.CreateOrReplaceEdge(Vertex.Get(@"BaseEdge:"), MinusZero.Instance.Root.Get(@"System\Meta\ZeroTypes\Edge\To"), newVertex);

                if(newVertex!=null)
                    ButtonSetOpen();
            }
            else // open
            {
                FormVisualiser v = (FormVisualiser)UIWpf.getParentFormVisualiser(this);

                if (v != null)                    
                    Edge.CopyAndReplaceEdge(v.Vertex, "BaseEdge", Vertex.Get("BaseEdge:"));
                else
                    //BaseCommands.Open(Vertex.Get("BaseEdge:"), null); // want Form visualiser
                    BaseCommands.OpenFormVisualiser(Vertex.Get("BaseEdge:"));
                
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

        public VertexVisualiser()
        {
            SetUpGrid();

            MinusZero mz = MinusZero.Instance;

            TextBlock = new TextBlock();

            Grid.SetColumn(TextBlock, 0);

            this.Children.Add(TextBlock);

            ButtonSetUp();

            if (mz != null && mz.IsInitialized)
            {
                Vertex = mz.CreateTempVertex();

                Vertex.Value = "VertexVisualiser" + this.GetHashCode();

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex, mz.Root.Get(@"System\Meta\Visualiser\Vertex"));

                ClassVertex.AddIsClassAndAllAttributesAndAssociations(Vertex.Get("BaseEdge:"), mz.Root.Get(@"System\Meta\ZeroTypes\Edge"));

                TextBlock.Background = (Brush)FindResource("0LightGrayBrush");

                TextBlock.Loaded += new RoutedEventHandler(OnLoad);

                TextBlock.PreviewMouseLeftButtonDown += dndPreviewMouseLeftButtonDown;
                TextBlock.PreviewMouseMove += dndPreviewMouseMove;
                TextBlock.Drop += dndDrop;
                TextBlock.AllowDrop = true;

                TextBlock.MouseEnter += dndMouseEnter;
            }
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            if (!UIWpf.HasParentsGotContextMenu(this))
                this.ContextMenu = new m0ContextMenu(this);
        }

        private void UpdateBaseEdge()
        {
            IVertex bv = Vertex.Get(@"BaseEdge:\To:");

            if (bv != null && bv.Value != null)
            {
                TextBlock.Text = bv.Value.ToString();

                ButtonSetOpen();
            }
            else
                if (bv != null)
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

        protected void VertexChange(object sender, VertexChangeEventArgs e)
        {
            if ((sender == Vertex) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "BaseEdge")))
                UpdateBaseEdge();

            if ((sender == Vertex.Get("BaseEdge:")) && (e.Type == VertexChangeType.EdgeAdded) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To"))
                || (sender == Vertex.Get("BaseEdge:")) && (e.Type == VertexChangeType.EdgeRemoved) && (GeneralUtil.CompareStrings(e.Edge.Meta.Value, "To"))
                || (sender == Vertex.Get(@"BaseEdge:\To:") && e.Type == VertexChangeType.ValueChanged))
                UpdateBaseEdge();
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

                UpdateBaseEdge();
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
            return Vertex.Get(@"BaseEdge:");
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
                if (Vertex.Get(@"BaseEdge:\To:") != null)
                {
                    isDraggin = true;

                    IVertex dndVertex = MinusZero.Instance.CreateTempVertex();

                    dndVertex.AddEdge(null, Vertex.Get(@"BaseEdge:"));

                    DataObject dragData = new DataObject("Vertex", dndVertex);
                    dragData.SetData("DragSource", this);

                    Dnd.DoDragDrop(this, dragData);

                    isDraggin = false;
                }
            }
        }

        private void dndDrop(object sender, DragEventArgs e)
        {
            Dnd.DoDropForVertexVisualiser(this, Vertex.Get(@"BaseEdge:"), e);

            UpdateBaseEdge();
        }

        private void dndMouseEnter(object sender, MouseEventArgs e)
        {
            hasButtonBeenDown = false;
        }
    }
}

