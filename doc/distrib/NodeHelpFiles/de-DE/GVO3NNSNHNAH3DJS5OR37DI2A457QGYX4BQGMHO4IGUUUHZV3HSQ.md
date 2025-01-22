## In-Depth
Im folgenden Beispiel wird ein T-Spline-Kegelgrundkörper mit dem Block `TSplineSurface.ByConePointsRadius` erstellt. Position und Höhe des Kegels werden durch die beiden Eingaben `startPoint` und `endPoint` gesteuert. Nur der Basisradius kann mit der Eingabe `radius` angepasst werden, und der obere Radius ist immer null. `radialSpans` und `heightSpans` bestimmen die radialen und Höhenfelder. Die anfängliche Symmetrie der Form wird durch die Eingabe `symmetry` festgelegt. Wenn die X- oder Y-Symmetrie auf True gesetzt ist, muss der Wert der radialen Felder ein Vielfaches von 4 sein. Schließlich wird die Eingabe `inSmoothMode` verwendet, um zwischen der Vorschau im Modus Glatt und Quader der T-Spline-Oberfläche zu wechseln.

## Beispieldatei

![Example](./GVO3NNSNHNAH3DJS5OR37DI2A457QGYX4BQGMHO4IGUUUHZV3HSQ_img.jpg)
