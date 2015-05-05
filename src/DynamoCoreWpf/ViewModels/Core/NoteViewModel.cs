using System;
using System.Linq;
using Dynamo.Models;
using Dynamo.Selection;

namespace Dynamo.ViewModels
{
    public partial class NoteViewModel: ViewModelBase
    {
        #region Events

        public event EventHandler RequestsSelection;
        public virtual void OnRequestsSelection(Object sender, EventArgs e)
        {
            if (RequestsSelection != null)
            {
                RequestsSelection(this, e);
            }
        }

        #endregion

        #region Properties

        private NoteModel _model;
        public readonly WorkspaceViewModel WorkspaceViewModel;

        public NoteModel Model
        {
            get { return _model; }
            set 
            { 
                _model = value;
                RaisePropertyChanged("Model");
            }
        }

        /// <summary>
        /// Element's left position is two-way bound to this value
        /// </summary>
        public double Left
        {
            get { return _model.X; }
            set
            {
                _model.X = value;
                RaisePropertyChanged("Left");
            }
        }

        /// <summary>
        /// Element's top position is two-way bound to this value
        /// </summary>
        public double Top
        {
            get { return _model.Y; }
            set
            {
                _model.Y = value;
                RaisePropertyChanged("Top");
            }
        }

        public double ZIndex
        {
            get { return 3; }
        }

        public string Text
        {
            get { return _model.Text; }
            set { _model.Text = value; }
        }

        public bool IsSelected
        {
            get { return _model.IsSelected; }
        }

        #endregion

        public NoteViewModel(WorkspaceViewModel workspaceViewModel, NoteModel model)
        {
            this.WorkspaceViewModel = workspaceViewModel;
            _model = model;
            model.PropertyChanged += note_PropertyChanged;
            DynamoSelection.Instance.Selection.CollectionChanged += SelectionOnCollectionChanged;
        }

        private void SelectionOnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            AddToGroupCommand.RaiseCanExecuteChanged();
            UngroupCommand.RaiseCanExecuteChanged();
        }

        private void Select(object parameter)
        {
            OnRequestsSelection(this, EventArgs.Empty);
        }

        public void UpdateSizeFromView(double w, double h)
        {
            this._model.SetSize(w,h);     
        }

        private bool CanSelect(object parameter)
        {
            if (!DynamoSelection.Instance.Selection.Contains(_model))
            {
                return true;
            }
            return false;
        }

        //respond to changes on the model's properties
        void note_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "X":
                    RaisePropertyChanged("Left");
                    break;
                case "Y":
                    RaisePropertyChanged("Top");
                    break;
                case "Text":
                    RaisePropertyChanged("Text");
                    break;
                case "IsSelected":
                    RaisePropertyChanged("IsSelected");
                    break;

            }
        }

        private void CreateGroup(object parameters)
        {
            WorkspaceViewModel.DynamoViewModel.AddAnnotationCommand.Execute(null);
        }

        private bool CanCreateGroup(object parameters)
        {
            return DynamoSelection.Instance.Selection.OfType<ModelBase>().Any();
        }

        private void UngroupNote(object parameters)
        {
            WorkspaceViewModel.DynamoViewModel.UngroupModelCommand.Execute(null);
        }

        private bool CanUngroupNote(object parameters)
        {
            var groups = WorkspaceViewModel.Model.Annotations;
            if (groups != null
                && DynamoSelection.Instance.Selection.OfType<NoteModel>().Any())
                return (from model in groups
                        let noteModel = DynamoSelection.Instance.Selection.OfType<NoteModel>().FirstOrDefault()
                        where model.SelectedModels.Any(x => x.GUID == noteModel.GUID)
                        select model).Any();
            return false;
        }

        private void AddToGroup(object parameters)
        {
            WorkspaceViewModel.DynamoViewModel.AddModelsToGroupModelCommand.Execute(null);
        }

        private bool CanAddToGroup(object parameters)
        {
            var groups = WorkspaceViewModel.Model.Annotations;
            return groups != null && groups.Any(x => x.IsSelected);
        }
    }
}
