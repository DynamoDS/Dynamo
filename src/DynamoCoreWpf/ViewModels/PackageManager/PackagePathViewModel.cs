using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.PackageManager;
using DelegateCommand = Dynamo.UI.Commands.DelegateCommand;

namespace Dynamo.ViewModels
{
    public class PackagePathEventArgs : EventArgs
    {
        /// <summary>
        /// Indicate whether user wants to add the current path to the list
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Indicate the path to the custom packages folder
        /// </summary>
        public string Path { get; set; }
    }

    public class PackagePathViewModel : ViewModelBase
    {
        public ObservableCollection<string> RootLocations { get; private set; }
        private int selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
                RaiseCanExecuteChanged();
            }
        }

        public event EventHandler<PackagePathEventArgs> RequestShowFileDialog;
        public virtual void OnRequestShowFileDialog(object sender, PackagePathEventArgs e)
        {
            if (RequestShowFileDialog != null)
            {
                RequestShowFileDialog(sender, e);
            }
        }

        private IPreferences setting
        {
            get { return loadPackageParams.Preferences; }
        }
        private readonly PackageLoader packageLoader;
        private readonly LoadPackageParams loadPackageParams;
        private readonly CustomNodeManager customNodeManager;

        public DelegateCommand AddPathCommand { get; private set; }
        public DelegateCommand DeletePathCommand { get; private set; }
        public DelegateCommand MovePathUpCommand { get; private set; }
        public DelegateCommand MovePathDownCommand { get; private set; }
        public DelegateCommand UpdatePathCommand { get; private set; }
        public DelegateCommand SaveSettingCommand { get; private set; }

        public PackagePathViewModel(PackageLoader loader, LoadPackageParams loadParams, CustomNodeManager customNodeManager)
        {
            this.packageLoader = loader;
            this.loadPackageParams = loadParams;
            this.customNodeManager = customNodeManager;
            RootLocations = new ObservableCollection<string>(setting.CustomPackageFolders);

            AddPathCommand = new DelegateCommand(p => InsertPath());
            DeletePathCommand = new DelegateCommand(p => RemovePathAt((int) p), CanDelete);
            MovePathUpCommand = new DelegateCommand(p => SwapPath((int) p, ((int) p) - 1), CanMoveUp);
            MovePathDownCommand = new DelegateCommand(p => SwapPath((int) p, ((int) p) + 1), CanMoveDown);
            UpdatePathCommand = new DelegateCommand(p => UpdatePathAt((int) p));
            SaveSettingCommand = new DelegateCommand(CommitChanges);

            SelectedIndex = 0;
        }
        /// <summary>
        /// This constructor overload has been added for backwards comptability.
        /// </summary>
        /// <param name="setting"></param>
        public PackagePathViewModel(IPreferences setting)
        {

            RootLocations = new ObservableCollection<string>(setting.CustomPackageFolders);

            AddPathCommand = new DelegateCommand(p => InsertPath());
            DeletePathCommand = new DelegateCommand(p => RemovePathAt((int)p), CanDelete);
            MovePathUpCommand = new DelegateCommand(p => SwapPath((int)p, ((int)p) - 1), CanMoveUp);
            MovePathDownCommand = new DelegateCommand(p => SwapPath((int)p, ((int)p) + 1), CanMoveDown);
            UpdatePathCommand = new DelegateCommand(p => UpdatePathAt((int)p));
            SaveSettingCommand = new DelegateCommand(CommitChanges);

            SelectedIndex = 0;
        }


        private void RaiseCanExecuteChanged()
        {
            MovePathDownCommand.RaiseCanExecuteChanged();
            MovePathUpCommand.RaiseCanExecuteChanged();
            AddPathCommand.RaiseCanExecuteChanged();
            DeletePathCommand.RaiseCanExecuteChanged();
        }

        private bool CanDelete(object param)
        {
            return RootLocations.Count > 1;
        }

        private bool CanMoveUp(object param)
        {
            return SelectedIndex > 0;
        }

        private bool CanMoveDown(object param)
        {
            return SelectedIndex < RootLocations.Count - 1;
        }

        // The position of the selected entry must always be the first parameter.
        private void SwapPath(int x, int y)
        {
            if (x < 0 || y < 0 || x >= RootLocations.Count || y >= RootLocations.Count)
                return;

            var tempPath = RootLocations[x];
            RootLocations[x] = RootLocations[y];
            RootLocations[y] = tempPath;

            SelectedIndex = y;
        }

        private void InsertPath()
        {
            var args = new PackagePathEventArgs();

            ShowFileDialog(args);

            if (args.Cancel)
                return;

            RootLocations.Insert(RootLocations.Count, args.Path);

            RaiseCanExecuteChanged();
        }

        private void ShowFileDialog(PackagePathEventArgs e)
        {
            OnRequestShowFileDialog(this, e);

            if (e.Cancel == false && RootLocations.Contains(e.Path))
                e.Cancel = true;
        }

        private void UpdatePathAt(int index)
        {
            var args = new PackagePathEventArgs
            {
                Path = RootLocations[SelectedIndex]
            };

            ShowFileDialog(args);

            if (args.Cancel)
                return;

            RootLocations[index] = args.Path;
        }

        private void RemovePathAt(int index)
        {
            RootLocations.RemoveAt(index);

            if (index <= SelectedIndex && SelectedIndex > 0)
                SelectedIndex--;

            RaiseCanExecuteChanged();
        }

        private void CommitChanges(object param)
        {
            setting.CustomPackageFolders = new List<string>(RootLocations);
            if (this.packageLoader != null)
            {
                this.packageLoader.LoadCustomNodesAndPackages(loadPackageParams, customNodeManager);
            }
        }

    }
}
