using System;
using AngleSharp.Css.Dom;
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
            AllowedTags.Add(@"meta");
            AllowedTags.Add(@"style");

            AllowedAttributes.Add(@"content");
            AllowedAttributes.Add(@"http-equiv");
            AllowedAttributes.Add(@"id");
            AllowedAttributes.Add(@"class");

            AllowedCssProperties.Add(@"src");
            AllowedCssProperties.Add(@"word-break");
            AllowedCssProperties.Add(@"word-wrap");
            AllowedCssProperties.Add(@"-moz-tab-size");
            AllowedCssProperties.Add(@"-o-tab-size");
            AllowedCssProperties.Add(@"tab-size");
            AllowedCssProperties.Add(@"-webkit-hyphens");
            AllowedCssProperties.Add(@"-moz-hyphens");
            AllowedCssProperties.Add(@"-ms-hyphens");
            AllowedCssProperties.Add(@"hyphens");
            AllowedCssProperties.Add(@"background-position-x");
            AllowedCssProperties.Add(@"background-position-y");
            AllowedCssProperties.Add(@"transition-property");
            AllowedCssProperties.Add(@"transition-duration");
            AllowedCssProperties.Add(@"transition-timing-function");
            AllowedCssProperties.Add(@"transition-delay");
            AllowedCssProperties.Add(@"box-shadow");
            AllowedCssProperties.Add(@"position");
            AllowedCssProperties.Add(@"justify-content");
            AllowedCssProperties.Add(@"table-layout");
            AllowedCssProperties.Add(@"align-items");
            AllowedCssProperties.Add(@"background-position");
            AllowedCssProperties.Add(@"background-size");

            AllowedSchemes.Add(@"file");
            AllowedSchemes.Add(@"data");

            AllowedAtRules.Add(CssRuleType.Media);
            AllowedAtRules.Add(CssRuleType.Keyframe);
            AllowedAtRules.Add(CssRuleType.Keyframes);
            AllowedAtRules.Add(CssRuleType.FontFace);

            RemovingAtRule += ChangedEvent;
            RemovingAttribute += ChangedEvent;
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
