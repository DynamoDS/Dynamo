using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
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
        private string _runText = string.Empty;
        private object _selectedItem;

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

        internal ViewModel()
        {
            Assemblies = new ObservableCollection<IAssemblyData>();
            SetAssemblyPathCommand = new DelegateCommand(SetAssemblyPath, CanSetAssemblyPath);
            SetResultsPathCommand = new DelegateCommand(SetResultsPath, CanSetResultsPath);
            SetWorkingPathCommand = new DelegateCommand(SetWorkingPath, CanSetWorkingPath);
            RunCommand = new DelegateCommand<object>(Run, CanRun);
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
                Program.RunAssembly(parameter as IAssemblyData);
            }
            else if (parameter is IFixtureData)
            {
                Program.RunFixture(parameter as IFixtureData);
            }
            else if (parameter is ITestData)
            {
                Program.RunTest(parameter as ITestData);
            }
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
