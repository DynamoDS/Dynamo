<!--- Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(curve, param) --->
<!--- 5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ --->
## Im Detail
`Curve.NormalAtParameter (curve, param)` gibt einen Vektor zurück, der am angegebenen Kurvenparameter an der Normalenrichtung ausgerichtet ist. Die Parametrisierung einer Kurve wird im Bereich von 0 bis 1 gemessen, wobei 0 den Anfang der Kurve und 1 das Ende der Kurve darstellt.

Im folgenden Beispiel wird zunächst mithilfe eines `NurbsCurve.ByControlPoints`-Blocks ein NurbsCurve-Objekt mit einem Satz zufällig generierter Punkte als Eingabe erstellt. Ein Zahlen-Schieberegler im Bereich von 0 bis 1 wird verwendet, um die `parameter`-Eingabe für einen `Curve.NormalAtParameter`-Block zu steuern.
___
## Beispieldatei

![Curve.NormalAtParameter(curve, param](./5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ_img.jpg)
