namespace ProtoCore.Runtime
{
    public class Context
    {
        public bool IsReplicating { get; set; }
        public bool IsImplicitCall { get; set; }

        public Context()
        {
            IsReplicating = false;
            IsImplicitCall = false;
        }
    }
}