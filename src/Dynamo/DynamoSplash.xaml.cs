using System;
using System.Windows;
using System.Reflection;
using System.ComponentModel;

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
