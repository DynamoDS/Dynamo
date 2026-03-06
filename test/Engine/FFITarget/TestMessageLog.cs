namespace FFITarget
{
    public class TestLogWarning
    {
        public const string DefaultTestWarningString = @"Log me as a warning.";
        public TestLogWarning()
        {
        }

        public static int Ctor()
        {
            DynamoServices.LogWarningMessageEvents.OnLogWarningMessage(DefaultTestWarningString);
            return 0;
        }
    }

    public class TestVersionedInfoMessage
    {
        public const string InfoMessageText = @"This is a versioned info message.";
        public const string InfoMessageWithoutVersionText = @"This is an info message without version.";

        public TestVersionedInfoMessage()
        {
        }

        /// <summary>
        /// Returns a value and logs an info message with version 3.0.0
        /// </summary>
        public static int WithVersion()
        {
            var version = new System.Version(3, 0, 0);
            DynamoServices.LogWarningMessageEvents.OnLogInfoMessage(InfoMessageText, version);
            return 42;
        }

        /// <summary>
        /// Returns a value and logs an info message without version
        /// </summary>
        public static int WithoutVersion()
        {
            DynamoServices.LogWarningMessageEvents.OnLogInfoMessage(InfoMessageWithoutVersionText);
            return 99;
        }
    }
    
}

