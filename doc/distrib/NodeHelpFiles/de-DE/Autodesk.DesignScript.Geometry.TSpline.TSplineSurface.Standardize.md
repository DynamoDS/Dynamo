## Im Detail
Der Block `TSplineSurface.Standardize` wird verwendet, um eine T-Spline-Oberfläche zu standardisieren.
Standardisieren bedeutet, eine T-Spline-Oberfläche für die NURBS-Konvertierung vorzubereiten. Dabei werden alle T-Punkte verlängert, bis sie durch mindestens zwei Isokurven von den Sternpunkten getrennt sind. Durch die Standardisierung wird die Form der Oberfläche nicht geändert, es können jedoch Steuerpunkte hinzugefügt werden, um die Geometrieanforderungen zu erfüllen, die für die NURBS-Kompatibilität der Oberfläche erforderlich sind.

Im folgenden Beispiel wird eine der Flächen einer mit `TSplineSurface.ByBoxLengths` generierten T-Spline-Oberfläche unterteilt.
Mit dem Block `TSplineSurface.IsStandard` wird geprüft, ob die Oberfläche dem Standard entspricht. Dies führt jedoch zu einem negativen Ergebnis.
Anschließend wird `TSplineSurface.Standardize` verwendet, um die Oberfläche zu standardisieren. Die resultierende Oberfläche wird mit `TSplineSurface.IsStandard` geprüft, was bestätigt, dass sie jetzt den Standard erfüllt.
The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the subdivided face in the surface.
___
## Beispieldatei

![TSplineSurface.Standardize](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Standardize_img.jpg)
