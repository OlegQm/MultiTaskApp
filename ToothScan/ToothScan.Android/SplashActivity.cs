using Android.App;
using Android.Content.PM;
using Android.OS;

namespace ToothScan.Droid
{
    [Activity(Theme = "@style/Theme.Splash", MainLauncher = true, NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            StartActivity(typeof(MainActivity));
            this.RequestedOrientation = ScreenOrientation.Portrait;
        }
    }
}