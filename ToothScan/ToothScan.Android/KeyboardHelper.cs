using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using ToothScan.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(KeyboardHelper))]
namespace ToothScan.Droid
{
    class KeyboardHelper : IKeyboardHelper
    {
        static Context _context;

        public KeyboardHelper()
        {

        }

        public static void Init(Context context)
        {
            _context = context;
        }

        public void HideKeyboard()
        {
            var context = Android.App.Application.Context;
            var inputMethodManager = context.GetSystemService(Context.InputMethodService) as InputMethodManager;
            if (inputMethodManager != null && context is Activity)
            {
                var activity = context as Activity;
                var token = activity.CurrentFocus?.WindowToken;
                inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);
                activity.Window.DecorView.ClearFocus();
            }
        }
    }
}