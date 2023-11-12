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
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;

namespace Grafika.Views
{
    /// <summary>
    /// Logika interakcji dla klasy BezierCurve.xaml
    /// </summary>
    public partial class BezierCurve : Window
    {
        private int degree = 3; // Stopień krzywej Béziera
        List<Point> controlPoints;
        private Point selectedPoint;
        private bool isDragging = false;

        public BezierCurve()
        {
            InitializeComponent();
            controlPoints = new List<Point>();
            degreeTextBox.Text = degree.ToString();
            pointListBox.ItemsSource = controlPoints;
            DataContext = this;
            DrawPoints();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                int indexOfPoint = controlPoints.IndexOf(selectedPoint);
                if (indexOfPoint == -1) return;

                Point mousePosition = e.GetPosition(canvas);
                selectedPoint.X = mousePosition.X;
                selectedPoint.Y = mousePosition.Y;
                controlPoints[indexOfPoint] = selectedPoint;
                DrawBezierCurve();
                DrawPoints();
            }
        }
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void SetPointClick(object sender, MouseButtonEventArgs e)
        {

            Point mousePosition = e.GetPosition(canvas);

            bool existsPoint = controlPoints.Any(p => CalculateDistance(p, mousePosition) < 10);

            if (int.TryParse(degreeTextBox.Text, out int newDegree))
            {
                degree = newDegree;
            }

            if (!existsPoint)
            {
                if (degree >= controlPoints.Count)
                {
                    controlPoints.Add(new Point(mousePosition.X, mousePosition.Y));
                }
                else
                {
                    MessageBox.Show($"Za dużo punktów, Stopień równy {degree} pozwala na dodanie {degree + 1} punktów");
                }
            }
            else if (existsPoint)
            {
                isDragging = true;
                selectedPoint = controlPoints.FirstOrDefault(p => CalculateDistance(p, mousePosition) < 10);
            }
            DrawBezierCurve();
            DrawPoints();
        }

        public void DrawPoints()
        {
            foreach (var p in controlPoints)
            {
                Ellipse ellipse = new Ellipse
                {
                    Fill = Brushes.Red,
                    Width = 8,
                    Height = 8,
                };
                Canvas.SetLeft(ellipse, p.X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, p.Y - ellipse.Height / 2);
                canvas.Children.Add(ellipse);
            }
            pointListBox.Items.Refresh();
        }


        private void DrawBezierCurve()
        {
            canvas.Children.Clear();
            Path bezierPath = new Path();
            bezierPath.Stroke = Brushes.Black;
            bezierPath.StrokeThickness = 5;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = controlPoints[0];

            PolyBezierSegment polyBezierSegment = new PolyBezierSegment();
            polyBezierSegment.Points = new PointCollection(CalculateBezierPoints());

            pathFigure.Segments.Add(polyBezierSegment);
            pathGeometry.Figures.Add(pathFigure);
            bezierPath.Data = pathGeometry;

            canvas.Children.Add(bezierPath);
        }

        private PointCollection CalculateBezierPoints()
        {
            PointCollection bezierPoints = new PointCollection();

            //for (int i = 0; i <= degree; i++)
            //{
            //    double t = (double)i / degree;
            //    Point point = CalculateBezierPoint(t);
            //    bezierPoints.Add(point);
            //}

            for (double i = 0; i <= 1; i += 0.005)
            {
                Point bezierPoint = CalculateBezierPoint(i);
                bezierPoints.Add(bezierPoint);
            }

            for (int i = 0; i < bezierPoints.Count - 1; i++)
            {
                Line line = new Line
                {
                    X1 = bezierPoints[i].X,
                    Y1 = bezierPoints[i].Y,
                    X2 = bezierPoints[i + 1].X,
                    Y2 = bezierPoints[i + 1].Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 5
                };

                canvas.Children.Add(line);
            }

            return bezierPoints;
        }

        private Point CalculateBezierPoint(double t)
        {
            int n = controlPoints.Count - 1;

            double x = 0;
            double y = 0;

            for (int i = 0; i <= n; i++)
            {
                double berstein = Bernstein(n, i, t);
                x += berstein * controlPoints[i].X;
                y += berstein * controlPoints[i].Y;
            }

            return new Point(x, y);
        }

        private int Newton(int n, int i)  //https://pomax.github.io/bezierinfo/
        {
            int result = 1;

            for (int j = 1; j <= i; j++)
            {
                result = result * (n - j + 1) / j;
            }

            return result;
        }

        private double Bernstein(int n, int i, double t)
        {
            return Newton(n, i) * Math.Pow(1.0 - t, n - i) * Math.Pow(t, i);
        }

        private double CalculateDistance(Point point1, Point point2)
        {
            double deltaX = point2.X - point1.X;
            double deltaY = point2.Y - point1.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        private void ClearCanvas(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            controlPoints.Clear();
        }

    }
}
