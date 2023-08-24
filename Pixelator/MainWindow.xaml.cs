using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Drawing.Color;
using DrawingColor = System.Drawing.Color;
using Image = System.Windows.Controls.Image;

namespace Pixelator
{
    /// Written By Sushicat ฅ^•ﻌ•^ฅ
    public partial class MainWindow : Window
    {
        bool selectedImage = false;
        enum GridMode
        {
            Auto,
            Manual
        }

        GridMode gridMode = GridMode.Auto;

        public void SetGridMode(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.IsChecked == true)
            {
                gridMode = GridMode.Auto;
            }
            else
            {
                gridMode  = GridMode.Manual;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            imagePicture.AllowDrop = true;
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            selectedImage = true;

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Image files|*.bmp; *.png; *.jpg";
            openFileDialog.FilterIndex = 1;
            if(openFileDialog.ShowDialog() == true) 
            {
                imagePicture.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }


        private void SaveImage_Click(object sender, RoutedEventArgs e) // Written By Chat (GPT-3.5)
        {
            

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Bitmap Image|*.bmp|PNG Image|*.png|JPEG Image|*.jpg";
            saveFileDialog.FilterIndex = 1;

            if (saveFileDialog.ShowDialog() == true)
            {
                // Get the pixelated image from the Image control
                BitmapSource pixelatedImageSource = (BitmapSource)pixelatedImagePicture.Source;

                // Save the pixelated image to the selected file
                BitmapEncoder encoder = null;

                switch (saveFileDialog.FilterIndex)
                {
                    case 1: // BMP
                        encoder = new BmpBitmapEncoder();
                        break;
                    case 2: // PNG
                        encoder = new PngBitmapEncoder();
                        break;
                    case 3: // JPEG
                        encoder = new JpegBitmapEncoder();
                        break;
                }

                if (encoder != null)
                {
                    encoder.Frames.Add(BitmapFrame.Create(pixelatedImageSource));
                    using (var stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }
                }
            }
        }

        private void SelectImage_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void SelectImage_DragDrop(object sender, DragEventArgs e) // Written By Chat (GPT-3.5)
        {
            selectedImage = true;
            try
            {
                string[] picPaths = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (picPaths != null && picPaths.Length > 0)
                {
                    string pic = picPaths[0]; // Assuming you're handling only one image at a time

                    BitmapImage bitmap = new BitmapImage(new Uri(pic));

                    // Update the UI from the UI thread
                    Dispatcher.Invoke(() =>
                    {
                        imagePicture.Source = bitmap;
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Pixelate_Click(object sender, RoutedEventArgs e)
        {

            if (!selectedImage)
                return;

            int GridHeight = 0;
            int GridWidth = 0;
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
                Color lastColor = Color.FromArgb(1, 0, 0, 0);
                heights.Add(1);

                for (int i = 0; i < bitmapSource.PixelWidth; i++)
                {

                    for (int j = 0; j < bitmapSource.PixelHeight; j++)
                    {
                        if (GetPixelColor(bitmapSource, i, j) == lastColor)
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

                #endregion

                #region Widths Calculation

                int currentWidth = 1;
                int WidthIndex = 0;
                lastColor = Color.FromArgb(1, 0, 0, 0);
                widths.Add(1);

                for (int j = 0; j < bitmapSource.PixelHeight; j++)
                {

                    for (int i = 0; i < bitmapSource.PixelWidth; i++)
                    {
                        if (GetPixelColor(bitmapSource, i, j) == lastColor)
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

                #endregion


                //int GridWidth = (from i in widths group i by i into grp orderby grp.Count() descending select grp.Key).First();
                //int GridHeight = (from i in heights group i by i into grp orderby grp.Count() descending select grp.Key).First();

                #region GridWidth
                List<List<int>> sortedWidths = SortAndCountFrequencies(widths);

                List<float> widthsValue = new List<float>();

                for (int i = 0; i < sortedWidths[0].Count; i++)
                {
                    widthsValue.Add(sortedWidths[0][i] * sortedWidths[1][i]);
                }

                bool isdivisible = false;
                int biggestDivisible = 0;// = BiggestValueIndexInList(widthsValue, 3);

                int biggestCount = 0;
                while (!isdivisible)
                {
                    biggestDivisible = sortedWidths[0][biggestCount];

                    if (bitmapSource.PixelWidth % biggestDivisible == 0)
                    {
                        isdivisible = true;
                        continue;
                    }
                    biggestCount++;

                }

                GridWidth = biggestDivisible;
                #endregion

                #region GridLHeight

                List<List<int>> sortedHeights = SortAndCountFrequencies(heights);

                List<float> heightsValue = new List<float>();

                for (int i = 0; i < sortedHeights[0].Count; i++)
                {
                    heightsValue.Add(sortedHeights[0][i] * sortedHeights[1][i]);
                }

                int biggestHeightIndex = BiggestValueIndexInList(heightsValue, 3);

                GridHeight = sortedHeights[0][biggestHeightIndex];

                #endregion

                /*
                for (int i = 0; i < Math.Min(6, sortedHeights.le); i++)
                {
                    Debug.WriteLine(sortedHeights[0][i]);
                    Debug.WriteLine(sortedHeights[1][i]);
                    Debug.WriteLine(sortedHeights[1][i] * sortedHeights[0][i]);
                    Debug.WriteLine(GridHeight);
                    Debug.WriteLine("");

                }*/
            }
            else if(gridMode == GridMode.Manual)
            {
                if(PixelTextBoxX.Text != "")
                    GridWidth = bitmapSource.PixelWidth / int.Parse(PixelTextBoxX.Text);
                else if(PixelTextBoxY.Text != "")
                    GridWidth = bitmapSource.PixelHeight / int.Parse(PixelTextBoxY.Text);

            }

            GridHeight = GridWidth;


            newHeight = bitmapSource.PixelHeight / GridHeight;
            newWidth = bitmapSource.PixelWidth / GridWidth;
            Debug.WriteLine("HELLO THIS IS NEW HEIGHT: " + newHeight);
            Debug.WriteLine("HELLO THIS IS NEW Width: " + newWidth);
            Debug.WriteLine("New Grid Width: " + GridWidth);
            Debug.WriteLine("New Grid Height: " + GridHeight);

            System.Drawing.Color[,] NewImageColors = new System.Drawing.Color[newHeight, newWidth]; //= { { GetPixelColor(bitmapSource, 1, 1), GetPixelColor(bitmapSource, 16, 1) }, { GetPixelColor(bitmapSource, 1, 16), GetPixelColor(bitmapSource, 16, 16) } };
            
            for(int i = 0; i < newWidth; i++)
            {
                for(int j = 0; j < newHeight; j++)
                {
                    NewImageColors[j, i] = GetMostContainedColor(bitmapSource, i * GridWidth, j * GridHeight, GridWidth); //GetPixelColor(bitmapSource, i*GridWidth+(GridWidth/2), j*GridHeight + (GridHeight / 2));// //  
                }
            }

            pixelatedImagePicture.Source = CreateImageFromColorArray(NewImageColors);



            /*  
            // Written By Chat (GPT-3.5)
            // Convert ImageSource to BitmapSource
            BitmapSource source = (BitmapSource)imagePicture.Source;

            // Convert BitmapSource to Bitmap
            Bitmap originalBmp = new Bitmap(source.PixelWidth, source.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = originalBmp.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, originalBmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            originalBmp.UnlockBits(data);

            // Create a new Bitmap with the desired dimensions
            Bitmap resizedBmp = new Bitmap(newWidth, newHeight);

            using (Graphics graphics = Graphics.FromImage(resizedBmp))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(originalBmp, 0, 0, newWidth, newHeight);
            }

            // Convert Bitmap to BitmapSource
            BitmapSource resizedBitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                resizedBmp.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // Assign the resized image to your Image control
            pixelatedImagePicture.Source = resizedBitmapSource;

            // Dispose of the original and resized Bitmaps
            originalBmp.Dispose();
            resizedBmp.Dispose();
        */
        }

        private int BiggestValueIndexInList(List<float> list, int untilIndex)
        {
            int BiggestIndex = 0;
            float BiggestValue = 0;


            for (int i = 1; i < untilIndex; i++)
            {
                if (BiggestValue < list[i])
                {
                    BiggestValue = list[i];
                    BiggestIndex = i;
                }
            }
            return BiggestIndex;
        }

        static List<List<int>> SortAndCountFrequencies(List<int> lengths)
        {
            var frequencyMap = new Dictionary<int, int>();

            foreach (var length in lengths)
            {
                if (frequencyMap.ContainsKey(length))
                {
                    frequencyMap[length]++;
                }
                else
                {
                    frequencyMap[length] = 1;
                }
            }

            // Written By Chat (GPT-3.5)
            var sortedFrequencies = frequencyMap.OrderByDescending(kv => kv.Value)
                                              .ThenBy(kv => kv.Key)
                                              .ToList();

            var sortedLengths = sortedFrequencies.Select(kv => kv.Key).ToList();
            var frequencies = sortedFrequencies.Select(kv => kv.Value).ToList();

            // Calculate the average frequency
            double averageFrequency = frequencies.Average();

            // Set a threshold for removing values based on a factor (e.g., 0.5)
            double thresholdFactor = 0.5;
            double threshold = averageFrequency * thresholdFactor;

            // Remove values with frequencies below the threshold
            var filteredSortedLengths = new List<int>();
            var filteredFrequencies = new List<int>();

            for (int i = 0; i < sortedLengths.Count; i++)
            {
                if (frequencies[i] >= threshold)
                {
                    filteredSortedLengths.Add(sortedLengths[i]);
                    filteredFrequencies.Add(frequencies[i]);
                }
            }

            return new List<List<int>> { filteredSortedLengths, filteredFrequencies };
        }

        public System.Drawing.Color GetPixelColor(BitmapSource image, int x, int y)
        {
            int bytesPerPixel = (image.Format.BitsPerPixel + 7) / 8;
            int stride = bytesPerPixel * image.PixelWidth;

            byte[] pixelData = new byte[bytesPerPixel];
            image.CopyPixels(new Int32Rect(x, y, 1, 1), pixelData, stride, 0);

            return System.Drawing.Color.FromArgb(pixelData[3], pixelData[2], pixelData[1], pixelData[0]);
        }

        public Color GetMostContainedColor(BitmapSource image, int x, int y, int range)// Partially Written By Chat (GPT-3.5)
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

        private Color FindMostCommonColor(Dictionary<Color, int> colorCount)
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

        public WriteableBitmap CreateImageFromColorArray(DrawingColor[,] colorArray) // Partially Written By Chat (GPT-3.5)
        {
            int width = colorArray.GetLength(1);
            int height = colorArray.GetLength(0);

            WriteableBitmap writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            writeableBitmap.Lock();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    DrawingColor sourceColor = colorArray[y, x];
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

        private void ShowPixelImage(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is Image image && image.Source is System.Windows.Media.Imaging.BitmapSource)
                {
                    BitmapSource pixelatedImageSource = (BitmapSource)image.Source;

                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(pixelatedImageSource));
                    string tempImagePath = System.AppDomain.CurrentDomain.BaseDirectory + "temp.png";
                    using (FileStream fs = new FileStream(tempImagePath, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                    Process.Start(tempImagePath);
                }
            }
        }
    }
}
