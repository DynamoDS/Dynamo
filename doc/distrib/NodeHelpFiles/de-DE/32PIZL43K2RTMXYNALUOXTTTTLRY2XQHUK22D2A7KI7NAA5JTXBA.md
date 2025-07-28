<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction) --->
<!--- 32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA --->
## Im Detail
`Curve.ExtrudeAsSolid (curve, direction)` extrudiert eine eingegebene geschlossene, planare Kurve mithilfe eines eingegebenen Vektors, um die Richtung der Extrusion zu bestimmen. Die Länge des Vektors wird für den Extrusionsabstand verwendet. Dieser Block verschließt die Enden der Extrusion, um einen Volumenkörper zu erstellen.

Im folgenden Beispiel wird zunächst mithilfe eines `NurbsCurve.ByPoints`-Blocks ein NurbsCurve-Objekt mit einem Satz zufällig generierter Punkte als Eingabe erstellt. Mit einem Codeblock werden die X-, Y- und Z-Komponenten eines `Vector.ByCoordinates`-Blocks angegeben. Dieser Vektor wird dann als Richtungseingabe in einem `Curve.ExtrudeAsSolid`-Block verwendet.
___
## Beispieldatei

![Curve.ExtrudeAsSolid(curve, direction)](./32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA_img.jpg)
