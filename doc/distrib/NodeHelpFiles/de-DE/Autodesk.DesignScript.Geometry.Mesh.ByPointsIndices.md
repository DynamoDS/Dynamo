## Im Detail
`Mesh.ByPointsIndices` erstellt anhand einer Liste mit `Points`, die die `vertices` der Netzdreiecke repräsentieren, und einer Liste mit `indices`, die darstellen, wie das Netz zusammengefügt wird, ein neues Netz. Die Eingabe `points` muss eine einfache Liste eindeutiger Scheitelpunkte im Netz sein. Die Eingabe `indices` muss eine einfache Liste mit Ganzzahlen sein. Jeder Satz mit drei Ganzzahlen bezeichnet ein Dreieck im Netz. Die Ganzzahlen geben den Index des Scheitelpunkts in der Scheitelpunktliste an. Die Eingabe indices muss 0-indiziert sein, wobei der erste Punkt der Scheitelpunktliste den Index 0 aufweisen muss.

Im folgenden Beispiel wird ein `Mesh.ByPointsIndices`-Block verwendet, um ein Netz anhand einer Liste mit neun `points` und einer Liste mit 36 `indices` zu erstellen, wobei die Scheitelpunktkombination für jedes der 12 Dreiecke des Netzes angegeben wird.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByPointsIndices_img.png)
