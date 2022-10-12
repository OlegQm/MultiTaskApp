using Android.Content;
using Android.Runtime;
using Android.Views;
using ToothScan;
using ToothScan.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SearchBarExtended), typeof(SearcherRenderExtended))]
namespace ToothScan.Droid
{
    class SearcherRenderExtended : SearchBarRenderer
    {
        public SearcherRenderExtended(Context context) : base(context)
        {
        }
        protected override void OnVisibilityChanged(Android.Views.View changedView, [GeneratedEnum] ViewStates visibility)
        {
            base.OnVisibilityChanged(changedView, visibility);
        }
    }
}