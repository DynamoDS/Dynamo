using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dynamo.DynamoPackagesUI.Utilities
{
    public interface IPackageManagerMessageBox
    {
        MessageBoxResult Show(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage);

        MessageBoxResult ShowConfirmToInstallPackage(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage);

        MessageBoxResult ShowPackageContainPythonScript(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage);

        MessageBoxResult ShowPackageUseNewerDynamo(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage);

        MessageBoxResult ShowUninstallToContinue(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage);

        MessageBoxResult ShowUninstallToContinue2(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage);

        MessageBoxResult ShowAlreadyInstallDynamo(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage);

        MessageBoxResult ShowNeedToRestart(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage);

        MessageBoxResult ShowConfirmToUninstallPackage(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage);

        MessageBoxResult ShowError(string message, string caption, MessageBoxButton options, MessageBoxImage boxImage);

    }
}