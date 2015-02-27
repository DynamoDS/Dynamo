using System;

namespace ProtoCore.CodeModel
{
    public class CodeFile
    {
        //For now, we'll just have a filename
        public String FilePath { get; set; }

        public static bool operator ==(CodeFile lhs, CodeFile rhs)
        {
            if (object.ReferenceEquals(lhs, rhs))
                return true;

            if ((object)lhs == null || (object)rhs == null)
                return false;

            return lhs.FilePath == rhs.FilePath;
        }
        public static bool operator !=(CodeFile lhs, CodeFile rhs)
        {
            return !(lhs == rhs);
        }

    }
}
