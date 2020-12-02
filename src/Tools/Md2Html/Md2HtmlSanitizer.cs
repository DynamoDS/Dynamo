using System;
using Ganss.XSS;

namespace Md2Html
{
    /// <summary>
    /// HtmlSanitizer class that tracks sanitizing updates
    /// </summary>
    internal class Md2HtmlSanitizer : HtmlSanitizer
    {
        /// <summary>
        /// Is True if the content was updated.
        /// </summary>
        internal bool ContentWasUpdated { get; set; }

        internal Md2HtmlSanitizer()
        {
            RemovingAtRule += ChangedEvent;
            RemovingAttribute += ChangedEvent;
            RemovingComment += ChangedEvent;
            RemovingCssClass += ChangedEvent;
            RemovingStyle += ChangedEvent;
            RemovingTag += ChangedEvent;
        }

        private void ChangedEvent(object sender, EventArgs e)
        {
            ContentWasUpdated = true;
        }
    }
}
