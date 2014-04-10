﻿using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.RevitAddIns;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace RevitTestFrameworkRunner
{
    public class ViewModel : NotificationObject
    {
        private ObservableCollection<IAssemblyData> _assemblies;
        private ObservableCollection<RevitProduct> _products;
 
        private string _runText = string.Empty;
        private object _selectedItem;
        private int _selectedProductIndex;

        public DelegateCommand SetAssemblyPathCommand { get; set; }
        public DelegateCommand SetResultsPathCommand { get; set; }
        public DelegateCommand SetWorkingPathCommand { get; set; }
        public DelegateCommand<object> RunCommand { get; set; }

        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
                RaisePropertyChanged("RunText");
                RunCommand.RaiseCanExecuteChanged();
            }
        }

        public int SelectedProductIndex
        {
            get { return _selectedProductIndex; }
            set
            {
                _selectedProductIndex = value;

                Program._revitPath = _selectedProductIndex == -1 ? 
                    string.Empty : 
                    Path.Combine(Products[value].InstallLocation, "revit.exe");

                RaisePropertyChanged("SelectedProductIndex");
            }
        }

        public string RunText
        {
            get
            {
                if (SelectedItem is IAssemblyData)
                {
                    return "Run all tests in assembly.";
                }
                
                if (SelectedItem is IFixtureData)
                {
                    return "Run all tests in fixture.";
                }
                 
                if(SelectedItem is ITestData)
                {
                    return "Run test.";
                }

                return "Nothing selected.";
            }
            set
            {
                _runText = value;
                RaisePropertyChanged("RunText");
            }
        }
        
        public ObservableCollection<IAssemblyData> Assemblies
        {
            get { return _assemblies; }
            set
            {
                _assemblies = value;
                RaisePropertyChanged("Assemblies");
            }
        }

        public ObservableCollection<RevitProduct> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                RaisePropertyChanged("Products");
            }
        }

        public string ResultsPath
        {
            get { return Program._results; }
            set
            {
                Program._results = value;
                RaisePropertyChanged("ResultsPath");
                RunCommand.RaiseCanExecuteChanged();
            }
        }

        public string AssemblyPath
        {
            get { return Program._testAssembly; }
            set
            {
                Program._testAssembly = value;
                Program.Refresh(this);
                RaisePropertyChanged("AssemblyPath");
            }
        }

        public string WorkingPath
        {
            get { return Program._workingDirectory; }
            set
            {
                Program._workingDirectory = value;
                Program.Refresh(this);
                RaisePropertyChanged("WorkingPath");
            }
        }

        public bool IsDebug
        {
            get { return Program._isDebug; }
            set
            {
                Program._isDebug = value;
                RaisePropertyChanged("IsDebug");
            }
        }

        internal ViewModel()
        {
            Assemblies = new ObservableCollection<IAssemblyData>();
            Products = new ObservableCollection<RevitProduct>();
            SetAssemblyPathCommand = new DelegateCommand(SetAssemblyPath, CanSetAssemblyPath);
            SetResultsPathCommand = new DelegateCommand(SetResultsPath, CanSetResultsPath);
            SetWorkingPathCommand = new DelegateCommand(SetWorkingPath, CanSetWorkingPath);
            RunCommand = new DelegateCommand<object>(Run, CanRun);

            Products.CollectionChanged += Products_CollectionChanged;
        }

        void Products_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // When the products collection is changed, we want to set
            // the selected product index to the first in the list
            if (Products.Count > 0)
            {
                SelectedProductIndex = 0;
            }
            else
            {
                SelectedProductIndex = -1;
            }
        }

        private bool CanRun(object parameter)
        {
            return SelectedItem != null;
        }

        private void Run(object parameter)
        {
            if (File.Exists(Program._results))
            {
                File.Delete(Program._results);
            }

            if (parameter is IAssemblyData)
            {
                var ad = parameter as IAssemblyData;
                Program._runCount = ad.Fixtures.SelectMany(f => f.Tests).Count();
                Program.RunAssembly(ad);
            }
            else if (parameter is IFixtureData)
            {
                var fd = parameter as IFixtureData;
                Program._runCount = fd.Tests.Count;
                Program.RunFixture(fd);
            }
            else if (parameter is ITestData)
            {
                Program._runCount = 1;
                Program.RunTest(parameter as ITestData);
            }

            Program.Cleanup();
        }

        private bool CanSetWorkingPath()
        {
            return true;
        }

        private void SetWorkingPath()
        {
            var dirs = new FolderBrowserDialog();

            if (dirs.ShowDialog() == DialogResult.OK)
            {
                WorkingPath = dirs.SelectedPath;
            }
        }

        private bool CanSetResultsPath()
        {
            return true;
        }

        private void SetResultsPath()
        {
            var files = new SaveFileDialog()
            {
                InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Filter = "xml files (*.xml) | *.xml",
                RestoreDirectory = true,
                DefaultExt = ".xml"
            };

            var filesResult = files.ShowDialog();

            if (filesResult != null && filesResult == true)
            {
                ResultsPath = files.FileName;
            }
        }

        private bool CanSetAssemblyPath()
        {
            return true;
        }

        private void SetAssemblyPath()
        {
            var files = new OpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Filter = "dll files (*.dll) | *.dll",
                RestoreDirectory = true,
                DefaultExt = ".dll"
            };

            var filesResult = files.ShowDialog();

            if (filesResult != null && filesResult == true)
            {
                AssemblyPath = files.FileName;
            }
        }
    }
}
