using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.ViewModels;
using Dynamo.Wpf.ViewModels.Core;
using Dynamo.Wpf.ViewModels.FileTrust;

namespace Dynamo.Wpf.Views.Notifications
{
    /// <summary>
    /// Interaction logic for NotificationUI.xaml
    /// </summary>
    public partial class NotificationUI : Popup
    {
        public NotificationUI(DynamoView dynamoViewWindow)
        {
            InitializeComponent();          
        }
    }
}
