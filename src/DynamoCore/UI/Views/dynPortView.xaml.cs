using System;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.Connectors
{
    public partial class dynPortView : UserControl, IViewModelView<PortViewModel>
    {
        #region constructors

        public dynPortView()
        {
            InitializeComponent();
        }

        #endregion constructors

        public bool Visible
        {
            get
            {
                throw new NotImplementedException("Implement port Visibility parameter getter.");
            }
            set
            {
                throw new NotImplementedException("Implement port Visibility parameter setter.");
            }
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dynSettings.ReturnFocusToSearch();

            if (ViewModel != null)
                ViewModel.ConnectCommand.Execute(null);
    
            //set the handled flag so that the element doesn't get dragged
            e.Handled = true;
        }

        public PortViewModel ViewModel
        {
            get
            {
                if (this.DataContext is PortViewModel)
                    return (PortViewModel) this.DataContext;
                else
                    return null;
            }
        }
    }

}
