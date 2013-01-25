using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Elements;
using Dynamo.Controls;
using System.Windows;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace DynamoSandbox
{
    class Program
    {
        static dynSandbox sandbox;

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            //SplashScreen splashScreen = null;
            //splashScreen = new SplashScreen(Assembly.GetExecutingAssembly(), "splash.png");
            //splashScreen.Show(false, true);

            try
            {
                //show the window
                sandbox = new dynSandbox();
                sandbox.ShowDialog();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }

            //dynamoForm.Closed += new EventHandler(dynamoForm_Closed);
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Debug.WriteLine(args.Name);
            
            if (args.Name == "RevitAPI, Version=2013.0.0.0, Culture=neutral, PublicKeyToken=null")
            {
                return Assembly.LoadFrom(@"C:\Program Files\Autodesk\Revit Architecture 2013\Program\RevitAPI.dll");
            }

            return null;
        }
    }
}
