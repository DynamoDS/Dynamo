namespace ProtoCore
{
    public static class CompilerOptions
    {
        public enum OptImperative
        {
            kAutoAllocate   = 1 << 0,
            kInferTypes     = 1 << 1
        }

        public static int optSet { get; set; }
    }
}

