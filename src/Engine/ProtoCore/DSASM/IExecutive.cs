namespace ProtoCore.DSASM
{
    interface IExecutive
    {
        Executable exe { get; set; }
    }

    public interface IExecutiveProvider
    {
        Executive CreateExecutive(RuntimeCore runtimeCore, bool isFep);
    }

    public class ExecutiveProvider : IExecutiveProvider
    {
        public Executive CreateExecutive(RuntimeCore runtimeCore, bool isFep)
        {
            return new Executive(runtimeCore, isFep);
        }
    }

}
