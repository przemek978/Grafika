using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Color = System.Windows.Media.Color;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows.Input;

namespace Grafika.Views
{
    public partial class ColorsView : Window
    {
        private bool programmaticChange = false;
        private System.Windows.Point previousPosition;
        private bool isRotating = false;
        private double currentRotation = 0.0;
        public ColorsView()
        {
            InitializeComponent();
            CreateRGBCube();

            // Dodaj kontrolę myszy do Viewport3D
            viewport3D.MouseDown += Viewport3D_MouseDown;
            viewport3D.MouseUp += Viewport3D_MouseUp;
            viewport3D.MouseMove += Viewport3D_MouseMove;
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

        //private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    double angle = slider1.Value; // Pobierz wartość z suwaka
        //    RotateTransform3D rotateTransform = new RotateTransform3D();

        //    // Obróć modele wokół osi Y
        //    meshMain1.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 1, 0), angle));
        //    meshMain2.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 1, 0), angle));
        //    meshMain3.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 1, 0), angle));
        //    meshMain4.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 1, 0), angle));
        //}
        private void CreateRGBCube()
        {
            // Wymiary kostki
            double width = 1.0;
            double height = 1.0;
            double depth = 1.0;

            // Przykładowe współrzędne wierzchołków
            Point3D[] vertices = new Point3D[]
            {
        new Point3D(-0.5, -0.5, -0.5),
        new Point3D(0.5, -0.5, -0.5),
        new Point3D(0.5, 0.5, -0.5),
        new Point3D(-0.5, 0.5, -0.5),
        new Point3D(-0.5, -0.5, 0.5),
        new Point3D(0.5, -0.5, 0.5),
        new Point3D(0.5, 0.5, 0.5),
        new Point3D(-0.5, 0.5, 0.5)
            };

            // Indeksy trójkątów definiujące ściany kostki
            int[] triangleIndices = new int[]
            {
        0, 1, 2, 2, 3, 0, // Przód
        3, 2, 6, 6, 7, 3, // Góra
        7, 6, 5, 5, 4, 7, // Tył
        4, 5, 1, 1, 0, 4, // Dół
        1, 5, 6, 6, 2, 1, // Prawa
        4, 0, 3, 3, 7, 4  // Lewa
            };

            Color[] colors = new Color[]
            {
        Colors.Red, Colors.Green, Colors.Blue, Colors.White, Colors.Yellow, Colors.Magenta
            };

            Model3DGroup modelGroup = new Model3DGroup();

            for (int i = 0; i < 6; i++)
            {
                DiffuseMaterial diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(colors[i]));

                MeshGeometry3D mesh = new MeshGeometry3D();

                for (int j = 0; j < 6; j++)
                {
                    mesh.Positions.Add(vertices[triangleIndices[i * 6 + j]]);
                }

                GeometryModel3D model = new GeometryModel3D(mesh, diffuseMaterial);
                modelGroup.Children.Add(model);
            }

            ModelVisual3D cubeModelVisual = new ModelVisual3D();
            cubeModelVisual.Content = modelGroup;

            viewport3D.Children.Add(cubeModelVisual);
        }

        private void Viewport3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                previousPosition = e.GetPosition(viewport3D);
                isRotating = true;
            }
        }

        private void Viewport3D_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isRotating = false;
        }

        private void Viewport3D_MouseMove(object sender, MouseEventArgs e)
        {
            if (isRotating)
            {
                System.Windows.Point currentPosition = e.GetPosition(viewport3D);
                double dx = currentPosition.X - previousPosition.X;
                double dy = currentPosition.Y - previousPosition.Y;

                // Zastosuj obroty w oparciu o zmiany myszy
                //RotateCamera(viewport3D.Camera as PerspectiveCamera,dx, dy);
               // CreateRGBCube();
                previousPosition = currentPosition;
            }
        }

        private void RotateCamera(PerspectiveCamera camera, double dx, double dy,double dz)
        {
            double rotationSpeed = 0.5;
            double radiansX = dx * rotationSpeed * Math.PI / 180.0;
            double radiansY = dy * rotationSpeed * Math.PI / 180.0;

            Matrix3D rotation = new Matrix3D();
            rotation.Rotate(new Quaternion(camera.UpDirection, radiansX));
            rotation.Rotate(new Quaternion(camera.LookDirection, radiansY));

            camera.LookDirection = rotation.Transform(camera.LookDirection);
            camera.UpDirection = rotation.Transform(camera.UpDirection);
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
        //private void rGBCubeToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    var bmp = pictureBox1.Source== null ? new Bitmap((int)pictureBox1.Width, (int)pictureBox1.Height) : new Bitmap(pictureBox1);
        //    //var bmp = pictureBox1.Source== null ? new Bitmap((int)pictureBox1.Width, (int)pictureBox1.Height) : new Bitmap(pictureBox1);

        //    Graphics gc = Graphics.FromImage(bmp);

        //    gc.DrawLine(Pens.Black, 0, 127, 255, 127); //gora przod
        //    gc.DrawLine(Pens.Black, 127, 0, 382, 0); // gora tyl
        //    gc.DrawLine(Pens.Black, 0, 127, 127, 0); // laczenie gora lewa
        //    gc.DrawLine(Pens.Black, 255, 127, 382, 0); // laczenie gora prawa
        //    gc.DrawLine(Pens.Black, 0, 382, 0, 127); // laczenie lewy bok przod
        //    gc.DrawLine(Pens.Black, 0, 382, 255, 382); // laczenie dol		
        //    gc.DrawLine(Pens.Black, 255, 127, 255, 382); // laczenie prawy bok przod
        //    gc.DrawLine(Pens.Black, 382, 255, 382, 0); // laczenie prawy bok tyl
        //    gc.DrawLine(Pens.Black, 255, 382, 382, 255); // laczenie dol prawy
        //    pictureBox1.Source = bmp;

        //    int R = 255, G = 1, B = 255;
        //    for (int j = 128; j < 382; j++)
        //    {
        //        for (int i = 1; i < 255; i++)
        //        {
        //            bmp.SetPixel(i, j, Color.FromArgb(255, R, G, B));
        //            G++;
        //        }
        //        R--;
        //        G = 1;
        //    }
        //    R = 255;
        //    G = 1;
        //    B = 1;
        //    int x = 127;
        //    for (int i = 1; i < 127; i++)
        //    {
        //        for (int j = 0; j < 254; j++)
        //        {
        //            bmp.SetPixel(j + x, i, Color.FromArgb(255, R, G, B));
        //            G++;
        //        }
        //        B = B + 2;
        //        G = 1;
        //        x--;
        //    }
        //    G = 255;
        //    R = 255;
        //    B = 255;
        //    x = 127;
        //    int x1 = 255;
        //    for (int i = 1; i < 127; i++)
        //    {
        //        for (int j = 0; j < 254; j++)
        //        {
        //            bmp.SetPixel(i + x1, j + x, Color.FromArgb(255, R, G, B));
        //            R--;
        //        }
        //        x--;
        //        B = B - 2;
        //        R = 255;
        //    }
        //}

    }
}
