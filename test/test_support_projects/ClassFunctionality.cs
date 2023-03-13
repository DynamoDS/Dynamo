using System;

namespace EmbeddedInterop
{
    /// <summary>
    /// Tests for basic functional testing of FFI implementations
    /// </summary>
    public class EmbeddedInteropTestClass
    {
        public static Microsoft.Office.Core.ContentVerificationResults GetOfficeInteropType()
        {
            return Microsoft.Office.Core.ContentVerificationResults.contverresValid;
        }

        public static bool TestOfficeInteropType(Microsoft.Office.Core.ContentVerificationResults arg1)
        {
            if (arg1 ==  Microsoft.Office.Core.ContentVerificationResults.contverresValid)
                return true;

            return false;
        }
    }
}
