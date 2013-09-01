namespace Dynamo.Models
{
    public class NoteModel:ModelBase
    {

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

        public NoteModel(double x, double y)
        {
            X = x;
            Y = y;
        }

    }
}
