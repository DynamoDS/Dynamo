using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
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
                return BrandingResourceProvider.GetImageSource(ResourceName.AboutBoxLogo);
            }
        }
    }
}
