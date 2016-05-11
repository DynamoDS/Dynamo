using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Core;

namespace Dynamo.PluginManager.Model
{
    [Serializable]
    public class PluginModel :NotificationObject
    {
        public PluginModel() { }
        public PluginModel(string file, string shortcutKey){
            FilePath = file;
            ShortcutKey = shortcutKey;
            
        }
        public string PluginName{
            get{
                return Path.GetFileNameWithoutExtension(FilePath);
            }
        }
        
        private string filePath;
        public string FilePath {
            get{
            return filePath;
                } set {
             filePath = value;
             RaisePropertyChanged("FilePath");
           }
        }
        private string shortcutKey;
        public string ShortcutKey {
            get
            {
                return shortcutKey;
            }
            set{
                shortcutKey = value;
                RaisePropertyChanged("ShortcutKey");
            }
        }
    }
    class PluginManagerModel
    {/*
        private List<Plugin> PluginsList;
        public PluginManagerModel()
        {
            PluginsList = new List<Plugin>();   
        }
        public void UpdatePluginsList()
        {
            var appDatafolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var pluginFolder = Path.Combine(appDatafolder, "Dynamo\\Plugins");
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
            }
    

        }  * */
    }
}
