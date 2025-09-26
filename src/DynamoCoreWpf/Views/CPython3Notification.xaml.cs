using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;
using Dynamo.Wpf.ViewModels.FileTrust;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Views
{
    /// <summary>
    /// Interaction logic for CPython3Notification.xaml
    /// </summary>
    public partial class CPython3Notification : Popup
    {
        private DynamoView mainWindow;
        private DynamoViewModel dynViewModel;
        private CPython3NotificationViewModel notificationViewModel;

        public CPython3Notification(DynamoView dynamoViewWindow)
        {
            //InitializeComponent();

            //mainWindow = dynamoViewWindow;
            //var dynamoView = dynamoViewWindow as DynamoView;
            //if (dynamoView == null) return;
            //dynViewModel = dynamoView.DataContext as DynamoViewModel;

            //notificationViewModel = dynViewModel.CPython3NotificationViewModel;

            //if (notificationViewModel == null)
            //    notificationViewModel = new CPython3NotificationViewModel();

            //DataContext = fileTrustWarningViewModel;

            //if (dynamoViewWindow == null) return;
        }
    }
}
