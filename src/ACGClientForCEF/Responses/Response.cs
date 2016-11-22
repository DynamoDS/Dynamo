using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACGClientForCEF
{
    public class CefResponse
    {
        internal CefResponse(IRestResponse response)
        {
            InternalRestReponse = response;
        }

        public CefResponseBody Deserialize()
        {
            try
            {
                //return jsonDeserializer.Deserialize<CefResponseBody>(InternalRestReponse);
                return JsonConvert.DeserializeObject<CefResponseBody>(InternalRestReponse.Content);
            }
            catch
            {
                return null;
            }
        }

        public IRestResponse InternalRestReponse { get; set; }
    }

    public class CefResponseBody
    {
        public Boolean success { get; set; }
        public string message { get; set; }
    }

    public class CefResponseWithContentBody
    {
        public Boolean success { get; set; }
        public string message { get; set; }
        public dynamic content { get; set; }
    }

    public class ResponseWithContent : CefResponse
    {
        public ResponseWithContent(IRestResponse response) : base(response)
        {

        }

        public CefResponseWithContentBody DeserializeWithContent()
        {
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                }
            };

            return new CefResponseWithContentBody()
            {
                message = InternalRestReponse.StatusDescription,
                content = JsonConvert.DeserializeObject<dynamic>(InternalRestReponse.Content),
                success = true
            };
        }
    }

}