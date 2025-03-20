## Im Detail
`TSplineInitialSymmetry.ByAxial` definiert, ob die T-Spline-Geometrie entlang einer ausgewählten Achse (x, y, z) symmetrisch ist. Symmetrie kann auf einer, zwei oder allen drei Achsen auftreten. Nachdem die Symmetrie bei der Erstellung der T-Spline-Geometrie bestimmt wurde, beeinflusst sie alle nachfolgenden Vorgänge und Änderungen.

Im folgenden Beispiel wird `TSplineSurface.ByBoxCorners` verwendet, um eine T-Spline-Oberfläche zu erstellen. Von den Eingaben dieses Blocks wird `TSplineInitialSymmetry.ByAxial` verwendet, um die anfängliche Symmetrie auf der Oberfläche zu definieren. `TSplineTopology.RegularFaces` und `TSplineSurface.ExtrudeFaces` werden dann verwendet, um eine Fläche der T-Spline-Oberfläche auszuwählen bzw. zu extrudieren. Der Extrusionsvorgang wird dann um die Symmetrieachsen gespiegelt, die mit dem Block `TSplineInitialSymmetry.ByAxial` definiert wurden.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByAxial_img.gif)
