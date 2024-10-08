﻿using Grafika.Models;
using Grafika.Models.Grafika.Views;
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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Grafika.Views
{
    /// <summary>
    /// Logika interakcji dla klasy Canvas.xaml
    /// </summary>
    public partial class CanvasView : Window
    {
        private bool isDrawing = false;
        private bool isDragging = false;
        private bool isResizing = false;
        private Point startPoint;
        private ShapeType selectedShape = ShapeType.Line;
        private ResizeDirection resizeDirection = ResizeDirection.None;
        private Shape currentShape;
        private List<Shape> shapes = new List<Shape>();

        public CanvasView()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
        }
        private void ShapeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            int shapeNumber = 0;
            if (radioButton == null)
                return;
            if (radioButton.Tag != null)
                int.TryParse(radioButton.Tag.ToString(), out shapeNumber);

            switch (shapeNumber)
            {
                case 1:
                    selectedShape = ShapeType.Line;
                    SizeStackPanel2.Visibility = Visibility.Visible;
                    setInputs(new Line(), false);
                    break;
                case 2:
                    selectedShape = ShapeType.Rectangle;
                    SizeStackPanel2.Visibility = Visibility.Visible;
                    setInputs(new Rectangle(), false);
                    break;
                case 3:
                    selectedShape = ShapeType.Circle;
                    SizeStackPanel2.Visibility = Visibility.Collapsed;
                    setInputs(new Ellipse(), false);
                    break;
            }
        }

        ////Rysowanie/////////////////////////////////////////////////////////////////////////////////////////////////
        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double x = Convert.ToDouble(XTextBox.Text);
                double y = Convert.ToDouble(YTextBox.Text);
                double width = Convert.ToDouble(SizeTextBox1.Text);
                double height = 0;
                if (selectedShape != ShapeType.Circle)
                {
                    height = Convert.ToDouble(SizeTextBox2.Text);
                }
                Shape newShape;
                switch (selectedShape)
                {
                    case ShapeType.Line:
                        newShape = new Line();
                        break;
                    case ShapeType.Rectangle:
                        newShape = new Rectangle();
                        break;
                    case ShapeType.Circle:
                        newShape = new Ellipse();
                        break;
                    default:
                        newShape = new Line();
                        break;
                }
                Draw(newShape, x, y, width, height);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Nieprawidłowe wartości wprowadzone do utworzenia kształtu.");
            }
        }
        private void Draw(Shape shape, double x1, double y1, double size1, double size2)
        {
            if (shape is Line line)
            {
                line.X1 = x1;
                line.Y1 = y1;
                line.X2 = size1;
                line.Y2 = size2;
                line.Stroke = Brushes.Black;
                shapes.Add(line);
                canvas.Children.Add(line);
            }
            else if (shape is Rectangle rectangle)
            {
                rectangle.Width = size1;
                rectangle.Height = size2;
                rectangle.Stroke = Brushes.Black;
                rectangle.Fill = Brushes.Green;
                Canvas.SetLeft(rectangle, x1);
                Canvas.SetTop(rectangle, y1);
                shapes.Add(rectangle);
                canvas.Children.Add(rectangle);
            }
            else if (shape is Ellipse circle)
            {
                circle.Width = size1;
                circle.Height = size1;
                circle.Stroke = Brushes.Black;
                circle.Fill = Brushes.Blue;
                Canvas.SetLeft(circle, x1);
                Canvas.SetTop(circle, y1);
                shapes.Add(circle);
                canvas.Children.Add(circle);
            }
        }
        ////Metody do obsługi canvas//////////////////////////////////////////////////////////////////////////////////
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvas.Focus();
            startPoint = e.GetPosition(canvas);
            currentShape = GetShapeUnderMouse(startPoint);

            if (currentShape != null)
            {
                isDragging = false;
                isDrawing = false;
                isResizing = false;

                if (IsPointOnEllipseEdge(startPoint, currentShape))
                {
                    isResizing = true;
                    resizeDirection = ResizeDirection.Ellipse;
                }

                else if (IsPointNearTopEdge(startPoint, currentShape))
                {
                    isResizing = true;
                    if (IsPointNearLeftEdge(startPoint, currentShape))
                    {
                        resizeDirection = ResizeDirection.TopLeft;
                    }
                    else if (IsPointNearRightEdge(startPoint, currentShape))
                    {
                        resizeDirection = ResizeDirection.TopRight;
                    }
                    else
                    {
                        resizeDirection = ResizeDirection.Top;
                    }
                }

                else if (IsPointNearBottomEdge(startPoint, currentShape))
                {
                    isResizing = true;
                    if (IsPointNearLeftEdge(startPoint, currentShape))
                    {
                        resizeDirection = ResizeDirection.BottomLeft;
                    }
                    else if (IsPointNearRightEdge(startPoint, currentShape))
                    {
                        resizeDirection = ResizeDirection.BottomRight;
                    }
                    else
                    {
                        resizeDirection = ResizeDirection.Bottom;
                    }
                }

                else if (IsPointNearLeftEdge(startPoint, currentShape))
                {
                    isResizing = true;
                    resizeDirection = ResizeDirection.Left;
                }
                else if (IsPointNearRightEdge(startPoint, currentShape))
                {
                    isResizing = true;
                    resizeDirection = ResizeDirection.Right;
                }
                else
                {
                    isDragging = true;
                    isResizing = false;
                }

                PopulateEditFields(currentShape);
            }
            else
            {
                isDrawing = true;
                isDragging = false;
                isResizing = false;
            }

        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point endPoint = e.GetPosition(canvas);
            if (isDrawing)
            {
                switch (selectedShape)
                {
                    case ShapeType.Line:
                        if (currentShape == null)
                        {
                            currentShape = new Line();
                            shapes.Add(currentShape);
                            canvas.Children.Add(currentShape);
                        }
                        ((Line)currentShape).X1 = startPoint.X;
                        ((Line)currentShape).Y1 = startPoint.Y;
                        ((Line)currentShape).X2 = endPoint.X;
                        ((Line)currentShape).Y2 = endPoint.Y;
                        break;
                    case ShapeType.Rectangle:
                        if (currentShape == null)
                        {
                            currentShape = new Rectangle();
                            currentShape.Fill = Brushes.Green;
                            shapes.Add(currentShape);
                            canvas.Children.Add(currentShape);
                        }
                        currentShape.Width = Math.Abs(startPoint.X - endPoint.X);
                        currentShape.Height = Math.Abs(startPoint.Y - endPoint.Y);
                        break;
                    case ShapeType.Circle:
                        if (currentShape == null)
                        {
                            currentShape = new Ellipse();
                            currentShape.Fill = Brushes.Blue;
                            shapes.Add(currentShape);
                            canvas.Children.Add(currentShape);
                        }
                        currentShape.Width = Math.Min(Math.Abs(startPoint.X - endPoint.X), Math.Abs(startPoint.Y - endPoint.Y));
                        currentShape.Height = currentShape.Width;
                        break;
                }
                if (!(currentShape is Line))
                {
                    Canvas.SetLeft(currentShape, Math.Min(startPoint.X, endPoint.X));
                    Canvas.SetTop(currentShape, Math.Min(startPoint.Y, endPoint.Y));
                }
                currentShape.Stroke = Brushes.Black;

            }
            else if (isDragging && currentShape != null)
            {
                if (currentShape is Line line)
                {
                    double offsetX = endPoint.X - startPoint.X;
                    double offsetY = endPoint.Y - startPoint.Y;

                    line.X1 += offsetX;
                    line.Y1 += offsetY;
                    line.X2 += offsetX;
                    line.Y2 += offsetY;
                }
                else
                {
                    double offsetX = endPoint.X - startPoint.X;
                    double offsetY = endPoint.Y - startPoint.Y;

                    Canvas.SetLeft(currentShape, Canvas.GetLeft(currentShape) + offsetX);
                    Canvas.SetTop(currentShape, Canvas.GetTop(currentShape) + offsetY);
                }
                startPoint = endPoint;
            }
            else if (isResizing && currentShape != null)
            {
                double newLeft = Canvas.GetLeft(currentShape);
                double newTop = Canvas.GetTop(currentShape);
                double newWidth = currentShape.Width;
                double newHeight = currentShape.Height;

                double offsetX = (endPoint.X - startPoint.X) * 0.01;
                double offsetY = (endPoint.Y - startPoint.Y) * 0.01;

                if (resizeDirection == ResizeDirection.TopLeft)
                {
                    newTop = endPoint.Y;
                    newLeft = endPoint.X;
                    newHeight = Canvas.GetTop(currentShape) + currentShape.Height - endPoint.Y;
                    newWidth = Canvas.GetLeft(currentShape) + currentShape.Width - endPoint.X;
                }
                else if (resizeDirection == ResizeDirection.TopRight)
                {
                    newTop = endPoint.Y;
                    newHeight = Canvas.GetTop(currentShape) + currentShape.Height - endPoint.Y;
                    newWidth = endPoint.X - Canvas.GetLeft(currentShape);
                }
                else if (resizeDirection == ResizeDirection.BottomLeft)
                {
                    newLeft = endPoint.X;
                    newHeight = endPoint.Y - Canvas.GetTop(currentShape);
                    newWidth = Canvas.GetLeft(currentShape) + currentShape.Width - endPoint.X;
                }
                else if (resizeDirection == ResizeDirection.BottomRight)
                {
                    newHeight = endPoint.Y - Canvas.GetTop(currentShape);
                    newWidth = endPoint.X - Canvas.GetLeft(currentShape);
                }

                else if (resizeDirection == ResizeDirection.Top)
                {
                    newTop = endPoint.Y;
                    newHeight = Canvas.GetTop(currentShape) + currentShape.Height - endPoint.Y;
                }
                else if (resizeDirection == ResizeDirection.Bottom)
                {
                    newHeight = endPoint.Y - Canvas.GetTop(currentShape);
                }
                else if (resizeDirection == ResizeDirection.Left)
                {
                    newLeft = endPoint.X;
                    newWidth = Canvas.GetLeft(currentShape) + currentShape.Width - endPoint.X;
                }
                else if (resizeDirection == ResizeDirection.Right)
                {
                    newWidth = endPoint.X - Canvas.GetLeft(currentShape);
                }
                else if (resizeDirection == ResizeDirection.Ellipse)
                {
                    if (currentShape is Ellipse ellipse)
                    {
                        double centerX = Canvas.GetLeft(ellipse) + ellipse.Width / 2;
                        double centerY = Canvas.GetTop(ellipse) + ellipse.Height / 2;

                        double newRadius = Math.Max(Math.Abs(endPoint.X - centerX), Math.Abs(endPoint.Y - centerY));

                        Canvas.SetLeft(ellipse, centerX - newRadius);
                        Canvas.SetTop(ellipse, centerY - newRadius);
                        ellipse.Width = 2 * newRadius;
                        ellipse.Height = 2 * newRadius;
                    }
                }

                if (currentShape is Rectangle rectangle)
                {
                    if (newWidth >= 0 && newHeight >= 0)
                    {
                        Canvas.SetLeft(rectangle, newLeft);
                        Canvas.SetTop(rectangle, newTop);
                        rectangle.Width = newWidth;
                        rectangle.Height = newHeight;
                    }
                }
                else if (currentShape is Line line)
                {
                    if (resizeDirection == ResizeDirection.Left)
                    {
                        line.X1 += offsetX;
                        line.Y1 += offsetY;
                    }
                    else if (resizeDirection == ResizeDirection.Right)
                    {
                        line.X2 += offsetX;
                        line.Y2 += offsetY;
                    }
                }

            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
            isDragging = false;
            isResizing = false;
            currentShape = GetShapeUnderMouse(startPoint);
            PopulateEditFields(currentShape);
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentShape != null)
            {
                double offsetX = 0;
                double offsetY = 0;

                switch (e.Key)
                {
                    case Key.Up:
                        offsetY = -2;
                        break;
                    case Key.Down:
                        offsetY = 2;
                        break;
                    case Key.Left:
                        offsetX = -2;
                        break;
                    case Key.Right:
                        offsetX = 2;
                        break;
                }

                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    if (e.Key == Key.Up || e.Key == Key.Down)
                    {
                        currentShape.Height += offsetY;
                    }
                    else if (e.Key == Key.Left || e.Key == Key.Right)
                    {
                        currentShape.Width += offsetX;
                    }
                }
                else
                {
                    Canvas.SetLeft(currentShape, Canvas.GetLeft(currentShape) + offsetX);
                    Canvas.SetTop(currentShape, Canvas.GetTop(currentShape) + offsetY);
                }
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /////Zapis i odczyt z pliku//////////////////////////////////////////////////////////////////////////////////////////////////
        private void SaveToFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "XML Files|*.xml";
            if (dialog.ShowDialog() == true)
            {
                List<ShapeData> shapeDataList = new List<ShapeData>();

                foreach (var shape in shapes)
                {
                    if (shape is Line)
                    {
                        Line line = (Line)shape;
                        shapeDataList.Add(new LineData
                        {
                            X1 = line.X1,
                            Y1 = line.Y1,
                            X2 = line.X2,
                            Y2 = line.Y2,
                        });
                    }
                    else if (shape is Rectangle)
                    {
                        Rectangle rectangle = (Rectangle)shape;
                        shapeDataList.Add(new RectangleData
                        {
                            X = Canvas.GetLeft(rectangle),
                            Y = Canvas.GetTop(rectangle),
                            Width = rectangle.Width,
                            Height = rectangle.Height,
                        });
                    }
                    else if (shape is Ellipse)
                    {
                        Ellipse ellipse = (Ellipse)shape;
                        shapeDataList.Add(new CircleData
                        {
                            X = Canvas.GetLeft(ellipse),
                            Y = Canvas.GetTop(ellipse),
                            Diameter = ellipse.Width,
                        });
                    }
                }

                CanvasData canvasData = new CanvasData { Shapes = shapeDataList };

                using (StreamWriter sw = new StreamWriter(dialog.FileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CanvasData));
                    serializer.Serialize(sw, canvasData);
                }
            }
        }
        private void LoadFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "XML Files|*.xml";
            if (dialog.ShowDialog() == true)
            {
                using (StreamReader sr = new StreamReader(dialog.FileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CanvasData));
                    CanvasData canvasData = (CanvasData)serializer.Deserialize(sr);

                    foreach (var shapeData in canvasData.Shapes)
                    {
                        if (shapeData is LineData)
                        {
                            LineData lineData = (LineData)shapeData;
                            Draw(new Line(), lineData.X1, lineData.Y1, lineData.X2, lineData.Y2);
                        }
                        else if (shapeData is RectangleData)
                        {
                            RectangleData rectangleData = (RectangleData)shapeData;
                            Draw(new Rectangle(), rectangleData.X, rectangleData.Y, rectangleData.Width, rectangleData.Height);
                        }
                        else if (shapeData is CircleData)
                        {
                            CircleData circleData = (CircleData)shapeData;
                            Draw(new Ellipse(), circleData.X, circleData.Y, circleData.Diameter, circleData.Diameter);
                        }
                    }
                }
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        ////Wyszukiwanie istniejących//////////////////////////////////////////////////////////////////////////////////
        private Shape GetShapeUnderMouse(Point point)
        {
            foreach (var shape in shapes)
            {
                if (shape is Ellipse ellipse)
                {
                    if (IsPointInsideEllipse(point, ellipse))
                    {
                        return shape;
                    }
                }
                else if (shape is Rectangle rectangle)
                {
                    if (IsPointInsideRectangle(point, rectangle))
                    {
                        return shape;
                    }
                }
                else if (shape is Line line)
                {
                    if (IsPointNearLine(point, line))
                    {
                        return shape;
                    }
                }
            }

            return null;
        }

        private void PopulateEditFields(Shape selectedShape)
        {
            if (selectedShape != null)
            {
                setInputs(selectedShape, true);
            }
            else
            {
                XTextBox.Text = "";
                YTextBox.Text = "";
                SizeTextBox1.Text = "";
                SizeTextBox2.Text = "";
            }
        }

        ////Wyszukiwanie kształtów////////////////////////////////////////////////////////////////////////////////
        private bool IsPointInsideEllipse(Point point, Ellipse ellipse)
        {
            double halfWidth = ellipse.Width / 2;
            double halfHeight = ellipse.Height / 2;
            double centerX = Canvas.GetLeft(ellipse) + halfWidth;
            double centerY = Canvas.GetTop(ellipse) + halfHeight;

            double normalizedX = (point.X - centerX) / halfWidth;
            double normalizedY = (point.Y - centerY) / halfHeight;

            return (normalizedX * normalizedX + normalizedY * normalizedY) <= 1.0;
        }

        private bool IsPointInsideRectangle(Point point, Rectangle rectangle)
        {
            double left = Canvas.GetLeft(rectangle);
            double top = Canvas.GetTop(rectangle);
            double right = left + rectangle.Width;
            double bottom = top + rectangle.Height;

            return point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom;
        }

        private bool IsPointNearLine(Point point, Line line)
        {
            double x1 = line.X1;
            double y1 = line.Y1;
            double x2 = line.X2;
            double y2 = line.Y2;

            double distance = Math.Abs((y2 - y1) * point.X - (x2 - x1) * point.Y + x2 * y1 - y2 * x1) /
                             Math.Sqrt(Math.Pow(y2 - y1, 2) + Math.Pow(x2 - x1, 2));

            return distance <= 5;
        }

        ////Wyszukiwanie krawędzi////////////////////////////////////////////////////////////////////////////////
        private bool IsPointNearTopEdge(Point point, Shape shape)
        {
            if (shape is Rectangle rectangle)
            {
                double top = Canvas.GetTop(rectangle);
                double margin = 5;

                return point.Y >= top - margin && point.Y <= top + margin;
            }

            return false;
        }

        private bool IsPointNearBottomEdge(Point point, Shape shape)
        {
            if (shape is Rectangle rectangle)
            {
                double top = Canvas.GetTop(rectangle);
                double bottom = top + rectangle.Height;
                double margin = 5;

                return point.Y >= bottom - margin && point.Y <= bottom + margin;
            }

            return false;
        }

        private bool IsPointNearLeftEdge(Point point, Shape shape)
        {
            if (shape is Rectangle rectangle)
            {
                double left = Canvas.GetLeft(rectangle);
                double margin = 5;

                return point.X >= left - margin && point.X <= left + margin;
            }
            else if (shape is Line line)
            {
                double minX = Math.Min(line.X1, line.X2);
                double margin = 5;

                return point.X >= minX - margin && point.X <= minX + margin;
            }

            return false;
        }

        private bool IsPointNearRightEdge(Point point, Shape shape)
        {
            if (shape is Rectangle rectangle)
            {
                double left = Canvas.GetLeft(rectangle);
                double right = left + rectangle.Width;
                double margin = 5;

                return point.X >= right - margin && point.X <= right + margin;
            }
            else if (shape is Line line)
            {
                double maxX = Math.Max(line.X1, line.X2);
                double margin = 5;

                return point.X >= maxX - margin && point.X <= maxX + margin;
            }

            return false;
        }
        private bool IsPointOnEllipseEdge(Point point, Shape shape)
        {
            if (shape is Ellipse ellipse)
            {
                double centerX = Canvas.GetLeft(ellipse) + ellipse.Width / 2;
                double centerY = Canvas.GetTop(ellipse) + ellipse.Height / 2;
                double radius = ellipse.Width / 2;

                double distance = Math.Sqrt(Math.Pow(point.X - centerX, 2) + Math.Pow(point.Y - centerY, 2));

                double margin = 5;

                return Math.Abs(distance - radius) <= margin;
            }

            return false;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        ////Inne/////////////////////////////////////////////////////////////////////////////////////////////
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            XTextBox.Text = "";
            YTextBox.Text = "";
            SizeTextBox1.Text = "";
            SizeTextBox2.Text = "";
            XEditTextBox.Text = "";
            YEditTextBox.Text = "";
            SizeEditTextBox1.Text = "";
            SizeEditTextBox2.Text = "";
            shapes.Clear();
        }

        public void setInputs(Shape shape, bool isEdit)
        {
            if (!isEdit)
            {
                if (shape is Line line)
                {
                    XLabel.Content = "X1";
                    YLabel.Content = "Y1";
                    SizeLabel1.Content = "X2";
                    SizeLabel2.Content = "Y2";
                }
                else if (shape is Rectangle rectangle)
                {
                    XLabel.Content = "X";
                    YLabel.Content = "Y";
                    SizeLabel1.Content = "Szerokość";
                    SizeLabel2.Content = "Wysokość";
                }
                else if (shape is Ellipse ellipse)
                {
                    XLabel.Content = "X";
                    YLabel.Content = "Y";
                    SizeLabel1.Content = "Średnica";
                    SizeLabel2.Content = "";
                }
            }
            else
            {
                if (shape is Line line)
                {
                    XEditTextBox.Text = line.X1.ToString();
                    YEditTextBox.Text = line.Y1.ToString();
                    SizeEditTextBox1.Text = line.X2.ToString();
                    SizeEditTextBox2.Text = line.Y2.ToString();
                    XEditLabel.Content = "X1";
                    YEditLabel.Content = "Y1";
                    SizeEditLabel1.Content = "X2";
                    SizeEditLabel2.Visibility = Visibility.Visible;
                    SizeEditTextBox2.Visibility = Visibility.Visible;
                    SizeEditLabel2.Content = "Y2";
                }
                else if (shape is Rectangle rectangle)
                {
                    XEditTextBox.Text = Canvas.GetLeft(rectangle).ToString();
                    YEditTextBox.Text = Canvas.GetTop(rectangle).ToString();
                    SizeEditTextBox1.Text = rectangle.Width.ToString();
                    SizeEditTextBox2.Text = rectangle.Height.ToString();
                    XEditLabel.Content = "X";
                    YEditLabel.Content = "Y";
                    SizeEditLabel1.Content = "Szerokość";
                    SizeEditLabel2.Visibility = Visibility.Visible;
                    SizeEditTextBox2.Visibility = Visibility.Visible;
                    SizeEditLabel2.Content = "Wysokość";
                }
                else if (shape is Ellipse ellipse)
                {
                    XEditTextBox.Text = Canvas.GetLeft(ellipse).ToString();
                    YEditTextBox.Text = Canvas.GetTop(ellipse).ToString();
                    SizeEditTextBox1.Text = ellipse.Width.ToString();
                    SizeEditTextBox2.Text = "";
                    XEditLabel.Content = "X";
                    YEditLabel.Content = "Y";
                    SizeEditLabel1.Content = "Średnica";
                    SizeEditLabel2.Content = "";
                    SizeEditLabel2.Visibility = Visibility.Collapsed;
                    SizeEditTextBox2.Visibility = Visibility.Collapsed;
                }
            }

        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentShape != null)
            {
                try
                {
                    if (double.TryParse(XEditTextBox.Text, out double x) &&
                        double.TryParse(YEditTextBox.Text, out double y) &&
                        double.TryParse(SizeEditTextBox1.Text, out double s1)
                        )
                    {
                        double.TryParse(SizeEditTextBox2.Text, out double s2);
                        if (!(currentShape is Line))
                        {
                            Canvas.SetLeft(currentShape, x);
                            Canvas.SetTop(currentShape, y);
                        }

                        if (currentShape is Rectangle rectangle)
                        {
                            rectangle.Width = s1;
                            rectangle.Height = s2;
                        }
                        else if (currentShape is Ellipse ellipse)
                        {
                            ellipse.Width = s1;
                            ellipse.Height = s1;
                        }
                        else if (currentShape is Line line)
                        {
                            line.X1 = x;
                            line.Y1 = y;
                            line.X2 = s1;
                            line.Y2 = s2;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Nieprawidłowe wartości wprowadzone do edycji kształtu.");
                }
            }
        }
    }
}
