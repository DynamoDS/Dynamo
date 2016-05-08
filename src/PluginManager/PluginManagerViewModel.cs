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

namespace Dynamo.PluginManager.ViewModel
{

    public class PluginManagerViewModel : NotificationObject
    {

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

        private string PluginPreferenceFile;
        private string PluginFolder;
        public ObservableCollection<PluginModel> PluginModelList { get; private set; }
        public PluginManagerViewModel()
        {
            PluginModelList = new ObservableCollection<PluginModel>();
            // ImportPlugins();
            TempPopulateList();
            RemovePluginCommand = new DelegateCommand(p => RemovePluginAt((int)p), CanRemove);
            AddPluginCommand = new DelegateCommand(p => AddPlugin());
        }
        private void AddPlugin()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Python files (*.py)|*.py";
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
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(String.Format(ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning));
                }
            }
        }
        private void RemovePluginAt(int index)
        {
            PluginModelList.RemoveAt(index);
            if (index <= SelectedIndex && SelectedIndex > 0)
            {
                SelectedIndex--;
            }
            //  RaiseCanExecuteChanged();
        }
        private bool CanRemove(object param)
        {
            return PluginModelList.Count > 1;
        }
        private void TempPopulateList()
        {
            var appDatafolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var PluginFolder = Path.Combine(appDatafolder, "Dynamo", "Plugins");
            var PluginPreferenceFile = Path.Combine(appDatafolder, "PluginPreference.xml");
            string[] fileList = Directory.GetFiles(PluginFolder);
            for (int i = 0; i < fileList.Count(); i++)
            {
                PluginModelList.Add(new PluginModel(fileList[i], null));
            }
        }
        private void ImportPlugins()
        {
            var appDatafolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var PluginFolder = Path.Combine(appDatafolder, "Dynamo", "Plugins");
            var PluginPreferenceFile = Path.Combine(appDatafolder, "PluginPreference.xml");
            if (!Directory.Exists(PluginFolder))
            {
                System.IO.Directory.CreateDirectory(PluginFolder);
                XmlDocument doc = new XmlDocument();
                XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(docNode);

                doc.Save(PluginPreferenceFile);
                //System.IO.File.Create(PluginPreferenceFile);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(PluginPreferenceFile);
                // foreach


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
