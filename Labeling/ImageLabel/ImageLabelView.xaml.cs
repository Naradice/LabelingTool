using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// ユーザー コントロールの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=234236 を参照してください

namespace Labeling.ImageLabel
{
    public sealed partial class ImageLabelView : UserControl
    {
        private ImageLabelViewModel imageLabelViewModel = null;

        public ImageLabelView(IEnumerable<Images> files, int index, labelClasses labelManagement)
        {
            this.InitializeComponent();
            imageLabelViewModel = new ImageLabelViewModel(files, index, labelManagement);
            labelButton.IsChecked = true;
            this.DataContext = imageLabelViewModel;
        }

        private void add_button_Click(object sender, RoutedEventArgs e)
        {
            imageLabelViewModel.addNewClass();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if(!imageLabelViewModel.checkLabelNameDuplication(textbox.Text))
                Debug.WriteLine("name is duplicated");
            imageLabelViewModel.updateCurrentClass(textbox.Text);
        }

        private void visibility_button_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            DependencyObject parent = VisualTreeHelper.GetParent(button);
            if (parent != null)
                while (!(parent is ListViewItem))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
            ListViewItem myListBoxItem = parent as ListViewItem;
            myListBoxItem.IsSelected = true;
            int selected = imageLabelViewModel.SelectedIndex;
            SolidColorBrush targetColor =  imageLabelViewModel.getColor(selected);
            imageLabelViewModel.ImageView.changeTransparent(targetColor, !(bool)button.IsChecked);
        }

        private void classList_Loaded(object sender, RoutedEventArgs e)
        {
            Grid grid = FindName("sidebar") as Grid;
            imageLabelViewModel.SideBarWidth = grid.ActualWidth;
        }

        private void classList_DropDownOpened(object sender, object e)
        {
            imageLabelViewModel.IsTextReadOnly = true;
        }

        private void classList_DropDownClosed(object sender, object e)
        {
            imageLabelViewModel.IsTextReadOnly = false;
        }

        private void label_button_Click(object sender, RoutedEventArgs e)
        {
            deleteButton.IsChecked = false;
            updateButton.IsChecked = false;
            imageLabelViewModel.ImageView.toggleLabelingState();
            
        }

        private void label_delete_button_Click(object sender, RoutedEventArgs e)
        {
            labelButton.IsChecked = false;
            updateButton.IsChecked = false;
            imageLabelViewModel.ImageView.toggleDeleteState();
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            deleteButton.IsChecked = false;
            labelButton.IsChecked = false;
            imageLabelViewModel.ImageView.toggleUpdateState();
        }

        private void detectButton_Click(object sender, RoutedEventArgs e)
        {
            //imageLabelViewModel
        }

        private void binaryButton_Click(object sender, RoutedEventArgs e)
        {
            //imageLabelViewModel.changeColor2Gray();
        }
    }
}
