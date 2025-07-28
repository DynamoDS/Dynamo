## Im Detail
Im folgenden Beispiel wird eine T-Spline-Oberfläche durch Extrudieren einer NURBS-Kurve erstellt. Sechs Kanten werden mit dem Block `TSplineTopology.EdgeByIndex` ausgewählt - drei auf jeder Seite der Form. Die beiden Kantensätze werden zusammen mit der Oberfläche an den Block `TSplineSurface.MergeEdges` übergeben. Die Reihenfolge der Kantengruppen wirkt sich auf die Form aus - die erste Kantengruppe wird verschoben, damit sie auf die zweite Gruppe trifft, die an derselben Stelle bleibt. Die Eingabe `insertCreases` fügt die Option zum Knicken der Naht entlang der zusammengeführten Kanten hinzu. Das Ergebnis wird zum Zwecke einer besseren Vorschau zur Seite verschoben.
___
## Beispieldatei

![TSplineSurface.MergeEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MergeEdges_img.gif)
