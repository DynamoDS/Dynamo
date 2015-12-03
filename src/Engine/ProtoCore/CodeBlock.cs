using System;
using System.Collections.Generic;

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
        public Language language { get; set; }
        public string fingerprint { get; set; }
        public string version { get; set; }
        public List<ProtoCore.AST.Node> Attributes { get; set; }
        public string body { get; set; }

        public LanguageCodeBlock(Language lang = Language.NotSpecified)
        {
            language = lang;
            Attributes = new List<AST.Node>();
            fingerprint = null;
        }


        public LanguageCodeBlock(LanguageCodeBlock rhs)
        {
            language = rhs.language;
            fingerprint = rhs.fingerprint;

            // TODO Jun: copy construct the attributes
            Attributes = new List<AST.Node>();

            body = rhs.body;
        }

        /// <summary>
        /// Equality check for properties of a language block
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(LanguageCodeBlock other)
        {
            // Attributes, fingerprint, body and version are not yet used/set for language blocks 
            // They only have default values.
            bool eqAttributes = true; 
            bool eqProperties = fingerprint == other.fingerprint;
            bool eqBody = string.Equals(body, other.body);

            return 
                language == other.language 
                && eqProperties 
                && eqAttributes 
                && eqBody;
        }
    }
}

