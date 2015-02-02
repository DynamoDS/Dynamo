using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Interfaces
{
    public enum ResourceName
    {
        StartPageLogo,
        AboutBoxLogo
    }

    public interface IHostApplication
    {
        string GetImageSource(ResourceName resourceName);
        string GetStringResource(ResourceName resourceName);
    }
}
