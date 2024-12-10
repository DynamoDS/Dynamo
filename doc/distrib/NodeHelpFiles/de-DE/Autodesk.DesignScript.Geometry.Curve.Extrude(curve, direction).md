## Im Detail
`Curve.Extrude (curve, direction)` extrudiert eine eingegebene Kurve mithilfe eines eingegebenen Vektors, um die Richtung der Extrusion zu bestimmen. Die Länge des Vektors wird für den Extrusionsabstand verwendet.

Im folgenden Beispiel wird zunächst mithilfe eines `NurbsCurve.ByControlPoints`-Blocks ein NurbsCurve-Objekt mit einem Satz zufällig generierter Punkte als Eingabe erstellt. Ein Codeblock wird verwendet, um die X-, Y- und Z-Komponenten eines `Vector.ByCoordinates`-Blocks anzugeben. Dieser Vektor wird dann als `direction`-Eingabe in einem `Curve.Extrude`-Block verwendet.
___
## Beispieldatei

![Curve.Extrude(curve, direction)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20direction)_img.jpg)
