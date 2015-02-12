using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.ViewModels
{
    partial class DynamoViewModel 
    {
        public IBrandingResourceProvider BrandingResourceProvider { get; private set; }

        public ImageSource AboutBoxIcon
        {
            get
            {
                return new BitmapImage(
                        new Uri(@"pack://application:,,,/DynamoCoreWpf;component/UI/Images/AboutWindow/logo_about.png",
                            UriKind.Absolute));
            }
        }
    }
}
