using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.Graphics.Imaging;

namespace Labeling.ImageLabel
{
    class ImageViewModel:BaseModel
    {
        private ObservableCollection<Images> imageList = null;
        private double frameWidth = 0;
        private double frameHeight = 0;

        private string labelFolderName = "label";
        private string classFileName = "classes.txt";

        private labelClasses labelManagement = null;
        private OpenCV operation = new OpenCV();
        private FileUtils fileUtils = new FileUtils();
        private SoftwareBitmap originalBitmap = null;
        private SoftwareBitmap outputBitmap = null;


        public ObservableCollection<Images> ImageList
        {
            get => imageList;
            set
            {
                if (imageList == value)
                    return;
                imageList = value;
                RaisePropertyChanged();
            }
        }

        private int previouseIndex = -1;
        private int selectedIndex = -1;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value)
                    return;
                previouseIndex = selectedIndex;
                selectedIndex = value;
                // avoid to operate when screen is loaded
                if (originalBitmap != null)
                {
                    var temp = new SoftwareBitmap(BitmapPixelFormat.Bgra8, originalBitmap.PixelWidth, originalBitmap.PixelHeight, BitmapAlphaMode.Premultiplied);
                    originalBitmap.CopyTo(temp);
                    new Task(async () => { await clipImagesAsync(previouseIndex, temp); }).Start();
                }
                setImageSize();
                initalizeSoftwareBitmap();
                loadYoloBB(SelectedIndex).ConfigureAwait(false);
                RaisePropertyChanged();

            }
        }

        private double imageFrameWidth = 0;
        public double ImageFrameWidth
        {
            get => imageFrameWidth;
            set
            {
                if (imageFrameWidth == value)
                    return;
                imageFrameWidth = value;
                RaisePropertyChanged();
            }
        }

        private double imageFrameHeight = 0;
        public double ImageFrameHeight
        {
            get => imageFrameHeight;
            set
            {
                if (imageFrameHeight == value)
                    return;
                imageFrameHeight = value;
                RaisePropertyChanged();
            }
        }

        private double x;
        public double X
        {
            get => x;
            set
            {
                if (x == value)
                    return;
                x = value;
                RaisePropertyChanged();
            }
        }

        private double y;
        public double Y
        {
            get => y;
            set
            {
                if (y == value)
                    return;
                y = value;
                RaisePropertyChanged();
            }
        }

        public ImageViewModel(IEnumerable<Images> images, int index, labelClasses labelManagement)
        {
            this.ImageList = (ObservableCollection<Images>)images;
            this.labelManagement = labelManagement;
            CreateLabelFolderAsync(index).ConfigureAwait(false);
            SelectedIndex = index;
        }

        public void setFrameSize(double width, double height)
        {
            this.frameWidth = width;
            this.frameHeight = height;
            setImageSize();
        } 

        public bool setImageSize()
        {
            if(this.frameWidth > 0 && this.frameHeight > 0)
            {
                this.imageList[this.selectedIndex].setImageSize(this.frameWidth, this.frameHeight);
                this.imageList[this.selectedIndex].updateAllRectsize();

                return true;
            }
            return false;
        }

        public Images getCurrentImage()
        {
            return this.imageList[this.selectedIndex];
        }

        public RectangleItem getCurrentRect()
        {
            return this.imageList[this.selectedIndex].RectList.Last();
        }

        public void setCurrentRectSize(double top, double left, double width, double height)
        {
            this.imageList[this.selectedIndex].setCurrentRectSize(top, left, width, height);
        }

        public void addNewRect(RectangleItem rect)
        {
            this.imageList[this.selectedIndex].RectList.Add(rect);
        }

        public bool deleteRect(Windows.UI.Xaml.Shapes.Rectangle rect)
        {
            Images currentIamge = this.imageList[this.selectedIndex];
            int index = currentIamge.getIndexOfRectangle(rect);
            if (currentIamge.removeRectItemAt(index))
            {
                saveAsYolo(selectedIndex);
                return true;
            }
            return false;
        }

        public async Task<bool> deleteRectAsync(Windows.UI.Xaml.Shapes.Rectangle rect)
        {
            Images currentIamge = this.imageList[this.selectedIndex];
            int index = currentIamge.getIndexOfRectangle(rect);
            if (currentIamge.removeRectItemAt(index))
            {
                await saveAsYolo(selectedIndex);
                return true;
            }
            return false;
        }

        public void updateRectThickness(Windows.UI.Xaml.Shapes.Rectangle rect, int size)
        {
            int index = getIndexOfRect(rect);
            updateRectThicknessOfIndex(index, size);
        }

        public void updateRectThicknessOfIndex(int index, int size)
        {
            if(size > 0)
            {
                Images currentIamge = this.imageList[this.selectedIndex];
                if (index > -1 && currentIamge.RectList.Count > index)
                {
                    currentIamge.updateRectThickness(index, size);
                }
            }
        }

        public int getIndexOfRect(Windows.UI.Xaml.Shapes.Rectangle rect)
        {
            Images currentIamge = this.imageList[this.selectedIndex];
            return currentIamge.getIndexOfRectangle(rect);
        }
        public RectPosition GetRectPosition(int index, double X, double Y)
        {
            Images currentIamge = this.imageList[this.selectedIndex];
            return currentIamge.getRectPosition(index, X, Y);
        }

        public bool updateRect(Windows.UI.Xaml.Shapes.Rectangle rect, RectPosition clickedPosition, double X, double Y)
        {
            Images currentIamge = this.imageList[this.selectedIndex];
            int index = currentIamge.getIndexOfRectangle(rect);
            return currentIamge.updateRectItem(index, clickedPosition, X, Y);
        }

        public bool updateRectOfIndex(int index, RectPosition clickedPosition, double X, double Y)
        {
            Images currentIamge = this.imageList[this.selectedIndex];
            return currentIamge.updateRectItem(index, clickedPosition, X, Y);
        }

        /// <summary>
        /// change border dash to solid
        /// </summary>
        public void fixCurrentRect()
        {
            var current_rect = this.imageList[this.selectedIndex].RectList.Last();
            current_rect.DashArray = null;
        }

        public void toBeTranparent(SolidColorBrush targetColor)
        {
            foreach(RectangleItem item in this.imageList[this.selectedIndex].RectList)
            {
                if(item.Color == targetColor)
                {
                    item.RectThickness = 0.0;
                }
            }
        }

        public void toBeVisible(SolidColorBrush targetColor)
        {
            foreach (RectangleItem item in this.imageList[this.selectedIndex].RectList)
            {
                if (item.Color == targetColor)
                {
                    item.RectThickness = 1.0;
                }
            }
        }

        public async Task<bool> saveBoundingBox() {
            int rectIndex = imageList[selectedIndex].yoloLines.Count();
            int labelIndex = imageList[selectedIndex].RectList[rectIndex].label.index;
            int imageIndex = selectedIndex;
            double imageWidth = imageList[selectedIndex].ImageWidth;
            double imageHeight = imageList[selectedIndex].ImageHeight;
            imageList[imageIndex].addYoloItem($"{labelIndex} {imageList[selectedIndex].RectList[rectIndex].bboxCorrdinate(imageWidth, imageHeight)}");
            return await saveAsYolo(imageIndex);
        }

        public async Task<bool> updateBoundingBox(int rectIndex)
        {
            int labelIndex = imageList[selectedIndex].RectList[rectIndex].label.index;
            int imageIndex = selectedIndex;
            double imageWidth = imageList[selectedIndex].ImageWidth;
            double imageHeight = imageList[selectedIndex].ImageHeight;
            imageList[imageIndex].updateYoloItem(rectIndex, $"{labelIndex} {imageList[selectedIndex].RectList[rectIndex].bboxCorrdinate(imageWidth, imageHeight)}");
            return await saveAsYolo(imageIndex);
        }

        public void updateActualSizesOfIndex(int rectIndex)
        {
            imageList[selectedIndex].updateActualRectsize(rectIndex);
        }

        public async Task<bool> clipImagesAsync(int _selectedIndex_ , SoftwareBitmap imageSoftwareBitmap)
        {
            if (_selectedIndex_ > -1)
            {
                for (int i = 0; i < imageList[_selectedIndex_].RectList.Count; i++)
                {
                    await fileUtils.saveImage(
                        operation.getClippedSoftwareBitmap(imageSoftwareBitmap, imageList[_selectedIndex_].RectList[i]),
                        imageList[_selectedIndex_].encoderId,
                        imageList[_selectedIndex_].ImageLocation,
                        i.ToString()
                        );
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public void changeColor2Gray()
        {
            if(operation != null)
            {
                outputBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, originalBitmap.PixelWidth, originalBitmap.PixelHeight, BitmapAlphaMode.Premultiplied);
                operation.ChangeColor2Gray(originalBitmap, outputBitmap);
                imageList[selectedIndex].setImage(outputBitmap);
            }
        }

        private void initalizeSoftwareBitmap()
        {
            SoftwareBitmap inputBitmap = imageList[selectedIndex].softwareBitmap;
            if(inputBitmap != null) {
                originalBitmap = SoftwareBitmap.Convert(inputBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                outputBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, originalBitmap.PixelWidth, originalBitmap.PixelHeight, BitmapAlphaMode.Premultiplied);
            }
        }

        private async Task<bool> CreateLabelFolderAsync(int imageIndex)
        {
            string path = "";
            try
            {
                path = System.IO.Path.GetDirectoryName(imageList[imageIndex].ImageLocation);
                Windows.Storage.StorageFolder storageFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(path);
                await storageFolder.CreateFolderAsync(labelFolderName, Windows.Storage.CreationCollisionOption.OpenIfExists);
            } catch (Exception e){
                Debug.WriteLine($"Error: ImageViewModel : CreateLabelFolderAsync : couldn't create {path}.: {e.Message}");
                return false; 
            }
            return true;

        }

        private async Task<Windows.Storage.StorageFile> getLabelFilePath(int imageIndex) {
            string path = "";
            string filename = "";
            try
            {
                path = System.IO.Path.GetDirectoryName(imageList[imageIndex].ImageLocation);
                path = System.IO.Path.Combine(path, labelFolderName);
                Windows.Storage.StorageFolder storageFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(path);
                filename = imageList[imageIndex].ImageLocation.ToLower().Replace(imageList[imageIndex].fileType.ToLower(), ".txt").Split('\\').Last();
                return await storageFolder.CreateFileAsync(filename, Windows.Storage.CreationCollisionOption.OpenIfExists);
            }
            catch
            {
                Debug.WriteLine($"Error::: ImageViewModel:: cannot open the file {filename} of {path} in getLabelFilePath");
                return null;
            }

        }
        private async Task<bool> saveAsYolo(int imageIndex)
        {

            Windows.Storage.StorageFile bbfile = await getLabelFilePath(imageIndex);
            try
            {
                if (bbfile != null)
                {
                    await Windows.Storage.FileIO.WriteLinesAsync(bbfile, imageList[imageIndex].yoloLines);
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                Debug.WriteLine("Error::: ImageViewModel:: saveYolo");
                return false;
            }
            return true;
        }

        private async Task<bool> loadYoloBB(int imageIndex)
        {
            Windows.Storage.StorageFile bbfile = await getLabelFilePath(imageIndex);
            if (bbfile != null)
            {
                if (!imageList[imageIndex].loaded)
                {
                    IList<string> bblist = await Windows.Storage.FileIO.ReadLinesAsync(bbfile, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                    int fileLineCount = bblist.Count();
                    if (fileLineCount > 0)
                    {
                        double imageWidth = imageList[selectedIndex].ImageWidth;
                        double imageHeight = imageList[selectedIndex].ImageHeight;
                        if (!imageList[selectedIndex].loadYoloBB(labelManagement.classNames, bblist, imageWidth, imageHeight))
                        {
                            Debug.WriteLine("Error::: ImageViewModel:: loadYoloBB: Couldn't map coordinates to list.");
                            return false;
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Error::: ImageViewModel:: loadYoloBB: no lines in a corrdinate file.");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine("Info::: ImageViewModel:: loadYoloBB: already loaded.");
                    return false;
                }
                return true;
            }
            else
            {
                Debug.WriteLine("Info::: ImageViewModel:: loadYoloBB: no file exist.");
                return false;
            }
        }

        private async Task<bool> loadClassFile(int imageIndex)
        {
            string path = "";
            path = System.IO.Path.GetDirectoryName(imageList[imageIndex].ImageLocation);
            path += labelFolderName;
            try
            {
                Windows.Storage.StorageFolder storageFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(path);
                Windows.Storage.StorageFile classFile = await storageFolder.CreateFileAsync(classFileName, Windows.Storage.CreationCollisionOption.OpenIfExists);
                IList<string> bblist = await Windows.Storage.FileIO.ReadLinesAsync(classFile, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }catch(Exception e)
            {
                Debug.WriteLine($"Error: ImageViewModel: loadClassFile: {e.Message}");
            }
            return true;
        }
    }
}
