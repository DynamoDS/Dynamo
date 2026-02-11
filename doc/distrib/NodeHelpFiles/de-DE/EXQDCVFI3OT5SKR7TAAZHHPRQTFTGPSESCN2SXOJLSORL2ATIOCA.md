<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction, distance) --->
<!--- EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA --->
## Im Detail
Curve.ExtrudeAsSolid (direction, distance) extrudiert eine eingegebene geschlossene, planare Kurve mithilfe eines eingegebenen Vektors, um die Richtung der Extrusion zu bestimmen. Für den Extrusionsabstand wird eine separate `distance`-Eingabe verwendet. Dieser Block verschließt die Enden der Extrusion, um einen Volumenkörper zu erstellen.

Im folgenden Beispiel wird zunächst mithilfe eines `NurbsCurve.ByPoints`-Blocks ein NurbsCurve-Objekt mit einem Satz zufällig generierter Punkte als Eingabe erstellt. Mit `code block` werden die X-, Y- und Z-Komponenten eines `Vector.ByCoordinates`-Blocks angegeben. Dieser Vektor wird dann als Richtungseingabe in einem `Curve.ExtrudeAsSolid`-Block, und ein Zahlen-Schieberegler wird verwendet, um die `distance`-Eingabe zu steuern.
___
## Beispieldatei

![Curve.ExtrudeAsSolid(direction, distance)](./EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA_img.jpg)
