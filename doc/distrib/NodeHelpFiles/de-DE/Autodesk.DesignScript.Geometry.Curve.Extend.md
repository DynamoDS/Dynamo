## Im Detail
Mit Extend wird eine Eingabekurve um einen bestimmten Eingabeabstand verlängert. Die pickSide-Eingabe verwendet den Start- oder Endpunkt der Kurve als Eingabe und bestimmt, welches Ende der Kurve verlängert werden soll. Im folgenden Beispiel wird zunächst eine NURBS-Kurve mit einem ByControlPoints-Block erstellt, wobei eine Reihe zufällig generierter Punkte als Eingabe verwendet wird. Der Abfrageblock Curve.EndPoint wird verwendet, um den Endpunkt der Kurve zu suchen, der als pickSide-Eingabe verwendet werden soll. Mit einem Zahlen-Schieberegler können Sie die Strecke der Verlängerung steuern.
___
## Beispieldatei

![Extend](./Autodesk.DesignScript.Geometry.Curve.Extend_img.jpg)

