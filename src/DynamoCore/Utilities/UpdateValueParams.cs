using ProtoCore.Namespace;

namespace Dynamo.Utilities
{
    public class UpdateValueParams
    {
        public UpdateValueParams(string value, string propertyName, ElementResolver elementResolver = null)
        {
            ElementResolver = elementResolver;
            PropertyName = propertyName;
            Value = value;
        }

        public string PropertyName { get; private set; }
        public string Value { get; private set; }
        public ElementResolver ElementResolver { get; private set; }
    }
}
