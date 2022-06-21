using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Labeling.ImageLabel;
// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace Labeling
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class ImageListView : UserControl
    {
        public ObservableCollection<Images> images = new ObservableCollection<Images>();
        private ObservableCollection<string> paths = new ObservableCollection<string>();
        private MainPage parent = null;
        private ImageListViewModel imageListViewModel = null;
        private labelClasses labelManagement = null;

        public ImageListView(MainPage parent, IList<string> labels, string labelPath)
        {
            InitializeComponent();
            ContentGridView.ItemsSource = images;
            this.labelManagement = new labelClasses(labels, labelPath);
            this.parent = parent;
            this.imageListViewModel = new ImageListViewModel(this); 
            this.DataContext = imageListViewModel;
        }

        public void addFiles(IReadOnlyList<Windows.Storage.StorageFile> files)
        {
            foreach (var file in files)
            {
                if (file.ContentType.Contains("image"))
                {
                    images.Add(new Images(file));
                }
            }
        }

        public async Task addFilesAsync(IList<Windows.Storage.StorageFile> files)
        {
            foreach (var file in files)
            {
                //BindingOperations.
                if (file.ContentType.Contains("image"))
                {
                    await Task.Delay(10);
                    images.Add(new Images(file));
                }
            }
        }

        public void transitToSelectedPage(int index)
        {
            if (parent != null)
                parent.UserInterface = new ImageLabel.ImageLabelView(images, index, this.labelManagement);
        }
    }
}
