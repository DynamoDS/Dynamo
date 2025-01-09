<!--- Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPointsWeightsKnots --->
<!--- T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q --->
## Podrobnosti
Pomocí uzlu `NurbsCurve.ByControlPointsWeightsKnots` je možné ručně řídit tloušťky a uzly objektu NurbsCurve. Seznam tlouštěk by měl mít stejnou délku jako seznam řídicích bodů. Velikost seznamu uzlů musí být rovna počtu řídicích bodů plus stupeň plus 1.

V následujícím příkladu nejprve vytvoříme objekt NurbsCurve interpolací mezi řadou náhodných bodů. K nalezení odpovídajících částí křivky použijeme uzly, tloušťky a řídicí body. K úpravě seznamu tlouštěk můžeme použít uzel `List.ReplaceItemAtIndex`. Nakonec pomocí uzlu `NurbsCurve.ByControlPointsWeightsKnots` znovu vytvoříme objekt NurbsCurve s upravenými tloušťkami.

___
## Vzorový soubor

![ByControlPointsWeightsKnots](./T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q_img.jpg)

