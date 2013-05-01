using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Dynamo.Selection;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Nodes
{
    public class dynNoteViewModel: dynViewModelBase
    {
        private dynNoteModel _note;

        public dynNoteModel Note
        {
            get { return _note; }
            set 
            { 
                _note = value;
                RaisePropertyChanged("Note");
            }
        }

        public double X
        {
            get { return _note.X; }
        }

        public double Y
        {
            get { return _note.Y; }
        }

        public string Text
        {
            get { return _note.Text; }
        }

        public DelegateCommand SelectCommand { get; set; }

        public Visibility NoteVisibility
        {
            get
            {
                if(dynSettings.Controller.DynamoViewModel.CurrentSpace.Notes.Contains(_note))
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
        }
        
        public dynNoteViewModel(dynNoteModel note)
        {
            _note = note;
            note.PropertyChanged += note_PropertyChanged;

            dynSettings.Controller.DynamoViewModel.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Model_PropertyChanged);
            SelectCommand = new DelegateCommand(Select, CanSelect);
        
        }

        private void Select()
        {
            DynamoSelection.Instance.Selection.Add(_note);
        }

        private bool CanSelect()
        {
            if (!DynamoSelection.Instance.Selection.Contains(_note))
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
                    RaisePropertyChanged("X");
                    break;
                case "Y":
                    RaisePropertyChanged("Y");
                    break;
                case "Text":
                    RaisePropertyChanged("Text");
                    break;
            }
        }
    }
}
