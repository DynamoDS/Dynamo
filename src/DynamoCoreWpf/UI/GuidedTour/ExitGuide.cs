using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Wpf.UI.GuidedTour
{
    internal class ExitGuide
    {
        /// <summary>
        /// Represents the Height of Exit Guide modal
        /// </summary>
        [JsonProperty("Height")]
        public double Height { get; set; }

        /// <summary>
        /// Represents the Width of Exit Guide modal
        /// </summary>
        [JsonProperty("Width")]
        public double Width { get; set; }

        /// <summary>
        /// Represents the key to the resources related to the Title of Exit Guide modal
        /// </summary>
        [JsonProperty("Title")]
        public string Title { get; set; }

        /// <summary>
        /// Represents the formatted text key to the resources related to the Title of Exit Guide modal
        /// </summary>
        [JsonProperty("FormattedText")]
        public string FormattedText { get; set; }
    }
}
