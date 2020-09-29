using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.DesignScript.Interfaces;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Dynamo.Python
{
    /// Implements AvalonEdit ICompletionData interface to provide the entries in the
    /// completion drop down.
    /// TODO: 3.0 We can change this name to PythonCompletionData as we will have the same completion object type for different Python engines. 
    public class IronPythonCompletionData : ICompletionData
    {

        private static Dictionary<CompletionType, BitmapImage> TypeToIcon;
        private readonly IronPythonCompletionProvider provider;

        public enum CompletionType
        {
            NAMESPACE,
            METHOD,
            FIELD,
            CLASS,
            PROPERTY,
            ENUM
        };

        private static readonly Dictionary<ExternalCodeCompletionType, CompletionType> EnumMap =
            new Dictionary<ExternalCodeCompletionType, CompletionType>()
            {
                {ExternalCodeCompletionType.Namespace, CompletionType.NAMESPACE },
                {ExternalCodeCompletionType.Method, CompletionType.METHOD },
                {ExternalCodeCompletionType.Field, CompletionType.FIELD },
                {ExternalCodeCompletionType.Class, CompletionType.CLASS },
                {ExternalCodeCompletionType.Property, CompletionType.PROPERTY },
                {ExternalCodeCompletionType.Enum, CompletionType.ENUM },
            };

        internal static CompletionType ConvertCompletionType(ExternalCodeCompletionType completionType)
        {
            if (EnumMap.ContainsKey(completionType))
            {
                return EnumMap[completionType];
            }
            //if the type can't be found return method by default.
            return CompletionType.METHOD;
        }

        internal IronPythonCompletionData(IExternalCodeCompletionData data)
        {
            this.Text = data.Text;
            this._description = data.Description;

            BuildCompletionTypeToIconMap();

            this._image = TypeToIcon[ConvertCompletionType(data.CompletionType)];

        }
        public IronPythonCompletionData(string text, string stub, bool isInstance, CompletionType type, IronPythonCompletionProvider provider)
        {
            this.Text = text;
            this.Stub = stub;
            this.IsInstance = isInstance;
            this.provider = provider;

            BuildCompletionTypeToIconMap();

            this._image = TypeToIcon[type];
        }

        // image
        private readonly BitmapImage _image;
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
            get { return this.Text; }
        }

        // description
        private string _description;

        public object Description
        {
            get
            {
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

        private static BitmapImage GetBitmapImage(Assembly assembly, string resourceFileName)
        {
            var name = string.Format(@"PythonNodeModelsWpf.Resources.{0}", resourceFileName);

            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.StreamSource = assembly.GetManifestResourceStream(name);
            bitmapImage.EndInit();
            return bitmapImage;
        }

        private static void BuildCompletionTypeToIconMap()
        {
            if (IronPythonCompletionData.TypeToIcon == null || IronPythonCompletionData.TypeToIcon.Count == 0)
            {
                var assembly = Assembly.GetExecutingAssembly();

                TypeToIcon = new Dictionary<CompletionType, BitmapImage>
                {
                    {CompletionType.METHOD, GetBitmapImage(assembly, "method.png")},
                    {CompletionType.NAMESPACE, GetBitmapImage(assembly, @"namespace.png")},
                    {CompletionType.FIELD, GetBitmapImage(assembly, @"field.png")},
                    {CompletionType.CLASS, GetBitmapImage(assembly, @"class.png")},
                    {CompletionType.PROPERTY, GetBitmapImage(assembly, @"property.png")},
                    {CompletionType.ENUM, GetBitmapImage(assembly, @"property.png")}
                };
            }
        }
    }

}
