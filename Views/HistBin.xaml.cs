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
        private WriteableBitmap processedImage;

        int globalWidth, globalHeight;
        byte[] globalPixels;
        int[] globalPixelsAsGray;
        List<Color> globalPixelsAsColors = new List<Color>();
        int bytesPerPixel, stride;

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

                globalWidth = originalImage.PixelWidth;
                globalHeight = originalImage.PixelHeight;
                bytesPerPixel = (originalImage.Format.BitsPerPixel + 7) / 8;
                stride = globalWidth * bytesPerPixel;

                byte[] pixelData = new byte[globalHeight * stride];
                originalImage.CopyPixels(pixelData, stride, 0);
                globalPixels = pixelData;

                setGlobalColorsFromPixelsArray();
                processedImage = new WriteableBitmap(originalImage);
                displayedImage.Source = originalImage;

                setGlobalGrayPixels();
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
                StretchHistogram();
            }
        }

        private void EqualizeHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (originalImage != null)
            {
                EqualizeHistogram();
            }
        }

        private void StretchHistogram()
        {
            int min, max;
            int[] grayscalePixels = new int[globalPixelsAsColors.Count];

            for (int i = 0; i < globalPixelsAsColors.Count; i++)
            {
                grayscalePixels[i] = ((globalPixelsAsColors[i].R + globalPixelsAsColors[i].B + globalPixelsAsColors[i].G) / 3);
            }

            min = grayscalePixels.Min();
            max = grayscalePixels.Max();

            for (int i = 0; i < grayscalePixels.Length; i++)
            {
                grayscalePixels[i] = (int)((double)(grayscalePixels[i] - min) / (max - min) * 255);
            }
            drawImageFromBytes(convertPixelsArrayToDrawableArray(grayscalePixels));
        }

        private void EqualizeHistogram()
        {
            int[] grayscalePixels = calculateGrayPixels();
            int numberOfPixels = globalPixels.Length;

            int[] cdf = new int[256];
            cdf[0] = grayscalePixels[0];
            for (int i = 1; i < 256; i++)
            {
                cdf[i] = cdf[i - 1] + grayscalePixels[i];
            }

            byte[] equalizedPixels = new byte[numberOfPixels];
            for (int i = 0; i < numberOfPixels; i += 4)
            {
                int gray = (globalPixels[i] + globalPixels[i + 1] + globalPixels[i + 2]) / 3;
                int newGray = (int)(255.0 * (cdf[gray] - cdf.Min()) / (cdf.Max() - cdf.Min()));
                equalizedPixels[i] = equalizedPixels[i + 1] = equalizedPixels[i + 2] = (byte)newGray;
                equalizedPixels[i + 3] = 255;
            }
            drawImageFromBytes(equalizedPixels);
            setGlobalColorsFromPixelsArray(equalizedPixels);
        }

        private void ManualThreshold_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ValueTextBox.Text, out int threshold))
            {
                byte[] binarizedPixels = makeBinarizationFromThreshold(threshold);
                drawImageFromBytes(binarizedPixels);
            }
            else
            {
                MessageBox.Show("Podaj wartość operacji");
            }
        }

        private void PercentBlackSelection_Click(object sender, RoutedEventArgs e)
        {
            int[] histogramArray = calculateGrayPixels();
            int numberOfPixels = globalPixelsAsColors.Count;
            int seekedThreshold = 0, percentage = 50;
            bool foundThreshold = false;
            int cumulativeNumber = 0, index = 0;

            while (!foundThreshold)
            {
                cumulativeNumber += histogramArray[index];
                double currentPercentage = (double)cumulativeNumber / numberOfPixels * 100;
                if (currentPercentage < percentage)   
                    index++;
                else
                    foundThreshold = true;

                seekedThreshold = index;
            }
            byte[] binarized = makeBinarizationFromThreshold(seekedThreshold);

            drawImageFromBytes(binarized);
        }

        private void MeanIterativeSelection_Click(object sender, RoutedEventArgs e)
        {
            int threshold = 128;
            int[] histogramArray = calculateGrayPixels();
            while (true)
            {
                int sumBelow = 0;
                int countBelow = 0;
                int sumAbove = 0;
                int countAbove = 0;

                for (int i = 0; i < histogramArray.Length; i++)
                {
                    if (i < threshold)
                    {
                        sumBelow += i * histogramArray[i];
                        countBelow += histogramArray[i];
                    }
                    else
                    {
                        sumAbove += i * histogramArray[i];
                        countAbove += histogramArray[i];
                    }
                }

                int newThreshold = (sumBelow / countBelow + sumAbove / countAbove) / 2;
                if (newThreshold == threshold)
                {
                    break;
                }

                threshold = newThreshold;
            }

            byte[] binarizedPixels = makeBinarizationFromThreshold(threshold);

            drawImageFromBytes(binarizedPixels);
        }

        private byte[] makeBinarizationFromThreshold(int threshold)
        {
            byte[] binarized = new byte[globalPixels.Length];
            for (int i = 0; i < globalPixels.Length; i += 4)
            {
                binarized[i] = (byte)(globalPixelsAsGray[i] < threshold ? 0 : 255);
                binarized[i + 1] = (byte)(globalPixelsAsGray[i + 1] < threshold ? 0 : 255);
                binarized[i + 2] = (byte)(globalPixelsAsGray[i + 2] < threshold ? 0 : 255);
                binarized[i + 3] = 255;
            }

            return binarized;
        }


        private byte[] convertPixelsArrayToDrawableArray(int[] array)
        {
            byte[] bytes = new byte[globalPixels.Length];
            int index = 0;
            for (int i = 0; i < globalPixels.Length; i += 4)
            {
                bytes[i] = (byte)array[index];
                bytes[i + 1] = (byte)array[index];
                bytes[i + 2] = (byte)array[index];
                bytes[i + 3] = 255;
                index++;
            }
            return bytes;
        }

        private void drawImageFromBytes(byte[] bytes)
        {
            processedImage.WritePixels(new Int32Rect(0, 0, globalWidth, globalHeight), bytes, stride, 0);

            displayedImage.Source = processedImage;
        }

        private void setGlobalColorsFromPixelsArray()
        {
            globalPixelsAsColors.Clear();
            for (int i = 0; i + 3 < globalPixels.Length; i += 4)
            {
                globalPixelsAsColors.Add(Color.FromArgb(255, globalPixels[i + 2], globalPixels[i + 1], globalPixels[i]));
            }
        }

        private void setGlobalColorsFromPixelsArray(byte[] array)
        {
            globalPixelsAsColors.Clear();
            for (int i = 0; i + 3 < array.Length; i += 4)
            {
                globalPixelsAsColors.Add(Color.FromArgb(255, array[i + 2], array[i + 1], array[i]));
            }
        }

        private int[] calculateGrayPixels()
        {
            int[] grayscaleBytes = new int[256];
            int average;


            for (int i = 0; i < globalPixelsAsColors.Count; i++)
            {
                average = ((globalPixelsAsColors[i].R + globalPixelsAsColors[i].B + globalPixelsAsColors[i].G) / 3);
                grayscaleBytes[average]++;
            }
            return grayscaleBytes;
        }

        private void setGlobalGrayPixels()
        {
            int average;

            globalPixelsAsGray = new int[globalPixels.Length];
            for (int i = 0; i < globalPixels.Length; i += 4)
            {
                average = (globalPixels[i] + globalPixels[i + 1] + globalPixels[i + 2]) / 3;
                globalPixelsAsGray[i] = globalPixelsAsGray[i + 1] = globalPixelsAsGray[i + 2] = average;
                globalPixelsAsGray[i + 3] = 255;
            }
        }
    }
}
