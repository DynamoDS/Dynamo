namespace ProtoCore.CodeModel
{
    public struct CodePoint
    {
        public CodeFile SourceLocation { get; set; }
        public int LineNo { get; set; }
        public int CharNo { get; set; }

        public override int GetHashCode()
        {
            int hash = ((null == SourceLocation) ? 0 : SourceLocation.GetHashCode());
            return hash ^ LineNo.GetHashCode() ^ CharNo.GetHashCode();
        }

        public static bool operator ==(CodePoint lhs, CodePoint rhs)
        {
            return lhs.LineNo == rhs.LineNo &&
                lhs.CharNo == rhs.CharNo &&
                lhs.SourceLocation == rhs.SourceLocation;
        }

        public static bool operator !=(CodePoint lhs, CodePoint rhs)
        {
            return !(lhs == rhs);
        }
    }

    public struct CodeRange
    {
        public CodePoint StartInclusive { get; set; }
        public CodePoint EndExclusive { get; set; }

        public override int GetHashCode()
        {
            return StartInclusive.GetHashCode() ^ EndExclusive.GetHashCode();
        }

        public static bool operator ==(CodeRange lhs, CodeRange rhs)
        {
            return lhs.StartInclusive == rhs.StartInclusive &&
                lhs.EndExclusive == lhs.EndExclusive;
        }

        public static bool operator !=(CodeRange lhs, CodeRange rhs)
        {
            return !(lhs == rhs);
        }
    }
}
