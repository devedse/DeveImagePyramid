﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devedse.DeveImagePyramid
{
    public class PretzelImageCombined
    {
        private PretzelImage[,] _innerImages;

        private int _tileWidth;
        private int _tileHeight;

        public int TileWidth { get { return _tileWidth; } }
        public int TileHeight { get { return _tileHeight; } }

        public PretzelImageCombined(PretzelImage topLeft, PretzelImage bottomLeft, PretzelImage topRight, PretzelImage bottomRight)
        {
            if (topLeft.Width != topRight.Width || topRight.Width != bottomLeft.Width || bottomLeft.Width != bottomRight.Width || topLeft.Height != topRight.Height || topRight.Height != bottomLeft.Height || bottomLeft.Height != bottomRight.Height)
            {
                throw new ArgumentException("Not all images are of the same size");
            }

            _innerImages = new PretzelImage[,] { { topLeft, bottomLeft }, { topRight, bottomRight } };

            _tileWidth = topLeft.Width;
            _tileHeight = topLeft.Height;
        }

        public Pixel GetPixel(int x, int y)
        {
            int xTile = x / _tileWidth;
            int yTile = y / _tileHeight;

            int xPos = x % _tileWidth;
            int yPos = y % _tileHeight;

            var thisTile = _innerImages[xTile, yTile].Data;
            var startPos = yPos * 3 * _tileWidth + xPos * 3;

            byte r = thisTile[startPos + 0];
            byte g = thisTile[startPos + 1];
            byte b = thisTile[startPos + 2];

            var pixelHere = new Pixel(r, g, b);
            return pixelHere;
        }
    }
}
