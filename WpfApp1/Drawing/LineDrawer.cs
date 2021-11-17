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
using System.Drawing.Drawing2D;
using SkiaSharp;

namespace MapAssist.Drawing
{
    public static class LineDrawer
    {
        public static void DrawLine(SKCanvas skCanvas,
            SKPoint from,
            SKPoint to,
            SKColor lineColor,
            float lineThickness,
            float arrowHeadSize
            )
        {
            // Skia has no native support for arrowheads

            DrawLine(skCanvas, from, to, lineColor, lineThickness);
        }

        public static void DrawLine(SKCanvas skCanvas,
            SKPoint from,
            SKPoint to,
            SKColor lineColor,
            float lineThickness)
        {
            var paint = new SKPaint()
            {
                Color = lineColor,
                StrokeWidth = lineThickness,
                IsAntialias = true
            };

            skCanvas.DrawLine(from, to, paint);
        }
    }
}
