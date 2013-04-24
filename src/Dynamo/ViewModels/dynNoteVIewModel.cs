using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    public class dynNoteViewModel:dynViewModelBase
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
            set
            {
                _x = value;
                RaisePropertyChanged("X");
            }
        }

        public double Y
        {
            get { return _note.Y; }
            set 
            { _
                _y = value; 
                RaisePropertyChanged("Y");
            }
        }

        public string Text
        {
            get { return _note.Text; }
            set {RaisePropertyChanged("Text");}
        }

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
                    X = _note.X;
                    break;
                case "Y":
                    Y = _note.Y;
                    break;
                case "Text":
                    Text = _note.Text;
                    break;
            }
        }
    }
}
