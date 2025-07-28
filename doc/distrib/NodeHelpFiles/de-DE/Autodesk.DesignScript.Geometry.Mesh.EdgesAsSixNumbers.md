## Im Detail
`Mesh.EdgesAsSixNumbers` bestimmt die X-, Y- und Z-Koordinaten der Scheitelpunkte, aus denen sich die einzelnen Kanten in einem angegebenen Netz zusammensetzen. Dies ergibt sechs Zahlen pro Kante. Dieser Block kann verwendet werden, um das Netz oder seine Kanten abzufragen oder zu rekonstruieren.

Im folgenden Beispiel wird mit `Mesh.Cuboid` ein Quadernetz erstellt, das dann als Eingabe für den Block `Mesh.EdgesAsSixNumbers` verwendet wird, um die Liste der Kanten abzurufen, die als sechs Zahlen ausgedrückt werden. Die Liste wird mithilfe von `List.Chop` in Listen mit je 6 Elementen unterteilt. Anschließend werden `List.GetItemAtIndex` und `Point.ByCoordinates` verwendet, um die Listen der Start- und Endpunkte jeder Kante zu rekonstruieren. Schließlich wird `List.ByStartPointEndPoint` verwendet, um die Kanten des Netzes neu zu konstruieren.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgesAsSixNumbers_img.jpg)
