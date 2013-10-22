using System.Windows.Controls;
using Dynamo.UI;

namespace Dynamo.Nodes.Views
{
    /// <summary>
    /// Interaction logic for dynConnectorView.xaml
    /// </summary>
    public partial class dynConnectorView : UserControl
    {
        public dynConnectorView()
        {
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoModernDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoColorsAndBrushesDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);

            InitializeComponent();

            Dynamo.Controls.DragCanvas.SetCanBeDragged(this, false);
            Canvas.SetZIndex(this, 1);
        }

    }
}
