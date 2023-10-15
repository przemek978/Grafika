using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;
using Color = System.Drawing.Color;
using Encoder = System.Drawing.Imaging.Encoder;
using Point = System.Windows.Point;

namespace Grafika.Views
{
    /// <summary>
    /// Logika interakcji dla klasy PPM.xaml
    /// </summary>
    public partial class PPM : Window
    {
        private TranslateTransform imageTranslate;
        private ScaleTransform imageScale;
        private double imageScaleFactor = 1.0;
        private Point lastMousePosition;

        public PPM()
        {
            InitializeComponent();

            TransformGroup transformGroup = new TransformGroup();
            imageScale = new ScaleTransform();
            imageTranslate = new TranslateTransform();
            transformGroup.Children.Add(imageScale);
            transformGroup.Children.Add(imageTranslate);
            displayedImage.RenderTransform = transformGroup;

            //Obsługa przesunięcia kółka myszki do zmiany skali

            displayedImage.PreviewMouseWheel += (sender, e) =>
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    double scaleChange = e.Delta > 0 ? 1.1 : 0.9;
                    imageScale.ScaleX *= scaleChange;
                    imageScale.ScaleY *= scaleChange;
                    imageScaleFactor *= scaleChange;
                    //LimitTranslation();
                    e.Handled = true;
                }
            };

            // Obsługa przesuwania obrazu przy użyciu myszki
            displayedImage.PreviewMouseLeftButtonDown += (sender, e) =>
            {
                lastMousePosition = e.GetPosition(displayedImage);
                displayedImage.CaptureMouse();
            };

            displayedImage.PreviewMouseLeftButtonUp += (sender, e) =>
            {
                displayedImage.ReleaseMouseCapture();
            };

            displayedImage.PreviewMouseMove += (sender, e) =>
            {
                if (displayedImage.IsMouseCaptured)
                {
                    Point newPosition = e.GetPosition(displayedImage);
                    double deltaX = newPosition.X - lastMousePosition.X;
                    double deltaY = newPosition.Y - lastMousePosition.Y;
                    lastMousePosition = newPosition;

                    imageTranslate.X += deltaX;
                    imageTranslate.Y += deltaY;
                    //LimitTranslation();
                }
            };

