using System;
using Ganss.XSS;

namespace Md2Html
{
    /// <summary>
    /// HtmlSanitizer class that tracks sanitizing changes
    /// </summary>
    internal class Md2HtmlSanitizer : HtmlSanitizer
    {
        /// <summary>
        /// Is True if any content needed to be sanitized.
        /// </summary>
        internal bool Changed { get; set; }

        internal Md2HtmlSanitizer()
        {
            RemovingAtRule += ChangedEvent;
            RemovingAttribute += ChangedEvent;
            RemovingComment += ChangedEvent;
            RemovingCssClass += ChangedEvent;
            RemovingStyle += ChangedEvent;
            RemovingTag += ChangedEvent;
        }

        ~Md2HtmlSanitizer()
        {
            RemovingAtRule -= ChangedEvent;
            RemovingAttribute -= ChangedEvent;
            RemovingComment -= ChangedEvent;
            RemovingCssClass -= ChangedEvent;
            RemovingStyle -= ChangedEvent;
            RemovingTag -= ChangedEvent;
        }

        private void ChangedEvent(object sender, EventArgs e)
        {
            Changed = true;
        }
    }
}
