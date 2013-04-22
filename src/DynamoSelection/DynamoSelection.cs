using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Selection
{
    public class DynamoSelection : NotificationObject
    {
        private static DynamoSelection _instance;
        private ObservableCollection<ISelectable> selection = new ObservableCollection<ISelectable>();

        public static DynamoSelection Instance
        {
            get
            {
                if (_instance != null)
                {
                    _instance = new DynamoSelection();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Returns a collection of ISelectable elements.
        /// </summary>
        public ObservableCollection<ISelectable> Selection
        {
            get { return selection; }
            set
            {
                selection = value;
                RaisePropertyChanged("Selection");
            }
        }

        private DynamoSelection()
        {
            
        }
    }

    public interface ISelectable
    {
        void Select();
        void Deselect();
    }

    public static class Extensions
    {
        public static void RemoveAll(this ObservableCollection<ISelectable> list)
        {
            while (list.Count > 0)
            {
                list.RemoveAt(list.Count - 1);
            }
        }
    }
}
