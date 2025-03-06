using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;

namespace Dynamo.Wpf.Views.Debug
{
    /// <summary>
    /// Interaction logic for UpdateNodeIconsWindow.xaml
    /// </summary>
    public partial class UpdateNodeIconsWindow : Window, INotifyPropertyChanged
    {
        #region properties
        //The path that will be used to store log files related to this process
        private static readonly string logPath = Path.Combine(Directory.GetCurrentDirectory(), @"NodeIconUpdateLog");
        private string errorFile = Path.Combine(logPath, @"error_icons.csv");
        private string logFile = Path.Combine(logPath, @"success_icons.csv");


        IEnumerable<NodeSearchElement> entries;

        public event PropertyChangedEventHandler PropertyChanged;
        internal void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string newIconPaths;
        /// <summary>
        /// Folder(s) containing new icons
        /// </summary>
        public string NewIconPaths
        {
            get { return newIconPaths; }
            set
            {
              if (value != newIconPaths)
              {
                 newIconPaths = value;
                 OnPropertyChanged(nameof(NewIconPaths));
              }
            }
        }
        private string output;
        /// <summary>
        /// The Output log from the process
        /// </summary>
        public string Output
        {
            get { return output; }
            set
            {
                if (value != output)
                {
                    output = value;
                    OnPropertyChanged(nameof(Output));
                }
            }
        }
        private bool isUpdateEnabled;
        /// <summary>
        /// Used to Enable/Disable the Update button
        /// </summary>
        public bool IsUpdateEnabled
        {
            get { return isUpdateEnabled; }
            set
            {
                if (value != isUpdateEnabled)
                {
                    isUpdateEnabled = value;
                    OnPropertyChanged(nameof(IsUpdateEnabled));
                }
            }
        }

        private ObservableCollection<NodeIconMetadata> updatedIconList;
        /// <summary>
        /// Contains all the icons that will be updated, with all information required to update them.
        /// </summary>
        public ObservableCollection<NodeIconMetadata> UpdatedIconList
        {
            get { return updatedIconList; }
            set
            {
                if (value != updatedIconList)
                {
                    updatedIconList = value;
                    OnPropertyChanged(nameof(UpdatedIconList));
                }
            }
        }

        public class NodeIconMetadata
        {
            /// <summary>
            /// New Path of the current icon
            /// </summary>
            public string NewIconPath { get; set; }
            /// <summary>
            /// Node name related to the current icon
            /// </summary>
            public string NodeName { get; set; }
            /// <summary>
            /// Icon name related to the current icon
            /// </summary>
            public string IconName { get; set; }
            /// <summary>
            /// Icon suffix (Small/Large)
            /// </summary>
            public string IconSuffix { get; set; }
            /// <summary>
            /// Base64 string of the current icon
            /// </summary>
            public string Icon_Base64String { get; set; }
            /// <summary>
            /// Resx file containing the current icon
            /// </summary>
            public string IconResxFile { get; set; }
            /// <summary>
            /// Flag to indicate if the icon is updated or added
            /// </summary>
            public bool IsUpdated { get; set; }
            /// <summary>
            /// Old data(Base64) of the current icon, used to display the old image for comparison.
            /// </summary>
            public string OldData { get; set; }
            public NodeIconMetadata(string nodeName, string newIconPath, string iconName, string iconSuffix, string oldData, string resxFile, bool isUpdated = false)
            {
                this.IconSuffix = iconSuffix;
                this.NodeName = nodeName;
                this.NewIconPath = newIconPath;
                this.IconName = iconName;
                this.Icon_Base64String = GetBase64StringForImage(newIconPath);
                this.IsUpdated = isUpdated;
                this.OldData = oldData;
                this.IconResxFile = resxFile;
            }
        }
        #endregion properties

