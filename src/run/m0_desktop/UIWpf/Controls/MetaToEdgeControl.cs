using m0.Foundation;
using m0.Graph;
using m0.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace m0.UIWpf.Controls
{
    public class MetaToEdgeControl : Border
    {
        private static readonly object IconCacheLock = new object();
        private static readonly Dictionary<string, Dictionary<string, string>> IconPathsByDirectory =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Dictionary<string, string>> RequestedIconPathByDirectory =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, BitmapImage> BitmapByIconPath =
            new Dictionary<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);

        private readonly StackPanel contentPanel;
        private readonly Image iconImage;
        private readonly Label metaLabel;
        private readonly Label toLabel;

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

        public static void BaseEdgeChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            MetaToEdgeControl control = (MetaToEdgeControl)dependencyObject;

            control.UpdateVisuals((IEdge)args.NewValue);
        }

        public static void IsSelectedChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            MetaToEdgeControl control = (MetaToEdgeControl)dependencyObject;

            control.UpdateSelectionState();
        }

        public MetaToEdgeControl()
        {
            Background = Brushes.Transparent;
            BorderThickness = new Thickness(0);
            Padding = new Thickness(0);

            contentPanel = new StackPanel();
            contentPanel.Orientation = Orientation.Horizontal;
            contentPanel.Background = Brushes.Transparent;

            double enlargedIconSize = WpfUtil.IconSize * 2;
            double iconVerticalOverflow = (enlargedIconSize - WpfUtil.IconSize) / 2;

            iconImage = new Image();
            iconImage.Width = enlargedIconSize;
            iconImage.Height = enlargedIconSize;
            iconImage.Margin = new Thickness(0, -iconVerticalOverflow, 4, -iconVerticalOverflow);
            iconImage.VerticalAlignment = VerticalAlignment.Center;
            iconImage.Visibility = Visibility.Collapsed;
            RenderOptions.SetBitmapScalingMode(iconImage, BitmapScalingMode.HighQuality);

            metaLabel = new Label();
            metaLabel.Padding = new Thickness(0);
            metaLabel.Margin = new Thickness(0);
            metaLabel.Background = Brushes.Transparent;
            metaLabel.Foreground = GetMetaForegroundBrush();
            metaLabel.FontStyle = FontStyles.Italic;
            metaLabel.FontWeight = WpfUtil.MetaWeight;
            metaLabel.VerticalContentAlignment = VerticalAlignment.Center;

            toLabel = new Label();
            toLabel.Padding = new Thickness(0);
            toLabel.Margin = new Thickness(0);
            toLabel.Background = Brushes.Transparent;
            toLabel.Foreground = GetForegroundBrush();
            toLabel.FontWeight = WpfUtil.ValueWeight;
            toLabel.VerticalContentAlignment = VerticalAlignment.Center;

            contentPanel.Children.Add(iconImage);
            contentPanel.Children.Add(metaLabel);
            contentPanel.Children.Add(toLabel);

            Child = contentPanel;
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

            BitmapImage iconBitmap = TryLoadIconBitmap(edge);

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

            return metaValue + " : ";
        }

        private string GetToText(IEdge edge)
        {
            object toValue = edge?.To?.Value;

            if (toValue == null)
                return "[$Empty]";

            if (GeneralUtil.CompareStrings(toValue, ""))
                return "\"\"";

            return toValue.ToString();
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

        private void UpdateSelectionState()
        {
            if (IsSelected)
            {
                Background = GetForegroundBrush();
                contentPanel.Background = GetForegroundBrush();

                metaLabel.Foreground = GetBackgroundBrush();
                toLabel.Foreground = GetBackgroundBrush();
            }
            else
            {
                Background = Brushes.Transparent;
                contentPanel.Background = Brushes.Transparent;

                metaLabel.Foreground = GetMetaForegroundBrush();
                toLabel.Foreground = GetForegroundBrush();
            }

            metaLabel.Background = Brushes.Transparent;
            toLabel.Background = Brushes.Transparent;
        }

        private BitmapImage TryLoadIconBitmap(IEdge edge)
        {
            string iconPath = TryFindIconPath(edge);

            if (iconPath == null)
                return null;

            try
            {
                lock (IconCacheLock)
                {
                    if (BitmapByIconPath.TryGetValue(iconPath, out BitmapImage cachedBitmap))
                        return cachedBitmap;
                }

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(iconPath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();

                lock (IconCacheLock)
                    BitmapByIconPath[iconPath] = bitmap;

                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private string TryFindIconPath(IEdge edge)
        {
            string iconName = GetIconName(edge);

            if (string.IsNullOrWhiteSpace(iconName))
                return null;

            string iconDirectory = GetIconDirectory();

            if (string.IsNullOrWhiteSpace(iconDirectory) || Directory.Exists(iconDirectory) == false)
                return null;

            string normalizedIconName = iconName.Trim();

            lock (IconCacheLock)
            {
                Dictionary<string, string> requestedIconPathMap = GetRequestedIconPathMap(iconDirectory);

                if (requestedIconPathMap.TryGetValue(normalizedIconName, out string cachedResolvedPath))
                    return string.IsNullOrWhiteSpace(cachedResolvedPath) ? null : cachedResolvedPath;

                Dictionary<string, string> iconPathMap = GetIconPathMap(iconDirectory);

                if (iconPathMap.TryGetValue(normalizedIconName, out string exactMatch))
                {
                    requestedIconPathMap[normalizedIconName] = exactMatch;
                    return exactMatch;
                }

                string partialMatch = iconPathMap
                    .FirstOrDefault(iconPathItem => iconPathItem.Key.IndexOf(normalizedIconName, StringComparison.OrdinalIgnoreCase) >= 0)
                    .Value;

                requestedIconPathMap[normalizedIconName] = partialMatch ?? string.Empty;

                return partialMatch;
            }
        }

        private string GetIconDirectory()
        {
            MinusZero minusZero = MinusZero.Instance;

            string applicationPath = minusZero?.ApplicationPath;

            if (string.IsNullOrWhiteSpace(applicationPath))
                return null;

            string iconsRootDirectory = Path.Combine(applicationPath, "icons");

            string metaIconDirectoryName =
                minusZero?.Root?.Get(false, @"Home:\CurrentUser:\Settings:\MetaIconsDirectoryName:")?.Value?.ToString();

            if (string.IsNullOrWhiteSpace(metaIconDirectoryName))
                return iconsRootDirectory;

            return Path.Combine(iconsRootDirectory, metaIconDirectoryName);
        }

        private string GetIconName(IEdge edge)
        {
            string iconNameFromIsEdge = GetIconNameFromIsEdge(edge);

            if (string.IsNullOrWhiteSpace(iconNameFromIsEdge) == false)
                return iconNameFromIsEdge;

            object metaValue = edge?.Meta?.Value;

            if (metaValue == null)
                return null;

            return metaValue.ToString();
        }

        private static Dictionary<string, string> GetIconPathMap(string iconDirectory)
        {
            if (IconPathsByDirectory.TryGetValue(iconDirectory, out Dictionary<string, string> cachedIconPathMap))
                return cachedIconPathMap;

            Dictionary<string, string> iconPathMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string iconPath in Directory.GetFiles(iconDirectory, "*.png", SearchOption.TopDirectoryOnly))
            {
                string iconFileName = Path.GetFileNameWithoutExtension(iconPath);

                if (string.IsNullOrWhiteSpace(iconFileName) == false && iconPathMap.ContainsKey(iconFileName) == false)
                    iconPathMap.Add(iconFileName, iconPath);
            }

            IconPathsByDirectory[iconDirectory] = iconPathMap;

            return iconPathMap;
        }

        private static Dictionary<string, string> GetRequestedIconPathMap(string iconDirectory)
        {
            if (RequestedIconPathByDirectory.TryGetValue(iconDirectory, out Dictionary<string, string> cachedRequestedIconPathMap))
                return cachedRequestedIconPathMap;

            Dictionary<string, string> requestedIconPathMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            RequestedIconPathByDirectory[iconDirectory] = requestedIconPathMap;

            return requestedIconPathMap;
        }

        private string GetIconNameFromIsEdge(IEdge edge)
        {
            if (edge?.To == null)
                return null;

            IEdge isEdge = edge.To.OutEdges.FirstOrDefault(outEdge =>
                outEdge?.Meta?.Value != null &&
                (GeneralUtil.CompareStrings(outEdge.Meta.Value, "$Is") || GeneralUtil.CompareStrings(outEdge.Meta.Value, "$Is:")));

            if (isEdge?.To?.Value == null)
                return null;

            return isEdge.To.Value.ToString();
        }
    }
}
