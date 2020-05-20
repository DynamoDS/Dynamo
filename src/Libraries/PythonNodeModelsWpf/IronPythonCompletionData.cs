using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media.Imaging;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Dynamo.Python
{
    /// Implements AvalonEdit ICompletionData interface to provide the entries in the
    /// completion drop down.
    public class IronPythonCompletionData : ICompletionData
    {
        
        private static Dictionary<CompletionType, BitmapImage> TypeToIcon;
        private IronPythonCompletionProvider provider;

        public enum CompletionType
        {
            NAMESPACE,
            METHOD,
            FIELD,
            CLASS,
            PROPERTY,
            ENUM
        };

        public IronPythonCompletionData(string text, string stub, bool isInstance, CompletionType type, IronPythonCompletionProvider provider)
        {
            this.Text = text;
            this.Stub = stub;
            this.IsInstance = isInstance;
            this.provider = provider;

            if (IronPythonCompletionData.TypeToIcon == null)
            {
                var assembly = Assembly.GetExecutingAssembly();

                TypeToIcon = new Dictionary<CompletionType, BitmapImage>();
                TypeToIcon.Add(CompletionType.METHOD, GetBitmapImage(assembly, "method.png"));
                TypeToIcon.Add(CompletionType.NAMESPACE, GetBitmapImage(assembly, @"namespace.png"));
                TypeToIcon.Add(CompletionType.FIELD, GetBitmapImage(assembly, @"field.png"));
                TypeToIcon.Add(CompletionType.CLASS, GetBitmapImage(assembly, @"class.png"));
                TypeToIcon.Add(CompletionType.PROPERTY, GetBitmapImage(assembly, @"property.png"));
                TypeToIcon.Add(CompletionType.ENUM, GetBitmapImage(assembly, @"property.png"));
            }

            this._image = TypeToIcon[type];
        }
        
        // image
        private BitmapImage _image;
        public System.Windows.Media.ImageSource Image
        {
            get
            {
                return _image;
            }
        }

        public string Text { get; private set; }

        public string Stub { get; private set; }

        public bool IsInstance { get; private set; }

        // Use this property if you want to show a fancy UIElement in the drop down list.
        public object Content
        {
            get { return this.Text;  }
        }

        // description
        private string _description;
        
        public object Description
        {
            get {
                // lazily get the description
                if (_description == null)
                {
                    _description = provider.GetDescription(this.Stub, this.Text, this.IsInstance).TrimEnd('\r', '\n');
                }

                return _description;
            }
        }

        public double Priority { get { return 0; } }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }

        private BitmapImage GetBitmapImage(Assembly assembly, string resourceFileName)
        {
            var name = string.Format(@"PythonNodeModelsWpf.Resources.{0}", resourceFileName);

            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.StreamSource = assembly.GetManifestResourceStream(name);
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }

}
