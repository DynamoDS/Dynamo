using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProtoCore.Utils
{
    public static class EncodingUtils
    {
        // The encoding of original file is utf-8, so when we read in the file,
        // the character is encoded in utf-8. But csharp internally use 16bits 
        // to encode a character, that utf-8 bytes are interpreted as 16bits 
        // unicode. We need to decode it with unicode encoding to get the 
        // original utf-8 bytes.
        public static Byte[] UTF8StringToUTF8Bytes(string value)
        {
            Byte[] unicodebytes = Encoding.Unicode.GetBytes(value);
            Byte[] utf8bytes = new Byte[unicodebytes.Length];

            int utf8idx = 0;
            for (int idx = 0; idx < unicodebytes.Length; ++idx)
            {
                Byte b = unicodebytes[idx];
                if (b != 0)
                    utf8bytes[utf8idx++] = b;
            }
            Array.Resize(ref utf8bytes, utf8idx);
            return utf8bytes;
        }

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

