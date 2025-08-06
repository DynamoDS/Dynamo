using System.Windows.Forms;

namespace Dynamo.Wpf.UI
{
    /// <summary>
    /// This interface will be used when mocking SaveFileDialog in unit tests.
    /// </summary>
    internal interface IFileSaver
    {
        string Filter { get; set; }
        string DefaultExt { get; set; }
        string FileName { get; set; }
        public bool AddExtension { get; set; }
        public string InitialDirectory { get; set; }
        bool? ShowDialog(); // Returns true if OK, false if Cancel, null for other cases
    }

    /// <summary>
    /// Concrete implementation of IFileSaver that wraps the SaveFileDialog and can be used in Unit Tests.
    /// </summary>
    public class CustomSaveFileDialog : IFileSaver
    {
        private readonly SaveFileDialog _dialog = new SaveFileDialog();

        public string Filter
        {
            get => _dialog.Filter;
            set => _dialog.Filter = value;
        }

        public string DefaultExt
        {
            get => _dialog.DefaultExt;
            set => _dialog.DefaultExt = value;
        }

        public string FileName
        {
            get => _dialog.FileName;
            set => _dialog.FileName = value;
        }

        public bool AddExtension
        {
            get => _dialog.AddExtension;
            set => _dialog.AddExtension = value;
        }

        public string InitialDirectory
        {
            get => _dialog.InitialDirectory;
            set => _dialog.InitialDirectory = value;
        }
      
        public bool? ShowDialog()
        {
            DialogResult result = _dialog.ShowDialog();
            if (result == DialogResult.OK) return true;
            if (result == DialogResult.Cancel) return false;
            return null;
        }
    }
}
