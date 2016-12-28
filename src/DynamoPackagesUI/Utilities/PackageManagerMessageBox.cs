using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dynamo.DynamoPackagesUI.Utilities
{
    public class PackageManagerMessageBox : IPackageManagerMessageBox
    {
        public MessageBoxResult Show(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage)
        {
            return MessageBox.Show(message, caption, options, boxImage);
        }

        public MessageBoxResult ShowConfirmToInstallPackage(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage)
        {
            return Show(message, caption, options, boxImage);
        }

        public MessageBoxResult ShowPackageContainPythonScript(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage)
        {
            return Show(message, caption, options, boxImage);
        }

        public MessageBoxResult ShowPackageUseNewerDynamo(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage)
        {
            return Show(message, caption, options, boxImage);
        }

        public MessageBoxResult ShowUninstallToContinue(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage)
        {
            return Show(message, caption, options, boxImage);
        }

        public MessageBoxResult ShowUninstallToContinue2(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage)
        {
            return Show(message, caption, options, boxImage);
        }

        public MessageBoxResult ShowAlreadyInstallDynamo(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage)
        {
            return Show(message, caption, options, boxImage);
        }

        public MessageBoxResult ShowNeedToRestart(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage)
        {
            return Show(message, caption, options, boxImage);
        }

        public MessageBoxResult ShowConfirmToUninstallPackage(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage)
        {
            return Show(message, caption, options, boxImage);
        }

        public MessageBoxResult ShowError(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage)
        {
            return Show(message, caption, options, boxImage);
        }
    }
}
