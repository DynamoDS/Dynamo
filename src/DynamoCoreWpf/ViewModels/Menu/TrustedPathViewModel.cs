using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Wpf.Properties;
using DelegateCommand = Dynamo.UI.Commands.DelegateCommand;
using Dynamo.Models;
using System.Windows.Data;
using System.Globalization;
using System.Linq;
using Dynamo.Configuration;

namespace Dynamo.ViewModels
{

    public class TrustedPathEventArgs : EventArgs
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

    public class TrustedPathViewModel : ViewModelBase
    {
        public ObservableCollection<string> TrustedLocations { get; private set; }
       
        public event EventHandler<TrustedPathEventArgs> RequestShowFileDialog;
        public virtual void OnRequestShowFileDialog(object sender, TrustedPathEventArgs e)
        {
            if (RequestShowFileDialog != null)
            {
                RequestShowFileDialog(sender, e);
            }
        }

        public DelegateCommand AddPathCommand { get; private set; }
        public DelegateCommand DeletePathCommand { get; private set; }
        public DelegateCommand UpdatePathCommand { get; private set; }
        public DelegateCommand SaveSettingCommand { get; private set; }

        public TrustedPathViewModel()
        { 
            InitializeTrustedLocations();
            InitializeCommands();
        }

        /// <summary>
        /// This constructor overload has been added for backwards comptability.
        /// </summary>
        /// <param name="setting"></param>
        public TrustedPathViewModel(IPreferences setting)
        {
            InitializeTrustedLocations();
            InitializeCommands();
        }
        private void InitializeCommands() 
        {
            AddPathCommand = new DelegateCommand(p => InsertPath());
            DeletePathCommand = new DelegateCommand(p => RemovePathAt(ConvertPathToIndex(p)), p => CanDelete(ConvertPathToIndex(p)));
            UpdatePathCommand = new DelegateCommand(p => UpdatePathAt(ConvertPathToIndex(p)), p => CanUpdate(ConvertPathToIndex(p)));
            SaveSettingCommand = new DelegateCommand(CommitChanges);
        }

        private int ConvertPathToIndex(object path)
        {
            return TrustedLocations.IndexOf(path as string);
        }

        private void RaiseCanExecuteChanged()
        {
            AddPathCommand.RaiseCanExecuteChanged();
            DeletePathCommand.RaiseCanExecuteChanged();
            UpdatePathCommand.RaiseCanExecuteChanged();
        }

        private bool CanDelete(int param)
        {
            return TrustedLocations.Count > 1;
        }

        private bool CanUpdate(int param)
        {
            //add any exceptions or built in trusted locations here
            return true;
        }

        private void InsertPath()
        {
            var args = new TrustedPathEventArgs();

            ShowFileDialog(args);

            if (args.Cancel)
                return;

            TrustedLocations.Insert(TrustedLocations.Count, args.Path);
            RaiseCanExecuteChanged();
        }

        private void ShowFileDialog(TrustedPathEventArgs e)
        {
            OnRequestShowFileDialog(this, e);

            if (e.Cancel == false && TrustedLocations.Contains(e.Path))
                e.Cancel = true;
        }

        private void UpdatePathAt(int index)
        {
            var args = new TrustedPathEventArgs
            {
                Path = TrustedLocations[index]
            };

            ShowFileDialog(args);

            if (args.Cancel)
                return;

            TrustedLocations[index] = args.Path;
        }

        private void RemovePathAt(int index)
        {
            var pathToRemove = TrustedLocations[index];
            TrustedLocations.RemoveAt(index);
            RaiseCanExecuteChanged();
        }

        private void CommitChanges(object param)
        {
            var newpaths = CommitTrustedLocations();
            //TODO: to be implemented
        }

        internal void InitializeTrustedLocations()
        {
            //TODO: to be implemented
            TrustedLocations = new ObservableCollection<string>();
        }

        private List<string> CommitTrustedLocations()
        {
            var trustedLocations = new List<string>(TrustedLocations);
            return trustedLocations;
        }
    }
}
