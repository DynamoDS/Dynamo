<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneBestFitThroughPoints --->
<!--- QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ --->
## In-Depth
Mit `TSplineSurface.ByPlaneBestFitThroughPoints` wird eine T-Spline-Grundkörper-Ebenenoberfläche aus einer Liste von Punkten erstellt. Zum Erstellen der T-Spline-Ebene verwendet der Block die folgenden Eingaben:
- `points`: Satz von Punkten zum Definieren der Ausrichtung und des Ursprungs der Ebene. In Fällen, in denen die Eingabepunkte nicht auf einer einzelnen Ebene liegen, wird die Ausrichtung der Ebene basierend auf der besten Passung bestimmt. Zum Erstellen der Oberfläche sind mindestens drei Punkte erforderlich.
- `minCorner` und `maxCorner`: Die Ecken der Ebene, dargestellt als Punkte mit X- und Y-Werten (Z-Koordinaten werden ignoriert). Diese Ecken stellen die Grenzen der ausgegebenen T-Spline-Oberfläche dar, wenn sie auf die XY-Ebene übertragen wird. Die Punkte `minCorner` und `maxCorner` müssen nicht mit den Eckscheitelpunkten in 3D zusammenfallen.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

Im folgenden Beispiel wird eine planare T-Spline-Oberfläche mit einer zufällig generierten Liste von Punkten erstellt. Die Größe der Oberfläche wird durch die beiden als `minCorner`- und `maxCorner`-Eingaben verwendeten Punkte gesteuert.

## Beispieldatei

![Example](./QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ_img.jpg)
