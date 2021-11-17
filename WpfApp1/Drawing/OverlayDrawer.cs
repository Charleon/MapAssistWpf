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
using MapAssist.Settings;
using MapAssist.Types;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;

namespace MapAssist.Drawing
{
    public class OverlayDrawer
    {
        private readonly MapAssistConfiguration _configuration;
        GameData _gameData;
        AreaData _areaData;
        SKBitmap _currentMap;
        int _generatedMapScale;
        SKPoint _offsetAfterCrop;
        Area _currentArea;
        IReadOnlyList<PointOfInterest> _pointsOfInterest;
        HUDDrawer _hudDrawer;
        public OverlayDrawer(MapAssistConfiguration configuration) : base()
        {
            _configuration = configuration;
            _hudDrawer = new HUDDrawer(configuration);
        }

        /// <summary>
        /// Draws to the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Draw(object sender, WriteableBitmap writeableBitmap, System.Drawing.Size drawableAreaSize)
        {
            if (_gameData != null && _areaData.Area == _gameData.Area)
            {
                var surface = SkiaWrapper.CreateSurfaceFromWriteableBitmap(writeableBitmap);
                SKCanvas skCanvas = surface.Canvas;
                skCanvas.Clear(new SKColor(0, 0, 0, 0));
                SKBitmap mapLayer = _currentMap.Copy();

                try
                {
                    _hudDrawer.DrawHUD(
                        skCanvas,
                        mapLayer,
                        _generatedMapScale,
                        _areaData,
                        _gameData,
                        _offsetAfterCrop,
                        _pointsOfInterest);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Tried to draw on SK Canvas but failed!");

                    writeableBitmap.Finalize();
                    surface.Dispose();

                    return;
                }


                writeableBitmap.Finalize();
                surface.Dispose();
            }
        }

        public void UpdateGameAndAreaData(GameData gameData, AreaData areaData, IReadOnlyList<PointOfInterest> pointsOfInterest)
        {
            _gameData = gameData;
            _areaData = areaData;
            _pointsOfInterest = pointsOfInterest;

            if (_gameData != null && _areaData != null)
            {
                // Only Regenerate map if we have switched map since last render.
                if (_currentArea != areaData.Area)
                {
                    ((_currentMap, _offsetAfterCrop), _generatedMapScale) = MiniMapDrawer.DrawMiniMapBackground(_areaData, _configuration, _pointsOfInterest, _configuration.Rendering.Size);
                    _offsetAfterCrop = new SKPoint(_offsetAfterCrop.X / _generatedMapScale, _offsetAfterCrop.Y / _generatedMapScale);
                    _currentArea = areaData.Area;
                }
            }
        }
    }
}
