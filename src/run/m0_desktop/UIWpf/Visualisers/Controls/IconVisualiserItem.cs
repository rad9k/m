using m0.Foundation;
using m0.UIWpf;
using m0.UIWpf.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace m0.UIWpf.Visualisers.Controls
{
    public class IconVisualiserItem : Border
    {
        private readonly StackPanel contentPanel;
        private readonly Image iconImage;
        private readonly MetaToEdgeControl labelControl;

        public IEdge BaseEdge { get; private set; }

        public bool IsSelected { get; private set; }

        public bool IsKeyboardHighlighted { get; private set; }

        public IconVisualiserItem()
        {
            BorderThickness = new Thickness(1);
            Padding = new Thickness(4);
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;

            contentPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            iconImage = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Stretch = Stretch.Uniform,
                SnapsToDevicePixels = true,
                UseLayoutRounding = true
            };

            RenderOptions.SetBitmapScalingMode(iconImage, BitmapScalingMode.Fant);

            labelControl = new MetaToEdgeControl
            {
                ShowIcon = false,
                ExternalBackgroundMode = true,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            contentPanel.Children.Add(iconImage);
            contentPanel.Children.Add(labelControl);

            Child = contentPanel;
        }

        public void Initialize(IEdge baseEdge, double iconSize)
        {
            BaseEdge = baseEdge;

            iconImage.Width = iconSize;
            iconImage.Height = iconSize;

            ImageSource iconSource = IconServer.GetIconByEdge(baseEdge);

            if (iconSource == null)
            {
                iconImage.Source = null;
                iconImage.Visibility = Visibility.Collapsed;
            }
            else
            {
                iconImage.Source = iconSource;
                iconImage.Visibility = Visibility.Visible;
            }

            labelControl.BaseEdge = baseEdge;
            labelControl.RefreshVisuals();

            ApplyVisualState();
        }

        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
            ApplyVisualState();
        }

        public void SetKeyboardHighlighted(bool isKeyboardHighlighted)
        {
            IsKeyboardHighlighted = isKeyboardHighlighted;
            ApplyVisualState();
        }

        public void ApplyScaleTransform(double scale)
        {
            if (scale != 1.0)
                LayoutTransform = new ScaleTransform(scale, scale);
            else
                LayoutTransform = null;
        }

        public void ApplyVisualState()
        {
            Brush backgroundBrush;
            Brush borderBrush;

            if (IsKeyboardHighlighted)
            {
                backgroundBrush = (Brush)FindResource("0HighlightBrush");
                borderBrush = (Brush)FindResource("0HighlightBrush");
                labelControl.IsHighlighted = true;
                labelControl.IsSelected = IsSelected;
                labelControl.IsKeyboardHighlightedSelected = IsSelected;
            }
            else if (IsSelected)
            {
                backgroundBrush = (Brush)FindResource("0SelectionBrush");
                borderBrush = (Brush)FindResource("0SelectionBrush");
                labelControl.IsHighlighted = false;
                labelControl.IsSelected = true;
                labelControl.IsKeyboardHighlightedSelected = false;
            }
            else
            {
                backgroundBrush = (Brush)FindResource("0BackgroundBrush");
                borderBrush = (Brush)FindResource("0LightGrayBrush");
                labelControl.IsHighlighted = false;
                labelControl.IsSelected = false;
                labelControl.IsKeyboardHighlightedSelected = false;
            }

            Background = backgroundBrush;
            BorderBrush = borderBrush;
        }
    }
}
