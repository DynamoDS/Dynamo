<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByTorusCoordinateSystemRadii --->
<!--- TTAJ2WGGNFLM755ADOCD3G7N4MJBQI66CAC7SXM3XCYLEIPLBOCQ --->
## In-Depth
Im folgenden Beispiel wird eine T-Spline-Torusoberfläche erstellt, deren Ursprung im angegebenen Koordinatensystem `cs` liegt. Neben- und Hauptradius der Form werden durch die Eingaben `innerRadius` und `outerRadius` festgelegt. Die Werte für `innerRadiusSpans` und `outerRadiusSpans` steuern die Definition der Oberfläche entlang der beiden Richtungen. Die anfängliche Symmetrie der Form wird durch die Eingabe `symmetry` festgelegt. Wenn axiale Symmetrie auf die Form angewendet wird und für die X- oder Y-Achse aktiv ist, muss der Wert `outerRadiusSpans` des Torus ein Vielfaches von 4 sein. Für radiale Symmetrie gelten keine solchen Anforderungen. Schließlich wird die Eingabe `inSmoothMode` verwendet, um zwischen der Vorschau im Modus Glatt und Quader der T-Spline-Oberfläche zu wechseln.

## Beispieldatei

![Example](./TTAJ2WGGNFLM755ADOCD3G7N4MJBQI66CAC7SXM3XCYLEIPLBOCQ_img.jpg)
