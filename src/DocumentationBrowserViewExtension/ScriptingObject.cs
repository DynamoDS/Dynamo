using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Dynamo.DocumentationBrowser
{

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ScriptingObject
    {
        private DocumentationBrowserViewModel viewModel;

        public ScriptingObject(DocumentationBrowserViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        /// <summary>
        /// The method takes a string code and will execute a different program based on that code
        /// </summary>
        /// <param name="message"></param>
        public void Notify(string message)
        {
            if (string.Equals(message, "insert"))
            {
                // Insert the graph inside the current worskspace
                this.viewModel.InsertGraph();
                return;
            }
            else if (message.Contains("expandcollapse"))
            {
                var breadCrumbText = message.Split('-')[1];
                this.viewModel.CollapseExpandPackage(breadCrumbText);
                return;
            }
        }
    }
}
