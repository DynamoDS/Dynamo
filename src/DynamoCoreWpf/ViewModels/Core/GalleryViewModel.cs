using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Dynamo.Wpf.ViewModels.Core
{
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

        public GalleryViewModel(DynamoViewModel dynamoViewModel) { 
            
            contents = dynamoViewModel.Model.GalleryContents.GalleryUIContents;

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
            MoveIndex(1);
        }

        internal bool CanMoveNext(object parameters)
        {
            return true;
        }

        public void MovePrev(object parameters)
        {
            MoveIndex(-1);
        }

        internal bool CanMovePrev(object parameters)
        {
            return true;
        }

        private void MoveIndex(int index)
        {
            contents[currentIndex].IsCurrent = false;

            currentIndex = (currentIndex + index) % (contents.Count);
            
            while(currentIndex < 0)
            {
                currentIndex += contents.Count;
            }

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
