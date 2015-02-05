using System.Windows.Media;

namespace Dynamo.Wpf.Interfaces
{
    public enum ResourceName
    {
        StartPageLogo,
        AboutBoxLogo,
        UsageConsentFormImage,
    }

    public interface IBrandingResourceProvider
    {
        ImageSource GetImageSource(ResourceName resourceName);
        string GetString(ResourceName resourceName);
    }
}
