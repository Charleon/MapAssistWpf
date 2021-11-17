using Gma.System.MouseKeyHook;
using MapAssist;
using MapAssist.Settings;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        MapAssistConfiguration _configuration;
        MainRunner _mainRunner;
        IKeyboardMouseEvents _globalHook;

        public const int WS_EX_TRANSPARENT = 0x00000020; public const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);


        private MapAssistConfiguration ReadConfiguration()
        {
            try
            {
                return new MapAssistConfiguration();
            }
            catch (ConfigurationReadException e)
            {
                MessageBox.Show(e.Message, "Configuration parsing error");
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Configuration parsing error");
                System.Windows.Application.Current.Shutdown();
            }

            return null;
        }
        public MainWindow()
        {
            IKeyboardMouseEvents globalHook = Hook.GlobalEvents();

            MapAssistConfiguration mapAssistConfiguration = ReadConfiguration();
            _configuration = new MapAssistConfiguration();
            _mainRunner = new MainRunner(_configuration, globalHook, this);

            InitializeComponent();

            Closing += OnWindowClosing;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MakeWindowClickThrough();
            var writeableBitmap = CreateImage(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            //UpdateImage(writeableBitmap);
            MapImage.Source = writeableBitmap;
            _mainRunner.Run(writeableBitmap);
            base.OnSourceInitialized(e);
        }

        private WriteableBitmap CreateImage(int width, int height)
        {
            var writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256Transparent);
            return writeableBitmap;
        }

        private void MakeWindowClickThrough()
        {
            // Get this window's handle         
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            // Change the extended window style to include WS_EX_TRANSPARENT         
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            _globalHook.Dispose();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
