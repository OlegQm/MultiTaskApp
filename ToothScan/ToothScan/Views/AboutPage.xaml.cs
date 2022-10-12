using System;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using Xamarin.Essentials;
using Plugin.Toast;
using System.Threading.Tasks;

namespace ToothScan.Views
{
    public partial class AboutPage : ContentPage
    {
        private string SavedText = null;
        private bool _isFirstTorchOnRequest = true;
        public AboutPage()
        {
            InitializeComponent();
        }
        /**
         * Implementing the Share Text Function
         */
        private async Task ShareText(string text)
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Text = text,
                Title = "Share"
            });
        }
        /**
         * Go to a page with a list of Bluetooth devices
         */
        private async void Bluetooth_Clicked(object sender, EventArgs e)
        {
            Devices page = new Devices();
            Bluetooth.IsEnabled = false;
            await Navigation.PushAsync(page);
            Bluetooth.IsEnabled = true;
        }
        /**
         * Implementing the QR and Barcode scanning page,
           adding a flashlight button, and implementing the transfer
           of the scanned text to the home page after scanning.
         */
        private async void ScannerButton_Clicked(object sender, EventArgs e)
        {
            ScannerButton.IsEnabled = false;
            var overlay = new ZXingDefaultOverlay
            {
                ShowFlashButton = true,
                TopText = "Hold your phone up to the QR code",
                BottomText = "Scanning will happen automatically",
                Opacity = 0.8,
            };
            var scan = new ZXingScannerPage(null, overlay)
            {
                Title = "Scanning"
            };
            await Navigation.PushAsync(scan);
            ScannerButton.IsEnabled = true;
            overlay.FlashButtonClicked += async (s, ed) =>
            {
                if (_isFirstTorchOnRequest)
                {
                    try
                    {
                        scan.ToggleTorch();
                        scan.IsTorchOn = true;
                        _isFirstTorchOnRequest = false;
                    }
                    catch
                    {
                        await DisplayAlert("Warning", "There was a problem with the flashlight. Make sure this component is present.", "OK");
                    }
                }
                else
                {
                    try
                    {
                        scan.ToggleTorch();
                        scan.IsTorchOn = !scan.IsTorchOn;
                    }
                    catch
                    {
                        await DisplayAlert("Warning", "Something was wrong.", "OK");
                    }
                }
            };
            scan.OnScanResult += (result) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Navigation.PopAsync();
                        QRCode.Text = result.Text;
                    });
                };
        }
        /**
         * Implementation of finding links:
           searching for substrings starting with 'http://', ​​'https://' and 'www.' and ending
           with the first available space, deleting the rest of the text
           and opening these substrings in a browser with the given parameters
         */
        private async void LinkSearchButton_Clicked(object sender, EventArgs e)
        {
            string SearchResource;
            if (QRCode.Text == null)
                SearchResource = "https://github.com/OlegQm/";
            else
            {
                SearchResource = QRCode.Text;
            }
            SavedText = SearchResource;
            bool LinkIsRecognized = false;
            string http = "http://";
            string https = "https://";
            string www = "www.";
            if (SearchResource.IndexOf(http) > (-1))
            {
                SearchResource += " \n";
                SearchResource = SearchResource.Remove(0, SearchResource.IndexOf(http));
                char[] SearchResoureSymbols = SearchResource.ToCharArray();
                if (SearchResource.IndexOf(" ") < SearchResource.IndexOf("\n"))
                {
                    SearchResource = SearchResource.Remove(SearchResource.IndexOf(" "), SearchResoureSymbols.Length - SearchResource.IndexOf(" "));
                    LinkIsRecognized = true;
                    while (SearchResource.IndexOf(" ") > (-1))
                        SearchResource.Remove(SearchResource.IndexOf(" "), 1);
                    QRCode.Text = SearchResource;
                }
                else
                {
                    SearchResource = SearchResource.Remove(SearchResource.IndexOf("\n"), SearchResoureSymbols.Length - SearchResource.IndexOf("\n"));
                    LinkIsRecognized = true;
                    while (SearchResource.IndexOf("\n") > (-1))
                        SearchResource.Remove(SearchResource.IndexOf("\n"), 1);
                }
                QRCode.Text = SearchResource;
            }
            else if (SearchResource.IndexOf(https) > (-1))
            {
                SearchResource += " \n";
                SearchResource = SearchResource.Remove(0, SearchResource.IndexOf(https));
                char[] SearchResoureSymbols = SearchResource.ToCharArray();
                if (SearchResource.IndexOf(" ") < SearchResource.IndexOf("\n"))
                {
                    SearchResource = SearchResource.Remove(SearchResource.IndexOf(" "), SearchResoureSymbols.Length - SearchResource.IndexOf(" "));
                    LinkIsRecognized = true;
                    while (SearchResource.IndexOf(" ") > (-1))
                        SearchResource.Remove(SearchResource.IndexOf(" "), 1);
                }
                else
                {
                    SearchResource = SearchResource.Remove(SearchResource.IndexOf("\n"), SearchResoureSymbols.Length - SearchResource.IndexOf("\n"));
                    LinkIsRecognized = true;
                    while (SearchResource.IndexOf("\n") > (-1))
                        SearchResource.Remove(SearchResource.IndexOf("\n"), 1);
                }
                QRCode.Text = SearchResource;
            }
            else if (SearchResource.IndexOf(www) > (-1))
            {
                SearchResource += " \n";
                SearchResource = SearchResource.Remove(0, SearchResource.IndexOf(www));
                char[] SearchResoureSymbols = SearchResource.ToCharArray();
                if (SearchResource.IndexOf(" ") < SearchResource.IndexOf("\n"))
                {
                    SearchResource = SearchResource.Remove(SearchResource.IndexOf(" "), SearchResoureSymbols.Length - SearchResource.IndexOf(" "));
                    LinkIsRecognized = true;
                    while (SearchResource.IndexOf(" ") > (-1))
                        SearchResource.Remove(SearchResource.IndexOf(" "), 1);
                }
                else
                {
                    SearchResource = SearchResource.Remove(SearchResource.IndexOf("\n"), SearchResoureSymbols.Length - SearchResource.IndexOf("\n"));
                    LinkIsRecognized = true;
                    while (SearchResource.IndexOf("\n") > (-1))
                        SearchResource.Remove(SearchResource.IndexOf("\n"), 1);
                }
                QRCode.Text = SearchResource;
            }
            else
            {
                LinkIsRecognized = false;
            }
             
            if (LinkIsRecognized == false || SearchResource.IndexOf("\n") > (-1))
            {
                SearchResource = "https://www.google.com/search?q=" + SearchResource;
                try
                {
                    await Browser.OpenAsync(SearchResource, new BrowserLaunchOptions
                    {
                        LaunchMode = BrowserLaunchMode.SystemPreferred,
                        TitleMode = BrowserTitleMode.Show,
                        PreferredToolbarColor = Color.AliceBlue,
                        PreferredControlColor = Color.Violet
                    });
                }
                catch
                {
                    await DisplayAlert("Something was wrong", "Try to copy and paste link by yourself.", "OK");
                }
            }
            else
            {
                try
                {
                    await Browser.OpenAsync(SearchResource, new BrowserLaunchOptions
                    {
                        LaunchMode = BrowserLaunchMode.SystemPreferred,
                        TitleMode = BrowserTitleMode.Show,
                        PreferredToolbarColor = Color.AliceBlue,
                        PreferredControlColor = Color.Violet
                    });
                }
                catch
                {
                    await DisplayAlert("Something was wrong", "Try to copy and paste link by yourself.", "OK");
                }
            }
        }
        /**
         * Implementation of a normal text search,
           setting browser parameters and inserting the search text into it
         */
        private async void NormalSearchButton_Clicked(object sender, EventArgs e)
        {
            string SearchResource;
            if (QRCode.Text == null)
                SearchResource = "https://github.com/OlegQm/";
            else
            {
                SearchResource = QRCode.Text;
                SearchResource = "https://www.google.com/search?q=" + SearchResource;
            }
            try
            {
                await Browser.OpenAsync(SearchResource, new BrowserLaunchOptions
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred,
                    TitleMode = BrowserTitleMode.Show,
                    PreferredToolbarColor = Color.AliceBlue,
                    PreferredControlColor = Color.Violet
                });
            }
            catch
            {
                await DisplayAlert("Something was wrong", "Try to copy and paste text by yourself.", "OK");
            }
        }
        /**
         * Copy text to clipboard
         */
        private async void CopyButton_Clicked(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(QRCode.Text);
            CrossToastPopUp.Current.ShowToastMessage("Copied to clipboard");
        }
        /**
         * Saving scanned text to a variable and clearing scanned text
         */
        private void ClearButton_Clicked(object sender, EventArgs e)
        {
            if (QRCode.Text != null)
            {
                SavedText = QRCode.Text;
            }
            QRCode.Text = null;
        }
        /**
         * Return scanned text from a variable
         */
        private void BackButton_Clicked(object sender, EventArgs e)
        {
            QRCode.Text = SavedText;
        }
        /**
         * Go to drawing page
         */
        private async void DrawButton_Clicked(object sender, EventArgs e)
        {
            FingerPaintPage page = new FingerPaintPage();
            DrawButton.IsEnabled = false;
            await Navigation.PushAsync(page);
            DrawButton.IsEnabled = true;
        }
        /**
         * Implementing the share QR code text function
         */
        private async void ShareButton_Clicked(object sender, EventArgs e)
        {
            if (QRCode.Text != null)
                await ShareText(QRCode.Text);
            else
                CrossToastPopUp.Current.ShowToastMessage("No text found");
        }
        /**
        * Implementing the share project link function
        */
        private async void ShareProject_Clicked(object sender, EventArgs e)
        {
            await ShareText("https://github.com/OlegQm/Text_search_and_returning_Xamarin");
        }
    }
}