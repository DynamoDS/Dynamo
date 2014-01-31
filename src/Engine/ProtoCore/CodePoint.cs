using System;

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

        public bool Before(CodePoint cp)
        {
            if (cp.SourceLocation != SourceLocation)
                return false;

            return (cp.LineNo < LineNo || (cp.LineNo == LineNo && (cp.CharNo < CharNo)));
        }
    }

    public struct CodeRange
    {
        public CodePoint StartInclusive { get; set; }
        public CodePoint EndExclusive { get; set; }

        public bool InsideRange(CodePoint cp)
        {
            // Reject the obvious cases first (codes in different files)...
            if (cp.SourceLocation != StartInclusive.SourceLocation)
                return false;
            if (cp.SourceLocation != EndExclusive.SourceLocation)
                return false;

            if (StartInclusive.LineNo <= cp.LineNo && EndExclusive.LineNo >= cp.LineNo)
            {
                if (StartInclusive.LineNo == cp.LineNo && StartInclusive.CharNo > cp.CharNo)
                    return false;
                else if (EndExclusive.LineNo == cp.LineNo && EndExclusive.CharNo < cp.CharNo)
                    return false;

                return true;
            }

            return false;
        }

        public bool InsideRange(CodeRange cr)
        {
            // Reject the obvious cases first (codes in different files)...
            if (cr.StartInclusive.SourceLocation != StartInclusive.SourceLocation)
                return false;
            if (cr.EndExclusive.SourceLocation != EndExclusive.SourceLocation)
                return false;

            if (StartInclusive.LineNo <= cr.StartInclusive.LineNo && EndExclusive.LineNo >= cr.EndExclusive.LineNo)
            {
                if (StartInclusive.LineNo == cr.StartInclusive.LineNo && StartInclusive.CharNo > cr.StartInclusive.CharNo)
                    return false;
                else if (EndExclusive.LineNo == cr.EndExclusive.LineNo && EndExclusive.CharNo < cr.EndExclusive.CharNo)
                    return false;

                return true;
            }

            return false;
        }

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
