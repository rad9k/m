using m0.Foundation;
using m0.Graph;
using m0.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace m0.UIWpf
{
    public static class IconServer
    {
        /// <summary>
        /// Decode meta icons at a modest pixel width before WPF scales them to ~30 logical DIP.
        /// Loading full 256px sources and scaling with certain modes caused visible gray halos on alpha.
        /// </summary>
        private const int IconDecodeMaxSidePixels = 128;

        private static readonly object IconCacheLock = new object();
        private static readonly Dictionary<string, BitmapImage> BitmapByIconPath =
            new Dictionary<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Dictionary<string, string>> RequestedIconPathByDirectory =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        private static bool MetaIconDirectoryNameMissingLogged;

        public static BitmapImage GetIconByVertex(IVertex vertex)
        {
            BitmapImage iconByIsEdge = GetIconByString(GetIconNameFromIsEdge(vertex));

            if (iconByIsEdge != null)
                return iconByIsEdge;

            return GetIconByString("$Empty");
        }

        public static BitmapImage GetIconByString(string iconName)
        {
            string iconPath = TryFindIconPath(iconName);

            if (iconPath == null)
                return null;

            return LoadIconBitmap(iconPath);
        }

        private static BitmapImage LoadIconBitmap(string iconPath)
        {
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
                bitmap.DecodePixelWidth = IconDecodeMaxSidePixels;
                bitmap.UriSource = new Uri(iconPath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();

                lock (IconCacheLock)
                    BitmapByIconPath[iconPath] = bitmap;

                MinusZero.Instance.Log(1, "IconServer", "loaded icon '" + iconPath + "' pixelFormat=" + bitmap.Format + " size=" + bitmap.PixelWidth + "x" + bitmap.PixelHeight);

                return bitmap;
            }
            catch (Exception iconLoadException)
            {
                MinusZero.Instance.Log(1, "IconServer", "failed to load icon '" + iconPath + "': " + iconLoadException.Message);
                return null;
            }
        }

        private static string TryFindIconPath(string iconName)
        {
            if (string.IsNullOrWhiteSpace(iconName))
                return null;

            string normalizedIconName = iconName.Trim();

            string iconDirectory = GetIconDirectory();

            if (string.IsNullOrWhiteSpace(iconDirectory) || Directory.Exists(iconDirectory) == false)
                return null;

            lock (IconCacheLock)
            {
                Dictionary<string, string> requestedIconPathMap = GetRequestedIconPathMap(iconDirectory);

                if (requestedIconPathMap.TryGetValue(normalizedIconName, out string cachedResolvedPath))
                    return string.IsNullOrWhiteSpace(cachedResolvedPath) ? null : cachedResolvedPath;
            }

            string iconPath = Path.Combine(iconDirectory, normalizedIconName + ".png");
            string resolvedIconPath = File.Exists(iconPath) ? iconPath : null;

            lock (IconCacheLock)
                GetRequestedIconPathMap(iconDirectory)[normalizedIconName] = resolvedIconPath ?? string.Empty;

            if (resolvedIconPath == null)
                MinusZero.Instance.Log(1, "IconServer", "no icon match for '" + normalizedIconName + "' in '" + iconDirectory + "'");
            else
                MinusZero.Instance.Log(1, "IconServer", "resolved icon '" + normalizedIconName + "' -> '" + resolvedIconPath + "'");

            return resolvedIconPath;
        }

        private static Dictionary<string, string> GetRequestedIconPathMap(string iconDirectory)
        {
            if (RequestedIconPathByDirectory.TryGetValue(iconDirectory, out Dictionary<string, string> cachedRequestedIconPathMap))
                return cachedRequestedIconPathMap;

            Dictionary<string, string> requestedIconPathMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            RequestedIconPathByDirectory[iconDirectory] = requestedIconPathMap;

            return requestedIconPathMap;
        }

        private static string GetIconDirectory()
        {
            MinusZero minusZero = MinusZero.Instance;

            string applicationPath = minusZero?.ApplicationPath;

            if (string.IsNullOrWhiteSpace(applicationPath))
                return null;

            string metaIconDirectoryName =
                minusZero?.Root?.Get(false, @"Home:\CurrentUser:\Settings:\MetaIconsDirectoryName:")?.Value?.ToString();

            if (string.IsNullOrWhiteSpace(metaIconDirectoryName))
            {
                if (MetaIconDirectoryNameMissingLogged == false)
                {
                    MinusZero.Instance.Log(1, "IconServer", "meta icon directory name is not configured");
                    MetaIconDirectoryNameMissingLogged = true;
                }

                return null;
            }

            return Path.Combine(applicationPath, "icons", metaIconDirectoryName);
        }

        private static string GetIconNameFromIsEdge(IVertex vertex)
        {
            if (vertex == null)
                return null;

            IEdge isEdge = vertex.OutEdges.FirstOrDefault(outEdge =>
                outEdge?.Meta?.Value != null &&
                (GeneralUtil.CompareStrings(outEdge.Meta.Value, "$Is") || GeneralUtil.CompareStrings(outEdge.Meta.Value, "$Is:")));

            if (isEdge?.To?.Value == null)
                return null;

            return isEdge.To.Value.ToString();
        }
    }
}
