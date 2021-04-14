using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.Wpf.Extensions;
using System.Windows.Media.Imaging;
using Dynamo.GraphMetadata.Controls;

namespace Dynamo.GraphMetadata
{
    public class GraphMetadataViewModel : NotificationObject
    {
        private readonly ViewLoadedParams viewLoadedParams;
        private HomeWorkspaceModel currentWorkspace;

        public string GraphDescription 
        { 
            get { return currentWorkspace.Description; } 
            set { currentWorkspace.Description = value; RaisePropertyChanged(nameof(GraphDescription)); } 
        }

        public string GraphAuthor
        {
            get { return currentWorkspace.Author; }
            set { currentWorkspace.Author = value; RaisePropertyChanged(nameof(GraphAuthor)); }
        }

        public Uri HelpLink
        {
            get { return currentWorkspace.GraphDocumentationURL; }
            set { currentWorkspace.GraphDocumentationURL = value; RaisePropertyChanged(nameof(HelpLink)); }
        }

        public BitmapImage Thumbnail
        {
            get 
            {
                var bitmap = ImageFromBase64(currentWorkspace.Thumbnail);
                return bitmap;
            }
            set
            {
                var base64 = Base64FromImage(value);
                currentWorkspace.Thumbnail = base64;
                RaisePropertyChanged(nameof(Thumbnail));
            }
        }

        public List<CustomPropertyControl> CustomProperties { get; set; }

        public GraphMetadataViewModel(ViewLoadedParams viewLoadedParams)
        {
            this.viewLoadedParams = viewLoadedParams;
            this.currentWorkspace = viewLoadedParams.CurrentWorkspaceModel as HomeWorkspaceModel;

            this.viewLoadedParams.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
        }

        private void OnCurrentWorkspaceChanged(Graph.Workspaces.IWorkspaceModel obj)
        {
            if (!(obj is HomeWorkspaceModel hwm))
                return;

            currentWorkspace = hwm;
            RaisePropertyChanged(nameof(GraphDescription));
            RaisePropertyChanged(nameof(GraphAuthor));
            RaisePropertyChanged(nameof(HelpLink));
            RaisePropertyChanged(nameof(Thumbnail));
        }

        private static BitmapImage ImageFromBase64(string b64string)
        {
            if (string.IsNullOrEmpty(b64string))
                throw new ArgumentException($"'{nameof(b64string)}' cannot be null or empty.", nameof(b64string));

            var bytes = Convert.FromBase64String(b64string);
            BitmapImage bitmapImage = new BitmapImage();

            using (var stream = new MemoryStream(bytes))
            {
                bitmapImage.StreamSource = stream;
            }

            return bitmapImage;
        }

        private static string Base64FromImage(BitmapImage source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }

            return Convert.ToBase64String(data);
        }
    }
}
