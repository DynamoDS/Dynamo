using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamo.PackageManager;
using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;

namespace Dynamo.DocumentationBrowser
{
    internal class MarkdownHandler
    {
        private const string NODE_ANNOTATION_NOT_FOUND = "Dynamo.DocumentationBrowser.Docs.NodeAnnotationNotFound.md";
        private MarkdownPipeline pipeline;


        private static MarkdownHandler instance;
        public static MarkdownHandler Instance
        {
            get
            {
                if (instance is null) { instance = new MarkdownHandler(); }
                return instance;
            }
        }

        public MarkdownHandler()
        {
            var pipelineBuilder = new MarkdownPipelineBuilder();
            pipeline = pipelineBuilder
                .UseAdvancedExtensions()
                .Build();
        }

        internal void GetAnnotationFromMd(ref StringWriter writer, string mdFilePath)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            string mdString;
            if (string.IsNullOrEmpty(mdFilePath))
                mdString = GetNodeAnnotationNotFoundContent();

            else
            {
                var fileInfo = new FileInfo(mdFilePath);
                while (IsFileLocked(fileInfo))
                {
                    Console.WriteLine("File is busy....");
                }

                using (Stream stream = File.Open(mdFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader reader = new StreamReader(stream))
                {
                    mdString = reader.ReadToEnd();
                }
            }

            var renderer = new HtmlRenderer(writer);
            pipeline.Setup(renderer);

            var document = MarkdownParser.Parse(mdString, pipeline);
            renderer.Render(document);
        }

        private string GetNodeAnnotationNotFoundContent()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var result = "";

            using (Stream stream = assembly.GetManifestResourceStream(NODE_ANNOTATION_NOT_FOUND))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            return result;

        }

        private bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
    }
}
