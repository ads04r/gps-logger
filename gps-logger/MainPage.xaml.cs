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
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace gps_logger
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private String sCurrentFile;
        public LogFileViewModel ViewModel { get; set; }
        private LogFile selected;

        public MainPage()
        {
            this.InitializeComponent();
            sCurrentFile = "";
            this.ViewModel = new LogFileViewModel();
        }

        private async System.Threading.Tasks.Task log_position(double lat, double lon)
        {
            StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;
            StorageFolder sdCard = (await externalDevices.GetFoldersAsync()).FirstOrDefault();

            if (sdCard != null)
            {
                Windows.Storage.StorageFile sampleFile = await sdCard.CreateFileAsync("gps.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(sampleFile, lat.ToString() + "," + lon.ToString());
            }
        }

        private async System.Threading.Tasks.Task get_position()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            txtLat.Text = "";
            txtLon.Text = "";
            txtTime.Text = "";
            String sLine = "";

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                    Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 10 };

                    // Carry out the operation.
                    Geoposition pos = await geolocator.GetGeopositionAsync();

                    // await log_position(pos.Coordinate.Latitude, pos.Coordinate.Longitude);
                    //txtGPS.Text = pos.Coordinate.Latitude.ToString() + ", " + pos.Coordinate.Longitude.ToString();
                    txtLat.Text = pos.Coordinate.Latitude.ToString();
                    txtLon.Text = pos.Coordinate.Longitude.ToString();
                    txtTime.Text = DateTime.Now.ToString("h:mm");
                    sLine = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + txtLat.Text + "\t" + txtLon.Text + "\t5\n";

                    StorageFolder fLocal = ApplicationData.Current.LocalCacheFolder;
                    StorageFile fLogFile = await fLocal.CreateFileAsync(sCurrentFile, CreationCollisionOption.OpenIfExists);
                    await FileIO.AppendTextAsync(fLogFile, sLine);

                    break;

                case GeolocationAccessStatus.Denied:
                    //txtGPS.Text = "Access denied";
                    break;

                case GeolocationAccessStatus.Unspecified:
                    //txtGPS.Text = "Cannot get a fix";
                    break;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sCurrentFile == "")
            {
                sCurrentFile = "gps_" + DateTime.UtcNow.ToString("yyyy_MM_dd") + ".txt";
            }
            txtStartPosition.Text = "";
            await get_position();
            await ViewModel.Refresh();
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot p = (Pivot)sender;
            PivotItem pi = (PivotItem)p.SelectedItem;
            string header = (string)pi.Header;

            if (header != "Journeys")
            {
                await ViewModel.Refresh();
            }
        }

        private async void Remove_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.Popups.MessageDialog showDialog = new Windows.UI.Popups.MessageDialog("Are you sure you want to delete " + selected.file.Name + "?");
            showDialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes")
            {
                Id = 0
            });
            showDialog.Commands.Add(new Windows.UI.Popups.UICommand("No")
            {
                Id = 1
            });
            showDialog.DefaultCommandIndex = 0;
            showDialog.CancelCommandIndex = 1;
            var result = await showDialog.ShowAsync();
            if ((int)result.Id == 0)
            {
                await selected.file.DeleteAsync();
                await ViewModel.Refresh();
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Plain Text", new List<String>() { ".txt" });
            savePicker.SuggestedFileName = selected.file.Name;
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if(file != null)
            {
                await selected.file.CopyAndReplaceAsync(file);
            }
        }

        private async void SaveAll_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FolderPicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            Windows.Storage.StorageFolder path = await savePicker.PickSingleFolderAsync();
            if (path != null)
            {
                for(int i = 0; i < lvFiles.Items.Count; i++)
                {
                    LogFile lf = (LogFile)lvFiles.Items[i];
                    Windows.Storage.StorageFile newfile = await path.CreateFileAsync(lf.file.Name, CreationCollisionOption.ReplaceExisting);
                    if (newfile != null)
                    {
                        await lf.file.CopyAndReplaceAsync(newfile);
                    }
                }
            }
        }

        private async void RemoveAll_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.Popups.MessageDialog showDialog = new Windows.UI.Popups.MessageDialog("Are you sure you want to delete ALL your GPS logs?");
            showDialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes")
            {
                Id = 0
            });
            showDialog.Commands.Add(new Windows.UI.Popups.UICommand("No")
            {
                Id = 1
            });
            showDialog.DefaultCommandIndex = 0;
            showDialog.CancelCommandIndex = 1;
            var result = await showDialog.ShowAsync();
            if ((int)result.Id == 0)
            {
                for (int i = 0; i < lvFiles.Items.Count; i++)
                {
                    LogFile lf = (LogFile)lvFiles.Items[i];
                    await lf.file.DeleteAsync();
                }

                await ViewModel.Refresh();
            }
        }

        private void lvFiles_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ListView lv = (ListView)sender;
            TextBlock tb = (TextBlock)e.OriginalSource;
            selected = (LogFile)tb.DataContext;
            lvFileMenu.ShowAt(lv, e.GetPosition(lv));
        }
    }
}
