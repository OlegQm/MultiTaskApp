using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace ToothScan.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutDevicePage : ContentPage
    {
        public AboutDevicePage()
        {
            Battery.BatteryInfoChanged += Battery_BatteryInfoChanged;
            Battery.EnergySaverStatusChanged += Battery_EnergySaverStatusChanged;
            DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;

            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            /*
             * Hardware
             */
            HardwareInfo0.Text = "<span style=\"color:blue\">Device Name:</span>";
            HardwareInfo1.Text = $"{DeviceInfo.Name}";
            HardwareInfo2.Text = "<span style=\"color:blue\">Android Version:</span>";
            HardwareInfo3.Text = $"{DeviceInfo.VersionString}";
            HardwareInfo4.Text = "<span style=\"color:blue\">Model:</span>";
            HardwareInfo5.Text = $"{DeviceInfo.Model}";
            HardwareInfo6.Text = "<span style=\"color:blue\">Manufacturer:</span>";
            HardwareInfo7.Text = $"{DeviceInfo.Manufacturer}";
            HardwareInfo8.Text = "<span style=\"color:blue\">Type:</span>";
            HardwareInfo9.Text = $"{DeviceInfo.DeviceType}";
            /*
             * Screen
             */
            ScreenInfo0.Text = "<span style=\"color:blue\">Size (px):</span>";
            ScreenInfo1.Text = $"{DeviceDisplay.MainDisplayInfo.Width} × {DeviceDisplay.MainDisplayInfo.Height}";
            ScreenInfo2.Text = "<span style=\"color:blue\">Size (dip):</span>";
            ScreenInfo3.Text = $"{(int)(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density)} × {Math.Round(DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density)}";
            ScreenInfo4.Text = "<span style=\"color:blue\">Orientation:</span>";
            ScreenInfo5.Text = $"{DeviceDisplay.MainDisplayInfo.Orientation}";

            var level = Battery.ChargeLevel;
            var state = Battery.State;
            var source = Battery.PowerSource;
            var saver_energy = Battery.EnergySaverStatus;
            /*
             * Battery
             */
            BatteryInfo0.Text = "<span style=\"color:blue\">Charge level:</span>";
            BatteryInfo1.Text = $"{level * 100}%";
            BatteryInfo2.Text = "<span style=\"color:blue\">State:</span>";
            BatteryInfo3.Text = $"{state}";
            BatteryInfo4.Text = "<span style=\"color:blue\">Source:</span>";
            BatteryInfo5.Text = $"{source}";
            BatteryInfo6.Text = "<span style=\"color:blue\">Saving energy:</span>";
            BatteryInfo7.Text = $"{saver_energy}";
        }
        private void Battery_BatteryInfoChanged(object sender, BatteryInfoChangedEventArgs e)
        {
            BatteryInfo1.Text = null;
            BatteryInfo3.Text = null;
            BatteryInfo5.Text = null;

            BatteryInfo1.Text = $"{e.ChargeLevel * 100}%";
            BatteryInfo3.Text = $"{e.State}";
            BatteryInfo5.Text = $"{e.PowerSource}";
        }
        private void Battery_EnergySaverStatusChanged(object sender, EnergySaverStatusChangedEventArgs e)
        {
            BatteryInfo7.Text = null;
            BatteryInfo7.Text = $"{e.EnergySaverStatus}";
        }
        private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            ScreenInfo1.Text = null;
            ScreenInfo3.Text = null;
            ScreenInfo5.Text = null;
            if (e.DisplayInfo.Orientation == DisplayOrientation.Portrait)
            {
                ScreenInfo1.Text = $"{DeviceDisplay.MainDisplayInfo.Width} × {DeviceDisplay.MainDisplayInfo.Height}";
                ScreenInfo3.Text = $"{(int)(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density)} × {Math.Round(DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density)}";
                ScreenInfo5.Text = $"{DeviceDisplay.MainDisplayInfo.Orientation}";
            }
            else
            {
                ScreenInfo1.Text = $"{DeviceDisplay.MainDisplayInfo.Width} × {DeviceDisplay.MainDisplayInfo.Height}";
                ScreenInfo3.Text = $"{Math.Round(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density)} × {(int)(DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density)}";
                ScreenInfo5.Text = $"{DeviceDisplay.MainDisplayInfo.Orientation}";
            }
        }
    }
}