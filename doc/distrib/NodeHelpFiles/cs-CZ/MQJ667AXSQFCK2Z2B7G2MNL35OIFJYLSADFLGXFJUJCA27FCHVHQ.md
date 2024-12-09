<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToFaces --->
<!--- MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ --->
## Podrobnosti
Uzel `TSplineSurface.BridgeEdgesToFaces` spojuje dvě sady ploch, buď ze stejného povrchu nebo ze dvou různých povrchů. Uzel vyžaduje vstupy popsané níže. První tři vstupy jsou dostačující k vygenerování mostu, zbývající vstupy jsou volitelné. Výsledný povrch je podřazený povrchu, ke kterému patří první skupina hran.

V níže uvedeném příkladu se pomocí uzlu `TSplineSurface.ByTorusCenterRadii` vytvoří povrch anuloidu. Dvě z jeho ploch jsou vybrány a použity jako vstup pro uzel `TSplineSurface.BridgeFacesToFaces` společně s povrchem anuloidu. Zbývající vstupy demonstrují, jak je možné most dále upravit:
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`: (volitelné) odstraní mosty mezi okrajovými mosty, aby se zabránilo vyostření.
- `keepSubdCreases`: (volitelné) zachová vyostření dílčích částí vstupní topologie, což vede k vyostření počátku a konce mostu. Povrch anuloidu nemá žádné vyostřené hrany, takže tento vstup nemá žádný vliv na tvar.
- `firstAlignVertices`(volitelné) a `secondAlignVertices`: určením posunuté dvojice vrcholů získá most lehké otočení.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align

## Vzorový soubor

![Example](./MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ_img.gif)