            displayedImage.MouseMove += OnImageMouseMove;
        }

        void LimitTranslation()
        {
            double minX = displayedImage.ActualWidth - (displayedImage.ActualWidth / imageScale.ScaleX);
            double minY = displayedImage.ActualHeight - (displayedImage.ActualHeight / imageScale.ScaleY);

            if (imageTranslate.X < 0)
            {
                imageTranslate.X = 0;
            }
            else if (imageTranslate.X > minX)
            {
                imageTranslate.X = minX;
            }

            if (imageTranslate.Y < 0)
            {
                imageTranslate.Y = 0;
            }
            else if (imageTranslate.Y > minY)
            {
                imageTranslate.Y = minY;
            }
        }


        private void OnImageMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(displayedImage);
            BitmapSource bitmapSource = displayedImage.Source as BitmapSource;

            if (bitmapSource != null)
            {
                int x = (int)(mousePosition.X * (bitmapSource.PixelWidth / displayedImage.ActualWidth));
                int y = (int)(mousePosition.Y * (bitmapSource.PixelHeight / displayedImage.ActualHeight));

                if (x >= 0 && x < bitmapSource.PixelWidth && y >= 0 && y < bitmapSource.PixelHeight)
                {
                    byte[] pixelData = new byte[4];
                    CroppedBitmap crop = new CroppedBitmap(bitmapSource, new Int32Rect(x, y, 1, 1));
                    crop.CopyPixels(pixelData, 4, 0);

                    Color pixelColor = Color.FromArgb(pixelData[3], pixelData[2], pixelData[1], pixelData[0]);
                    pixelInfoTextBlock.Text = $"R: {pixelColor.R}, G: {pixelColor.G}, B: {pixelColor.B}";
                }
            }
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
                        LoadAndDisplayPPMP3(filePath, double.Parse(scaleTextBox.Text));
                    }
                    else if (ppmFormat == "P6")
                    {
                        LoadAndDisplayPPMP6(filePath, double.Parse(scaleTextBox.Text));
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
            // Wczytywanie i wyświetlanie obrazu w formacie JPEG
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

        private void SaveToJPEG_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Pliki JPEG|*.jpg";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    BitmapSource bitmapSource = (BitmapSource)displayedImage.Source;

                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();

                    int.TryParse(qualityTextBox.Text, out int Quality);
                    encoder.QualityLevel = Quality != 0 ? Quality : 95; // Ustaw jakość obrazu (0-100)

                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }
                    MessageBox.Show("Obraz został zapisany w formacie JPEG.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd podczas zapisywania pliku JPEG: " + ex.Message);
                }
            }
        }

        private void LoadAndDisplayPPMP3(string filePath, double scale)
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
                    string line;

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
                                    break;
                                }
                            }
                        }

                        if (width > 0 && height > 0 && maxValue > 0)
                        {
                            break;
                        }
                    }

                    Bitmap image = new Bitmap((int)(width * scale), (int)(height * scale));
                    List<string> allPixels = new List<string>();
                    char[] buffer = new char[4096];

                    while (true)
                    {
                        int bytesRead = reader.ReadBlock(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                        {
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
                            if (maxValue > 255)
                            {
                                double scalingFactor = 255.0 / maxValue;
                                red = (int)(red * scalingFactor);
                                green = (int)(green * scalingFactor);
                                blue = (int)(blue * scalingFactor);
                            }
                            red = (int)((red / 255.0) * 255);
                            green = (int)((green / 255.0) * 255);
                            blue = (int)((blue / 255.0) * 255);


                            for (int sy = 0; sy < scale; sy++)
                            {
                                for (int sx = 0; sx < scale; sx++)
                                {
                                    int scaledX = (int)(x * scale + sx);
                                    int scaledY = (int)(y * scale + sy);
                                    image.SetPixel(scaledX, scaledY, Color.FromArgb(255, red, green, blue));
                                }
                            }
                        }
                    }
                    displayedImage.Source = BitmapToImageSource(image);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania pliku: " + ex.Message);
            }
        }

        private void LoadAndDisplayPPMP6(string filePath, double scale)
        {
            try
            {

                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    string format = Encoding.ASCII.GetString(reader.ReadBytes(2));
                    if (format != "P6")
                    {
                        return;
                    }

                    int width = 0;
                    int height = 0;
                    int maxValue = 0;
                    string dimensionsLine;
                    string line;

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

                    //Bitmap image = new Bitmap((int)(width * scale), (int)(height * scale));
                    WriteableBitmap image = new WriteableBitmap((int)(width * scale), (int)(height * scale), 96, 96, PixelFormats.Rgb24, null);
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

        private void ReadPPMP6(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PPM P6 files (.ppm)|.ppm";
            if (openFileDialog.ShowDialog() == true)
            {
                var pathToFile = openFileDialog.FileName;

                try
                {
                    using (FileStream fs = File.OpenRead(pathToFile))
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        string format = Encoding.ASCII.GetString(reader.ReadBytes(2));
                        if (format != "P6")
                        {
                            MessageBox.Show("The PPM file is not in P6 format.");
                            return;
                        }

                        string firstLine;
                        int width = 0;
                        int height = 0;
                        int maxValue = 0;
                        while ((firstLine = ReadLine(reader)) != null)
                        {
                            int commentIndex = firstLine.IndexOf('#');
                            if (commentIndex >= 0)
                            {
                                firstLine = firstLine.Substring(0, commentIndex);
                            }

                            string[] tokens = firstLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

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
                            if (maxValue > 0 && height > 0 && width > 0)
                            {
                                break;
                            }
                        }

                        WriteableBitmap ppmBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);

                        int dataSize = width * height * 3;
                        byte[] pixelData = new byte[dataSize];
                        int bytesRead = 0;

                        while (bytesRead < dataSize)
                        {
                            int bytesToRead = Math.Min(dataSize - bytesRead, 4096);
                            int bytesReadThisBlock = reader.Read(pixelData, bytesRead, bytesToRead);
                            if (bytesReadThisBlock == 0)
                            {
                                break;
                            }
                            bytesRead += bytesReadThisBlock;
                        }

                        ppmBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 3, 0);
                        displayedImage.Source = ppmBitmap;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading the PPM image: " + ex.Message);
                }
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
