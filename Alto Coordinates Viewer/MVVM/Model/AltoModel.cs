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

        private string _StringName;
        public string StringName
        {
            get { return _StringName; }
            set { _StringName = value; RaisePropertiesChanged(nameof(StringName)); }
        }


        private double _x;
        public double X
        {
            get { return _x; }
            set { _x = value; RaisePropertiesChanged(nameof(X)); }
        }

        private double _y;
        public double Y
        {
            get { return _y; }
            set { _y = value; RaisePropertiesChanged(nameof(Y)); }
        }

        private double _width;
        public double Width
        {
            get { return _width; }
            set { _width = value; RaisePropertiesChanged(nameof(Width)); }
        }

        private double _height;
        public double Height
        {
            get { return _height; }
            set { _height = value; RaisePropertiesChanged(nameof(Height)); }
        }

        private double _scaledX;
        public double ScaledX
        {
            get { return _scaledX; }
            set { _scaledX = value; RaisePropertiesChanged(nameof(ScaledX)); }
        }

        private double _scaledY;
        public double ScaledY
        {
            get { return _scaledY; }
            set { _scaledY = value; RaisePropertiesChanged(nameof(ScaledY)); }
        }

        private double _scaledWidth;
        public double ScaledWidth
        {
            get  { return _scaledWidth; }
            set { _scaledWidth = value; RaisePropertiesChanged(nameof(ScaledWidth)); }
        }

        private double _scaledHeight;
        public double ScaledHeight
        {
            get  { return _scaledHeight; }
            set { _scaledHeight = value; RaisePropertiesChanged(nameof(ScaledHeight)); }
        }

        private Brush _colorBox;
        public Brush ColorBox
        {
            get { return _colorBox; }
            set { _colorBox = value; RaisePropertiesChanged(nameof(ColorBox)); }
        }

        private int _boxTickness;
        public int BoxTickness
        {
            get { return _boxTickness; }
            set { _boxTickness = value; RaisePropertiesChanged(nameof(BoxTickness)); }
        }

        private Brush _backgroundBoxColor;
        public Brush BackgroundBoxColor
        {
            get { return _backgroundBoxColor; }
            set { _backgroundBoxColor = value; RaisePropertiesChanged(nameof(BackgroundBoxColor)); }
        }

        private double _opacityBackground;
        public double OpacityBackground
        {
            get { return _opacityBackground; }
            set { _opacityBackground = value; RaisePropertiesChanged(nameof(OpacityBackground)); }
        }


    }
}
