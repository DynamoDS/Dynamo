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

namespace Dynamo.Elements
{
    /// <summary>
    /// Interaction logic for dynNote.xaml
    /// </summary>
    public partial class dynNote : UserControl
    {
        public dynNote()
        {
            InitializeComponent();

            //noteText.IsReadOnly = true;
            moveDot.Visibility = System.Windows.Visibility.Hidden;

            noteText.PreviewMouseDoubleClick += new MouseButtonEventHandler(noteText_MouseDoubleClick);
            noteText.LostFocus += new RoutedEventHandler(noteText_LostFocus);
            //noteText.LostMouseCapture += new MouseEventHandler(noteText_LostMouseCapture);
            noteText.PreviewLostKeyboardFocus += new KeyboardFocusChangedEventHandler(noteText_LostKeyboardFocus);
            noteText.GotFocus += new RoutedEventHandler(noteText_GotFocus);
            noteText.PreviewGotKeyboardFocus += new KeyboardFocusChangedEventHandler(noteText_GotKeyboardFocus);
            //noteText.GotMouseCapture += new MouseEventHandler(noteText_GotMouseCapture);

        }

        void noteText_GotMouseCapture(object sender, MouseEventArgs e)
        {
            moveDot.Visibility = System.Windows.Visibility.Visible;
        }

        void noteText_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            moveDot.Visibility = System.Windows.Visibility.Visible;
        }

        void noteText_GotFocus(object sender, RoutedEventArgs e)
        {
            moveDot.Visibility = System.Windows.Visibility.Visible;
        }

        void noteText_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            moveDot.Visibility = System.Windows.Visibility.Hidden;
        }

        void noteText_LostMouseCapture(object sender, MouseEventArgs e)
        {
            moveDot.Visibility = System.Windows.Visibility.Hidden;
        }

        void noteText_LostFocus(object sender, RoutedEventArgs e)
        {
            moveDot.Visibility = System.Windows.Visibility.Hidden;
        }

        void noteText_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            moveDot.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
