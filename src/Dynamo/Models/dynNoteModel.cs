using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Nodes
{
    public class dynNoteModel:NotificationObject
    {
        private double _x;
        private double _y;

        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
                RaisePropertyChanged("X");
            }
        }
        public double Y 
        { 
            get { return _y; }
            set { 
                _y = value;
                RaisePropertyChanged("Y");
            }
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RaisePropertyChanged("Text");
            }
        }
        public dynNoteModel(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
