using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Labeling.ImageLabel;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Labeling
{
    public class Images: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        // current/org
        double widthRatio = 0;
        double heightRatio = 0;
        public bool loaded = false;
        public string fileType = null;
        public ObservableCollection<string> yoloLines = new ObservableCollection<string>();

        private string imageLocation = "";
        public string ImageLocation { 
            get => this.imageLocation;
            set {
                if (value == imageLocation)
                    return;
                imageLocation = value;
                //NotifyPropertyChanged();
            } 
        }

        public SoftwareBitmap softwareBitmap = null;
        public Guid encoderId = new Guid();
        private SoftwareBitmapSource image = new SoftwareBitmapSource();
        public SoftwareBitmapSource Image
        {
            get => image;
            set
            {
                if (value == image)
                    return;
                image = value;
                //NotifyPropertyChanged();
            }
        }

        private double imageWidth = 0;
        public double ImageWidth
        {
            get => imageWidth;
            set
            {
                if (imageWidth == value)
                    return;
                imageWidth = value;
                NotifyPropertyChanged();
            }
        }

        private double imageHeight  = 0;
        public double ImageHeight 
        {
            get => imageHeight ;
            set
            {
                if (imageHeight  == value)
                    return;
                imageHeight  = value;
                NotifyPropertyChanged();
            }
        }


        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async void setImage(string path)
        {
            Windows.Storage.StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(path);
            using (Windows.Storage.Streams.IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read)) {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                await this.Image.SetBitmapAsync(softwareBitmap);
            }

            switch (fileType.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    encoderId = BitmapEncoder.JpegEncoderId;
                    break;
                case ".png":
                    encoderId = BitmapEncoder.PngEncoderId;
                    break;
                case ".gif":
                    encoderId = BitmapEncoder.GifEncoderId;
                    break;
                case ".tiff":
                    encoderId = BitmapEncoder.TiffEncoderId;
                    break;
                case ".bmp":
                    encoderId = BitmapEncoder.BmpEncoderId;
                    break;
                default:
                    encoderId = new Guid();
                    break;
            }
        }

        public async void setImage(Windows.Storage.StorageFile file)
        {
            using (Windows.Storage.Streams.IRandomAccessStream stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                try
                {
                    await this.Image.SetBitmapAsync(softwareBitmap);
                }catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            switch (fileType.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    encoderId = BitmapEncoder.JpegEncoderId;
                    break;
                case ".png":
                    encoderId = BitmapEncoder.PngEncoderId;
                    break;
                case ".gif":
                    encoderId = BitmapEncoder.GifEncoderId;
                    break;
                case ".tiff":
                    encoderId = BitmapEncoder.TiffEncoderId;
                    break;
                case ".bmp":
                    encoderId = BitmapEncoder.BmpEncoderId;
                    break;
                default:
                    encoderId = new Guid();
                    break;
            }
        }

        public async void setImage(SoftwareBitmap newImage)
        {
            await this.image.SetBitmapAsync(newImage);
        }


        public void setImageSize(double width, double height)
        {
            double ratio = width / this.softwareBitmap.PixelWidth;
            double temp_ratio = height / this.softwareBitmap.PixelHeight;
            if (ratio > temp_ratio)
                ratio = temp_ratio;

            ImageWidth = ratio * this.softwareBitmap.PixelWidth;
            ImageHeight = ratio * this.softwareBitmap.PixelHeight;
            widthRatio = ImageWidth/ this.softwareBitmap.PixelWidth;
            heightRatio = ImageHeight / this.softwareBitmap.PixelHeight;
        }

        private ObservableCollection<RectangleItem> rectList = new ObservableCollection<RectangleItem>();
        public ObservableCollection<RectangleItem> RectList
        {
            get => rectList;
            set
            {
                if (rectList == value)
                    return;
                rectList = value;
                NotifyPropertyChanged();

            }
        }

        public int getIndexOfRectangle(Rectangle item)
        {
            for(int i = 0; i < rectList.Count;i++)
            {
                if (item.Tag.ToString() == rectList[i].ID.ToString())
                    return i;
            }
            return -1;
        }

        public void setCurrentRectSize(double top, double left, double width, double height)
        {
            var current_rect = this.RectList.Last();
            if (current_rect != null)
            {
                current_rect.updateRect(top, left, width, height, 1/heightRatio, 1/widthRatio);
            }
        }

        public void updateAllRectsize()
        {
            if(rectList.Count() > 0)
            {
                foreach(var rect in rectList)
                {
                    rect.RectTop = rect.ActualTop * heightRatio;
                    rect.RectLeft = rect.ActualLeft * widthRatio;
                    rect.RectWidth = rect.ActualWidth * widthRatio;
                    rect.RectHeight = rect.ActualHeight * heightRatio;
                }
            }
        }

        public bool removeRectItemAt(int index)
        {
            if (this.RectList.Count > index && index > -1)
            {
                this.RectList.RemoveAt(index);
                this.yoloLines.RemoveAt(index);
                string[] items;
                for(int i = index; yoloLines.Count > i; i++)
                {
                    items = haveYoloStyle(yoloLines[i]);
                    items.SetValue(i.ToString(), 0);
                    yoloLines[i] = string.Join(" ", items);
                }


                return true;
            }
            return false;
        }

        public RectPosition getRectPosition(int index, double X, double Y)
        {
            RectangleItem rect = RectList[index];
            double top = rect.RectTop;
            double left = rect.RectLeft;

            uint positionState = 0b_0000;
            bool isTopSide = Y - top < 10;
            bool isRightSide = rectList[index].RectWidth - (X - left) < 10;
            bool isLeftSide = X - left < 10;
            bool isBottomSide = rectList[index].RectHeight - (Y - top)  < 10;

            if (isTopSide)
                positionState += 0b_0001;
            else if (isBottomSide)
                positionState += 0b_0010;
            if (isRightSide)
                positionState += 0b_0100;
            else if (isLeftSide)
                positionState += 0b_1000;

            RectPosition position = (RectPosition)Convert.ToInt32(positionState);

            return position;
        }

        public bool updateRectItem(int index, RectPosition clickedPosition ,double X, double Y)
        {
            if (this.RectList.Count > index && index > -1)
            {
                double top = rectList[index].RectTop;
                double left = rectList[index].RectLeft;
                double width = rectList[index].RectWidth;
                double height = rectList[index].RectHeight;

                switch (clickedPosition)
                {
                    case RectPosition.Top:
                        height += top - Y;
                        top = Y;
                        break;
                    case RectPosition.TopRight:
                        height += top - Y;
                        top = Y;
                        width = X - left;
                        break;
                    case RectPosition.Right:
                        width = X - left;
                        break;
                    case RectPosition.BottomRight:
                        height = Y - top;
                        width = X - left;
                        break;
                    case RectPosition.Bottom:
                        height = Y - top;
                        break;
                    case RectPosition.BottomLeft:
                        width += left - X;
                        left = X;
                        height = Y - top;
                        break;
                    case RectPosition.Left:
                        width += left - X;
                        left = X;
                        break;
                    case RectPosition.TopLeft:
                        height += top - Y;
                        width += left - X;
                        top = Y;
                        left = X;
                        break;
                    default:
                        break;
                }

                rectList[index].RectTop = top;
                rectList[index].RectLeft = left;
                rectList[index].RectWidth = width;
                rectList[index].RectHeight = height;
            }

            return false;
        }

        public void updateRectThickness(int index, int size)
        {
            rectList[index].RectThickness = size;
        }

        public void addYoloItem(string item)
        {
            yoloLines.Add(item);
        }

        public void updateYoloItem(int index, string item)
        {
            yoloLines[index] = item;
        }

        public bool loadYoloBB(IList<labelClass> classNames, IList<string> lines, double frameWidth, double frameHeight)
        {
            
            if (lines.Count() > 0 && rectList.Count() == 0)
            {
                for (int i = 0; i < lines.Count(); i++)
                {
                    RectangleItem rect = new RectangleItem(null);
                    rect.loadBboxCorrdinate(frameWidth, frameHeight, haveYoloStyle(lines[i]), classNames);
                    rect.clearBorder();
                    rectList.Add(rect);
                    yoloLines.Add(lines[i]);
                }
                updateAllActualRectsize();
                loaded = true;
            }
            else
            {
                Debug.WriteLine("Error: Images Class : loadYoloBB: corrdinate list have no line.");
                return false;
            }
            return true;
        }

        private string[] haveYoloStyle(string item)
        {
            string[] coordinateStrings = item.Split(' ');
            if (coordinateStrings.Length != 5)
            {
                Debug.WriteLine("Info::: RectangleItem:: loadBboxCorrdinate: coordinate strings doesn't have expected format.");
                return null;
            }
            return coordinateStrings;
        }

        public void updateActualRectsize(int index)
        {
            if (index > -1 && rectList.Count > index)
            {
                var rect = rectList[index];
                rect.updateRectSizes(1 / heightRatio, 1 / widthRatio);

            }
        }

        public void saveRectanglesAsImage()
        {

        }

        private void updateAllActualRectsize()
        {
            if (rectList.Count() > 0)
            {
                foreach (var rect in rectList)
                {
                    rect.updateRectSizes(1/heightRatio, 1/widthRatio);
                }
            }
        }


        public Images(Windows.Storage.StorageFile file) {
            this.fileType = file.FileType;
            this.ImageLocation = file.Path;
            setImage(file);
        }
    }
}
