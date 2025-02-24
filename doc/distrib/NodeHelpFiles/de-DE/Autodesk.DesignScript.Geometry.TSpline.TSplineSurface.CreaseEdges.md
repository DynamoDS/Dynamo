## In-Depth
Mit `TSplineSurface.CreaseEdges` wird der angegebenen Kante auf einer T-Spline-Oberfläche ein scharfer Knick hinzugefügt.
Im folgenden Beispiel wird eine T-Spline-Oberfläche aus einem T-Spline-Torus erstellt. Eine Kante wird mithilfe des Blocks `TSplineTopology.EdgeByIndex` ausgewählt, und ein Knick wird mithilfe des Blocks `TSplineSurface.CreaseEdges` auf diese Kante angewendet. Die Scheitelpunkte auf beiden Kanten werden ebenfalls geknickt. Die Position der ausgewählten Kante wird mithilfe der Blöcke `TSplineEdge.UVNFrame` und `TSplineUVNFrame.Poision` als Vorschau angezeigt.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseEdges_img.jpg)
