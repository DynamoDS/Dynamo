using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginManager
{
    public class PluginManagerImportScript
    {
        public static void ImportPythonScript()
        {
            //MessageBox.Show("Hello World!");
            //string[] fileFilter = { string.Format("Python Files", "*.py") };//; *.ds" ), string.Format(Resources.FileDialogAssemblyFiles, "*.dll"),
                                                                            //  string.Format(Resources.FileDialogDesignScriptFiles, "*.ds"), string.Format(Resources.FileDialogAllFiles,"*.*")};
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Python files (*.py)|*.py";
            //openFileDialog.Filter = String.Join("|", fileFilter);
            openFileDialog.Title = "Import Python File";
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = true;
            
                        DialogResult result = openFileDialog.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            try
                            {
                             /*   foreach (var file in openFileDialog.FileNames)
                                {
                                    EngineController.ImportLibrary(file);
                                }
                                SearchViewModel.SearchAndUpdateResults();*/
                                foreach(var file in openFileDialog.FileNames)
                               {
                                        PluginManagerIronPythonEvaluator.EvaluatePythonFile(file);
                               }
                            }
                            catch (Exception ex)
                            {
                      
                   System.Windows.MessageBox.Show(String.Format(ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Warning));
                             }   
                        }
        }
    }
}
