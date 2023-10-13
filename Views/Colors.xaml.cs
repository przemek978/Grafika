using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Colorss = System.Windows.Media.Colors;
using Color = System.Windows.Media.Color;
using System.Windows.Media.Imaging;

namespace Grafika.Views
{
    public partial class Colors : Window
    {
        private bool programmaticChange = false;

        public Colors()
        {
            InitializeComponent();
            //Create3DBox();
        }

        private void RGBtoCMYK_Checked(object sender, RoutedEventArgs e)
        {
            //if (RGBInputs != null && CMYKInputs != null)
            //{
            //    RGBInputs.Visibility = Visibility.Visible;
            //    CMYKInputs.Visibility = Visibility.Collapsed;
            //}
            if (RGBInputs != null && CMYKInputs != null)
            {
                Grid.SetRow(RGBInputs, 1);
                Grid.SetRow(CMYKInputs, 2);
            }
        }

        private void CMYKtoRGB_Checked(object sender, RoutedEventArgs e)
        {
            //if (RGBInputs != null && CMYKInputs != null)
            //{
            //    CMYKInputs.Visibility = Visibility.Visible;
            //    RGBInputs.Visibility = Visibility.Collapsed;
            //}
            if (RGBInputs != null && CMYKInputs != null)
            {
                Grid.SetRow(CMYKInputs, 1);
                Grid.SetRow(RGBInputs, 2);
            }
        }

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Obsługuje zmiany wartości suwaków RGB i CMYK
            if (!programmaticChange)
                UpdateConvertedColor(false);
        }

        private void ColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Obsługuje zmiany wartości TextBoxów RGB i CMYK
            if (!programmaticChange)
                UpdateConvertedColor(true);
        }

        private void UpdateConvertedColor(bool type)
        {
            int red;
            int green;
            int blue;
            if (convertedColor != null)
            {
                if (RGBtoCMYK.IsChecked == true)
                {
                    // Konwersja z RGB do CMYK
                    if (!type)
                    {
                        programmaticChange = true;
                        red = (int)redSlider.Value;
                        green = (int)greenSlider.Value;
                        blue = (int)blueSlider.Value;
                        redTextBox.Text = red.ToString();
                        greenTextBox.Text = green.ToString();
                        blueTextBox.Text = blue.ToString();
                        programmaticChange = false;
                    }
                    else
                    {
                        programmaticChange = true;
                        red = int.Parse(redTextBox.Text);
                        green = int.Parse(greenTextBox.Text);
                        blue = int.Parse(blueTextBox.Text);

                        redSlider.Value = red;
                        greenSlider.Value = green;
                        blueSlider.Value = blue;
                        programmaticChange = false;
                    }
                    float r = red / 255.0f;
                    float g = green / 255.0f;
                    float b = blue / 255.0f;

                    float k = 1 - Math.Max(Math.Max(r, g), b);
                    float c = (1 - r - k) / (1 - k);
                    float m = (1 - g - k) / (1 - k);
                    float y = (1 - b - k) / (1 - k);

                    cyanSlider.Value = c * 100;
                    magentaSlider.Value = m * 100;
                    yellowSlider.Value = y * 100;
                    blackSlider.Value = k * 100;

                    SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, (byte)blue));
                    convertedColor.Fill = brush;
                    colorCodeTextBlock.Text = "#" + red.ToString("X2") + green.ToString("X2") + blue.ToString("X2");

                }
                else if (CMYKtoRGB.IsChecked == true)
                {
                    // Konwersja z CMYK do RGB
                    double cyan = cyanSlider.Value;
                    double magenta = magentaSlider.Value;
                    double yellow = yellowSlider.Value;
                    double black = blackSlider.Value;

                    // Wykonaj konwersję i ustaw odpowiedni kolor w Rectangle (convertedColor)
                    // Implementacja konwersji z CMYK do RGB


                    // Aktualizacja koloru w Rectangle (convertedColor)
                    // Tutaj można użyć innej implementacji konwersji z CMYK do RGB
                    SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(0, 255, 0)); // Przykładowy kolor
                    convertedColor.Fill = brush;
                }
            }
        }
        public void ChangePixelColors(ModelVisual3D model, Color newColor)
        {
            if (model.Content is GeometryModel3D geometryModel)
            {
                if (geometryModel.Material is DiffuseMaterial diffuseMaterial)
                {
                    // Pobierz materiał
                    ImageBrush imageBrush = (ImageBrush)diffuseMaterial.Brush;
                    BitmapSource bitmapSource = (BitmapSource)imageBrush.ImageSource;

                    int width = bitmapSource.PixelWidth;
                    int height = bitmapSource.PixelHeight;

                    FormatConvertedBitmap formattedBitmap = new FormatConvertedBitmap(bitmapSource, PixelFormats.Bgra32, null, 0);

                    int stride = (width * formattedBitmap.Format.BitsPerPixel + 7) / 8;
                    byte[] pixels = new byte[height * stride];
                    formattedBitmap.CopyPixels(pixels, stride, 0);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int index = y * stride + 4 * x;
                            byte r = newColor.R;
                            byte g = newColor.G;
                            byte b = newColor.B;

                            pixels[index + 2] = r;
                            pixels[index + 1] = g;
                            pixels[index] = b;
                        }
                    }

                    WriteableBitmap modifiedBitmap = new WriteableBitmap(formattedBitmap);
                    modifiedBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
                    imageBrush.ImageSource = modifiedBitmap;
                }
            }
        }

        private void convert_Click(object sender, RoutedEventArgs e)
        {
            if (RGBtoCMYK.IsChecked == true)
            {
                int red = (int)redSlider.Value;
                int green = (int)greenSlider.Value;
                int blue = (int)blueSlider.Value;
                float r = red / 255.0f;
                float g = green / 255.0f;
                float b = blue / 255.0f;

                float k = 1 - Math.Max(Math.Max(r, g), b);
                float c = (1 - r - k) / (1 - k);
                float m = (1 - g - k) / (1 - k);
                float y = (1 - b - k) / (1 - k);

                cyanSlider.Value = c * 100;
                magentaSlider.Value = m * 100;
                yellowSlider.Value = y * 100;
                blackSlider.Value = k * 100;

                SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, (byte)blue));
                convertedColor.Fill = brush;
                colorCodeTextBlock.Text = "#" + red.ToString("X2") + green.ToString("X2") + blue.ToString("X2");

            }
            else
            {

            }
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double angle = slider1.Value; // Pobierz wartość kąta z slidera
            AxisAngleRotation3D rotation = new AxisAngleRotation3D(new Vector3D(1, 0, 0), angle);

            // Obrót pierwszej ściany (czerwona)
            RotateTransform3D rotateTransform1 = new RotateTransform3D(rotation);
            meshMain1.Transform = rotateTransform1;

            // Obrót drugiej ściany (zielona)
            RotateTransform3D rotateTransform2 = new RotateTransform3D(rotation);
            meshMain2.Transform = rotateTransform2;

            // Obrót trzeciej ściany (niebieska)
            RotateTransform3D rotateTransform3 = new RotateTransform3D(rotation);
            meshMain3.Transform = rotateTransform3;
        }
    }
}
