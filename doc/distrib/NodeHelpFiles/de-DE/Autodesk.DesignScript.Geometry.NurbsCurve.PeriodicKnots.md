## Im Detail
Verwenden Sie `NurbsCurve.PeriodicKnots`, wenn Sie eine geschlossene NURBS-Kurve in ein anderes System (z. B. Alias) exportieren müssen oder wenn dieses System die Kurve in ihrer periodischen Form erwartet. Viele CAD-Werkzeuge erwarten diese Form für die Roundtrip-Genauigkeit.

`PeriodicKnots` gibt den Knotenvektor in der *periodischen* (ungeklammerten) Form zurück. `Knots` gibt ihn in der *geklammerten* Form zurück. Beide Arrays haben die gleiche Länge; es sind zwei verschiedene Möglichkeiten, dieselbe Kurve zu beschreiben. In der geklammerten Form wiederholen sich die Knoten am Anfang und Ende, sodass die Kurve am Parameterbereich fixiert wird. In der periodischen Form wiederholt sich der Knotenabstand stattdessen am Anfang und Ende, wodurch eine glatte, geschlossene Schleife entsteht.

Im folgenden Beispiel wird eine periodische NURBS-Kurve mit `NurbsCurve.ByControlPointsWeightsKnots` erstellt. Watch-Blöcke vergleichen `Knots` und `PeriodicKnots`, sodass Sie die gleiche Länge, jedoch unterschiedliche Werte sehen. Knots ist die geklammerte Form (wiederholte Knoten an den Enden), und PeriodicKnots ist die ungeklammerte Form mit dem sich wiederholenden Differenzmuster, das die Periodizität der Kurve definiert.
___
## Beispieldatei

![PeriodicKnots](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicKnots_img.jpg)
