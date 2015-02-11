using System.Windows;

using Dynamo.Core;
using Dynamo.Models;

namespace Dynamo.Wpf.ViewModels
{
    public class RunSettingsViewModel : NotificationObject
    {
        public RunSettings Model { get; private set; }

        public RunType RunType
        {
            get
            {
                return Model.RunType;
            }
            set
            {
                Model.RunType = value;
            }
        }

        public int RunPeriod
        {
            get { return Model.RunPeriod; }
            set
            {
                Model.RunPeriod = value; 
            }
        }

        public Visibility RunPeriodInputVisibilty
        {
            get
            {
                switch (RunType)
                {
                    case RunType.Manual:
                    case RunType.Automatic:
                        return Visibility.Collapsed;
                    case RunType.Periodic:
                        return Visibility.Visible;
                    default:
                        return Visibility.Hidden;
                }
            }
        }

        public RunSettingsViewModel(RunSettings settings)
        {
            Model = settings;
            Model.PropertyChanged += settings_PropertyChanged;
        }

        void settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "RunType":
                    RaisePropertyChanged("RunType");
                    RaisePropertyChanged("RunPeriod");
                    RaisePropertyChanged("RunPeriodInputVisibilty");
                    break;
            }
        }
    }
}
