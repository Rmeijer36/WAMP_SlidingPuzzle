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
using Windows.Graphics.Imaging;


namespace Tile_Game
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //framework is a very nice thing indeed!
        FrameworkElement[] tiles = new FrameworkElement[16];
        private Windows.Foundation.Collections.IPropertySet appSettings;
        private const String imageKey = "currentImage";
        private int tileHeight;
        private int tileWidth;
        private List<Rectangle> ImgTiles;
        private StorageFile pictureCopy;
        private Rectangle[][] imageTiles {get; set;}
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            tileWidth = (int) TileCanvas.ActualWidth / 4;
            tileHeight = (int) TileCanvas.ActualHeight / 4;
            ImgTiles = new List<Rectangle>();
            /*
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Rectangle rect = new Rectangle();
                    rect.Width = tileWidth;
                    rect.Height = tileHeight;
                    rect.Fill = new SolidColorBrush(Colors.Aqua);
                    rect.Stroke = new SolidColorBrush(Colors.Black);
                    rect.StrokeThickness = 1;
                    ImgTiles.Add(rect);
                    TileGrid.Children.Add(rect);
                    Grid.SetColumn(rect, i);
                    Grid.SetRow(rect, j);
                }
                
            }*/
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
            TakePicBtn.IsEnabled = false;
            OpenImgBtn.IsEnabled = false;
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            StorageFile file = await openPicker.PickSingleFileAsync();
            
            //a copy of the picture to give to other componants for later use (a backup)
            pictureCopy = file;
            if (file != null)
            {
                // Application now has read/write access to the picked file
                //OutputTextBlock.Text = "Picked photo: " + file.Name;
                BitmapImage bitmapImage = new BitmapImage();
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    configurePicture(fileStream);
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
            //run scramble method
        }

        private void PlayGameBtn_Click(object sender, RoutedEventArgs e)
        {
            TakePicBtn.IsEnabled = false;
            OpenImgBtn.IsEnabled = false;
        }

        private async void TakePicBtn_Click(object sender, RoutedEventArgs e)
        {
            PlayGameBtn.IsEnabled = false;
            TakePicBtn.IsEnabled = false;
            OpenImgBtn.IsEnabled = false;

            CameraCaptureUI mainCamera = new CameraCaptureUI();
            mainCamera.PhotoSettings.CroppedAspectRatio = new Size(16, 9);
            appSettings = ApplicationData.Current.LocalSettings.Values;
            StorageFile file = await mainCamera.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (file != null)
            {
                using (IRandomAccessStream fileStream = await file.OpenReadAsync())
                {
                    configurePicture(fileStream);
                }


                appSettings[imageKey] = file.Path;
            }
        }

        private async void configurePicture(IRandomAccessStream fileStream)
        {
            //http://www.microsoft.com/en-us/showcase/details.aspx?uuid=bef08d57-fa4d-4d9c-9080-6ee55b8623c0
            //Image incoder object
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
            //image size object
            BitmapBounds tileBounds = new BitmapBounds();
            //image cropper object
            BitmapTransform transform = new BitmapTransform();

            //height and width are set to the size of 1 square, quarter of the full picture
            tileBounds.Height = (uint)decoder.PixelHeight / 4;
            tileBounds.Width = (uint)decoder.PixelWidth / 4;

            for (int i = 0; i < 16; ++i)
            {
                TileGrid.Children.Remove(tiles[i]);
                //selects the starting point to get the image piece from
                tileBounds.X = (uint)((i % 4) * tileBounds.Width);
                tileBounds.Y = (uint)((i / 4) * tileBounds.Height);
                transform.Bounds = tileBounds;

                //read image piece to byte array
                PixelDataProvider pixelProvider = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.ColorManageToSRgb);
                byte[] pixels = pixelProvider.DetachPixelData();

                //convert byte arrray to bitmap image
                WriteableBitmap bitmap = new WriteableBitmap((int)tileBounds.Width, (int)tileBounds.Height);
                Stream pixelStream = bitmap.PixelBuffer.AsStream();
                pixelStream.Write(pixels, 0, (int)(tileBounds.Height * tileBounds.Width * 4));

                //create ui element from bitmap
                tiles[i] = new Image();
                tiles[i].MaxHeight = 150;
                tiles[i].MaxWidth = 300;
                ((Image)tiles[i]).Source = bitmap;
                tiles[i].UseLayoutRounding = true;

                TileGrid.Children.Add(tiles[i]);
                Grid.SetColumn(tiles[i], i % 4);
                Grid.SetRow(tiles[i], i / 4);

                //add the method for tile movement to each tile
                tiles[i].Tapped += Tile_Tapped;
            }
        }
        private void hasWon()
        {

        }
        private void Tile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //take information of the moving square and the blank square and use it to check if the image is complete
            int senderRow = Grid.GetRow((FrameworkElement)sender);
            int senderCol = Grid.GetColumn((FrameworkElement)sender);
            int blankRow = Grid.GetRow(tiles[15]);
            int blankCol = Grid.GetColumn(tiles[15]);

            //if the object clicked on are only one square apart in one direction and 0 squares apart in the other
            if (senderCol == blankCol && Math.Abs(senderRow - blankRow) == 1 || senderRow == blankRow && Math.Abs(senderCol - blankCol) == 1)
            {
                //swap locations of the objects
                Grid.SetColumn((FrameworkElement)sender, blankCol);
                Grid.SetColumn(tiles[15], senderCol);

                Grid.SetRow((FrameworkElement)sender, blankRow);
                Grid.SetRow(tiles[15], senderRow);
            }

            //check if the player has won
            hasWon();
        }

        
    }


}
