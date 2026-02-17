using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace MOGWAI_RUNTIME
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public void SetScreenOn()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
            });
        }

        public void LeaveScreenOff()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Window?.ClearFlags(WindowManagerFlags.KeepScreenOn);
            });
        }
    }
}
