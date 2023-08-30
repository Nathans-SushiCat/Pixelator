using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Drawing;
using Image = System.Windows.Controls.Image;
using Pixelator.Models;
using Pixelator.Extentions;
using Pixelator.Helper;

namespace Pixelator.Core
{
    internal class ImageConverter
    {

        public static BitmapSource DeleteImageBorder(BitmapSource img, double tolerance)
        {

            int offsetLeft = int.MaxValue;
            int offsetRight = int.MaxValue;
            int offsetTop = int.MaxValue;
            int offsetDown = int.MaxValue;

            Color lastColor;


            //This is just so its faster on Bigger Images
            int jumpHeightIndexes = (img.PixelHeight / 500) + 1;
            int jumpWidthIndexes = (img.PixelWidth / 500) + 1;
             
            //Top offset
            for (int i = 0; i < img.PixelWidth; i += jumpWidthIndexes)
            {
                lastColor = GetPixelColor(img, i, 0);

                int offsetnow = 1;

                for (int j = 1; j < img.PixelHeight; j++)
                {
                    if (ColorHelper.ComparePixelColorWithTolerance( GetPixelColor(img, i, j), lastColor, tolerance))
                    {
                        offsetnow++;
                        continue;
                    }
                    break;
                }
                if (offsetnow < offsetTop) offsetTop = offsetnow;
            }


            //Down offset
            for (int i = 0; i < img.PixelWidth; i += jumpWidthIndexes)
            {
                lastColor = GetPixelColor(img, i, img.PixelHeight - 1);

                int offsetnow = 1;

                for (int j = img.PixelHeight - 2; j > 0; j--)
                {
                    if (ColorHelper.ComparePixelColorWithTolerance(GetPixelColor(img, i, j), lastColor, tolerance))
                    {
                        offsetnow++;
                        continue;
                    }
                    break;
                }
                if (offsetnow < offsetDown) offsetDown = offsetnow;
            }


            //Left offset
            for (int i = 0; i < img.PixelHeight; i += jumpHeightIndexes)
            {
                lastColor = GetPixelColor(img, 0, i);

                int offsetnow = 1;

                for (int j = 1; j < img.PixelWidth; j++)
                {
                    if (ColorHelper.ComparePixelColorWithTolerance(GetPixelColor(img, j, i), lastColor, tolerance))
                    {
                        offsetnow++;
                        continue;
                    }
                    break;
                }
                if (offsetnow < offsetLeft) offsetLeft = offsetnow;
            }

            //Right offset
            for (int i = 0; i < img.PixelHeight; i += jumpHeightIndexes)
            {
                lastColor = GetPixelColor(img, img.PixelWidth - 1, i);

                int offsetnow = 1;

                for (int j = img.PixelWidth - 2; j > 0; j--)
                {
                    if (ColorHelper.ComparePixelColorWithTolerance(GetPixelColor(img, j, i), lastColor, tolerance))
                    {
                        offsetnow++;
                        continue;
                    }
                    break;
                }
                if (offsetnow < offsetRight) offsetRight = offsetnow;

            }
            Debug.WriteLine("Right: " + offsetRight);
            Debug.WriteLine("Left: " + offsetLeft);
            Debug.WriteLine("Top: " + offsetTop);
            Debug.WriteLine("Down: " + offsetDown);

            return new CroppedBitmap(img, new Int32Rect(offsetLeft, offsetTop, img.PixelWidth - offsetLeft - offsetRight, img.PixelHeight - offsetTop - offsetDown)
);
        }

        public static System.Drawing.Color GetPixelColor(BitmapSource image, int x, int y)
        {
            int bytesPerPixel = (image.Format.BitsPerPixel + 7) / 8;
            int stride = bytesPerPixel * image.PixelWidth;

            byte[] pixelData = new byte[bytesPerPixel];
            image.CopyPixels(new Int32Rect(x, y, 1, 1), pixelData, stride, 0);

            return System.Drawing.Color.FromArgb(pixelData[3], pixelData[2], pixelData[1], pixelData[0]);
        }

        public static System.Drawing.Color GetMostContainedColor(BitmapSource image, int x, int y, int range)// Partially Written By Chat (GPT-3.5)
        {
            int bytesPerPixel = (image.Format.BitsPerPixel + 7) / 8;
            int stride = bytesPerPixel * image.PixelWidth;

            int minX = Math.Max(0, x);
            int minY = Math.Max(0, y);
            int maxX = Math.Min(image.PixelWidth - 1, x + range);
            int maxY = Math.Min(image.PixelHeight - 1, y + range);

            var colorCount = new Dictionary<Color, int>();

            for (int cy = minY; cy <= maxY; cy++)
            {
                for (int cx = minX; cx <= maxX; cx++)
                {
                    byte[] pixelData = new byte[bytesPerPixel];
                    image.CopyPixels(new Int32Rect(cx, cy, 1, 1), pixelData, stride, 0);

                    Color color = Color.FromArgb(pixelData[3], pixelData[2], pixelData[1], pixelData[0]);

                    if (colorCount.ContainsKey(color))
                    {
                        colorCount[color]++;
                    }
                    else
                    {
                        colorCount[color] = 1;
                    }
                }
            }

            Color mostCommonColor = FindMostCommonColor(colorCount);
            return mostCommonColor;
        }

