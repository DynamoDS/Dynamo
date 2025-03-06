## In-Depth
`TSplineEdge.IsBorder` gibt `True` zurück, wenn die eingegebene T-Spline-Kante ein Rand ist.

Im folgenden Beispiel werden die Kanten von zwei T-Spline-Oberflächen untersucht. Die Oberflächen sind ein Zylinder und dessen verdickte Version. Um alle Kanten auszuwählen, werden in beiden Fällen `TSplineTopology.EdgeByIndex`-Blöcke verwendet und die Indizes eingegeben - eine Reihe von Ganzzahlen zwischen 0 und n, wobei n die Anzahl der Kanten ist, die durch `TSplineTopology.EdgesCount` bereitgestellt werden. Dies ist eine Alternative zur direkten Auswahl von Kanten mit `TSplineTopology.DecomposedEdges`. `TSplineSurface.CompressIndices` wird zusätzlich bei verdickten Zylindern verwendet, um die Kantenindizes neu anzuordnen.
Mit dem Block `TSplineEdge.IsBorder` wird geprüft, welche Kanten Randkanten sind. Die Position der Randkanten des flachen Zylinders wird mithilfe der Blöcke `TSplineEdge.UVNFrame` und `TSplineUVNFrame.Position` markiert. Der verdickte Zylinder hat keine Randkanten.

## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.IsBorder_img.jpg)
