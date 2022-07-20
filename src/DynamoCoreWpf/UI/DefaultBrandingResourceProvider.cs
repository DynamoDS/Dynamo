using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.UI.Views;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.Wpf.UI
{
    internal class DefaultBrandingResourceProvider : IBrandingResourceProvider
    {
        #region interface members

        public ImageSource GetImageSource(ResourceNames.ConsentForm resourceName)
        {
            ImageSource image = null;
            switch (resourceName)
            {
                case ResourceNames.ConsentForm.Image:
                    image = new BitmapImage(
                        new Uri(@"pack://application:,,,/DynamoCoreWpf;component/UI/Images/consent_form_image.png",
                            UriKind.Absolute));
                    break;
            }
            return EnsureImageLoaded(image, resourceName.ToString());
        }

        public ImageSource GetImageSource(ResourceNames.StartPage resourceName)
        {
            ImageSource image = null;
            switch (resourceName)
            {
                case ResourceNames.StartPage.Image:
                    image = new BitmapImage(
                        new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/StartPage/dynamo-logo.png",
                            UriKind.Absolute));
                    break;
            }
            return EnsureImageLoaded(image, resourceName.ToString());
        }

        public string GetString(ResourceNames.MainWindow resourceName)
        {
            string resource = string.Empty;
            switch (resourceName)
            {
                case ResourceNames.MainWindow.Title:
                    resource = ProductName;
                    break;
            }
            return EnsureStringLoaded(resource, resourceName.ToString());
        }

        public string GetString(ResourceNames.ConsentForm resourceName)
        {
            string resource = string.Empty;
            switch (resourceName)
            {
                case ResourceNames.ConsentForm.Title:
                    resource = Properties.Resources.UsageReportPromptDialogTitle;
                    break;
            }

            return EnsureStringLoaded(resource, resourceName.ToString());
        }

        public Window CreateAboutBox(DynamoViewModel model)
        {
            return new AboutWindow(model);
        }

        public string ProductName { get { return "Dynamo"; } }

        public string AdditionalPackagePublisherTermsOfUse
        {
            get { return String.Empty; } // No additional terms of use.
        }

        #endregion

        #region private members

        private static ImageSource EnsureImageLoaded(ImageSource image, string name)
        {
            if (image != null)
                return image;

            throw new InvalidEnumArgumentException(
                String.Format("Resource name not handled: {0}", name));
        }

        private static string EnsureStringLoaded(string str, string name)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new InvalidEnumArgumentException(
                    String.Format("Resource name not handled: {0}", name));
            }
            return str;
        }

        #endregion
    }
}
