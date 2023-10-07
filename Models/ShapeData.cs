using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grafika.Models
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using System.Xml.Serialization;

    namespace Grafika.Views
    {
        [Serializable]
        public class ShapeData
        {
            public double X { get; set; } // Pozycja X
            public double Y { get; set; } // Pozycja Y
            public double Width { get; set; } // Szerokość
            public double Height { get; set; } // Wysokość
            public string FillColor { get; set; } // Kolor wypełnienia w formie ciągu tekstowego, np. "#FF0000" dla czerwonego
            public string StrokeColor { get; set; } // Kolor obrysu w formie ciągu tekstowego, np. "#000000" dla czarnego
            public double StrokeThickness { get; set; } // Grubość obrysu

            public ShapeData()
            {
                X = 0;
                Y = 0;
                Width = 0;
                Height = 0;
                FillColor = "#FFFFFF";
                StrokeColor = "#000000"; 
                StrokeThickness = 5;
            }
        }

        [Serializable]
        public class LineData : ShapeData
        {
            public double X1 { get; set; }
            public double Y1 { get; set; }
            public double X2 { get; set; }
            public double Y2 { get; set; }
        }

        [Serializable]
        public class RectangleData : ShapeData
        {

        }

        [Serializable]
        public class CircleData : ShapeData
        {
            public double Diameter { get; set; }
        }

        [Serializable]
        [XmlInclude(typeof(LineData))]
        [XmlInclude(typeof(RectangleData))]
        [XmlInclude(typeof(CircleData))]
        public class CanvasData
        {
            public List<ShapeData> Shapes { get; set; } = new List<ShapeData>();
        }
    }

}
