using System.Collections.Generic;

namespace ProtoCore.DSASM
{
    interface IExecutive
    {
        Executable exe { get; set; }
    }

    public interface IExecutiveProvider
    {
        Executive CreateExecutive(Core core, RuntimeCore runtimeCore, bool isFep);
    }

    public class ExecutiveProvider : IExecutiveProvider
    {
        public Executive CreateExecutive(Core core, RuntimeCore runtimeCore, bool isFep)
        {
            return new Executive(core, runtimeCore, isFep);
        }
    }

}
