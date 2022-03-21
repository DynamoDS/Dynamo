using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Configuration
{
    public class GroupStyleItem: StyleItem
    {
        private bool isChecked = false;

        /// <summary>
        /// This property will say it we should display the checkmark in the MenuItem (appearing in the GroupStyles context menu)
        /// </summary>
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }
    }
}
