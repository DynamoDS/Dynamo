using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Nodes
{
    class dynNoteVIewModel:dynViewModelBase
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

        public dynNoteVIewModel(dynNoteModel note)
        {
            _note = note;
            note.PropertyChanged += note_PropertyChanged;
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
