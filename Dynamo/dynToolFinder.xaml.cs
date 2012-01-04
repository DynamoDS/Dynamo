//Copyright 2012 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using Dynamo.Utilities;

namespace Dynamo.Elements
{
    public delegate void ToolFinderFinishedHandler(object sender, EventArgs e);

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class dynToolFinder : UserControl
    {
        public event ToolFinderFinishedHandler ToolFinderFinished;

        bool _listShowing = false;
        List<string> toolNames = new List<string>();
        bool hasStartedSearching = false;

        protected virtual void OnToolFinderFinished(EventArgs e)
        {
            if (ToolFinderFinished != null)
                ToolFinderFinished(this, e);
        }

        public dynToolFinder()
        {
           InitializeComponent();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            //when a key is pressed iterate over all loaded types
            //using the current text in the text box and repopulate
            //Assembly elementsAssembly = System.Reflection.Assembly.GetExecutingAssembly();

            //if (!_listShowing && this.toolNameBox.Text != "")
            //{
            //    //show the drop down list
            //    _listShowing = true;

            //    //show the list box
            //    toolSelectListBox.Visibility = System.Windows.Visibility.Visible;

            //}

            //if (e.Key == Key.Enter)
            //{
            //    //if the list is showing
            //    //enter will build whatever element is chosen in
            //    //the list
            //    if (_listShowing)
            //    {
            //        if (toolSelectListBox.SelectedIndex >= 0)
            //        {
            //            //create an element with the type name selected
            //            foreach (Type t in elementsAssembly.GetTypes())
            //            {
            //                object[] attribs = t.GetCustomAttributes(typeof(ElementNameAttribute), false);
            //                if (attribs.Length > 0)
            //                {
            //                    if (t.Name == ((ListBoxItem)toolSelectListBox.Items[toolSelectListBox.SelectedIndex]).Content.ToString())
            //                    {
            //                        dynElement newEl = dynElementSettings.SharedInstance.Bench.AddDynElement(t, (attribs[0] as ElementNameAttribute).ElementName, Guid.NewGuid(), 0.0, 0.0);
            //                        newEl.CheckInputs();
                                    
            //                        //turn off the tool list box by sending an event
            //                        //that is picked up by the bench

            //                        OnToolFinderFinished(EventArgs.Empty);

            //                        break;
            //                    }
            //                }
            //            }

            //            return;
            //        }
            //    }
            //}
            //else if (e.Key == Key.Down || e.Key == Key.Up)
            //{
            //    if (_listShowing)
            //    {
            //        if (e.Key == Key.Down)
            //        {
            //            toolSelectListBox.SelectedIndex++;
            //        }
            //        else if (e.Key == Key.Up)
            //        {
            //            toolSelectListBox.SelectedIndex--;
            //        }
            //        return;
            //    }
            //}
            
            ////clear the list of list box items
            ////and our list holding the tool names
            //toolSelectListBox.Items.Clear();
            //toolNames.Clear();
            
            //if (string.IsNullOrEmpty(toolNameBox.Text))
            //{
            //    toolSelectListBox.Items.Clear();
            //    toolNames.Clear();
            //    toolSelectListBox.Visibility = System.Windows.Visibility.Hidden;
            //    _listShowing = false;
            //    return;
            //}

            
       
            //foreach (Type t in elementsAssembly.GetTypes())
            //{
            //    //only load types that are in the right namespace, are not abstract
            //    //and have the elementname attribute
            //    object[] attribs = t.GetCustomAttributes(typeof(ElementNameAttribute), false);
            //    object[] descrips = t.GetCustomAttributes(typeof(ElementDescriptionAttribute), false);

            //    if (t.Namespace == "Dynamo.Elements" && !t.IsAbstract && attribs.Length > 0)
            //    {
            //        if (t.Name.Contains(toolNameBox.Text) && toolNameBox.Text != "")
            //        {
            //            if (!toolNames.Contains(t.Name))
            //            {
            //                ListBoxItem lbi = new ListBoxItem();

            //                lbi.Content = t.Name;
            //                //lbi.Content = ((ElementNameAttribute)attribs[0]).ElementName;

            //                if (descrips.Length > 0)
            //                {
            //                    lbi.ToolTip = ((ElementDescriptionAttribute)descrips[0]).ElementDescription;
            //                }

            //                toolSelectListBox.Items.Add(lbi);
            //                toolNames.Add(t.Name);
            //            }
            //        }
            //    }
            //}

            //e.Handled = true;
        }

        private void toolNameBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //the first time the user clicks in the text
            //box...blank out the text
            if (!hasStartedSearching)
            {
                toolNameBox.Text = "";
                hasStartedSearching = true;
                e.Handled = true;
            }

            

        }


    }
}
