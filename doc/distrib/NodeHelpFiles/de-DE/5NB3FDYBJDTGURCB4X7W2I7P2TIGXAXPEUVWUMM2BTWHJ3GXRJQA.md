<!--- Autodesk.DesignScript.Geometry.Curve.Extrude(curve, direction, distance) --->
<!--- 5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA --->
## Im Detail
`Curve.Extrude (curve, direction, distance)` extrudiert eine eingegebene Kurve mithilfe eines eingegebenen Vektors, um die Richtung der Extrusion zu bestimmen. Für den Extrusionsabstand wird eine separate `distance`-Eingabe verwendet.

Im folgenden Beispiel wird zunächst mithilfe eines `NurbsCurve.ByControlPoints`-Blocks ein NurbsCurve-Objekt mit einem Satz zufällig generierter Punkte als Eingabe erstellt. Mit einem Codeblock werden die X-, Y- und Z-Komponenten eines `Vector.ByCoordinates`-Blocks angegeben. Dieser Vektor wird dann als Richtungseingabe in einem `Curve.Extrude`-Block und `number slider` wird zum Steuern der `distance`-Eingabe verwendet.
___
## Beispieldatei

![Curve.Extrude(curve, direction, distance)](./5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA_img.jpg)
