using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.Namespace;

namespace ProtoTest
{
    [TestFixture]
    public class NameSpaceTests
    {
        [Test]
        public void SymbolName()
        {
            Symbol symbol = new Symbol("Com.Autodesk.Designscript.ProtoGeometry.Point");
            Assert.AreEqual("Point", symbol.Name);
            Assert.AreEqual("Com.Autodesk.Designscript.ProtoGeometry.Point", symbol.FullName);
        }

        [Test]
        public void NameMatching()
        {
            Symbol symbol = new Symbol("Com.Autodesk.Designscript.ProtoGeometry.Point");
            Assert.IsTrue(symbol.Matches("Com.Autodesk.Designscript.ProtoGeometry.Point"));
            Assert.IsTrue(symbol.Matches("ProtoGeometry.Point"));
            Assert.IsTrue(symbol.Matches("Designscript.Point"));
            Assert.IsTrue(symbol.Matches("Autodesk.Point"));
            Assert.IsTrue(symbol.Matches("Com.Point"));
            Assert.IsTrue(symbol.Matches("Com.Autodesk.Point"));
            Assert.IsTrue(symbol.Matches("Com.Designscript.Point"));
            Assert.IsTrue(symbol.Matches("Com.ProtoGeometry.Point"));
            Assert.IsTrue(symbol.Matches("Autodesk.ProtoGeometry.Point"));
            Assert.IsTrue(symbol.Matches("Autodesk.Designscript.Point"));
            Assert.IsTrue(symbol.Matches("Designscript.ProtoGeometry.Point"));
            Assert.IsTrue(symbol.Matches("Point"));
            Assert.IsFalse(symbol.Matches("Autodesk.Com.Designscript.Point"));
            Assert.IsFalse(symbol.Matches("Com.ProtoGeometry.Autodesk.Point"));
            Assert.IsFalse(symbol.Matches("Com.Designscript.Autodesk.Point"));
            Assert.IsFalse(symbol.Matches("Autodesk"));
        }

        [Test]
        public void AddSymbols()
        {
            SymbolTable table = new SymbolTable();
            Assert.IsNotNull(table.AddSymbol("Com.Autodesk.Designscript.ProtoGeometry.Point"));
            Assert.IsNotNull(table.AddSymbol("Autodesk.Designscript.ProtoGeometry.Point"));
            Assert.IsNotNull(table.AddSymbol("Designscript.ProtoGeometry.Point"));
            Assert.IsNotNull(table.AddSymbol("ProtoGeometry.Point"));
            Assert.IsNotNull(table.AddSymbol("Designscript.Point"));
            Assert.IsNotNull(table.AddSymbol("Com.Autodesk.Designscript.ProtoGeometry.Vector"));
            Assert.AreEqual(6, table.GetSymbolCount());
        }

        [Test]
        public void GetSymbol()
        {
            SymbolTable table = new SymbolTable();
            table.AddSymbol("Com.Autodesk.Designscript.ProtoGeometry.Point");
            var symbol = table.GetMatchingSymbols("Com.Autodesk.Point").First();
            Assert.AreEqual("Point", symbol.Name);
            Assert.AreEqual("Com.Autodesk.Designscript.ProtoGeometry.Point", symbol.FullName);
            Assert.AreEqual(symbol.FullName, table.GetFullyQualifiedName("Point"));
            Assert.AreEqual(symbol.FullName, table.GetFullyQualifiedName("Autodesk.ProtoGeometry.Point"));
            Assert.AreEqual(symbol.FullName, table.GetFullyQualifiedName("Designscript.ProtoGeometry.Point"));
            Assert.IsEmpty(table.GetMatchingSymbols("Com.Designscript.Autodesk.Point"));
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(()=>table.GetMatchingSymbols("Com.Autodesk.Designscript.ProtoGeometry"));
        }

        [Test]
        public void GetSymbolMultipleInput()
        {
            SymbolTable table = new SymbolTable();
            table.AddSymbol("Autodesk.ProtoGeometry.Point");
            table.AddSymbol("Autodesk.Designscript.Point");
            table.AddSymbol("Com.Autodesk.Point");
            Assert.AreEqual(3, table.GetSymbolCount());
            Assert.AreEqual("Com.Autodesk.Point", table.GetFullyQualifiedName("Com.Point"));
            Assert.AreEqual("Autodesk.ProtoGeometry.Point", table.GetFullyQualifiedName("ProtoGeometry.Point"));
            Assert.AreEqual("Autodesk.Designscript.Point", table.GetFullyQualifiedName("Designscript.Point"));

            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(()=>table.GetFullyQualifiedName("Point"));
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => table.GetFullyQualifiedName("Autodesk.Point"));
        }

        [Test]
        public void TryGetSymbol()
        {
            SymbolTable table = new SymbolTable();
            table.AddSymbol("Autodesk.ProtoGeometry.Point");
            table.AddSymbol("Autodesk.Designscript.Point");
            table.AddSymbol("Com.Autodesk.Point");
            Assert.AreEqual(3, table.GetSymbolCount());
            
            Symbol symbol = null;
            Assert.IsTrue(table.TryGetUniqueSymbol("Com.Point", out symbol));
            Assert.IsNotNull(symbol);
            Assert.AreEqual("Com.Autodesk.Point", symbol.FullName);
            symbol.Id = 123;

            Assert.IsTrue(table.TryGetUniqueSymbol("ProtoGeometry.Point", out symbol));
            Assert.IsNotNull(symbol);
            Assert.AreEqual("Autodesk.ProtoGeometry.Point", symbol.FullName);

            Assert.IsTrue(table.TryGetUniqueSymbol("Designscript.Point", out symbol));
            Assert.IsNotNull(symbol);
            Assert.AreEqual("Autodesk.Designscript.Point", symbol.FullName);

            Assert.IsFalse(table.TryGetUniqueSymbol("Point", out symbol));
            Assert.IsNull(symbol);

            Assert.IsFalse(table.TryGetUniqueSymbol("Autodesk.Point", out symbol));
            Assert.IsNull(symbol);

            Assert.IsFalse(table.TryGetUniqueSymbol("Autodesk.Designscript", out symbol));
            Assert.IsNull(symbol);

            Assert.IsTrue(table.TryGetExactSymbol("Com.Autodesk.Point", out symbol));
            Assert.IsNotNull(symbol);
            Assert.AreEqual("Com.Autodesk.Point", symbol.FullName);
            Assert.AreEqual(123, symbol.Id);
        }

        [Test]
        public void ResolveSymbol()
        {
            SymbolTable table = new SymbolTable();
            table.AddSymbol("Com.Autodesk.Point");
            table.AddSymbol("Com.Autodesk.Geometry.Point");

            Symbol symbol = null;
            Assert.IsTrue(table.TryGetUniqueSymbol("Com.Autodesk.Point", out symbol));
            Assert.IsTrue(table.TryGetUniqueSymbol("Com.Autodesk.Geometry.Point", out symbol));
            Assert.IsFalse(table.TryGetUniqueSymbol("Com.Point", out symbol));
            Assert.IsFalse(table.TryGetUniqueSymbol("Point", out symbol));
        }
    }
}
