
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

                GlobalScope,

                ClassMemFuncBody,
                GlobalFuncBody,
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
                GlobalFuncSig,
                GlobalScope,
                GlobalFuncBody,
                Done
            }
        }
    }
}

