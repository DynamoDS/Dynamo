## Im Detail
`Cuboid.Height` gibt die Höhe des eingegebenen Quaders zurück. Beachten Sie, dass bei einer Transformation des Quaders in ein anderes Koordinatensystem mit einem Skalierfaktor die ursprünglichen Bemaßungen des Quaders zurückgegeben werden, nicht die Bemaßungen im realen Raum. Mit anderen Worten: Wenn Sie einen Quader mit einer Breite (X-Achse) von 10 erstellen und ihn in ein CoordinateSystem-Objekt mit einer 2-fachen Skalierung in X umwandeln, lautet die Breite immer noch 10.

Im folgenden Beispiel wird ein Quader anhand von Ecken generiert, und anschließend wird mithilfe eines `Cuboid.Height`-Blocks die Höhe ermittelt.

___
## Beispieldatei

![Height](./Autodesk.DesignScript.Geometry.Cuboid.Height_img.jpg)

