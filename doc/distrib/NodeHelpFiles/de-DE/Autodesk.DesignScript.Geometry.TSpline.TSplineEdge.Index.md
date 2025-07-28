## In-Depth
Beachten Sie, dass in einer T-Spline-Oberflächentopologie die Indizes für `Fläche`, `Kante` und `Scheitelpunkt` nicht unbedingt mit der Sequenznummer des Elements in der Liste übereinstimmen. Verwenden Sie den Block `TSplineSurface.CompressIndices`, um dieses Problem zu beheben.

Im folgenden Beispiel werden mit `TSplineTopology.DecomposedEdges` die Randkanten einer T-Spline-Oberfläche abgerufen, und anschließend wird der Block `TSplineEdge.Index` verwendet, um die Indizes der angegebenen Kanten zu erhalten.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Index_img.jpg)
