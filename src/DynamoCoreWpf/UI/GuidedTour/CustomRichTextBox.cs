using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This class represent a RichTextBox that will be used in Popups to show hyperlinks, texts, images, bullet points, gifs and other media items.
    /// </summary>
    public class CustomRichTextBox : RichTextBox
    {

        #region CustomText Dependency Property

        public static readonly DependencyProperty CustomTextProperty = DependencyProperty.Register("CustomText", typeof(string), typeof(CustomRichTextBox),
       new PropertyMetadata(string.Empty, CustomTextChangedCallback), CustomTextValidateCallback);

        private static void CustomTextChangedCallback(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as CustomRichTextBox).Document = GetCustomDocument(e.NewValue as string);
        }

        private static bool CustomTextValidateCallback(object value)
        {
            return value != null;
        }

        /// <summary>
        /// This Custom property is used for registing the dependency property
        /// </summary>
        public string CustomText
        {
            get
            {
                return (string)GetValue(CustomTextProperty);
            }
            set
            {
                SetValue(CustomTextProperty, value);
            }
        }

        #endregion

        /// <summary>
        /// This is the main method of the CustomRichTextBox and is parsing the text provided in order to show hyperlinks, images, bullet points, words in bold
        /// </summary>
        /// <param name="Text">Text using a specific format for showing hyperlinks, images, bullet points, words in bold</param>
        /// <returns></returns>
        private static FlowDocument GetCustomDocument(string Text)
        {
            //This will remove the space between each paragraph
            FlowDocument document = new FlowDocument()
            {
                PagePadding = new Thickness(0)
            };

            //This will get a list of all the paragraphs in the text (each paragraph is identified by '\n')
            List<string> paragraphsList = Text.Replace("\r", "").Split('\n').ToList();

            bool bBoldActive = false;
            bool bImageActive = false;
            bool bBulletListActive = false;
            bool bHyperlinkActive = false;
            string bulletEntryText = string.Empty;
            string imageName = string.Empty;
            string hyperlinkName = string.Empty;

            //This will iterate each paragraph ()
            foreach (string paragraph in paragraphsList)
            {
                Paragraph para = new Paragraph()
                {
                    Margin = new Thickness(0) // remove indent between paragraphs
                };
                BlockUIContainer imageContainer = new BlockUIContainer();
                List bulletedItemsList = null;

                //This iteration is just for words in a paragraph if the text provided has several paragraphs 
                foreach (string word in paragraph.Split(' ').ToList())
                {
                    //A format for bold text was found
                    if (word.Trim().StartsWith("**"))
                    {
                        bBoldActive = true;
                    }

                    //A format for inserting a image between text was found
                    if (word.StartsWith("%"))
                    {
                        bImageActive = true;
                    }

                    //A format for inserting bullet points was found (it can be added between paragraphs but at the end of each paragraph).
                    if (word.StartsWith("-"))
                    {
                        if (bulletedItemsList == null && bBulletListActive == false)
                            bulletedItemsList = new List();

                        bBulletListActive = true;
                    }

                    //A format for inserting a hyperlink between text was found              
                    if (word.StartsWith("#"))
                    {
                        bHyperlinkActive = true;
                    }

                    if (bHyperlinkActive)
                    {
                        //Finding the end character that indicates the hyperlink is complete (no empty spaces are allowed in hyperlink URL just in the name)
                        if (word.Contains("="))
                        {
                            string linkURL = word.Split('=').LastOrDefault();
                            hyperlinkName += word.Split('=').FirstOrDefault();

                            Run run3 = new Run(hyperlinkName.Replace("#", ""));
                            Hyperlink link = new Hyperlink(run3);
                            link.IsEnabled = true;
                            link.NavigateUri = new Uri(linkURL);
                            link.RequestNavigate += (sender, args) => Process.Start(new ProcessStartInfo(args.Uri.ToString()) { UseShellExecute = true });
                            para.Inlines.Add(link);
                            bHyperlinkActive = false;
                        }
                        //The hyperlink name is the next word followed by the # char (empty spaces are allowed) and the URL value is the one followed after the = char
                        else
                        {
                            hyperlinkName += word.Replace("#", "") + " ";
                            continue;
                        }   
                    }
                    else if (bBoldActive)
                    {
                        string boldText = word;
                        boldText = boldText.Replace("**", string.Empty);
                        para.Inlines.Add(new Bold(new Run(boldText)));
                    }
                    else if (bImageActive)
                    {
                        //this will contatenate each word so at the end we will have the full image path
                        imageName += word;
                    }
                    else if (bBulletListActive == true)
                    {
                        //Due that we are using the method Split(' ') for getting each word, then we need to add it back otherwise the words will be all together
                        bulletEntryText += (word + " ");
                    }
                    else
                    {
                        para.Inlines.Add(word);
                    }

                    //End of the text with bold formatting.
                    if (word.Trim().EndsWith("**"))
                    {
                        bBoldActive = false;
                    }
                    //End of the image between text formatting.
                    if (word.EndsWith("%"))
                    {
                        bImageActive = false;

                        var tmpImage = new Image();
                        tmpImage.Source = new BitmapImage(new Uri(imageName.Replace("%", ""), UriKind.RelativeOrAbsolute));
                        imageContainer.Child = tmpImage;
                        Figure imageFig = new Figure(imageContainer);
                        imageFig.Width = new FigureLength(30);
                        imageFig.Height = new FigureLength(30);
                        para.Inlines.Add(imageFig);
                    }
                    //End of the Bullet Items formatting
                    if (word.EndsWith("-") && bulletedItemsList != null)
                    {
                        bBulletListActive = false;
                        bulletedItemsList.ListItems.Add(new ListItem(new Paragraph(new Run(bulletEntryText.Replace("-", "")))));
                        bulletEntryText = string.Empty;
                    }
                    //Due that we are using the method Split(' ') for getting each word, then we need to add it back otherwise the words will be all together
                    para.Inlines.Add(" ");
                }
                document.Blocks.Add(para);

                //Not all the tooltips contain bullet point when we insert the bullet points only if it was set
                if (bulletedItemsList != null)
                {
                    document.Blocks.Add(bulletedItemsList);
                    bulletedItemsList = null;
                }
            }
            return document;
        }
    }
}
