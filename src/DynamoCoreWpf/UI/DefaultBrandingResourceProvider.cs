using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.Wpf.UI
{
    class DefaultBrandingResourceProvider : IBrandingResourceProvider
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
            }
            return image;
        }

        public string GetStringResource(ResourceName resourceName)
        {
            throw new NotImplementedException();
        }
    }
}
