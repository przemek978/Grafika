using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Grafika.Views
{
    /// <summary>
    /// Logika interakcji dla klasy Transform.xaml
    /// </summary>
    public partial class Transform : Window
    {
        private BitmapImage originalImage;
        public Transform()
        {
            InitializeComponent();
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Pliki JPEG|*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                if (filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || filePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    LoadAndDisplayJPEG(filePath);
                }
                else
                {
                    MessageBox.Show("Nieobsługiwany format pliku.");
                }
            }
        }

        private void LoadAndDisplayJPEG(string filePath)
        {
            try
            {
                BitmapImage image = new BitmapImage(new Uri(filePath));
                originalImage = image;
                displayedImage.Source = image;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas wczytywania pliku JPEG: " + ex.Message);
            }
        }
        private void ApplyAddition_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(ValueTextBox.Text, out double value))
            {
                ApplyArithmeticOperation(value);
            }
            else
            {
                MessageBox.Show("Podaj wartość operacji");
            }
        }

        private void ApplySubtraction_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(ValueTextBox.Text, out double value))
            {
                ApplyArithmeticOperation(-value);
            }
            else
            {
                MessageBox.Show("Podaj wartość operacji");
            }
        }

        private void ApplyMultiplication_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(ValueTextBox.Text, out double value))
            {
                ApplyArithmeticOperation(value,true);
            }
            else
            {
                MessageBox.Show("Podaj wartość operacji");
            }
        }

        private void ApplyDivision_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(ValueTextBox.Text, out double value) && value != 0)
            {
                ApplyArithmeticOperation(1.0 / value,true);
            }
            else
            {
                MessageBox.Show("Podaj wartość operacji");
            }
        }

        private void ApplyBrightness_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(ValueTextBox.Text, out double value))
            {
                ApplyBrightness(value); // Zmiana jasności
            }
            else
            {
                MessageBox.Show("Podaj wartość operacji");
            }
        }

        private void ApplyGrayscale_Click(object sender, RoutedEventArgs e)
        {
            ApplyToGrayscale();
        }  
        
        private void ApplyGrayscale2_Click(object sender, RoutedEventArgs e)
        {
            ApplyToGrayscaleAlternative();
        }

        private void ApplyArithmeticOperation(double factor, bool isMultiplication = false)
        {
            if (originalImage != null)
            {
                int width = originalImage.PixelWidth;
                int height = originalImage.PixelHeight;

                int stride = width * 4;
                byte[] pixelData = new byte[height * stride];
                originalImage.CopyPixels(pixelData, stride, 0);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offset = y * stride + x * 4;

                        byte newR, newG, newB;
                        if (isMultiplication)
                        {
                            newR = (byte)Math.Max(0, Math.Min(255, pixelData[offset + 2] * factor));
                            newG = (byte)Math.Max(0, Math.Min(255, pixelData[offset + 1] * factor));
                            newB = (byte)Math.Max(0, Math.Min(255, pixelData[offset] * factor));
                        }
                        else
                        {
                            newR = (byte)Math.Max(0, Math.Min(255, pixelData[offset + 2] + factor));
                            newG = (byte)Math.Max(0, Math.Min(255, pixelData[offset + 1] + factor));
                            newB = (byte)Math.Max(0, Math.Min(255, pixelData[offset] + factor));
                        }


                        pixelData[offset + 2] = newR;
                        pixelData[offset + 1] = newG;
                        pixelData[offset] = newB;
                    }
                }

                BitmapSource processedBitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixelData, stride);
                originalImage = ConvertBitmapSourceToBitmapImage(processedBitmap);
                displayedImage.Source = processedBitmap;
            }
            else
            {
                MessageBox.Show("Najpierw wczytaj obraz.");
            }
        }


        private void ApplyBrightness(double brightnessFactor)
        {
            if (originalImage != null)
            {
                int width = originalImage.PixelWidth;
                int height = originalImage.PixelHeight;

                int stride = width * 4;
                byte[] pixelData = new byte[height * stride];
                originalImage.CopyPixels(pixelData, stride, 0);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offset = y * stride + x * 4;

                        byte newR = (byte)Math.Max(0, Math.Min(255, pixelData[offset + 2] + brightnessFactor));
                        byte newG = (byte)Math.Max(0, Math.Min(255, pixelData[offset + 1] + brightnessFactor));
                        byte newB = (byte)Math.Max(0, Math.Min(255, pixelData[offset] + brightnessFactor));

                        pixelData[offset + 2] = newR;
                        pixelData[offset + 1] = newG;
                        pixelData[offset] = newB;
                    }
                }

                BitmapSource processedBitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixelData, stride);
                originalImage = ConvertBitmapSourceToBitmapImage(processedBitmap);
                displayedImage.Source = processedBitmap;
            }
            else
            {
                MessageBox.Show("Najpierw wczytaj obraz.");
            }
        }

        private void ApplyToGrayscale()
        {
            if (originalImage != null)
            {
                int width = originalImage.PixelWidth;
                int height = originalImage.PixelHeight;

                int stride = width * 4;
                byte[] pixelData = new byte[height * stride];
                originalImage.CopyPixels(pixelData, stride, 0);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offset = y * stride + x * 4;

                        byte grayValue = (byte)((pixelData[offset + 2] + pixelData[offset + 1] + pixelData[offset]) / 3);

                        pixelData[offset + 2] = grayValue;
                        pixelData[offset + 1] = grayValue;
                        pixelData[offset] = grayValue;
                    }
                }

                BitmapSource processedBitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixelData, stride);
                originalImage = ConvertBitmapSourceToBitmapImage(processedBitmap);
                displayedImage.Source = processedBitmap;
            }
            else
            {
                MessageBox.Show("Najpierw wczytaj obraz.");
            }

        }

        private void ApplyToGrayscaleAlternative()
        {
            if (originalImage != null)
            {
                int width = originalImage.PixelWidth;
                int height = originalImage.PixelHeight;

                int stride = width * 4;
                byte[] pixelData = new byte[height * stride];
                originalImage.CopyPixels(pixelData, stride, 0);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offset = y * stride + x * 4;

                        byte grayValue = (byte)(0.299 * pixelData[offset + 2] + 0.587 * pixelData[offset + 1] + 0.114 * pixelData[offset]);

                        pixelData[offset + 2] = grayValue;
                        pixelData[offset + 1] = grayValue;
                        pixelData[offset] = grayValue; 
                    }
                }

                BitmapSource processedBitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixelData, stride);
                originalImage = ConvertBitmapSourceToBitmapImage(processedBitmap);
                displayedImage.Source = processedBitmap;
            }
            else
            {
                MessageBox.Show("Najpierw wczytaj obraz.");
            }
        }


        public BitmapImage ConvertBitmapSourceToBitmapImage(BitmapSource bitmapSource)
        {
            if (bitmapSource != null)
            {
                BitmapImage bitmapImage = new BitmapImage();

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder(); // Możesz dostosować format do swoich potrzeb
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(memoryStream);

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
            else
            {
                MessageBox.Show("Wystąpił błąd");
                return null;
            }
        }

    }
}
