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

using MapAssist.Drawing.Skia;
using System;
using System.Collections.Generic;
using System.Drawing;
using MapAssist.Types;
using MapAssist.Settings;
using MapAssist.Cache;
using MapAssist.Helpers;
using System.Windows.Forms;
using System.Numerics;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Diagnostics;

namespace MapAssist.Drawing
{
    public class HUDDrawer
    {
        int _renderScale;
        AreaData _areaData;
        SKPoint _cropOffset;
        SKPoint _bitmapPlayerPosition;
        MapAssistConfiguration _configuration;

        public HUDDrawer(MapAssistConfiguration configuration)
        {
            _configuration = configuration;
            _renderScale = 2;
        }

        public void DrawHUD(
            SKCanvas skCanvas,
            SKBitmap mapBackground,
            int backgroundScale,
            AreaData areaData,
            GameData gameData,
            SKPoint cropOffset,
            IReadOnlyList<PointOfInterest> pointsOfInterest)
        {
            _renderScale = backgroundScale;
            _areaData = areaData;
            _cropOffset = cropOffset;
            _bitmapPlayerPosition = WorldCoordinatesToMapBitmapPixelCoordinates(GetObjectPositionInWorld(gameData.PlayerPosition));

            //skCanvas.DrawBitmap(mapBackground, new SKPoint(0, 0), new SKPaint());
            using (var skMapCanvas = new SKCanvas(mapBackground))
            {
                DrawPlayer(skMapCanvas, new SKPoint(_bitmapPlayerPosition.X, _bitmapPlayerPosition.Y));
                DrawMonsters(skMapCanvas);
                DrawDestinationLines(skMapCanvas, pointsOfInterest);
            }

            if (_configuration.Rendering.OverlayMode)
            {
                //DrawMapToScreenOverlay(screenGraphics, mapBackground, gameData);
            }
            else
            {
                SKBitmap rotatedResult = _configuration.Rendering.Rotate ? ImageUtils.RotateImage(mapBackground, 53) : mapBackground;

                SKImageInfo resizeInfo = new SKImageInfo
                {
                    Width = _configuration.Rendering.Size,
                    Height = _configuration.Rendering.Size
                };

                var cropped = ImageUtils.CropBitmap(rotatedResult.ToBitmap());
                SKBitmap scaled = ImageUtils.ResizeImage(cropped, _configuration.Rendering.Size, _configuration.Rendering.Size);
                cropped.ScalePixels(scaled, SKFilterQuality.High);
                DrawMapToScreenFlat(skCanvas, scaled);
            }

            //DrawWarningTexts(screenGraphics);

            /*StringDrawer.DrawString(screenGraphics,
                $"Drawing position: {renderingPosition.X} {renderingPosition.Y}",
                24,
                new Point(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2),
                Color.Red);*/
        }

        void DrawMapToScreenFlat(SKCanvas skCanvas, SKBitmap mapScreen)
        {
            var renderingPosition = new SKPoint(_configuration.Rendering.X, _configuration.Rendering.Y);

            skCanvas.DrawBitmap(mapScreen, renderingPosition, new SKPaint
            {
                IsAntialias = true,
                FilterQuality = SKFilterQuality.High,
                ImageFilter = SKImageFilter.CreateDropShadow(0,0,10,10, SKColors.Black)
            });
        }

        void DrawMapToScreenOverlay(Graphics screenGraphics, Bitmap mapScreen, GameData gameData)
        {
            float width = 0;
            float height = 0;
            var scale = 0.0F;
            var center = new Vector2();



            width = Screen.PrimaryScreen.WorkingArea.Width;
            height = Screen.PrimaryScreen.WorkingArea.Height;
            scale = (1024.0F / height * width * 3f / 4f / 2.3F) * _configuration.Rendering.ZoomLevel;
            center = new Vector2(width / 2, height / 2 + 20);

            screenGraphics.SetClip(new RectangleF(0, 0, width, height));
            /*case MapPosition.TopLeft:
                width = 640;
                height = 360;
                scale = (1024.0F / height * width * 3f / 4f / 3.35F) * _configuration.Rendering.ZoomLevel;
                center = new Vector2(width / 2, (height / 2) + 48);

                screenGraphics.SetClip(new RectangleF(0, 50, width, height));
                break;
            case MapPosition.TopRight:
                width = 640;
                height = 360;
                scale = (1024.0F / height * width * 3f / 4f / 3.35F) * _configuration.Rendering.ZoomLevel;
                center = new Vector2(width / 2, (height / 2) + 40);

                screenGraphics.TranslateTransform(Screen.PrimaryScreen.WorkingArea.Width - width, -8);
                screenGraphics.SetClip(new RectangleF(0, 50, width, height));
                break;
        }*/

            SKPoint playerPosInArea = gameData.PlayerPosition.OffsetFrom(_areaData.Origin).OffsetFrom(_cropOffset).Multiply(_renderScale);

            var playerPos = new Vector2(playerPosInArea.X, playerPosInArea.Y);

            Vector2 Transform(Vector2 p) =>
                center +
                DeltaInWorldToMinimapDelta(
                    p - playerPos,
                    (float)Math.Sqrt(width * width + height * height),
                    scale,
                    0);

            var p1 = Transform(new Vector2(0, 0));
            var p2 = Transform(new Vector2(mapScreen.Width, 0));
            var p4 = Transform(new Vector2(0, mapScreen.Height));

            PointF[] destinationPoints = {
                    new PointF(p1.X, p1.Y),
                    new PointF(p2.X, p2.Y),
                    new PointF(p4.X, p4.Y)
                };

            screenGraphics.DrawImage(mapScreen, destinationPoints);
        }

