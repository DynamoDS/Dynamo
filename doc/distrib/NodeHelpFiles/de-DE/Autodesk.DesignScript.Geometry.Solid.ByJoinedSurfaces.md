## Im Detail
Solid by Joined Surfaces verwendet eine Liste mit Oberflächen als Eingabe und gibt einen einzelnen Volumenkörper zurück, der durch die Oberflächen definiert wird. Die Oberflächen müssen eine geschlossene Oberfläche definieren. Im folgenden Beispiel beginnen Sie mit einem Kreis als Basisgeometrie. Der Kreis wird bearbeitet, um eine Oberfläche zu erstellen, und diese Oberfläche wird in die Z-Richtung verschoben. Anschließend extrudieren Sie den Kreis, um die Seiten zu erstellen. List.Create wird verwendet, um eine Liste der Basis-, Seiten- und oberen Flächen zu erstellen. Anschließend verwenden Sie ByJoinedSurfaces, um die Liste in einen einzelnen geschlossenen Volumenkörper umzuwandeln.
___
## Beispieldatei

![ByJoinedSurfaces](./Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces_img.jpg)

