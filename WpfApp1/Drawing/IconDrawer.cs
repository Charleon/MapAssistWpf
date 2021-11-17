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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using MapAssist.Types;
using MapAssist.Settings;
using SkiaSharp;

namespace MapAssist.Drawing
{
    public static class IconDrawer
    {
        public static SKBitmap GenerateIcon(PointOfInterestRendering poiSettings, int sizeMultiplier)
        {
            var iconSize = poiSettings.IconSize * sizeMultiplier;
            var bitmap = new SKBitmap(iconSize, iconSize);
            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                var paint = new SKPaint();
                paint.Color = poiSettings.IconColor;

                switch (poiSettings.IconShape)
                {

                    case Shape.Ellipse:
                        canvas.DrawCircle(iconSize/2, iconSize / 2, iconSize, paint);
                        break;
                    case Shape.Rectangle:
                        canvas.DrawRect(0, 0, iconSize, iconSize, paint);
                        break;
                }
            }
            return bitmap;
        }
    }
}
