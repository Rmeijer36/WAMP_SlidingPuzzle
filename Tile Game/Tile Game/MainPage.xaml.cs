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
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.UI.Core;


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
        private int tileHeight;
        private int tileWidth;
        private Rectangle[][] imageTiles;
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            tileWidth = (int) TileCanvas.ActualWidth / 4;
            tileHeight = (int) TileCanvas.ActualHeight / 4;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Rectangle rect = new Rectangle();
                    rect.Width = tileWidth;
                    rect.Height = tileHeight;
                    rect.Fill = new SolidColorBrush(Colors.Aqua);
                    TileGrid.Children.Add(rect);
                    Grid.SetColumn(rect, i);
                    Grid.SetRow(rect, j);
                }
                
            }
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

                for (int i = 0; i < 3; i++)
                {
                     for (int j = 0; j < 3; j++)
                     {
                         //crop the image and put it into each rectangle respectivly
                            //objImg[count++] = new CroppedBitmap(src, new Int32Rect(j * 120, i * 120, 120, 120));
                     }
                }
                   
              //  appSettings[imageKey] = file.Path;
            }
            else
            {
                //OutputTextBlock.Text = "PANIC";
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


                appSettings[imageKey] = file.Path;
            }
        }
    }
}
