using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labeling
{
    class ImageListViewModel: BaseModel
    {
        private ImageListView parent = null;

        public ImageListViewModel(ImageListView parent)
        {
            this.parent = parent;
        }
        private int selectedIndex = -1;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex == value)
                    return;
                selectedIndex = value;
                this.parent.transitToSelectedPage(selectedIndex);
            }
        }

    }
}