        /// <summary>
        /// Constructor
        /// </summary>
        public UpdateNodeIconsWindow(IEnumerable<NodeSearchElement> entries)
        {
            this.entries = entries;
            InitializeComponent();
            this.DataContext = this;
            UpdatedIconList = new ObservableCollection<NodeIconMetadata>();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Task.Factory.StartNew(() =>
                    ExportNodeIconData(NewIconPaths)
                );
            }
            catch(Exception ex)
            {
                WritetoLogFile(errorFile,"Error: " + ex.Message);
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void OnUpdateClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Task.Factory.StartNew(() =>
                    UpdateNodeIcons()
                );
            }
            catch (Exception ex)
            {
                WritetoLogFile(errorFile, "Error: " + ex.Message);
            }
        }

        #region Icon Review Helper Methods
        private void ExportNodeIconData(string newIconPaths)
        {
            if (string.IsNullOrEmpty(newIconPaths))
            {
                Output = "Please provide the path(s) to the new icons (comma-separated, if multiple)";
                return;
            }
            Output = string.Empty;
            var nip = newIconPaths.Split(',');
            foreach (var path in nip)
            {
                if (!Directory.Exists(path))
                {
                    Output = "Invalid path: " + path;
                    return;
                }
                Output += "Validated: " + path + Environment.NewLine;
            }
            //create a directory if not exists to store the node icon info
            
            if (Directory.Exists(logPath))
            {
                Directory.Delete(logPath, true);
            }
            Directory.CreateDirectory(logPath);
            var dynamoDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\", "src"));
            Dictionary<string, int> count = new Dictionary<string, int>() { { "success", 0 }, { "fail", 0 }, { "update", 0 }, { "add", 0 } };

            Output += "Loading icons and resources...";
            Dictionary<string, string> newIconFiles = GetAllFiles(nip, "*.png");
            Output += "processing " + newIconFiles.Count() + " icons found." + Environment.NewLine;
            Dictionary<string, string> resxFiles = GetAllFiles(new[] { dynamoDir }, "*Images.resx");
            Dictionary<string, Dictionary<string, NodeIconMetadata>> nodeIconInfo = new Dictionary<string, Dictionary<string, NodeIconMetadata>>();

            foreach (var newIcon in newIconFiles)
            {
                var imgName = newIcon.Key.ToLower().Split('_')[0];
                var node = FindNodeFromIconImage(imgName);
                if (node != null)
                {
                    var assemblyName = Path.GetFileName(node.Assembly);
                    var nodeName = node.FullName;
                    var iconName = node.IconName;
                    bool updated = false;
                    string iconPath = newIcon.Value;
                    var iconSuffix = Path.GetFileNameWithoutExtension(iconPath).ToLower().EndsWith("small") ? "Small" : "Large";
                    string oldData = null;
                    string resxFile = string.Empty;

                    //lets check if updating the icon or adding it
                    if (resxFiles.ContainsKey(Path.GetFileNameWithoutExtension(assemblyName)))
                    {
                        //get the resx file for this assembly
                        resxFile = resxFiles[Path.GetFileNameWithoutExtension(assemblyName)];
                        updated = DoesIconExistInResxFile(resxFile, iconName, iconSuffix, out oldData);
                        if (updated) count["update"]++;
                        else count["add"]++;
                    }
                    if (string.IsNullOrEmpty(resxFile))
                    {
                        WritetoLogFile(errorFile, "No resx file found for " + iconPath);
                        count["fail"]++;
                        continue;
                    }

                    //add the new icon under the assembly recorded
                    if (nodeIconInfo.ContainsKey(assemblyName))
                    {
                        nodeIconInfo[assemblyName].Add(iconPath, new NodeIconMetadata(nodeName, iconPath, iconName, iconSuffix, oldData, resxFile, updated));
                    }
                    else
                    {
                        nodeIconInfo.Add(assemblyName, new Dictionary<string, NodeIconMetadata>() { { iconPath, new NodeIconMetadata(nodeName, iconPath, iconName, iconSuffix, oldData, resxFile, updated) } });
                    }
                    WritetoLogFile(logFile, "Processed: " + newIcon.Value);
                    count["success"]++;
                }
                else
                {
                    WritetoLogFile(errorFile, newIcon.Value);
                    count["fail"]++;
                }
            }
            Output += "Processing Complete!" + Environment.NewLine;
            if (count["success"] > 0) { IsUpdateEnabled = true; }
            if (count["fail"] > 0) { Output += count["fail"] + " icons could not be processed, check logs for details." + Environment.NewLine; }

            Output += "Successfully processed: " + count["success"] + " | " + "Icons failed to process: " + count["fail"] + Environment.NewLine +
                "To be updated: " + count["update"] + " | " + "To be added: " + count["add"] + Environment.NewLine +
                "Log File: " + logPath;
            UpdatedIconList = GetUpdatedIcons(nodeIconInfo);
        }

