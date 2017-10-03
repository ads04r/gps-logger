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
using Windows.Devices.Geolocation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace gps_logger
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

        private async System.Threading.Tasks.Task get_position()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            txtGPS.Text = "Waiting...";

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                    Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 10 };

                    // Carry out the operation.
                    Geoposition pos = await geolocator.GetGeopositionAsync();

                    txtGPS.Text = pos.Coordinate.Latitude.ToString() + ", " + pos.Coordinate.Longitude.ToString();
                    break;

                case GeolocationAccessStatus.Denied:
                    txtGPS.Text = "Access denied";
                    break;

                case GeolocationAccessStatus.Unspecified:
                    txtGPS.Text = "Cannot get a fix";
                    break;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await get_position();
        }
    }
}
