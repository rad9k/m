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
        /// <summary>
        /// Decode meta icons at a modest pixel width before WPF scales them to ~30 logical DIP.
        /// Loading full 256px sources and scaling with certain modes caused visible gray halos on alpha.
        /// </summary>
        private const int MetaIconDecodeMaxSidePixels = 128;

        private static readonly object IconCacheLock = new object();
        private static readonly Dictionary<string, Dictionary<string, string>> IconPathsByDirectory =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Dictionary<string, string>> RequestedIconPathByDirectory =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, BitmapImage> BitmapByIconPath =
            new Dictionary<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);

        private static bool MetaIconDirectoryNameMissingLogged;

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

            if (GeneralUtil.CompareStrings(edge?.Meta?.Value, "$Empty") && GeneralUtil.CompareStrings(toValue, "$Empty"))
                return "[$Empty]";

            if (GeneralUtil.CompareStrings(edge?.Meta?.Value, "$Empty") && GeneralUtil.CompareStrings(toValue, ""))
                return "[$Empty]";

            if (GeneralUtil.CompareStrings(toValue, ""))
                return "";

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
                Background = null;
                contentPanel.Background = null;

                metaLabel.Foreground = GetMetaForegroundBrush();
                toLabel.Foreground = GetForegroundBrush();
            }

            metaLabel.Background = null;
            toLabel.Background = null;
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
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmap.DecodePixelWidth = MetaIconDecodeMaxSidePixels;
                bitmap.UriSource = new Uri(iconPath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();

                lock (IconCacheLock)
                    BitmapByIconPath[iconPath] = bitmap;

                MinusZero.Instance.Log(1, "MetaToEdgeControl", "loaded icon '" + iconPath + "' pixelFormat=" + bitmap.Format + " size=" + bitmap.PixelWidth + "x" + bitmap.PixelHeight);

                return bitmap;
            }
            catch (Exception iconLoadException)
            {
                MinusZero.Instance.Log(1, "MetaToEdgeControl", "failed to load icon '" + iconPath + "': " + iconLoadException.Message);
                return null;
            }
        }

        private string TryFindIconPath(IEdge edge)
        {
            List<string> iconNames = GetIconNameCandidates(edge);

            if (iconNames.Count == 0)
                return null;

            string iconDirectory = GetIconDirectory();

            if (string.IsNullOrWhiteSpace(iconDirectory) || Directory.Exists(iconDirectory) == false)
                return null;

            lock (IconCacheLock)
            {
                Dictionary<string, string> requestedIconPathMap = GetRequestedIconPathMap(iconDirectory);
                Dictionary<string, string> iconPathMap = GetIconPathMap(iconDirectory);

                foreach (string iconName in iconNames)
                {
                    string iconPath = TryResolveIconPath(iconDirectory, requestedIconPathMap, iconPathMap, iconName);

                    if (iconPath != null)
                        return iconPath;
                }

                return null;
            }
        }

        private string TryResolveIconPath(
            string iconDirectory,
            Dictionary<string, string> requestedIconPathMap,
            Dictionary<string, string> iconPathMap,
            string iconName)
        {
            if (string.IsNullOrWhiteSpace(iconName))
                return null;

            string normalizedIconName = iconName.Trim();

            if (requestedIconPathMap.TryGetValue(normalizedIconName, out string cachedResolvedPath))
                return string.IsNullOrWhiteSpace(cachedResolvedPath) ? null : cachedResolvedPath;

            iconPathMap.TryGetValue(normalizedIconName, out string exactMatch);

            requestedIconPathMap[normalizedIconName] = exactMatch ?? string.Empty;

            if (exactMatch == null)
                MinusZero.Instance.Log(1, "MetaToEdgeControl", "no icon match for '" + normalizedIconName + "' in '" + iconDirectory + "'");
            else
                MinusZero.Instance.Log(1, "MetaToEdgeControl", "resolved icon '" + normalizedIconName + "' -> '" + exactMatch + "'");

            return exactMatch;
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

            return Path.Combine(iconsRootDirectory, metaIconDirectoryName);
        }

        private List<string> GetIconNameCandidates(IEdge edge)
        {
            List<string> iconNames = new List<string>();

            object metaValue = edge?.Meta?.Value;
            bool isMetaEmpty = GeneralUtil.CompareStrings(metaValue, "$Empty");

            if (metaValue != null && isMetaEmpty == false)
                AddIconNameCandidate(iconNames, metaValue.ToString());

            if (edge?.To?.Value != null)
                AddIconNameCandidate(iconNames, edge.To.Value.ToString());

            if (isMetaEmpty)
                AddIconNameCandidate(iconNames, "$Empty");

            AddIconNameCandidate(iconNames, GetIconNameFromIsEdge(edge));

            return iconNames;
        }

        private void AddIconNameCandidate(List<string> iconNames, string iconName)
        {
            if (string.IsNullOrWhiteSpace(iconName))
                return;

            if (iconNames.Any(existingIconName => GeneralUtil.CompareStrings(existingIconName, iconName)))
                return;

            iconNames.Add(iconName);
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

            MinusZero.Instance.Log(1, "MetaToEdgeControl", "indexed icon directory '" + iconDirectory + "' fileCount=" + iconPathMap.Count);

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
