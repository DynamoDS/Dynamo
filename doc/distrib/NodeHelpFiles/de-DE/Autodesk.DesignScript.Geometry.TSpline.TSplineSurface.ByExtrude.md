## In-Depth
Im folgenden Beispiel wird eine T-Spline-Oberfläche als Extrusion des Werts `curve` eines bestimmten Profils erstellt. Die Kurve kann offen oder geschlossen sein. Die Extrusion erfolgt mit einem vorgegebenen Wert für `direction` und kann in beide Richtungen erfolgen, gesteuert durch die Eingaben `frontDistance` und `backDistance`. Die Felder können einzeln für die beiden Extrusionsrichtungen mit den vorgegebenen Angaben für `frontSpans` und `backSpans` festgelegt werden. Um die Definition der Oberfläche entlang der Kurve zu erhalten, steuert `profileSpans` die Anzahl der Flächen, und `uniform` verteilt sie entweder gleichmäßig oder berücksichtigt die Krümmung. Schließlich steuert `inSmoothMode`, ob die Oberfläche im Modus Glatt oder Quader angezeigt wird.

## Beispieldatei
![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByExtrude_img.gif)
