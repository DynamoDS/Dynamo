namespace ProtoCore
{
    public class LangVerify
    {
        private Language language;
        private string fingerprint;
        private string version;

        public bool Verify(ProtoCore.LanguageCodeBlock codeblock)
        {
            language = codeblock.language;
            fingerprint = codeblock.fingerprint;
            version = codeblock.version;

            bool result = VerifyFingerprint() && VerifyVersion();
            return result;
        }

        public bool VerifyFingerprint()
        {
            /*
            if (this.fingerprint != null)
                //do check
            else
                return true;
            */
            return true;
        }

        public bool VerifyVersion()
        {
            /*
            if (this.version != null)
                //do check
            else
                return true;
            */
            return true;
        }
    }
}
