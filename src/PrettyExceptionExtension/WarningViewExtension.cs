using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dynamo.WarningHelper
{
    public class WarningHelperViewExtension : IViewExtension, INotifyPropertyChanged
    {
        public ObservableCollection<Exception> Notifications { get; private set; }
     
        public string Name
        {
            get
            {
                return "PrettyPrintExceptionHandler";
            }
        }

        public string UniqueId
        {
            get
            {
                return "ef6cd025-514f-44cd-b6b1-69d9f5cce004";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void Dispose()
        {
           // UnregisterEventHandlers();
        }

        public void Loaded(ViewLoadedParams p)
        {

            //this is hacky, but I do not want to further disturb our public APIs.
            //get the dynamoModel from the dynamo view and get the preload data from there.
            var data = (p.DynamoWindow.DataContext as DynamoViewModel).Model.PreloadData;
            if (data == null)
            {
                return;
            }
            Notifications = new ObservableCollection<Exception>();
            Notifications.CollectionChanged += (o, e) => { NotifyPropertyChanged("ShowBadge"); };

            data.Exceptions.ForEach(x => Notifications.Add(x));


            //add a new menuItem to the Dynamo mainMenu.
            var notificationsMenuItem = new MenuItem();

            
            var showItem = new MenuItem();
            showItem.Header = "Display All Notifications";
            showItem.Click += (o, e) =>
            {
                foreach (var exception in this.Notifications)
                {
                    var window = new WarningView(exception, DynamoApplications.Properties.Resources.MismatchedAssemblyVersionShortMessage);
                    window.ShowDialog();
                }
            };

            var dismissItem = new MenuItem();
            dismissItem.Header = "Dismiss All Notifications";
            dismissItem.Click += (o, e) => { this.Notifications.Clear(); };

            notificationsMenuItem.Items.Add(showItem);
            notificationsMenuItem.Items.Add(dismissItem);


            //grid that will go into menu header
            var headerContent = new Grid();
            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = new GridLength(70, GridUnitType.Star);
            ColumnDefinition c2 = new ColumnDefinition();
            c2.Width = new GridLength(30, GridUnitType.Star);

            headerContent.ColumnDefinitions.Add(c1);
            headerContent.ColumnDefinitions.Add(c2);


            headerContent.MinWidth = 25;
            headerContent.Width = double.NaN;

            var color = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray);
            var icon = new Image() { Source = FontAwesome.WPF.ImageAwesome.CreateImageSource(FontAwesome.WPF.FontAwesomeIcon.ExclamationCircle, color) };
            icon.Height = 15;
            icon.HorizontalAlignment = HorizontalAlignment.Left;

            headerContent.Children.Add(icon);
            Grid.SetRow(icon, 0);
            Grid.SetColumn(icon, 0);


            var badgeGrid = new Grid();
          //attach the visibility of the badge to the number of notifications without a binding...
            Notifications.CollectionChanged += (o, e) => { if (Notifications.Count > 0) {
                    badgeGrid.Visibility = Visibility.Visible;
                }
                else {
                    badgeGrid.Visibility = Visibility.Hidden;
                } };
           

            var circle = new Ellipse { Stroke = new SolidColorBrush(Colors.LightGreen), Fill = new SolidColorBrush(Colors.LightGreen) };
            circle.Stretch = Stretch.Uniform;
            circle.HorizontalAlignment = HorizontalAlignment.Stretch;
            circle.VerticalAlignment = VerticalAlignment.Stretch;
            circle.MinWidth = 10;

            headerContent.Children.Add(badgeGrid);
            Grid.SetColumn(badgeGrid, 1);
            
            var countLabel = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                
            };

            //create a binding between the label and the count of notifications
            var binding = new Binding();
            binding.Path = new PropertyPath("Notifications.Count");
            countLabel.DataContext = this;
            countLabel.SetBinding(TextBlock.TextProperty, binding);
            countLabel.HorizontalAlignment = HorizontalAlignment.Center;
            countLabel.VerticalAlignment = VerticalAlignment.Center;


            badgeGrid.Children.Add(circle);
            badgeGrid.Children.Add(countLabel);
           
            notificationsMenuItem.Header = headerContent;

            p.dynamoMenu.Items.Add(notificationsMenuItem);
        }

        private void Notifications_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void NoticationsMenuItem_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
           //when pressed menu item will display all of the things
        }

        private void LogWarningMessageEvents_LogWarningMessage(DynamoServices.LogWarningMessageEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void Startup(ViewStartupParams p)
        {
            
        }
    }
}
