//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

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

      public FunctionNamePrompt(string name, string currentCategory, IEnumerable<string> categories, string description)
      {
          InitializeComponent();

          this.Owner = dynSettings.Bench;
          this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

          // set the current name
          this.nameBox.Focus();
          this.nameBox.Text = name;

          // set the description
          this.DescriptionInput.Text = description;

          // sort the categories
          var sortedCats = categories.ToList();
          sortedCats.Sort();

          foreach (var item in sortedCats)
          {
              this.categoryBox.Items.Add(item);
          }

          // set the current category
          this.categoryBox.Text = currentCategory;
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
