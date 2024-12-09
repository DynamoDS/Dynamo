## Im Detail
`Mesh.Reduce` erstellt ein neues Netz mit einer reduzierten Anzahl von Dreiecken. Die Eingabe für `triangleCount` definiert die Zielanzahl der Dreiecke des Ausgabenetzes. Beachten Sie, dass `Mesh.Reduce` die Form des Netzes bei extrem aggressiven Zielwerten für `triangleCount` erheblich ändern kann. Im folgenden Beispiel wird `Mesh.ImportFile` verwendet, um ein Netz zu importieren, das dann durch den Block `Mesh.Reduce` reduziert und für eine bessere Vorschau und einen besseren Vergleich an eine andere Position verschoben wird.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.Reduce_img.jpg)
