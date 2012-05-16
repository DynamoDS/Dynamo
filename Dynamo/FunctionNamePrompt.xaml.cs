using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dynamo.Elements
{
   /// <summary>
   /// Interaction logic for FunctionNamePrompt.xaml
   /// </summary>
   public partial class FunctionNamePrompt : Window
   {
      public FunctionNamePrompt(ICollection<string> categories, string error)
      {
         InitializeComponent();
         this.nameBox.Focus();

         foreach (var item in categories)
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

      public string Category
      {
         get { return this.categoryBox.Text; }
      }
   }
}
