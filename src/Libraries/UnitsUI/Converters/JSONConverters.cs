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
            string typedId = System.Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            return DynamoUnits.Quantity.ByTypeID(typedId);
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
            string typedId = System.Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            return DynamoUnits.Unit.ByTypeID(typedId);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var unit = (DynamoUnits.Unit)value;
            writer.WriteValue(unit.TypeId);
        }
    }
}
