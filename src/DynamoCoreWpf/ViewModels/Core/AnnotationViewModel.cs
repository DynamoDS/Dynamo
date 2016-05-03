using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using Dynamo.Models;
using System;
using System.Windows.Input;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Selection;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Dynamo.Views;
using Color = System.Windows.Media.Color;
using System.Windows;

namespace Dynamo.ViewModels
{
    public class AnnotationViewModel : ViewModelBase
    {
        private AnnotationModel annotationModel;
        public readonly WorkspaceViewModel WorkspaceViewModel;        
      
        public AnnotationModel AnnotationModel
        {
            get { return annotationModel; }
            set
            {
                annotationModel = value;
                RaisePropertyChanged("AnnotationModel");
            }
        }

        public double DisplayScale
        {
            get { return annotationModel.DisplayScale; }
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
            get { return 1; }
            
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

        private DelegateCommand _addToGroupCommand;
        public DelegateCommand AddToGroupCommand
        {
             get
            {
                if (_addToGroupCommand == null)
                    _addToGroupCommand =
                        new DelegateCommand(AddToGroup, CanAddToGroup);

                return _addToGroupCommand;
            }
        }

        private bool CanAddToGroup(object obj)
        {
            return DynamoSelection.Instance.Selection.Count >= 0;
        }

        private void AddToGroup(object obj)
        {
            if (annotationModel.IsSelected)
            {
                var selectedModels = DynamoSelection.Instance.Selection.OfType<ModelBase>();
                foreach (var model in selectedModels)
                {
                    if (!(model is AnnotationModel))
                    {
                        this.AnnotationModel.AddToSelectedModels(model, true);
                    }
                }
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

        public IEnumerable<ModelBase> SelectedModels
        {
            get { return annotationModel.SelectedModels; }
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
            if (parameter == null) return;

            WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(
                    Guid.Empty, AnnotationModel.GUID, "FontSize", parameter.ToString()));

            WorkspaceViewModel.DynamoViewModel.RaiseCanExecuteUndoRedo();
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
                case "SelectedModels":
                    this.AnnotationModel.UpdateBoundaryFromSelection();
                    break;
            }
        }

        /// <summary>
        /// Selects this group and models within it.
        /// </summary>
        internal void Select()
        {
            var annotationGuid = this.AnnotationModel.GUID;
            this.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynamoModel.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));

            //Select all the models inside the group - This avoids some performance bottleneck 
            //with many nodes selected at the same time - which makes moving the group very slow
            DynamoSelection.Instance.Selection.AddRange(this.AnnotationModel.SelectedModels);
        }

        private double resize_zoom, resize_contentWidth, resize_contentHeight;
        private Point resize_mousePos;
        private List<Tuple<ModelBase, double, double>> resize_modelPos;
        private const double resize_extendSize = 10;

        internal void OnAnnotationResizeStarted(Point mousePosition)
        {
            var g = annotationModel;
            resize_mousePos = mousePosition;

            resize_contentWidth = g.Width - resize_extendSize;
            resize_contentHeight = g.Height - resize_extendSize - g.TextBlockHeight;

            resize_zoom = g.DisplayScale;
            resize_modelPos = new List<Tuple<ModelBase, double, double>>();

            foreach (var m in g.SelectedModels)
            {
                // Tuple = <ModelBase, X multiplier, Y multiplier>
                resize_modelPos.Add(
                    new Tuple<ModelBase, double, double>(
                        m, (m.X - g.X - resize_extendSize) / resize_zoom,
                        (m.Y - g.Y - resize_extendSize - g.TextBlockHeight) / resize_zoom));
            }
        }

        internal void OnAnnotationResizeDelta(Point mousePosition)
        {
            var g = annotationModel;
            var mouseDelta = mousePosition - resize_mousePos;
            if ((-mouseDelta.X > resize_contentWidth) || (-mouseDelta.Y > resize_contentHeight)) return;

            var newZoom = resize_zoom * Math.Sqrt(
                (mouseDelta.X / resize_contentWidth + 1) *
                (mouseDelta.Y / resize_contentHeight + 1));
            if ((newZoom < 0.2) || (newZoom > 1)) return;

            foreach (var t in resize_modelPos)
            {
                // Tuple = <ModelBase, X multiplier, Y multiplier>
                t.Item1.X = t.Item2 * newZoom + resize_extendSize + g.X;
                t.Item1.Y = t.Item3 * newZoom + resize_extendSize + g.Y + g.TextBlockHeight;
            }
            g.DisplayScale = newZoom;

            g.UpdateBoundaryFromSelection();
        }
    }
}
