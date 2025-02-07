<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderVertices --->
<!--- HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ --->
## Im Detail
`TSplineTopology.BorderVertices` gibt eine Liste der Randscheitelpunkte zurück, die in einer T-Spline-Oberfläche enthalten sind.

Im folgenden Beispiel werden zwei T-Spline-Oberflächen mit `TSplineSurface.ByCylinderPointsRadius` erstellt. Eine ist eine offene Oberfläche, die andere wird durch `TSplineSurface.Thicken` verdickt, wodurch sie zu einer geschlossenen Oberfläche wird. Wenn beide mit dem Block `TSplineTopology.BorderVertices` untersucht werden, gibt die erste Oberfläche eine Liste der Randscheitelpunkte zurück, während die zweite eine leere Liste zurückgibt. Das liegt daran, dass die Oberfläche umschlossen ist und keine Randscheitelpunkte vorhanden sind.
___
## Beispieldatei

![TSplineTopology.BorderVertices](./HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ_img.jpg)
