## Im Detail
`Mesh.PlaneCut` gibt ein Netz zurück, das von einer bestimmten Ebene geschnitten wurde. Das Ergebnis des Schnitts ist der Teil des Netzes, der auf der Seite der Ebene in Richtung der Normalen der `plane`-Eingabe liegt. Der Parameter `makeSolid` steuert, ob das Netz als `Solid` behandelt wird. In diesem Fall wird der Schnitt mit möglichst wenigen Dreiecken gefüllt, um jedes Loch abzudecken.

Im folgenden Beispiel wird ein ausgehöhltes Netz, das mit einer `Mesh.BooleanDifference`-Operation erzeugt wurde, von einer Ebene in einem Winkel geschnitten.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.PlaneCut_img.jpg)
