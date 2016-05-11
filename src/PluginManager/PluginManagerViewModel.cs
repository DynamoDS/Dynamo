using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
using System.Collections.ObjectModel;
using Dynamo.PluginManager.Model;
using System.IO;
using System.Xml;
using Dynamo.UI.Commands;
using System.Windows.Forms;
using PluginManager;

namespace Dynamo.PluginManager.ViewModel
{

    public class PluginManagerViewModel : NotificationObject
    {
        private PluginManagerExtension pluginManagerContext;
        public DelegateCommand EditShortcutKeyCommand { get; private set; }
        public DelegateCommand RunScriptCommand { get; private set; }
        public DelegateCommand RemovePluginCommand { get; private set; }
        public DelegateCommand AddPluginCommand { get; private set; }
        private int selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
                //  RaiseCanExecuteChanged();
            }
        }

        private string PluginPreferenceFilePath;
        private string PluginFolderPath;
        public ObservableCollection<PluginModel> PluginModelList { get; private set; }
        public PluginManagerViewModel(PluginManagerExtension pluginManagerContext)
        {
            this.pluginManagerContext = pluginManagerContext;
            PluginModelList = new ObservableCollection<PluginModel>();
            
            //TempPopulateList();
            RunScriptCommand = new DelegateCommand(p => RunScript((string)p));
            RemovePluginCommand = new DelegateCommand(p => RemovePluginAt((int)p), CanRemove);
            AddPluginCommand = new DelegateCommand(p => AddPlugin());
            EditShortcutKeyCommand = new DelegateCommand(p => EditShortcutKey((int)p), CanEditShortcutKey);
           // ImportPlugins();

        }
        private bool CanEditShortcutKey(object param)
        {
            return (PluginModelList.Count > 0);
        }
        private void EditShortcutKey(int index)
        {
            //TODO: Validity test for the shortcut && add property firing
                string newShortCutKey = TextBoxPromptDialog.ShowDialog("Enter Shortcut key(e.g Ctrl+A)", "Shortcut Key");
            pluginManagerContext.ChangeShortcutKey(PluginModelList.ElementAt(index), newShortCutKey);
            PluginModelList.ElementAt(index).ShortcutKey = newShortCutKey;
            UpdatePluginManagerPreferenceXMLFile();
        }
        private void RunScript(string filePath)
        {
            PluginManagerIronPythonEvaluator.EvaluatePythonFile(filePath, pluginManagerContext);
        }
        private void AddPlugin()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Python files (*.py)|*.py|All Files(*.*)|*.*";
            openFileDialog.Title = "Import Python File";
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = true;

            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    foreach (var file in openFileDialog.FileNames)
                    {
                        PluginModelList.Add(new PluginModel(file, null));
                        pluginManagerContext.AddPluginMenuItem(new PluginModel(file, null));
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(String.Format(ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning));
                }
            }
            UpdatePluginManagerPreferenceXMLFile();
        }
        private void RemovePluginAt(int index)
        {
            pluginManagerContext.RemovePluginMenuItem(PluginModelList.ElementAt(SelectedIndex));
            PluginModelList.RemoveAt(index);
            if (index <= SelectedIndex && SelectedIndex > 0)
            {
                SelectedIndex--;
            }
            UpdatePluginManagerPreferenceXMLFile();
        }
        private bool CanRemove(object param)
        {
            return PluginModelList.Count > 1;
        }
        private void TempPopulateList()
        {
            var appDatafolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
             PluginFolderPath = Path.Combine(appDatafolder, "Dynamo", "Plugins");
            PluginPreferenceFilePath = Path.Combine(appDatafolder, "PluginPreference.xml");
            string[] fileList = Directory.GetFiles(PluginFolderPath);
            for (int i = 0; i < fileList.Count(); i++)
            {
                PluginModelList.Add(new PluginModel(fileList[i], null));
            }
        }
       internal void ImportPlugins()
        {
            //TODO:Shift this import to separate class
            var appDatafolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            PluginFolderPath = Path.Combine(appDatafolder, "Dynamo","Dynamo Core", "Plugins");
             PluginPreferenceFilePath = Path.Combine(appDatafolder,PluginFolderPath, "PluginPreference.xml");
            if (!Directory.Exists(PluginFolderPath))
            {
                System.IO.Directory.CreateDirectory(PluginFolderPath);
                /*XmlDocument doc = new XmlDocument();
                XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(docNode);

                doc.Save(PluginPreferenceFilePath);
                System.IO.File.Create(PluginPreferenceFilePath);*/
            }
            else
            {
                ReadPluginManagerPreferenceXMLFile();
                foreach (PluginModel model in PluginModelList)
                {
                    pluginManagerContext.AddPluginMenuItem(model);
                }
            }
        }
        private void UpdatePluginManagerPreferenceXMLFile()
        {
            System.IO.File.WriteAllText(PluginPreferenceFilePath, string.Empty);
            FileStream fileStream = new FileStream(PluginPreferenceFilePath,FileMode.OpenOrCreate);
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ObservableCollection<PluginModel>));
            serializer.Serialize(fileStream, PluginModelList);
            fileStream.Close();

        }
        private void ReadPluginManagerPreferenceXMLFile()
        {
            if (File.Exists(PluginPreferenceFilePath))
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ObservableCollection<PluginModel>));
                StreamReader reader = new StreamReader(PluginPreferenceFilePath);
                PluginModelList = (ObservableCollection<PluginModel>)serializer.Deserialize(reader);
                reader.Close();
            }

        }

        /* 
          if (!Directory.Exists(pluginFolder))
          {
              System.IO.Directory.CreateDirectory(pluginFolder);
          }
          else
          {
              string[] files = Directory.GetDirectories(pluginFolder);
              for(int i =0; i< files.Count(); i++)
              {
                  if (Path.GetExtension(files[i]).Equals(".py"))
                  {

                  }
              }
          }*/


    }
}
