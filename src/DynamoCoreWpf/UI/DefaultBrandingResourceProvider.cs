using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.Wpf.UI
{
    internal class DefaultBrandingResourceProvider : IBrandingResourceProvider
    {
        public ImageSource GetImageSource(ResourceName resourceName)
        {
            ImageSource image = null;
            switch (resourceName)
            {
                case ResourceName.AboutBoxLogo:
                    image = new BitmapImage(
                        new Uri(@"pack://application:,,,/DynamoCoreWpf;component/UI/Images/AboutWindow/logo_about.png",
                            UriKind.Absolute));
                    break;
                case ResourceName.UsageConsentFormImage:
                    image = new BitmapImage(
                        new Uri(@"pack://application:,,,/DynamoCoreWpf;component/UI/Images/consent_form_image.png",
                            UriKind.Absolute));
                    break;
            }
            if (image == null)
            {
                throw new InvalidEnumArgumentException(
                    String.Format("Resource name not handled: {0}", resourceName));
            }
            return image;
        }

        public string GetString(ResourceName resourceName)
        {
            throw new NotImplementedException();
        }

        public string GetUsageConsentDialogString(UsageConsentFormStringResource resourceName)
        {
            string resource = string.Empty;
            switch (resourceName)
            {
                case UsageConsentFormStringResource.UsageConsentFormTitle:
                    resource = Properties.Resources.UsageReportPromptDialogTitle;
                    break;
                case UsageConsentFormStringResource.UsageConsentFormMessage1:
                    resource = Properties.Resources.UsageReportPromptDialogMessagePart1;
                    break;
                case UsageConsentFormStringResource.UsageConsentFormMessage2:
                    resource = Properties.Resources.UsageReportPromptDialogMessagePart2;
                    break;
                case UsageConsentFormStringResource.UsageConsentFormNodeUsage:
                    resource = Properties.Resources.UsageReportPromptDialogNodeUsage;
                    break;
                case UsageConsentFormStringResource.UsageConsentFormFeatureUsage:
                    resource = Properties.Resources.UsageReportPromptDialogFeatureUsage;
                    break;
                case UsageConsentFormStringResource.UsageConsentFormConsent:
                    resource = Properties.Resources.UsageReportPromptDialogConsent;
                    break;

            }
            if (string.IsNullOrEmpty(resource))
            {
                throw new InvalidEnumArgumentException(
                    String.Format("Resource name not handled: {0}", resourceName));
            }
            return resource;
        }
    }
}
