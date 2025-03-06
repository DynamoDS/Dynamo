<!--- Autodesk.DesignScript.Geometry.Curve.CoordinateSystemAtSegmentLength --->
<!--- ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q --->
## Im Detail
Coordinate System At Segment Length gibt ein Koordinatensystem zurück, das an der Eingabekurve an der angegebenen Kurvenlänge ausgerichtet ist, gemessen vom Startpunkt der Kurve. Die X-Achse des resultierenden Koordinatensystems verläuft in Richtung der Kurvennormalen, und die Y-Achse in Richtung der Kurventangente an der angegebenen Länge. Im folgenden Beispiel wird zunächst eine NURBS-Kurve mit einem ByControlPoints-Block erstellt, wobei eine Reihe zufällig generierter Punkte als Eingabe verwendet wird. Ein Zahlen-Schieberegler wird verwendet, um die Eingabe der Segmentlänge für einen CoordinateSystemAtParameter-Block zu steuern. Wenn die angegebene Länge länger ist als die Länge der Kurve, gibt dieser Block ein Koordinatensystem am Endpunkt der Kurve zurück.
___
## Beispieldatei

![CoordinateSystemAtSegmentLength](./ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q_img.jpg)

