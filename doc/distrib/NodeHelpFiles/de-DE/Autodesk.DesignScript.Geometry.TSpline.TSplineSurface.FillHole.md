## Im Detail
Im folgenden Beispiel werden die Lücken in einer zylindrischen T-Spline-Oberfläche mit dem Block `TSplineSurface.FillHole` gefüllt, der die folgenden Eingaben erfordert:
- `edges`: Anzahl von Randkanten, die von der zu füllenden T-Spline-Oberfläche ausgewählt wurden
- `fillMethod`: Numerischer Wert von 0-3, der die Füllmethode angibt:
    * 0 füllt die Öffnung mit Tessellation
    * 1 füllt die Öffnung mit einer einzelnen Vieleckfläche
    * 2 erstellt einen Punkt in der Mitte der Öffnung, von dem aus dreieckige Flächen strahlenförmig in Richtung der Kanten verlaufen
    * 3 ähnelt Methode 2, mit dem Unterschied, dass die Scheitelpunkte im Zentrum zu einem Scheitelpunkt verschweißt werden, anstatt nur aufeinander gestapelt zu werden.
- `keepSubdCreases`: Boolescher Wert, der angibt, ob unterteilte Knicke beibehalten werden.
___
## Beispieldatei

![TSplineSurface.FillHole](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FillHole_img.gif)
