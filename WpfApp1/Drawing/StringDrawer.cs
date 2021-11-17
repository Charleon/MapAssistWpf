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

using System.Drawing;
using MapAssist.Cache;
using SkiaSharp;

namespace MapAssist.Drawing
{
    public static class StringDrawer
    {
        public static void DrawString(SKCanvas skCanvas,
            string text,
            SKFont font,
            int fontSize,
            SKPoint location,
            SKColor color)
        {
            var paint = new SKPaint
            {
                IsAntialias = true,
                IsEmbeddedBitmapText = true,
                FilterQuality = SKFilterQuality.High,
                ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 5, 5, SKColors.Black)
            };
            paint.Color = color;
            skCanvas.DrawText(text, location.X, location.Y, font, paint);
        }

        public static void DrawString(SKCanvas skCanvas,
            string text,
            int fontSize,
            SKPoint location,
            SKColor color)
        {
            var paint = new SKPaint();
            paint.Color = color;
            SKFont font = FontCache.GetFont("Arial", fontSize);
            skCanvas.DrawText(text, location.X, location.Y, font, paint);
        }
    }
}
