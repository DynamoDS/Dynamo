using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Dynamo.Controls;
using Dynamo.Models;

namespace Dynamo.Wpf
{
    public interface INodeViewInjection : IDisposable
    {
        void SetupCustomUIElements( dynNodeView nodeView);
    }
}
