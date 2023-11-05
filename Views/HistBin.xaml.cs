using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Drawing.Imaging;
using System.IO;
using Rectangle = System.Drawing.Rectangle;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Color = System.Drawing.Color;

namespace Grafika.Views
{
    /// <summary>
    /// Logika interakcji dla klasy HistBin.xaml
    /// </summary>
    public partial class HistBin : Window
    {
        private BitmapImage originalImage;
        private BitmapImage processedImage;

        public HistBin()
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

        private void NormalizeHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (originalImage != null)
            {
                Bitmap bitmap = BitmapImage2Bitmap(originalImage);
                StretchHistogram(bitmap);
                processedImage = Bitmap2BitmapImage(bitmap);
                displayedImage.Source = processedImage;
            }
        }

        private void EqualizeHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (originalImage != null)
            {
                Bitmap bitmap = BitmapImage2Bitmap(originalImage);
                EqualizeHistogram(bitmap);
                processedImage = Bitmap2BitmapImage(bitmap);
                displayedImage.Source = processedImage;
            }
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (var stream = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(stream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(stream);
                return bitmap;
            }
        }

        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }
            bitmapImage.Freeze();
            return bitmapImage;
        }

        private void StretchHistogram(Bitmap bitmap)
        {
            int JminR = 255;
            int JmaxR = 0;

            int JminG = 255;
            int JmaxG = 0;

            int JminB = 255;
            int JmaxB = 0;

            // Przeszukaj obraz i znajdź minimalne i maksymalne jasności w składowych R, G i B
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    int R = pixelColor.R;
                    int G = pixelColor.G;
                    int B = pixelColor.B;

                    // Dla składowej R
                    if (R < JminR)
                    {
                        JminR = R; // Aktualizuj minimalną jasność
                    }

                    if (R > JmaxR)
                    {
                        JmaxR = R; // Aktualizuj maksymalną jasność
                    }

                    // Dla składowej G
                    if (G < JminG)
                    {
                        JminG = G; // Aktualizuj minimalną jasność
                    }

                    if (G > JmaxG)
                    {
                        JmaxG = G; // Aktualizuj maksymalną jasność
                    }

                    // Dla składowej B
                    if (B < JminB)
                    {
                        JminB = B; // Aktualizuj minimalną jasność
                    }

                    if (B > JmaxB)
                    {
                        JmaxB = B; // Aktualizuj maksymalną jasność
                    }
                }
            }

            // Rozszerz histogram we wszystkich składowych koloru
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    int newR = (int)(255.0 / (JmaxR - JminR) * (pixelColor.R - JminR));
                    int newG = (int)(255.0 / (JmaxG - JminG) * (pixelColor.G - JminG));
                    int newB = (int)(255.0 / (JmaxB - JminB) * (pixelColor.B - JminB));

                    newR = Math.Max(0, Math.Min(255, newR)); // Upewnij się, że wartość mieści się w zakresie [0, 255]
                    newG = Math.Max(0, Math.Min(255, newG));
                    newB = Math.Max(0, Math.Min(255, newB));

                    Color newPixelColor = Color.FromArgb(newR, newG, newB);
                    bitmap.SetPixel(x, y, newPixelColor);
                }
            }
        }




        private void EqualizeHistogram(Bitmap bitmap)
        {
            // Pobierz obraz jako tablicę pikseli
            BitmapData imageData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte[] pixelData = new byte[imageData.Stride * imageData.Height];
            System.Runtime.InteropServices.Marshal.Copy(imageData.Scan0, pixelData, 0, pixelData.Length);

            int[] histogram = new int[256];
            int totalPixels = pixelData.Length / 3;

            // Oblicz histogram czerwieni (R)
            for (int i = 2; i < pixelData.Length; i += 3)
            {
                int redValue = pixelData[i];
                histogram[redValue]++;
            }

            // Skumulowany histogram
            int[] cumulativeHistogram = new int[256];
            cumulativeHistogram[0] = histogram[0];
            for (int i = 1; i < 256; i++)
            {
                cumulativeHistogram[i] = cumulativeHistogram[i - 1] + histogram[i];
            }

            // Przeskalowanie histogramu
            for (int i = 2; i < pixelData.Length; i += 3)
            {
                int redValue = pixelData[i];
                int newRed = (int)(255.0 * cumulativeHistogram[redValue] / totalPixels);
                pixelData[i] = (byte)newRed;
            }

            // Zapisz zmienione dane z powrotem do obrazu
            System.Runtime.InteropServices.Marshal.Copy(pixelData, 0, imageData.Scan0, pixelData.Length);
            bitmap.UnlockBits(imageData);
        }

        private void ManualThreshold_Click(object sender, RoutedEventArgs e)
        {
            int threshold = GetManualThresholdFromUser();
            Bitmap binaryImage = ApplyManualThreshold(threshold);
            DisplayImage(binaryImage);
        }

        private void PercentBlackSelection_Click(object sender, RoutedEventArgs e)
        {
            Bitmap binaryImage = ApplyPercentBlackSelection();
            DisplayImage(binaryImage);
        }

        private void MeanIterativeSelection_Click(object sender, RoutedEventArgs e)
        {
            Bitmap binaryImage = ApplyMeanIterativeSelection();
            DisplayImage(binaryImage);
        }

        private void EntropySelection_Click(object sender, RoutedEventArgs e)
        {
            Bitmap binaryImage = ApplyEntropySelection();
            DisplayImage(binaryImage);
        }

        private void MinimumError_Click(object sender, RoutedEventArgs e)
        {
            Bitmap binaryImage = ApplyFuzzyMinimumError();
            DisplayImage(binaryImage);
        }

        private void FuzzyMinimumError_Click(object sender, RoutedEventArgs e)
        {
            Bitmap binaryImage = ApplyFuzzyMinimumError();
            DisplayImage(binaryImage);
        }

        private int GetManualThresholdFromUser()
        {
            return 128;
        }

        private Bitmap ApplyManualThreshold(int threshold)
        {
            BitmapSource source = (BitmapSource)displayedImage.Source;
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            Bitmap binaryImage = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = GetPixelColor(source, x, y);
                    int grayValue = (int)(0.3 * pixelColor.R + 0.59 * pixelColor.G + 0.11 * pixelColor.B);
                    int binaryValue = grayValue >= threshold ? 255 : 0;
                    binaryImage.SetPixel(x, y, Color.FromArgb(binaryValue, binaryValue, binaryValue));
                }
            }

            return binaryImage;
        }

        private Bitmap ApplyPercentBlackSelection()
        {
            BitmapSource source = (BitmapSource)displayedImage.Source;
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            Bitmap binaryImage = new Bitmap(width, height);

            int threshold = CalculateBlackThreshold(source);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = GetPixelColor(source, x, y);
                    int grayValue = (int)(0.3 * pixelColor.R + 0.59 * pixelColor.G + 0.11 * pixelColor.B);
                    int binaryValue = grayValue >= threshold ? 255 : 0;
                    binaryImage.SetPixel(x, y, Color.FromArgb(binaryValue, binaryValue, binaryValue));
                }
            }

            return binaryImage;
        }

        private Bitmap ApplyMeanIterativeSelection()
        {
            BitmapSource source = (BitmapSource)displayedImage.Source;
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            Bitmap binaryImage = new Bitmap(width, height);

            int threshold = CalculateMeanIterativeThreshold(source);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = GetPixelColor(source, x, y);
                    int grayValue = (int)(0.3 * pixelColor.R + 0.59 * pixelColor.G + 0.11 * pixelColor.B);
                    int binaryValue = grayValue >= threshold ? 255 : 0;
                    binaryImage.SetPixel(x, y, Color.FromArgb(binaryValue, binaryValue, binaryValue));
                }
            }

            return binaryImage;
        }

        private Bitmap ApplyEntropySelection()
        {
            BitmapSource source = (BitmapSource)displayedImage.Source;
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            Bitmap binaryImage = new Bitmap(width, height);

            int threshold = CalculateEntropyThreshold(source);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = GetPixelColor(source, x, y);
                    int grayValue = (int)(0.3 * pixelColor.R + 0.59 * pixelColor.G + 0.11 * pixelColor.B);
                    int binaryValue = grayValue >= threshold ? 255 : 0;
                    binaryImage.SetPixel(x, y, Color.FromArgb(binaryValue, binaryValue, binaryValue));
                }
            }

            return binaryImage;
        }

        private Bitmap ApplyFuzzyMinimumError()
        {
            BitmapSource source = (BitmapSource)displayedImage.Source;
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            Bitmap binaryImage = new Bitmap(width, height);

            int threshold = CalculateFuzzyMinimumErrorThreshold(source);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = GetPixelColor(source, x, y);
                    int grayValue = (int)(0.3 * pixelColor.R + 0.59 * pixelColor.G + 0.11 * pixelColor.B);
                    int binaryValue = grayValue >= threshold ? 255 : 0;
                    binaryImage.SetPixel(x, y, Color.FromArgb(binaryValue, binaryValue, binaryValue));
                }
            }

            return binaryImage;
        }


        private int CalculateBlackThreshold(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;

            // Przykładowa implementacja obliczania progu czarnego
            int totalPixels = width * height;

            // Tu możesz dodać logikę obliczania progu
            int threshold = 128; // Dla przykładu przyjmujemy próg 128

            return threshold;
        }

        private int CalculateEntropyThreshold(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;

            // Przykładowa implementacja obliczania progu entropii
            int[] histogram = new int[256];
            int totalPixels = width * height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = GetPixelColor(source, x, y);
                    int grayValue = (int)(0.3 * pixelColor.R + 0.59 * pixelColor.G + 0.11 * pixelColor.B);
                    histogram[grayValue]++;
                }
            }

            double entropy = 0;

            for (int i = 0; i < 256; i++)
            {
                double probability = (double)histogram[i] / totalPixels;
                if (probability > 0)
                {
                    entropy -= probability * Math.Log(probability, 2);
                }
            }

            // Załóżmy, że próg entropii to 50% entropii (dla uproszczenia)
            int threshold = (int)(0.5 * entropy * 255);

            return threshold;
        }


        private int CalculateMeanIterativeThreshold(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;

            // Przykładowa implementacja obliczania progu selekcji iteracyjnej średniej
            int totalPixels = width * height;
            int sumGrayValues = 0;
            int threshold = 128; // Przykładowy próg początkowy

            while (true)
            {
                int backgroundSum = 0;
                int backgroundCount = 0;
                int foregroundSum = 0;
                int foregroundCount = 0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color pixelColor = GetPixelColor(source, x, y);
                        int grayValue = (int)(0.3 * pixelColor.R + 0.59 * pixelColor.G + 0.11 * pixelColor.B);

                        if (grayValue <= threshold)
                        {
                            backgroundSum += grayValue;
                            backgroundCount++;
                        }
                        else
                        {
                            foregroundSum += grayValue;
                            foregroundCount++;
                        }
                    }
                }

                int newThreshold = (backgroundSum / backgroundCount + foregroundSum / foregroundCount) / 2;

                if (Math.Abs(newThreshold - threshold) < 1)
                {
                    threshold = newThreshold;
                    break;
                }

                threshold = newThreshold;
            }

            return threshold;
        }

        private int CalculateFuzzyMinimumErrorThreshold(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;

            // Przykładowa implementacja obliczania progu błędu minimalnego
            int totalPixels = width * height;
            int threshold = 128; // Przykładowy próg początkowy

            while (true)
            {
                int backgroundSum = 0;
                int backgroundCount = 0;
                int foregroundSum = 0;
                int foregroundCount = 0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color pixelColor = GetPixelColor(source, x, y);
                        int grayValue = (int)(0.3 * pixelColor.R + 0.59 * pixelColor.G + 0.11 * pixelColor.B);

                        if (grayValue <= threshold)
                        {
                            backgroundSum += grayValue;
                            backgroundCount++;
                        }
                        else
                        {
                            foregroundSum += grayValue;
                            foregroundCount++;
                        }
                    }
                }

                int newThreshold = (backgroundSum / backgroundCount + foregroundSum / foregroundCount) / 2;

                if (Math.Abs(newThreshold - threshold) < 1)
                {
                    threshold = newThreshold;
                    break;
                }

                threshold = newThreshold;
            }

            return threshold;
        }


        private void DisplayImage(Bitmap binaryImage)
        {
            displayedImage.Source = BitmapToBitmapImage(binaryImage);
        }

        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private Color GetPixelColor(BitmapSource source, int x, int y)
        {
            int pixelStride = (source.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[pixelStride];
            source.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, pixelStride, 0);

            Color color = Color.FromArgb(pixels[2], pixels[1], pixels[0]);
            return color;
        }


    }
}
