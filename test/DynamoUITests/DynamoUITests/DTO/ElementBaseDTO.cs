namespace DynamoTests.DTO
{
    public class ElementBaseDTO
    {
        public string AccessibilityId { get; set; }
        public string Text { get; set; }
        public string Name { get { return Text; } }
    }
}