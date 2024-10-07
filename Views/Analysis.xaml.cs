using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace Grafika.Views
{
    /// <summary>
    /// Logika interakcji dla klasy Analysis.xaml
    /// </summary>
    public partial class Analysis : Window
    {
        private WriteableBitmap currentBitmap;
        private WriteableBitmap snapshot;
        private BitmapImage originalImage;
        private bool IsChangeOriginal = false;
        int width;
        int height;
        public Analysis()
        {
            InitializeComponent();
            TransformGroup group = new TransformGroup();

            ScaleTransform xform = new ScaleTransform();
            group.Children.Add(xform);

            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);

            displayedImage.RenderTransform = group;
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

                var Width = originalImage.PixelWidth;
                var Height = originalImage.PixelHeight;
                var bytesPerPixel = (originalImage.Format.BitsPerPixel + 7) / 8;
                var stride = Width * bytesPerPixel;

                byte[] pixelData = new byte[Height * stride];
                // byte[] pixels = new byte[bpp * width * height];

                originalImage.CopyPixels(pixelData, stride, 0);

                var Pixels = pixelData;

                currentBitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Rgb24, null);
                currentBitmap.WritePixels(new Int32Rect(0, 0, Width, Height), pixelData, 3 * Width, 0);

                width = currentBitmap.PixelWidth;
                height = currentBitmap.PixelHeight;

                displayedImage.Source = originalImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas wczytywania pliku JPEG: " + ex.Message);
            }
        }

        private int[,] mask;
        private byte[] PerformDilatation(byte[] pixels, int width, int height)
        {
            var newPixels = new byte[width * height * 3];

            if (mask == null) { return null; }
            int offset = ((mask.GetLength(0) - 1) / 2);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 3;
                    byte maxR = 0, maxG = 0, maxB = 0;
                    for (int offsetY = -offset; offsetY <= offset; offsetY++)
                    {
                        for (int offsetX = -offset; offsetX <= offset; offsetX++)
                        {
                            int pixelY = y + offsetY;
                            int pixelX = x + offsetX;

                            if (pixelY >= 0 && pixelX >= 0 && pixelX < width && pixelY < height)
                            {
                                int currentIndex = (pixelY * width + pixelX) * 3;
                                if (mask[offsetY + offset, offsetX + offset] == 1)
                                {
                                    maxR = Math.Max(maxR, pixels[currentIndex]);
                                    maxG = Math.Max(maxG, pixels[currentIndex + 1]);
                                    maxB = Math.Max(maxB, pixels[currentIndex + 2]);
                                }
                            }
                        }
                    }

                    newPixels[index] = maxR;
                    newPixels[index + 1] = maxG;
                    newPixels[index + 2] = maxB;
                }
            }
            return newPixels;
        }
        private byte[] PerformErosion(byte[] pixels, int width, int height)
        {
            var newPixels = new byte[width * height * 3];

            if (mask == null) { return null; }
            int offset = ((mask.GetLength(0) - 1) / 2);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 3;
                    byte maxR = 255, maxG = 255, maxB = 255;
                    for (int offsetY = -offset; offsetY <= offset; offsetY++)
                    {
                        for (int offsetX = -offset; offsetX <= offset; offsetX++)
                        {
                            int pixelY = y + offsetY;
                            int pixelX = x + offsetX;

                            if (pixelY >= 0 && pixelX >= 0 && pixelX < width && pixelY < height)
                            {
                                int currentIndex = (pixelY * width + pixelX) * 3;
                                if (mask[offsetY + offset, offsetX + offset] == 1)
                                {
                                    maxR = Math.Min(maxR, pixels[currentIndex]);
                                    maxG = Math.Min(maxG, pixels[currentIndex + 1]);
                                    maxB = Math.Min(maxB, pixels[currentIndex + 2]);
                                }
                            }
                        }
                    }

                    newPixels[index] = maxR;
                    newPixels[index + 1] = maxG;
                    newPixels[index + 2] = maxB;
                }
            }
            return newPixels;
        }

        private int[,] ParseMask()
        {
            string[] mask = StructuringElementSize.Text.Split(' ');

            int row = 0, col = 0;
            var tmpSize = Math.Sqrt(mask.Length);
            if (tmpSize % 1 != 0 || tmpSize % 2 == 0 || tmpSize < 3)
            {
                MessageBox.Show($"Nieprawidłowa liczba elementów konstrukcyjnych, podaj minimum 9 elementów.\n Podano {mask.Length}");
                return null;
            }

            int kernelSize = (int)tmpSize;
            var kernel = new int[kernelSize, kernelSize];
            foreach (var val in mask)
            {
                var parsingResult = int.TryParse(val, out kernel[row, col]);
                if (!parsingResult || kernel[row, col] > 2 || kernel[row, col] < 0)
                {
                    MessageBox.Show($"Nieprawidłowe dane wejściowe maski: {val}");
                    return null;
                }
                row++;
                if (row == kernelSize)
                {
                    row = 0; col++;
                }
            }

            return kernel;
        }

        private void Count_Click(object sender, RoutedEventArgs e)
        {
            if (currentBitmap == null) { return; }
            mask = ParseMask();
            var width = currentBitmap.PixelWidth;
            var height = currentBitmap.PixelHeight;
            var pixels = new byte[width * height * 3];
            var newPixels = new byte[width * height * 3];
            currentBitmap.CopyPixels(pixels, 3 * width, 0);
            double red = RedSlider.Value, green = GreenSlider.Value, blue = BlueSlider.Value;
            var colorValue = System.Drawing.Color.FromArgb((byte)red, (byte)green, (byte)blue);
            var parsingResult = int.TryParse(Tolerance.Text, out var tolerance);
            if (!parsingResult)
            {
                MessageBox.Show("Invalid tolerance value.");
                return;
            }
            double valueFloor = Math.Round(colorValue.GetHue()) - tolerance;
            double valueCeiling = Math.Round(colorValue.GetHue()) + tolerance;
            var pixelCount = 0;

            for (int i = 0; i < width * height * 3;)
            {
                var r = pixels[i];
                var g = pixels[i + 1];
                var b = pixels[i + 2];
                var color = System.Drawing.Color.FromArgb(r, g, b);

                var hue = color.GetHue();

                if (hue <= valueCeiling && hue >= valueFloor)
                {
                    newPixels[i++] = (byte)255;
                    newPixels[i++] = (byte)255;
                    newPixels[i++] = (byte)255;
                }
                else
                {
                    newPixels[i++] = (byte)0;
                    newPixels[i++] = (byte)0;
                    newPixels[i++] = (byte)0;
                }
            }

            newPixels = PerformErosion(newPixels, width, height);
            if (newPixels != null)
            {
                newPixels = PerformDilatation(newPixels, width, height);
                newPixels = PerformDilatation(newPixels, width, height);

                for (int i = 0; i < width * height * 3; i += 3)
                {
                    var c = (int)newPixels[i];


                    if (c == 255)
                    {
                        pixelCount++;
                    }
                }


                var newBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
                newBitmap.WritePixels(new Int32Rect(0, 0, width, height), newPixels, width * 3, 0);
                currentBitmap = newBitmap;
                displayedImage.Source = currentBitmap;
                var overallPixels = width * height;
                var pixelPercent = (double)pixelCount / overallPixels * 100;
                MessageBox.Show($"Procent z \n R: {red.ToString("F2")} \n G: {green.ToString("F2")} \n B: {blue.ToString("F2")} \n to {pixelPercent.ToString("F2")}%");
            }
        }
    }
}
