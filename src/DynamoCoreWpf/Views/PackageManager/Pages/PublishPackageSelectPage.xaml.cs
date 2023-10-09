using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PublishPackageSelectPage.xaml
    /// </summary>
    public partial class PublishPackageSelectPage : Page
    {
        private PublishPackageViewModel PublishPackageViewModel;

        public PublishPackageSelectPage()
        {
            InitializeComponent();

            this.DataContextChanged += PublishPackagePublishPage_DataContextChanged;
            this.Tag = "Select Package Contents";
        }

        private void PublishPackagePublishPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PublishPackageViewModel = this.DataContext as PublishPackageViewModel;
        }


        public void Dispose()
        {
            this.DataContextChanged -= PublishPackagePublishPage_DataContextChanged;
        }
    }


    public class DependencyTypeToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DependencyType.Folder)
            {
                // Return visible only if the item is a Folder
                return "/DynamoCoreWpf;component/UI/Images/file-generic-16px.png";
            }

            // If the item is anything else (Assembly, File, Custom Node) return collapsed
            return "/DynamoCoreWpf;component/UI/Images/folder-generic-16px.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class DependencyTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is DependencyType dependencyType)
            {
                if (dependencyType.ToString() == parameter?.ToString())
                {
                    // Return visible if the item matches the specified dependency type
                    return Visibility.Visible;
                }
            }
            // If the item does not match the specified dependency type or value is null, return collapsed
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
