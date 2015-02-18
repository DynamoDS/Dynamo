using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

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
                RaisePropertyChanged("RunType");
                RaisePropertyChanged("RunPeriodInputVisibilty");
            }
        }

        public int RunPeriod
        {
            get { return Model.RunPeriod; }
            set
            {
                Model.RunPeriod = value; 
                RaisePropertyChanged("RunPeriod");
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
        }
    }

    public class RunPeriodConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("{0}{1}", value, "ms");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int ms;
            return !Int32.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out ms) ? 100 : Math.Abs(ms);
        }
    }
}
