using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Encoder = System.Drawing.Imaging.Encoder;

namespace Grafika.Views
{
    /// <summary>
    /// Logika interakcji dla klasy PPM.xaml
    /// </summary>
    public partial class PPM : Window
    {
        private Bitmap currentImage;
        public PPM()
        {
            InitializeComponent();
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Pliki PPM|*.ppm";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                string ppmFormat = ReadPPMFormat(filePath);

                if (ppmFormat == "P3")
                {
                    LoadAndDisplayPPMP3(filePath);
                }
                else if (ppmFormat == "P6")
                {
                    LoadAndDisplayPPMP6(filePath);
                }
                else
                {
                    MessageBox.Show("Nieobsługiwany format PPM.");
                }
            }
        }

        private void SaveToJPEG_Click(object sender, RoutedEventArgs e)
        {
            if (currentImage != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Pliki JPEG (*.jpg)|*.jpg";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    SaveToJPEG(currentImage, filePath, 80); // 80 to stopień kompresji (od 0 do 100)
                }
            }
        }

        private string ReadPPMFormat(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fs))
            {
                return reader.ReadLine().Trim();
            }
        }

        private void LoadAndDisplayPPMP3(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fs))
            {
                // Odczytaj nagłówek
                string format = reader.ReadLine();
                string dimensionsLine = reader.ReadLine();
                string maxColorValueLine = reader.ReadLine();

                string[] dimensions = dimensionsLine.Split(' ');
                int width = int.Parse(dimensions[0]);
                int height = int.Parse(dimensions[1]);

                Bitmap image = new Bitmap(width, height);

                for (int y = 0; y < height; y++)
                {
                    string line = reader.ReadLine();
                    string[] pixels = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int x = 0; x < width; x++)
                    {
                        int red = int.Parse(pixels[x * 3]);
                        int green = int.Parse(pixels[x * 3 + 1]);
                        int blue = int.Parse(pixels[x * 3 + 2]);

                        // Skalowanie wartości koloru do zakresu 0-255
                        red = (int)((red / 255.0) * 255);
                        green = (int)((green / 255.0) * 255);
                        blue = (int)((blue / 255.0) * 255);

                        Color pixelColor = Color.FromArgb(255, red, green, blue);
                        image.SetPixel(x, y, pixelColor);
                    }
                }

                displayedImage.Source = BitmapToImageSource(image);
            }
        }

        private void LoadAndDisplayPPMP6(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                // Pomijamy analizę nagłówka
                reader.BaseStream.Seek(3, SeekOrigin.Current); // Pomijamy ewentualny komentarz
                reader.BaseStream.Seek(12, SeekOrigin.Current); // Pomijamy wymiary i maksymalną wartość koloru

                int width = 100; // Przykładowa szerokość
                int height = 100; // Przykładowa wysokość

                Bitmap image = new Bitmap(width, height);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte red = reader.ReadByte();
                        byte green = reader.ReadByte();
                        byte blue = reader.ReadByte();
                        Color pixelColor = Color.FromArgb(255, red, green, blue);
                        image.SetPixel(x, y, pixelColor);
                    }
                }

                displayedImage.Source = BitmapToImageSource(image);
            }
        }

        private Bitmap LoadJPEG(string filePath)
        {
            return new Bitmap(filePath);
        }

        private void DisplayImage(Bitmap image)
        {
            displayedImage.Source = BitmapToImageSource(image);
        }

        private void SaveToJPEG(Bitmap image, string filePath, long quality)
        {
            EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;
            image.Save(filePath, jpegCodec, encoderParams);
        }

        private ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Bmp);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo encoder in encoders)
            {
                if (encoder.MimeType == mimeType)
                {
                    return encoder;
                }
            }
            return null;
        }
    }
}
