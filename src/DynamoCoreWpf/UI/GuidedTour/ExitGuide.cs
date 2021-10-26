using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Wpf.UI.GuidedTour
{
    public class ExitGuide
    {
        [JsonProperty("Height")]
        public double Height { get; set; }

        [JsonProperty("Width")]
        public double Width { get; set; }

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("FormattedText")]
        public string FormattedText { get; set; }
    }
}
