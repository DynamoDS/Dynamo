using Dynamo.Publish.ViewModels;
using Dynamo.Wpf.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dynamo.Publish.Views
{
    /// <summary>
    /// Interaction logic for PublishView.xaml
    /// </summary>
    public partial class PublishView : Window
    {
        private PublishViewModel viewModel;

        public PublishView(PublishViewModel viewModel)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            InitializeComponent();
            updateTextBlocks(viewModel.IsDynamoProRunning);

            DataContext = viewModel;
            this.viewModel = viewModel;
            viewModel.UIDispatcher = Dispatcher;

            Closed += OnPublishViewClosed;
        }

        private void updateTextBlocks(bool isProVersion)
        {
            if (isProVersion)
            {
                Title = Properties.Resources.DynamoProPublishViewTitle;
                textBlockDefaultName.Text = Properties.Resources.DynamoProPublishViewDefaultNameTitle;
                textBlockDefaultDescription.Text = Properties.Resources.DynamoProPublishViewDefaultDescriptionTitle;
                Privacy.Text = Properties.Resources.DynamoProInviteIntegrity;
                manageTextBlock.Text = Properties.Resources.DynamoProManageButtonTitle;
            }
            else
            {
                Title = Properties.Resources.PublishViewTitle;
                textBlockDefaultName.Text = Properties.Resources.PublishViewDefaultNameTitle;
                textBlockDefaultDescription.Text = Properties.Resources.PublishViewDefaultDescriptionTitle;
                Privacy.Text = Properties.Resources.InviteIntegrity;
                manageTextBlock.Text = Properties.Resources.ManageButtonTitle;
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(viewModel.ManagerURL);
        }

        private void OnPublishViewClosed(object sender, EventArgs e)
        {
            textBoxName.Clear();
            textBoxDescription.Clear();
            viewModel.ClearShareLink();
        }
    }
}
