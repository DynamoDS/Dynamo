using System.Collections.Generic;
/// <summary>
/// The Term DTO refers to Data Transfer Object (design pattern)
/// </summary>
namespace DynamoTests.DTO
{
    public class TabElementDTO
    {
        public string TabName { get; set; }
        public string TabAutomationId { get; set; }

        public List<ControlElementDTO> TabControls { get; set; }
    }
}
