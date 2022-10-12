using Xamarin.Forms;
using SkiaSharp;

namespace ToothScan.Views
{
    class FingerPaintPolyline
    {
        public FingerPaintPolyline()
        {
            Path = new SKPath();
        }

        public SKPath Path { get; set; }

        public Color StrokeColor { get; set; }

        public float StrokeWidth { get; set; }
    }
}