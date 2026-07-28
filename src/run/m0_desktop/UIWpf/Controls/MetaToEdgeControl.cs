using m0.Foundation;
using m0.Graph;
using m0.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace m0.UIWpf.Controls
{
    public class MetaToEdgeControl : Border
    {
        private const int MaxMetaToTextLength = 256;

        private readonly StackPanel contentPanel;
        private readonly Image iconImage;
        private readonly Label metaLabel;
        private readonly Label toLabel;
        private int visualUpdateBatchDepth;
        private bool visualUpdatePending;

        public IEdge BaseEdge
        {
            get { return (IEdge)GetValue(BaseEdgeProperty); }
            set { SetValue(BaseEdgeProperty, value); }
        }

        public static readonly DependencyProperty BaseEdgeProperty =
            DependencyProperty.Register("BaseEdge", typeof(IEdge), typeof(MetaToEdgeControl), new UIPropertyMetadata(BaseEdgeChangedCallback));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(MetaToEdgeControl), new UIPropertyMetadata(false, IsSelectedChangedCallback));

        public bool IsHighlighted
        {
            get { return (bool)GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }

        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register("IsHighlighted", typeof(bool), typeof(MetaToEdgeControl), new UIPropertyMetadata(false, IsHighlightedChangedCallback));

        public bool IsMouseHoverHighlighted
        {
            get { return (bool)GetValue(IsMouseHoverHighlightedProperty); }
            set { SetValue(IsMouseHoverHighlightedProperty, value); }
        }

        public static readonly DependencyProperty IsMouseHoverHighlightedProperty =
            DependencyProperty.Register("IsMouseHoverHighlighted", typeof(bool), typeof(MetaToEdgeControl), new UIPropertyMetadata(false, IsMouseHoverHighlightedChangedCallback));

        public bool ShowIcon
        {
            get { return (bool)GetValue(ShowIconProperty); }
            set { SetValue(ShowIconProperty, value); }
        }

        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.Register("ShowIcon", typeof(bool), typeof(MetaToEdgeControl), new UIPropertyMetadata(false, ShowIconChangedCallback));

        public bool ExternalBackgroundMode
        {
            get { return (bool)GetValue(ExternalBackgroundModeProperty); }
            set { SetValue(ExternalBackgroundModeProperty, value); }
        }

        public static readonly DependencyProperty ExternalBackgroundModeProperty =
            DependencyProperty.Register("ExternalBackgroundMode", typeof(bool), typeof(MetaToEdgeControl), new UIPropertyMetadata(false, ExternalBackgroundModeChangedCallback));

        public bool IsKeyboardHighlightedSelected
        {
            get { return (bool)GetValue(IsKeyboardHighlightedSelectedProperty); }
            set { SetValue(IsKeyboardHighlightedSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsKeyboardHighlightedSelectedProperty =
            DependencyProperty.Register("IsKeyboardHighlightedSelected", typeof(bool), typeof(MetaToEdgeControl), new UIPropertyMetadata(false, IsKeyboardHighlightedSelectedChangedCallback));

        public static void BaseEdgeChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            MetaToEdgeControl control = (MetaToEdgeControl)dependencyObject;

            control.RequestVisualUpdate((IEdge)args.NewValue);
        }

        public static void IsSelectedChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            MetaToEdgeControl control = (MetaToEdgeControl)dependencyObject;

            control.UpdateSelectionState();
        }

        public static void IsHighlightedChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            MetaToEdgeControl control = (MetaToEdgeControl)dependencyObject;

            control.UpdateSelectionState();
        }

        public static void IsMouseHoverHighlightedChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            MetaToEdgeControl control = (MetaToEdgeControl)dependencyObject;

            control.UpdateSelectionState();
        }

        public static void ShowIconChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            MetaToEdgeControl control = (MetaToEdgeControl)dependencyObject;

            control.RequestVisualUpdate(control.BaseEdge);
        }

        public static void ExternalBackgroundModeChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            MetaToEdgeControl control = (MetaToEdgeControl)dependencyObject;

            control.UpdateSelectionState();
        }

        public static void IsKeyboardHighlightedSelectedChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            MetaToEdgeControl control = (MetaToEdgeControl)dependencyObject;

            control.UpdateSelectionState();
        }

        public MetaToEdgeControl()
        {
            Background = null;
            BorderThickness = new Thickness(0);
            Padding = new Thickness(0);
            Margin = new Thickness(-3, 0, 3, 0);
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;

            contentPanel = new StackPanel();
            contentPanel.Orientation = Orientation.Horizontal;
            contentPanel.Background = null;

            double enlargedIconSize = WpfUtil.IconSize * 1.0;
            double iconVerticalOverflow = (enlargedIconSize - WpfUtil.IconSize) / 2;

            iconImage = new Image();
            iconImage.Width = enlargedIconSize;
            iconImage.Height = enlargedIconSize;
            iconImage.Margin = new Thickness(0, -iconVerticalOverflow, 3, -iconVerticalOverflow);
            iconImage.VerticalAlignment = VerticalAlignment.Center;
            iconImage.HorizontalAlignment = HorizontalAlignment.Center;
            iconImage.Stretch = Stretch.Uniform;
            iconImage.SnapsToDevicePixels = true;
            iconImage.UseLayoutRounding = true;
            iconImage.Visibility = Visibility.Collapsed;
            // Fant + strong downscale on transparent PNGs often produces gray "tiles" behind the glyph.
            RenderOptions.SetBitmapScalingMode(iconImage, BitmapScalingMode.Fant);

            metaLabel = new Label();
            metaLabel.Padding = new Thickness(0);
            metaLabel.Margin = new Thickness(0);
            metaLabel.Background = null;
            metaLabel.Foreground = GetMetaForegroundBrush();
            metaLabel.FontStyle = FontStyles.Italic;
            metaLabel.FontWeight = WpfUtil.MetaWeight;
            metaLabel.VerticalContentAlignment = VerticalAlignment.Center;

            toLabel = new Label();
            toLabel.Padding = new Thickness(0);
            toLabel.Margin = new Thickness(0);
            toLabel.Background = null;
            toLabel.Foreground = GetForegroundBrush();
            toLabel.FontWeight = WpfUtil.ValueWeight;
            toLabel.VerticalContentAlignment = VerticalAlignment.Center;

            contentPanel.Children.Add(iconImage);
            contentPanel.Children.Add(metaLabel);
            contentPanel.Children.Add(toLabel);

            Child = contentPanel;
        }

        public void RefreshVisuals()
        {
            UpdateVisuals(BaseEdge);
        }

        public void UpdateEdgeAndIcon(IEdge edge, bool showIcon)
        {
            BeginVisualUpdateBatch();

            try
            {
                ShowIcon = showIcon;
                BaseEdge = edge;
            }
            finally
            {
                EndVisualUpdateBatch(true);
            }
        }

        public void RefreshSelectionState()
        {
            UpdateSelectionState();
        }

        private void BeginVisualUpdateBatch()
        {
            visualUpdateBatchDepth++;
        }

        private void EndVisualUpdateBatch(bool forceVisualUpdate)
        {
            if (visualUpdateBatchDepth == 0)
                return;

            visualUpdateBatchDepth--;

            if (visualUpdateBatchDepth != 0)
                return;

            if (visualUpdatePending || forceVisualUpdate)
            {
                visualUpdatePending = false;
                UpdateVisuals(BaseEdge);
            }
        }

        private void RequestVisualUpdate(IEdge edge)
        {
            if (visualUpdateBatchDepth != 0)
            {
                visualUpdatePending = true;
                return;
            }

            UpdateVisuals(edge);
        }

        private void UpdateVisuals(IEdge edge)
        {
            bool isMetaEmpty = IsMetaValueEmpty(edge);

            if (isMetaEmpty)
            {
                metaLabel.Content = null;
                metaLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                metaLabel.Content = GetMetaText(edge);
                metaLabel.Visibility = Visibility.Visible;
            }

            toLabel.Content = GetToText(edge);

            if (!ShowIcon)
            {
                iconImage.Source = null;
                iconImage.Visibility = Visibility.Collapsed;
                UpdateSelectionState();
                return;
            }

            ImageSource iconBitmap = IconServer.GetIconByEdge(edge);

            if (iconBitmap == null)
            {
                iconImage.Source = null;
                iconImage.Visibility = Visibility.Collapsed;
            }
            else
            {
                iconImage.Source = iconBitmap;
                iconImage.Visibility = Visibility.Visible;
            }

            UpdateSelectionState();
        }

        private bool IsMetaValueEmpty(IEdge edge)
        {
            object metaValue = edge?.Meta?.Value;

            if (metaValue == null)
                return true;

            if (GeneralUtil.CompareStrings(metaValue, ""))
                return true;

            if (GeneralUtil.CompareStrings(metaValue, "$Empty"))
                return true;

            return false;
        }

        private string GetMetaText(IEdge edge)
        {
            object metaValue = edge?.Meta?.Value;

            if (metaValue == null)
                return string.Empty;

            return GetDisplayText(metaValue) + " : ";
        }

        private string GetToText(IEdge edge)
        {
            object toValue = edge?.To?.Value;

            if (toValue == null)
                return "[$Empty]";

            if (GeneralUtil.CompareStrings(edge?.Meta?.Value, "$Empty") && GeneralUtil.CompareStrings(toValue, "$Empty"))
                return "[$Empty]";

            if (GeneralUtil.CompareStrings(edge?.Meta?.Value, "$Empty") && GeneralUtil.CompareStrings(toValue, ""))
                return "[$Empty]";

            if (GeneralUtil.CompareStrings(toValue, ""))
                return "";

            return GetDisplayText(toValue);
        }

        private string GetDisplayText(object value)
        {
            string text = value.ToString().Replace("\r", "").Replace("\n", "");

            if (text.Length > MaxMetaToTextLength)
                return text.Substring(0, MaxMetaToTextLength);

            return text;
        }

        private Brush GetForegroundBrush()
        {
            return FindResource("0ForegroundBrush") as Brush ?? Brushes.Black;
        }

        private Brush GetBackgroundBrush()
        {
            return FindResource("0BackgroundBrush") as Brush ?? Brushes.White;
        }

        private Brush GetMetaForegroundBrush()
        {
            return FindResource("0GrayBrush") as Brush ?? GetForegroundBrush();
        }

        private Brush GetHighlightBrush()
        {
            return FindResource("0HighlightBrush") as Brush ?? GetForegroundBrush();
        }

        private Brush GetHighlightForegroundBrush()
        {
            return FindResource("0HighlightForegroundBrush") as Brush ?? GetForegroundBrush();
        }

        private Brush ResolveLabelForegroundBrush()
        {
            if (IsKeyboardHighlightedSelected)
                return GetForegroundBrush();

            if (IsHighlighted)
                return GetHighlightForegroundBrush();

            if (IsMouseHoverHighlighted && IsSelected)
                return GetHighlightBrush();

            if (IsSelected)
                return GetBackgroundBrush();

            if (IsMouseHoverHighlighted)
                return GetHighlightBrush();

            return null;
        }

        private void ApplyLabelForeground(Brush normalMetaForeground, Brush normalToForeground)
        {
            Brush resolved = ResolveLabelForegroundBrush();

            metaLabel.Foreground = resolved ?? normalMetaForeground;
            toLabel.Foreground = resolved ?? normalToForeground;
        }

        private void UpdateSelectionState()
        {
            if (ExternalBackgroundMode)
            {
                Background = null;
                contentPanel.Background = null;

                ApplyLabelForeground(GetMetaForegroundBrush(), GetForegroundBrush());

                metaLabel.Background = null;
                toLabel.Background = null;
                return;
            }

            if (IsHighlighted)
            {
                Background = GetHighlightBrush();
                contentPanel.Background = GetHighlightBrush();

                Brush foreground = IsSelected
                    ? GetForegroundBrush()
                    : GetHighlightForegroundBrush();

                metaLabel.Foreground = foreground;
                toLabel.Foreground = foreground;
            }
            else if (IsMouseHoverHighlighted && IsSelected)
            {
                Background = GetForegroundBrush();
                contentPanel.Background = GetForegroundBrush();

                Brush highlightBrush = GetHighlightBrush();
                metaLabel.Foreground = highlightBrush;
                toLabel.Foreground = highlightBrush;
            }
            else if (IsSelected)
            {
                Background = GetForegroundBrush();
                contentPanel.Background = GetForegroundBrush();

                metaLabel.Foreground = GetBackgroundBrush();
                toLabel.Foreground = GetBackgroundBrush();
            }
            else if (IsMouseHoverHighlighted)
            {
                Background = GetBackgroundBrush();
                contentPanel.Background = GetBackgroundBrush();

                Brush highlightBrush = GetHighlightBrush();
                metaLabel.Foreground = highlightBrush;
                toLabel.Foreground = highlightBrush;
            }
            else
            {
                Background = null;
                contentPanel.Background = null;

                metaLabel.Foreground = GetMetaForegroundBrush();
                toLabel.Foreground = GetForegroundBrush();
            }

            metaLabel.Background = null;
            toLabel.Background = null;
        }
    }
}
