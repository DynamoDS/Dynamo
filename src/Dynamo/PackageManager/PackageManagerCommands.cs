using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Dynamo.Utilities;

namespace Dynamo.Commands
{

    public class PackageManagerLoginCommand : ICommand
    {
        public PackageManagerLoginCommand()
        {

        }

        public void Execute(object parameters)
        {
            
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class PackageManagerShowLoginCommand : ICommand
    {
        private bool init;
        private PackageManager.PackageManagerLoginUI ui;

        public PackageManagerShowLoginCommand()
        {
            this.init = false;
        }

        public void Execute(object parameters)
        {
            if (!init)
            {
                ui = dynSettings.Controller.PackageManagerLoginController.View;
                dynSettings.Bench.outerCanvas.Children.Add(ui);
                init = true;
            }

            ui.Visibility = Visibility.Visible;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class PackageManagerGetAvailableCommand : ICommand
    {
        public PackageManagerGetAvailableCommand()
        {

        }

        public void Execute(object parameters)
        {
            Dynamo.Utilities.dynSettings.PackageManagerClient.GetAvailable();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }


    public static partial class DynamoCommands
    {

        private static Dynamo.Commands.PackageManagerGetAvailableCommand getAvailableCmd;

        public static Dynamo.Commands.PackageManagerGetAvailableCommand GetAvailableCmd
        {
            get
            {
                if (getAvailableCmd == null)
                    getAvailableCmd = new Dynamo.Commands.PackageManagerGetAvailableCommand();
                return getAvailableCmd;
            }
        }

        private static Dynamo.Commands.PackageManagerShowLoginCommand packageManagerShowLoginCmd;

        public static Dynamo.Commands.PackageManagerShowLoginCommand PackageManagerShowLoginCmd
        {
            get
            {
                if (packageManagerShowLoginCmd == null)
                    packageManagerShowLoginCmd = new Dynamo.Commands.PackageManagerShowLoginCommand();
                return packageManagerShowLoginCmd;
            }
        }

        private static Dynamo.Commands.PackageManagerLoginCommand loginCmd;

        public static Dynamo.Commands.PackageManagerLoginCommand LoginCmd
        {
            get
            {
                if (loginCmd == null)
                    loginCmd = new Dynamo.Commands.PackageManagerLoginCommand();
                return loginCmd;
            }
        }
    }
}
