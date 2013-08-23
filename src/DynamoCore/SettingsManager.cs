using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Dynamo.Utilities;

namespace Dynamo
{
    public class SettingsManager
    {
        public void Load()
        {
            var json = "{\"id\":\"[13]\", \"value\": [true]}";

            // load the defaults 
            // load user settings 
            var ds = new RestSharp.Deserializers.JsonDeserializer();
            var jss = new JavaScriptSerializer();
            var table = jss.Deserialize<Dictionary<string, List<object>>>(json);

        }

        public void Load(string filename)
        {

        }

        public void SaveUserSettings()
        {

        }




    }
}
