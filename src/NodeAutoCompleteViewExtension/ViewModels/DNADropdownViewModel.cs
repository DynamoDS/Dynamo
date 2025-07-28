using System.Windows.Media;
using Dynamo.Search.SearchElements;

namespace Dynamo.NodeAutoComplete.ViewModels
{
    public class DNADropdownViewModel
    {
        public string Description { get; set; }
        public string Parameters { get; set; }
        public ImageSource SmallIcon { get; set; }
        internal ClusterResultItem ClusterResultItem { get; set; }
    }
}
