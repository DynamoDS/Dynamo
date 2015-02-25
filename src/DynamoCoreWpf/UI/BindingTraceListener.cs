using System.Diagnostics;
using System.Text;
using System.Windows;

namespace Dynamo.UI
{
    public class BindingErrorTraceListener : DefaultTraceListener
    {
        private static BindingErrorTraceListener _Listener;

        public static void SetTrace()
        { SetTrace(SourceLevels.Error, TraceOptions.None); }

        public static void SetTrace(SourceLevels level, TraceOptions options)
        {
            if (_Listener == null)
            {
                _Listener = new BindingErrorTraceListener();
                PresentationTraceSources.DataBindingSource.Listeners.Add(_Listener);
            }

            _Listener.TraceOutputOptions = options;
            PresentationTraceSources.DataBindingSource.Switch.Level = level;
        }

        public static void CloseTrace()
        {
            if (_Listener == null)
            { return; }

            _Listener.Flush();
            _Listener.Close();
            PresentationTraceSources.DataBindingSource.Listeners.Remove(_Listener);
            _Listener = null;
        }



        private StringBuilder _Message = new StringBuilder();

        private BindingErrorTraceListener()
        { }

        public override void Write(string message)
        { _Message.Append(message); }

        public override void WriteLine(string message)
        {
            _Message.Append(message);

            var final = _Message.ToString();
            _Message.Length = 0;

            MessageBox.Show(final, "Binding Error", MessageBoxButton.OK,
              MessageBoxImage.Error);
        }
    }
}