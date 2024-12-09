
namespace TestAssemblyIsolationNamespace2
{
    /// <summary>
    /// Tests
    /// </summary>
    public class TestAssemblyIsolationClass2
    {
        public static string SerializeSomething(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, new Newtonsoft.Json.JsonSerializerSettings());
        }

        public static object DeserializeSomething(string input)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(input, new Newtonsoft.Json.JsonSerializerSettings());
        }
    }
}
