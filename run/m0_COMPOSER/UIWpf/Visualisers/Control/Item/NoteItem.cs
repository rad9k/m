using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using m0.Foundation;
using System.Windows.Controls;
using m0.UIWpf;
using System.Windows.Media;
using System.Windows;
using m0.Graph;
using m0.ZeroTypes;
using m0.UIWpf.Commands;

namespace m0_COMPOSER.UIWpf.Visualisers.Control.Item
{
    public class NoteItem : Border, IItem
    {
        Canvas Canvas;

        public void OpenDefaultVisualiser()
        {
            OpenFormVisualiser();
        }

        public void OpenFormVisualiser()
        {
            if (BaseEdge == null)
                return;

            IVertex edgeVertex = EdgeHelper.CreateTempEdgeVertex(BaseEdge);

            BaseCommands.OpenFormVisualiser(edgeVertex, false);
        }


        public void Add(Canvas canvas)
        {
            Canvas = canvas;

            Canvas.Children.Add(this);


            ItemDictionary.Add(this);
        }

        public void Remove()
        {
            Canvas.Children.Remove(this);


            ItemDictionary.Remove(this);
        }

        public IEdge BaseEdge { get; set; }

        public bool IsCentered { get { return false; } }

        public String Label { get; set; }        

        public bool CanResizeHorizontally { get { return true; } }

        public bool CanResizeVertically { get { return false; } }

        public IZoomScrollViewerHost Host { get; set; }

        bool isSelected;

        public bool IsSelected { get { return isSelected; } }

        bool showLabel;

        bool showVelocity;

        public double HiddenLeft { get; set; }

        public double HiddenRight { get; set; }

        public double HiddenHorizontalCenter { get; set; }

        public double HiddenVerticalCenter { get; set; }

        public double HiddenTop { get; set; }

        public double HiddenBottom { get; set; }

        public void SetHiddenFromReal()
        {
            HiddenLeft = Canvas.GetLeft(this);

            HiddenRight = HiddenLeft + this.Width;

            HiddenTop = Canvas.GetTop(this);

            HiddenBottom = HiddenTop + Height;
        }

        public void PlayHighlight()
        {            
            BorderThickness = new Thickness(2);

            BorderBrush = (Brush)WpfUtil.FindResource("0HardHighlightBrush");

            Background = (Brush)WpfUtil.FindResource("0HardHighlightBrush");

            if (showLabel)
            {
                labelControl.Background = (Brush)WpfUtil.FindResource("0HardHighlightBrush");

                labelControl.Foreground = (Brush)WpfUtil.FindResource("0BackgroundBrush");

                labelControl.Visibility = System.Windows.Visibility.Visible;
            }
            else
                labelControl.Visibility = System.Windows.Visibility.Hidden;
        }

        public void StopHighlight()
        {
            if (isSelected)
                SelectHighlight();
            else
                NoHighlight();
        }

        public void SelectHighlight()
        {
            isSelected = true;

            BorderThickness = new Thickness(3);

            BorderBrush = (Brush)WpfUtil.FindResource("0HighlightBrush");
            
            Background = (Brush)WpfUtil.FindResource("0HighlightBrush");

            if (showLabel)
            {
                labelControl.Background = (Brush)WpfUtil.FindResource("0HighlightBrush");

                labelControl.Foreground = (Brush)WpfUtil.FindResource("0BackgroundBrush");

                labelControl.Visibility = System.Windows.Visibility.Visible;
            }
            else
                labelControl.Visibility = System.Windows.Visibility.Hidden;                        
        }

        public void NoHighlight()
        {
            isSelected = false;

            BorderThickness = new Thickness(2);

            BorderBrush = (Brush)WpfUtil.FindResource("0ForegroundBrush");

            if (showVelocity)
            {
                Background = velocityBrush_Background;

                if (showLabel)
                {
                    labelControl.Background = velocityBrush_Background;
                    labelControl.Foreground = velocityBrush_Foreground;
                    labelControl.Visibility = System.Windows.Visibility.Visible;
                }
                else                
                    labelControl.Visibility = System.Windows.Visibility.Hidden;                
            }
            else
            {
                Background = (Brush)WpfUtil.FindResource("0LightForegroundBrush");

                if (showLabel)
                {
                    labelControl.Foreground = (Brush)WpfUtil.FindResource("0BackgroundBrush");
                    labelControl.Background = (Brush)WpfUtil.FindResource("0LightForegroundBrush");
                    labelControl.Visibility = System.Windows.Visibility.Visible;
                }
                else                
                    labelControl.Visibility = System.Windows.Visibility.Hidden;                
            }
        }

        TextBlock labelControl;        

        public NoteItem(IEdge baseEdge, IZoomScrollViewerHost host, bool _showLabel, bool _showVelocity)
        {
            BaseEdge = baseEdge;            

            Host = host;

            showLabel = _showLabel;

            showVelocity = _showVelocity;

            //

            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

            BorderThickness = new System.Windows.Thickness(2);

            labelControl = new TextBlock();

            this.Child = labelControl;                        
           
            NoHighlight();

            this.SizeChanged += NoteItem_SizeChanged;
        }

        private void NoteItem_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (showLabel)
            {
                if (this.Height > 17 && this.Width > 25)
                    labelControl.Visibility = System.Windows.Visibility.Visible;
                else
                    labelControl.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        Brush velocityBrush_Background = (Brush)WpfUtil.FindResource("0LightForegroundBrush");
        Brush velocityBrush_Foreground = (Brush)WpfUtil.FindResource("0BackgroundBrush");

        public void Update()
        {
            labelControl.Text = " " + Label;            

            if (showVelocity)
            {
                velocityBrush_Background = (Brush)WpfUtil.FindResource("0LightForegroundBrush");

                int? velocity = GraphUtil.GetIntegerValue(BaseEdge.To.Get(false, "Velocity:"));

                if (velocity != null)
                {
                    byte colorByte = (byte)(255 - ((int)velocity * 2));

                    Color velocityColor = Color.FromRgb(colorByte, colorByte, colorByte);

                    velocityBrush_Background = new SolidColorBrush(velocityColor);

                    velocityBrush_Foreground = new SolidColorBrush(WpfUtil.GetNegativeColorWhiteOrBlack(velocityColor));


                    if (showLabel)
                    {
                        labelControl.Foreground = velocityBrush_Foreground;
                        labelControl.Background = velocityBrush_Background;
                        labelControl.Visibility = System.Windows.Visibility.Visible;
                    }
                }

                Background = velocityBrush_Background;
            }            
        }

        public double Left {
            get { return Canvas.GetLeft(this); }
            set { Canvas.SetLeft(this, value); }
        }

        public double Right {
            get { return Left + Width; }
            set { Width = value - Left; }
        }

        public double HorizontalCenter { get; set; }

        public double VerticalCenter { get; set; }

        public double Top
        {
            get { return Canvas.GetTop(this); }
            set { Canvas.SetTop(this, value); }
        }

        public double Bottom
        {
            get { return Top + Height; }
            set { Height = value - Top; }
        }
    }
}
