using Dynamo.Core;
using Dynamo.ViewModels;
using System;
using System.IO;
using System.Windows;

namespace Dynamo.DocumentationBrowser
{
    public class DocumentationBrowserViewModel : NotificationObject, IDisposable
    {
        private Uri link;
        public Uri Link
        {
            get { return this.link; }
            private set
            {
                this.link = value;
                OnLinkChanged(value);
            }
        }

        public string Content { get; private set; }

        public bool isRemoteResource;
        public bool IsRemoteResource
        {
            get { return this.isRemoteResource; }
            set { this.isRemoteResource = value; this.RaisePropertyChanged(nameof(IsRemoteResource)); }
        }

        internal Action<Uri> LinkChanged;
        private void OnLinkChanged(Uri link) => LinkChanged?.Invoke(link);


        public DocumentationBrowserViewModel()
        {
            this.isRemoteResource = false;
        }

        public void HandleOpenDocumentationLinkEvent(OpenDocumentationLinkEventArgs e)
        {
            this.IsRemoteResource = e.Link.IsFile;
            if (IsRemoteResource) LoadFileContent(e.Link);

            this.Link = e.Link;
        }

        public void LoadFileContent(Uri link)
        {
            try
            {
                if (!File.Exists(link.LocalPath)) throw new FileNotFoundException("Could not find specified documentation file" + link.AbsoluteUri);
                this.Content = File.ReadAllText(link.LocalPath);
            }
            catch(FileNotFoundException e)
            {
                this.Content = e.Message;
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
