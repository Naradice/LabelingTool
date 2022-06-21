using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;

namespace Labeling.ImageLabel
{
    class ImageLabelViewModel : BaseModel
    {
        private ImageView imageView = null;
        public ObservableCollection<labelClass> Classes;
        private labelClasses labelManagement = null;
        public ImageView ImageView
        {
            get => imageView;
            set
            {
                if (imageView == value)
                    return;
                imageView = value;
                RaisePropertyChanged();
            }
        }

        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (value == -1)
                {
                    SelectedIndex = selectedIndex;
                    return;
                }
                selectedIndex = value;
                if (selectedIndex > -1 && selectedIndex < Classes.Count)
                    ImageView.changeBorderColor(Classes[selectedIndex]);
                RaisePropertyChanged();
            }
        }

        private double sideBarWidth = 360.0;
        public double SideBarWidth
        {
            get => sideBarWidth;
            set
            {
                if (value == sideBarWidth)
                    return;
                sideBarWidth = value;
                RaisePropertyChanged();
            }
        }

        private bool isTextReadOnly = false;
        public bool IsTextReadOnly
        {
            get => isTextReadOnly;
            set
            {
                if (value == isTextReadOnly)
                    return;
                isTextReadOnly = value;
                RaisePropertyChanged();
            }
        }

        public ImageLabelViewModel(IEnumerable<Images> files, int index, labelClasses labelManagement)
        {
            this.labelManagement = labelManagement;
            Classes = labelManagement.classNames;
            ImageView = new ImageView(files, index, this.labelManagement);
            ImageView.LabelStateChanged += e_labelStateChanged;
        }

        public void addNewClass()
        {
            labelManagement.addNewClass("class name");
        }

        public void updateCurrentClass(string className)
        {
            if(selectedIndex > -1)
                labelManagement.updateClassName(SelectedIndex, className);
        }

        public bool checkLabelNameDuplication(string newLabelName)
        {
            return labelManagement.checkLabelNameDuplication(selectedIndex, newLabelName);
        }

        public SolidColorBrush getColor(int index)
        {
            return labelManagement.getColor(index);

        }

        public labelClass getlabelClass(int index)
        {
            return labelManagement.getClassOfIndex(index);
        }

        public void changeColor2Gray()
        {
            imageView.changeColor2Gray();
        }

        private void e_labelStateChanged(object sender, LabbeledEventArgs e)
        {
            //addNewClass();
        }
    }
}
