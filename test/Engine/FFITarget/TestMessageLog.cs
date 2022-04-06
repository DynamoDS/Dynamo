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
    
}
