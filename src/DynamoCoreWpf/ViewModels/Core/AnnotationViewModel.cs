﻿using System.Windows.Media;
using Dynamo.Models;
using System;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Dynamo.Views;
using Color = System.Windows.Media.Color;

namespace Dynamo.ViewModels
{
    public class AnnotationViewModel : ViewModelBase
    {
        private AnnotationModel annotationModel;
        public readonly WorkspaceViewModel WorkspaceViewModel;        
        private double zIndex = 2;
        
        public AnnotationModel AnnotationModel
        {
            get { return annotationModel; }
            set
            {
                annotationModel = value;
                RaisePropertyChanged("AnnotationModel");
            }
        }

        public Double Width
        {
            get { return annotationModel.Width; }
        }

        public Double Height
        {
            get { return annotationModel.Height; }
            set
            {
                annotationModel.Height = value;              
            }
        }

        public Double Top
        {
            get { return annotationModel.Y; }
            set
            {
                annotationModel.Y = value;                
            }
        }
       
        public Double Left
        {
            get { return annotationModel.X; }
            set { annotationModel.X = value; }
        }

        public double ZIndex
        {
            get { return zIndex; }
            set
            {
                zIndex = value;
                RaisePropertyChanged("ZIndex");
            }
        }

        public String AnnotationText
        {
            get { return annotationModel.AnnotationText; }
            set
            {
                annotationModel.AnnotationText = value;                
            }
        }
       
        private Color _background;
        public Color Background
        {
            get
            {
                var solidColorBrush =
                    (SolidColorBrush)
                        new BrushConverter().ConvertFromString(annotationModel.Background);
                if (solidColorBrush != null) _background = solidColorBrush.Color;
                return _background;
            }
            set
            {
                annotationModel.Background = value.ToString();                
            }
        }
        
        public PreviewState PreviewState
        {
            get
            {               
                if (annotationModel.IsSelected)
                {
                    return PreviewState.Selection;
                }

                return PreviewState.None;
            }
        }

        private DelegateCommand _changeFontSize;
        public DelegateCommand ChangeFontSize
        {
            get
            {
                if (_changeFontSize == null)
                    _changeFontSize =
                        new DelegateCommand(UpdateFontSize, CanChangeFontSize);

                return _changeFontSize;
            }
        }
      
        public Double FontSize
        {
            get
            {
                return annotationModel.FontSize;
            }
            set
            {
                annotationModel.FontSize = value;                
            }
        }
       
        public AnnotationViewModel(WorkspaceViewModel workspaceViewModel, AnnotationModel model)
        {            
            annotationModel = model;           
            this.WorkspaceViewModel = workspaceViewModel;                                     
            model.PropertyChanged += model_PropertyChanged;
            // Group is created already.So just populate it.
            var selectNothing = new DynamoModel.SelectModelCommand(Guid.Empty, System.Windows.Input.ModifierKeys.None.AsDynamoType());
            WorkspaceViewModel.DynamoViewModel.ExecuteCommand(selectNothing);
        }

        private bool CanChangeFontSize(object obj)
        {
            return true;
        }

        private void UpdateFontSize(object parameter)
        {
            if (parameter != null)
            {
                FontSize = Convert.ToDouble(parameter);
            }
        }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "X":
                    RaisePropertyChanged("Left");
                    break;
                case "Y":
                    RaisePropertyChanged("Top");
                    break;
                case "Width":
                    RaisePropertyChanged("Width");
                    break;
                case "Height":
                    RaisePropertyChanged("Height");
                    break;
                case "AnnotationText":
                    RaisePropertyChanged("AnnotationText");
                    break;
                case "Background":
                    RaisePropertyChanged("Background");
                    break;                              
                case "IsSelected":
                    RaisePropertyChanged("PreviewState");
                    break;
                case "FontSize":
                    RaisePropertyChanged("FontSize");
                    break;
            }
        }      
    }
}
