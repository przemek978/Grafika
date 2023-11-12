using Grafika.Models;
using Grafika.Models.Grafika.Views;
using HelixToolkit.Wpf;
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
using System.Xml.Serialization;
using Polygon = System.Windows.Shapes.Polygon;

namespace Grafika.Views
{
    /// <summary>
    /// Logika interakcji dla klasy Transformations2D.xaml
    /// </summary>
    public partial class Transformations2D : Window
    {
        private bool isDrawing = false;
        private bool isDragging = false;
        private Point startPoint;
        private Shape currentShape;
        private List<Polygon> shapes = new List<Polygon>();
        private List<Point> points = new List<Point>();
        Point centerOfPolygon;


        public Transformations2D()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            points.Add(new Point(200, 200));
            points.Add(new Point(400, 200));
            points.Add(new Point(450, 400));
            points.Add(new Point(400, 600));
            points.Add(new Point(200, 600));
            centerOfPolygon = new Point((canvas.ActualWidth / 2) - 20, canvas.ActualHeight / 2);
            pointListBox.ItemsSource = points;
            DataContext = this;
        }

        ////Metody do obsługi canvas//////////////////////////////////////////////////////////////////////////////////
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvas.Focus();
            startPoint = e.GetPosition(canvas);
            currentShape = GetShapeUnderMouse(startPoint);

