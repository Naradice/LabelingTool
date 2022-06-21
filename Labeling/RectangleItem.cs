using Labeling.ImageLabel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Foundation;

namespace Labeling
{
    public class RectangleItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private double top;
        public double RectTop
        {
            get => top;
            set
            {
                if (top == value)
                    return;
                top = value;
                NotifyPropertyChanged();
            }
        }

        private double left = 0;
        public double RectLeft
        {
            get => left;
            set
            {
                if (left == value)
                    return;
                left = value;
                NotifyPropertyChanged();
            }
        }
        private double rectWidth = 0;
        public double RectWidth
        {
            get => rectWidth;
            set
            {
                if (rectWidth == value)
                    return;
                rectWidth = value;
                NotifyPropertyChanged();
            }
        }

        private double rectHeight = 0;
        public double RectHeight
        {
            get => rectHeight;
            set
            {
                if (rectHeight == value)
                    return;
                rectHeight = value;
                NotifyPropertyChanged();
            }
        }
        private Windows.UI.Xaml.Media.DoubleCollection dashArray = null;
        public Windows.UI.Xaml.Media.DoubleCollection DashArray
        {
            get => dashArray;
            set
            {
                if (dashArray == value)
                    return;
                dashArray = value;
                NotifyPropertyChanged();
            }
        }

        private Windows.UI.Xaml.Media.SolidColorBrush color = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.White);
        public Windows.UI.Xaml.Media.SolidColorBrush Color
        {
            get => color;
            set
            {
                if (color == null)
                    return;
                color = value;
                NotifyPropertyChanged();
            }
        }

        private double rectThickness = 1;
        public double RectThickness
        {
            get => rectThickness;
            set
            {
                if (rectThickness == value)
                    return;
                rectThickness = value;
                NotifyPropertyChanged();
            }
        }

        private Guid id = Guid.NewGuid();
        public Guid ID
        {
            get => id;
        }

        public double ActualWidth = 0;
        public double ActualHeight = 0;
        public double ActualTop = 0;
        public double ActualLeft = 0;
        public labelClass label;

        public string bboxCorrdinate(double frameWidth, double frameHeight)
        {
            double center_x = left + rectWidth / 2;
            double center_y = top + rectHeight / 2;
            double y = center_y / frameWidth;
            double x = center_x / frameHeight;
            double height_ratio = rectHeight / frameWidth;
            double width_ratio = rectWidth / frameHeight;
            return $"{x.ToString()} {y.ToString()} {width_ratio.ToString()} {height_ratio.ToString()}";
        }

        /// <summary>
        /// set cordinate based on saved info
        /// </summary>
        /// <param name="frameWidth">current image width</param>
        /// <param name="frameHeight">current bimage height</param>
        /// <param name="cordinates">index center_x_ratio center_y_ratio width_ratio height_ratio</param>
        /// <returns></returns>
        public Boolean loadBboxCorrdinate(double frameWidth, double frameHeight, string[] coordinateStrings, IList<labelClass> labelNames)
        {
            if(coordinateStrings == null)
            {
                Debug.WriteLine("Info::: RectangleItem:: loadBboxCorrdinate: coordinate strings doesn't have expected format.");
                return false;
            }

            int index = 0;
            double x = 0, y = 0, width_ratio = 0, height_ratio = 0;
            try
            {
                index = int.Parse(coordinateStrings[0]);
                x = Double.Parse(coordinateStrings[1]);
                y = Double.Parse(coordinateStrings[2]);
                width_ratio = Double.Parse(coordinateStrings[3]);
                height_ratio = Double.Parse(coordinateStrings[4]);
            }
            catch
            {
                Debug.WriteLine("Info::: RectangleItem:: loadBboxCorrdinate: cannot convert string to double.");
                return false;
            }

            changeClass(labelNames[index]);

            double rectHeight = height_ratio * frameWidth;
            double rectWidth = width_ratio * frameHeight;

            double center_x = x * frameHeight;
            double center_y = y * frameWidth;

            RectWidth = rectWidth;
            RectHeight = rectHeight;
            RectLeft = center_x - rectWidth / 2; 
            RectTop = center_y - rectHeight / 2;

            return true;
        }
        /// <summary>
        /// Calculate original image coordinates from Canvas coordinate with ratios.
        /// PixelHeight/Width is original image height/width
        /// ImageHeight/Width is image height/width showed in Canvas
        /// </summary>
        /// <param name="heightRatio">PixelHeight/ImageHeight</param>
        /// <param name="widthRatio">PixelWidth/ImageWidth</param>
        /// <returns></returns>
        public void updateRectSizes(double heightRatio, double widthRatio)
        {
            ActualTop = RectTop * heightRatio;
            ActualLeft = RectLeft * widthRatio;
            ActualWidth = RectWidth * widthRatio;
            ActualHeight = RectHeight * heightRatio;
        }

         public void changeClass(labelClass label)
        {
            if (label != null)
            {
                this.label = label;
                this.Color = label.ClassColor;
            }
        }

        public void updateRect(double top, double left, double width, double height, double heightRatio, double widthRatio)
        {
            this.RectTop = top;
            this.RectLeft = left;
            this.RectWidth = width;
            this.RectHeight = height;
            this.ActualTop = top * heightRatio;
            this.ActualLeft = left * widthRatio;
            this.ActualWidth = width * widthRatio;
            this.ActualHeight = height * heightRatio;
        }

        public void clearBorder()
        {
            DashArray = null;
        }

        public RectangleItem(labelClass label)
        {
            var dash = new Windows.UI.Xaml.Media.DoubleCollection();
            dash.Add(5);
            this.DashArray = dash;
            this.RectThickness = 1;
            changeClass(label);
        }

    }

    public enum RectPosition
    {
        /*
         * 0000-> None (0)
         * 0001-> Top  (1)
         * 0010-> Bottom (2)
         * 0100-> Right (4)
         * 1000-> Left (8)
         * 
         * 0101 -> TopRight (5)
         * 1001 -> TopLeft (9)
         * 
         * 0110 -> BottomRight (6)
         * 1010 -> BottomLeft (10)
         */
        None = 0,
        Top = 1,
        Bottom = 2,
        Right = 4,
        Left = 8,
        TopRight = 5,
        TopLeft = 9,
        BottomRight = 6,
        BottomLeft = 10
    }
}
