using Microsoft.Win32;
using Pixelator.Core;
using Pixelator.Extentions;
using Pixelator.Helper;
using Pixelator.Models;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Color = System.Drawing.Color;
using DrawingColor = System.Drawing.Color;
using Image = System.Windows.Controls.Image;

namespace Pixelator
{
    /// Written By Sushicat ฅ^•ﻌ•^ฅ
    public partial class MainWindow : Window
    {
        bool selectedImage = false;

        bool deleteBorder = true;
        

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
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            imagePicture.Source = FileHelper.SelectImage();
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e) => FileHelper.SaveImage(pixelatedImagePicture.Source); // Written By Chat (GPT-3.5)

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

        private void ShowPixelImage_LeftDown(object sender, MouseButtonEventArgs e)
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

        private void Pixelate_Click(object sender, RoutedEventArgs e)
        {
            // TODO
            pixelatedImagePicture.Source = Core.ImageConverter.PixelateImage(imagePicture,true, gridMode,  PixelTextBoxX.Text.ToInt(), PixelTextBoxY.Text.ToInt());
        }

    }
}
