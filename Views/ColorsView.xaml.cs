using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HelixToolkit.Wpf;
using Color = System.Windows.Media.Color;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Point = System.Windows.Point;
using MDriven.WPF.Media3D;
using System.Linq;

namespace Grafika.Views
{
    public partial class ColorsView : Window
    {
        private bool programmaticChange = false;

        private ModelVisual3D modelVisual;
        private double horizontalRotation = 0.0;
        private double verticalRotation = 0.0;
        private bool isRotating = false;
        private Point lastMousePos;

        public ColorsView()
        {
            InitializeComponent();
            CreateRGBCube();

            viewport3D.RotateGesture = new MouseGesture(MouseAction.LeftClick);
            viewport3D.PanGesture = new MouseGesture(MouseAction.RightClick);
            viewport3D.ZoomGesture = new MouseGesture(MouseAction.MiddleClick);
            viewport3D.CameraRotationMode = CameraRotationMode.Turntable;
            viewport3D.MouseDown += Viewport3D_MouseDown;
            viewport3D.MouseMove += Viewport3D_MouseMove;
            viewport3D.MouseUp += Viewport3D_MouseUp;
        }

        private void RGBtoCMYK_Checked(object sender, RoutedEventArgs e)
        {
            if (RGBInputs != null && CMYKInputs != null)
            {
                Grid.SetRow(RGBInputs, 1);
                Grid.SetRow(CMYKInputs, 2);
            }
        }

        private void CMYKtoRGB_Checked(object sender, RoutedEventArgs e)
        {
            if (RGBInputs != null && CMYKInputs != null)
            {
                Grid.SetRow(CMYKInputs, 1);
                Grid.SetRow(RGBInputs, 2);
            }
        }

        private void ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!programmaticChange)
                UpdateConvertedColor(false);
        }

        private void ColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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
                        int.TryParse(redTextBox.Text, out red);
                        int.TryParse(greenTextBox.Text, out green);
                        int.TryParse(blueTextBox.Text, out blue);

                        redSlider.Value = red;
                        greenSlider.Value = green;
                        blueSlider.Value = blue;
                        programmaticChange = false;
                    }
                    float r = red / 255.0f;
                    float g = green / 255.0f;
                    float b = blue / 255.0f;

                    float k = Math.Min(Math.Min(1 - r, 1 - g), 1 - b);
                    float c, m, y;
                    if (k < 1)
                    {
                        c = (1 - r - k) / (1 - k);
                        m = (1 - g - k) / (1 - k);
                        y = (1 - b - k) / (1 - k);

                        cyanSlider.Value = Math.Round(c * 100, 0);
                        magentaSlider.Value = Math.Round(m * 100, 0);
                        yellowSlider.Value = Math.Round(y * 100, 0);
                    }
                    else
                    {
                        cyanSlider.Value = 0;
                        magentaSlider.Value = 0;
                        yellowSlider.Value = 0;
                    }
                    blackSlider.Value = Math.Round(k * 100, 0);

                    cyanTextBox.Text = cyanSlider.Value.ToString();
                    magentaTextBox.Text = magentaSlider.Value.ToString();
                    yellowTextBox.Text = yellowSlider.Value.ToString();
                    blackTextBox.Text = blackSlider.Value.ToString();


                    SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, (byte)blue));
                    convertedColor.Fill = brush;
                    colorCodeTextBlock.Text = "#" + red.ToString("X2") + green.ToString("X2") + blue.ToString("X2");

                }
                else if (CMYKtoRGB.IsChecked == true)
                {
                    int cyan;
                    int magenta;
                    int yellow;
                    int black;

                    if (!type)
                    {
                        programmaticChange = true;
                        cyan = (int)cyanSlider.Value;
                        magenta = (int)magentaSlider.Value;
                        yellow = (int)yellowSlider.Value;
                        black = (int)blackSlider.Value;
                        cyanTextBox.Text = cyan.ToString();
                        magentaTextBox.Text = magenta.ToString();
                        yellowTextBox.Text = yellow.ToString();
                        blackTextBox.Text = black.ToString();
                        programmaticChange = false;
                    }
                    else
                    {
                        programmaticChange = true;
                        int.TryParse(cyanTextBox.Text, out cyan);
                        int.TryParse(magentaTextBox.Text, out magenta);
                        int.TryParse(yellowTextBox.Text, out yellow);
                        int.TryParse(blackTextBox.Text, out black);

                        cyanSlider.Value = cyan;
                        magentaSlider.Value = magenta;
                        yellowSlider.Value = yellow;
                        blackSlider.Value = black;
                        programmaticChange = false;
                    }

                    double c = cyan / 100.0;
                    double m = magenta / 100.0;
                    double y = yellow / 100.0;
                    double k = black / 100.0;

                    int r = (int)(255 * (1 - Math.Min(1, c * (1 - k) + k)));
                    int g = (int)(255 * (1 - Math.Min(1, m * (1 - k) + k)));
                    int b = (int)(255 * (1 - Math.Min(1, y * (1 - k) + k)));

                    redSlider.Value = r;
                    greenSlider.Value = g;
                    blueSlider.Value = b;
                    redTextBox.Text = r.ToString();
                    greenTextBox.Text = g.ToString();
                    blueTextBox.Text = b.ToString();

                    SolidColorBrush brush = new SolidColorBrush(Color.FromRgb((byte)r, (byte)g, (byte)b));
                    convertedColor.Fill = brush;
                    colorCodeTextBlock.Text = "#" + ((byte)r).ToString("X2") + ((byte)g).ToString("X2") + ((byte)b).ToString("X2");
                }
            }
        }

        private void CreateRGBCube()
        {
            int sideLength = 10;
            viewport3D.Children.Add(new DefaultLights());

            for (int x = 0; x < sideLength; x++)
            {
                for (int y = 0; y < sideLength; y++)
                {
                    for (int z = 0; z < sideLength; z++)
                    {
                        Color color = Color.FromRgb((byte)(x * 255 /sideLength), (byte)(y * 255 / sideLength), (byte)(z * 255 / sideLength));
                        var material = new DiffuseMaterial(new SolidColorBrush(color));

                        var cube = new BoxVisual3D
                        {
                            Center = new Point3D(x, y, z),
                            Length = 1.0,
                            Width = 1.0,
                            Height = 1.0,
                            Material = material
                        };
                        viewport3D.Children.Add(cube);

                    }
                }
            }
        }

        private void Viewport3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                lastMousePos = e.GetPosition(viewport3D);
                isRotating = true;
            }
        }

        private void Viewport3D_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isRotating = false;
        }

        private void Viewport3D_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isRotating && modelVisual != null)
            {
                Point currentMousePos = e.GetPosition(viewport3D);
                double deltaX = currentMousePos.X - lastMousePos.X;
                double deltaY = currentMousePos.Y - lastMousePos.Y;
                lastMousePos = currentMousePos;

                horizontalRotation += deltaX;
                verticalRotation += deltaY;

                var rotationTransform = new RotateTransform3D(
                    new AxisAngleRotation3D(new Vector3D(0, 1, 0), horizontalRotation),
                    new Point3D(0, 0, 0)
                );

                modelVisual.Transform = rotationTransform;
            }
        }

    }
}
