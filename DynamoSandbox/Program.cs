using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Elements;
using Dynamo.Controls;
using System.Windows;
using System.Windows.Forms;
using System.Reflection;

namespace DynamoSandbox
{
    class Program
    {
        static dynBench dynamoForm;

        static void Main(string[] args)
        {
            SplashScreen splashScreen = null;
            splashScreen = new SplashScreen(Assembly.GetExecutingAssembly(), "splash.png");
            splashScreen.Show(false, true);

            //show the window
            //dynamoForm = new dynBench(null, splashScreen);
            //dynamoForm.Show();

            //dynamoForm.Closed += new EventHandler(dynamoForm_Closed);
        }
    }
}
