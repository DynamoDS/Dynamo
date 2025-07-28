## Im Detail
`Mesh.VertexIndicesByTri` gibt eine flache Liste von Scheitelpunktindizes zurück, die den einzelnen Netzdreiecken entsprechen. Die Indizes sind in Dreiergruppen angeordnet, und Indexgruppierungen können mithilfe des Blocks `List.Chop` mit der `lengths`-Eingabe von 3 leicht rekonstruiert werden.

Im folgenden Beispiel wird ein `MeshToolkit.Mesh` mit 20 Dreiecken in ein `Geometry.Mesh` konvertiert. `Mesh.VertexIndicesByTri` wird verwendet, um die Liste mit Indizes abzurufen, die dann mithilfe von `List.Chop` in Listen mit Dreieraufteilung unterteilt wird. Die Listenstruktur wird mithilfe von `List.Transpose` umgekehrt, um drei Listen der obersten Ebene mit 20 Indizes zu erhalten, die den Punkten A, B und C in jedem Netzdreieck entsprechen. Der Block `IndexGroup.ByIndices` wird verwendet, um Indexgruppen mit je drei Indizes zu erstellen. Die strukturierte Liste der `IndexGroups` und der Liste der Scheitelpunkte werden dann als Eingabe für `Mesh.ByPointsFaceIndices` verwendet, um ein konvertiertes Netz zu erhalten.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.VertexIndicesByTri_img.jpg)
