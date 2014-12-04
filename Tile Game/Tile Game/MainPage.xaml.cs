using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.ApplicationSettings;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Storage;
using Windows.Storage.Pickers;

using Windows.Media;
using Windows.Media.Capture;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace Tile_Game
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        //when cutting the image each square should be 182x182
        private Windows.Foundation.Collections.IPropertySet appSettings;
        private const String imageKey = "currentImage";
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void CompositionTarget_Rendering(object sender, object e)
        {

        }

        private async void OpenImgBtn_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario
            //OutputTextBlock.Text = "";
            PlayGameBtn.IsEnabled = false;
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                //OutputTextBlock.Text = "Picked photo: " + file.Name;
                BitmapImage bitmapImage = new BitmapImage();
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    bitmapImage.SetSource(fileStream);
                }
                MainImage.Source = bitmapImage;


              //  appSettings[imageKey] = file.Path;
            }
            else
            {
                //OutputTextBlock.Text = "Operation cancelled.";
            }
        }

        private void ScrambleImgBtn_Click(object sender, RoutedEventArgs e)
        {
            PlayGameBtn.IsEnabled = true;
        }

        private void PlayGameBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void TakePicBtn_Click(object sender, RoutedEventArgs e)
        {
            PlayGameBtn.IsEnabled = false;
            CameraCaptureUI mainCamera = new CameraCaptureUI();
            mainCamera.PhotoSettings.CroppedAspectRatio = new Size(16, 9);
            appSettings = ApplicationData.Current.LocalSettings.Values;
            StorageFile file = await mainCamera.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (file != null)
            {
                BitmapImage bitmapImage = new BitmapImage();
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    bitmapImage.SetSource(fileStream);
                }
                MainImage.Source = bitmapImage;
                appSettings[imageKey] = file.Path;
            }
        }
    }
}
