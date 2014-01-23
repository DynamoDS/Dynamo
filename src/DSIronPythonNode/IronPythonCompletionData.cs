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
                TypeToIcon = new Dictionary<CompletionType, BitmapImage>();
                var assembly = Assembly.GetExecutingAssembly();

                var bi = new BitmapImage();

                bi.BeginInit();
                bi.StreamSource = assembly.GetManifestResourceStream(@"DynamoPython.Resources.method.png");
                bi.EndInit();
                TypeToIcon.Add(CompletionType.METHOD, bi);

                var b2 = new BitmapImage();

                b2.BeginInit();
                b2.StreamSource = assembly.GetManifestResourceStream(@"DynamoPython.Resources.namespace.png");
                b2.EndInit();
                TypeToIcon.Add(CompletionType.NAMESPACE, b2);

                var b3 = new BitmapImage();

                b3.BeginInit();
                b3.StreamSource = assembly.GetManifestResourceStream(@"DynamoPython.Resources.field.png");
                b3.EndInit();
                TypeToIcon.Add(CompletionType.FIELD, b3);

                var b4 = new BitmapImage();

                b4.BeginInit();
                b4.StreamSource = assembly.GetManifestResourceStream(@"DynamoPython.Resources.class.png");
                b4.EndInit();
                TypeToIcon.Add(CompletionType.CLASS, b4);

                var b5 = new BitmapImage();

                b5.BeginInit();
                b5.StreamSource = assembly.GetManifestResourceStream(@"DynamoPython.Resources.property.png");
                b5.EndInit();
                TypeToIcon.Add(CompletionType.PROPERTY, b5);

                var b6 = new BitmapImage();

                b6.BeginInit();
                b6.StreamSource = assembly.GetManifestResourceStream(@"DynamoPython.Resources.property.png");
                b6.EndInit();
                TypeToIcon.Add(CompletionType.ENUM, b6);
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

    }

}
