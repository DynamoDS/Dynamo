using Dynamo.Core;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dynamo.DocumentationBrowser
{
    public class DocumentationBrowserViewModel : NotificationObject, IDisposable
    {
        private string link;
        public string Link
        {
            get { return link; }
            private set { link = value; OnLinkedChanged(value); }
        }

        public bool isRemoteResource;
        public bool IsRemoteResource
        {
            get { return this.IsRemoteResource; }
            set { this.isRemoteResource = value; this.RaisePropertyChanged(nameof(IsRemoteResource)); }
        }

        internal static Action<string> LinkChanged;
        private static void OnLinkedChanged(string link) => LinkChanged?.Invoke(link);


        public DocumentationBrowserViewModel()
        {
        }

        public void OpenDocumentationLink(string link)
        {
            link = @"C:\Users\radug\metaspace\metaspace-Server - Documents\03 Client Projects\Autodesk\0020 - Dynamo Error Messages\06 Solution\warning_htmls\kPropertyOfClassNotFound.html";
            try
            {
                string strContent = File.ReadAllText(link);
                Link = strContent;
            }
            catch (Exception ex)
            {
                // silently fail
                MessageBox.Show(ex.Message);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
