using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Dynamo.Interfaces
{
    public enum ResourceName
    {
        StartPageLogo,
        AboutBoxLogo
    }

    public interface IHostApplication
    {
        ImageSource GetImageSource(ResourceName resourceName);
        string GetStringResource(ResourceName resourceName);
    }
}
