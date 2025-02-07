## Im Detail
Curve by IsoCurve on Surface erstellt eine Kurve, die die Isokurve auf einer Oberfläche ist, indem die U- oder V-Richtung und der Parameter in der entgegengesetzten Richtung angegeben werden, in der die Kurve erstellt werden soll. Die Eingabe direction bestimmt, welche Richtung der Isokurve erstellt werden soll. Der Wert 1 entspricht der U-Richtung, während der Wert 0 der V-Richtung entspricht. Im folgenden Beispiel erstellen Sie zuerst ein Raster aus Punkten und verschieben diese um einen bestimmten Betrag in Z-Richtung. Diese Punkte werden zur Erstellung einer Oberfläche mithilfe eines NurbsSurface.ByPoints-Blocks verwendet. Diese Oberfläche wird als baseSurface eines ByIsoCurveOnSurface-Blocks verwendet. Mit einem auf einen Bereich von 0 bis 1 eingestellten Zahlen-Schieberegler und einem Schritt von 1 wird gesteuert, ob die Isokurve in U- oder V-Richtung extrahiert wird. Mit einem zweiten Zahlen-Schieberegler wird der Parameter bestimmt, bei dem die Isokurve extrahiert wird.
___
## Beispieldatei

![ByIsoCurveOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByIsoCurveOnSurface_img.jpg)

