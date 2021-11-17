/**
 *   Copyright (C) 2021 okaygo
 *
 *   https://github.com/misterokaygo/MapAssist/
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 **/

using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace MapAssist.Helpers
{
    public static class ImageUtils
    {

        public static SKBitmap RotateImage(SKBitmap bitmap, double angle)
        {
            double radians = Math.PI * angle / 180;
            float sine = (float)Math.Abs(Math.Sin(radians));
            float cosine = (float)Math.Abs(Math.Cos(radians));
            int originalWidth = bitmap.Width;
            int originalHeight = bitmap.Height;
            int rotatedWidth = (int)(cosine * originalWidth + sine * originalHeight);
            int rotatedHeight = (int)(cosine * originalHeight + sine * originalWidth);

            var rotatedBitmap = new SKBitmap(rotatedWidth, rotatedHeight);

            using (var surface = new SKCanvas(rotatedBitmap))
            {
                surface.Translate(rotatedWidth / 2, rotatedHeight / 2);
                surface.RotateDegrees((float)angle);
                surface.Translate(-originalWidth / 2, -originalHeight / 2);
                surface.DrawBitmap(bitmap, new SKPoint());
            }
            return rotatedBitmap;
        }

        /// <summary>
        /// TODO: Figure out how to do this with Skaia, since we don't want conversion between frameworks if we can avoid it
        /// </summary>
        public static SKBitmap CropBitmap(Bitmap originalBitmap)
        {
            // Find the min/max non-white/transparent pixels
            var min = new Point(int.MaxValue, int.MaxValue);
            var max = new Point(int.MinValue, int.MinValue);

            unsafe
            {
                var bData = originalBitmap.LockBits(new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                byte bitsPerPixel = 32;
                byte* scan0 = (byte*)bData.Scan0.ToPointer();

                for (int y = 0; y < bData.Height; ++y)
                {
                    for (int x = 0; x < bData.Width; ++x)
                    {
                        byte* data = scan0 + y * bData.Stride + x * bitsPerPixel / 8;
                        // data[0 = blue, 1 = green, 2 = red, 3 = alpha]
                        if (data[3] == byte.MaxValue)
                        {
                            if (x < min.X) min.X = x;
                            if (y < min.Y) min.Y = y;

                            if (x > max.X) max.X = x;
                            if (y > max.Y) max.Y = y;
                        }
                    }
                }
                originalBitmap.UnlockBits(bData);
            }

            // Create a new bitmap from the crop rectangle
            var cropRectangle = new Rectangle(min.X, min.Y, max.X - min.X, max.Y - min.Y);
            var newBitmap = new Bitmap(cropRectangle.Width, cropRectangle.Height);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.DrawImage(originalBitmap, 0, 0, cropRectangle, GraphicsUnit.Pixel);
            }

            return newBitmap.ToSKBitmap();
        }


        public static SKBitmap ResizeImage(SKBitmap bitmap, int width, int height, bool keepAspectRatio = true)
        {
            var size = new SKSize(bitmap.Width, bitmap.Height).ResizeKeepAspect(width, height);
            SKBitmap scaled = new SKBitmap((int)size.Width, (int)size.Height);
            bitmap.ScalePixels(scaled, SKFilterQuality.High);

            return scaled;
        }

        public static SKSize ResizeKeepAspect(this SKSize src, int maxWidth, int maxHeight, bool enlarge = false)
        {
            maxWidth = enlarge ? maxWidth : (int)Math.Min(maxWidth, src.Width);
            maxHeight = enlarge ? maxHeight : (int)Math.Min(maxHeight, src.Height);

            double rnd = Math.Min(maxWidth / (double)src.Width, maxHeight / (double)src.Height);
            return new SKSize((int)Math.Round(src.Width * rnd), (int)Math.Round(src.Height * rnd));
        }

        public static Bitmap CreateFilledRectangle(Color color, int width, int height)
        {
            var rectangle = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(rectangle);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillRectangle(new SolidBrush(color), 0, 0, width, height);
            graphics.Dispose();
            return rectangle;
        }

        public static Bitmap CreateFilledEllipse(Color color, int width, int height)
        {
            var ellipse = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(ellipse);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillEllipse(new SolidBrush(color), 0, 0, width, height);
            graphics.Dispose();
            return ellipse;
        }
    }
}
