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
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
   /// <summary>
   /// Interaction logic for FunctionNamePrompt.xaml
   /// </summary>
   public partial class FunctionNamePrompt : Window
   {
      public FunctionNamePrompt(IEnumerable<string> categories, string error)
      {
         InitializeComponent();
         this.Owner = dynSettings.Bench;
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

      public string Category
      {
         get { return this.categoryBox.Text; }
      }
   }
}
