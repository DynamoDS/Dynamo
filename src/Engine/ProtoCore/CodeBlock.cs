using System;
using System.Collections.Generic;

namespace ProtoCore
{
    public enum Language
    {
        kInvalid = -1,
        kAssociative,
        kImperative,
        kOptions,
        kCount
    }

    public class LanguageCodeBlock
    {
        public const int OUTERMOST_BLOCK_ID = Int16.MinValue;

        public int id { get; set; }
        public Language language { get; set; }
        public string fingerprint { get; set; }
        public string version { get; set; }
        public List<ProtoCore.AST.Node> Attributes { get; set; }
        public string body { get; set; }

        public LanguageCodeBlock(Language lang = Language.kInvalid)
        {
            language = lang;
            Attributes = new List<AST.Node>();
            fingerprint = null;
            version = null;
        }


        public LanguageCodeBlock(LanguageCodeBlock rhs)
        {
            id = rhs.id;
            language = rhs.language;
            fingerprint = rhs.version;
            version = rhs.version;

            // TODO Jun: copy construct the attributes
            Attributes = new List<AST.Node>();

            body = rhs.body;
        }
    }

    public class Script
    {
        public List<LanguageCodeBlock> codeblockList { get; set; }
        public Script()
        {
            codeblockList = new List<LanguageCodeBlock>();
        }
    }
}