        public Vector2 DeltaInWorldToMinimapDelta(Vector2 delta, double diag, float scale, float deltaZ = 0)
        {
            var CAMERA_ANGLE = -26F * 3.14159274F / 180;

            var cos = (float)(diag * Math.Cos(CAMERA_ANGLE) / scale);
            var sin = (float)(diag * Math.Sin(CAMERA_ANGLE) /
                               scale);

            return new Vector2((delta.X - delta.Y) * cos, deltaZ - (delta.X + delta.Y) * sin);
        }

        public Point GetConfiguredLocation(Size mapSize)
        {
            int xOffset = (int)(Screen.PrimaryScreen.WorkingArea.Width * 0.05);
            int yOffset = (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.1);

            return new Point(
                Screen.PrimaryScreen.WorkingArea.X + xOffset,
                Screen.PrimaryScreen.WorkingArea.Y + yOffset
                );
        }


        private SKPoint GetObjectPositionInWorld(Point objectPosition)
        {
            return objectPosition.OffsetFrom(_areaData.Origin);
        }

        private void DrawPlayer(SKCanvas skCanvas, SKPoint playerPositionInBitmap)
        {
            if (_configuration.Rendering.Player.CanDrawIcon())
            {
                SKBitmap playerIcon = IconCache.GetIcon(_configuration.Rendering.Player, _renderScale);
                skCanvas.DrawBitmap(playerIcon, playerPositionInBitmap);
            }
        }

        /*public void DrawWarningTexts(SKCanvas skCanvas)
        {
            var msgCount = 0;
            foreach (var warning in GameMemory.WarningMessages)
            {
                var fontSize = _configuration.Map.WarnImmuneNPCFontSize;
                Font font = FontCache.GetFont(_configuration.Map.WarnImmuneNPCFont, fontSize);
                StringDrawer.DrawString(graphics,
                    warning,
                    font,
                    fontSize,
                    _configuration.Map.WarnNPCHorizontalAlign,
                    _configuration.Map.WarnNPCVerticalAlign,
                    new Point(Screen.PrimaryScreen.WorkingArea.Width / 2, 10 + (msgCount * (fontSize + fontSize / 2))),
                    _configuration.Map.WarnNPCFontColor);
                msgCount++;
            }
        }*/

        private void DrawDestinationLines(SKCanvas skCanvas, IReadOnlyList<PointOfInterest> pointsOfInterest)
        {
            foreach (PointOfInterest poi in pointsOfInterest)
            {
                SKPoint poiBitmapPoiPosition = WorldCoordinatesToMapBitmapPixelCoordinates(poi.Position.OffsetFrom(_areaData.Origin));

                if (poi.RenderingSettings.CanDrawLine())
                {
                    if (poi.RenderingSettings.CanDrawArrowHead())
                    {
                        LineDrawer.DrawLine(skCanvas,
                            _bitmapPlayerPosition,
                            poiBitmapPoiPosition,
                            poi.RenderingSettings.LineColor,
                            Scale(poi.RenderingSettings.LineThickness),
                            Scale(poi.RenderingSettings.ArrowHeadSize));
                    }
                    else
                    {
                        LineDrawer.DrawLine(skCanvas,
                            _bitmapPlayerPosition,
                            poiBitmapPoiPosition,
                            poi.RenderingSettings.LineColor,
                            Scale(poi.RenderingSettings.LineThickness));
                    }
                }
            }
        }

        private float Scale(float input)
        {
            return input * _renderScale;
        }

        private int Scale(int input)
        {
            return input * _renderScale;
        }

        private void DrawMonsters(SKCanvas skCanvas)
        {
            MobRendering render = Utils.GetMobRendering();
            foreach (var monster in GameMemory.Monsters)
            {
                
                var sz = new SKSize(Scale(5), Scale(5));
                var sz2 = new SKSize(Scale(2), Scale(2));
                var midPoint = monster.Position.OffsetFrom(_areaData.Origin);
                var bitmapMidPoint = WorldCoordinatesToMapBitmapPixelCoordinates(midPoint);
                var rect = SKRect.Create(bitmapMidPoint, sz);
                skCanvas.DrawRect(rect, new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = monster.UniqueFlag == 0 ? render.NormalColor : render.UniqueColor,
                    StrokeWidth = 2,
                    IsAntialias = true,
                    PathEffect = SKPathEffect.CreateCorner(2f)
                });

                var i = 0;
                foreach (var immunity in monster.Immunities)
                {
                    var paint2 = new SKPaint();
                    paint2.Color = ResistColors.ResistColor[immunity];
                    var iPoint = new Point((i * -2) + (1 * (monster.Immunities.Count - 1)) - 1, 3);
                    var rect2 = SKRect.Create(bitmapMidPoint.OffsetFrom(iPoint), sz2);
                    skCanvas.DrawRect(rect2, paint2);
                    i++;
                }
            }
        }

        private SKPoint WorldCoordinatesToMapBitmapPixelCoordinates(SKPoint worldObjectCoordinate)
        {
            var positionAfterCrop = worldObjectCoordinate.OffsetFrom(_cropOffset);
            var scaledPosition = new SKPoint(Scale(positionAfterCrop.X), Scale(positionAfterCrop.Y));
            return scaledPosition;
        }
    }
}
