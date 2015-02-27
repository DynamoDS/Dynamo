using System.Windows;
using System.Windows.Media;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.Interfaces
{
    public struct ResourceNames
    {
        public enum ConsentForm
        {
            Image,        
            Title, 
            AgreementOne,  
            AgreementTwo,
            FeatureUsage,
            NodeUsage,
            Consent,
        }

        public enum StartPage
        {
            Image,       
            Title, 
        }

        public enum MainWindow
        {
            Image,
            Title,
        }
    }

    public interface IBrandingResourceProvider
    {
        ImageSource GetImageSource(ResourceNames.ConsentForm resourceName);
        ImageSource GetImageSource(ResourceNames.StartPage resourceName);

        string GetString(ResourceNames.MainWindow resourceName);
        string GetString(ResourceNames.ConsentForm resourceName);

        Window CreateAboutBox(DynamoViewModel model);
    }
}
