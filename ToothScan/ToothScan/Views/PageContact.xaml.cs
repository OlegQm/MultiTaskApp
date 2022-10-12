using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ToothScan.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PageContact : ContentPage
    {
        private List<string> AdressesList = new List<string>();
        private string LetterSubject = null;
        /*
         * Adding to the e-mail list for complaints and suggestions 
         */
        public PageContact()
        {
            InitializeComponent();
            AdressesList.Add("toothscancontact@gmail.com");
        }
        /*
         * Clearing the list of emails to send when the page disappears
         */
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            AdressesList.Clear();
        }
        /*
          Implementation of the function of sending letters
          (indicating the name, body of the letter and recipients,
          as well as sending the letter to the mail from the mail
          list ('toothscancontact@gmail.com'))
         */
        private async Task SendEmail(string subject, string body, List<string> recipients)
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = subject,
                    Body = body,
                    To = recipients,
                };
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException)
            {
                await DisplayAlert("Warning", "E-mail not supported on your device", "OK");
                
            }
            catch (Exception)
            {
                await DisplayAlert("Warning", "Something was wrong. Please, check your internet connection.", "OK");
            }
        }
        /*
         * Copy Telegram tag to clipboard
         */
        private async void TelegramButton_Clicked(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(Telegram.Text);
            CrossToastPopUp.Current.ShowToastMessage("Copied to clipboard");
        }
        /*
         * Copy Instagram tag to clipboard
         */
        private async void InstagramButton_Clicked(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(Instagram.Text);
            CrossToastPopUp.Current.ShowToastMessage("Copied to clipboard");
        }
        /*
         * Switching to the application for sending letters on the click of a button
         */
        private async void SendButton_Clicked(object sender, EventArgs e)
        {
            await SendEmail("From ToothScan App", EmailLetter.Text, AdressesList);
        }
        /*
           Saving the text of the letter to a
           variable and clearing the text of the letter
         */
        private void ClearEmail_Clicked(object sender, EventArgs e)
        {
            LetterSubject = EmailLetter.Text;
            EmailLetter.Text = null;
        }
        /*
           Adding the text of the letter from a local variable,
           clearing the variable to store the text of the letter
         */
        private void BackEmailText_Clicked(object sender, EventArgs e)
        {
            EmailLetter.Text = LetterSubject;
            LetterSubject = null;
        }
        /*
           A function that disables the message clear button if its text
           is empty and vice versa
         */
        private void EmailText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue == null || e.NewTextValue == "")
                ClearEmail.IsEnabled = false;
            else
                ClearEmail.IsEnabled = true;
        }
    }
}