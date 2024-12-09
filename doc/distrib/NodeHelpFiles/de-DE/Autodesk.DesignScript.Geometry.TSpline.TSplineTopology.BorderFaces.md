## Im Detail
`TSplineTopology.BorderFaces` gibt eine Liste der in der T-Spline-Oberfläche enthaltenen Randflächen zurück.

Im folgenden Beispiel werden zwei T-Spline-Oberflächen mit `TSplineSurface.ByCylinderPointsRadius` erstellt. Eine ist eine offene Oberfläche, die andere wird mit `TSplineSurface.Thicken` verdickt, wodurch sie zu einer geschlossenen Oberfläche wird. Wenn beide mit dem Block `TSplineTopology.BorderFaces` untersucht werden, gibt die erste Oberfläche eine Liste der Randkanten zurück, während die zweite eine leere Liste zurückgibt. Das liegt daran, dass die Oberfläche umschlossen ist und keine Randkanten vorhanden sind.
___
## Beispieldatei

![TSplineTopology.BorderFaces](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderFaces_img.jpg)
