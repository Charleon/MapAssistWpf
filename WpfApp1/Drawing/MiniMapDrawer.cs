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
using MapAssist.Types;
using MapAssist.Settings;
using MapAssist.Cache;
using MapAssist.Helpers;
using SkiaSharp;
using System.Drawing;
using MapAssist.Drawing.Skia;

namespace MapAssist.Drawing
{
    public static class MiniMapDrawer
    {
        public static ((SKBitmap, SKPoint), int) DrawMiniMapBackground(AreaData areaData,
            MapAssistConfiguration configuration,
            IReadOnlyList<PointOfInterest> pointsOfInterest,
            int preferredRenderingWidth)
        {
            var miniMap = new SKBitmap(areaData.CollisionGrid[0].Length, areaData.CollisionGrid.Length);

            using (SKCanvas canvas = new SKCanvas(miniMap))
            {
                for (var y = 0; y < areaData.CollisionGrid.Length; y++)
                {
                    for (var x = 0; x < areaData.CollisionGrid[y].Length; x++)
                    {
                        int type = areaData.CollisionGrid[y][x];
                        SKColor? typeColor = configuration.MapColors.LookupMapColor(type);
                        if (typeColor.HasValue)
                        {
                            miniMap.SetPixel(x, y, typeColor.Value);
                        }
                    }
                }
                
                int mapWidth = miniMap.Width;
                int mapWidthDoubled = mapWidth * 2;
                int decidedSizeMultiplier = 1;
                
                if(Math.Abs(mapWidthDoubled - preferredRenderingWidth) < Math.Abs(mapWidth - preferredRenderingWidth) &&
                    !configuration.Rendering.OverlayMode)
                {
                    decidedSizeMultiplier = 2;
                }

                if(decidedSizeMultiplier != 1)
                {
                    SKImageInfo resizeInfo = new SKImageInfo(miniMap.Width * decidedSizeMultiplier, miniMap.Height * decidedSizeMultiplier);
                    miniMap = miniMap.Resize(resizeInfo, SKFilterQuality.High);
                }

                using (SKCanvas skCanvas = new SKCanvas(miniMap))
                {
                    foreach (PointOfInterest poi in pointsOfInterest)
                    {
                        SKPoint offset = new SKPoint(poi.Position.OffsetFrom(areaData.Origin).X * decidedSizeMultiplier, poi.Position.OffsetFrom(areaData.Origin).Y * decidedSizeMultiplier);
                        if (poi.RenderingSettings.CanDrawIcon())
                        {
                            SKBitmap icon = IconCache.GetIcon(poi.RenderingSettings, decidedSizeMultiplier);
                            SKPoint drawPosition = new SKPoint(offset.X - icon.Width / 2, offset.Y - icon.Height / 2);
                            skCanvas.DrawBitmap(icon, new SKPoint(drawPosition.X, drawPosition.Y), new SKPaint
                            {
                                IsAntialias = true,
                                FilterQuality = SKFilterQuality.High,
                                ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 10, 10, SKColors.Black)
                            });
                        }

                        if (!string.IsNullOrWhiteSpace(poi.Label) && poi.RenderingSettings.CanDrawLabel())
                        {
                            SKFont font = FontCache.GetFont(poi.RenderingSettings, decidedSizeMultiplier);
                            StringDrawer.DrawString(skCanvas,
                                poi.Label, 
                                font,
                                poi.RenderingSettings.LabelFontSize,
                                offset,
                                poi.RenderingSettings.LabelColor
                                );
                        }
                    }
                }
                return ((miniMap, new SKPoint(0,0)), decidedSizeMultiplier);
            }
        }
    }
}
