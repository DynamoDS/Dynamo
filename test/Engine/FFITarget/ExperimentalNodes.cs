
using System.Diagnostics.CodeAnalysis;


namespace FFITarget
{
    public static class ClassWithExperimentalMethod
    {
        [Experimental("FFI_1")]
        public static string ExperimentalMethod()
        {
            return "I am an experimental node!";
        }
    }
    [Experimental("FFI_2")]
    public static class ExperimentalClass
    {

        public static string Method()
        {
            return "my owning class is experimental";
        }
    }
}
