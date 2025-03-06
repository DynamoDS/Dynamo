## Im Detail
Im folgenden Beispiel wird ein T-Spline-Quader mithilfe des Blocks `TSplineSurface.ByBoxLengths` erstellt, bei dem Ursprung, Breite, Länge, Höhe, Felder und Symmetrie angegeben sind.
Mit `EdgeByIndex` wird dann eine Kante aus der Kantenliste auf der generierten Oberfläche ausgewählt. Die ausgewählte Kante wird dann mithilfe von `TSplineSurface.SlideEdges` entlang der benachbarten Kanten verschoben, gefolgt von den symmetrischen Gegenstücken.
___
## Beispieldatei

![TSplineTopology.EdgeByIndex](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.EdgeByIndex_img.jpg)
