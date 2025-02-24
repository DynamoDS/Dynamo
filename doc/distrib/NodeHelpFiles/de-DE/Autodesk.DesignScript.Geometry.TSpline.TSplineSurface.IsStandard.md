## Im Detail
Eine T-Spline-Oberfläche entspricht dem Standard, wenn alle T-Punkte durch mindestens zwei Isokurven von Sternpunkten getrennt sind. Die Standardisierung ist erforderlich, um eine T-Spline-Oberfläche in eine NURBS-Oberfläche zu konvertieren.

Im folgenden Beispiel ist eine der Flächen einer mit `TSplineSurface.ByBoxLengths` generierten T-Spline-Oberfläche unterteilt. `TSplineSurface.IsStandard` wird verwendet, um zu prüfen, ob die Oberfläche dem Standard entspricht. Dies führt jedoch zu einem negativen Ergebnis.
Anschließend wird `TSplineSurface.Standardize` verwendet, um die Oberfläche zu standardisieren. Neue Steuerpunkte werden ohne Änderung der Oberflächenform eingeführt. Die resultierende Oberfläche wird mit `TSplineSurface.IsStandard` geprüft, wodurch bestätigt wird, dass sie jetzt dem Standard entspricht.
Die Blöcke `TSplineFace.UVNFrame` und `TSplineUVNFrame.Position` werden verwendet, um die unterteilte Fläche auf der Oberfläche hervorzuheben.
___
## Beispieldatei

![TSplineSurface.IsStandard](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsStandard_img.jpg)
