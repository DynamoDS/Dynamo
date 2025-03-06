<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByRadial --->
<!--- PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ --->
## In-Depth
`TSplineInitialSymmetry.ByRadial` definiert, ob die T-Spline-Geometrie radiale Symmetrie aufweist. Radiale Symmetrie kann nur für T-Spline-Grundkörper eingeführt werden, die dies zulassen - Kegel, Kugel, Drehung, Torus. Nachdem sie bei der Erstellung der T-Spline-Geometrie festgelegt wurde, beeinflusst radiale Symmetrie alle nachfolgenden Vorgänge und Änderungen.

Eine gewünschte Anzahl von `symmetricFaces` muss definiert werden, um die Symmetrie anzuwenden, wobei 1 der Mindestwert ist. Unabhängig davon, mit wie vielen Radius- und Höhenfeldern die T-Spline-Oberfläche beginnen muss, wird sie bis zum Erreichen der mit `symmetricFaces` ausgewählten Anzahl weiter unterteilt.

Im folgenden Beispiel wird `TSplineSurface.ByConePointsRadii` erstellt, und radiale Symmetrie wird mithilfe des Blocks `TSplineInitialSymmetry.ByRadial` angewendet. Anschließend werden die Blöcke `TSplineTopology.RegularFaces` und `TSplineSurface.ExtrudeFaces` verwendet, um eine Fläche der T-Spline-Oberfläche auszuwählen und zu extrudieren. Die Extrusion wird symmetrisch angewendet, und der Schieberegler für die Anzahl der symmetrischen Flächen zeigt, wie die radialen Felder unterteilt werden.

## Beispieldatei

![Example](./PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ_img.gif)
