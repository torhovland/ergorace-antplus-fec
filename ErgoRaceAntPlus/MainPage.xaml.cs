using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewBackRequestedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs;
using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ErgoRaceAntPlus
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        readonly List<DeviceInformation> comPorts = new List<DeviceInformation>();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void TopLevelNav_OnLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var port in await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector()))
            {
                comPorts.Add(port);

                using (var serialPort = await SerialDevice.FromIdAsync(port.Id))
                using (var inputStream = serialPort.InputStream)
                using (var outputStream = serialPort.OutputStream)
                using (var reader = new DataReader(inputStream))
                using (var writer = new DataWriter(outputStream))
                {
                    serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                    serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                    serialPort.BaudRate = 9600;
                    serialPort.Parity = SerialParity.None;
                    serialPort.StopBits = SerialStopBitCount.One;
                    serialPort.DataBits = 8;
                    serialPort.Handshake = SerialHandshake.None;

                    writer.WriteString("VE\r\n");
                    await writer.StoreAsync();

                    reader.InputStreamOptions = InputStreamOptions.Partial;

                    for (int i = 0; i < 10; i++)
                    {
                        var length = await reader.LoadAsync(2048);

                        if (length > 0)
                        {
                            var s = reader.ReadString(length);
                            var t = s;
                        }

                        await Task.Delay(500);
                    }
                }
            }

            TopLevelNav.SelectedItem = TopLevelNav.MenuItems[0];
            ContentFrame.Navigate(typeof(DashboardPage));
        }

        private void TopLevelNav_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            TopLevelNav.IsBackEnabled = true;
            TopLevelNav.SelectedItem = args.IsSettingsInvoked ? null : TopLevelNav.MenuItems[0];
            ContentFrame.Navigate(args.IsSettingsInvoked ? typeof(SettingsPage) : typeof(DashboardPage));
        }

        private void TopLevelNav_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
                ContentFrame.GoBack();

            TopLevelNav.SelectedItem = ContentFrame.Content is SettingsPage ? null : TopLevelNav.MenuItems[0];
            TopLevelNav.IsBackEnabled = ContentFrame.CanGoBack;
        }
    }
}
