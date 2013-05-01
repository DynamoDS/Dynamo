namespace Dynamo.Nodes
{
    public class dynNoteModel:dynModelBase
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

        public dynNoteModel(double x, double y)
        {
            X = x;
            Y = y;
        }

    }
}
