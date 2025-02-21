using System.Windows;
using System.Windows.Controls;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class TextBase : TextBlock
    {
        private bool isRightOriented;
        private bool isBottomOriented;
        private string observableText;

        public bool IsRightOriented
        {
            get => isRightOriented;
            set => isRightOriented = value;
        }

        public bool IsBottomOriented
        {
            get => isBottomOriented;
            set => isBottomOriented = value;
        }

        public string ObservableText
        {
            get => observableText;
            set => observableText = value;
        }

        public TextBase()
        {
        }

        public virtual void Regenerate(Point p)
        {
        }
    }
}
