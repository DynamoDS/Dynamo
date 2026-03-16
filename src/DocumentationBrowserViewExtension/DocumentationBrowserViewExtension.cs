using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dynamo.Configuration;
using Dynamo.DocumentationBrowser.Properties;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Search.SearchElements;
using Dynamo.Selection;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Interfaces;
using DynamoProperties = Dynamo.Properties;
using MenuItem = System.Windows.Controls.MenuItem;
using Microsoft.Win32;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// The DocumentationBrowser view extension displays web or local html files on the Dynamo right panel.
    /// It reacts to documentation display request events in Dynamo to know what and when to display documentation.
    /// </summary>
    public class DocumentationBrowserViewExtension : ViewExtensionBase, IViewExtension, ILogSource, ILayoutSpecSource
    {
        private ViewStartupParams viewStartupParamsReference;
        private ViewLoadedParams viewLoadedParamsReference;
        private MenuItem documentationBrowserMenuItem;
        private PackageManagerExtension pmExtension;
        private const string FALLBACK_DOC_DIRECTORY_NAME = "fallback_docs";
        //these fields should only be directly set by tests.
        internal DirectoryInfo fallbackDocPath;
        internal DirectoryInfo webBrowserUserDataFolder;

        internal Dictionary<string, string> BreadCrumbsDict { get; set; }

        internal DocumentationBrowserView BrowserView { get; private set; }
        internal DocumentationBrowserViewModel ViewModel { get; private set; }
        private DynamoViewModel DynamoViewModel { get; set; }

        /// <summary>
        /// Extension Name
        /// </summary>
        public override string Name => Properties.Resources.ExtensionName;

        /// <summary>
        /// GUID of the extension
        /// </summary>
        public override string UniqueId => "68B45FC0-0BD1-435C-BF28-B97CB03C71C8";

        public DocumentationBrowserViewExtension()
        {
            // initialise the ViewModel and View for the window
            this.ViewModel = new DocumentationBrowserViewModel();
            this.BrowserView = new DocumentationBrowserView(this.ViewModel);
        }

        #region ILogSource

        public event Action<ILogMessage> MessageLogged;

        internal void OnMessageLogged(ILogMessage msg)
        {
            MessageLogged?.Invoke(msg);
        }
        #endregion

        #region IViewExtension lifecycle

        private Func<LayoutSpecification> layouthandler;

        // Interface implementation allowing us to subscribe to the LayoutSpecification handler 
        public event Action<string> RequestApplyLayoutSpec;
        // Interface implementation allowing us to subscribe to the LayoutSpecification handler 
        public event Func<LayoutSpecification> RequestLayoutSpec
        {
            add { layouthandler += value; }
            remove { layouthandler -= value; }
        }

        public override void Startup(ViewStartupParams viewStartupParams)
        {
            this.viewStartupParamsReference = viewStartupParams;

            pmExtension = viewStartupParams.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            PackageDocumentationManager.Instance.AddDynamoPaths(viewStartupParams.PathManager);

            var pathManager = viewStartupParams.PathManager;
            if (!string.IsNullOrEmpty(pathManager.DynamoCoreDirectory))
            {
                var docsDir = new DirectoryInfo(Path.Combine(pathManager.DynamoCoreDirectory, Thread.CurrentThread.CurrentCulture.ToString(), FALLBACK_DOC_DIRECTORY_NAME));
                if (!docsDir.Exists)
                {
                    docsDir = new DirectoryInfo(Path.Combine(pathManager.DynamoCoreDirectory, "en-US", FALLBACK_DOC_DIRECTORY_NAME));
                }
                fallbackDocPath = docsDir.Exists ? docsDir : null;
            }

            if (!string.IsNullOrEmpty(pathManager.HostApplicationDirectory))
            {
                //when running over any host app like Revit, FormIt, Civil3D... the path to the fallback_docs can change.
                //e.g. for Revit the variable HostApplicationDirectory = C:\Program Files\Autodesk\Revit 2023\AddIns\DynamoForRevit\Revit
                //Then we need to remove the last folder from the path so we can find the fallback_docs directory.
                var hostAppDirectory = Directory.GetParent(pathManager.HostApplicationDirectory).FullName;
                var docsDir = new DirectoryInfo(Path.Combine(hostAppDirectory, Thread.CurrentThread.CurrentCulture.ToString(), FALLBACK_DOC_DIRECTORY_NAME));
                if (!docsDir.Exists)
                {
                    docsDir = new DirectoryInfo(Path.Combine(hostAppDirectory, "en-US", FALLBACK_DOC_DIRECTORY_NAME));
                }
                fallbackDocPath = docsDir.Exists ? docsDir : null;
            }

            //When executing Dynamo as Sandbox or inside any host like Revit, FormIt, Civil3D the WebView2 cache folder will be located in the AppData folder
            var userDataDir = new DirectoryInfo(pathManager.UserDataDirectory);
            webBrowserUserDataFolder = userDataDir.Exists ? userDataDir : null;

            if (this.BrowserView == null) return;

            if(fallbackDocPath != null)
            {
                this.BrowserView.FallbackDirectoryName = fallbackDocPath.FullName;
            }

            if(webBrowserUserDataFolder != null)
            {
                this.BrowserView.WebBrowserUserDataFolder = webBrowserUserDataFolder.FullName;
            }
        }

        public override void Loaded(ViewLoadedParams viewLoadedParams)
        {
            if (viewLoadedParams == null) throw new ArgumentNullException(nameof(viewLoadedParams));
            this.viewLoadedParamsReference = viewLoadedParams; 

            this.ViewModel.MessageLogged += OnViewModelMessageLogged;
            this.ViewModel.HandleInsertFile += OnInsertFile;
            PackageDocumentationManager.Instance.MessageLogged += OnMessageLogged;


            // Add a button to Dynamo View menu to manually show the window
            this.documentationBrowserMenuItem = new MenuItem { Header = Resources.MenuItemText, IsCheckable = true };
            this.documentationBrowserMenuItem.Checked += MenuItemCheckHandler;
            this.documentationBrowserMenuItem.Unchecked += MenuItemUnCheckedHandler;
            this.viewLoadedParamsReference.AddExtensionMenuItem(this.documentationBrowserMenuItem);

            // subscribe to the documentation open request event from Dynamo
            this.viewLoadedParamsReference.RequestOpenDocumentationLink += HandleRequestOpenDocumentationLink;

            // subscribe to node help audit request (e.g. from Debug menu)
            this.viewLoadedParamsReference.NodeHelpAuditRequested += OnNodeHelpAuditRequested;

            // subscribe to property changes of DynamoViewModel so we can show/hide the browser on StartPage display
            (viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel).PropertyChanged += HandleStartPageVisibilityChange;

            // pmExtension could be null, if this is the case we bail before interacting with it.
            if (pmExtension is null)
                return;

            // subscribe to package loaded so we can add the package documentation 
            // to the Package documentation manager when a package is loaded
            pmExtension.PackageLoader.PackgeLoaded += OnPackageLoaded;

            // add packages already loaded to the PackageDocumentationManager
            foreach (var pkg in pmExtension.PackageLoader.LocalPackages)
            {
                OnPackageLoaded(pkg);
            }

            this.DynamoViewModel = (viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel);

            this.ViewModel.Locale = DynamoViewModel.PreferenceSettings.Locale;

            // set the viewmodel UIElement property to be used for Nodes Library manipulation
            this.ViewModel.DynamoView = viewLoadedParams.DynamoWindow;
        }

        public override void Shutdown()
        {
            // Do nothing for now
        }

        private void OnInsertFile(object sender, InsertDocumentationLinkEventArgs e)
        {
            if (e.Data.Equals(Resources.FileNotFoundFailureMessage))
            {
                var message = String.Format(Resources.ToastFileNotFoundLocationNotificationText, e.Name);
                DynamoViewModel.ToastManager.CreateRealTimeInfoWindow(message, true);

                return;
            }

            if (DynamoViewModel.Model.CurrentWorkspace is HomeWorkspaceModel)
            {
                var homeWorkspace = DynamoViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
                if (homeWorkspace != null && homeWorkspace.RunSettings.RunType != RunType.Manual)
                {
                    DynamoViewModel.ToastManager.CreateRealTimeInfoWindow(Resources.ToastInsertGraphNotificationText, true);
                    homeWorkspace.RunSettings.RunType = RunType.Manual;
                }
            }
            
            var existingGroups = GetExistingGroups();

            // Insert the file and select all the elements that were inserted 
            this.DynamoViewModel.Model.InsertFileFromPath(e.Data);

            if (!DynamoSelection.Instance.Selection.Any()) return;

            Dispatcher.CurrentDispatcher.BeginInvoke(() =>
            {
                GroupInsertedGraph(existingGroups, e.Name);
            });
            //we want to wait for the new group to be inserted and actually rendered, so we add the layout command
            //as a background priority task on the ui dispatcher.
            Dispatcher.CurrentDispatcher.BeginInvoke(() =>
            {
                // We have selected all the nodes and notes from the inserted graph
                // Now is the time to auto layout the inserted nodes
                this.DynamoViewModel.GraphAutoLayoutCommand.Execute(null);
                this.DynamoViewModel.FitViewCommand.Execute(false);
            },DispatcherPriority.Background);
        }


        private void GroupInsertedGraph(List<AnnotationViewModel> existingGroups, string graphName)
        {
            var selection = GetCurrentSelection();
            var hostGroups = GetAllHostingGroups(existingGroups);

            foreach (var group in hostGroups)
            {
                group.DissolveNestedGroupsCommand.Execute(null);
            }

            foreach (var group in hostGroups)
            {
                selection.RemoveAll(x => group.AnnotationModel.ContainsModel(x as ModelBase));
            }
            
            DynamoSelection.Instance.Selection.AddRange(selection);
            DynamoSelection.Instance.Selection.AddRange(hostGroups.Select(x => x.AnnotationModel));

            // Add the inserted nodes into a group
            var annotation = this.DynamoViewModel.Model.CurrentWorkspace.AddAnnotation(Resources.InsertedGroupSubTitle, Guid.NewGuid());
            if (annotation != null)
            {
                annotation.AnnotationText = graphName;

                var annotationViewModel = DynamoViewModel.CurrentSpaceViewModel.Annotations
                        .First(x => x.AnnotationModel == annotation);

                GroupStyleItem styleItem = null;
                //This will try to find the GroupStyle review 
                styleItem =  annotationViewModel.GroupStyleList.OfType<GroupStyleItem>().FirstOrDefault(x => x.Name.Equals(DynamoProperties.Resources.GroupStyleDefaultReview));
                if(styleItem == null)
                {
                    //If no GroupStyle is found matching the specific criteria we will use the first one
                    styleItem = annotationViewModel.GroupStyleList.OfType<GroupStyleItem>().First();
                }
                var groupStyleItem = new GroupStyleItem {Name = styleItem.Name, HexColorString = styleItem.HexColorString};
                annotationViewModel.UpdateGroupStyle(groupStyleItem);

                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.AddRange(annotation.Nodes);
                DynamoSelection.Instance.Selection.Add(annotation);

                if (annotation.HasNestedGroups)
                {
                    DynamoSelection.Instance.Selection.AddRange(annotation.Nodes.OfType<AnnotationModel>().SelectMany(x => x.Nodes));
                }
            }
        }

        private List<AnnotationViewModel> GetAllHostingGroups(List<AnnotationViewModel> existingGroups)
        {
            List<AnnotationViewModel> hostGroups = new List<AnnotationViewModel>();

            foreach (var group in this.DynamoViewModel.CurrentSpaceViewModel.Annotations)
            {
                if (existingGroups.Contains(group)) continue;
                if (group.AnnotationModel.HasNestedGroups)
                {
                    hostGroups.Add(group);
                }
            }

            return hostGroups;
        }

        /// <summary>
        /// This method will return a reorganized version of the current selection
        /// discarding nodes if they are in groups, selecting the group instead
        /// </summary>
        /// <returns></returns>
        private List<ISelectable> GetCurrentSelection()
        {
            List<ISelectable> selection = new List<ISelectable>();

            foreach (var selected in DynamoSelection.Instance.Selection)
            {
                var nodeOrGroup = SelectNodeOrGroup(selected, selection);
                if (nodeOrGroup != null)
                {
                    selection.Add(nodeOrGroup);
                }
            }

            return selection;
        }

        private ISelectable SelectNodeOrGroup(ISelectable selected, List<ISelectable> selection)
        {
            foreach (var group in this.DynamoViewModel.CurrentSpaceViewModel.Annotations)
            {
                // Check if the current selected element is part of a group
                if (group.Nodes.Contains(selected))
                {
                    // If that's the case, and the group is not part of the selection set yet, add the group to the selection set
                    if (!selection.Contains(group.AnnotationModel))
                    {
                        return group.AnnotationModel;
                    }
                    // Else (if the element is part of a group, and the group has been added already to the selection set) skip this iteration
                    return null;
                }
            }
            // if the element was not part of a group, add it to the selection set
            return selected;
        }

        private List<AnnotationViewModel> GetExistingGroups()
        {
            List<AnnotationViewModel> result = new List<AnnotationViewModel>();

            foreach (var group in this.DynamoViewModel.CurrentSpaceViewModel.Annotations)
            {
                result.Add(group);
            }

            return result;
        }

        private void RequestLoadLayoutSpecs()
        {
            if (BreadCrumbsDict != null) return;

            var output = layouthandler?.Invoke();
            if (output == null) return;

            PopulateBreadCrumbsDictionary(output);
        }

        private void PopulateBreadCrumbsDictionary(LayoutSpecification layoutSpec)
        {
            BreadCrumbsDict = new Dictionary<string, string>();

            if (layoutSpec == null || !layoutSpec.sections.Any())
            {
                return;
            }
            var section = layoutSpec.sections.First();
            var breadCrumb = string.Empty;


            if (section.childElements.Count == 0) return;

            foreach (var child in section.childElements)
            {
                breadCrumb = child.text + " / ";

                RecursiveIncludeSearch(child, breadCrumb);
            }

            this.ViewModel.BreadCrumbsDictionary = BreadCrumbsDict;
        }
        
        private void RecursiveIncludeSearch(LayoutElement child, string breadCrumb)
        {
            string crumb = breadCrumb;

            if (child.childElements.Any())
            {
                foreach (var grandchild in child.childElements)
                {
                    crumb = breadCrumb + grandchild.text + " / ";

                    RecursiveIncludeSearch(grandchild, crumb);
                }
            }

            foreach (var info in child.include)
            {
                var typeArray = info.path.Split('.');
                var type = typeArray[typeArray.Length - 2] + "." + typeArray[typeArray.Length - 1];

                BreadCrumbsDict[type] = crumb.Remove(crumb.Length-3);
            }
        }


        private void OnPackageLoaded(Package pkg)
        {
            // Add documentation files from the package to the DocManager
            PackageDocumentationManager.Instance.AddPackageDocumentation(pkg.NodeDocumentaionDirectory, pkg.Name);
        }

        private void OnNodeHelpAuditRequested()
        {
            RunNodeHelpAudit();
        }

        internal void RunNodeHelpAudit()
        {
            if (DynamoViewModel == null || ViewModel == null)
            {
                OnMessageLogged(LogMessage.Warning(Resources.NodeHelpAuditNotReady, WarningLevel.Mild));
                return;
            }

            var docManager = PackageDocumentationManager.Instance;
            if (docManager == null)
            {
                OnMessageLogged(LogMessage.Warning(Resources.NodeHelpAuditManagerMissing, WarningLevel.Mild));
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = Resources.NodeHelpAuditSaveDialogFilter,
                DefaultExt = ".csv",
                AddExtension = true,
                FileName = $"NodeHelpAudit_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                Title = Resources.NodeHelpAuditSaveDialogTitle
            };

            var owner = viewLoadedParamsReference?.DynamoWindow;
            var dialogResult = owner == null ? saveDialog.ShowDialog() : saveDialog.ShowDialog(owner);
            if (dialogResult != true)
            {
                return;
            }

            var targetPath = saveDialog.FileName;
            try
            {
                var entries = DynamoViewModel.Model?.SearchModel?.Entries?.Where(entry => entry.IsVisibleInSearch).ToList();
                if (entries == null || entries.Count == 0)
                {
                    return;
                }

                var packages = pmExtension?.PackageLoader?.LocalPackages?.ToList() ?? new List<Package>();
                var packageRoots = BuildPackageRootIndex(packages);
                var packageAssemblies = BuildPackageAssemblyLookup(packages);

                // Collect all per-entry data on the UI thread (CreateNode, GetMinimumQualifiedName, etc. are not thread-safe).
                var auditRows = new List<NodeHelpAuditRowDto>();
                foreach (var entry in entries)
                {
                    try
                    {
                        var node = entry.CreateNode();
                        var minimumQualifiedName = DynamoViewModel.GetMinimumQualifiedName(node);
                        var packageName = ResolvePackageName(entry, packageRoots, packageAssemblies);

                        var mdPath = docManager.GetAnnotationDoc(minimumQualifiedName, packageName) ?? string.Empty;
                        var isBuiltInByPath = !string.IsNullOrEmpty(packageName) && ViewModel.IsBuiltInDocPath(mdPath);
                        var isOwnedByPackage = !string.IsNullOrEmpty(packageName) && !isBuiltInByPath;

                        var sampleGraphPath = string.IsNullOrWhiteSpace(mdPath)
                            ? string.Empty
                            : ViewModel.DynamoGraphFromMDFilePath(mdPath, isOwnedByPackage);

                        var category = entry.FullCategoryName ?? string.Empty;
                        var library = GetLibraryName(category);

                        auditRows.Add(new NodeHelpAuditRowDto(
                            library,
                            category,
                            entry.Name ?? string.Empty,
                            entry.FullName ?? string.Empty,
                            mdPath,
                            sampleGraphPath));
                    }
                    catch (Exception)
                    {
                    }
                }

                // Only file I/O and string building on the background thread.
                _ = Task.Run(() =>
                {
                    try
                    {
                        var csv = new StringBuilder();
                        var csvHeader = Resources.NodeHelpAuditCsvHeader;
                        if (string.IsNullOrWhiteSpace(csvHeader))
                        {
                            csvHeader = "Library,Category,Name,FullName,MissingMd,MissingDyn,MissingImage,MarkdownPath,SampleGraphPath,ImagePaths";
                        }
                        csv.AppendLine(csvHeader);

                        foreach (var row in auditRows)
                        {
                            try
                            {
                                var missingMd = string.IsNullOrWhiteSpace(row.MdPath) || !File.Exists(row.MdPath);
                                var missingDyn = string.IsNullOrWhiteSpace(row.SampleGraphPath) || !File.Exists(row.SampleGraphPath);

                                var imagePaths = GetImagePathsFromMarkdownFile(row.MdPath);
                                var missingImage = imagePaths.Count > 0 && imagePaths.Any(path => !File.Exists(path));
                                var imagePathsValue = imagePaths.Count == 0 ? string.Empty : string.Join(";", imagePaths);

                                csv.AppendLine(string.Join(",",
                                    EscapeCsv(row.Library),
                                    EscapeCsv(row.Category),
                                    EscapeCsv(row.Name),
                                    EscapeCsv(row.FullName),
                                    missingMd,
                                    missingDyn,
                                    missingImage,
                                    EscapeCsv(row.MdPath),
                                    EscapeCsv(row.SampleGraphPath),
                                    EscapeCsv(imagePathsValue)));
                            }
                            catch (Exception)
                            {
                            }
                        }

                        File.WriteAllText(targetPath, csv.ToString(), new UTF8Encoding(false));
                    }
                    catch (Exception)
                    {
                    }
                });
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// DTO for one node help audit row. Filled on the UI thread; file I/O and CSV fields derived on the background thread.
        /// </summary>
        private sealed class NodeHelpAuditRowDto
        {
            internal string Library { get; }
            internal string Category { get; }
            internal string Name { get; }
            internal string FullName { get; }
            internal string MdPath { get; }
            internal string SampleGraphPath { get; }

            internal NodeHelpAuditRowDto(string library, string category, string name, string fullName, string mdPath, string sampleGraphPath)
            {
                Library = library ?? string.Empty;
                Category = category ?? string.Empty;
                Name = name ?? string.Empty;
                FullName = fullName ?? string.Empty;
                MdPath = mdPath ?? string.Empty;
                SampleGraphPath = sampleGraphPath ?? string.Empty;
            }
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.Contains("\"") || value.Contains(",") || value.Contains("\n") || value.Contains("\r"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }

        private static List<string> GetImagePathsFromMarkdownFile(string markdownPath)
        {
            if (string.IsNullOrWhiteSpace(markdownPath) || !File.Exists(markdownPath))
            {
                return new List<string>();
            }

            string markdownContent;
            try
            {
                markdownContent = File.ReadAllText(markdownPath);
            }
            catch
            {
                return new List<string>();
            }

            if (string.IsNullOrWhiteSpace(markdownContent))
            {
                return new List<string>();
            }

            var baseDirectory = Path.GetDirectoryName(markdownPath);
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                return new List<string>();
            }

            var results = new List<string>();

            foreach (Match match in Regex.Matches(markdownContent, @"!\[[^\]]*\]\((?<path>[^)\s]+)[^)]*\)", RegexOptions.IgnoreCase))
            {
                AddImagePath(match.Groups["path"].Value, baseDirectory, results);
            }

            foreach (Match match in Regex.Matches(markdownContent, "<img[^>]+src=[\"'](?<path>[^\"']+)[\"']", RegexOptions.IgnoreCase))
            {
                AddImagePath(match.Groups["path"].Value, baseDirectory, results);
            }

            return results.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static void AddImagePath(string rawPath, string baseDirectory, List<string> results)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                return;
            }

            var trimmed = rawPath.Trim().Trim('"', '\'');
            if (trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                {
                    return;
                }

                if (uri.Scheme == Uri.UriSchemeFile)
                {
                    results.Add(uri.LocalPath);
                    return;
                }
            }

            var unescaped = Uri.UnescapeDataString(trimmed);
            var combined = Path.IsPathRooted(unescaped)
                ? unescaped
                : Path.Combine(baseDirectory, unescaped);

            results.Add(Path.GetFullPath(combined));
        }

        private static string GetLibraryName(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return string.Empty;
            }

            var separatorIndex = category.IndexOf('.');
            return separatorIndex > 0 ? category.Substring(0, separatorIndex) : category;
        }

        private static List<(string Root, string Name)> BuildPackageRootIndex(IEnumerable<Package> packages)
        {
            var roots = new List<(string Root, string Name)>();
            foreach (var package in packages)
            {
                if (string.IsNullOrWhiteSpace(package?.RootDirectory))
                {
                    continue;
                }

                roots.Add((NormalizeDirectory(package.RootDirectory), package.Name));
            }

            return roots;
        }

        private static PackageAssemblyLookup BuildPackageAssemblyLookup(IEnumerable<Package> packages)
        {
            var byPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var byFileName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var package in packages)
            {
                if (package?.LoadedAssemblies == null)
                {
                    continue;
                }

                foreach (var assembly in package.LoadedAssemblies)
                {
                    var localPath = assembly?.LocalFilePath;
                    if (string.IsNullOrWhiteSpace(localPath))
                    {
                        continue;
                    }

                    var fullPath = Path.GetFullPath(localPath);
                    if (!byPath.ContainsKey(fullPath))
                    {
                        byPath.Add(fullPath, package.Name);
                    }

                    var fileName = Path.GetFileName(fullPath);
                    if (!string.IsNullOrWhiteSpace(fileName) && !byFileName.ContainsKey(fileName))
                    {
                        byFileName.Add(fileName, package.Name);
                    }
                }
            }

            return new PackageAssemblyLookup(byPath, byFileName);
        }

        private static string ResolvePackageName(NodeSearchElement entry, List<(string Root, string Name)> packageRoots, PackageAssemblyLookup packageAssemblies)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            if (!entry.ElementType.HasFlag(ElementTypes.Packaged))
            {
                return string.Empty;
            }

            var path = GetEntryPath(entry);
            if (!string.IsNullOrWhiteSpace(path) && packageRoots != null && packageRoots.Count > 0)
            {
                var fullPath = Path.GetFullPath(path);
                foreach (var root in packageRoots)
                {
                    if (fullPath.StartsWith(root.Root, StringComparison.OrdinalIgnoreCase))
                    {
                        return root.Name ?? string.Empty;
                    }
                }
            }

            if (packageAssemblies != null)
            {
                var assemblyPath = entry.Assembly ?? string.Empty;
                if (Path.IsPathRooted(assemblyPath))
                {
                    var fullAssemblyPath = Path.GetFullPath(assemblyPath);
                    if (packageAssemblies.ByPath.TryGetValue(fullAssemblyPath, out var packageName))
                    {
                        return packageName ?? string.Empty;
                    }
                }

                var assemblyFileName = Path.GetFileName(assemblyPath);
                if (!string.IsNullOrWhiteSpace(assemblyFileName) &&
                    packageAssemblies.ByFileName.TryGetValue(assemblyFileName, out var packageByFileName))
                {
                    return packageByFileName ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static string GetEntryPath(NodeSearchElement entry)
        {
            if (entry is CustomNodeSearchElement customNode && !string.IsNullOrWhiteSpace(customNode.Path))
            {
                return customNode.Path;
            }

            if (!string.IsNullOrWhiteSpace(entry.Assembly) && Path.IsPathRooted(entry.Assembly))
            {
                return entry.Assembly;
            }

            return string.Empty;
        }

        private static string NormalizeDirectory(string path)
        {
            var fullPath = Path.GetFullPath(path);
            if (!fullPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                fullPath += Path.DirectorySeparatorChar;
            }

            return fullPath;
        }

        private sealed class PackageAssemblyLookup
        {
            internal PackageAssemblyLookup(Dictionary<string, string> byPath, Dictionary<string, string> byFileName)
            {
                ByPath = byPath ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                ByFileName = byFileName ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            internal Dictionary<string, string> ByPath { get; }
            internal Dictionary<string, string> ByFileName { get; }
        }

        private void MenuItemUnCheckedHandler(object sender, RoutedEventArgs e)
        {
            viewLoadedParamsReference.CloseExtensioninInSideBar(this);
        }

        private void MenuItemCheckHandler(object sender, RoutedEventArgs e)
        {
            AddToSidebar(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            if (this.viewLoadedParamsReference != null)
            {
                this.viewLoadedParamsReference.RequestOpenDocumentationLink -= HandleRequestOpenDocumentationLink;
                this.viewLoadedParamsReference.NodeHelpAuditRequested -= OnNodeHelpAuditRequested;
            }

            if (this.ViewModel != null)
            {
                this.ViewModel.MessageLogged -= OnViewModelMessageLogged;
                this.ViewModel.HandleInsertFile -= OnInsertFile;
            }

            if (this.documentationBrowserMenuItem != null)
            {
                this.documentationBrowserMenuItem.Checked -= MenuItemCheckHandler;
                this.documentationBrowserMenuItem.Unchecked -= MenuItemUnCheckedHandler;
            }

            this.BrowserView?.Dispose();
            this.ViewModel?.Dispose();

            if (this.viewLoadedParamsReference != null)
            {
                (this.viewLoadedParamsReference.DynamoWindow.DataContext as DynamoViewModel).PropertyChanged -=
                    HandleStartPageVisibilityChange;
            }

            if (this.pmExtension != null)
            {
                this.pmExtension.PackageLoader.PackgeLoaded -= OnPackageLoaded;
            }

            PackageDocumentationManager.Instance.MessageLogged -= OnMessageLogged;
            PackageDocumentationManager.Instance.Dispose();
        }

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// This method handles the documentation open requests coming from Dynamo.
        /// The incoming request is routed to the ViewModel for processing.
        /// </summary>
        /// <param name="args">The incoming event data.</param>
        public void HandleRequestOpenDocumentationLink(OpenDocumentationLinkEventArgs args)
        {
            if (args == null) return;

            // ignore events targeting remote resources so the sidebar is not displayed
            if (args.IsRemoteResource)
                return;

            // make sure the breadcrumbs dictionary has been loaded
            RequestLoadLayoutSpecs();

            // make sure the view is added to the Sidebar
            // this also forces the Sidebar to open
            AddToSidebar(false);

            // forward the event to the ViewModel to handle
            this.ViewModel?.HandleOpenDocumentationLinkEvent(args);

            // Check the menu item
            this.documentationBrowserMenuItem.IsChecked = true;
        }

        private void OnViewModelMessageLogged(ILogMessage msg)
        {
            OnMessageLogged(msg);
        }

        private void AddToSidebar(bool displayDefaultContent)
        {
            // verify the browser window has been initialized
            if (this.BrowserView == null)
            {
                OnMessageLogged(LogMessage.Error(Resources.BrowserViewCannotBeAddedToSidebar));
                return;
            }

            // make sure the documentation window is not empty before displaying it
            // we have to do this here because we cannot detect when the sidebar is displayed
            if (displayDefaultContent)
            {
                this.ViewModel?.EnsurePageHasContent();
            }

            this.viewLoadedParamsReference?.AddToExtensionsSideBar(this, this.BrowserView);
        }

        // hide browser directly when startpage is shown to deal with air space problem.
        // https://github.com/dotnet/wpf/issues/152
        private void HandleStartPageVisibilityChange(object sender, PropertyChangedEventArgs e)
        {
            DynamoViewModel dynamoViewModel = sender as DynamoViewModel;

            if (dynamoViewModel != null && e.PropertyName == nameof(DynamoViewModel.ShowStartPage))
            {
                ViewModel.ShowBrowser = !dynamoViewModel.ShowStartPage;
            }
        }

        public override void Closed()
        {
            if (this.documentationBrowserMenuItem != null)
            {
                this.documentationBrowserMenuItem.IsChecked = false;
            }
        }
    }
}
