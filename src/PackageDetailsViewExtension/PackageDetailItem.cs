using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.PackageManager;
using Dynamo.PythonServices;
using Greg.Responses;
using PythonNodeModels;

namespace Dynamo.PackageDetails
{
    /// <summary>
    /// A wrapper class for a PackageVersion object, used in the PackageDetailsExtension.
    /// </summary>
    public class PackageDetailItem : NotificationObject
    {
        #region Private Fields

        private PackageVersion packageVersion;
        private string packageVersionNumber;
        private string hosts;
        private string pythonVersion;
        private string copyRightHolder;
        private string copyRightYear;
        private List<string> packages;
        private bool canInstall;
        private string packageName;
        private PackageLoader PackageLoader { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// A reference to the PackageVersion object, a response from the GregClient.
        /// </summary>
        public PackageVersion PackageVersion
        {
            get => packageVersion;
            set
            {
                packageVersion = value;
                RaisePropertyChanged(nameof(PackageVersion));
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
                RaisePropertyChanged(nameof(PackageName));
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
                RaisePropertyChanged(nameof(PackageVersionNumber));
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
                RaisePropertyChanged(nameof(Hosts));
            }
        }

        /// <summary>
        /// The Copyright holder
        /// </summary>
        public string CopyRightHolder
        {
            get => copyRightHolder;
            set
            {
                copyRightHolder = value;
                RaisePropertyChanged(nameof(CopyRightHolder));
            }
        }

        /// <summary>
        /// The Copyright Year
        /// </summary>
        public string CopyRightYear
        {
            get => copyRightYear;
            set
            {
                copyRightYear = value;
                RaisePropertyChanged(nameof(CopyRightYear));
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
                RaisePropertyChanged(nameof(PythonVersion));
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
                RaisePropertyChanged(nameof(Packages));
            }
        }

        public bool CanInstall
        {
            get => canInstall;
            set
            {
                canInstall = value;
                RaisePropertyChanged(nameof(CanInstall));
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="packageVersion"></param>
        /// <param name="canInstall"></param>
        public PackageDetailItem(string packageName, PackageVersion packageVersion, bool canInstall)
        {
            this.PackageName = packageName;
            this.PackageVersion = packageVersion;
            this.PackageVersionNumber = PackageVersion.version;
            this.CopyRightHolder = PackageVersion.copyright_holder;
            this.CopyRightYear = PackageVersion.copyright_year;
            this.CanInstall = canInstall;
            
            // To avoid displaying package self-dependencies.
            // For instance, avoiding Clockwork showing that it depends on Clockwork.
            this.Packages = PackageVersion.full_dependency_ids
                .Select(x => x.name)
                .Where(x => x != PackageName)
                .ToList();

            DetectDependencies();
        }

        /// <summary>
        /// Reads the GregResponse collection of dependency information and sets values
        /// for PythonVersion and Hosts respectively.
        /// </summary>
        private void DetectDependencies()
        {
            List<string> pythonEngineVersions = new List<string>();
            List<string> hostDependencies = new List<string>();

            if (PackageVersion?.host_dependencies != null)
            {
                foreach (string stringValue in PackageVersion.host_dependencies)
                {
                    if (stringValue == PythonEngineManager.IronPython2EngineName ||
                        stringValue == PythonEngineManager.CPython3EngineName)
                    {
                        pythonEngineVersions.Add(stringValue);
                    }
                    else
                    {
                        hostDependencies.Add(stringValue);
                    }
                }
            }

            PythonVersion = pythonEngineVersions.Count > 0 ? string.Join(", ", pythonEngineVersions) : Dynamo.Properties.Resources.NoneString;
            Hosts = hostDependencies.Count > 0 ? string.Join(", ", hostDependencies) : Dynamo.Properties.Resources.NoneString;
        }
    }
}
