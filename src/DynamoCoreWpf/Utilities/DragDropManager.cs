using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Dynamo.Wpf.Utilities
{
    public class DragDropManager
    {
        public DataObject Data { get; set; }

        public event EventHandler DropReleased;
        public void OnDropReleased(UIElement sender)
        {
            if (Data is null) return;

            DropReleased?.Invoke(sender, EventArgs.Empty);
            Data = null;
        }

        private static DragDropManager instance;

        public static DragDropManager Instance
        {
            get
            {
                if (instance is null) { instance = new DragDropManager(); }
                return instance;
            }
        }

        public bool IsMouseInsideControl(UIElement controlToCheck)
        {
            bool isInside = false;
            VisualTreeHelper.HitTest(
                controlToCheck,
                d =>
                {
                    if (d == controlToCheck)
                    {
                        isInside = true;
                    }
                    return HitTestFilterBehavior.Stop;
                },
                ht => HitTestResultBehavior.Stop,
                new PointHitTestParameters(Mouse.GetPosition(controlToCheck)));
            return isInside;
        }

        private DragDropManager() { }
    }
}
