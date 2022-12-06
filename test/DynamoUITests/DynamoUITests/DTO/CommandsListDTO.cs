namespace DynamoTests.DTO
{
    /// <summary>
    /// Data structure used for testing context menu commands
    /// in the package list control
    /// </summary> 
    public class CommandsListDTO
    {
        /// <summary>
        /// context menu command id to execute
        /// </summary>
        public string CommandId { get; set; }
        /// <summary>
        /// caption of the first message box
        /// (set to empty string or null if no message box is expected)
        /// </summary>
        public string MessageBox1Id { get; set; }
        /// <summary>
        /// caption of the second message box
        /// (set to empty string or null if no message box is expected)
        /// </summary>
        public string MessageBox2Id { get; set; }
    }
}

