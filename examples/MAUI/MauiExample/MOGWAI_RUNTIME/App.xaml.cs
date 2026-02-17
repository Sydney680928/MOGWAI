using Microsoft.Maui.Platform;
using MOGWAI_RUNTIME.Classes;
using System.Diagnostics;
using Application = Microsoft.Maui.Controls.Application;

#if WINDOWS
using Microsoft.Windows.AppLifecycle;
#endif

#if ANDROID
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Android.Content.Res;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
#endif

namespace MOGWAI_RUNTIME
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

#if WINDOWS
            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
            {
                var mauiWindow = handler.VirtualView;
                var nativeWindow = handler.PlatformView;
                nativeWindow.Activate();
                IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                try
                {
                    var w = Preferences.Default.Get("WindowSizeWidth", appWindow.Size.Width);
                    var h = Preferences.Default.Get("WindowSizeHeight", appWindow.Size.Height);                 
                    var t = Preferences.Default.Get("WindowPositionTop", appWindow.Position.Y);
                    var l = Preferences.Default.Get("WindowPositionLeft", appWindow.Position.X);

                    var rect = new Windows.Graphics.RectInt32(l, t, w, h);
                    appWindow.MoveAndResize(rect);
                }
                catch
                {

                }

                if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter p)
                {
                    p.IsResizable = true;
                    p.IsMaximizable = false;
                    p.IsMinimizable = true;
                    p.IsAlwaysOnTop = false;
                }

                appWindow.Changed += (sender, args) => 
                {
                    if (args.DidSizeChange)
                    {
                        if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter p)
                        {
                            if (p.State == Microsoft.UI.Windowing.OverlappedPresenterState.Restored)
                            {
                                Preferences.Default.Set("WindowSizeWidth", appWindow.Size.Width);
                                Preferences.Default.Set("WindowSizeHeight", appWindow.Size.Height);
                            }
                        }
                    }
                    else if (args.DidPositionChange)
                    {
                        Preferences.Default.Set("WindowPositionTop", appWindow.Position.Y);
                        Preferences.Default.Set("WindowPositionLeft", appWindow.Position.X);
                    }
                };
            });

#endif

            Microsoft.Maui.Handlers.EntryHandler.ViewMapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                if (handler.PlatformView is AppCompatEditText v)
                {
                    var color = Color.FromRgb(0x28, 0x42, 0x5D);
                    v.BackgroundTintList = ColorStateList.ValueOf(color.ToAndroid());
                }

#elif IOS

                if (handler.PlatformView is Microsoft.Maui.Platform.MauiTextField v)
                {
                    var color = Color.FromRgb(0x28, 0x42, 0x5D);
                    v.BackgroundColor = color.ToPlatform();
                    v.BorderStyle = UIKit.UITextBorderStyle.RoundedRect;
                }
#endif
            });

            if (Current != null)
            {
                Current.UserAppTheme = AppTheme.Light;               
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
#if ANDROID
            Current?.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
#endif
            var mainPage = new NavigationPage(new Pages.MainPage());
            Window window = new Window(mainPage);
            window.Title = "MOGWAI RUNTIME";
            return window;
        }
    }
}
