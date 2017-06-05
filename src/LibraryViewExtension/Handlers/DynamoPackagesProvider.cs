using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using Dynamo.Models;
using Dynamo.PackageManager;
using Greg.Responses;
using Newtonsoft.Json;

namespace Dynamo.LibraryUI.Handlers
{
    class DynamoPackagesProvider : ResourceProviderBase
    {
        private PackageManagerClient dynamopackagesClient;
        private IEventController controller;
        private Dictionary<string, PackageHeader> packages = new Dictionary<string, PackageHeader>();

        public DynamoPackagesProvider(IEventController controller, DynamoModel dynamoModel)
        {
            this.controller = controller;
            dynamopackagesClient = dynamoModel.GetPackageManagerExtension().PackageManagerClient;
        }

        public override Stream GetResource(IRequest request, out string extension)
        {
            extension = "json";
            object data = null;
            Uri url = new Uri(request.Url);
            var segments = url.Segments;
            if (segments.Length < 2) return null;

            switch (segments[1])
            {
                case "packages":
                    if (!packages.Any())
                    {
                        return GetPackagesStream();
                    }
                    data = GetAllPackages();
                    break;
                case "package":
                case "package/":
                    data = GetPackage(segments[2]);
                    break;
                default:
                    break;
            }

            if (data == null) return null;

            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            var serializer = new JsonSerializer();
            serializer.Serialize(sw, data);

            sw.Flush();
            ms.Position = 0;
            return ms;
        }

        private Stream GetPackagesStream()
        {
            Task.Run(() =>
            {
                packages = dynamopackagesClient.ListAll().ToDictionary(p => p._id);
            });
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream("Dynamo.LibraryUI.web.packageManager.packages.json");
        }

        private ResponseWithContentBody<PackageHeader> GetPackage(string id)
        {
            PackageHeader header;
            if (!packages.TryGetValue(id, out header))
            {
                var result = dynamopackagesClient.DownloadPackageHeader(id, out header);
                if (!result.Success)
                {
                    controller.RaiseEvent("error", result.Error);
                    return null;
                }
                packages.Add(id, header);
            }
            if(header.keywords == null)
            {
                header.keywords = new List<string>();
            }

            return new ResponseWithContentBody<PackageHeader>()
            {
                content = header,
                message = "Success",
                success = true
            };
        }

        private ResponseWithContentBody<IEnumerable<PackageHeader>> GetAllPackages()
        {
            if (!packages.Any())
            {
                packages = dynamopackagesClient.ListAll().ToDictionary(p => p._id);
            }

            return new ResponseWithContentBody<IEnumerable<PackageHeader>>()
            {
                content = packages.Values,
                message = "Found packages",
                success = true
            };
        }
    }
}
