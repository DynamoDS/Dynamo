## Im Detail
Dieser Block zählt die Anzahl der Kanten in einem bereitgestellten Netz. Wenn das Netz aus Dreiecken besteht, was bei allen Netzen in `MeshToolkit` der Fall ist, gibt der Block `Mesh.EdgeCount` nur eindeutige Kanten zurück. Daher ist zu erwarten, dass die Anzahl der Kanten nicht dreimal so hoch ist wie die Anzahl der Dreiecke im Netz. Mit dieser Annahme kann sichergestellt werden, dass das Netz keine nicht verschweißten Flächen enthält (diese können in importierten Netzen auftreten).

Im folgenden Beispiel werden `Mesh.Cone` und `Number.Slider` verwendet, um einen Kegel zu erstellen, der dann als Eingabe zum Zählen der Kanten verwendet wird. Sowohl `Mesh.Edges` als auch `Mesh.Triangles` können verwendet werden, um eine Vorschau der Struktur und des Rasters eines Netzes in der Vorschau anzuzeigen, wobei `Mesh.Edges` eine bessere Leistung für komplexe und schwere Netze zeigt.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgeCount_img.jpg)
