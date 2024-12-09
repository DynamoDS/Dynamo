## Im Detail
`Mesh.Remesh` erstellt ein neues Netz, in dem die Dreiecke in einem bestimmten Objekt unabhängig von einer Änderung der Dreiecksnormalen gleichmäßiger verteilt werden. Diese Operation kann für Netze mit variabler Dreiecksdichte nützlich sein, um das Netz für die Festigkeitsanalyse vorzubereiten. Durch wiederholtes Neuvernetzen eines Netzes werden zunehmend gleichmäßigere Netze erzeugt. Für Netze, deren Scheitelpunkte bereits äquidistant sind (z. B. ein Ikosphärennetz), ist das Ergebnis des `Mesh.Remesh`-Blocks dasselbe Netz.
Im folgenden Beispiel wird `Mesh.Remesh` für ein importiertes Netz mit einer hohen Dreiecksdichte in Bereichen mit hohem Detaillierungsgrad verwendet. Das Ergebnis des `Mesh.Remesh`-Blocks wird zur Seite verschoben, und `Mesh.Edges` wird verwendet, um das Ergebnis zu visualisieren.

`(The example file used is licensed under creative commons)`

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.Remesh_img.jpg)
