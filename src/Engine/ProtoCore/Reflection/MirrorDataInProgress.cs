using ProtoCore.DSASM;
using ProtoCore.Mirror;

namespace ProtoCore
{
    /// <summary>
    /// WARNING: this class is used as temp value for CachedValue in NodeModel.
    /// Don't try to get some value, in any case it will be something wrong.
    /// </summary>
    public class MirrorDataInProgress : MirrorData
    {
        private static MirrorDataInProgress instance;

        public static MirrorDataInProgress Instance
        {
            get { return instance ?? (instance = new MirrorDataInProgress()); }

        }

        private MirrorDataInProgress()
            : base(null, StackValue.BuildNull())
        {
            // Nothing here.
        }

    }
}
