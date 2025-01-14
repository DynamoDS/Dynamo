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
    public class IronPythonCompletionData : ICompletionData
    {

        private static Dictionary<CompletionType, BitmapImage> TypeToIcon;

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
            Text = data.Text;

            BuildCompletionTypeToIconMap();

            Image = TypeToIcon[ConvertCompletionType(data.CompletionType)];

            description = new Lazy<string>(() => data.Description);
        }

        public System.Windows.Media.ImageSource Image { get; }

        public string Text { get; private set; }

        public string Stub { get; private set; }

        public bool IsInstance { get; private set; }

        // Use this property if you want to show a fancy UIElement in the drop down list.
        public object Content
        {
            get { return Text; }
        }

        private readonly Lazy<string> description = null;
        public object Description => description?.Value ?? "";


        public double Priority { get { return 0; } }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
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
