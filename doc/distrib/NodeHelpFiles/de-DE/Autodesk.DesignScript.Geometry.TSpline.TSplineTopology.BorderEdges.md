## Im Detail
`TSplineTopology.BorderEdges` gibt eine Liste der Randkanten in der T-Spline-Oberfläche zurück.

Im folgenden Beispiel werden zwei T-Spline-Oberflächen mit `TSplineSurface.ByCylinderPointsRadius` erstellt. Eine ist eine offene Oberfläche, die andere wird mit `TSplineSurface.Thicken` verdickt, wodurch sie zu einer geschlossenen Oberfläche wird. Wenn beide mit dem Block `TSplineTopology.BorderEdges` untersucht werden, gibt die erste Oberfläche eine Liste der Randkanten zurück, während die zweite eine leere Liste zurückgibt. Das liegt daran, dass die Oberfläche umschlossen ist und keine Randkanten vorhanden sind.
___
## Beispieldatei

![TSplineTopology.BorderEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderEdges_img.jpg)
