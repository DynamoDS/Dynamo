using System.Windows.Controls;

namespace CoreNodeModelsWpf
{
    /// <summary>
    /// Interaction logic for ElementSelectionControl.xaml
    /// </summary>
    public partial class ElementSelectionControl : UserControl
    {
        public ElementSelectionControl()
        {
            Resources.MergedDictionaries.Add(Dynamo.UI.SharedDictionaryManager.DynamoModernDictionary);
            InitializeComponent();
        }
    }
}
