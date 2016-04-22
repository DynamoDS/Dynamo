namespace Dynamo.Updates
{
    /// <summary>
    /// This class represents current version of the product.
    /// </summary>
    public class BinaryVersion
    {
        #region Public Class Operational Methods

        /// <summary>
        /// Parses a given string version representation 
        /// </summary>
        /// <param name="version">Version to parse</param>
        /// <returns>Result <see cref="BinaryVersion"/> object</returns>
        public static BinaryVersion FromString(string version)
        {
            if (string.IsNullOrEmpty(version))
                return null;

            string[] parts = version.Split('.');
            if (null == parts || (parts.Length < 3))
                return null;

            ushort major = 0, minor = 0, build = 0, priv = 0;
            if (!ushort.TryParse(parts[0], out major))
                return null;
            if (!ushort.TryParse(parts[1], out minor))
                return null;
            if (!ushort.TryParse(parts[2], out build))
                return null;
            if (parts.Length > 3)
            {
                if (!ushort.TryParse(parts[3], out priv))
                    return null;
            }

            return new BinaryVersion(major, minor, build, priv);
        }

        /// <summary>
        /// Returns string representation of the <see cref="BinaryVersion"/>
        /// </summary>
        /// <returns>String representation of the version</returns>
        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}",
                this.FileMajor.ToString(),
                this.FileMinor.ToString(),
                this.FileBuild.ToString(),
                this.FilePrivate.ToString());
        }

        /// <summary>
        /// Returns the hash code for this <see cref="BinaryVersion"/>
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            int high = (int)((this.Value & 0xffffffff00000000) >> 32);
            int low = (int)(this.Value & 0x00000000ffffffff);
            return high ^ low;
        }

        /// <summary>
        /// Determines whether this instance and another specified 
        /// <see cref="BinaryVersion"/> object are equal by reference
        /// </summary>
        /// <param name="other">The <see cref="BinaryVersion"/> to compare to this instance.</param>
        /// <returns>True if references are equal</returns>
        public override bool Equals(object other)
        {
            BinaryVersion rhs = other as BinaryVersion;
            return (this == rhs);
        }

        /// <summary>
        /// Compares two <see cref="BinaryVersion"/> objects. The result specifies 
        /// if the first given version is older than the second one 
        /// </summary>
        /// <param name="lhs">A <see cref="BinaryVersion"/> to compare.</param>
        /// <param name="rhs">A <see cref="BinaryVersion"/> to compare.</param>
        /// <returns>True if the first version is older than the second one</returns>
        public static bool operator <(BinaryVersion lhs, BinaryVersion rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
                return false;

            if (((object)lhs) == null || (((object)rhs) == null))
                return false; // Cannot compare with either one being null.

            return lhs.Value < rhs.Value;
        }

        /// <summary>
        /// Compares two <see cref="BinaryVersion"/> objects. The result specifies 
        /// if the first given version is older or the same as the second one 
        /// </summary>
        /// <param name="lhs">A <see cref="BinaryVersion"/> to compare.</param>
        /// <param name="rhs">A <see cref="BinaryVersion"/> to compare.</param>
        /// <returns>True if the first version is older or the same as the second one</returns>
        public static bool operator <=(BinaryVersion lhs, BinaryVersion rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
                return false;

            if (((object)lhs) == null || (((object)rhs) == null))
                return false; // Cannot compare with either one being null.

            return lhs.Value <= rhs.Value;
        }

        /// <summary>
        /// Compares two <see cref="BinaryVersion"/> objects. The result specifies 
        /// if the first given version is newer than the second one 
        /// </summary>
        /// <param name="lhs">A <see cref="BinaryVersion"/> to compare.</param>
        /// <param name="rhs">A <see cref="BinaryVersion"/> to compare.</param>
        /// <returns>True if the first version is newer than the second one</returns>
        public static bool operator >(BinaryVersion lhs, BinaryVersion rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
                return false;

            if (((object)lhs) == null || (((object)rhs) == null))
                return false; // Cannot compare with either one being null.

            return lhs.Value > rhs.Value;
        }

        /// <summary>
        /// Compares two <see cref="BinaryVersion"/> objects. The result specifies 
        /// if the first given version is newer or the same as the second one 
        /// </summary>
        /// <param name="lhs">A <see cref="BinaryVersion"/> to compare.</param>
        /// <param name="rhs">A <see cref="BinaryVersion"/> to compare.</param>
        /// <returns>True if the first version is newer or the same as the second one</returns>
        public static bool operator >=(BinaryVersion lhs, BinaryVersion rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
                return false;

            if (((object)lhs) == null || (((object)rhs) == null))
                return false; // Cannot compare with either one being null.

            return lhs.Value >= rhs.Value;
        }

        /// <summary>
        /// Compares two <see cref="BinaryVersion"/> objects. The result specifies 
        /// if the versions are the same
        /// </summary>
        /// <param name="lhs">A <see cref="BinaryVersion"/> to compare.</param>
        /// <param name="rhs">A <see cref="BinaryVersion"/> to compare.</param>
        /// <returns>True if the versions are the same</returns>
        public static bool operator ==(BinaryVersion lhs, BinaryVersion rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
                return true;

            if (((object)lhs) == null || (((object)rhs) == null))
                return false;

            return lhs.Value == rhs.Value;
        }

        /// <summary>
        /// Compares two <see cref="BinaryVersion"/> objects. The result specifies 
        /// if the versions are different
        /// </summary>
        /// <param name="lhs">A <see cref="BinaryVersion"/> to compare.</param>
        /// <param name="rhs">A <see cref="BinaryVersion"/> to compare.</param>
        /// <returns>True if the versions are different</returns>
        public static bool operator !=(BinaryVersion lhs, BinaryVersion rhs)
        {
            return !(lhs == rhs);
        }

        #endregion

        #region Public Class Properties

        internal ushort FileMajor { get; private set; }
        internal ushort FileMinor { get; private set; }
        internal ushort FileBuild { get; private set; }
        internal ushort FilePrivate { get; private set; }
        internal ulong Value { get; private set; }

        #endregion

        private BinaryVersion(ushort major, ushort minor, ushort build, ushort priv)
        {
            this.FileMajor = major;
            this.FileMinor = minor;
            this.FileBuild = build;
            this.FilePrivate = priv;

            ulong v1 = ((((ulong)major) << 48) & 0xffff000000000000);
            ulong v2 = ((((ulong)minor) << 32) & 0x0000ffff00000000);
            ulong v3 = ((((ulong)build) << 16) & 0x00000000ffff0000);
            ulong v4 = (((ulong)priv) & 0x000000000000ffff);
            this.Value = v1 | v2 | v3 | v4;
        }
    }
}
