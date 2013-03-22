using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Dynamo.Commands
{
    namespace PackageManager
    {
        public class LoginCommand : ICommand
        {
            public LoginCommand()
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

        public class GetAvailableCommand : ICommand
        {
            public GetAvailableCommand()
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
    }

    public static partial class DynamoCommands
    {

        public static class PackageManager
        {
            private static Dynamo.Commands.PackageManager.GetAvailableCommand getAvailableCmd;

            public static Dynamo.Commands.PackageManager.GetAvailableCommand GetAvailableCmd
            {
                get
                {
                    if (getAvailableCmd == null)
                        getAvailableCmd = new Dynamo.Commands.PackageManager.GetAvailableCommand();
                    return getAvailableCmd;
                }
            }

            private static Dynamo.Commands.PackageManager.LoginCommand loginCmd;

            public static Dynamo.Commands.PackageManager.LoginCommand LoginCmd
            {
                get
                {
                    if (loginCmd == null)
                        loginCmd = new Dynamo.Commands.PackageManager.LoginCommand();
                    return loginCmd;
                }
            }
        }
    }
}