        private static Color FindMostCommonColor(Dictionary<Color, int> colorCount)
        {
            int maxCount = 0;
            Color mostCommonColor = Color.Black; // Default to a color

            foreach (var pair in colorCount)
            {
                if (pair.Value > maxCount)
                {
                    maxCount = pair.Value;
                    mostCommonColor = pair.Key;
                }
            }

            return mostCommonColor;
        }

        public static WriteableBitmap CreateImageFromColorArray(Color[,] colorArray) // Partially Written By Chat (GPT-3.5)
        {
            int width = colorArray.GetLength(1);
            int height = colorArray.GetLength(0);

            WriteableBitmap writeableBitmap = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);

            writeableBitmap.Lock();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color sourceColor = colorArray[y, x];
                    System.Drawing.Color targetColor = System.Drawing.Color.FromArgb(sourceColor.A, sourceColor.R, sourceColor.G, sourceColor.B);

                    int index = y * writeableBitmap.BackBufferStride + x * 4;

                    unsafe
                    {
                        byte* buffer = (byte*)writeableBitmap.BackBuffer;
                        buffer[index + 0] = targetColor.B;
                        buffer[index + 1] = targetColor.G;
                        buffer[index + 2] = targetColor.R;
                        buffer[index + 3] = targetColor.A;
                    }
                }
            }

            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            writeableBitmap.Unlock();

            return writeableBitmap;
        }

        public static System.Windows.Media.ImageSource PixelateImage(Image imagePicture, double colorCompareTolerance, bool borderRemoved, GridMode gridMode, int outputImageWidth , int outputImageHeight)
        {
            float GridHeight = 0;
            float GridWidth = 0;
            int newWidth = 0;
            int newHeight = 0;
            System.Windows.Controls.Image image = imagePicture;
            BitmapSource bitmapSource = (BitmapSource)image.Source;
            


            if (gridMode == GridMode.Auto)
            {
                Debug.WriteLine("CALCULATING");



                List<int> widths = new List<int>();
                List<int> heights = new List<int>();

                #region Heights Calculation
                int currentHeight = 1;
                int heightIndex = 0;
                Color lastColor = GetPixelColor(bitmapSource, 0, 0);
                heights.Add(1);

                for (int i = 0; i < bitmapSource.PixelWidth; i++)
                {
                    for (int j = 1; j < bitmapSource.PixelHeight; j++)
                    {
                        if (ColorHelper.ComparePixelColorWithTolerance(GetPixelColor(bitmapSource, i, j),lastColor, colorCompareTolerance))
                        {
                            currentHeight++;
                            heights[heightIndex] = currentHeight;
                            lastColor = GetPixelColor(bitmapSource, i, j);
                        }
                        else
                        {
                            currentHeight = 1;
                            heightIndex++;
                            heights.Add(1);
                            lastColor = GetPixelColor(bitmapSource, i, j);

                        }
                    }
                    heightIndex++;
                    heights.Add(1);

                }
                heights.RemoveAll(num => num == 1);
                heights.RemoveAll(num => num == 2);
                heights.RemoveAll(num => num == 3);
                heights.RemoveAll(num => num == 4);

                #endregion

                #region Widths Calculation

                int currentWidth = 1;
                int WidthIndex = 0;
                lastColor = GetPixelColor(bitmapSource, 0, 0);
                widths.Add(1);

                for (int j = 0; j < bitmapSource.PixelHeight; j++)
                {

                    for (int i = 1; i < bitmapSource.PixelWidth; i++)
                    {
                        if (ColorHelper.ComparePixelColorWithTolerance(GetPixelColor(bitmapSource, i, j), lastColor, colorCompareTolerance))
                        {
                            currentWidth++;
                            widths[WidthIndex] = currentWidth;
                            lastColor = GetPixelColor(bitmapSource, i, j);
                        }
                        else
                        {
                            currentWidth = 1;
                            WidthIndex++;
                            widths.Add(1);
                            lastColor = GetPixelColor(bitmapSource, i, j);

                        }
                    }
                    WidthIndex++;
                    widths.Add(1);

                }
                widths.RemoveAll(num => num == 1);
                widths.RemoveAll(num => num == 2);
                widths.RemoveAll(num => num == 3);
                widths.RemoveAll(num => num == 4);

                #endregion


                //int GridWidth = (from i in widths group i by i into grp orderby grp.Count() descending select grp.Key).First();
                //int GridHeight = (from i in heights group i by i into grp orderby grp.Count() descending select grp.Key).First();

                GridWidth = CalculateGridWidth(bitmapSource, borderRemoved, widths, heights);


            }
            else if (gridMode == GridMode.Manual)
            {
                Debug.WriteLine("Width: " + (float)bitmapSource.PixelWidth / outputImageWidth);
                Debug.WriteLine("new Width!!!!!!!!!: " + bitmapSource.PixelWidth / ((float)bitmapSource.PixelWidth / outputImageWidth));
                if (outputImageWidth != 0)
                    GridWidth = (float)bitmapSource.PixelWidth / outputImageWidth;
                else if (outputImageHeight != 0)
                    GridWidth = (float)bitmapSource.PixelHeight / outputImageHeight;
                else
                    throw new Exception("No Input!!");
            }

            GridHeight = GridWidth;


            newHeight = (int)Math.Round(bitmapSource.PixelHeight / GridHeight);
            newWidth = (int)Math.Round(bitmapSource.PixelWidth / GridWidth);
            Debug.WriteLine("HELLO THIS IS NEW HEIGHT: " + newHeight);
            Debug.WriteLine("HELLO THIS IS NEW Width: " + newWidth);
            Debug.WriteLine("New Grid Width: " + GridWidth);
            Debug.WriteLine("New Grid Height: " + GridHeight);

            System.Drawing.Color[,] NewImageColors = new System.Drawing.Color[newHeight, newWidth]; //= { { GetPixelColor(bitmapSource, 1, 1), GetPixelColor(bitmapSource, 16, 1) }, { GetPixelColor(bitmapSource, 1, 16), GetPixelColor(bitmapSource, 16, 16) } };

            for (int i = 0; i < newWidth; i++)
            {
                for (int j = 0; j < newHeight; j++)
                {
                    NewImageColors[j, i] = GetMostContainedColor(bitmapSource, (int)Math.Round(i * GridWidth), (int)Math.Round(j * GridHeight), (int)Math.Round(GridWidth)); //GetPixelColor(bitmapSource, i*GridWidth+(GridWidth/2), j*GridHeight + (GridHeight / 2));// //  
                }
            }

            return CreateImageFromColorArray(NewImageColors);

        }

        public static int CalculateGridWidth(BitmapSource bitmapSource, bool borderRemoved,List<int> widths, List<int> heights)
        {

            int GridWidth;

            List<List<int>> sortedWidths = widths.SortAndCountFrequencies();

            List<float> widthsValue = new List<float>(sortedWidths[0].Count);

            for (int i = 0; i < sortedWidths[0].Count; i++)
            {
                widthsValue.Add(sortedWidths[0][i] * sortedWidths[1][i]);
            }

            if (borderRemoved)
            {

                GridWidth = sortedWidths[0][widthsValue.BiggestValueIndexInList(4)];

                for (int i = 0; i < sortedWidths[0].Count; i++)
                {
                    Debug.WriteLine(sortedWidths[0][i]);
                    Debug.WriteLine(sortedWidths[1][i]);
                    Debug.WriteLine(sortedWidths[1][i] * sortedWidths[0][i]);
                    Debug.WriteLine(GridWidth);
                    Debug.WriteLine("");

                }
            } else
            {

                #region GridWidth
               


                bool isdivisible = false;
                int biggestDivisible = 0;

                int biggestCount = 0;
                while (!isdivisible)
                {
                    if (sortedWidths[0].Count() > biggestCount)
                        biggestDivisible = sortedWidths[0][biggestCount];
                    else
                    {
                        Debug.WriteLine("Didn't find Pixel Width");
                        // <---------------------------------------- Try  Height ---------------------------------------->
                        //throw new Exception("Didn't find Pixel size!!");

                        #region GridLHeight

                        List<List<int>> sortedHeights = heights.SortAndCountFrequencies();

                        biggestDivisible = sortedHeights[0][0];
                        break;
                        #endregion
                    }


                    if (bitmapSource.PixelWidth % biggestDivisible == 0)
                    {
                        isdivisible = true;
                        continue;
                    }
                    biggestCount++;

                }

                GridWidth = biggestDivisible;
                #endregion

                for (int i = 0; i < Math.Min(6, sortedWidths[0].Count); i++)
                {
                    Debug.WriteLine(sortedWidths[0][i]);
                    Debug.WriteLine(sortedWidths[1][i]);
                    Debug.WriteLine(sortedWidths[1][i] * sortedWidths[0][i]);
                    Debug.WriteLine(GridWidth);
                    Debug.WriteLine("");

                }

                List<List<int>> sortedHeights1 = heights.SortAndCountFrequencies();


                for (int i = 0; i < Math.Min(6, sortedWidths[0].Count); i++)
                {
                    Debug.WriteLine("Height");
                    Debug.WriteLine(sortedHeights1[0][i]);
                    Debug.WriteLine(sortedHeights1[1][i]);
                    Debug.WriteLine(sortedHeights1[1][i] * sortedHeights1[0][i]);
                    Debug.WriteLine(GridWidth);
                    Debug.WriteLine("");

                }
            }

            return GridWidth;

        }

    }

}
