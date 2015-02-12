using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.ViewModels
{
    partial class DynamoViewModel 
    {
        public IBrandingResourceProvider BrandingResourceProvider { get; private set; }
    }
}
