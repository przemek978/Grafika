using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
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
            var pixelList = new List<Color>();
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fs))
            {
                // Odczytaj format
                string format = reader.ReadLine();

                // Odczytaj szerokość i wysokość
                int width = 0;
                int height = 0;
                int maxValue = 0;
                string dimensionsLine;
                string line;

                while ((dimensionsLine = reader.ReadLine()) != null)
                {
                    // Usuń komentarze w linii
                    int commentIndex = dimensionsLine.IndexOf('#');
                    if (commentIndex >= 0)
                    {
                        dimensionsLine = dimensionsLine.Substring(0, commentIndex);
                    }

                    string[] tokens = dimensionsLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string token in tokens)
                    {
                        if (int.TryParse(token, out int value))
                        {
                            if (width == 0)
                            {
                                width = value;
                            }
                            else if (height == 0)
                            {
                                height = value;
                                // Zakończ, jeśli znaleziono już szerokość i wysokość
                            }
                            else if(maxValue == 0)
                            {
                                maxValue = value;
                                break;
                            }
                        }
                    }

                    if (width > 0 && height > 0 && maxValue>0)
                    {
                        break; // Zakończ, jeśli znaleziono szerokość i wysokość
                    }
                }

                Bitmap image = new Bitmap(width, height);
                List<string> allPixels = new List<string>();
                char[] buffer = new char[4096]; // Rozmiar bufora do wczytywania danych

                while (true)
                {
                    int bytesRead = reader.ReadBlock(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        // Koniec pliku
                        break;
                    }

                    string dataBlock = new string(buffer, 0, bytesRead);
                    if (dataBlock.Contains('#'))
                    {
                        while (dataBlock.Contains('#'))
                        {
                            dataBlock = removeComments(dataBlock);
                        }
                    }
                    // Usuń komentarze z bloku danych
                    dataBlock = removeComments(dataBlock);

                    string[] lines = dataBlock.Split(new string[] { "\n" }, StringSplitOptions.None);

                    foreach (var l in lines)
                    {
                        string[] tokens = l.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var token in tokens)
                        {
                            allPixels.Add(token);
                        }
                    }
                }

                for (int y = 0; y < height; y++)
                {

                    for (int x = 0; x < width; x++)
                    {
                        int red = 0;
                        int green = 0;
                        int blue = 0;

                        if (allPixels.Count >= 3)
                        {
                            red = int.Parse(allPixels[x * 3]);
                            green = int.Parse(allPixels[x * 3 + 1]);
                            blue = int.Parse(allPixels[x * 3 + 2]);
                        }
                        red = (int)((red / 255.0) * 255);
                        green = (int)((green / 255.0) * 255);
                        blue = (int)((blue / 255.0) * 255);

                        Color pixelColor = Color.FromArgb(255, red%256, green%256, blue % 256);
                        image.SetPixel(x, y, pixelColor);
                    }
                }

                displayedImage.Source = BitmapToImageSource(image);
            }
        }

        private string removeComments(string line)
        {
            int commentIndex = line.IndexOf('#');
            if (commentIndex == 0)
            {
                return null;
            }
            if (commentIndex >= 0)
            {
                var endcomment = line.Substring(commentIndex);
                var endcommentIndex = endcomment.IndexOf("\n");
                var tmp = line.Substring(0, commentIndex);
                var tmp2 = endcomment.Substring(endcommentIndex);
                line = tmp + tmp2;
            }
            return line;
        }

        private void LoadAndDisplayPPMP6(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                // Odczytaj format
                string format = Encoding.ASCII.GetString(reader.ReadBytes(2));
                if (format != "P6")
                {
                    // Niepoprawny format
                    return;
                }

                // Odczytaj szerokość i wysokość
                int width = 0;
                int height = 0;
                int maxValue = 0;
                bool dimensionsRead = false;

                while (!dimensionsRead)
                {
                    string line = ReadLine(reader);
                    if (line.StartsWith("#"))
                    {
                        // Pomijaj komentarze
                        continue;
                    }

                    string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Length == 2 && int.TryParse(tokens[0], out int w) && int.TryParse(tokens[1], out int h))
                    {
                        width = w;
                        height = h;
                        dimensionsRead = true;
                    }
                }

                maxValue = int.Parse(ReadLine(reader));

                Bitmap image = new Bitmap(width, height);
                byte[] pixelData = reader.ReadBytes(width * height * 3);

                int dataIndex = 0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int red = pixelData[dataIndex++];
                        int green = pixelData[dataIndex++];
                        int blue = pixelData[dataIndex++];

                        Color pixelColor = Color.FromArgb(255, red, green, blue);
                        image.SetPixel(x, y, pixelColor);
                    }
                }

                displayedImage.Source = BitmapToImageSource(image);
            }
        }

        private string ReadLine(BinaryReader reader)
        {
            List<byte> buffer = new List<byte>();
            byte currentByte;

            while ((currentByte = reader.ReadByte()) != 10) // 10 is the ASCII code for newline
            {
                buffer.Add(currentByte);
            }

            return Encoding.ASCII.GetString(buffer.ToArray());
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
