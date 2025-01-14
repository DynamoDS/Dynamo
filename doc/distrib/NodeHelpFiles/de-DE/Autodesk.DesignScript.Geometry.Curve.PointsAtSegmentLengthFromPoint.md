## Im Detail
Points At Segment Length From Point gibt eine Liste von Punkten entlang einer Kurve zurück, die fortlaufend anhand einer eingegebenen Segmentlänge gemessen wird, die an einem bestimmten Punkt entlang der Kurve beginnt. Im folgenden Beispiel wird zunächst eine NURBS-Kurve mit einem ByControlPoints-Block erstellt, wobei eine Reihe zufällig generierter Punkte als Eingabe verwendet wird. Ein PointAtParameter-Block wird mit einem auf den Bereich 0 bis 1 eingestellten Zahlen-Schieberegler verwendet, um den Startpunkt entlang der Kurve für einen PointsAtSegmentLengthFromPoint-Block zu bestimmen. Schließlich wird ein zweiter Zahlen-Schieberegler verwendet, um die zu verwendende Kurvensegmentlänge anzupassen
___
## Beispieldatei

![PointsAtSegmentLengthFromPoint](./Autodesk.DesignScript.Geometry.Curve.PointsAtSegmentLengthFromPoint_img.jpg)

