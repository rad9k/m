using m0.Foundation;
using m0.UIWpf;
using m0.UIWpf.Controls;
using System;
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
            BorderThickness = new Thickness(0);
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
            UpdateIconSize(iconSize);

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

        public void UpdateIconSize(double iconSize)
        {
            iconImage.Width = iconSize;
            iconImage.Height = iconSize;
            labelControl.MaxWidth = Math.Max(iconSize * 2.5, 80);
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

        public void ApplyVisualState()
        {
            if (IsKeyboardHighlighted)
            {
                Background = (Brush)FindResource("0HighlightBrush");
                labelControl.IsHighlighted = true;
                labelControl.IsSelected = IsSelected;
                labelControl.IsKeyboardHighlightedSelected = IsSelected;
            }
            else if (IsSelected)
            {
                Background = (Brush)FindResource("0SelectionBrush");
                labelControl.IsHighlighted = false;
                labelControl.IsSelected = true;
                labelControl.IsKeyboardHighlightedSelected = false;
            }
            else
            {
                Background = (Brush)FindResource("0BackgroundBrush");
                labelControl.IsHighlighted = false;
                labelControl.IsSelected = false;
                labelControl.IsKeyboardHighlightedSelected = false;
            }
        }
    }
}
