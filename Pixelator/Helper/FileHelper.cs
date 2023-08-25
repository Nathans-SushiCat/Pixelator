using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace Pixelator.Helper
{
    internal class FileHelper
    {
        public static void SaveImage(ImageSource pixelatedImagePicture) // Written By Chat (GPT-3.5)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Bitmap Image|*.bmp|PNG Image|*.png|JPEG Image|*.jpg";
            saveFileDialog.FilterIndex = 1;

            if (saveFileDialog.ShowDialog() == true)
            {
                // Get the pixelated image from the Image control
                BitmapSource pixelatedImageSource = (BitmapSource)pixelatedImagePicture;

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

        public static BitmapImage SelectImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Image files|*.bmp; *.png; *.jpg";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() ?? false)
            {
                 return new BitmapImage(new Uri(openFileDialog.FileName));
            }
            return null;
        }
    }
}
