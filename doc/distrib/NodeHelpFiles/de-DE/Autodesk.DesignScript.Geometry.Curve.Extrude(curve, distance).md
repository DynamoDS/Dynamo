## Im Detail
`Curve.Extrude (curve, distance)` extrudiert eine eingegebene Kurve. Dabei wird der Abstand der Extrusion durch eine eingegebene Nummer bestimmt. Die Richtung des Normalenvektors entlang der Kurve wird für die Extrusionsrichtung verwendet.

Im folgenden Beispiel wird zunächst mithilfe eines `NurbsCurve.ByControlPoints`-Blocks ein NurbsCurve-Objekt mit einem Satz zufällig generierter Punkte als Eingabe erstellt. Anschließend verwenden wir einen `Curve.Extrude`-Block zum Extrudieren der Kurve. Im `Curve.Extrude`-Block wird ein Zahlen-Schieberegler als `distance`-Eingabe verwendet.
___
## Beispieldatei

![Curve.Extrude(curve, distance)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20distance)_img.jpg)
