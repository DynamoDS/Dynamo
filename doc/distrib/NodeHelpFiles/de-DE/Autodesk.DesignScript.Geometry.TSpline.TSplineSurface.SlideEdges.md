## Im Detail
Im folgenden Beispiel wird eine einfache T-Spline-Quaderoberfläche erstellt, und eine der Kanten wird mithilfe des Blocks `TSplineTopology.EdgeByIndex` ausgewählt. Um die Position des ausgewählten Scheitelpunkts besser zu verstehen, wird diese mithilfe der Blöcke `TSplineEdge.UVNFrame` und `TSplineUVNFrame.Position` visualisiert. Die gewählte Kante wird zusammen mit der zugehörigen Oberfläche als Eingabe für den Block `TSplineSurface.SlideEdges` übergeben. Die Eingabe `amount` bestimmt, wie stark die Kante in Richtung benachbarter Kanten verschoben wird (ausgedrückt in Prozent). Die Eingabe `roundness` steuert die Flachheit oder Rundheit der Abschrägung. Der Effekt der Rundheit ist im Modus Quader besser verständlich. Das Ergebnis des Verschiebevorgangs wird dann für die Vorschau zur Seite verschoben.

___
## Beispieldatei

![TSplineSurface.SlideEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SlideEdges_img.jpg)
