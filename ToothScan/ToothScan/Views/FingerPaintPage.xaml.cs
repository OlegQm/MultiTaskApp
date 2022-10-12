using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Reflection;
using TouchTracking;
using Xamarin.Forms.Xaml;

namespace ToothScan.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FingerPaintPage : ContentPage
    {
        Dictionary<long, FingerPaintPolyline> inProgressPolylines = new Dictionary<long, FingerPaintPolyline>();
        List<FingerPaintPolyline> completedPolylines = new List<FingerPaintPolyline>();
        List<FingerPaintPolyline> completedPolylinesCopy = new List<FingerPaintPolyline>(); //Stores a reverse copy of the 'completedPolylines' list
        private bool EraserChosen = false;
        private bool IsEraser = false;

        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };
        public FingerPaintPage()
        {
            InitializeComponent();
            EraseLine.IsEnabled = false;
        }
        /**
         * Clearing the main and temporary line list
         */
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            completedPolylines.Clear();
            completedPolylinesCopy.Clear();
        }
        /**
         * Setting default object properties on button click
         */
        void ClearButton_Clicked(object sender, EventArgs args)
        {
            colorPicker.BackgroundColor = Color.Transparent;
            widthPicker.BackgroundColor = Color.Transparent;
            completedPolylines.Clear();
            completedPolylinesCopy.Clear();
            EraseLine.IsEnabled = false;
            ClearButton.IsEnabled = false;
            NextLine.IsEnabled = false;
            canvasView.InvalidateSurface();
        }
        /**
         * Click handling function (multiple steps)
         */
        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            switch (args.Type)
            {
                /**
                   Screen touch function, it sets the color and width of the line,
                   and also remembers the location of the touch in a special list ('inProgressPolylines')
                   that stores the coordinates of the lines that are in the process of being created
                 */
                case TouchActionType.Pressed:
                    if (!inProgressPolylines.ContainsKey(args.Id))
                    {
                        Color strokeColor;
                        if (EraserChosen == false)
                            strokeColor = (Color)typeof(Color).GetRuntimeField(colorPicker.Items[colorPicker.SelectedIndex]).GetValue(null);
                        else
                            strokeColor = Color.Transparent;
                        float strokeWidth = ConvertToPixel(new float[] { 1, 2, 5, 10, 20 }[widthPicker.SelectedIndex]);
                        colorPicker.BackgroundColor = strokeColor;
                        widthPicker.BackgroundColor = strokeColor;

                        FingerPaintPolyline polyline = new FingerPaintPolyline
                        {
                            StrokeColor = strokeColor,
                            StrokeWidth = strokeWidth
                        };
                        polyline.Path.MoveTo(ConvertToPixel(args.Location));
                        if(!inProgressPolylines.ContainsKey(args.Id))
                        {
                            inProgressPolylines.Add(args.Id, polyline);
                            canvasView.InvalidateSurface();
                        }
                    }
                    break;
                /**
                   A function that processes the movement of a finger across the screen,
                   which creates a line on the screen, the coordinates of which are stored
                   in the list 'inProgressPolylines'
                 */
                case TouchActionType.Moved:
                    EraseLine.IsEnabled = true;
                    if (inProgressPolylines.ContainsKey(args.Id))
                    {
                        FingerPaintPolyline polyline = inProgressPolylines[args.Id];
                        polyline.Path.LineTo(ConvertToPixel(args.Location));
                        canvasView.InvalidateSurface();
                    }
                    break;
                /**
                   A function that is called in the case of a successfully drawn line,
                   when the user releases the finger, it adds the generated line to the
                   'completedPolylines' list, which stores the lines completed by the user,
                   and also removes the coordinates of this completed line from the 'inProgressPolylines' list
                 */
                case TouchActionType.Released:
                    if(EraseLine.IsEnabled == false)
                        ClearButton.IsEnabled = true;
                    if (completedPolylinesCopy.Count!=0)
                        NextLine.IsEnabled = true;
                    if (inProgressPolylines.ContainsKey(args.Id))
                    {
                        if (!completedPolylines.Contains(inProgressPolylines[args.Id]))
                        {
                            completedPolylines.Add(inProgressPolylines[args.Id]);
                        }
                        inProgressPolylines.Remove(args.Id);
                        canvasView.InvalidateSurface();
                    }
                    break;
                /**
                   A function that is called in case of an unsuccessfully drawn
                   line (there was a tap on the screen, but the user did not draw a line),
                   when the user releases his finger, it will remove the coordinates of this incomplete
                   line (point) from the 'inProgressPolylines' list
                 */
                case TouchActionType.Cancelled:
                    if (inProgressPolylines.ContainsKey(args.Id))
                    {
                        inProgressPolylines.Remove(args.Id);
                        canvasView.InvalidateSurface();
                    }
                    break;
            }
        }
        /**
           A function that implements the construction of
           lines on the user's screen, the coordinates of which are stored
           in the lists 'completedPolylines' and 'inProgressPolylines'
         */
        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            foreach (FingerPaintPolyline polyline in completedPolylines)
            {
                paint.Color = polyline.StrokeColor.ToSKColor();
                paint.StrokeWidth = polyline.StrokeWidth;
                canvas.DrawPath(polyline.Path, paint);
            }

            foreach (FingerPaintPolyline polyline in inProgressPolylines.Values)
            {
                paint.Color = polyline.StrokeColor.ToSKColor();
                paint.StrokeWidth = polyline.StrokeWidth;
                canvas.DrawPath(polyline.Path, paint);
            }
        }

        /**
         * This function convert point from type 'TouchTrackingPoint' to type 'SKPoint'
         */
        SKPoint ConvertToPixel(TouchTrackingPoint pt)
        {
            return new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / canvasView.Width),
                               (float)(canvasView.CanvasSize.Height * pt.Y / canvasView.Height));
        }
        /**
         * Function to convert a number (of type float) to pixels
         */
        float ConvertToPixel(float fl)
        {
            return (float)(canvasView.CanvasSize.Width * fl / canvasView.Width);
        }
        /**
           The function of removing the last added line from the screen,
           it adds the last element of the 'completedPolylines' list to
           the 'completedPolylinesCopy' list, removes this element from the
           'completedPolylines' list, and then updates the image on the screen,
           which allows you to remove the last line added by the user
         */
        private void ClearLine_Clicked(object sender, EventArgs e)
        {
            if (completedPolylines.Count != 0)
            {
                NextLine.IsEnabled = true;
                completedPolylinesCopy.Add(completedPolylines[completedPolylines.Count - 1]);
                completedPolylines.Remove(completedPolylines[completedPolylines.Count - 1]);
                canvasView.InvalidateSurface();
            }
            if(completedPolylines.Count == 0)
            {
                ClearButton.IsEnabled = false;
                EraseLine.IsEnabled = false;
            }
        }
        /**
           The function of adding the last 'completedPolylinesCopy' line to the screen,
           it adds the last element of the 'completedPolylinesCopy' list to
           the 'completedPolylines' list, removes this element from the
           'completedPolylinesCopy' list, and then updates the image on the screen,
           which allows you to add the last 'completedPolylinesCopy' line to the screen
        */
        private void NextLine_Clicked(object sender, EventArgs e)
        {
            if (completedPolylinesCopy.Count != 0)
            {
                ClearButton.IsEnabled = true;
                EraseLine.IsEnabled = true;
                completedPolylines.Add(completedPolylinesCopy[completedPolylinesCopy.Count - 1]);
                completedPolylinesCopy.Remove(completedPolylinesCopy[completedPolylinesCopy.Count - 1]);
                canvasView.InvalidateSurface();
            }
            if(completedPolylinesCopy.Count == 0)
            {
                NextLine.IsEnabled = false;
            }
        }
        /**
           A function that allows you to draw a line in 'Transparent'
           color on top of other lines, which allows you to create the
           illusion of removing part of the line
         */
        private void EraseLine_Clicked(object sender, EventArgs e)
        {
            if (IsEraser == false)
            {
                if (completedPolylines.Count != 0)
                {
                    EraserChosen = true;
                    IsEraser = true;
                    EraseLine.Text = "Pencil";
                    paint.BlendMode = SKBlendMode.Src;
                    colorPicker.IsEnabled = false;
                    Title = "Erasing";
                    ClearButton.IsEnabled = false;
                }
            }
            else
            {
                EraserChosen = false;
                IsEraser = false;
                EraseLine.Text = "Eraser";
                colorPicker.IsEnabled = true;
                Title = "Drawing";
                ClearButton.IsEnabled = true;
            }
        }
    }
}