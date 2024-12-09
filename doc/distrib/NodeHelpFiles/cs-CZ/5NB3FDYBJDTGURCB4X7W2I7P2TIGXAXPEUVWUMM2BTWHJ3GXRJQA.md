<!--- Autodesk.DesignScript.Geometry.Curve.Extrude(curve, direction, distance) --->
<!--- 5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA --->
## Podrobnosti
Uzel `Curve.Extrude (curve, direction, distance)`vysune vstupní křivku pomocí vstupního vektoru, který určuje směr vysunutí. Pro vzdálenost vysunutí se použije samostatný vstup `distance`.

V následujícím příkladu nejprve vytvoříme objekt NurbsCurve pomocí uzlu `NurbsCurve.ByControlPoints`, přičemž jako vstup bude použita sada náhodně vygenerovaných bodů. Blok kódu slouží k určení komponent X, Y a Z uzlu `Vector.ByCoordinates`. Tento vektor se pak použije jako vstup směru v uzlu `Curve.Extrude`, zatímco posuvník se použije k určení vstupu `distance`.
___
## Vzorový soubor

![Curve.Extrude(curve, direction, distance)](./5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA_img.jpg)
