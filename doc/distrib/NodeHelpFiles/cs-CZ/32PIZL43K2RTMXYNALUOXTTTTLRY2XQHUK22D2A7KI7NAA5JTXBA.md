<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction) --->
<!--- 32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA --->
## Podrobnosti
Uzel `Curve.ExtrudeAsSolid (curve, direction)` vysune vstupní uzavřenou rovinnou křivku pomocí vstupního vektoru určujícího směr vysunutí. Délka vektoru se použije jako vzdálenost vysunutí. Tento uzel uzavírá konce vysunutí za účelem vytvoření tělesa.

V následujícím příkladu nejprve vytvoříme objekt NurbsCurve pomocí uzlu `NurbsCurve.ByPoints`, přičemž jako vstup bude použita sada náhodně vygenerovaných bodů. Blok kódu slouží k určení komponent X, Y a Z uzlu `Vector.ByCoordinates`. Tento vektor je poté použit jako vstup směru v uzlu `Curve.ExtrudeAsSolid`.
___
## Vzorový soubor

![Curve.ExtrudeAsSolid(curve, direction)](./32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA_img.jpg)
