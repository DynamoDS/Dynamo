using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Geometry;

using DynamoConversions;

using NUnit.Framework;

namespace GeometryUITests
{
    //[TestFixture]
    //public class GeometryUITests
    //{
    //    [Test]
    //    [Category("UnitTests")]
    //    public void ExportAsSAT_ExportsWithCorrectUnits()
    //    {
    //        var temp_file = System.IO.Path.GetTempFileName();
    //        var c = Cuboid.ByLengths(3, 4, 5);

    //        var testUnit = ConversionUnit.Feet;
    //        var conversionFactor = Conversions.ConversionDictionary[testUnit]*1000;

    //        c.ExportToSAT(temp_file, conversionFactor);

    //        int i = 0;

    //        foreach (string line in File.ReadLines(temp_file))
    //        {
    //            ++i;

    //            if (i != 3)
    //                continue;

    //            var fields = line.Split(' ');

    //            var unitsString = fields[0];

    //            var unit = Convert.ToDouble(unitsString);

    //            Assert.IsTrue(Math.Abs(unit) - conversionFactor < 0.0001);

    //            break;
    //        }
    //    }
    //}
}
