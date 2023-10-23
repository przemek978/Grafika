using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace Grafika.Views
{
    /// <summary>
    /// Logika interakcji dla klasy Transform.xaml
    /// </summary>
    public partial class Transform : Window
    {
        public Transform()
        {
            InitializeComponent();
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Pliki PPM|*.ppm|Pliki JPEG|*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                if (filePath.EndsWith(".ppm", StringComparison.OrdinalIgnoreCase))
                {
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
                else if (filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || filePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
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
                displayedImage.Source = image;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas wczytywania pliku JPEG: " + ex.Message);
            }
        }

        private void LoadAndDisplayPPMP3(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(fs))
                {
                    string format = reader.ReadLine();

                    int width = 0;
                    int height = 0;
                    int maxValue = 0;
                    string dimensionsLine;
                    string tmp = string.Empty;

                    while ((dimensionsLine = reader.ReadLine()) != null)
                    {
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
                                }
                                else if (maxValue == 0)
                                {
                                    maxValue = value;
                                }
                                else
                                {
                                    tmp += token + '\n';
                                }
                            }
                        }

                        if (width > 0 && height > 0 && maxValue > 0)
                        {
                            break;
                        }
                    }
                    WriteableBitmap image = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
                    int dataSize = width * height * 3;
                    List<byte> allPixels = new List<byte>();

                    while (true)
                    {
                        char[] buffer = new char[4096];
                        int bytesRead = reader.ReadBlock(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                        {
                            break;
                        }

                        int lastNewlineIndex = -1;
                        string dataBlock = tmp + new string(buffer, 0, buffer.Length);

                        if (Array.LastIndexOf(buffer, '\n') != 4095)
                        {
                            lastNewlineIndex = Array.LastIndexOf(buffer, '\n');
                            dataBlock = tmp + new string(buffer, 0, lastNewlineIndex);
                        }
                        tmp = string.Empty;
                        if (lastNewlineIndex >= 0)
                        {
                            if (bytesRead - lastNewlineIndex - 1 > 0)
                                tmp = new string(buffer, lastNewlineIndex + 1, bytesRead - lastNewlineIndex - 1);
                        }

                        if (dataBlock.Contains('#'))
                        {
                            while (dataBlock.Contains('#'))
                            {
                                dataBlock = removeComments(dataBlock);
                            }
                        }

                        dataBlock = removeComments(dataBlock);

                        string[] lines = dataBlock.Split(new string[] { "\n" }, StringSplitOptions.None);

                        foreach (var l in lines)
                        {
                            string[] tokens = l.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var token in tokens)
                            {
                                string value;
                                if (maxValue > 255)
                                {
                                    double scalingFactor = 255.0 / maxValue;
                                    value = ((int)(int.Parse(token) * scalingFactor)).ToString();
                                }
                                else
                                {
                                    value = token;
                                }
                                allPixels.Add(byte.Parse(value));
                            }
                        }
                    }
                    var pixels = allPixels.ToArray();
                    image.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 3, 0);
                    displayedImage.Source = image;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania pliku: " + ex.Message);
            }
        }

        private void LoadAndDisplayPPMP6(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    string format = Encoding.ASCII.GetString(reader.ReadBytes(2));

                    int width = 0;
                    int height = 0;
                    int maxValue = 0;
                    string dimensionsLine;

                    while ((dimensionsLine = ReadLine(reader)) != null)
                    {
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
                                }
                                else if (maxValue == 0)
                                {
                                    maxValue = value;
                                    break;
                                }
                            }
                        }

                        if (width > 0 && height > 0 && maxValue > 0)
                        {
                            break;
                        }
                    }

                    WriteableBitmap image = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
                    int dataSize = width * height * 3;
                    byte[] allPixels = new byte[dataSize];
                    int bytesRead = 0;

                    char[] buffer = new char[4096];

                    while (bytesRead < dataSize)
                    {
                        int bytesToRead = Math.Min(dataSize - bytesRead, 4096);
                        int bytesReadThisBlock = reader.Read(allPixels, bytesRead, bytesToRead);
                        if (bytesReadThisBlock == 0)
                        {
                            break;
                        }
                        bytesRead += bytesReadThisBlock;
                    }
                    image.WritePixels(new Int32Rect(0, 0, width, height), allPixels, width * 3, 0);

                    displayedImage.Source = image;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania pliku: " + ex.Message);
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

        private string ReadPPMFormat(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fs))
            {
                return reader.ReadLine().Trim();
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
    }
}
