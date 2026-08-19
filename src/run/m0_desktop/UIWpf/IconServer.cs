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
        private static readonly Dictionary<string, Dictionary<string, string>> IconPathsByDirectory =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, BitmapImage> BitmapByIconPath =
            new Dictionary<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Dictionary<string, string>> RequestedIconPathByDirectory =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public static BitmapImage GetIconByVertex(IVertex vertex)
        {
            BitmapImage iconByIsEdge = GetIconByString(GetIconNameFromIsEdge(vertex));

            if (iconByIsEdge != null)
                return iconByIsEdge;

            return GetIconByString("$Empty");
        }

        public static BitmapImage GetIconByEdge(IEdge edge)
        {
            string iconPath = TryFindIconPath(edge);

            BitmapImage icon = iconPath != null ? LoadIconBitmap(iconPath) : null;

            if (icon != null)
                return icon;

            return GetIconByString("$Empty");
        }

        public static BitmapImage GetIconByString(string iconName)
        {
            string iconPath = TryFindIconPath(iconName);

            if (iconPath == null)
                return null;

            return LoadIconBitmap(iconPath);
        }

        private static string TryFindIconPath(IEdge edge)
        {
            List<string> iconNames = GetIconNameCandidates(edge);

            if (iconNames.Count == 0)
                return null;

            return TryFindIconPath(iconNames);
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

                return bitmap;
            }
            catch (Exception iconLoadException)
            {
                return null;
            }
        }

        private static string TryFindIconPath(string iconName)
        {
            if (string.IsNullOrWhiteSpace(iconName))
                return null;

            return TryFindIconPath(new List<string> { iconName });
        }

        private static string TryFindIconPath(List<string> iconNames)
        {
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

        private static string TryResolveIconPath(
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

            return exactMatch;
        }

        private static List<string> GetIconNameCandidates(IEdge edge)
        {
            List<string> iconNames = new List<string>();

            object metaValue = edge?.Meta?.Value;
            bool isMetaEmpty = GeneralUtil.CompareStrings(metaValue, "$Empty");

            if (metaValue != null && isMetaEmpty == false)
                AddIconNameCandidate(iconNames, metaValue.ToString());

            if (edge?.To?.Value != null)
                AddIconNameCandidate(iconNames, edge.To.Value.ToString());

            AddIconNameCandidate(iconNames, GetIconNameFromIsEdge(edge?.To));

            if (isMetaEmpty)
                AddIconNameCandidate(iconNames, "$Empty");

            return iconNames;
        }

        private static void AddIconNameCandidate(List<string> iconNames, string iconName)
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
                return null;
            }

            return Path.Combine(applicationPath, "icons", metaIconDirectoryName);
        }

        private static string GetIconNameFromIsEdge(IVertex vertex)
        {
            if (vertex == null)
                return null;

            IList<IEdge> isEdges = GraphUtil.GetQueryOut(vertex, "$Is", null);

            if (isEdges.Count == 0)
                isEdges = GraphUtil.GetQueryOut(vertex, "$Is:", null);

            IEdge isEdge = isEdges.FirstOrDefault();

            if (isEdge?.To?.Value == null)
                return null;

            return isEdge.To.Value.ToString();
        }
    }
}
