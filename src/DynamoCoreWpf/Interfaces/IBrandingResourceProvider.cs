using System.Windows.Media;

namespace Dynamo.Wpf.Interfaces
{
    public enum ResourceName
    {
        StartPageLogo,
        AboutBoxLogo,
        UsageConsentFormImage,
        
    }

    public enum UsageConsentFormStringResource
    {
        UsageConsentFormTitle,
        UsageConsentFormMessage1,
        UsageConsentFormFeatureUsage,
        UsageConsentFormNodeUsage,
        UsageConsentFormMessage2,
        UsageConsentFormConsent
    }

    public interface IBrandingResourceProvider
    {
        ImageSource GetImageSource(ResourceName resourceName);
        string GetString(ResourceName resourceName);
        string GetUsageConsentDialogString(UsageConsentFormStringResource resourceName);
    }
}
