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
            AgreementThree,
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
        string ProductName { get; }

        /// <summary>
        /// This property returns the full file path to the additional terms of 
        /// use document, if any. The terms of use will be shown in addition to 
        /// the existing Dynamo TermsOfUse.rtf before the user is allowed to 
        /// publish any package. If specified, this property should represent an
        /// existing file, otherwise an exception will be thrown. If there is no
        /// additional terms of use document to show, this property should return
        /// an empty string.
        /// </summary>
        string AdditionalPackagePublisherTermsOfUse { get; }
    }
}
