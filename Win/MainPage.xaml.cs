using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using D3D;
using Windows.UI.Input;
using Windows.UI.Core;
using Windows.Gaming.Input;
using System.Collections;
using System.Threading;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Win
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Client _client;

        public MainPage()
        {
            this.InitializeComponent();

            _client = new Client();

            Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;

            if (!_client.XBOX)
            {
                Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
                Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
                Window.Current.CoreWindow.PointerMoved += CoreWindow_PointerMoved;
            }
            else
            {
                Gamepad.GamepadAdded += GamepadAdded;
                Gamepad.GamepadRemoved += Gamepad_GamepadRemoved;
            }

            UI ui = new UI();
            Service.Register<UI>(ui);

            ui.FpsTextBox = fps;
            ui.CountTextBox = count;
        }

        private void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            Rect bounds = Window.Current.Bounds;

            _client.Resize(bounds);
        }

        private void Gamepad_GamepadRemoved(object sender, Gamepad e)
        {
            int indexRemoved = _client.MyGamepads.IndexOf(e);

            if (indexRemoved > -1)
            {
                if (_client.MainGamepad == _client.MyGamepads[indexRemoved])
                {
                    _client.MainGamepad = null;
                }

                _client.MyGamepads.RemoveAt(indexRemoved);
            }
        }

        private void GamepadAdded(object sender, Gamepad e)
        {
            _client.MyGamepads.Add(e);
            _client.MainGamepad = _client.MyGamepads[0];
        }

        private void CoreWindow_PointerMoved(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            PointerPoint point = args.CurrentPoint;

            double x = point.Position.X;
            double y = point.Position.Y;

            Mouse mouse = new Mouse();

            if (point.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {

                if (point.Properties.IsLeftButtonPressed)
                {
                    mouse.left = true;
                }
                if (point.Properties.IsMiddleButtonPressed)
                {
                    mouse.mid = true;
                }
                if (point.Properties.IsRightButtonPressed)
                {
                    mouse.right = true;
                }
            }

            _client.OnBaseMouseMove(mouse, x, y);
        }

        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            _client.OnBaseKeys(args.VirtualKey, true);
        }

        private void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            _client.OnBaseKeys(args.VirtualKey, false);
        }

        private void swapChainPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _client.Init(swapChainPanel, Window.Current.Bounds, CloseApp);
        }

        public void CloseApp()
        {
            Windows.ApplicationModel.Core.CoreApplication.Exit();
        }

        private void swapChainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void swapChainPanel_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void go_Click(object sender, RoutedEventArgs e)
        {
            _client.Go_Click();
        }
    }
}
