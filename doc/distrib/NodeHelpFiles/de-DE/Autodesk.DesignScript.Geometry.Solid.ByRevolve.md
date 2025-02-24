## Im Detail
`Solid.ByRevolve` erstellt eine Oberfläche, indem eine bestimmte Profilkurve um eine Achse gedreht wird. Die Achse wird durch einen `axisOrigin`-Punkt und einen `axisDirection`-Vektor definiert. Der Startwinkel bestimmt (gemessen in Grad) den Startpunkt der Oberfläche, und mit `sweepAngle` wird festgelegt, wie weit um die Achse die Oberfläche fortgesetzt wird.

Im folgenden Beispiel wird eine Kurve, die mit einer Kosinusfunktion generiert wurde, als Profilkurve und zwei Zahlen-Schieberegler zum Steuern von `startAngle` und `sweepAngle` verwendet. In diesem Beispiel bleiben für `axisOrigin` und `axisDirection` die Vorgabewerte des Weltursprungs und der Welt-Z-Achse erhalten.

___
## Beispieldatei

![ByRevolve](./Autodesk.DesignScript.Geometry.Solid.ByRevolve_img.jpg)

