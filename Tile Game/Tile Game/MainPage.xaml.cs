/*
 * FILE        : MainPage.xaml.cs
 * PROJECT     : Windows and Mobile Programming - Final Project (Option 1 - Tile Game)
 * AUTHORS     : Richard Meijer & Connor McQuade
 * SUBMIT DATE : 12/10/2014
 * DESCRIPTION : Main Page for the game. Contains the play area and appbar with the options to: Select a photo, Create a new photo using the camera, 
 *               Randomize the tiles/Start the game, Solve the puzzle/Reset, Toggle numbers ON/OFF, and view the leaderboard.
 */
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
using Windows.UI.ViewManagement;
using Windows.Graphics.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;


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
        private bool toggleNum = false;
        private List<Tile> tiles = new List<Tile>();
        static readonly double[] xPosition = new double[5] { 0, 150, 300, 450, 600 };
        static readonly double[] yPosition = new double[5] { 0, 150, 300, 450, 600 };

        private bool leaderBoardOn = false;
        private bool didntClickAutoSolve = true;
        private double userHighScore;
        private double currentHighScore = 0.00;
        private string currentHighScoreName = "Name";
        private bool checkForWin = false;
        BitmapImage previewImage = new BitmapImage();
        WriteableBitmap mainImage = new WriteableBitmap(182, 182);
        private Stopwatch watch;
        private double totalTime;
        // DEBUGGING: Needed time variable to test win text
        private int time = 1;

        /* ----- CONSTRUCTOR ----- */
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            watch = new Stopwatch();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Tile t = new Tile(xPosition[j], yPosition[i], mainImage);
                    tiles.Add(t);
                }
            }
            //ToggleNumbers();
            ReloadTiles();
        }

        void CompositionTarget_Rendering(object sender, object e)
        {
            totalTime = watch.Elapsed.TotalSeconds;
            TimerText.Text = "Time Taken: " + String.Format("{0:0.00}", totalTime);
            userHighScore = watch.Elapsed.TotalSeconds;
            CheckTiles();
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
                previewImage = new BitmapImage();
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    previewImage.SetSource(fileStream);
                }
                appSettings[imageKey] = file.Path;

                SolvePuzzle();
                SplitPicture(file);
                //ToggleNumbers();
                ReloadImages();
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
                previewImage = new BitmapImage();
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    previewImage.SetSource(fileStream);
                }

                SolvePuzzle();
                SplitPicture(file);
                //ToggleNumbers();
                ReloadImages();
            }
            else
            {
                //OutputTextBlock.Text = "PANIC";
            }
        }

        private void btnRandomizeButtom_Click(object sender, RoutedEventArgs e)
        {
            didntClickAutoSolve = true;
            RandomizeTiles();
        }

        private void btnToggleNumber_Click(object sender, RoutedEventArgs e)
        {
            ToggleNumbers();
        }

        private void btnSolveButton_Click(object sender, RoutedEventArgs e)
        {
            SolvePuzzle();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Options.IsOpen = true;
        }

        private void submitNamePrompt_Click(object sender, RoutedEventArgs e)
        {
            submitNamePrompt.IsEnabled = false;
            enterNamePrompt.IsEnabled = false;
            submitNamePrompt.Opacity = 0;
            enterNamePrompt.Opacity = 0;
            currentHighScoreName = enterNamePrompt.Text;
            EnableBar();
        }

        private void btnViewLeaderboard_Click(object sender, RoutedEventArgs e)
        {
            //change to go to a new high score page
            if (leaderBoardOn == false)
            {
                watch.Stop();
                textBlockWin.Opacity = 100;
                textBlockWin.Text = "High Score:\n" + currentHighScoreName + " -> " + currentHighScore;
                DisableBar();
                btnRandomizeButtom.IsEnabled = false;
                GameGrid.Opacity = 0;
                leaderBoardOn = true;
            }
            else
            {
                if (watch.Elapsed.TotalSeconds > 0)
                {
                    watch.Start();
                }
                textBlockWin.Opacity = 0;
                textBlockWin.Text = "";
                EnableBar();
                btnRandomizeButtom.IsEnabled = true;
                GameGrid.Opacity = 100;
                leaderBoardOn = false;
            }

        }
        /* ----- BUTTON CLICKS END ----- */

        /* ----- METHODS - GAME LOGIC ----- */
        private void RandomizeTiles()
        {
            // Start the game
            gamePlay = true;
            
            List<Point> pointList = new List<Point>();
            int pointCount = 0;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridWidth; y++)
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
            checkForWin = true;
            ReloadTiles();
            watch.Restart();
        }

        async private void SplitPicture(StorageFile newFile)
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
                        await GetCroppedBitmap(newFile, new Point(XPosit[x], YPosit[y]), new Size(maxWidth, maxHeight), 1, nameNumber);
                        nameNumber++;
                    }
                }
            }
            ReloadImages();
        }

        private async Task GetCroppedBitmap(StorageFile originalImgFile, Point startPoint, Size cropSize, double scale, int whichTile)
        {
            btnRandomizeButtom.IsEnabled = true;
            checkForWin = false;
            textBlockWin.Text = "";
            GameGrid.Opacity = 100;
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

        private void ReloadImages()
        {
            if (!toggleNum)
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
        }

        private void ReloadTiles()
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

            if (toggleNum == true)
            {
                //Textblocks
                txtTile1.Margin = new Thickness(tiles[0].xPositon, tiles[0].yPositon, 0, 0);
                txtTile2.Margin = new Thickness(tiles[1].xPositon, tiles[1].yPositon, 0, 0);
                txtTile3.Margin = new Thickness(tiles[2].xPositon, tiles[2].yPositon, 0, 0);
                txtTile4.Margin = new Thickness(tiles[3].xPositon, tiles[3].yPositon, 0, 0);
                txtTile5.Margin = new Thickness(tiles[4].xPositon, tiles[4].yPositon, 0, 0);
                txtTile6.Margin = new Thickness(tiles[5].xPositon, tiles[5].yPositon, 0, 0);
                txtTile7.Margin = new Thickness(tiles[6].xPositon, tiles[6].yPositon, 0, 0);
                txtTile8.Margin = new Thickness(tiles[7].xPositon, tiles[7].yPositon, 0, 0);
                txtTile9.Margin = new Thickness(tiles[8].xPositon, tiles[8].yPositon, 0, 0);
                txtTile10.Margin = new Thickness(tiles[9].xPositon, tiles[9].yPositon, 0, 0);
                txtTile11.Margin = new Thickness(tiles[10].xPositon, tiles[10].yPositon, 0, 0);
                txtTile12.Margin = new Thickness(tiles[11].xPositon, tiles[11].yPositon, 0, 0);
                txtTile13.Margin = new Thickness(tiles[12].xPositon, tiles[12].yPositon, 0, 0);
                txtTile14.Margin = new Thickness(tiles[13].xPositon, tiles[13].yPositon, 0, 0);
                txtTile15.Margin = new Thickness(tiles[14].xPositon, tiles[14].yPositon, 0, 0);
            }
            CheckTiles();
        }

        private void GameGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            double clickPointY = e.GetCurrentPoint(GameGrid).Position.Y;
            double clickPointX = e.GetCurrentPoint(GameGrid).Position.X;

            if (gamePlay && tiles.Count() != 0)
            {
                for (int i = 0; i < numTiles; i++)
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
            ReloadTiles();
        }

        private void ReleaseTiles()
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
                ReloadTiles();
                CheckTiles();
            }
        }      

        private void GameGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            double clickPointY = e.GetCurrentPoint(GameGrid).Position.Y;
            double clickPointX = e.GetCurrentPoint(GameGrid).Position.X;

            ReleaseTiles();

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

        private void SolvePuzzle()
        {
            watch.Stop();
            watch.Reset();
            gamePlay = false;
            didntClickAutoSolve = false;
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

            for (int i = 0; i < numTiles; i++)
            {
                tiles[i].setPoint(pointList[i].X, pointList[i].Y);
                tiles[i].setMinMax();
            }
            ReloadTiles();
        }

        private void ToggleNumbers()
        {
            SolidColorBrush b = new SolidColorBrush(Windows.UI.Colors.Black);
            
            ReloadTiles();
            toggleNum = !toggleNum;

            // Toggles values
            if (toggleNum)
            {
                btnToggleNumber.Content = "Toggle\nNumbers\n(ON)";
            }
            else
            {
                btnToggleNumber.Content = "Toggle\nNumbers\n(OFF)";
            }

            // Displays the numbers
            if (toggleNum == false)
            {
                Tile1.Fill = tile1ImageBrush;
                Tile2.Fill = tile2ImageBrush;
                Tile3.Fill = tile3ImageBrush;
                Tile4.Fill = tile4ImageBrush;
                Tile5.Fill = tile5ImageBrush;
                Tile6.Fill = tile6ImageBrush;
                Tile7.Fill = tile7ImageBrush;
                Tile8.Fill = tile8ImageBrush;
                Tile9.Fill = tile9ImageBrush;
                Tile10.Fill = tile10ImageBrush;
                Tile11.Fill = tile11ImageBrush;
                Tile12.Fill = tile12ImageBrush;
                Tile13.Fill = tile13ImageBrush;
                Tile14.Fill = tile14ImageBrush;
                Tile15.Fill = tile15ImageBrush;
                txtTile1.Text = "";
                txtTile2.Text = "";
                txtTile3.Text = "";
                txtTile4.Text = "";
                txtTile5.Text = "";
                txtTile6.Text = "";
                txtTile7.Text = "";
                txtTile8.Text = "";
                txtTile9.Text = "";
                txtTile10.Text = "";
                txtTile11.Text = "";
                txtTile12.Text = "";
                txtTile13.Text = "";
                txtTile14.Text = "";
                txtTile15.Text = "";
            }
            else
            {
                txtTile1.Text = " 1";
                txtTile2.Text = " 2";
                txtTile3.Text = " 3";
                txtTile4.Text = " 4";
                txtTile5.Text = " 5";
                txtTile6.Text = " 6";
                txtTile7.Text = " 7";
                txtTile8.Text = " 8";
                txtTile9.Text = " 9";
                txtTile10.Text = "10";
                txtTile11.Text = "11";
                txtTile12.Text = "12";
                txtTile13.Text = "13";
                txtTile14.Text = "14";
                txtTile15.Text = "15";
                Tile1.Fill = b;
                Tile2.Fill = b;
                Tile3.Fill = b;
                Tile4.Fill = b;
                Tile5.Fill = b;
                Tile6.Fill = b;
                Tile7.Fill = b;
                Tile8.Fill = b;
                Tile9.Fill = b;
                Tile10.Fill = b;
                Tile11.Fill = b;
                Tile12.Fill = b;
                Tile13.Fill = b;
                Tile14.Fill = b;
                Tile15.Fill = b;
            }
            ReloadTiles();
            ReloadImages();
        }

        private void CheckTiles()
        {
            bool correctOrder = true;
            int i = 0;

            //run though each tile and check that it in the right order
            for (int y = 0; y < gridWidth; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    //if the positons are what they should be if its in the order than we do nothing

                    if (tiles[i].xPositon == xPosition[x] && tiles[i].yPositon == yPosition[y]) { }
                    //if the are not we set the bool to false
                    else
                    {
                        correctOrder = false;
                        textBlockWin.Text = "";
                    }
                    i++;
                }
            }
            //tell the user
            if (correctOrder && gamePlay && checkForWin)
            {
                HasWon();
                checkForWin = false;
            }
        } 



        private void MainMenuGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseTiles();
        }

        private void GameGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseTiles();
        }

        private void Page_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleaseTiles();
        }

        private void HasWon()
        {
            watch.Stop();
            userHighScore = watch.Elapsed.TotalSeconds;
            // Display message, save time & redirect to leaderboard
            if (watch.Elapsed.TotalSeconds > 0 && didntClickAutoSolve == true)
            {
                btnRandomizeButtom.IsEnabled = false;
                GameGrid.Opacity = 0;
                CompareHighScore();
            }
            didntClickAutoSolve = true;
            
        }
        private void CompareHighScore()
        {
            
            if (userHighScore > currentHighScore)
            {
                textBlockWin.Text = "Congratulations! High Score!\nEnter your name then Load another picture to play again!";
                DisableBar();
                submitNamePrompt.IsEnabled = true;
                enterNamePrompt.IsEnabled = true;
                submitNamePrompt.Opacity = 100;
                enterNamePrompt.Opacity = 100;
                currentHighScore = userHighScore;
                watch.Reset();
            }
            else
            {
                textBlockWin.Text = "Congratulations!\n Load another picture to play again!";
            }
        }
        
        private void DisableBar()
        {
            btnLoadCamera.IsEnabled = false;
            btnLoadPicture.IsEnabled = false;
            btnRandomizeButtom.IsEnabled = false;
            btnSolveButton.IsEnabled = false;
            btnToggleNumber.IsEnabled = false;
        }
        private void EnableBar()
        {
            btnLoadCamera.IsEnabled = true;
            btnLoadPicture.IsEnabled = true;
            btnRandomizeButtom.IsEnabled = true;
            btnSolveButton.IsEnabled = true;
            btnToggleNumber.IsEnabled = true;
        }
    }
}
