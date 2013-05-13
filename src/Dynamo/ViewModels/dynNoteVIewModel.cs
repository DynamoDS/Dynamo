using System.Windows;
using System.Windows.Input;
using Dynamo.Selection;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Nodes
{
    public class dynNoteViewModel: dynViewModelBase
    {

        #region Properties
        
        private dynNoteModel _model;

        public dynNoteModel Model
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
            get { return 2; }
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

        public DelegateCommand SelectCommand { get; set; }

        public Visibility NoteVisibility
        {
            get
            {
                if(dynSettings.Controller.DynamoViewModel.CurrentSpace.Notes.Contains(_model))
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
        }
        
        #endregion

        public dynNoteViewModel(dynNoteModel model)
        {
            _model = model;
            model.PropertyChanged += note_PropertyChanged;

            dynSettings.Controller.DynamoViewModel.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Model_PropertyChanged);
            SelectCommand = new DelegateCommand(Select, CanSelect);
        }

        private void Select()
        {
            if (!_model.IsSelected)
            {
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.ClearSelection();
                }

                if (!DynamoSelection.Instance.Selection.Contains(_model))
                {
                    DynamoSelection.Instance.Selection.Add(_model);
                }

            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.Selection.Remove(_model);
                }
            }
        }

        public void UpdateSizeFromView(double w, double h)
        {
            this._model.Width = w;
            this._model.Height = h;
        }


        private bool CanSelect()
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
        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentSpace":
                    RaisePropertyChanged("NoteVisibility");
                    break;
            }
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
    }
}
