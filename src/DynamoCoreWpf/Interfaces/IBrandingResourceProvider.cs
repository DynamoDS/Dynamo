using System.Windows.Media;

namespace Dynamo.Wpf.Interfaces
{
    public struct ResourceNames
    {
        public enum AboutBox
        {
            Title,   
            Image       
        }

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
        ImageSource GetImageSource(ResourceNames.AboutBox resourceName);
        ImageSource GetImageSource(ResourceNames.ConsentForm resourceName);
        ImageSource GetImageSource(ResourceNames.StartPage resourceName);

        string GetString(ResourceNames.MainWindow resourceName);
        string GetString(ResourceNames.AboutBox resourceName);
        string GetString(ResourceNames.ConsentForm resourceName);
    }
}
