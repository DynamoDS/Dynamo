## Im Detail
Plane At Segment Length gibt eine Ebene zurück, die an einem Punkt an einer Kurve ausgerichtet ist, der einen bestimmten Abstand entlang der Kurve aufweist, gemessen vom Startpunkt. Wenn die Eingabelänge größer als die Gesamtlänge der Kurve ist, verwendet dieser Block den Endpunkt der Kurve. Der Normalenvektor der resultierenden Ebene entspricht der Tangente der Kurve. Im folgenden Beispiel erstellen Sie zunächst eine NURBS-Kurve mit einem ByControlPoints-Block, wobei eine Reihe zufällig generierter Punkte als Eingabe verwendet wird. Ein Zahlen-Schieberegler wird verwendet, um die Parametereingabe für einen PlaneAtSegmentLength-Block zu steuern.
___
## Beispieldatei

![PlaneAtSegmentLength](./Autodesk.DesignScript.Geometry.Curve.PlaneAtSegmentLength_img.jpg)

