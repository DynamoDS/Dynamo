using Dynamo.UI.Controls;
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

namespace Dynamo.Nodes
{
    class CodeBlockCompletionData : ICompletionData
    {
        private static Dictionary<CompletionType, BitmapImage> TypeToIcon;
        private CodeBlockEditor codeEditor;

        public enum CompletionType
        {
            Namespace,
            Method,
            Constructor,
            Class,
            Property,
        };

        public CodeBlockCompletionData(string text, string stub, bool isInstance, CompletionType type, CodeBlockEditor codeEditor)
        {
            this.Text = text;
            this.Stub = stub;
            this.IsInstance = isInstance;
            this.codeEditor = codeEditor;

            if (CodeBlockCompletionData.TypeToIcon == null)
            {
                var assembly = Assembly.GetExecutingAssembly();

                TypeToIcon = new Dictionary<CompletionType, BitmapImage>();
                TypeToIcon.Add(CompletionType.Method, GetBitmapImage(assembly, "method.png"));
                TypeToIcon.Add(CompletionType.Constructor, GetBitmapImage(assembly, "constructor.png"));
                TypeToIcon.Add(CompletionType.Class, GetBitmapImage(assembly, @"class.png"));
                TypeToIcon.Add(CompletionType.Property, GetBitmapImage(assembly, @"property.png"));
            }

            this.image = TypeToIcon[type];
        }
        
        // image
        private BitmapImage image;
        public System.Windows.Media.ImageSource Image
        {
            get
            {
                return image;
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
        private string description;
        
        // TODO: Implement this
        public object Description
        {
            get {
                // lazily get the description
                if (description == null)
                {
                    //description = codeEditor.GetDescription();
                }

                return description;
            }
        }

        public double Priority { get { return 0; } }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }

        internal static CodeBlockCompletionData ConvertMirrorToCompletionData(StaticMirror mirror, CodeBlockEditor codeEditor)
        {
            MethodMirror method = mirror as MethodMirror;
            if (method != null)
            {
                string methodName = method.MethodName;
                string signature = "";
                CompletionType type = method.IsConstructor ? CompletionType.Constructor : CompletionType.Method;
                return new CodeBlockCompletionData(methodName, signature, !method.IsStatic, type, codeEditor);
            }
            PropertyMirror property = mirror as PropertyMirror;
            if (property != null)
            {
                string propertyName = property.PropertyName;
                string stub = "";
                return new CodeBlockCompletionData(propertyName, stub, !property.IsStatic, CompletionType.Property, codeEditor);
            }
            return null;
        }

        private static BitmapImage GetBitmapImage(Assembly assembly, string resourceFileName)
        {
            var name = string.Format(@"Dynamo.UI.Images.CodeBlock.{0}", resourceFileName);

            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            var resources = assembly.GetManifestResourceNames();
            bitmapImage.StreamSource = assembly.GetManifestResourceStream(name);
            
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }
}
