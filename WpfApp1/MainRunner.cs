using Gma.System.MouseKeyHook;
using MapAssist.Drawing;
using MapAssist.Helpers;
using MapAssist.Settings;
using MapAssist.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MapAssist
{
    public class MainRunner
    {
        MapAssistConfiguration _configuration;
        WriteableBitmap _canvasImage;
        DispatcherTimer _timer;
        private MapApi _mapApi;
        GameData _currentGameData;
        private AreaData _areaData;
        OverlayDrawer _overlayDrawer;
        WindowWrapper _windowWrapper;
        Screen _gameScreen;

        public MainRunner(MapAssistConfiguration configuration, IKeyboardMouseEvents keyboardMouseEvents, Window windowHandle)
        {
            _windowWrapper = new WindowWrapper(windowHandle);
            _configuration = configuration;
            _timer = new DispatcherTimer();
            _overlayDrawer = new OverlayDrawer(configuration);
            keyboardMouseEvents.KeyPress += HandleInput;
        }

        public void Run(WriteableBitmap canvasImage)
        {
            _canvasImage = canvasImage;
            _timer.Interval = new TimeSpan(0, 0, 0, 0, _configuration.Map.UpdateTime);
            _timer.Tick += new EventHandler(UpdateLoop_Tick);
            _timer.Start();

            Initialize();
        }

        private void Initialize()
        {
        }

        private void HandleInput(object sender, KeyPressEventArgs keyPressEvent)
        {

        }

        public void UpdateLoop_Tick(object sender, EventArgs e)
        {
            GameData gameData = GameMemory.GetGameData(_configuration);

            if (gameData != null)
            {
                if (gameData.HasGameChanged(_currentGameData))
                {
                    Console.WriteLine($"Game changed: {gameData}");
                    _mapApi?.Dispose();
                    _mapApi = new MapApi(gameData.Difficulty, gameData.MapSeed, _configuration);
                }

                if (gameData.HasMapChanged(_currentGameData))
                {
                    Debug.WriteLine($"Area changed: {gameData.Area}");
                    if (gameData.Area != Area.None)
                    {
                        _areaData = _mapApi.GetMapData(gameData.Area);
                    }
                }

                if (_mapApi != null && _areaData != null)
                {
                    List<PointOfInterest> pointsOfInterest = PointOfInterestHandler.Get(_mapApi, _areaData, _configuration);
                    _overlayDrawer.UpdateGameAndAreaData(_currentGameData, _areaData, pointsOfInterest);
                }

            }

            _currentGameData = gameData;

            if (_currentGameData != null)
            {
                _gameScreen = Screen.FromHandle(_currentGameData.MainWindowHandle);
                _windowWrapper.SyncWithGame(_gameScreen.WorkingArea);
                //_canvasImage.Width = _canvasImage.MaxWidth = _windowWrapper.Width;
                //_canvasImage.Height = _canvasImage.MaxWidth = _windowWrapper.Width;
                //_canvasImage.Height = _canvasImage.MaxHeight = _windowWrapper.Height;
            }

            if (_mapApi != null && _areaData != null)
            {
                List<PointOfInterest> pointsOfInterest = PointOfInterestHandler.Get(_mapApi, _areaData, _configuration);
                _overlayDrawer.UpdateGameAndAreaData(_currentGameData, _areaData, pointsOfInterest);
                _overlayDrawer.Draw(this, _canvasImage, new System.Drawing.Size(_windowWrapper.Width, _windowWrapper.Height));
            }
        }

    }
}
