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
        private UIInputManager _uim;
        private BoxApp _box;

        public MainPage()
        {
            this.InitializeComponent();

            _box = new BoxApp(swapChainPanel);
            Service.Register<BoxApp>(_box);

            UI ui = new UI();
            Service.Register<UI>(ui);

            ui.FpsTextBox = fps;
            ui.CountTextBox = count;

            _uim = new UIInputManager();
            Service.Register<UIInputManager>(_uim);
            _uim.CloseApp = CloseApp;

            Rect bounds = Window.Current.Bounds;
            _uim.Bounds = bounds;

            _client = new Client();

            Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;

            if (!_uim.XBOX)
            {
                Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
                Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
                Window.Current.CoreWindow.PointerMoved += CoreWindow_PointerMoved;
            }
            else
            {
                Gamepad.GamepadAdded += GamepadAdded;
                Gamepad.GamepadRemoved += GamepadRemoved;
            }
        }

        private void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            Rect bounds = Window.Current.Bounds;
            _uim.Bounds = bounds;
            _uim.Resize();
        }

        private void GamepadRemoved(object sender, Gamepad e)
        {
            _uim.GamepadRemoved(e);
        }

        private void GamepadAdded(object sender, Gamepad e)
        {
            _uim.GamepadAdded(e);
        }

        private void CoreWindow_PointerMoved(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args)
        {
            _uim.PointerMoved(args.CurrentPoint);
        }

        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            _uim.OnBaseKeys(args.VirtualKey, true);
        }

        private void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            _uim.OnBaseKeys(args.VirtualKey, false);
        }

        private void swapChainPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _client.Init();
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
            _box.Count = _box.Count + 1;
        }
    }
}
