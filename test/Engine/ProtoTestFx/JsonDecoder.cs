using System;
using System.Collections.Generic;
using System.IO;
using ProtoCore.Utils;
using Newtonsoft.Json;

namespace ProtoTestFx
{
    public static class JsonDecoder
    {
        /// <summary>
        /// Validity check for the Json verification string
        /// </summary>
        /// <param name="verificationFormatJSON"></param>
        /// <returns></returns>
        private static bool ValidateJsonString(string verificationFormatJSON)
        {
            // TODO
            return true;
        }

        /// <summary>
        /// Parses the json string and returns a Dictionary of verification pairs
        /// </summary>
        /// <param name="verificationFormatJSON"></param>
        /// <returns></returns>
        public static Dictionary<string, object> BuildVerifyDictionary(string verificationFormatJSON)
        {
            bool isStringValid = ValidateJsonString(verificationFormatJSON);

            // Immediately fail if the string format is not valid
            Validity.Assert(isStringValid);

            Dictionary<string, object> verification = new Dictionary<string, object>();
            JsonTextReader reader = new JsonTextReader(new StringReader(verificationFormatJSON));
            while (reader.Read())
            {
                // Start of new item
                if (reader.TokenType == JsonToken.StartObject)
                {
                    // Start of new entry
                    // Ignore and move to the entry pair
                }
                else if (reader.TokenType == JsonToken.PropertyName)
                {
                    KeyValuePair<string, object> pair = GetVerifyPair(reader);
                    if (verification.ContainsKey(pair.Key))
                    {
                        verification[pair.Key] = pair.Value;
                    }
                    else
                    {
                        verification.Add(pair.Key, pair.Value);
                    }
                }
            }
            return verification;
        }

        private static bool IsPrimitiveType(JsonToken type)
        {
            bool isPrimitive = type == JsonToken.Integer
                || type == JsonToken.Integer
                || type == JsonToken.Float
                || type == JsonToken.String
                || type == JsonToken.Boolean
                || type == JsonToken.Null;
            return isPrimitive;
        }

        /// <summary>
        /// Returns a single verification pair from the current state of the reader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static KeyValuePair<string, object> GetVerifyPair(JsonTextReader reader)
        {
            Validity.Assert(reader.TokenType == JsonToken.PropertyName);

            // Get the varname
            string varname = reader.Value as string;

            // Move to the data value
            reader.Read();
            object data = GetValue(reader);

            return new KeyValuePair<string, object>(varname, data);
        }

        private static object GetValue(JsonTextReader reader)
        {
            JsonToken type = reader.TokenType;
            if(IsPrimitiveType(type))
            {
                return GetPrimitiveValue(reader);
            }
            else if (type == JsonToken.StartArray)
            {
                return GetArrayValue(reader);
            }
            else
            {
                Validity.Assert(false, "Type not recognized");
            }
            return null;
        }

        private static object GetPrimitiveValue(JsonTextReader reader)
        {
            Validity.Assert(IsPrimitiveType(reader.TokenType));
            object value = reader.Value;
            return value;
        }

        /// <summary>
        /// Generates an object array from the json array
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static object GetArrayValue(JsonTextReader reader)
        {
            Validity.Assert(reader.TokenType == JsonToken.StartArray);

            List<object> array = new List<object>();

            // Move to the first element
            reader.Read(); 
            JsonToken token = reader.TokenType;

            while (token != JsonToken.EndArray)
            {
                // Get the current element
                object arrayElement = GetValue(reader);
                array.Add(arrayElement);

                // Next element
                reader.Read();
                token = reader.TokenType;
            }
            return array.ToArray();
        }
    }
}
