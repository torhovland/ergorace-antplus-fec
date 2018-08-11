using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void TopLevelNav_OnLoaded(object sender, RoutedEventArgs e)
        {
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
