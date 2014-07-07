using System.Configuration;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dynamo.SelfHostAPI
{
    public class WebApiServer
    {
        public static void Run()
        {
            string uri = ConfigurationManager.AppSettings["WebApiUriString"];
            var config = new HttpSelfHostConfiguration(uri);

            config.Routes.MapHttpRoute("API Default", "api/{controller}", new object[0]);
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings();
            config.Formatters.JsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            var server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();
        }
    }
}
