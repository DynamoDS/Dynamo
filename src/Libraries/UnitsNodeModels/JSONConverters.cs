using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitsUI.Converters
{

    internal class ForgeQuantityConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DynamoUnits.Quantity);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var typedId = System.Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            try
            {
                return DynamoUnits.Quantity.ByTypeID(typedId);
            }
            catch
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var quantity = (DynamoUnits.Quantity)value;
            writer.WriteValue(quantity.TypeId);
        }
    }

    internal class ForgeUnitConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DynamoUnits.Unit);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var typedId = System.Convert.ToString(reader.Value, CultureInfo.InvariantCulture);

            try
            {
                return DynamoUnits.Unit.ByTypeID(typedId);
            }
            catch
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var unit = (DynamoUnits.Unit)value;
            writer.WriteValue(unit.TypeId);
        }
    }

    internal class ForgeSymbolConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DynamoUnits.Symbol);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var typedId = System.Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            try
            {
                return DynamoUnits.Symbol.ByTypeID(typedId);
            }
            catch
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var symbol = (DynamoUnits.Symbol)value;
            writer.WriteValue(symbol.TypeId);
        }
    }
}
