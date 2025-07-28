## In-Depth
Dieser Block gibt ein TSplineUVNFrame-Objekt zurück, das für die Visualisierung der Scheitelpunktposition und -ausrichtung sowie für die Verwendung der U-, V- oder N-Vektoren für die weitere Bearbeitung der T-Spline-Oberfläche hilfreich sein kann.

Im folgenden Beispiel wird der UVN-Frame des ausgewählten Scheitelpunkts mithilfe des Blocks `TSplineVertex.UVNFrame` abgerufen. Der UVN-Frame wird dann verwendet, um die Normale des Scheitelpunkts zurückzugeben. Die Normalenrichtung wird schließlich verwendet, um den Scheitelpunkt mithilfe des Blocks `TSplineSurface.MoveVertices` zu verschieben.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.UVNFrame_img.jpg)