            if (currentShape != null)
            {
                isDragging = true;
                isDrawing = false;
            }
            else
            {
                isDrawing = true;
                isDragging = false;
            }

        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point endPoint = e.GetPosition(canvas);
            if (isDrawing)
            {

                //if (currentShape == null)
                //{
                //    currentShape = new System.Windows.Shapes.Polygon
                //    {
                //        Points = new PointCollection(points),
                //        Stroke = Brushes.Black,
                //        StrokeThickness = 1,
                //        Fill = Brushes.LightBlue
                //    };
                //    currentShape.Fill = Brushes.Green;
                //    shapes.Add(currentShape);
                //    canvas.Children.Add(currentShape);
                //}
                //currentShape.Width = Math.Abs(startPoint.X - endPoint.X);
                //currentShape.Height = Math.Abs(startPoint.Y - endPoint.Y);
                //currentShape.Stroke = Brushes.Black;
            }
            else if (isDragging && currentShape != null)
            {
                var shape = currentShape as Polygon;
                Point newPoint = e.GetPosition(canvas);

                double offsetX = newPoint.X - startPoint.X;
                double offsetY = newPoint.Y - startPoint.Y;

                // Przesuń współrzędne wszystkich punktów wielokąta
                for (int i = 0; i < shape.Points.Count; i++)
                {
                    shape.Points[i] = new Point(shape.Points[i].X + offsetX, shape.Points[i].Y + offsetY);
                }

                startPoint = newPoint;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
            isDragging = false;
            //currentShape = GetShapeUnderMouse(startPoint);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        private void AddPointButton_Click(object sender, RoutedEventArgs e)
        {
            AddPoint();
        }

        private void AddPoint()
        {
            try
            {
                double x = Convert.ToDouble(XTextBox.Text);
                double y = Convert.ToDouble(YTextBox.Text);
                points.Add(new Point(x, y));
                pointListBox.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nieprawidłowe wartości wprowadzone do utworzenia kształtu.");
            }
        }

        private void AddPolygonButton_Click(object sender, RoutedEventArgs e)
        {
            Draw();
        }

        private void Draw()
        {
            Polygon polygon = new Polygon
            {
                Points = new PointCollection(points),
                Stroke = Brushes.Black,
                StrokeThickness = 3,
                Fill = Brushes.DarkCyan,
            };
            shapes.Add(polygon);
            canvas.Children.Add(polygon);
            points.Clear();
            pointListBox.Items.Refresh();
        }

        private void Transform(double h, double v)
        {
            try
            {
                var shape = currentShape as Polygon;
                List<Point> rotatedPoints = new List<Point>();
                foreach (var point in shape.Points)
                {

                    double x = point.X + h;
                    double y = point.Y + v;

                    rotatedPoints.Add(new Point(x, y));

                }
                shape.Points = new PointCollection(rotatedPoints);
            }
            catch (Exception ex)
            {

            }
        }

        private void Rotation(double alfa, Point givenPoint)
        {
            try
            {
                var shape = currentShape as Polygon;
                List<Point> rotatedPoints = new List<Point>();
                foreach (var point in shape.Points)
                {
                    double translatedX = point.X - givenPoint.X;
                    double translatedY = point.Y - givenPoint.Y;

                    double x = givenPoint.X + translatedX * Math.Cos(alfa) - translatedY * Math.Sin(alfa);
                    double y = givenPoint.Y + translatedX * Math.Sin(alfa) + translatedY * Math.Cos(alfa);

                    rotatedPoints.Add(new Point(x, y));

                }
                shape.Points = new PointCollection(rotatedPoints);
            }
            catch (Exception ex)
            {

            }
        }

        private void Scale(double k, Point givenPoint)
        {
            try
            {
                var shape = currentShape as Polygon;
                List<Point> rotatedPoints = new List<Point>();
                foreach (var point in shape.Points)
                {
                    double x = point.X * k + (1 - k) * givenPoint.X;
                    double y = point.Y * k + (1 - k) * givenPoint.Y;

                    rotatedPoints.Add(new Point(x, y));

                }
                shape.Points = new PointCollection(rotatedPoints);
            }
            catch (Exception ex)
            {

            }
        }

        private void TranslationButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(translationXTextBox.Text, out double h) & double.TryParse(translationYTextBox.Text, out double v))
            {
                Transform(h, v);
            }
        }

        private void RotationButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(translationXTextBox.Text, out double x) & double.TryParse(translationYTextBox.Text, out double y) & double.TryParse(rotationAngleTextBox.Text, out double alfa))
            {
                Point givenPoint = new Point(x, y);
                Rotation(30, centerOfPolygon);

            }
        }

        private void ScalingButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(translationXTextBox.Text, out double x) & double.TryParse(translationYTextBox.Text, out double y) & double.TryParse(scalingFactorTextBox.Text, out double k))
            {
                Point givenPoint = new Point(x, y);
                Scale(k, centerOfPolygon);
            }
        }

        /////Zapis i odczyt z pliku//////////////////////////////////////////////////////////////////////////////////////////////////
        private void SaveToFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "XML Files|*.xml";
            if (dialog.ShowDialog() == true)
            {
                List<PolygonData> shapeDataList = new List<PolygonData>();

                foreach (var shape in shapes)
                {
                    var pointsList = new List<Point>();
                    foreach( var point in shape.Points)
                    {
                        pointsList.Add(point);
                    }

                    shapeDataList.Add(new PolygonData
                    {
                        points = pointsList,
                        FillColor = shape.Fill.ToString(),
                    }) ;

                }

                PolygonList canvasData = new PolygonList { Shapes = shapeDataList };

                using (StreamWriter sw = new StreamWriter(dialog.FileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PolygonList));
                    serializer.Serialize(sw, canvasData);
                }
            }
        }
        private void LoadFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "XML Files|*.xml";
            if (dialog.ShowDialog() == true)
            {
                using (StreamReader sr = new StreamReader(dialog.FileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CanvasData));
                    CanvasData canvasData = (CanvasData)serializer.Deserialize(sr);

                    foreach (var shapeData in canvasData.Shapes)
                    {
                        RectangleData rectangleData = (RectangleData)shapeData;
                        Draw();
                    }
                }
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        ////Wyszukiwanie istniejących//////////////////////////////////////////////////////////////////////////////////
        private Shape GetShapeUnderMouse(Point point)
        {
            foreach (var shape in canvas.Children)
            {
                if (shape is Polygon polygon)
                {
                    if (IsPointInsidePolygon(startPoint, polygon.Points))
                    {
                        return polygon;
                    }
                }
            }

            return null;
        }
        private bool IsPointInsidePolygon(Point point, PointCollection polygonPoints)
        {
            int count = polygonPoints.Count;
            bool inside = false;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                if (((polygonPoints[i].Y > point.Y) != (polygonPoints[j].Y > point.Y)) &&
                    (point.X < (polygonPoints[j].X - polygonPoints[i].X) * (point.Y - polygonPoints[i].Y) / (polygonPoints[j].Y - polygonPoints[i].Y) + polygonPoints[i].X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            XTextBox.Text = "";
            YTextBox.Text = "";
            translationXTextBox.Text = "";
            translationYTextBox.Text = "";
            rotationAngleTextBox.Text = "";
            scalingFactorTextBox.Text = "";
            shapes.Clear();
        }
    }
}
