<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, distance) --->
<!--- NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA --->
## Podrobnosti
Uzel `Curve.ExtrudeAsSolid (curve, distance)` vysune vstupní uzavřenou rovinnou křivku pomocí vstupního čísla určujícího vzdálenost vysunutí. Směr vysunutí je určen normálovým vektorem roviny, ve které křivka leží. Tento uzel uzavírá konce vysunutí a vytvoří těleso.

V následujícím příkladu nejprve vytvoříme objekt NurbsCurve pomocí uzlu `NurbsCurve.ByPoints`, přičemž jako vstup bude použita sada náhodně vygenerovaných bodů. Poté bude uzel `Curve.ExtrudeAsSolid` použit k vysunutí křivky jako tělesa. Vstup `distance` v uzlu `Curve.ExtrudeAsSolid` bude zadán pomocí posuvníku.
___
## Vzorový soubor

![Curve.ExtrudeAsSolid(curve, distance)](./NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA_img.jpg)
