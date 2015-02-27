using System.Collections.Generic;

namespace ProtoCore.DSASM
{
    interface IExecutive
    {
        Executable exe { get; set; }
    }

    public interface IExecutiveProvider
    {
        Executive CreateExecutive(Core core, bool isFep);
    }

    public class ExecutiveProvider : IExecutiveProvider
    {
        public Executive CreateExecutive(Core core, bool isFep)
        {
            return new Executive(core, isFep);
        }
    }

}
