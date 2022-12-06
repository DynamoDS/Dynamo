using System.Collections.Generic;

/// <summary>
/// The Term DTO refers to Data Transfer Object (design pattern)
/// </summary>
namespace DynamoTests.DTO
{
    public class ControlElementDTO
    {
        public string ControlName { get; set; }
        public string ControlAutomationId { get; set; }
        public string ControlErrorMsg { get; set; }
        
        public List<SubControlDTO> SubControls { get; set; }
    }
}
