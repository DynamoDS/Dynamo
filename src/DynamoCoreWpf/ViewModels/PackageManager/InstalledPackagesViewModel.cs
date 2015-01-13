using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Dynamo.PackageManager;

using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.ViewModels
{
    public class InstalledPackagesViewModel : NotificationObject
    {
        private ObservableCollection<PackageViewModel> _localPackages = new ObservableCollection<PackageViewModel>();
        public ObservableCollection<PackageViewModel> LocalPackages { get { return _localPackages; } }

        private readonly DynamoViewModel dynamoViewModel;
        public PackageLoader Model { get; private set; }

        public InstalledPackagesViewModel(DynamoViewModel dynamoViewModel, PackageLoader model)
        {
            this.Model = model;
            this.dynamoViewModel = dynamoViewModel;
            
            InitializeLocalPackages();
        }

        internal void GoToWebsite()
        {
            dynamoViewModel.PackageManagerClientViewModel.GoToWebsite();
        }

        private void InitializeLocalPackages()
        {
            foreach (var pkg in Model.LocalPackages)
            {
                LocalPackages.Add(new PackageViewModel(this.dynamoViewModel, pkg));
            }


            this.Model.LocalPackages.CollectionChanged += LocalPackagesOnCollectionChanged;
        }

        private void LocalPackagesOnCollectionChanged(object sender, 
            NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        LocalPackages.Add(new PackageViewModel(dynamoViewModel, item as Package));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        LocalPackages.Remove(_localPackages.ToList().First(x => x.Model == item));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    LocalPackages.Clear();
                    break;
            }

            RaisePropertyChanged("LocalPackages");
        }
    }
}
