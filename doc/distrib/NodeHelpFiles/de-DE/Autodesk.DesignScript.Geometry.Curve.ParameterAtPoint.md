## Im Detail
Parameter at Point gibt den Parameterwert eines angegebenen Punkts entlang einer Kurve zurück. Wenn sich der eingegebene Punkt nicht auf der Kurve befindet, gibt Parameter at Point den Parameter des Punkts auf der Kurve zurück, die dem eingegebenen Punkt am nächsten liegt. Im folgenden Beispiel wird zunächst eine NURBS-Kurve mit einem ByControlPoints-Block erstellt, wobei eine Reihe zufällig generierter Punkte als Eingabe verwendet wird. Ein zusätzlicher Einzelpunkt wird mit einem Codeblock erstellt, um die X- und Y-Koordinaten anzugeben. Der ParameterAtPoint-Block gibt den Parameter entlang der Kurve an dem Punkt zurück, der dem Eingabepunkt am nächsten liegt.
___
## Beispieldatei

![ParameterAtPoint](./Autodesk.DesignScript.Geometry.Curve.ParameterAtPoint_img.jpg)

