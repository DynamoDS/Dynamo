using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Dynamo.Wpf.ViewModels.Core
{
    public class GalleryContent : NotificationObject
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

    public class GalleryViewModel: ViewModelBase
    {
        #region public members
        public string CurrentImage { get { return currentContent.Image; } }
        public string CurrentHeader { get { return currentContent.Header; } }
        public string CurrentBody { get { return currentContent.Body; } }
        public List<GalleryContent> Contents { get { return contents; } }
        public DelegateCommand MoveNextCommand { get; set; }
        public DelegateCommand MovePrevCommand { get; set; }
        #endregion

        public GalleryViewModel(DynamoViewModel dynamoViewModel) 
        {          
            IPathManager pathManager = dynamoViewModel.Model.PathManager;
            var galleryFilePath = pathManager.GalleryFilePath;

            if (File.Exists(galleryFilePath))
            {
                contents = GalleryContents.Load(galleryFilePath).GalleryUIContents;
            }

            currentContent = new GalleryContent();
            if (contents != null && contents[0] != null)
            {
                currentContent.Image = contents[0].Image;
                currentContent.Header = contents[0].Header;
                currentContent.Body = contents[0].Body;
                contents[0].IsCurrent = true;
                MoveNextCommand = new DelegateCommand(MoveNext, CanMoveNext);
                MovePrevCommand = new DelegateCommand(MovePrev, CanMovePrev);
            }
        }

        public void MoveNext(object parameters)
        {
            MoveIndex(true);
        }

        internal bool CanMoveNext(object parameters)
        {
            return true;
        }

        public void MovePrev(object parameters)
        {
            MoveIndex(false);
        }

        internal bool CanMovePrev(object parameters)
        {
            return true;
        }

        /// <summary>
        /// Move the currentIndex of the Gallery Bullets
        /// </summary>
        /// <param name="forward">
        /// false for moving to the left
        /// true for moving to the right
        /// </param>
        private void MoveIndex(bool forward)
        {
            contents[currentIndex].IsCurrent = false;
            currentIndex = (currentIndex + (forward? 1:-1) + contents.Count) % (contents.Count);

            contents[currentIndex].IsCurrent = true;
            currentContent.Image = contents[currentIndex].Image;
            currentContent.Header = contents[currentIndex].Header;
            currentContent.Body = contents[currentIndex].Body;
            
            RaisePropertyChanged("CurrentImage");
            RaisePropertyChanged("CurrentHeader");
            RaisePropertyChanged("CurrentBody");
        }

        #region private fields
        private GalleryContent currentContent;
        private List<GalleryContent> contents;
        private int currentIndex = 0;
        #endregion
    }
}
