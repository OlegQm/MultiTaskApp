using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BluetoothClassic.Abstractions;
using System.Collections.ObjectModel;
using Plugin.Toast;
using Xamarin.Essentials;
using ToothScan.Models;

namespace ToothScan
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Devices : ContentPage
    {
        static System.Timers.Timer UpdateTimer; 
        private readonly IBluetoothAdapter _BluetoothAdapter;
        IAdapter Adapter;
        IBluetoothLE BluetoothBLE;
        ObservableCollection<IDevice> DevicesCollectionList = new ObservableCollection<IDevice>();
        private IDevice CurrentDevice;
        private string DeviceInfo = null;

        /**
         * Initializing components, creating a list of saved devices and adding the list to the 'ListView'
         */
        public Devices()
        {
            _BluetoothAdapter = DependencyService.Resolve<IBluetoothAdapter>();
            InitializeComponent();
            try
            {
                BluetoothBLE = CrossBluetoothLE.Current;
                Adapter = CrossBluetoothLE.Current.Adapter;
                DevicesCollectionList = new ObservableCollection<IDevice>();
                DevicesList.ItemsSource = DevicesCollectionList;
            }
            catch
            {
                DevicesDisplayAlert();
            }
        }
        private async void DevicesDisplayAlert()
        {
            await DisplayAlert("Warning", "Something was wrong", "OK");
        }
        /**
         * Update the state of visual elements on a page
         */
        private void RefreshUI()
        {
            if (_BluetoothAdapter.Enabled)
            {
                DisableBluetooth.IsEnabled = true;
                EnableBluetooth.IsEnabled = false;
                lvBondedDevices.ItemsSource = _BluetoothAdapter.BondedDevices;
            }
            else
            {
                DisableBluetooth.IsEnabled = false;
                EnableBluetooth.IsEnabled = true;
                lvBondedDevices.ItemsSource = null;
                DevicesList.IsVisible = false;
            }
        }
        /**
         * Turn on Bluetooth at the touch of a button
         */
        private void EnableBluetooth_Clicked(object sender, EventArgs e)
        {
            _BluetoothAdapter.Enable();
            RefreshUI();
        }
        /**
         * Turn off Bluetooth at the touch of a button
         */
        private void DisableBluetooth_Clicked(object sender, EventArgs e)
        {
            try
            {
                UpdateTimer.Stop();
                UpdateTimer.Dispose();
            }
            catch
            { }
            _BluetoothAdapter.Disable();
            RefreshUI();
        }
        /**
         * Implementation of the disconnect function
         */
        private async Task DisconnectIfConnectedAsync()
        {
            if (Connections.CurrentBluetoothConnection != null)
            {
                try
                {
                    Connections.CurrentBluetoothConnection.Dispose();
                }
                catch (Exception exception)
                {
                    await DisplayAlert("Error", exception.Message, "Close");
                }
            }
        }
        /**
         * Terminate established BLE connections and update interface
         */
        protected override async void OnAppearing()
        {
            RefreshUI();
            await DisconnectIfConnectedAsync();
        }
        /**
         * Terminate established BLE connections, update interface and stop timer
         */
        protected override async void OnDisappearing()
        {
            RefreshUI();
            await DisconnectIfConnectedAsync();
            try
            {
                UpdateTimer.Stop();
                UpdateTimer.Dispose();
            }
            catch { }
        }
        /**
         * Opening a window with information about the device
           when clicking on an item in the 'ListView'
         */
        private async void lvBondedDevices_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            BluetoothDeviceModel bluetoothDeviceModel = e.SelectedItem as BluetoothDeviceModel;

            if (bluetoothDeviceModel != null)
            {
                DeviceInfo = null;
                ShortDeviceInfo.Text = null;
                CopyShortInfo.IsEnabled = true;
                popupInfosView.IsVisible = true;

                ShortDeviceInfo.Text =
                $"<span style=\"color:blue\">Name: </span>{bluetoothDeviceModel.Name}<br><br>" +
                $"<span style=\"color:blue\">Adress: </span>{bluetoothDeviceModel.Address}<br><br>";

                DeviceInfo = "Name: " + bluetoothDeviceModel.Name + "\nAdress: " + bluetoothDeviceModel.Address;
            }
            else
                await DisplayAlert("Warning", "Can not recieve data", "OK");
        }
        /**
         * Establishing a connection with the selected device,
           opening a window displaying information, updating information every half second
         */
        private async void DevicesList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            CurrentDevice = DevicesList.SelectedItem as IDevice;
            var result = await DisplayAlert("Warning", "Do you want to connect?", "Connect", "Cancel");
            if (!result)
                return;

            await Adapter.StopScanningForDevicesAsync();
            try
            {
                await Adapter.ConnectToDeviceAsync(CurrentDevice);
                CopyBtn.IsEnabled = true;

                UpdateTimer = new System.Timers.Timer(500);
                UpdateTimer.Elapsed += MyTimer_Elapsed;
                UpdateTimer.Start();
                popupLoadingView.IsVisible = true;

                DeviceInformation.Text =
                $"<span style=\"color:blue\">State: </span><span style=\"color:dark-gray\">{CurrentDevice.State}</span><br><br>" +
                $"<span style=\"color:blue\">Name: </span>{CurrentDevice.Name}<br><br>" +
                $"<span style=\"color:blue\">ID: </span>{CurrentDevice.Id}<br><br>" +
                $"<span style=\"color:blue\">Adress: </span>{CurrentDevice.NativeDevice}<br><br>" +
                $"<span style=\"color:blue\">Rssi (signal): </span>{CurrentDevice.Rssi} dBm<br><br>" +
                $"<span style=\"color:blue\">Distance: </span>{distanceDetermination(CurrentDevice.Rssi)}";

                DeviceInfo = "State: " + CurrentDevice.State + "\nName: " + CurrentDevice.Name
                + "\nID: " + CurrentDevice.Id + "\nAdress: " + CurrentDevice.NativeDevice + "\nRssi: " + CurrentDevice.Rssi
                + "\nDistance: " + distanceDetermination(CurrentDevice.Rssi);
            }
            catch (DeviceConnectionException ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        /**
         * Implementation of the function of updating information every half second
         */
        private async void MyTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await CurrentDevice.UpdateRssiAsync();
            Device.BeginInvokeOnMainThread(() =>
            {
                DeviceInformation.Text = null;
                DeviceInformation.Text =
                $"<span style=\"color:blue\">State: </span>{CurrentDevice.State}<br><br>" +
                $"<span style=\"color:blue\">Name: </span>{CurrentDevice.Name}<br><br>" +
                $"<span style=\"color:blue\">ID: </span>{CurrentDevice.Id}<br><br>" +
                $"<span style=\"color:blue\">Adress: </span>{CurrentDevice.NativeDevice}<br><br>" +
                $"<span style=\"color:blue\">Rssi (signal): </span>{CurrentDevice.Rssi} dBm<br><br>" +
                $"<span style=\"color:blue\">Distance: </span>{distanceDetermination(CurrentDevice.Rssi)}";

                DeviceInfo = null;
                DeviceInfo = "State: " + CurrentDevice.State + "\nName: " + CurrentDevice.Name
                + "\nID: " + CurrentDevice.Id + "\nAdress: " + CurrentDevice.NativeDevice + "\nRssi: " + CurrentDevice.Rssi + "\nDistance: " + distanceDetermination(CurrentDevice.Rssi);
            });
        }
        /**
         * Copy text to clipboard
         */
        private async void CopyShortInfo_Clicked(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(DeviceInfo);
            CrossToastPopUp.Current.ShowToastMessage("Copied to clipboard");
        }
        /**
         * Closing the window with short information about the selected device
         */
        private void CloseShortInfo_Clicked(object sender, EventArgs e)
        {
            popupInfosView.IsVisible = false;
            CopyShortInfo.IsEnabled = false;
            DeviceInfo = null;
            ShortDeviceInfo.Text = null;
        }
        /**
         * Search for available BLE devices, add them to
         'ListView' and update 'ListView' every 10 seconds
         */
        private async void searchDevice(object sender, EventArgs e)
        {
            if (BluetoothBLE.State == BluetoothState.Off)
            {
                await DisplayAlert("Warning", "Bluetooth disable", "OK");
            }
            else
            {
                DevicesList.IsVisible = true;
                DevicesCollectionList.Clear();

                Adapter.ScanTimeout = 10000;
                Adapter.ScanMode = ScanMode.Balanced;


                Adapter.DeviceDiscovered += (obj, a) =>
                {
                    if (!DevicesCollectionList.Contains(a.Device))
                        DevicesCollectionList.Add(a.Device);
                };

                await Adapter.StartScanningForDevicesAsync();
            }
        }
        /**
         * Implementation of the function for determining the approximate
           distance to the device about its RSSI
         */
        private string distanceDetermination(int rssi)
        {
            if (rssi >= -50)
                return "Immediate";
            else if (rssi < -50 && rssi > -75)
                return "Near";
            else
                return "Far";
        }
        /**
         * Copy short device information to clipboard
         */
        private async void CopyBtn_Clicked(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(DeviceInfo);
            CrossToastPopUp.Current.ShowToastMessage("Copied to clipboard");
        }
        /**
         * Implementation of the function to close the window with
           short information about the device and stop the timer
         */
        private void CloseBtn_Clicked(object sender, EventArgs e)
        {
            UpdateTimer.Stop();
            UpdateTimer.Dispose();
            CopyBtn.IsEnabled = false;
            popupLoadingView.IsVisible = false;
            DeviceInformation.Text = null;
            DeviceInfo = null;
        }
    }
}