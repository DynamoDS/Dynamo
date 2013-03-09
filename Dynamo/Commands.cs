using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Controls;
using System.ComponentModel;
using Dynamo.Utilities;
using System.Windows.Input;

namespace Dynamo.Commands
{
    public class NodeFromSelectionCommand : ICommand
    {
        private Dynamo.Controls.DragCanvas _canvas;
        private DynamoController _controller;

        public NodeFromSelectionCommand(Dynamo.Controls.DragCanvas canvas, DynamoController controller)
        {
            _canvas = canvas;
            _controller = controller;
            _canvas.PropertyChanged += new PropertyChangedEventHandler(_canvas_PropertyChanged);
        }

        public void Execute(object parameters)
        {
            _controller.NodeFromSelection(
                _canvas.Selection.Where(x => x is dynNodeUI)
                    .Select(x => (x as dynNodeUI).NodeLogic));
        }

        public event EventHandler CanExecuteChanged;
        private void _canvas_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameters)
        {
            return _canvas.Selection.Count > 0;
        }
    }
}
