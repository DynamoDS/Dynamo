## Im Detail
Mit Split By Points wird eine Eingabekurve an bestimmten Punkten geteilt, und es wird eine Liste der resultierenden Segmente zurückgegeben. Wenn sich die angegebenen Punkte nicht auf der Kurve befinden, sucht dieser Block die Punkte entlang der Kurve, die den Eingabepunkten am nächsten liegen, und teilt die Kurve an diesen resultierenden Punkten. Im folgenden Beispiel erstellen Sie zunächst eine NURBS-Kurve mit einem ByPoints-Block,wobei eine Reihe zufällig generierter Punkte als Eingabe verwendet wird. Derselbe Punktsatz wird als Liste der Punkte in einem SplitByPoints-Block verwendet. Das Ergebnis ist eine Liste der Kurvensegmente zwischen den generierten Punkten.
___
## Beispieldatei

![SplitByPoints](./Autodesk.DesignScript.Geometry.Curve.SplitByPoints_img.jpg)

