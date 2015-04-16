using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;


namespace Dynamo.ViewModels
{
    public class TermsOfUseViewModel : NotificationObject
    {
        public string TermsOfUseFile
        {
            get
            {
                string executingAssemblyPathname = Assembly.GetExecutingAssembly().Location;
                string rootModuleDirectory = Path.GetDirectoryName(executingAssemblyPathname);
                return Path.Combine(rootModuleDirectory, "TermsOfUse.rtf");
            }
        }
    }
}
