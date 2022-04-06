namespace ProtoScript.Runners
{
    public partial class LiveRunner
    {
        private abstract class Task
        {
            protected LiveRunner runner;
            protected Task(LiveRunner runner)
            {
                this.runner = runner;
            }
            public abstract void Execute();
        }
    }
}