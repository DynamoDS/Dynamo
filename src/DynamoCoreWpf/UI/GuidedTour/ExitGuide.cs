using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dynamo.Wpf.UI.GuidedTour
{
    internal class ExitGuide
    {
        /// <summary>
        /// Represents the Height of Exit Guide modal
        /// </summary>
        [JsonPropertyName("Height")]
        public double Height { get; set; }

        /// <summary>
        /// Represents the Width of Exit Guide modal
        /// </summary>
        [JsonPropertyName("Width")]
        public double Width { get; set; }

        /// <summary>
        /// Represents the key to the resources related to the Title of Exit Guide modal
        /// </summary>
        [JsonPropertyName("Title")]
        public string Title { get; set; }

        /// <summary>
        /// Represents the formatted text key to the resources related to the Title of Exit Guide modal
        /// </summary>
        [JsonPropertyName("FormattedText")]
        public string FormattedText { get; set; }
    }
}