        private ObservableCollection<NodeIconMetadata> GetUpdatedIcons(Dictionary<string, Dictionary<string, NodeIconMetadata>> result)
        {
            var ll = result.Values.ToList();
            var l2 = ll.Select(x=> x.Values.ToList());
            return l2.SelectMany(x => x).ToObservableCollection();
        }
        private void WritetoLogFile(string logFile, string log)
        {
            using (StreamWriter outputFile = new StreamWriter(logFile, true))
            {
                outputFile.WriteLine(log);
            }
        }

        private NodeSearchElement FindNodeFromIconImage(string imgName)
        {
            var node = entries.FirstOrDefault(e => e.FullName.ToLower().Equals(imgName));
            imgName = string.Join(".", imgName.Split('.').Skip(1).ToArray());// because icon name does not include assembly name
            node = node ?? entries.FirstOrDefault(e => e.IconName.ToLower().Equals(imgName));
            return node;
        }

        private Dictionary<string, string> GetAllFiles(string[] paths, string searchPattern)
        {
            Dictionary<string, string> newfiles = new Dictionary<string, string>();
            try
            {
                foreach (var path in paths)
                {
                    string docPath = path;

                    var files = Directory.EnumerateFiles(docPath, searchPattern, SearchOption.AllDirectories);

                    if (searchPattern.Contains("png"))
                    {
                        foreach (var file in files)
                        {
                            var f = Path.GetFileNameWithoutExtension(file);
                            if (!f.ToLower().EndsWith("small") && !f.ToLower().EndsWith("large"))
                            {

                                throw new Exception("Invalid file name");
                            }
                            newfiles.Add(f.ToLower(), file);
                        }
                    }
                    else if (searchPattern.Contains("resx"))
                    {
                        foreach (var file in files)
                        {
                            var f = Path.GetFileNameWithoutExtension(file);
                            f = f.Replace("Images", "");
                            //UnitsUI resx file has a different name in the assembly
                            if (f.Equals("UnitsUI"))
                            {
                                f = "UnitsNodeModels";
                            }
                            newfiles.Add(f, file);
                        }
                    }
                }
                if (newfiles.Count == 0)
                {
                    throw new Exception("No files found in " + string.Join(",", paths));
                }
                return newfiles;
            }
            catch(Exception ex)
            {
                WritetoLogFile(errorFile, ex.Message);
                Output += "Error getting data: " + ex.Message;
            }
            return null;
        }

        private static string GetBase64StringForImage(string imgPath)
        {
            if (string.IsNullOrEmpty(imgPath)) return null;
            byte[] imageBytes = File.ReadAllBytes(imgPath);
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }

        private bool DoesIconExistInResxFile(string path, string iconName, string iconSuffix, out string data)
        {
            XDocument fromFile = XDocument.Load(path);
            IEnumerable<XElement> resxItems = from el in fromFile.Descendants("data") select el;
            var oldElement = resxItems.Where(x => x.Attribute("name").Value.ToLower().Equals((iconName + "." + iconSuffix).ToLower())).FirstOrDefault();
            if (oldElement != null)
            {
                string resXValue = oldElement.Descendants("value").FirstOrDefault().Value;
                //This validation is for an edge case in which the resource contains a path to the image instead of the base64 value
                if (resXValue.ToLower().Contains(".png") || resXValue.ToLower().Contains(".jpg"))
                {
                    var imageName = resXValue.Split(";")[0];
                    var imageFullPath = Path.Combine(Path.GetDirectoryName(path), imageName);
                    if(File.Exists(imageFullPath))
                    {
                        byte[] imageBytes = File.ReadAllBytes(imageFullPath);
                        data = Convert.ToBase64String(imageBytes);
                    }
                    else
                    {
                        data = null;
                        return false;
                    }                  
                }
                else
                {
                    data = resXValue;
                }             
                return true;
            }
            data = null;
            return false;
        }
        #endregion Icon Review Helper Methods

