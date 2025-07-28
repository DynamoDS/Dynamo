## Im Detail
Points At Chord Length From Point gibt eine Liste von Punkten entlang einer Kurve zurück, die sequenziell anhand einer eingegebenen Sehnenlänge gemessen wird, die an einem bestimmten Punkt entlang der Kurve beginnt. Im folgenden Beispiel wird zunächst eine NURBS-Kurve mithilfe eines ByControlPoints-Blocks erstellt, wobei eine Reihe zufällig generierter Punkte als Eingabe verwendet wird. Ein PointAtParameter-Block wird mit einem auf den Bereich 0 bis 1 eingestellten Zahlen-Schieberegler verwendet, um den Startpunkt entlang der Kurve für einen PointsAtChordLengthFromPoint-Block zu bestimmen. Schließlich wird ein zweiter Zahlen-Schieberegler verwendet, um die zu verwendende geradlinige Sehnenlänge anzupassen.
___
## Beispieldatei

![PointsAtChordLengthFromPoint](./Autodesk.DesignScript.Geometry.Curve.PointsAtChordLengthFromPoint_img.jpg)

