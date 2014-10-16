using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using Greg.Responses;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// A search element representing an element from the package manager </summary>
    public class PackageManagerSearchElement : SearchElementBase
    {
        private readonly DynamoViewModel dynamoViewModel;

        public DelegateCommand DownloadLatest { get; set; }
        public DelegateCommand UpvoteCommand { get; set; }
        public DelegateCommand DownvoteCommand { get; set; }

        /// <summary>
        /// The class constructor. </summary>
        /// <param name="header">The PackageHeader object describing the element</param>
        public PackageManagerSearchElement(DynamoViewModel dynamoViewModel, Greg.Responses.PackageHeader header)
        {
            this.dynamoViewModel = dynamoViewModel;

            this.Header = header;
            this.Weight = header.deprecated ? 0.1 : 1;

            if (header.keywords != null && header.keywords.Count > 0)
            {
                this.Keywords = String.Join(" ", header.keywords);
            } 
            else
            {
                this.Keywords = "";
            }
            this.Votes = header.votes;
            this.IsExpanded = false;
            this.DownloadLatest = new DelegateCommand((Action) Execute);
            this.UpvoteCommand = new DelegateCommand((Action) Upvote, CanUpvote);
            this.DownvoteCommand = new DelegateCommand((Action) Downvote, CanDownvote);
        }

        public void Upvote()
        {
            Task<bool>.Factory.StartNew(() => dynamoViewModel.Model.PackageManagerClient.Upvote(this.Id))
                .ContinueWith((t) =>
                {
                    if (t.Result)
                    {
                        this.Votes += 1;
                    }
                }
                , TaskScheduler.FromCurrentSynchronizationContext()); 

        }

        private bool CanUpvote()
        {
            return this.dynamoViewModel.Model.PackageManagerClient.HasAuthenticator;
        }

        public void Downvote()
        {
            Task<bool>.Factory.StartNew(() => dynamoViewModel.Model.PackageManagerClient.Downvote(this.Id))
                .ContinueWith((t) =>
                {
                    if (t.Result)
                    {
                        this.Votes -= 1;
                    }
                } , TaskScheduler.FromCurrentSynchronizationContext()); 
        }


        private bool CanDownvote()
        {
            return this.dynamoViewModel.Model.PackageManagerClient.HasAuthenticator;
        }

        private static IEnumerable<Tuple<PackageHeader, PackageVersion>> ListRequiredPackageVersions(
            IEnumerable<PackageHeader> headers, PackageVersion version)
        {
  
            return headers.Zip(
                version.full_dependency_versions,
                (header, v) => new Tuple<PackageHeader, string>(header, v))
                .Select(
                    (pair) =>
                        new Tuple<PackageHeader, PackageVersion>(
                        pair.Item1,
                        pair.Item1.versions.First(x => x.version == pair.Item2)));
        } 

        public override void Execute()
        {
            var version = versionNumberToDownload ?? this.Header.versions.Last();

            string message = "Are you sure you want to install " + this.Name +" "+ version.version + "?";

            var result = MessageBox.Show(message, "Package Download Confirmation",
                            MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.OK)
            {
                // get all of the headers
                var headers = version.full_dependency_ids.Select(dep=>dep._id).Select((id) =>
                    {
                        PackageHeader pkgHeader;
                        var res = dynamoViewModel.Model.PackageManagerClient.DownloadPackageHeader(id, out pkgHeader);
                        
                        if (!res.Success)
                            MessageBox.Show("Failed to download package with id: " + id + ".  Please try again and report the package if you continue to have problems.", "Package Download Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);

                        return pkgHeader;
                    }).ToList();

                // if any header download fails, abort
                if (headers.Any(x => x == null))
                {
                    return;
                }

                var allPackageVersions = ListRequiredPackageVersions(headers, version);

                // determine if any of the packages contain binaries or python scripts.  
                var containsBinaries =
                    allPackageVersions.Any(
                        x => x.Item2.contents.Contains(PackageManagerClient.PackageContainsBinariesConstant));

                var containsPythonScripts =
                    allPackageVersions.Any(
                        x => x.Item2.contents.Contains(PackageManagerClient.PackageContainsPythonScriptsConstant));

                // if any do, notify user and allow cancellation
                if (containsBinaries || containsPythonScripts)
                {
                    var res = MessageBox.Show("The package or one of its dependencies contains Python scripts or binaries. "+
                        "Do you want to continue?", "Package Download",
                        MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);

                    if (res == MessageBoxResult.Cancel) return;
                }

                // Determine if there are any dependencies that are made with a newer version
                // of Dynamo (this includes the root package)
                var dynamoVersion = dynamoViewModel.Model.Version;
                var dynamoVersionParsed = VersionUtilities.PartialParse(dynamoVersion, 3);
                var futureDeps = allPackageVersions.FilterFuturePackages(dynamoVersionParsed);

                // If any of the required packages use a newer version of Dynamo, show a dialog to the user
                // allowing them to cancel the package download
                if (futureDeps.Any())
                {
                    var sb = new StringBuilder();

                    sb.AppendLine(
                        "The following packages use a newer version of Dynamo than you are currently using: ");
                    sb.AppendLine();

                    foreach (var elem in futureDeps)
                    {
                        sb.AppendLine(elem.Item1.name + " " + elem.Item2);
                    }

                    sb.AppendLine();
                    sb.AppendLine("Do you want to continue?");

                    // If the user
                    if (MessageBox.Show(
                        sb.ToString(),
                        "Package Uses Newer Version of Dynamo!",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }

                var localPkgs = dynamoViewModel.Model.Loader.PackageLoader.LocalPackages;

                var uninstallsRequiringRestart = new List<Package>();
                var uninstallRequiringUserModifications = new List<Package>();
                var immediateUninstalls = new List<Package>();

                // if a package is already installed we need to uninstall it, allowing
                // the user to cancel if they do not want to uninstall the package
                foreach ( var localPkg in headers.Select(x => localPkgs.FirstOrDefault(v => v.Name == x.name)) )
                {
                    if (localPkg == null) continue;

                    if (localPkg.LoadedAssemblies.Any())
                    {
                        uninstallsRequiringRestart.Add(localPkg);
                        continue;
                    }

                    if (localPkg.InUse(this.dynamoViewModel.Model))
                    {
                        uninstallRequiringUserModifications.Add(localPkg);
                        continue;
                    }

                    immediateUninstalls.Add(localPkg);
                }

                string msg;

                if (uninstallRequiringUserModifications.Any())
                {
                    msg = "Dynamo needs to uninstall " + JoinPackageNames(uninstallRequiringUserModifications) + 
                        " to continue, but cannot as one of its types appears to be in use.  Try restarting Dynamo.";
                    MessageBox.Show(msg, "Cannot Download Package", MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

                if (uninstallsRequiringRestart.Any())
                {
                    // mark for uninstallation
                    uninstallsRequiringRestart.ForEach(
                        x =>
                            x.MarkForUninstall(
                                this.dynamoViewModel.Model.PreferenceSettings));

                    msg = "Dynamo needs to uninstall " + JoinPackageNames(uninstallsRequiringRestart) + 
                        " to continue but it contains binaries already loaded into Dynamo.  It's now marked "+
                        "for removal, but you'll need to first restart Dynamo.";
                    MessageBox.Show(msg, "Cannot Download Package", MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

                if (immediateUninstalls.Any())
                {
                    // if the package is not in use, tell the user we will be uninstall it and give them the opportunity to cancel
                    msg = "Dynamo has already installed " + JoinPackageNames(immediateUninstalls) + 
                        ".  \n\nDynamo will attempt to uninstall this package before installing.  ";
                    if (MessageBox.Show(msg, "Download Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                        return;
                }

                // form header version pairs and download and install all packages
                allPackageVersions
                        .Select( x => new PackageDownloadHandle(this.dynamoViewModel, x.Item1, x.Item2))
                        .ToList()
                        .ForEach(x=>x.Start());

            }

        }

        private string JoinPackageNames(IEnumerable<Package> pkgs)
        {
            return String.Join(", ", pkgs.Select(x => x.Name));
        } 

        #region Properties 

            private PackageVersion versionNumberToDownload = null;

            public List<Tuple<PackageVersion, DelegateCommand>> Versions
            {
                get
                {
                    return
                        Header.versions.Select(
                            x => new Tuple<PackageVersion, DelegateCommand>(x, new DelegateCommand(() =>
                                {
                                    this.versionNumberToDownload = x;
                                    this.Execute();
                                }, () => true))).ToList();
                } 
            }
            public string Maintainers { get { return String.Join(", ", this.Header.maintainers.Select(x=>x.username)); } }
            private int _votes;
            public int Votes
            {
                get { return _votes; } 
                set { _votes = value; RaisePropertyChanged("Votes"); }
            }
            public bool IsDeprecated { get { return this.Header.deprecated; } }
            public int Downloads { get { return this.Header.downloads; } }
            public string EngineVersion { get { return Header.versions[Header.versions.Count - 1].engine_version; } }
            public int UsedBy { get { return this.Header.used_by.Count; } } 
            public string LatestVersion { get { return Header.versions[Header.versions.Count - 1].version; } }
            
            /// <summary>
            /// Header property </summary>
            /// <value>
            /// The PackageHeader used to instantiate this object </value>
            public Greg.Responses.PackageHeader Header { get; internal set; }

            /// <summary>
            /// Type property </summary>
            /// <value>
            /// A string describing the type of object </value>
            public override string Type { get { return "Community Node"; } }

            /// <summary>
            /// Name property </summary>
            /// <value>
            /// The name of the node </value>
            public override string Name { get { return Header.name; } }

            /// <summary>
            /// Description property </summary>
            /// <value>
            /// A string describing what the node does</value>
            public override string Description { get { return Header.description ?? ""; } }

            /// <summary>
            /// Weight property </summary>
            /// <value>
            /// Number defining the relative importance of the element in search. 
            /// Higher = closer to the top of search results </value>
            public override double Weight { get; set; }

            public override bool Searchable { get { return true; } }

            /// <summary>
            /// Guid property </summary>
            /// <value>
            /// A string that uniquely defines the CustomNodeDefinition </value>
            public Guid Guid { get; internal set; }

            /// <summary>
            /// Id property </summary>
            /// <value>
            /// A string that uniquely defines the Package on the server  </value>
            public string Id { get { return Header._id; } }

            public override string Keywords { get; set; }

        #endregion
        

    }

}
