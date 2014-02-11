using System;
using System.ComponentModel;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;

namespace Dynamo.ViewModels
{
    public partial class NoteViewModel: ViewModelBase
    {

        #region Properties
        
        private NoteModel _model;

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

        //public bool NoteVisibility
        //{
        //    get
        //    {
        //        if(DynamoSettings.Controller.DynamoViewModel.CurrentWorkspace.Notes.Contains(_model))
        //            return true;
        //        return false;
        //    }
        //}
        
        #endregion

        public event EventHandler RequestsSelection;
        public virtual void OnRequestsSelection(Object sender, EventArgs e)
        {
            if (RequestsSelection != null)
            {
                RequestsSelection(this, e);
            }
        }

        public NoteViewModel(NoteModel model)
        {
            _model = model;
            model.PropertyChanged += note_PropertyChanged;

            DynamoSettings.Controller.DynamoViewModel.Model.PropertyChanged += Model_PropertyChanged;
        }

        private void Select(object parameter)
        {
            //if (!_model.IsSelected)
            //{
            //    if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            //    {
            //        DynamoSelection.Instance.ClearSelection();
            //    }

            //    if (!DynamoSelection.Instance.Selection.Contains(_model))
            //    {
            //        DynamoSelection.Instance.Selection.Add(_model);
            //    }

            //}
            //else
            //{
            //    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            //    {
            //        DynamoSelection.Instance.Selection.Remove(_model);
            //    }
            //}

            OnRequestsSelection(this, EventArgs.Empty);
        }

        public void UpdateSizeFromView(double w, double h)
        {
            _model.Width = w;
            _model.Height = h;
        }


        private bool CanSelect(object parameter)
        {
            if (!DynamoSelection.Instance.Selection.Contains(_model))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Repond to changes on the model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //switch (e.PropertyName)
            //{
                //case "CurrentWorkspace":
                //    RaisePropertyChanged("NoteVisibility");
                //    break;
            //}
        }

        //respond to changes on the model's properties
        void note_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
    }
}
