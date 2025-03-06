<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction, distance) --->
<!--- EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA --->
## Podrobnosti
Uzel Curve.ExtrudeAsSolid (direction, distance) vysune vstupní uzavřenou rovinnou křivku pomocí vstupního vektoru určujícího směr vysunutí. Pro vzdálenost vysunutí je použit samostatný vstup `distance`. Tento uzel uzavírá konce vysunutí za účelem vytvoření tělesa.

V následujícím příkladu nejprve vytvoříme objekt NurbsCurve pomocí uzlu `NurbsCurve.ByPoints`, přičemž jako vstup bude použita sada náhodně vygenerovaných bodů. K určení komponent X, Y a Z uzlu `Vector.ByCoordinates` se použije blok kódu. Tento vektor se pak použije jako vstup směru v uzlu `Curve.ExtrudeAsSolid`, zatímco posuvník se použije k určení vstupu `distance`.
___
## Vzorový soubor

![Curve.ExtrudeAsSolid(direction, distance)](./EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA_img.jpg)
