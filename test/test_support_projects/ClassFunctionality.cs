using System;

namespace EmbeddedInterop
{
    /// <summary>
    /// Tests for basic functional testing of FFI implementations
    /// </summary>
    public class EmbeddedInteropTestClass
    {
        public static Microsoft.Office.Interop.Excel.XlPivotLineType GetExcelInteropType()
        {
            return Microsoft.Office.Interop.Excel.XlPivotLineType.xlPivotLineBlank;
        }

        public static bool TestExcelInteropType(Microsoft.Office.Interop.Excel.XlPivotLineType arg1)
        {
            if (arg1 == Microsoft.Office.Interop.Excel.XlPivotLineType.xlPivotLineBlank)
                return true;

            return false;
        }
    }
}
