using System;

namespace ProtoCore
{
    public enum Language
    {
        NotSpecified = -1,
        Associative,
        Imperative,
    }

    public class LanguageCodeBlock
    {
        public Language Language { get; set; }
        public string Code { get; set; }

        public LanguageCodeBlock(Language lang = Language.NotSpecified)
        {
            Language = lang;
        }

        public LanguageCodeBlock(LanguageCodeBlock rhs)
        {
            Language = rhs.Language;
            Code = rhs.Code;
        }

        /// <summary>
        /// Equality check for properties of a language block
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(LanguageCodeBlock other)
        {
            return Language == other.Language && String.Equals(Code, other.Code);
        }
    }
}

