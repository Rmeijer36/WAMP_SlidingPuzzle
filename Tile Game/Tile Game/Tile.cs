using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Tile_Game
{
    public class Tile
    {
        public double tileSize = 150;
        public double xPositon;
        public double yPositon;
        public double xMax;
        public double xMin;
        public double yMax;
        public double yMin;
        
        public WriteableBitmap tileImage;
        
        public bool tileSelected;
        public bool moveLeft;
        public bool moveRight;
        public bool moveUp;
        public bool moveDown;

        public Tile(double x, double y, WriteableBitmap newTileImage)
        {
            xPositon = x;
            yPositon = y;
            tileSelected = false;
            tileImage = newTileImage;
            xMin = xPositon;
            yMin = yPositon;
            xMax = xMin + tileSize;
            yMax = yMin + tileSize;
        }     
   
        public void setPoint(double x, double y)
        {
            xPositon = x;
            yPositon = y;
         }       

        public void setMinMax()
        {
            xMin = xPositon;
            yMin = yPositon;
            xMax = xMin + tileSize;
            yMax = yMin + tileSize;
        }
    }
}