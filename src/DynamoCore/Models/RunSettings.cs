namespace Dynamo.Models
{
    public enum RunType { Manual, Automatic, Periodic }

    public class RunSettings
    {
        private int runPeriod;
        private RunType runType;

        public int RunPeriod
        {
            get { return runPeriod; }
            set
            {
                runPeriod = value;
            }
        }

        public RunType RunType
        {
            get { return runType; }
            set
            {
                runType = value;
            }
        }

        public RunSettings()
        {
            RunPeriod = 100;
            RunType = RunType.Manual;
        }

        public RunSettings(RunType runType, int period)
        {
            RunPeriod = period;
            RunType = runType;
        }
    }
}