        #region Icon Update Helper Methods

        private string IndentBase64String(string text, int lineLength = 80)
        {
            return Environment.NewLine+ "\t\t\t" + Regex.Replace(text, "(.{" + lineLength + "})", "$1" + Environment.NewLine + "\t\t\t") + Environment.NewLine;
        }
        private void UpdateNodeIcons()
        {
            if (UpdatedIconList == null || UpdatedIconList.Count() == 0) return;
            Output += Environment.NewLine + "Updating..." + Environment.NewLine;
            int ucount = 0;
            int ecount = 0;
            var bkupDir = Directory.CreateDirectory(Path.Combine(logPath, "backup"));
            Dictionary<string, XDocument> resxFiles = new Dictionary<string, XDocument>();
            
            foreach (var item in UpdatedIconList)
            {
                try
                {
                    XDocument resx;
                    if (!resxFiles.TryGetValue(item.IconResxFile, out resx))
                    {
                        resx = XDocument.Load(item.IconResxFile);
                        resxFiles.Add(item.IconResxFile, resx);
                        //create a backup copy of the resx files
                        File.Copy(item.IconResxFile, Path.Combine(bkupDir.FullName, Path.GetFileName(item.IconResxFile)), true);
                    }
                    var update = UpdateResxFile(item, resx);
                    if(update) ucount++;
                    else ecount++;
                }
                catch (Exception ex)
                {
                    WritetoLogFile(errorFile, "Error updating " + item.NewIconPath + " icon: " + ex.Message);
                }
            }
            Output += "Update Complete!" + Environment.NewLine + ucount + " Icons updated | " + ecount + " Icons update failed!" + Environment.NewLine;
        }
        private bool UpdateResxFile(NodeIconMetadata item, XDocument resx)
        {
            try
            {
                if (item.IsUpdated)
                {
                    IEnumerable<XElement> resxItems = from el in resx.Root.Descendants("data") select el;
                    var oldElement = resxItems.Where(x => x.Attribute("name").Value.ToLower().Equals((item.IconName + "." + item.IconSuffix).ToLower())).FirstOrDefault();
                    if (oldElement != null)
                    {
                        var currentElementValue = oldElement.Descendants("value").FirstOrDefault().Value;
                        if (currentElementValue.ToLower().Contains(".png") || currentElementValue.ToLower().Contains(".jpg"))
                        {
                            //If the resx file is referencing a png image then we need to update/add the type and mimetype attributes
                            XAttribute attRemove = oldElement.Attribute("type");
                            attRemove.Remove();
                            oldElement.SetAttributeValue("type", "System.Drawing.Bitmap, System.Drawing");
                            oldElement.SetAttributeValue("mimetype", "application/x-microsoft.net.object.bytearray.base64");
                        }

                        oldElement.Descendants("value").FirstOrDefault().Value = IndentBase64String(item.Icon_Base64String);
                        resx.Save(item.IconResxFile);
                        WritetoLogFile(logFile, "Updated: " + item.NewIconPath);
                        return true;
                    }
                }
                else
                {
                    var data = new XElement("data", new XAttribute("name", item.IconName + "." + item.IconSuffix),
                                           new XAttribute("type", "System.Drawing.Bitmap, System.Drawing"),
                                                              new XAttribute("mimetype", "application/x-microsoft.net.object.bytearray.base64"),
                                                                                     new XElement("value", IndentBase64String(item.Icon_Base64String)));
                    resx.Root.Add(data);
                    resx.Save(item.IconResxFile);
                    WritetoLogFile(logFile, "Added: " + item.NewIconPath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                WritetoLogFile(errorFile, "Error updating " + item.NewIconPath + " icon: " + ex.Message);
            }
            WritetoLogFile(errorFile, "Could not update: " + item.NewIconPath);
            return false;
        }

        #endregion Icon Update Helper Methods

        }
}
