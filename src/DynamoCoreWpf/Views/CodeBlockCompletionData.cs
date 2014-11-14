using Dynamo.UI.Controls;
using Dynamo.DSEngine.CodeCompletion;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ProtoCore.Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media.Imaging;


namespace Dynamo.UI
{
    class CodeBlockCompletionData : ICompletionData
    {
        #region Class data members

        private static Dictionary<CompletionData.CompletionType, BitmapImage> TypeToIcon;
        
        // image
        public System.Windows.Media.ImageSource Image { get; private set; }

        public string Text { get; private set; }

        public string Stub { get; private set; }

        // Use this property if you want to show a fancy UIElement in the drop down list.
        public object Content
        {
            get { return this.Text; }
        }

        // TODO: Implement this, tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5232
        public object Description { get; private set; }

        // TODO: Implement this, tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5231
        /// <summary>
        /// This property can be used in the selection logic. You can use it to
        /// prefer selecting those items which the user is accessing most frequently
        /// </summary>
        public double Priority { get; private set; }

        #endregion

        public CodeBlockCompletionData(CompletionData completionData)
        {
            this.Text = completionData.Text;
            this.Stub = completionData.Stub;
            //this.Description = completionData.Description;
            
            if (CodeBlockCompletionData.TypeToIcon == null)
            {
                var assembly = Assembly.GetExecutingAssembly();

                TypeToIcon = new Dictionary<CompletionData.CompletionType, BitmapImage>();
                TypeToIcon.Add(CompletionData.CompletionType.Method, GetBitmapImage(assembly, "method.png"));
                TypeToIcon.Add(CompletionData.CompletionType.Constructor, GetBitmapImage(assembly, "constructor.png"));
                TypeToIcon.Add(CompletionData.CompletionType.Class, GetBitmapImage(assembly, @"class.png"));
                TypeToIcon.Add(CompletionData.CompletionType.Property, GetBitmapImage(assembly, @"property.png"));
                TypeToIcon.Add(CompletionData.CompletionType.Keyword, GetBitmapImage(assembly, @"keyword.png"));
            }

            this.Image = TypeToIcon[completionData.Type];
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }

        private static BitmapImage GetBitmapImage(Assembly assembly, string resourceFileName)
        {
            var name = string.Format(@"Dynamo.Wpf.UI.Images.CodeBlock.{0}", resourceFileName);

            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
   
            bitmapImage.StreamSource = assembly.GetManifestResourceStream(name);

            bitmapImage.EndInit();
            return bitmapImage;
        }
    }
}
