## Im Detail
Patch versucht, eine Oberfläche mithilfe einer Eingabekurve als Begrenzung zu erstellen. Die Eingabekurve muss geschlossen sein. Im folgenden Beispiel verwenden Sie zunächst einen Point.ByCylindricalCoordinates-Block, um eine Reihe von Punkten in bestimmten Intervallen in einem Kreis zu erstellen, jedoch mit zufälligen Höhen und Radien. Anschließend verwenden Sie einen NurbsCurve.ByPoints-Block, um eine geschlossene Kurve basierend auf diesen Punkten zu erstellen. Ein Patch-Block wird verwendet, um eine Oberfläche von der geschlossenen Kurve der Begrenzung zu erstellen. Beachten Sie, dass nicht alle Anordnungen zu einer Kurve führen, die bearbeitet werden kann, da die Punkte mit zufälligen Radien und Höhen erstellt wurden.
___
## Beispieldatei

![Patch](./Autodesk.DesignScript.Geometry.Curve.Patch_img.jpg)

