<!--- Autodesk.DesignScript.Geometry.Curve.TrimSegmentsByParameter(curve, parameters, discardEvenSegments) --->
<!--- BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ --->
## Im Detail
`Curve.TrimSegmentsByParameter (parameters, discardEvenSegments)` teilt zunächst eine Kurve an Punkten, die durch eine eingegebenen Parameterliste bestimmt werden. Anschließend werden entweder die ungerade oder die gerade nummerierten Segmente zurückgegeben, wie durch den booleschen Wert der `discardEvenSegments`-Eingabe bestimmt.

Im folgenden Beispiel wird zunächst mithilfe eines `NurbsCurve.ByControlPoints`-Blocks ein NurbsCurve-Objekt mit einem Satz zufällig generierter Punkte als Eingabe erstellt. `code block` wird verwendet, um einen Zahlenbereich zwischen 0 und 1 zu erstellen, wobei Schritte von 0.1 erfolgen. Wenn Sie dies als Eingabeparameter für einen `Curve.TrimSegmentsByParameter`-Block verwenden, erhalten Sie eine Liste mit Kurven, die im Prinzip eine Version aus gestrichelten Linien der ursprünglichen Kurve darstellen.
___
## Beispieldatei

![Curve.TrimSegmentsByParameter(parameters, discardEvenSegments)](./BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ_img.jpg)
