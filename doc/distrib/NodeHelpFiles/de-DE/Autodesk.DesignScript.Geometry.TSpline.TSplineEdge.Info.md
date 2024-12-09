## In-Depth
`TSplineEdge.Info` gibt die folgenden Eigenschaften einer T-Spline-Oberflächenkante zurück:
- `uvnFrame`: Punkt auf der Hülle, dem U-Vektor, dem V-Vektor und dem Normalenvektor der T-Spline-Kante
- `index`: Index der Kante
- `isBorder`: Angabe, ob die ausgewählte Kante ein Rand einer T-Spline-Oberfläche ist
- `isManifold`: Angabe, ob die ausgewählte Kante mannigfaltig ist

Im folgenden Beispiel wird `TSplineTopology.DecomposedEdges` verwendet, um eine Liste aller Kanten einer T-Spline-Zylinder-Grundkörperoberfläche abzurufen, und `TSplineEdge.Info` wird verwendet, um die Eigenschaften zu untersuchen.


## Beispieldatei

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Info_img.jpg)
