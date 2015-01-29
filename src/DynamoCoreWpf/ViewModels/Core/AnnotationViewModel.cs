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
using Color = System.Windows.Media.Color;

namespace Dynamo.ViewModels
{
    public partial class AnnotationViewModel : ViewModelBase
    {
        private AnnotationModel _annotationModel;
        public readonly WorkspaceViewModel WorkspaceViewModel;

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
        }

        public Double Left
        {
            get { return _annotationModel.Left; }
        }

        public String AnnotationText
        {
            get { return _annotationModel.AnnotationText; }
            set
            {
                _annotationModel.AnnotationText = value;
                RaisePropertyChanged("AnnotationText");
            }
        }

        private Visibility _textBoxVisible;
        public Visibility MakeTextBoxVisible
        {
            get { return _textBoxVisible; }
            set
            {
                _textBoxVisible = value;
                RaisePropertyChanged("MakeTextBoxVisible");
            }
        }

        private Visibility _textBlockVisible;

        public Visibility MakeTextBlockVisible
        {
            get { return _textBlockVisible; }
            set
            {
                _textBlockVisible = value;
                RaisePropertyChanged("MakeTextBlockVisible");
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
                RaisePropertyChanged("BackGroundColor");
            }
        }

        public AnnotationViewModel(WorkspaceViewModel workspaceViewModel, AnnotationModel model)
        {
            //TODO: Have  events for property changed
            _annotationModel = model;
            this.WorkspaceViewModel = workspaceViewModel;
            this.MakeTextBlockVisible = Visibility.Visible;
            this.MakeTextBoxVisible = Visibility.Collapsed;
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
            }
        }

        internal void SaveTextboxValue(object parameter)
        {
            this.MakeTextBlockVisible = Visibility.Visible;
            this.MakeTextBoxVisible = Visibility.Collapsed;
        }

        internal bool CanSaveTextboxValue(object parameter)
        {
            return true;
        }

        internal void BringToFront(object parameter)
        {
            String message = "test";
        }

        internal bool CanBringToFront(object parameter)
        {
            return true;
        }
    }
}
