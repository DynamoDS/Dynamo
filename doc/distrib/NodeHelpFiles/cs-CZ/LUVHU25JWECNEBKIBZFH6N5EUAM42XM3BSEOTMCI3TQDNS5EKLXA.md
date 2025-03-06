<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSolid(curve, path, cutEndOff) --->
<!--- LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA --->
## Podrobnosti
Uzel `Curve.SweepAsSolid`vytvoří těleso tažením vstupní uzavřené křivky profilu podél zadané trajektorie.

V následujícím příkladu použijeme obdélník jako základní křivku profilu. Trajektorie je vytvořena pomocí funkce kosinus s posloupností úhlů, které mění souřadnice X sady bodů. Body jsou použity jako vstup do uzlu `NurbsCurve.ByPoints`. Poté vytvoříme těleso tažením obdélníku podél vytvořené kosinové křivky pomocí uzlu `Curve.SweepAsSolid`.
___
## Vzorový soubor

![Curve.SweepAsSolid(curve, path, cutEndOff)](./LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA_img.jpg)
