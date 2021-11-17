using System.Drawing;
using System.Windows;
using System.Windows.Media;
using Point = System.Drawing.Point;
using Color = System.Windows.Media.Color;

namespace MapAssist
{
    public class WindowWrapper
    {
        Window _window;
        Rectangle _previousGameWorkingAreaPosition;

        public Point Position { 
            get => new Point((int)_window.Left, (int)_window.Top); 
            set
            {
                _window.Left = value.X;
                _window.Top = value.Y;
            } 
        }

        public int Width { get => (int)_window.Width; set => _window.Width = value; }
        public int Height { get => (int)_window.Height; set => _window.Height = value; }

        public double Opacity { get => _window.Opacity; set => _window.Opacity = value; }

        public void SyncWithGame(Rectangle rect)
        {
            if (rect != _previousGameWorkingAreaPosition)
            {
                Position = rect.Location;
                Width = rect.Width;
                Height = rect.Height;
            }
        }

        public WindowWrapper(Window windowHandle)
        {
            _window = windowHandle;
        }
    }
}
