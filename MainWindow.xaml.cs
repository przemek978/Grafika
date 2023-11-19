using Grafika.Views;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Transform = Grafika.Views.Transform;

namespace Grafika
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Canvas_Click(object sender, RoutedEventArgs e)
        {
            CanvasView canvas = new CanvasView();
            canvas.Show();
            this.Hide();
        }

        private void PPM_Click(object sender, RoutedEventArgs e)
        {
            PPM pPM = new PPM();
            pPM.Show();
            this.Hide();
        }

        private void Colors_Click(object sender, RoutedEventArgs e)
        {
            ColorsView colors = new ColorsView();
            colors.Show();
            this.Hide();
        }

        private void Transform_Click(object sender, RoutedEventArgs e)
        {
            Transform transform = new Transform();
            transform.Show();
            this.Hide();
        }

        private void HistBin_Click(object sender, RoutedEventArgs e)
        {
            HistBin histBin = new();
            histBin.Show();
            this.Hide();
        }

        private void Curve_Click(object sender, RoutedEventArgs e)
        {
            BezierCurve curve = new BezierCurve();
            curve.Show();
            this.Hide();
        }

        private void Transform2D_Click(object sender, RoutedEventArgs e)
        {
            Transformations2D transformations2D = new Transformations2D();
            transformations2D.Show();
            this.Hide();
        }

        private void Operators_Click(object sender, RoutedEventArgs e)
        {
            Operators operators = new Operators();
            operators.Show();
            this.Hide();
        }

        private void Analysis_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
