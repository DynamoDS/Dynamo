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
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;

using Dynamo.Controls;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for DynamoSplash.xaml
    /// </summary>
    public partial class DynamoSplash : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        string currentVersion;
        public string CurrentVersion
        {
            get
            {
                return currentVersion;
            }
            set
            {
                currentVersion = value;
                NotifyPropertyChanged("CurrentVersion");
            }
        }

        string loadingStatus;
        public string LoadingStatus
        {
            get { return loadingStatus; }
            set
            {
                loadingStatus = value;
                NotifyPropertyChanged("LoadingStatus");
            }
        }

        public DynamoSplash()
        {
            InitializeComponent();

            this.CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        }
    }
}
