using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace Labeling
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private Control userInterfacce = null;
        private FileUtils fileUtils = new FileUtils();

        public Control UserInterface {
            get => userInterfacce;
            set {
                if (userInterfacce == value)
                    return;
                userInterfacce = value;
                RaisePropertyChanged();
            }
        }

        private ImageListView imageListView;
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        private async void folderIcon_Click(object sender, RoutedEventArgs e)
        {
            System.Collections.Generic.List<string> labels = new System.Collections.Generic.List<string>();
            System.Collections.Generic.List<Windows.Storage.StorageFile> files = new System.Collections.Generic.List<Windows.Storage.StorageFile>();
            if (await fileUtils.loadImageFiles(labels, files))
            {
                imageListView = new ImageListView(this, labels, fileUtils.Path);
                UserInterface = imageListView;

                await imageListView.addFilesAsync(files);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(e);
            Debug.WriteLine("");
        }
    }
}
