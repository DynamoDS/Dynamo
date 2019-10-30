using Dynamo.Core;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dynamo.DocumentationBrowser
{
    public class DocumentationBrowserViewModel : NotificationObject, IDisposable
    {
        private string linkString;
        public string LinkString
        {
            get { return linkString; }
            private set { linkString = value; OnLinkedChanged(value); }  
        }

        internal static Action<string> LinkChanged;
        private static void OnLinkedChanged(string link) => LinkChanged?.Invoke(link);


        public DocumentationBrowserViewModel()
        {
        }

        public void DocumentationLink(string link)
        {
            link = "C:/Users/matterlab_sylvester/Downloads/ErrorOperationFailed.html";
            string strContent = File.ReadAllText(link);
            LinkString = strContent;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
