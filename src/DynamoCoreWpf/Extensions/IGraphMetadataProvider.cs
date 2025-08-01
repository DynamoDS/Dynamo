using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.Extensions
{
    public interface IGraphMetadataProvider
    {
        MenuItem GetGraphMetadataMenuItem(DynamoViewModel dynamoViewModel);
    }
}
