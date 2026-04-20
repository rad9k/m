using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace m0.UIWpf.UX
{
    public class CanvasToPng
    {
        static double dpi = 192;
        const double BaseDpi = 96.0;
        const int FinalPaddingPixels = 10;
        const int RenderSafetyMarginDip = 20;
        const int EmptyImageSizeDip = 20;

        public static void SaveCanvasToPng(Canvas canvas, string filePath)
        {
            double dpiScale = dpi / BaseDpi;
            canvas.UpdateLayout();

            Rect contentBounds = GetCanvasContentBounds(canvas);
            if (contentBounds.IsEmpty || contentBounds.Width <= 0 || contentBounds.Height <= 0)
            {
                SaveEmptyImage(filePath);
                return;
            }

            contentBounds.Inflate(RenderSafetyMarginDip, RenderSafetyMarginDip);

            int renderWidthPixels = Math.Max(1, (int)Math.Ceiling(contentBounds.Width * dpiScale));
            int renderHeightPixels = Math.Max(1, (int)Math.Ceiling(contentBounds.Height * dpiScale));

            RenderTargetBitmap canvasBitmap = RenderCanvasWithTranslation(
                canvas,
                renderWidthPixels,
                renderHeightPixels,
                -contentBounds.X,
                -contentBounds.Y);

            BitmapSource compositedBitmap = CompositePbgra32OverWhite(canvasBitmap);
            BitmapSource croppedBitmap = CropToVisibleContent(canvasBitmap, compositedBitmap, FinalPaddingPixels);

            SaveBitmapToPng(croppedBitmap, filePath);
        }

        private static RenderTargetBitmap RenderCanvasWithTranslation(
            Canvas canvas,
            int pixelWidth,
            int pixelHeight,
            double translateX,
            double translateY)
        {
            RenderTargetBitmap target = new RenderTargetBitmap(
                pixelWidth,
                pixelHeight,
                dpi,
                dpi,
                PixelFormats.Pbgra32);

            Transform savedRenderTransform = canvas.RenderTransform;
            canvas.RenderTransform = new TranslateTransform(translateX, translateY);
            try
            {
                target.Render(canvas);
            }
            finally
            {
                canvas.RenderTransform = savedRenderTransform;
            }

            return target;
        }

        private static BitmapSource CompositePbgra32OverWhite(RenderTargetBitmap source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * 4;

            byte[] sourcePixels = new byte[height * stride];
            source.CopyPixels(sourcePixels, stride, 0);

            byte[] resultPixels = new byte[height * stride];

            for (int i = 0; i < resultPixels.Length; i += 4)
            {
                byte sourceAlpha = sourcePixels[i + 3];
                int invertedAlpha = 255 - sourceAlpha;

                resultPixels[i + 0] = (byte)Math.Min(255, sourcePixels[i + 0] + invertedAlpha);
                resultPixels[i + 1] = (byte)Math.Min(255, sourcePixels[i + 1] + invertedAlpha);
                resultPixels[i + 2] = (byte)Math.Min(255, sourcePixels[i + 2] + invertedAlpha);
                resultPixels[i + 3] = 255;
            }

            WriteableBitmap result = new WriteableBitmap(width, height, dpi, dpi, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, width, height), resultPixels, stride, 0);
            return result;
        }

        private static BitmapSource CropToVisibleContent(RenderTargetBitmap sourceWithAlpha, BitmapSource compositedBitmap, int paddingPixels)
        {
            int width = sourceWithAlpha.PixelWidth;
            int height = sourceWithAlpha.PixelHeight;
            int stride = width * 4;

            byte[] sourcePixels = new byte[height * stride];
            sourceWithAlpha.CopyPixels(sourcePixels, stride, 0);

            int left = width;
            int top = height;
            int right = -1;
            int bottom = -1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + (x * 4);
                    byte alpha = sourcePixels[offset + 3];

                    if (alpha == 0)
                        continue;

                    if (x < left) left = x;
                    if (x > right) right = x;
                    if (y < top) top = y;
                    if (y > bottom) bottom = y;
                }
            }

            if (right < left || bottom < top)
                return compositedBitmap;

            Int32Rect cropRect = new Int32Rect(
                left,
                top,
                right - left + 1,
                bottom - top + 1);

            CroppedBitmap croppedBitmap = new CroppedBitmap(compositedBitmap, cropRect);
            return AddWhitePadding(croppedBitmap, paddingPixels);
        }

        private static BitmapSource AddWhitePadding(BitmapSource source, int paddingPixels)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int paddedWidth = width + (2 * paddingPixels);
            int paddedHeight = height + (2 * paddingPixels);
            int sourceStride = width * 4;
            int paddedStride = paddedWidth * 4;

            byte[] sourcePixels = new byte[height * sourceStride];
            source.CopyPixels(sourcePixels, sourceStride, 0);

            byte[] paddedPixels = new byte[paddedHeight * paddedStride];

            for (int i = 0; i < paddedPixels.Length; i += 4)
            {
                paddedPixels[i + 0] = 255;
                paddedPixels[i + 1] = 255;
                paddedPixels[i + 2] = 255;
                paddedPixels[i + 3] = 255;
            }

            for (int y = 0; y < height; y++)
            {
                int sourceOffset = y * sourceStride;
                int destinationOffset = ((y + paddingPixels) * paddedStride) + (paddingPixels * 4);
                Array.Copy(sourcePixels, sourceOffset, paddedPixels, destinationOffset, sourceStride);
            }

            WriteableBitmap result = new WriteableBitmap(paddedWidth, paddedHeight, dpi, dpi, PixelFormats.Bgra32, null);
            result.WritePixels(new Int32Rect(0, 0, paddedWidth, paddedHeight), paddedPixels, paddedStride, 0);
            return result;
        }

        private static Rect GetCanvasContentBounds(Canvas canvas)
        {
            Rect bounds = Rect.Empty;

            foreach (UIElement child in canvas.Children)
            {
                if (child == null || child.Visibility != Visibility.Visible)
                    continue;

                Rect childBounds = VisualTreeHelper.GetDescendantBounds(child);

                if (childBounds.IsEmpty)
                    childBounds = new Rect(new Point(0, 0), child.RenderSize);

                if (childBounds.IsEmpty || childBounds.Width <= 0 || childBounds.Height <= 0)
                    continue;

                GeneralTransform transform = child.TransformToAncestor(canvas);
                Rect transformedBounds = transform.TransformBounds(childBounds);

                if (transformedBounds.IsEmpty || transformedBounds.Width <= 0 || transformedBounds.Height <= 0)
                    continue;

                if (bounds.IsEmpty)
                    bounds = transformedBounds;
                else
                    bounds.Union(transformedBounds);
            }

            return bounds;
        }

        private static void SaveBitmapToPng(BitmapSource bitmap, string filePath)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }

        private static void SaveEmptyImage(string filePath)
        {
            double dpiScale = dpi / BaseDpi;
            int emptyImageSize = Math.Max(1, (int)Math.Round(EmptyImageSizeDip * dpiScale));
            WriteableBitmap emptyBitmap = new WriteableBitmap(emptyImageSize, emptyImageSize, dpi, dpi, PixelFormats.Bgra32, null);
            byte[] white = new byte[emptyImageSize * emptyImageSize * 4];
            for (int i = 0; i < white.Length; i += 4)
            {
                white[i] = white[i + 1] = white[i + 2] = white[i + 3] = 255;
            }
            emptyBitmap.WritePixels(new Int32Rect(0, 0, emptyImageSize, emptyImageSize), white, emptyImageSize * 4, 0);
            SaveBitmapToPng(emptyBitmap, filePath);
        }
    }
}
