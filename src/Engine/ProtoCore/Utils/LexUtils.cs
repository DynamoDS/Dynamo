using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProtoCore.Utils
{
    public static class EncodingUtils
    {
        // ch is a unicode character
        public static Int64 ConvertCharacterToInt64(char ch)
        {
            char[] chs = new[] {ch};
            Byte[] utf8bytes = Encoding.UTF8.GetBytes(chs, 0, 1);
            Array.Resize(ref utf8bytes, 8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(utf8bytes);

            return BitConverter.ToInt64(utf8bytes, 0);
        }

        // data is utf-8 representation (8 bytes) of a unicode character
        public static Char ConvertInt64ToCharacter(Int64 data)
        {
            byte[] utf8bytes = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(utf8bytes);

            int endIndex = utf8bytes.Length - 1;
            while (utf8bytes[endIndex] == 0)
                endIndex--;
            Array.Resize(ref utf8bytes, endIndex + 1);

            return Encoding.UTF8.GetString(utf8bytes)[0];
        }

    }
}

