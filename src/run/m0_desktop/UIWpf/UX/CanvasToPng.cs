using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace m0.UIWpf.UX
{
    public class CanvasToPng
    {
        static double dpi = 96;

        public static void SaveCanvasToPng(Canvas canvas, string filePath)
        {
            // Render canvas to bitmap
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)canvas.ActualWidth,
                (int)canvas.ActualHeight,
                dpi,
                dpi,
                PixelFormats.Pbgra32);

            renderBitmap.Render(canvas);

            // Convert to a more manageable format
            FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(
                renderBitmap,
                PixelFormats.Bgra32,
                null,
                0);

            // Get pixels
            int width = convertedBitmap.PixelWidth;
            int height = convertedBitmap.PixelHeight;
            int stride = width * 4; // 4 bytes per pixel (BGRA)
            byte[] pixels = new byte[height * stride];
            convertedBitmap.CopyPixels(pixels, stride, 0);

            // Replace fully transparent pixels with white (but keep semi-transparent for antialiasing)
            ReplaceTransparentWithWhite(pixels);

            // Find non-white area bounds
            var bounds = FindVisibleBounds(pixels, width, height, stride);

            if (bounds == null)
            {
                // Entire image is white
                SaveEmptyImage(filePath);
                return;
            }

            // Add 10 pixel border
            int padding = 10;
            int croppedWidth = bounds.Value.Right - bounds.Value.Left + 1 + (2 * padding);
            int croppedHeight = bounds.Value.Bottom - bounds.Value.Top + 1 + (2 * padding);

            // Create new bitmap with border
            WriteableBitmap croppedBitmap = new WriteableBitmap(
                croppedWidth,
                croppedHeight,
                dpi,
                dpi,
                PixelFormats.Bgra32,
                null);

            // Fill with white color
            byte[] whitePixels = new byte[croppedHeight * croppedWidth * 4];
            for (int i = 0; i < whitePixels.Length; i += 4)
            {
                whitePixels[i] = 255;     // B
                whitePixels[i + 1] = 255; // G
                whitePixels[i + 2] = 255; // R
                whitePixels[i + 3] = 255; // A
            }
            croppedBitmap.WritePixels(
                new Int32Rect(0, 0, croppedWidth, croppedHeight),
                whitePixels,
                croppedWidth * 4,
                0);

            // Copy cropped area
            int sourceWidth = bounds.Value.Right - bounds.Value.Left + 1;
            int sourceHeight = bounds.Value.Bottom - bounds.Value.Top + 1;
            byte[] croppedPixels = new byte[sourceHeight * sourceWidth * 4];

            for (int y = 0; y < sourceHeight; y++)
            {
                int sourceY = bounds.Value.Top + y;
                int sourceOffset = sourceY * stride + bounds.Value.Left * 4;
                int destOffset = y * sourceWidth * 4;
                Array.Copy(pixels, sourceOffset, croppedPixels, destOffset, sourceWidth * 4);
            }

            // Paste into center (with border)
            croppedBitmap.WritePixels(
                new Int32Rect(padding, padding, sourceWidth, sourceHeight),
                croppedPixels,
                sourceWidth * 4,
                0);

            // Save to file
            SaveBitmapToPng(croppedBitmap, filePath);
        }

        private static void ReplaceTransparentWithWhite(byte[] pixels)
        {
            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte a = pixels[i + 3];

                // Only replace fully transparent pixels (alpha near 0)
                // Keep semi-transparent pixels for proper antialiasing
                if (a < 10)
                {
                    pixels[i] = 255;     // B
                    pixels[i + 1] = 255; // G
                    pixels[i + 2] = 255; // R
                    pixels[i + 3] = 255; // A
                }
            }
        }

        private static (int Left, int Top, int Right, int Bottom)? FindVisibleBounds(
            byte[] pixels, int width, int height, int stride)
        {
            int left = width;
            int top = height;
            int right = -1;
            int bottom = -1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + x * 4;
                    byte b = pixels[offset];
                    byte g = pixels[offset + 1];
                    byte r = pixels[offset + 2];
                    byte a = pixels[offset + 3];

                    // Check if pixel is not white (considering semi-transparent pixels)
                    bool isVisible = (r < 250 || g < 250 || b < 250) && a > 10;

                    if (isVisible)
                    {
                        if (x < left) left = x;
                        if (x > right) right = x;
                        if (y < top) top = y;
                        if (y > bottom) bottom = y;
                    }
                }
            }

            if (right == -1)
                return null; // No visible pixels found

            return (left, top, right, bottom);
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
            // Create small white bitmap
            WriteableBitmap emptyBitmap = new WriteableBitmap(20, 20, dpi, dpi, PixelFormats.Bgra32, null);
            byte[] white = new byte[20 * 20 * 4];
            for (int i = 0; i < white.Length; i += 4)
            {
                white[i] = white[i + 1] = white[i + 2] = white[i + 3] = 255;
            }
            emptyBitmap.WritePixels(new Int32Rect(0, 0, 20, 20), white, 20 * 4, 0);
            SaveBitmapToPng(emptyBitmap, filePath);
        }
    }
}