## In-Depth
`TSplineReflection.ByRadial` gibt ein `TSplineReflection`-Objekt zurück, das als Eingabe für den Block `TSplineSurface.AddReflections` verwendet werden kann. Der Block benötigt eine Ebene als Eingabe, und die Normale der Ebene dient als Achse zum Drehen der Geometrie. Ähnlich wie TSplineInitialSymmetry wirkt sich TSplineReflection, sobald bei der Erstellung von TSplineSurface festgelegt, auf alle nachfolgenden Vorgänge und Änderungen aus.

Im folgenden Beispiel wird die Reflexion einer T-Spline-Oberfläche mit `TSplineReflection.ByRadial` definiert. Die Eingaben `segmentsCount` und `segmentAngle` steuern die Art und Weise, wie Geometrie um die Normale einer bestimmten Ebene reflektiert wird. Die Ausgabe des Blocks wird dann als Eingabe für den Block `TSplineSurface.AddReflections` verwendet, um eine neue T-Spline-Oberfläche zu erstellen.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByRadial_img.gif)
