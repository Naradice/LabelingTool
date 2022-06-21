using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

////Ideas
//show list of classes
//conver cordinate
//can click a rectangle
//Cancel rectangl when right clicked
////Done
//be able to use flipview
//disable outside
namespace Labeling.ImageLabel
{
    public partial class ImageView : UserControl
    {
        private ImageViewModel model;
        private Image image = null;
        private FlipView flipView = null;
        private double SIDE_BAR_WIDTH = 80;
        private double TOP_BAR_HEIGHT = 45;
        //Flags
        private bool isLabeling = true;
        private bool isDeleting = false;
        private bool isMoving = false;
        private bool isUpdating = false;
        private int updatingIndex = -1;
        //
        private RectPosition updatingPosition = RectPosition.None;
        private RectangleItem current_rectangle = null;
        private labelClass current_label = null;
        private double current_left = 0;
        private double current_top = 0;
        private double current_right = 0;
        private double current_bottom = 0;
        private Canvas imageCanvas = null;
        private Windows.UI.Core.CoreCursor currentPointer = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Cross, 1);
        //private ObservableCollection<Windows.UI.Xaml.Shapes.Rectangle> labelBox = new ObservableCollection<Windows.UI.Xaml.Shapes.Rectangle>();
        public labelClass label = null;

        public ImageView(IEnumerable<Images> images, int index, labelClasses labelManagement)
        {
            this.Loaded += delegate { this.Focus(FocusState.Programmatic); };
            this.InitializeComponent();
            model = new ImageViewModel(images, index, labelManagement);
            image = FindName("ImageFrame") as Image;
            flipView = FindName("itemFlipView") as FlipView;
            flipView.Focus(FocusState.Programmatic);
            this.current_label = labelManagement.classNames[0];
            this.imageCanvas = this.FindName("ImageCanvas") as Canvas;
            this.DataContext = model;
        }

        public void toggleLabelingState()
        {
            if (!isLabeling)
            {
                currentPointer = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Cross, 1);
                isLabeling = true;
                isDeleting = false;
                isUpdating = false;
            }
        }

        public void toggleDeleteState()
        {
            if(!isDeleting)
            {
                currentPointer = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.UniversalNo, 1);
                isLabeling = false;
                isDeleting = true;
                isUpdating = false;
            }
        }

        public void toggleUpdateState()
        {
            if (!isUpdating)
            {
                currentPointer = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
                isLabeling = false;
                isDeleting = false;
                isUpdating = true;
            }
        }

        private void itemFlipView_Loaded(object sender, RoutedEventArgs e)
        {
            FlipView flipview = sender as FlipView;
            model.setFrameSize(flipview.ActualWidth, flipview.ActualHeight);
        }

        private void itemFlipView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FlipView flipview = sender as FlipView;
            model.setFrameSize(flipview.ActualWidth, flipview.ActualHeight);
        }

        public void changeBorderColor(labelClass label)
        {
            current_label = label;
            if (current_rectangle != null)
                current_rectangle.changeClass(label);
        }

        public void changeTransparent(SolidColorBrush targetColor = null, bool beTransparent = true)
        {
            if(targetColor != null)
                if (beTransparent)
                    model.toBeTranparent(targetColor);
                else
                    model.toBeVisible(targetColor);
        }

        public void changeColor2Gray() {
            model.changeColor2Gray();
        }

        private void Border_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (flipView != null)
            {
                var point = e.GetCurrentPoint(image);
                var current_image = model.getCurrentImage();
                var x = point.Position.X - (flipView.ActualWidth - current_image.ImageWidth) / 2 - SIDE_BAR_WIDTH;
                var y = point.Position.Y - (flipView.ActualHeight - current_image.ImageHeight) / 2 - TOP_BAR_HEIGHT;
                model.X = x;
                model.Y = y;
                if (isLabeling && isMoving)
                {
                    var width = x - current_left;
                    var height = y - current_top;
                    if (current_right > x)
                    {
                        current_left = x;
                        width = current_right - current_left;
                        //Canvas.SetLeft(current_rectangle, current_left);
                    }
                    if (current_bottom > y)
                    {
                        current_top = y;
                        height = current_bottom - current_top;
                        //Canvas.SetTop(current_rectangle, current_top);
                    }
                    model.setCurrentRectSize(current_top, current_left, width, height);
                }
                else if (isUpdating && isMoving)
                {
                    model.updateRectOfIndex(updatingIndex, updatingPosition, model.X, model.Y);
                }
            }
            else
            {
                flipView = FindName("itemFlipView") as FlipView;
            }
        }

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = currentPointer;
        }

        private void Border_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        }

        private void Border_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            Debug.WriteLine(e.Key);
        }

        private async void ImageBorder_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (isLabeling && current_rectangle != null && isMoving)
            {
                isMoving = false;
                model.fixCurrentRect();
                await model.saveBoundingBox();
                current_rectangle.DashArray = null;
                current_rectangle = null;
                current_top = 0;
                current_left = 0;
                current_right = 0;
                current_bottom = 0;
                LabbeledEventArgs args = new LabbeledEventArgs();
                args.created = true;
                OnLabelStateChanged(args);
            }
            else if(isUpdating)
            {
                isMoving = false;
                if (updatingIndex != -1)
                {
                    model.updateRectThicknessOfIndex(updatingIndex, 1);
                    model.updateActualSizesOfIndex(updatingIndex);
                    await model.updateBoundingBox(updatingIndex);
                }
                updatingIndex = -1;
            }
        }

        private void itemFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //model.clipImages();
        }

        private void ImageCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (isLabeling && !isMoving)
            {
                var current_image = model.getCurrentImage();
                isMoving = true;

                current_rectangle = new RectangleItem(current_label);
                model.addNewRect(current_rectangle);
                var point = e.GetCurrentPoint(imageCanvas);
                current_top = point.Position.Y - (flipView.ActualHeight - current_image.ImageHeight) / 2 - TOP_BAR_HEIGHT;
                current_bottom = current_top;
                current_left = point.Position.X - (flipView.ActualWidth - current_image.ImageWidth) / 2 - SIDE_BAR_WIDTH;
                current_right = current_left;
                model.setCurrentRectSize(current_top, current_left, 10, 10);
            }

        }

        protected virtual void OnLabelStateChanged(LabbeledEventArgs e)
        {
            EventHandler<LabbeledEventArgs> handler = LabelStateChanged;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<LabbeledEventArgs> LabelStateChanged;

        private void LabelBox_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!isMoving && !isLabeling)
            {
                Rectangle rectangle = sender as Rectangle;
                model.updateRectThickness(rectangle, 10);
            }
        }

        private void LabelBox_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!isMoving)
            {
                Rectangle rectangle = sender as Rectangle;
                model.updateRectThickness(rectangle, 1);
            }
        }

        private void LabelBox_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Rectangle rectangle;
            if (isDeleting)
            {
                rectangle = sender as Rectangle;
                model.deleteRect(rectangle);
            }
            if (isUpdating)
            {
                rectangle = sender as Rectangle;
                isMoving = true;
                updatingIndex = model.getIndexOfRect(rectangle);
                updatingPosition = model.GetRectPosition(updatingIndex, model.X, model.Y);
            }
        }
    }

    public class LabbeledEventArgs : EventArgs
    {
        public bool created { get; set; }
    }
}