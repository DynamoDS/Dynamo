using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Newtonsoft.Json;
using Color = System.Windows.Media.Color;

namespace Dynamo.ViewModels
{
    public class AnnotationViewModel : ViewModelBase
    {
        private AnnotationModel annotationModel;
        public readonly WorkspaceViewModel WorkspaceViewModel;        
        
        [JsonIgnore]
        public AnnotationModel AnnotationModel
        {
            get { return annotationModel; }
            set
            {
                annotationModel = value;
                RaisePropertyChanged("AnnotationModel");
            }
        }

        [JsonIgnore]
        public Double Width
        {
            get { return annotationModel.Width; }
        }

        [JsonIgnore]
        public Double Height
        {
            get { return annotationModel.Height; }
            set
            {
                annotationModel.Height = value;              
            }
        }

        [JsonIgnore]
        public Double Top
        {
            get { return annotationModel.Y; }
            set
            {
                annotationModel.Y = value;                
            }
        }

        [JsonIgnore]
        public Double Left
        {
            get { return annotationModel.X; }
            set { annotationModel.X = value; }
        }

        [JsonIgnore]
        public double ZIndex
        {
            get { return 1; }
            
        }

        [JsonIgnore]
        public String AnnotationText
        {
            get { return annotationModel.AnnotationText; }
            set
            {
                annotationModel.AnnotationText = value;                
            }
        }
       
        private Color _background;
        [JsonIgnore]
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

        [JsonIgnore]
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
        [JsonIgnore]
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
        [JsonIgnore]
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

        [JsonIgnore]
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

        [JsonIgnore]
        public IEnumerable<ModelBase> Nodes
        {
            get { return annotationModel.Nodes; }
        }
       
        public AnnotationViewModel(WorkspaceViewModel workspaceViewModel, AnnotationModel model)
        {             
            annotationModel = model;           
            this.WorkspaceViewModel = workspaceViewModel;
            model.PropertyChanged += model_PropertyChanged;
            //https://jira.autodesk.com/browse/QNTM-3770
            //Notes and Groups are serialized as annotations. Do not unselect the node selection during
            //Notes serialization
            if (model.Nodes.Count() > 0)
            {
                // Group is created already.So just populate it.
                var selectNothing = new DynamoModel.SelectModelCommand(Guid.Empty, System.Windows.Input.ModifierKeys.None.AsDynamoType());
                WorkspaceViewModel.DynamoViewModel.ExecuteCommand(selectNothing);
            }
            
        }

        internal void ClearSelection()
        {
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
            DynamoSelection.Instance.Selection.AddRange(this.AnnotationModel.Nodes);
        }
    }
}
