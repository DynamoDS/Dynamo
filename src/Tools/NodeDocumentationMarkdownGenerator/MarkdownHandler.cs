using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Dynamo.Wpf.Interfaces;
using ImageMagick;
using Newtonsoft.Json;

namespace NodeDocumentationMarkdownGenerator
{
    internal static class MarkdownHandler
    {
        /// <summary>
        /// Creates markdown files from the given collection of fileInfos
        /// </summary>
        /// <param name="fileInfos">Collection of files to create</param>
        /// <param name="outputDir">Folder path where files should be created</param>
        /// <param name="overWrite">if true, files in outputDir will be overwritten</param>
        /// <param name="compressImages">if true images matched from dictionary will be compressed (if possible)</param>
        /// <param name="compressGifs">if true animated gifs matched from dictionary will be compressed (if possible)</param>
        /// <param name="dictionaryPath">path to dictionary json file</param>
        /// <param name="layoutSpec">path to layout spec json</param>
        internal static void CreateMdFilesFromFileNames(
            IEnumerable<MdFileInfo> fileInfos, string outputDir, bool overWrite, 
            bool compressImages = false, bool compressGifs = false, string dictionaryPath = null, string layoutSpec = null)
        {
            ImageOptimizer optimizer = null;
            LayoutSpecification spec = null;
            List<DynamoDictionaryEntry> dictEntrys = null;
            string examplesDirectory = "";

            Console.WriteLine($"Starting generation of {fileInfos.Count()} markdown files...");

            // If there is a Dictionary path provided we do a couple of things. 
            if (!string.IsNullOrEmpty(dictionaryPath) &&
                    File.Exists(dictionaryPath))
            {
                // First we create DynamoDictionaryEntry's from the dictionary Json file.
                // This reflects the state of https://github.com/DynamoDS/DynamoDictionary at commit 1c259e6549de899793c1f99724c0535e9db46ad0
                var dictionaryJson = File.ReadAllText(dictionaryPath);
                dictEntrys = JsonConvert.DeserializeObject<List<DynamoDictionaryEntry>>(dictionaryJson);

                // Then we save a reference to the `EXAMPLES` folder from the Dynamo Dictionary repo.
                // This folder contains all images and sample files used in the dictionary.
                var mainDirectory = new FileInfo(dictionaryPath).Directory;
                examplesDirectory = Path.Combine(mainDirectory.FullName, "EXAMPLES");

                if (compressImages)
                {
                    optimizer = new ImageOptimizer();
                    optimizer.OptimalCompression = true;
                }
                if (!string.IsNullOrEmpty(layoutSpec))
                {
                    var layoutSpecString = File.ReadAllText(layoutSpec);
                    spec = JsonConvert.DeserializeObject<LayoutSpecification>(layoutSpecString);
                }
            }

            var filesCreatedCount = 0;
            foreach (var info in fileInfos)
            {
                var fileName = $"{info.FileName}.md";
                var filePath = Path.Combine(outputDir, fileName);

                var fileInfo = new FileInfo(filePath);

                if (File.Exists(filePath) &&
                    !overWrite)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Creation of {fileName} was skipped as the file already exist and the --overwrite flag was set to false");
                    Console.ResetColor();
                    continue;
                }

                string fileContent = null;

                // If there are any dictEntrys, we try and match the current fileInfo with an entry in the dictionary.
                // If a match is found the fileContent is set with the information in that entry.
                if (dictEntrys != null)
                {
                    DynamoDictionaryEntry matchingEntry = GetMatchingDictionaryEntry(dictEntrys, info, spec);
                    if (matchingEntry != null)
                    {
                        fileContent = GetContentFromDictionaryEntry(matchingEntry, examplesDirectory, optimizer,compressGifs, fileInfo);    
                    }
                }

                // If there are no dictEntrys or no match could be found, we set the fileContent to the default content.
                if (fileContent is null)
                {
                    fileContent = GetDefaultContent(info.NodeName);

                    // If we get to here and there aren't any dictEntrys, that means the dictionary is missing an entry for this node
                    // we log this to the console.
                    if (dictEntrys != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"No matching Dictionary entry found for {fileName} - file will only contain default content.");
                        Console.ResetColor();
                    }
                }

