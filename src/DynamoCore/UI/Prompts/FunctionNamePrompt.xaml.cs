using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dynamo.Controls;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
   /// <summary>
   /// Interaction logic for FunctionNamePrompt.xaml
   /// </summary>
   public partial class FunctionNamePrompt : Window
   {
      public FunctionNamePrompt(IEnumerable<string> categories)
      {
         InitializeComponent();
         //this.Owner = DynamoSettings.Bench;
         Owner = WPF.FindUpVisualTree<DynamoView>(this);
         WindowStartupLocation = WindowStartupLocation.CenterOwner;

         nameBox.Focus();

         var sortedCats = categories.ToList();
         sortedCats.Sort();

         foreach (var item in sortedCats)
         {
            categoryBox.Items.Add(item);
         }
      }

      void OK_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = true;
      }

      void Cancel_Click(object sender, RoutedEventArgs e)
      {
         DialogResult = false;
      }

      public string Text
      {
         get { return nameBox.Text; }
      }

      public string Description
      {
          get { return DescriptionInput.Text; }
      }

      public string Category
      {
         get { return categoryBox.Text; }
      }
   }
}
