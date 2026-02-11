## In-Depth
`TSplineReflection.ByAxial` gibt ein `TSplineReflection`-Objekt zurück, das als Eingabe für den Block `TSplineSurface.AddReflections` verwendet werden kann.
Die Eingabe des Blocks `TSplineReflection.ByAxial` ist eine Ebene, die als Spiegelebene dient. Ähnlich wie TSplineInitialSymmetry wirkt sich TSplineReflection, sobald für TSplineSurface festgelegt, auf alle nachfolgenden Vorgänge und Änderungen aus.

Im folgenden Beispiel wird `TSplineReflection.ByAxial` verwendet, um ein TSplineReflection-Objekt zu erstellen, das sich am oberen Ende des T-Spline-Kegels befindet. Die Reflexion wird dann als Eingabe für die `TSplineSurface.AddReflections`-Blöcke verwendet, um den Kegel wiederzugeben und eine neue T-Spline-Oberfläche zurückzugeben.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByAxial_img.jpg)
