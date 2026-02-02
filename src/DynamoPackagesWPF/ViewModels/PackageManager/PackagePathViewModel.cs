using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.PackageManager;
using Dynamo.Wpf.Properties;
using DelegateCommand = Dynamo.UI.Commands.DelegateCommand;
using Dynamo.Models;
using System.Windows.Data;
using System.Globalization;
using System.Linq;
using Dynamo.Configuration;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// A converter that returns true if a path is currently disabled as specified in the 
    /// PreferenceSettings. Value[0] should be the PackgagePathViewModel.
    /// Value[1] should be the path to check as a string.
    /// Returns false by default.
    /// </summary>
    public sealed class PathEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {   
            if(values != null && values.Length > 1)
            {
                if(values[0] is PackagePathViewModel vm && values[1] is string stringPath)
                {
                    return vm?.IsPathCurrentlyDisabled(stringPath);
                }
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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
       
        public event EventHandler<PackagePathEventArgs> RequestShowFileDialog;
        public virtual void OnRequestShowFileDialog(object sender, PackagePathEventArgs e)
        {
            if (RequestShowFileDialog != null)
            {
                RequestShowFileDialog(sender, e);
            }
        }

        /// <summary>
        /// Returns the preference settings.
        /// </summary>
        public IPreferences PreferenceSettings
        {
            get { return loadPackageParams.Preferences; }
        }
        internal readonly PackageLoader packageLoader;
        internal LoadPackageParams loadPackageParams;
        private readonly CustomNodeManager customNodeManager;
        private readonly List<string> packagePathsEnabled = new List<string>();

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
            InitializeRootLocations();
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            AddPathCommand = new DelegateCommand(p => InsertPath());
            DeletePathCommand = new DelegateCommand(p => RemovePathAt(ConvertPathToIndex(p)), p => CanDelete(ConvertPathToIndex(p)));
            MovePathUpCommand = new DelegateCommand(p => SwapPath(ConvertPathToIndex(p), ConvertPathToIndex(p) - 1), p => CanMoveUp(ConvertPathToIndex(p)));
            MovePathDownCommand = new DelegateCommand(p => SwapPath(ConvertPathToIndex(p), ConvertPathToIndex(p) + 1), p => CanMoveDown(ConvertPathToIndex(p)));
            UpdatePathCommand = new DelegateCommand(p => UpdatePathAt(ConvertPathToIndex(p)), p => CanUpdate(ConvertPathToIndex(p)));
            SaveSettingCommand = new DelegateCommand(CommitChanges);
        }

        /// <summary>
        /// This constructor overload has been added for backwards comptability.
        /// </summary>
        /// <param name="setting"></param>
        public PackagePathViewModel(IPreferences setting)
        {
            InitializeRootLocations();
            InitializeCommands();
        }

        private int ConvertPathToIndex(object path)
        {
            if(path is int pint)
            {
                return pint;
            }
            return RootLocations.IndexOf(path as string);
        }

        private void RaiseCanExecuteChanged()
        {
            MovePathDownCommand.RaiseCanExecuteChanged();
            MovePathUpCommand.RaiseCanExecuteChanged();
            AddPathCommand.RaiseCanExecuteChanged();
            DeletePathCommand.RaiseCanExecuteChanged();
            UpdatePathCommand.RaiseCanExecuteChanged();
        }

        private bool CanDelete(int param)
        {
            var programDataPackagePathIndex = GetIndexOfProgramDataPackagePath();
            var appDataPackagePathIndex = GetIndexOfDefaultAppDataPackagePath();
            if (RootLocations.IndexOf(Resources.PackagePathViewModel_BuiltInPackages) == param ||
                    programDataPackagePathIndex == param || appDataPackagePathIndex == param)
            {
                return false;
            }

                return RootLocations.Count > 1;
        }

        private bool CanMoveUp(int param)
        {
            return param > 0;
        }

        private bool CanMoveDown(int param)
        {
            return param < RootLocations.Count - 1;
        }

        private bool CanUpdate(int param)
        {
            var programDataPackagePathIndex = GetIndexOfProgramDataPackagePath();
            var appDataPackagePathIndex = GetIndexOfDefaultAppDataPackagePath();

            //editing builtin packages or programData package paths is not allowed.
            return RootLocations.IndexOf(Resources.PackagePathViewModel_BuiltInPackages) != param &&
                programDataPackagePathIndex != param && appDataPackagePathIndex != param;
        }

        private int GetIndexOfProgramDataPackagePath()
        {
            var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var programDataPackagePath = RootLocations.Where(x => x.StartsWith(programDataPath)).FirstOrDefault();
            var programDataPackagePathIndex = RootLocations.IndexOf(programDataPackagePath);
            return programDataPackagePathIndex;
        }

        private int GetIndexOfDefaultAppDataPackagePath()
        {
            var index = -1;
            if (PreferenceSettings is PreferenceSettings prefs)
            {
                var appDataPath = prefs.OnRequestUserDataFolder();
                index = RootLocations.IndexOf(appDataPath);
            }

            return index;
        }

        // The position of the selected entry must always be the first parameter.
        private void SwapPath(int x, int y)
        {
            if (x < 0 || y < 0 || x >= RootLocations.Count || y >= RootLocations.Count)
                return;

            var tempPath = RootLocations[x];
            RootLocations[x] = RootLocations[y];
            RootLocations[y] = tempPath;

            RaiseCanExecuteChanged();
        }

        private void InsertPath()
        {
            var args = new PackagePathEventArgs();

            ShowFileDialog(args);

            if (args.Cancel)
                return;

            RootLocations.Insert(RootLocations.Count, args.Path);
            SetPackagesScheduledState(args.Path, packagePathDisabled: false);
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
                Path = RootLocations[index]
            };

            ShowFileDialog(args);

            if (args.Cancel)
                return;

            RootLocations[index] = args.Path;
        }

        private void RemovePathAt(int index)
        {
            var pathToRemove = RootLocations[index];
            SetPackagesScheduledState(pathToRemove, packagePathDisabled: true);
            RootLocations.RemoveAt(index);
            RaiseCanExecuteChanged();
        }

        private void CommitChanges(object param)
        {
            var newpaths = CommitRootLocations();
            IEnumerable<string> additionalPaths = new List<string>();
            //if paths are modified, reload packages and update prefs.
            if (!PreferenceSettings.CustomPackageFolders.SequenceEqual(newpaths))
            {
                additionalPaths = newpaths.Except(PreferenceSettings.CustomPackageFolders);
                PreferenceSettings.CustomPackageFolders = newpaths;
                if (packageLoader != null)
                {
                    packageLoader.LoadNewCustomNodesAndPackages(additionalPaths, customNodeManager);
                }
            }
            // Load packages from paths enabled by disable-path toggles if they are not already loaded.
            if (packageLoader != null && packagePathsEnabled.Any())
            {
                var newPaths = packagePathsEnabled.Except(additionalPaths);
                packageLoader.LoadNewCustomNodesAndPackages(newPaths, customNodeManager);
            }
            packagePathsEnabled.Clear();
            // Dispose node Index writer to avoid file lock after new package path applied and packages loaded.
            Search.LuceneSearch.LuceneUtilityNodeSearch.DisposeWriter();
        }

        internal void SetPackagesScheduledState(string packagePath, bool packagePathDisabled)
        {
            var loadedPackages = packageLoader.LocalPackages.Where(x => x.LoadState.State == PackageLoadState.StateTypes.Loaded);
            var packagesInPath = loadedPackages.Where(x => x.RootDirectory.StartsWith(packagePath));

            // If there are no packages loaded from packagePath and the toggle is turned off
            // packages need to be loaded from packagePath once preferences dialog is closed.
            if (!packagesInPath.Any() && !packagePathDisabled)
            {
                packagePathsEnabled.Add(packagePath);
            }

            foreach (var pkg in packagesInPath)
            {
                if (packagePathDisabled)
                {
                    pkg.MarkForUnload();
                }
                else
                {
                    pkg.UnmarkForUnload();
                }
            }
        }

        internal void InitializeRootLocations()
        {
            RootLocations = new ObservableCollection<string>(PreferenceSettings.CustomPackageFolders);
            var index = RootLocations.IndexOf(DynamoModel.BuiltInPackagesToken);

            if (index != -1)
            {
                RootLocations[index] = Resources.PackagePathViewModel_BuiltInPackages;
            }
            RaisePropertyChanged(string.Empty);
        }

        private List<string> CommitRootLocations()
        {
            var rootLocations = new List<string>(RootLocations);
            var index = rootLocations.IndexOf(Resources.PackagePathViewModel_BuiltInPackages);

            if (index != -1)
            {
                rootLocations[index] = DynamoModel.BuiltInPackagesToken;
            }

            return rootLocations;
        }

        internal bool IsPathCurrentlyDisabled(string path)
        {
            if (!(PreferenceSettings is IDisablePackageLoadingPreferences disablePrefs))
            {
                return false;
            }
            //disabled if builtinpackages disabled and path is builtinpackages
            if ((disablePrefs.DisableBuiltinPackages && path == Resources.PackagePathViewModel_BuiltInPackages)
                //or if custompaths disabled and path is custom path
                || (disablePrefs.DisableCustomPackageLocations && PreferenceSettings.CustomPackageFolders.Contains(path))
                //or if custompaths disabled and path is known path that is not builtinpackages - needed because new paths that are not committed
                //will not be added to customPackagePaths yet.
                || (disablePrefs.DisableCustomPackageLocations && RootLocations.Contains(path) && path != Resources.PackagePathViewModel_BuiltInPackages))
            {
                return true;
            }

            return false;
        }


    }
}
