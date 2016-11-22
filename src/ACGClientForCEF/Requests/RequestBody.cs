using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACGClientForCEF.Requests
{
    public abstract class CefRequestBody
    {
        public static JsonSerializer jsonSerializer = new JsonSerializer();

        public virtual string AsJson()
        {
            return jsonSerializer.Serialize(this);
        }
    }
}
