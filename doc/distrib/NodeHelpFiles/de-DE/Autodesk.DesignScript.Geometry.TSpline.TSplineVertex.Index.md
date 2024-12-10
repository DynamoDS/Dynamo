## In-Depth
`TSplineVertex.Index` gibt die Indexnummer des ausgewählten Scheitelpunkts auf der T-Spline-Oberfläche zurück. Beachten Sie, dass in einer T-Spline-Oberflächentopologie die Indizes für Fläche, Kante und Scheitelpunkt nicht unbedingt mit der Sequenznummer des Elements in der Liste übereinstimmen. Verwenden Sie zur Behebung dieses Problems den Block `TSplineSurface.CompressIndices`.

Im folgenden Beispiel wird `TSplineTopology.StarPointVertices` auf einen T-Spline-Grundkörper in Form eines Quaders angewendet. `TSplineVertex.Index` wird dann zur Abfrage der Indizes von Sternpunkt-Scheitelpunkten verwendet, und `TSplineTopology.VertexByIndex` gibt die ausgewählten Scheitelpunkte zur weiteren Bearbeitung zurück.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Index_img.jpg)
