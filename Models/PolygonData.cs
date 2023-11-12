using Grafika.Models.Grafika.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Grafika.Models
{
    [Serializable]
    public class PolygonData
    {
        public List<Point> points { get; set; }
        public string FillColor { get; set; }
    }

    [Serializable]
    [XmlInclude(typeof(PolygonData))]
    public class PolygonList
    {
        public List<PolygonData> Shapes { get; set; } = new List<PolygonData>();
    }
}
