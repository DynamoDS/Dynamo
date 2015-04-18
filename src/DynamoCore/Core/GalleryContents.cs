using Dynamo.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Dynamo
{
    public class GalleryContent:NotificationObject
    {
        private string header;
        public string Header { get; set; }

        private string body;
        public string Body { get; set; }

        private string image;
        public string Image { get; set; }

        private bool isCurrent;
        public bool IsCurrent
        {
            get
            {
                return isCurrent;
            }
            set
            {
                isCurrent = value;
                RaisePropertyChanged("IsCurrent");
            }
        }
    }

    public class GalleryContents
    {
        public List<GalleryContent> GalleryUIContents { get; set; }

        public static GalleryContents Load(string filePath)
        {
            var galleryContents = new GalleryContents();

            if (string.IsNullOrEmpty(filePath) || (!File.Exists(filePath)))
                return galleryContents;

            try
            {
                var serializer = new XmlSerializer(typeof(GalleryContents));
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    galleryContents = serializer.Deserialize(fs) as GalleryContents;
                    fs.Close(); // Release file lock
                }
            }
            catch (Exception) { }

            return galleryContents;
        }
    }
}
