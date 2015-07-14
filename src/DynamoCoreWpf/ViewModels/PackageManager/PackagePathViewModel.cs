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
        public int SelectedIndex { get; set; }

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
            MovePathUpCommand = new DelegateCommand(p => SwapPath(SelectedIndex, SelectedIndex - 1));
            MovePathDownCommand = new DelegateCommand(p => SwapPath(SelectedIndex, SelectedIndex + 1));
            UpdatePathCommand = new DelegateCommand(p => UpdatePathAt(p as string, SelectedIndex));
            SaveSettingCommand = new DelegateCommand(CommitChanges, CanCommitChanges);
        }

        private bool CanDelete(object param)
        {
            return RootLocations.Count > 0;
        }

        private void SwapPath(int x, int y)
        {
            if (x < 0 || y < 0 || x >= RootLocations.Count || y >= RootLocations.Count)
                return;

            var tempPath = RootLocations[x];
            RootLocations[x] = RootLocations[y];
            RootLocations[y] = tempPath;
        }

        private void InsertPathAt(string path, int index)
        {
            RootLocations.Insert(index, path);
        }

        private void UpdatePathAt(string path, int index)
        {
            RootLocations[index] = path;
        }

        private void RemovePathAt(int index)
        {
            RootLocations.RemoveAt(index);
        }

        private void CommitChanges(object param)
        {
            setting.CustomPackageFolders = new List<string>(RootLocations);
        }

        private bool CanCommitChanges(object param)
        {
            return RootLocations.Count > 0;
        }
    }
}
