## Im Detail
Dieser Block gibt ein neues glattes Netz mit einem Kotangentenglättungsalgorithmus zurück, der die Scheitelpunkte nicht von ihrer ursprünglichen Position aus verteilt und besser geeignet ist, um Elemente und Kanten beizubehalten. Ein Skalierungswert muss in den Block eingegeben werden, um die räumliche Skalierung der Glättung festzulegen. Die Skalierungswerte können zwischen 0.1 und 64.0 liegen. Höhere Werte führen zu einem deutlicheren Glättungseffekt und damit zu einem scheinbar einfacheren Netz. Obwohl das neue Netz glatter und einfacher aussieht, hat es dieselbe Anzahl an Dreiecken, Kanten und Scheitelpunkten wie das ursprüngliche Netz.

Im folgenden Beispiel wird `Mesh.ImportFile` verwendet, um ein Objekt zu importieren. Anschließend wird `Mesh.Smooth` verwendet, um das Objekt mit einer Glättungsskala von 5 zu glätten. Das Objekt wird dann mit `Mesh.Translate` an eine andere Position verschoben, um eine bessere Vorschau zu erhalten, und `Mesh.TriangleCount` wird verwendet, um die Anzahl der Dreiecke im alten und neuen Netz zu verfolgen.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.Mesh.Smooth_img.jpg)
