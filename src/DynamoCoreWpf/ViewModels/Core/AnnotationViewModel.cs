using System.Data;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Dynamo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Utilities;
using Color = System.Windows.Media.Color;

namespace Dynamo.ViewModels
{
    public class AnnotationViewModel : ViewModelBase
    {
        private AnnotationModel _annotationModel;
        public readonly WorkspaceViewModel WorkspaceViewModel;
        private double _zIndex = 2;

        public AnnotationModel AnnotationModel
        {
            get { return _annotationModel; }
            set
            {
                _annotationModel = value;
                RaisePropertyChanged("AnnotationModel");
            }
        }

        public Double Width
        {
            get { return _annotationModel.Width; }
        }

        public Double Height
        {
            get { return _annotationModel.Height; }
        }

        public Double Top
        {
            get { return _annotationModel.Top; }
            set { _annotationModel.Top = value; }
        }
       
        public Double Left
        {
            get { return _annotationModel.Left; }
            set { _annotationModel.Left = value; }
        }

        public double ZIndex
        {
            get { return _zIndex; }
            set
            {
                _zIndex = value;
                RaisePropertyChanged("ZIndex");
            }
        }

        public String AnnotationText
        {
            get { return _annotationModel.AnnotationText; }
            set
            {
                _annotationModel.AnnotationText = value;                
            }
        }
       
        private int _textBoxWidth;
        public double TextBoxHeight
        {
            get { return this.Height; }
        }

        private Color _backGroundColor;
        public Color BackGroundColor
        {
            get
            {
                var solidColorBrush =
                    (SolidColorBrush)
                        new BrushConverter().ConvertFromString(_annotationModel.BackGroundColor);
                if (solidColorBrush != null) _backGroundColor = solidColorBrush.Color;
                return _backGroundColor;
            }
            set
            {
                _annotationModel.BackGroundColor = value.ToString();                
            }
        }

        public Rect2D RectRegion
        {
            get { return _annotationModel.RectRegion; }
        }

        public IEnumerable<NodeModel> SelectedNodes
        {
            get { return _annotationModel.SelectedNodes; }
        }

        public bool IsInDrag
        {
            get { return _annotationModel.IsInDrag; }
            set { _annotationModel.IsInDrag = value; }
        }

        public AnnotationViewModel(WorkspaceViewModel workspaceViewModel, AnnotationModel model)
        {            
            _annotationModel = model;           
            this.WorkspaceViewModel = workspaceViewModel;            
            this.IsInDrag = false;                
            model.PropertyChanged += model_PropertyChanged;
        }
       

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Left":
                    RaisePropertyChanged("Left");
                    break;
                case "Top":
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
                case "BackGroundColor":
                    RaisePropertyChanged("BackGroundColor");
                    break;
                case "SelectedNodes":
                    RaisePropertyChanged("SelectedNodes");
                    break; 
                case "RectRegion":
                    RaisePropertyChanged("RectRegion");
                    break;
            }
        }      
    }
}
