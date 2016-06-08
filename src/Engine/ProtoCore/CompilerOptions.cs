
namespace ProtoCore
{
    namespace CompilerDefinitions
    {
        public enum AccessModifier
        {
            Public,
            Protected,
            Private
        }

        public enum CompilePass
        {
            ClassName,
            ClassBaseClass,
            ClassHierarchy,
            ClassMemVar,

            ClassMemFuncSig,
            GlobalFuncSig,

            ClassMemFuncBody,
            GlobalFuncBody,
            GlobalScope,
            Done
        }

        public enum SubCompilePass
        {
            None,
            UnboundIdentifier,
        }
    }
}

