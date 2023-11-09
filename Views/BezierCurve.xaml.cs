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
    /// Logika interakcji dla klasy BezierCurve.xaml
    /// </summary>
    public partial class BezierCurve: Window
    {
        private int degree = 2; // Stopień krzywej Béziera

        public BezierCurve()
        {
            InitializeComponent();
        }



        private void DrawBezierCurveButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(degreeTextBox.Text, out int newDegree) && newDegree >= 1)
            {
                degree = newDegree;
                DrawBezierCurve(degree);
            }
            else
            {
                MessageBox.Show("Invalid degree value. Please enter a positive integer.");
            }
        }

        private void DrawBezierCurve(int degree)
        {
            canvas.Children.Clear();

            Point[] controlPoints = new Point[degree + 1];

            for (int i = 0; i <= degree; i++)
            {
                double x = (canvas.ActualWidth / degree) * i;
                double y = canvas.ActualHeight * (1 - Math.Pow(i / (double)degree, 2));
                controlPoints[i] = new Point(x, y);
            }

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure
            {
                StartPoint = controlPoints[0]
            };
            pathGeometry.Figures.Add(pathFigure);

            for (int i = 1; i <= controlPoints.Length - degree; i += degree)
            {
                BezierSegment bezierSegment = new BezierSegment
                {
                    Point1 = controlPoints[i],
                    Point2 = controlPoints[i + 1],
                    Point3 = controlPoints[i + degree - 1]
                };
                pathFigure.Segments.Add(bezierSegment);
            }

            Path bezierPath = new Path
            {
                Data = pathGeometry,
                Stroke = Brushes.Blue,
                StrokeThickness = 2
            };

            canvas.Children.Add(bezierPath);
        }

        private List<Point> GenerateControlPoints(int degree)
        {
            List<Point> points = new List<Point>();

            for (int i = 0; i <= degree; i++)
            {
                double x = (canvas.ActualWidth / degree) * i;
                double y = canvas.ActualHeight * (1 - Math.Pow(i / (double)degree, 2));
                points.Add(new Point(x, y));
            }

            return points;
        }

        private List<Point> CalculateBezierPoints(List<Point> controlPoints)
        {
            List<Point> result = new List<Point>();
            double step = 0.01;

            for (double t = 0; t <= 1; t += step)
            {
                Point point = new Point(0, 0);
                for (int i = 0; i <= degree; i++)
                {
                    double blend = BinomialCoefficient(degree, i) * Math.Pow(1 - t, degree - i) * Math.Pow(t, i);
                    point.X += controlPoints[i].X * blend;
                    point.Y += controlPoints[i].Y * blend;
                }
                result.Add(point);
            }

            return result;
        }

        private double BinomialCoefficient(int n, int k)
        {
            if (k < 0 || k > n)
                return 0;

            double result = 1;
            for (int i = 1; i <= k; i++)
            {
                result *= (n - i + 1);
                result /= i;
            }
            return result;
        }

    }
}
