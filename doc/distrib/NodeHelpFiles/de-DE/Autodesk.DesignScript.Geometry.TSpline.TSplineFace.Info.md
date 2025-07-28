## In-Depth
`TSplineFace.Info` gibt die folgenden Eigenschaften einer T-Spline-Oberfläche zurück:
- `uvnFrame`: Punkt auf der Hülle, dem U-Vektor, dem V-Vektor und dem Normalenvektor der T-Spline-Oberfläche
- `index`: Index der Fläche
- `valence`: Anzahl der Scheitelpunkte oder Kanten, die eine Fläche bilden
- `sides`: Anzahl der Kanten jeder T-Spline-Oberfläche

Im folgenden Beispiel werden `TSplineSurface.ByBoxCorners` und `TSplineTopology.RegularFaces` verwendet, um einen T-Spline zu erstellen bzw. die Flächen auszuwählen. `List.GetItemAtIndex` wird verwendet, um eine bestimmte Fläche des T-Spline auszuwählen, und `TSplineFace.Info` wird verwendet, um die Eigenschaften zu ermitteln.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Info_img.jpg)
