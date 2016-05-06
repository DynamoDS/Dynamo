using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.PluginManager.Model
{
    class Plugin
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string ShortcutKey { get; set; }
    }
    class PluginManagerModel
    {
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

        }
    }
}
