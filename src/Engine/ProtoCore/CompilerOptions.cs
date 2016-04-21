
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

        namespace Associative
        {
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
                GlobalInstanceFunctionBody,
                All
            }
        }

        namespace Imperative
        {
            public enum CompilePass
            {
                GlobalScope,
                Done
            }
        }
    }
}

