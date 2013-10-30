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
         //this.Owner = dynSettings.Bench;
         this.Owner = WPF.FindUpVisualTree<DynamoView>(this);
         this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

         this.nameBox.Focus();

         var sortedCats = categories.ToList();
         sortedCats.Sort();

         foreach (var item in sortedCats)
         {
            this.categoryBox.Items.Add(item);
         }
      }

      void OK_Click(object sender, RoutedEventArgs e)
      {
         this.DialogResult = true;
      }

      void Cancel_Click(object sender, RoutedEventArgs e)
      {
         this.DialogResult = false;
      }

      public string Text
      {
         get { return this.nameBox.Text; }
      }

      public string Description
      {
          get { return this.DescriptionInput.Text; }
      }

      public string Category
      {
         get { return this.categoryBox.Text; }
      }
   }
}
