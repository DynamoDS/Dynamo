## Podrobnosti
Uzel `Mesh.TrainglesAsNineNumbers` určuje souřadnice X, Y a Z vrcholů tvořících každý trojúhelník v zadané síti, což vytvoří devět čísel na jeden trojúhelník. Tento uzel může být užitečný k dotazování, rekonstrukci nebo převodu původní sítě.

V následujícím příkladu se k importu sítě použijí uzly `File Path` a `Mesh.ImportFile`. Poté se pomocí uzlu `Mesh.TrianglesAsNineNumbers` získají souřadnice vrcholů každého trojúhelníku. Tento seznam je poté rozdělen do trojic pomocí uzlu `List.Chop` se vstupem `lengths` nastaveným na hodnotu 3. Následně se pomocí uzlu `List.GetItemAtIndex` získá každá souřadnice X, Y a Z a znovu se sestaví vrcholy pomocí uzlu `Point.ByCoordinates`. Seznam bodů je dále rozdělen na trojice (3 body na každý trojúhelník) a použije se jako vstup pro uzel `Polygon.ByPoints`.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.TrianglesAsNineNumbers_img.jpg)