                try
                {
                    using (StreamWriter sw = File.CreateText(filePath))
                    {
                        sw.WriteLine(fileContent);
                    }
                }
                catch (Exception e)
                {
                    CommandHandler.LogExceptionToConsole(e);
                    continue;
                }
                filesCreatedCount++;
            }

            Console.WriteLine($"{filesCreatedCount} documentation files created");
        }

        private static string GetDefaultContent(string nodeName)
        {
            var content = new StringBuilder();
            content.AppendLine(string.Format(Properties.Resources.DocumentationHeader, nodeName));
            content.AppendLine(string.Format(Properties.Resources.DefaultContentInfo, Assembly.GetExecutingAssembly().GetName()));
            content.AppendLine();
            content.AppendLine(Properties.Resources.DefaultContentSeeMore);
            return content.ToString();
        }

        private static string GetContentFromDictionaryEntry(
            DynamoDictionaryEntry entry, string examplesDirectory, 
            ImageOptimizer optimizer, bool compressGifs, FileInfo fileInfo)
        {
            var imgDir = new DirectoryInfo(Path.Combine(examplesDirectory, entry.FolderPath, "img"));

            // Collection of any missing content from the DynamoDictionaryEntry.
            // This is because we want to flag any missing content so it can be addressed.
            var missingFields = new List<string>();

            // Sometimes the dictionary specifies an image file without it actually existing
            // so we check both if the directory and the file exist
            string imageString = string.Empty;
            var imageFile = entry.ImageFile.FirstOrDefault();
            if (imgDir.Exists &&
                imgDir.GetFiles($"{imageFile}.*").Length > 0 &&
                !TrySaveImage(imgDir, imageFile, optimizer, compressGifs, fileInfo, out imageString))
            {
                missingFields.Add("Image");
            }
  
            if (entry.InDepth == string.Empty) missingFields.Add("In Depth description");

            if (missingFields.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{fileInfo.Name} missing {string.Join(", ", missingFields)}");
                Console.ResetColor();
            }

            var content = new StringBuilder();
            content.AppendLine(Properties.Resources.DictionaryContentInDepthHeader);
            content.AppendLine(entry.InDepth);
            content.AppendLine("___");
            content.AppendLine(Properties.Resources.DictionaryContentExampleFileHeader);
            content.AppendLine();
            content.AppendLine(imageString);
            return content.ToString();
        }

        private static bool TrySaveImage(
            DirectoryInfo imgDir, string imgName, 
            ImageOptimizer optimizer,bool compressGifs, FileInfo fileInfo, 
            out string outputImagePath)
        {
            outputImagePath = string.Empty;
            var imageFileInfo = imgDir.GetFiles($"{imgName}.*").FirstOrDefault();

            if (!imageFileInfo.Exists) return false;

            

            try
            {
                using (Image image = Image.FromFile(imageFileInfo.FullName))
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    var oldSize = m.Length;
                    var ext = imageFileInfo.Extension.ToLower();
                    //the simple optimizer fails with animated gifs, or gifs that have already been semi optimized (different size frames)
                    if (optimizer != null && ext != ".gif" )
                    {
                        m.Position = 0;
                        optimizer.Compress(m);
                    }
                    if (compressGifs && ext == ".gif")
                    {
                        using (MagickImageCollection images = new MagickImageCollection(imageFileInfo))
                        {
                            //I've disabled these optimization steps for now because they are extremely slow.
                            //quantization is much faster and produces mostly very good results. (though not lossless)

                            //images.Coalesce();
                            //images.Optimize();
                            //images.OptimizeTransparency();
                            //reduce color bit depth from 8 to 5.
                            images.Quantize(new QuantizeSettings() { Colors = 32768 });
                            m.Position = 0;
                            m.SetLength(0);
                            images.Write(m);
                            //in the case where gif compression fails to create a gif that is smaller
                            //fallback to the old image stream.
                            if(m.Length > oldSize){
                                m.Position = 0;
                                m.SetLength(0);
                                image.Save(m, image.RawFormat);
                            }
                        }
                    }

                    var newsize = m.Length;
                    Program.VerboseControlLog($"reduced {imageFileInfo.Name} from {oldSize} to {newsize} ~{ (int)(100.0 - (((float)newsize / (float)oldSize) * 100.0))}% reduction");
                    var img = Image.FromStream(m);
                    var fileName = $"{Path.GetFileNameWithoutExtension(fileInfo.FullName)}_img{imageFileInfo.Extension}";
                    var path = Path.Combine(fileInfo.Directory.FullName, fileName);
                    img.Save(path);
                    outputImagePath = $"![{imgName}](./{fileName.Replace(" ", "%20")})";
                    
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

        }

        private static DynamoDictionaryEntry GetMatchingDictionaryEntry(
            IEnumerable<DynamoDictionaryEntry> dictEntrys, MdFileInfo info, 
            LayoutSpecification spec)
        {
            // The DynamoDictionary only gives information about the name of the node
            // and the folder path, the MdFileInfo knows about the name of the node and
            // its namespace. The DynamoDictionary Folder path is the same as the namespace
            // except its separated by "/" instead of ".".
            // In order to find the correct dict entry we need to convert the namespace
            // into a folder path, and then get the entry where the folder path and name is matching.
            var infoNameSpaceFolderPath = ConvertNamespaceToDictionaryFolderPath(info.NodeNamespace);
            var infoCategoryFolderPath = ConvertNamespaceToDictionaryFolderPath(info.FullCategory);

            // First we try and find a match using the fileInfo directly.
            // There are cases where a LayoutSpecification is not provided
            // and therefor we need to find a match from the fileInfo directly.
            // There are also instances where the particular node/namespace
            // has not been added to the LayoutSpecification (I think!!) which is
            // another reason to always try and make the match using the fileInfo directly first.
            var matchingEntry = dictEntrys
                .Where(x =>
                    x.FolderPath.StartsWith(infoNameSpaceFolderPath) ||
                    x.FolderPath.StartsWith(infoCategoryFolderPath) &&
                    x.Name == info.NodeName)
                .FirstOrDefault();

            // If we couldn't find a match using the fileInfo directly and a LayoutSpecification is provided
            // we try and find a match using that.
            if (matchingEntry is null && spec != null)
            {
                if (TryGetMatchingEntryFromLayoutSpec(spec.sections.Cast<LayoutElement>().ToList(), dictEntrys, info, string.Empty, out matchingEntry))
                {
                    return matchingEntry;
                }
            }

            return matchingEntry;
        }

        /// <summary>
        /// Trys to find a DictionaryEntry that matches the input MdFileInfo while applying LayoutElement from a LayoutSpecification
        /// by recursively checking LayoutElements from the LayoutSpecification until a match is found.
        /// </summary>
        /// <param name="sections">All Sections coming from a LayoutSpecification</param>
        /// <param name="dictEntrys">All DictionaryEntrys</param>
        /// <param name="info">MdFileInfo we are trying to find a DictionaryEntry match for</param>
        /// <param name="matchingPath">A path matching the DictionaryEntrys path, this path is generated during this recursive function</param>
        /// <param name="matchingEntry">The DictionaryEntry that matches the MdFileInfo</param>
        /// <returns></returns>
        private static bool TryGetMatchingEntryFromLayoutSpec(
            IEnumerable<LayoutElement> sections, IEnumerable<DynamoDictionaryEntry> dictEntrys, 
            MdFileInfo info, string matchingPath, 
            out DynamoDictionaryEntry matchingEntry)
        {
            var nodeNameWithoutArgs = info.NodeName
                .Split('(')
                .FirstOrDefault();

            matchingEntry = null;
            foreach (var item in sections)
            {
                if (!(item is LayoutSection))
                {
                    matchingPath = $"{matchingPath}/{item.text}";
                }

                if (item.include.Count > 0)
                {
                    // We need to test a lot of options here, the dictionary json paths can come from different places it seems
                    // Mostly when dealing with ZT nodes the full category name works, with NodeModels we need to use the namespace
                    var matchingLayoutInfo = item.include.Where(x => 
                            info.FullCategory.StartsWith(x.path) ||
                            $"{info.FullCategory}.{nodeNameWithoutArgs}" == x.path ||
                            info.NodeNamespace == x.path ||
                            info.NodeNamespace.StartsWith(x.path) ||
                            $"{info.NodeNamespace}.{nodeNameWithoutArgs}" == x.path)
                        .FirstOrDefault();

                    if (matchingLayoutInfo != null)
                    {
                        // At this point matchingPath looks something like : "/Display/Color"
                        // therefor we need to remove the first '/'
                        var path = matchingPath.Remove(0, 1);

                        // There are cases where the NodeName has two words separated by a "."
                        // this is true for many of the List nodes in CoreNodeModels, e.g. List.LaceLongest.
                        // The dictionary entry for List.LaceLongest only has LaceLongest in the name
                        // therefor we need to first try and match with the name, and after that try and match with the "List." part removed.
                        var endIndex = info.NodeName.LastIndexOf(".");
                        var nodeNameWithoutPrefix = endIndex > 0 ?
                            info.NodeName.Remove(0, endIndex + 1) :
                            info.NodeName;

                        // First we try and match the folder path that we create in this recursive func with the folder path in the dict entry.
                        // Then we check if either the NodeName on the info object or the nodeNameWithoutPrefix matches the Name from the dict entry.
                        // Lastly we need to make sure that the last part of the infos FullCategory is contained in the folder path of the dict entry.
                        // The last check is needed because the layoutspec does not always match what is in the dictionary json,
                        // heres an example:
                        // When trying to get a matching entry for the node "ByThreePoints" which namespace is
                        // ProtoGeometry.Autodesk.DesignScript.Geometry.Arc, the DynamoCore layout spec puts all
                        // ProtoGeometry.Autodesk.DesignScript.Geometry.Arc under "Curves", which means we get a matchingLayoutInfo
                        // when the matching path is Geometry/Curves, but in the dictionary json
                        // "ByThreePoints" folder path is specified as "Geometry/Curves/Arc/Query"
                        // meaning we are missing the "Arc" part, we can get Arc from the last part of the FullCategory.
                        // If we do not add that part to the matching we might end up with an incorrect entry.
                        var matchingEntries = dictEntrys
                            .Where(x =>
                                x.FolderPath.StartsWith(path) &&
                                // below regex compares the two string without considering whitespace.
                                // we need this as the Dictionary specify overloaded nodes with args like this:
                                // Max (int1, int2) - adding unnecessary white space between node name and args.
                                // the MdFileInfo will specify the same like this : Max(int1, int2).
                                (Regex.Replace(x.Name, @"\s+", "") == Regex.Replace(info.NodeName, @"\s+", "") ||
                                Regex.Replace(x.Name, @"\s+", "") == Regex.Replace(nodeNameWithoutPrefix, @"\s+", "")));

                        if (matchingEntries.Count() > 1)
                        {
                            matchingEntry = matchingEntries
                                .Where(x => x.FolderPath.Contains(info.FullCategory.Split(new char[] { '.' }).Last()))
                                .FirstOrDefault();
                        }
                        else
                        {
                            matchingEntry = matchingEntries.FirstOrDefault();
                        }

                        if (matchingEntry != null) return true;
                    }
                }

                if(TryGetMatchingEntryFromLayoutSpec(item.childElements, dictEntrys, info, matchingPath, out matchingEntry))
                {
                    return true;
                }

                if (string.IsNullOrEmpty(matchingPath)) continue;

                matchingPath = matchingPath.Substring(0, matchingPath.LastIndexOf('/'));
            }

            return false;
        }

        private static string ConvertNamespaceToDictionaryFolderPath(string infoNamespace)
        {           
            var folderPath = infoNamespace
                .Replace(".", "/");

            return folderPath;
        }
    }
}
