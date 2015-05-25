using Dynamo.Core;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;

namespace Dynamo.Wpf.ViewModels.Core
{
    public class GalleryContent : NotificationObject
    {
        public string Header { get; set; }
        public string Body { get; set; }
        public string ImagePath { get; set; }

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
        public List<GalleryContent> GalleryUiContents { get; set; }

        public GalleryContents()
        {
            GalleryUiContents = new List<GalleryContent>();
        }

        public static GalleryContents Load(string filePath)
        {
            try
            {
                GalleryContents galleryContents;
                var serializer = new XmlSerializer(typeof(GalleryContents));
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    galleryContents = serializer.Deserialize(fs) as GalleryContents;
                    fs.Close(); // Release file lock
                }
                return galleryContents;
            }
            catch (Exception)
            {
                return new GalleryContents();
            }
        }
    }

    public class GalleryViewModel : ViewModelBase
    {
        #region public members
        public string CurrentImagePath { get { return (currentContent == null) ? string.Empty : currentContent.ImagePath; } }
        public string CurrentHeader { get { return (currentContent == null) ? string.Empty : currentContent.Header; } }
        public string CurrentBody { get { return (currentContent == null) ? string.Empty : currentContent.Body; } }
        public string DynamoVersion { get; private set; }
        public IEnumerable<GalleryContent> Contents { get { return contents; } }
        public DelegateCommand MoveNextCommand { get; set; }
        public DelegateCommand MovePrevCommand { get; set; }
        public DelegateCommand CloseGalleryCommand { get; set; }
        #endregion

        public GalleryViewModel(DynamoViewModel dynamoViewModel)
        {
            dvm = dynamoViewModel;
            var pathManager = dynamoViewModel.Model.PathManager;
            var galleryFilePath = pathManager.GalleryFilePath;
            var galleryDirectory = Path.GetDirectoryName(galleryFilePath);

            DynamoVersion = string.Format(Properties.Resources.GalleryDynamoVersion,
                            pathManager.MajorFileVersion,
                            pathManager.MinorFileVersion);

            contents = GalleryContents.Load(galleryFilePath).GalleryUiContents;

            //Set image path relative to gallery Directory
            SetImagePath(galleryDirectory);

            currentContent = contents.FirstOrDefault();
            if (currentContent != null) //if contents is not empty
            {
                currentContent.IsCurrent = true;
            }


            MoveNextCommand = new DelegateCommand(p => MoveIndex(true), o => contents.Count > 1);
            MovePrevCommand = new DelegateCommand(p => MoveIndex(false), o => contents.Count > 1);
            CloseGalleryCommand = new DelegateCommand(p => dvm.CloseGalleryCommand.Execute(null), o => true);
        }

        /// <summary>
        /// Set image path relative to the gallery Directory.
        /// It looks for the images side-by-side with the gallerycontents.xml,
        /// if it's can't be found, it looks one level up in gallery/Data
        /// </summary>
        /// <param name="galleryDirectory">
        /// The directory in which the gallerycontents.xml resides
        /// </param>
        private void SetImagePath(string galleryDirectory)
        {
            foreach (GalleryContent content in contents)
            {
                var imagePath = Path.Combine(galleryDirectory, content.ImagePath);
                if (File.Exists(imagePath))
                {
                    content.ImagePath = imagePath;
                }
                else
                {
                    content.ImagePath = Path.Combine(galleryDirectory, @"..\Data", content.ImagePath);
                }
            }
        }

        /// <summary>
        /// Move the currentIndex of the Gallery Bullets
        /// </summary>
        /// <param name="forward">
        /// true for moving right
        /// false for moving left
        /// </param>
        private void MoveIndex(bool forward)
        {
            contents[currentIndex].IsCurrent = false;
            currentIndex = (currentIndex + (forward ? 1 : -1) + contents.Count) % (contents.Count);
            contents[currentIndex].IsCurrent = true;
            currentContent = contents[currentIndex];

            RaisePropertyChanged("CurrentImagePath");
            RaisePropertyChanged("CurrentHeader");
            RaisePropertyChanged("CurrentBody");
        }

        public bool HasContents { get { return currentContent != null; } }

        #region private fields
        private DynamoViewModel dvm;
        private GalleryContent currentContent;
        private List<GalleryContent> contents;
        private int currentIndex = 0;
        #endregion
    }
}
