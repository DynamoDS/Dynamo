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

        public double X { get;}
        public double Y { get;}
        public string Text { get;}

        public dynNoteVIewModel(dynNoteModel note)
        {
            _note = note;
            note.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(note_PropertyChanged);
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
