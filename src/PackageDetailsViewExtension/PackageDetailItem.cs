using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dynamo.Annotations;
using Dynamo.PackageManager;
using Greg.Responses;
using PythonNodeModels;

namespace Dynamo.PackageDetails
{
    /// <summary>
    /// A wrapper class for a PackageVersion object, used in the PackageDetailsExtension.
    /// </summary>
    public class PackageDetailItem : INotifyPropertyChanged
    {
        private PackageVersion packageVersion;
        private string packageVersionNumber;
        private string hosts;
        private string pythonVersion;
        private List<string> packages;
        private bool canInstall;
        private string packageName;

        /// <summary>
        /// A reference to the PackageVersion object, a response from the GregClient.
        /// </summary>
        public PackageVersion PackageVersion
        {
            get => packageVersion;
            set
            {
                packageVersion = value;
                OnPropertyChanged(nameof(PackageVersion));
            }
        }

        /// <summary>
        /// The name of the package this PackageDetailItem represents a specific version of.
        /// </summary>
        public string PackageName
        {
            get => packageName;
            set
            {
                packageName = value; 
                OnPropertyChanged(nameof(PackageName));
            }
        }

        /// <summary>
        /// The version number of this package, e.g. 0.0.0.1
        /// </summary>
        public string PackageVersionNumber
        {
            get => packageVersionNumber;
            set
            {
                packageVersionNumber = value;
                OnPropertyChanged(nameof(PackageVersionNumber));
            }
        }

        /// <summary>
        /// The host software(s) this package relies upon, e.g. Revit, Civil3D
        /// </summary>
        public string Hosts
        {
            get => hosts;
            set
            {
                hosts = value;
                OnPropertyChanged(nameof(Hosts));
            }
        }

        /// <summary>
        /// The version of Python referenced in this package, if any
        /// </summary>
        public string PythonVersion
        {
            get => pythonVersion;
            set
            {
                pythonVersion = value;
                OnPropertyChanged(nameof(PythonVersion));
            }
        }

        /// <summary>
        /// The other packages contained within this package, if any.
        /// </summary>
        public List<string> Packages
        {
            get => packages;
            set
            {
                packages = value;
                OnPropertyChanged(nameof(Packages));
            }
        }

        public bool CanInstall
        {
            get => canInstall;
            set
            {
                canInstall = value;
                OnPropertyChanged(nameof(CanInstall));
            }
        }

        private PackageLoader PackageLoader { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="packageLoader"></param>
        /// <param name="packageVersion"></param>
        public PackageDetailItem(string packageName, PackageLoader packageLoader, PackageVersion packageVersion)
        {
            this.PackageName = packageName;
            this.PackageVersion = packageVersion;
            this.PackageVersionNumber = PackageVersion.version;
            this.PackageLoader = packageLoader;
            
            // To avoid displaying package self-dependencies.
            // For instance, avoiding Clockwork showing that it depends on Clockwork.
            this.Packages = PackageVersion.full_dependency_ids
                .Select(x => x.name)
                .Where(x => x != PackageName)
                .ToList();
            
            DetectDependencies();
            DetectWhetherCanInstall();
        }
        
        /// <summary>
        /// Reads the GregResponse collection of dependency information and sets values
        /// for PythonVersion and Hosts respectively.
        /// </summary>
        private void DetectDependencies()
        {
            List<string> pythonEngineVersions = new List<string>();
            List<string> hostDependencies = new List<string>();

            if(PackageVersion != null && PackageVersion.host_dependencies != null)
            {
                foreach (string stringValue in PackageVersion.host_dependencies)
                {
                    if (stringValue == PythonEngineVersion.IronPython2.ToString() ||
                        stringValue == PythonEngineVersion.CPython3.ToString())
                    {
                        pythonEngineVersions.Add(stringValue);
                    }
                    else
                    {
                        hostDependencies.Add(stringValue);
                    }
                }
            }

            PythonVersion = pythonEngineVersions.Count > 0 ? string.Join(", ", pythonEngineVersions) : "None";
            Hosts = hostDependencies.Count > 0 ? string.Join(", ", hostDependencies) : "None";
        }

        /// <summary>
        /// Detects whether this particular package and version is installed on the user's machine
        /// using the PackageLoader.
        /// </summary>
        internal void DetectWhetherCanInstall()
        {
            // In order for CanInstall to be true, both the name and installed package version must match
            // what is found in the PackageLoader.LocalPackages which are designated as 'Loaded'.
            List<Package> sameNamePackages = PackageLoader
                .LocalPackages
                .Where(x => x.Name == PackageName)
                .ToList();

            if (sameNamePackages.Count < 1) CanInstall = true;
            else
            {
                CanInstall = !sameNamePackages
                    .Select(x => x.VersionName)
                    .Contains(packageVersion.version);
            }
            OnPropertyChanged(nameof(CanInstall));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
