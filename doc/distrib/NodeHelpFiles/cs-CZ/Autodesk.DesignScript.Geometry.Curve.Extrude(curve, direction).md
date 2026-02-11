## Podrobnosti
Uzel `Curve.Extrude (curve, direction)`vysune vstupní křivku pomocí vstupního vektoru, který určuje směr vysunutí. Délka vektoru se použije pro vzdálenost vysunutí.

V následujícím příkladu nejprve vytvoříme objekt NurbsCurve pomocí uzlu `NurbsCurve.ByControlPoints`, přičemž jako vstup bude použita sada náhodně vygenerovaných bodů. Blok kódu slouží k určení komponent X, Y a Z uzlu `Vector.ByCoordinates`. Tento vektor je poté použit jako vstup `direction` v uzlu `Curve.Extrude`.
___
## Vzorový soubor

![Curve.Extrude(curve, direction)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20direction)_img.jpg)
