<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormal --->
<!--- DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ --->
## In-Depth
Mit `TSplineSurface.ByPlaneOriginNormal` wird eine T-Spline-Grundkörper-Ebenenoberfläche unter Verwendung eines Ursprungspunkts und eines Normalenvektors generiert. Zum Erstellen der T-Spline-Ebene verwendet der Block die folgenden Eingaben:
- `origin`: Punkt, der den Ursprung der Ebene definiert.
- `normal`: Vektor, der die Normalenrichtung der erstellten Ebene angibt.
- `minCorner` und `maxCorner`: Ecken der Ebene, dargestellt als Punkte mit X- und Y-Werten (Z-Koordinaten werden ignoriert). Diese Ecken stellen die Grenzen der ausgegebenen T-Spline-Oberfläche dar, wenn sie in die XY-Ebene übertragen wird. Die Punkte `minCorner` und `maxCorner` müssen nicht mit den Eckscheitelpunkten in 3D zusammenfallen. Beispiel: Wenn `minCorner` auf (0,0) und `maxCorner` auf (5,10) festgelegt ist, lauten die Ebenenbreite und -länge 5 bzw. 10.
- `xSpans` und `ySpans`: Anzahl der Breiten- und Längenfelder/-unterteilungen der Ebene
- `symmetry`: Angabe, ob die Geometrie in Bezug auf die X-, Y- und Z-Achse symmetrisch ist
- `inSmoothMode`: Angabe, ob die resultierende Geometrie im Modus Glatt oder Quader angezeigt wird

Im folgenden Beispiel wird eine planare T-Spline-Oberfläche erstellt, indem der angegebene Ursprungspunkt und die Normale verwendet werden, die ein Vektor der X-Achse ist. Die Größe der Oberfläche wird durch die beiden als `minCorner`- und `maxCorner`-Eingaben verwendeten Punkte gesteuert.

## Beispieldatei

![Example](./DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ_img.jpg)
