﻿using System;
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
using Windows.UI.ViewManagement;
using Windows.Graphics.Imaging;
using System.Threading;
using System.Threading.Tasks;



namespace Tile_Game
{
    /// <summary>
    /// MainPage.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Windows.Foundation.Collections.IPropertySet appSettings;
        private const String imageKey = "currentImage";
        private int tileWidth = 150;
        private int gridWidth = 4;
        private int numTiles = 15;
        private int borderWidth = 600;
        private bool gamePlay = true;
        private List<Tile> tiles = new List<Tile>();
        static readonly double[] xPosition = new double[5] { 0, 150, 300, 450, 600 };
        static readonly double[] yPosition = new double[5] { 0, 150, 300, 450, 600 };
        BitmapImage previewImage = new BitmapImage();
        WriteableBitmap mainImage = new WriteableBitmap(182, 182);

        /* ----- CONSTRUCTOR ----- */
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Tile t = new Tile(xPosition[i], yPosition[j], mainImage);
                    tiles.Add(t);
                }
            }

            reloadTiles();
            refreshImages();
        }

        /* ----- BUTTON CLICKS START ----- */
        private async void btnLoadCamera_Click(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI mainCamera = new CameraCaptureUI();
            mainCamera.PhotoSettings.CroppedAspectRatio = new Size(16, 9);
            appSettings = ApplicationData.Current.LocalSettings.Values;
            StorageFile file = await mainCamera.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (file != null)
            {
                BitmapImage previewImage = new BitmapImage();
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    previewImage.SetSource(fileStream);
                }
                appSettings[imageKey] = file.Path;
                dividePicture(file);
                refreshImages();
                randomizeTiles();
            }
        }

        private async void btnLoadPicture_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario
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
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    previewImage.SetSource(fileStream);
                }

                dividePicture(file);
                refreshImages();
                randomizeTiles();
            }
            else
            {
                //OutputTextBlock.Text = "PANIC";
            }
        }

        private void btnRandomizeButtom_Click(object sender, RoutedEventArgs e)
        {
            randomizeTiles();
        }
        /* ----- BUTTON CLICKS END ----- */

        /* ----- METHODS - GAME LOGIC ----- */
        private void randomizeTiles()
        {
            // Start the game (to add: start a timer)
            gamePlay = true;

            List<Point> pointList = new List<Point>();
            int pointCount = 0;

            for (int y = 0; y < gridWidth; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (x == (gridWidth - 1) && y == (gridWidth - 1)) { }
                    else
                    {
                        Point point = new Point(xPosition[x], yPosition[y]);
                        pointList.Add(point);
                        pointCount++;
                    }
                }
            }

            //randomize this list
            Random rng = new Random();
            int n = pointList.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Point value = pointList[k];
                pointList[k] = pointList[n];
                pointList[n] = value;
            }

            //apply the new start points
            for (int i = 0; i < numTiles; i++)
            {
                tiles[i].setPoint(pointList[i].X, pointList[i].Y);
                tiles[i].setMinMax();
            }
            reloadTiles();
        }

        async private void dividePicture(StorageFile newFile)
        {
            double maxHeight = previewImage.PixelHeight;
            double maxWidth = previewImage.PixelWidth;
            double[] XPosit = new double[gridWidth];
            double[] YPosit = new double[gridWidth];

            maxHeight /= gridWidth;
            maxWidth /= gridWidth;

            XPosit[0] = 0;
            YPosit[0] = 0;

            for (int i = 1; i < gridWidth; i++)
            {
                XPosit[i] = maxWidth * i;
                YPosit[i] = maxHeight * i;
            }

            int nameNumber = 0;

            for (int y = 0; y < gridWidth; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (nameNumber < numTiles)
                    {
                        await GetCroppedBitmapAsync(newFile, new Point(XPosit[x], YPosit[y]), new Size(maxWidth, maxHeight), 1, nameNumber);
                        nameNumber++;
                    }
                }
            }
            refreshImages();
        }

        async private Task GetCroppedBitmapAsync(StorageFile originalImgFile, Point startPoint, Size cropSize, double scale, int whichTile)
        {
            if (double.IsNaN(scale) || double.IsInfinity(scale))
            {
                scale = 1;
            }

            uint startPointX = (uint)Math.Floor(startPoint.X * scale);
            uint startPointY = (uint)Math.Floor(startPoint.Y * scale);
            uint height = (uint)Math.Floor(cropSize.Height * scale);
            uint width = (uint)Math.Floor(cropSize.Width * scale);

            using (IRandomAccessStream stream = await originalImgFile.OpenReadAsync())
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                uint scaledWidth = (uint)Math.Floor(decoder.PixelWidth * scale);
                uint scaledHeight = (uint)Math.Floor(decoder.PixelHeight * scale);

                if (startPointX + width > scaledWidth)
                {
                    startPointX = scaledWidth - width;
                }

                if (startPointY + height > scaledHeight)
                {
                    startPointY = scaledHeight - height;
                }

                // Create cropping BitmapTransform and define the bounds... 
                BitmapTransform transform = new BitmapTransform();
                BitmapBounds bounds = new BitmapBounds();

                bounds.X = startPointX;
                bounds.Y = startPointY;
                bounds.Height = height;
                bounds.Width = width;
                transform.Bounds = bounds;

                transform.ScaledWidth = scaledWidth;
                transform.ScaledHeight = scaledHeight;

                // Get the cropped pixels within the bounds of transform...
                PixelDataProvider pix = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.ColorManageToSRgb);
                byte[] pixels = pix.DetachPixelData();

                // Stream the bytes into a WriteableBitmap...
                tiles[whichTile].tileImage = new WriteableBitmap((int)width, (int)height);
                Stream pixStream = tiles[whichTile].tileImage.PixelBuffer.AsStream();
                pixStream.Write(pixels, 0, (int)(width * height * gridWidth));

                return;
            }
        }

        private void refreshImages()
        {
            tile1ImageBrush.ImageSource = tiles[0].tileImage;
            tile2ImageBrush.ImageSource = tiles[1].tileImage;
            tile3ImageBrush.ImageSource = tiles[2].tileImage;
            tile4ImageBrush.ImageSource = tiles[3].tileImage;
            tile5ImageBrush.ImageSource = tiles[4].tileImage;
            tile6ImageBrush.ImageSource = tiles[5].tileImage;
            tile7ImageBrush.ImageSource = tiles[6].tileImage;
            tile8ImageBrush.ImageSource = tiles[7].tileImage;
            tile9ImageBrush.ImageSource = tiles[8].tileImage;
            tile10ImageBrush.ImageSource = tiles[9].tileImage;
            tile11ImageBrush.ImageSource = tiles[10].tileImage;
            tile12ImageBrush.ImageSource = tiles[11].tileImage;
            tile13ImageBrush.ImageSource = tiles[12].tileImage;
            tile14ImageBrush.ImageSource = tiles[13].tileImage;
            tile15ImageBrush.ImageSource = tiles[14].tileImage;
        }

        private void reloadTiles()
        {
            //Rectangles
            Tile1.Margin = new Thickness(tiles[0].xPositon, tiles[0].yPositon, 0, 0);
            Tile2.Margin = new Thickness(tiles[1].xPositon, tiles[1].yPositon, 0, 0);
            Tile3.Margin = new Thickness(tiles[2].xPositon, tiles[2].yPositon, 0, 0);
            Tile4.Margin = new Thickness(tiles[3].xPositon, tiles[3].yPositon, 0, 0);
            Tile5.Margin = new Thickness(tiles[4].xPositon, tiles[4].yPositon, 0, 0);
            Tile6.Margin = new Thickness(tiles[5].xPositon, tiles[5].yPositon, 0, 0);
            Tile7.Margin = new Thickness(tiles[6].xPositon, tiles[6].yPositon, 0, 0);
            Tile8.Margin = new Thickness(tiles[7].xPositon, tiles[7].yPositon, 0, 0);
            Tile9.Margin = new Thickness(tiles[8].xPositon, tiles[8].yPositon, 0, 0);
            Tile10.Margin = new Thickness(tiles[9].xPositon, tiles[9].yPositon, 0, 0);
            Tile11.Margin = new Thickness(tiles[10].xPositon, tiles[10].yPositon, 0, 0);
            Tile12.Margin = new Thickness(tiles[11].xPositon, tiles[11].yPositon, 0, 0);
            Tile13.Margin = new Thickness(tiles[12].xPositon, tiles[12].yPositon, 0, 0);
            Tile14.Margin = new Thickness(tiles[13].xPositon, tiles[13].yPositon, 0, 0);
            Tile15.Margin = new Thickness(tiles[14].xPositon, tiles[14].yPositon, 0, 0);
        }

        private void GameGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            double clickPointY = e.GetCurrentPoint(GameGrid).Position.Y;
            double clickPointX = e.GetCurrentPoint(GameGrid).Position.X;

            if (gamePlay && tiles.Count() != 0)
            {
                for (int i = 0; i < 15; i++)
                {
                    if (tiles[i].tileSelected)
                    {
                        //if the tiles selected can move LEFT/RIGHT/UP/DOWN...
                        if (tiles[i].moveLeft)
                        {
                            if (clickPointX >= tiles[i].xMin - (tileWidth / 2) && clickPointX <= tiles[i].xMax - (tileWidth / 2))
                            {
                                tiles[i].setPoint(clickPointX - (tileWidth / 2), tiles[i].yPositon);
                            }
                        }
                        if (tiles[i].moveRight)
                        {
                            if (clickPointX >= tiles[i].xMin + (tileWidth / 2) && clickPointX <= tiles[i].xMax + (tileWidth / 2))
                            {
                                tiles[i].setPoint(clickPointX - (tileWidth / 2), tiles[i].yPositon);
                            }
                        }
                        if (tiles[i].moveUp)
                        {
                            if (clickPointY >= tiles[i].yMin - (tileWidth / 2) && clickPointY <= tiles[i].yMax - (tileWidth / 2))
                            {
                                tiles[i].setPoint(tiles[i].xPositon, clickPointY - (tileWidth / 2));
                            }
                        }
                        if (tiles[i].moveDown)
                        {
                            if (clickPointY >= tiles[i].yMin + (tileWidth / 2) && clickPointY <= tiles[i].yMax + (tileWidth / 2))
                            {
                                tiles[i].setPoint(tiles[i].xPositon, clickPointY - (tileWidth / 2));
                            }
                        }
                    }
                }
            }
            reloadTiles();
        }

        private void releaseAllTiles()
        {
            if (tiles.Count() != 0)
            {
                for (int i = 0; i < numTiles; i++)
                {
                    if (tiles[i].tileSelected)
                    {
                        //snap the tile to the grid in the correct place
                        for (int a = 0; a < gridWidth; a++)
                        {
                            if (tiles[i].xPositon + (tileWidth / 2) > xPosition[a] && tiles[i].xPositon + (tileWidth / 2) < xPosition[a + 1])
                            {
                                tiles[i].setPoint(xPosition[a], tiles[i].yPositon);
                            }
                            if (tiles[i].yPositon + (tileWidth / 2) > yPosition[a] && tiles[i].yPositon + (tileWidth / 2) < yPosition[a + 1])
                            {
                                tiles[i].setPoint(tiles[i].xPositon, yPosition[a]);
                            }
                            if (tiles[i].xPositon < 0)
                            {
                                tiles[i].setPoint(0, tiles[i].yPositon);
                            }
                            if (tiles[i].yPositon < 0)
                            {
                                tiles[i].setPoint(tiles[i].xPositon, 0);
                            }
                            tiles[i].setMinMax();
                        }
                    }
                    tiles[i].tileSelected = false;
                }
                reloadTiles();
            }
        }      

        private void GameGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            double clickPointY = e.GetCurrentPoint(GameGrid).Position.Y;
            double clickPointX = e.GetCurrentPoint(GameGrid).Position.X;

            releaseAllTiles();

            if (gamePlay && tiles.Count() != 0)
            {
                for (int i = 0; i < numTiles; i++)
                {
                    if (tiles[i].xMin < clickPointX && clickPointX < tiles[i].xMax &&
                        tiles[i].yMin < clickPointY && clickPointY < tiles[i].yMax)
                    {
                        tiles[i].tileSelected = true;

                        //determine and save which way the tile can move
                        tiles[i].moveLeft = true;
                        tiles[i].moveRight = true;
                        tiles[i].moveUp = true;
                        tiles[i].moveDown = true;

                        for (int a = 0; a < numTiles; a++)
                        {
                            if (tiles[i].xMin == 0)
                                tiles[i].moveLeft = false;
                            if (tiles[i].xMax == borderWidth)
                                tiles[i].moveRight = false;
                            if (tiles[i].yMin == 0)
                                tiles[i].moveUp = false;
                            if (tiles[i].yMax == borderWidth)
                                tiles[i].moveDown = false;

                            if (tiles[i].yPositon == tiles[a].yPositon && tiles[i] != tiles[a])
                            {
                                if (tiles[a].xMin == tiles[i].xMax)
                                    tiles[i].moveRight = false;
                                if (tiles[a].xMax == tiles[i].xMin)
                                    tiles[i].moveLeft = false;
                            }
                            if (tiles[i].xPositon == tiles[a].xPositon && tiles[i] != tiles[a])
                            {
                                if (tiles[a].yMin == tiles[i].yMax)
                                    tiles[i].moveDown = false;
                                if (tiles[a].yMax == tiles[i].yMin)
                                    tiles[i].moveUp = false;
                            }
                        }
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Options.IsOpen = true;
        }

        private void MainMenuGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            releaseAllTiles();
        }

        private void GameGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            releaseAllTiles();
        }

        private void Page_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            releaseAllTiles();
        }

        private void hasWon()
        {

        }
    }
}
