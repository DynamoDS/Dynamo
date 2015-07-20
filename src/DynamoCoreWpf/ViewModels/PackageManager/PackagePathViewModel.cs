using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Dynamo.Interfaces;
using DelegateCommand = Dynamo.UI.Commands.DelegateCommand;


namespace Dynamo.ViewModels
{
    class PackagePathViewModel : ViewModelBase
    {
        public ObservableCollection<string> RootLocations { get; private set; }
        private int selectedIndex;
        public int SelectedIndex {
            get { return selectedIndex; }
            set 
            {
                selectedIndex = value;
                if (selectedIndex < RootLocations.Count - 1)
                    MovePathDownCommand.RaiseCanExecuteChanged();
                if (selectedIndex > 0)
                    MovePathUpCommand.RaiseCanExecuteChanged();
            }
        }

        private readonly IPreferences setting;


        public DelegateCommand AddPathCommand { get; private set; }
        public DelegateCommand DeletePathCommand { get; private set; }
        public DelegateCommand MovePathUpCommand { get; private set; }
        public DelegateCommand MovePathDownCommand { get; private set; }
        public DelegateCommand UpdatePathCommand { get; private set; }
        public DelegateCommand SaveSettingCommand { get; private set; }

        public PackagePathViewModel(IPreferences setting)
        {
            RootLocations = new ObservableCollection<string>(setting.CustomPackageFolders);
            SelectedIndex = 0;
            this.setting = setting;

            AddPathCommand = new DelegateCommand(p => InsertPathAt(p as string, RootLocations.Count));
            DeletePathCommand = new DelegateCommand(p => RemovePathAt(SelectedIndex), CanDelete);
            MovePathUpCommand = new DelegateCommand(p => SwapPath(SelectedIndex, SelectedIndex - 1), CanMoveUp);
            MovePathDownCommand = new DelegateCommand(p => SwapPath(SelectedIndex, SelectedIndex + 1), CanMoveDown);
            UpdatePathCommand = new DelegateCommand(p => UpdatePathAt(p as string, SelectedIndex));
            SaveSettingCommand = new DelegateCommand(CommitChanges);
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

        private void InsertPathAt(string path, int index)
        {
            RootLocations.Insert(index, path);

            DeletePathCommand.RaiseCanExecuteChanged();
            if (index <= SelectedIndex)
                SelectedIndex++;
        }

        private void UpdatePathAt(string path, int index)
        {
            RootLocations[index] = path;
        }

        private void RemovePathAt(int index)
        {
            RootLocations.RemoveAt(index);

            if (index <= SelectedIndex && SelectedIndex > 0)
                SelectedIndex--;
        }

        private void CommitChanges(object param)
        {
            setting.CustomPackageFolders = new List<string>(RootLocations);
        }

    }
}
