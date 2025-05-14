using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DevExpress.Mvvm;

namespace Alto_Coordinates_Viewer.MVVM.Model
{
    class AltoModel : BindableBase
    {

        private double _x;
        public double X
        {
            get => _x;
            set { _x = value; RaisePropertiesChanged(nameof(X)); }
        }

        private double _y;
        public double Y
        {
            get => _y;
            set { _y = value; RaisePropertiesChanged(nameof(Y)); }
        }

        private double _width;
        public double Width
        {
            get => _width;
            set { _width = value; RaisePropertiesChanged(nameof(Width)); }
        }

        private double _height;
        public double Height
        {
            get => _height;
            set { _height = value; RaisePropertiesChanged(nameof(Height)); }
        }

        private double _scaledX;
        public double ScaledX
        {
            get => _scaledX;
            set { _scaledX = value; RaisePropertiesChanged(nameof(ScaledX)); }
        }

        private double _scaledY;
        public double ScaledY
        {
            get => _scaledY;
            set { _scaledY = value; RaisePropertiesChanged(nameof(ScaledY)); }
        }

        private double _scaledWidth;
        public double ScaledWidth
        {
            get => _scaledWidth;
            set { _scaledWidth = value; RaisePropertiesChanged(nameof(ScaledWidth)); }
        }

        private double _scaledHeight;
        public double ScaledHeight
        {
            get => _scaledHeight;
            set { _scaledHeight = value; RaisePropertiesChanged(nameof(ScaledHeight)); }
        }

        private Brush _colorBox;

        public Brush ColorBox
        {
            get { return _colorBox; }
            set { _colorBox = value; RaisePropertiesChanged(nameof(ColorBox)); }
        }

        public string XmlLine { get; set; } // The original <String>...</String><SP.../> line


        //public double X { get; set; }
        //public double Y { get; set; }
        //public double Width { get; set; }
        //public double Height { get; set; }

        //public double ScaledX { get; set; }
        //public double ScaledY { get; set; }
        //public double ScaledWidth { get; set; }
        //public double ScaledHeight { get; set; }

    }
}
