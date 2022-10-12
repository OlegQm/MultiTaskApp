using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ToothScan.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "Options";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://github.com/OlegQm/Text_search_and_returning_Xamarin"));
        }

        public ICommand OpenWebCommand { get; }
    }
}