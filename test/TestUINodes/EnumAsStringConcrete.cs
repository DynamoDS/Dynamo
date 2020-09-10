using System.Xml;
using CoreNodeModels;
using Dynamo.Graph;

namespace TestUINodes
{
    /// <summary>
    /// Class created in order to test methods in EnumAsString and parent classes
    /// </summary>
    public class EnumAsStringConcrete : EnumAsString<testEnum>
    {
        public EnumAsStringConcrete() : base() {}

        public string PopulateItemsCore(string currentSelection) => 
            base.PopulateItemsCore(currentSelection).ToString();

        public void SerializeCore(XmlElement nodeElement, SaveContext context) =>
            base.SerializeCore(nodeElement, context);

        public void DesrializeCore(XmlElement nodeElement, SaveContext context) =>
            base.DeserializeCore(nodeElement, context);

        public bool UpdateValueCore(UpdateValueParams updateValueParams) => 
            base.UpdateValue(updateValueParams);
    }

    /// <summary>
    /// Enum created for testing
    /// </summary>
    public enum testEnum
    {
        A,
        B,
        C
    }
}
