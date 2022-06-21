using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Xaml.Media;
using System.Linq;
using Windows.UI.ViewManagement;
using System.Diagnostics;

namespace Labeling.ImageLabel
{
    public class labelClasses
    {
        public ObservableCollection<labelClass> classNames = new ObservableCollection<labelClass>();
        private ColorManagement colorManager = new ColorManagement();
        private Windows.Storage.StorageFile file = null;
        private string pathFolder = null;
        private IList<string> labelList = new List<string>();

        /// <summary>
        /// Todo: move content to loadLabels
        /// </summary>
        /// <param name="labels"></param>
        /// <param name="labelFolderPath"></param>
        public labelClasses(IList<string> labels, string labelFolderPath)
        {
            this.pathFolder = labelFolderPath;

            if (labels == null || labels.Count == 0)
            {
                addNewClass("label name");
            }
            else
            {
                load(labels);
            }
        }

        public async void addNewClass(string name)
        {
            if(file == null && pathFolder != null)
            {
                Windows.Storage.StorageFolder labelFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(pathFolder);
                this.file = await labelFolder.CreateFileAsync("label.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
            }

            int currentIndex = classNames.Count();
            int index = classNames.Count();
            classNames.Add(new labelClass(name, new SolidColorBrush(colorManager.popColor()), index));
            labelList.Add(name);
            await Windows.Storage.FileIO.WriteLinesAsync(this.file, labelList);

        }

        private async void load(IList<string> labels)
        {
            if (file == null && pathFolder != null)
            {
                Windows.Storage.StorageFolder labelFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(pathFolder);
                this.file = await labelFolder.CreateFileAsync("label.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
            }
            int index = 0;
            foreach (var label in labels)
            {
                classNames.Add(new labelClass(label, new SolidColorBrush(colorManager.popColor()), index));
                index++;
            }
            labelList = labels;
            await Windows.Storage.FileIO.WriteLinesAsync(this.file, labelList);
        }

        public async void updateClassName(int index, string name)
        {
            if (classNames.Count() > index)
            {
                classNames[index] = new labelClass(name, classNames[index].ClassColor, index);
                labelList[index] = name;
                await Windows.Storage.FileIO.WriteLinesAsync(this.file, labelList);
            }


        }

        public bool checkLabelNameDuplication(int index, string labelName)
        {
            labelClass l_class = null;
            for (int count = 0; count < classNames.Count(); count++)
            {
                    l_class = classNames[count];
                    if (count != index && l_class.ClassName == labelName)
                        return false;
            }
            return true;
        }

        public SolidColorBrush getColor(int index) {
            if(isIndexValid(index))
                return classNames[index].ClassColor;
            return null;
        }

        public labelClass getClassOfIndex(int index) {
            if (isIndexValid(index))
                return classNames[index];
            return null;
        }

        private bool isIndexValid(int index)
        {
            return index > -1 && index < classNames.Count;
        }
    }

    public class labelClass: BaseModel
    {
        private string className = "";
        public string ClassName
        {
            get => className;
            set
            {
                if (value == className)
                    return;
                className = value;
            }
        }
        private SolidColorBrush classColor = new SolidColorBrush(Colors.White);
        public SolidColorBrush ClassColor {
            get => classColor;
            set
            {
                if (value == classColor)
                    return;
                classColor = value;
                RaisePropertyChanged();
            }
        }

        private bool visibility = true;
        public bool Visibility
        {
            get => visibility;
            set
            {
                if (visibility == value)
                    return;
                visibility = value;
                RaisePropertyChanged();
            }
        }

        public int index = 0;

        public labelClass(string name, SolidColorBrush cl, int index) {
            ClassName = name;
            ClassColor = cl;
            this.index = index;
        }
    }

    class ColorManagement
    {
        private List<IColors> c = new List<IColors>();
        private int index = 0;
        public ColorManagement()
        {
            c.Add(new monocroColors());
            c.Add(new blueColors());
            c.Add(new redColors());
            c.Add(new greenColors());
        }
        public Color popColor()
        {
            int kind_color_index = index % 4;
            IColors kind_color = c[kind_color_index];
            Color color = kind_color.pop();
            index++;
            return color;
        }
    }

    interface IColors
    {
        Color pop();
    }

    class monocroColors : IColors
    {
        private List<Color> colors = new List<Color>() { new Color { R = 255, G = 255, B = 255, A = 255 }, new Color { R = 200, G = 200, B = 200, A = 255 }, new Color { R = 150, G = 150, B = 150, A = 255 }, new Color { R = 100, G = 100, B = 100, A = 255 }, new Color { R = 50, G = 50, B = 50, A = 255 } };
        private int index = 0;
        public Color pop()
        {
            Color color;
            if (index < colors.Count())
            {
                color = colors[index];
                index++;
            }
            else
            {
                color = new Color { R = 255, G = 255, B = 255, A = 255 };
            }
            return color;
        }
    }
    class blueColors:IColors
    {
        private List<Color> colors = new List<Color>() { new Color { R=0, G=56, B=224, A=255}, new Color { R=65, G=102, B=225, A=255}, new Color {R=90, G=123, B=224, A = 255 }, new Color { R = 135, G = 157, B = 224, A = 255 }, new Color {R=180, G=191, B=224, A = 255 } };
        private int index = 0;
        public Color pop()
        {
            Color color;
            if (index < colors.Count())
            {
                 color = colors[index];
                index++;
            }
            else
            {
                color = new Color { R = 255, G = 255, B = 255, A = 255 };
            }
            return color;
        }
    }

    class redColors:IColors
    {
        private List<Color> colors = new List<Color>() { new Color { R = 224, G = 0, B = 56, A = 255 }, new Color { R = 224, G = 65, B =105, A = 255 }, new Color { R = 224, G = 90, B = 123, A = 255 }, new Color { R = 224, G = 135, B = 157, A = 255 }, new Color { R = 224, G = 180, B = 191, A = 255 } };
        private int index = 0;
        public Color pop()
        {
            Color color;
            if (index < colors.Count())
            {
                color = colors[index];
                index++;
            }
            else
            {
                color = new Color { R = 255, G = 255, B = 255, A = 255 };
            }
            return color;
        }
    }

    class greenColors: IColors
    {
        private List<Color> colors = new List<Color>() { new Color { R = 56, G = 224, B = 0, A = 255 }, new Color { R = 105, G = 224 , B = 65, A = 255 }, new Color { R = 123 , G = 224, B = 90, A = 255 }, new Color { R = 157, G = 224, B = 135, A = 255 }, new Color { R = 191, G = 224, B = 180, A = 255 } };
        private int index = 0;
        public Color pop()
        {
            Color color;
            if (index < colors.Count())
            {
                color = colors[index];
                index++;
            }
            else
            {
                color = new Color { R = 255, G = 255, B = 255, A = 255 };
            }
            return color;
        }
    }
}
