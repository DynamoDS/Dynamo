<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedEdges --->
<!--- 7LMFKLQNCV53W7KLS5QWD3E27NGGA33QPHSXMUGH323WVXWJY3GQ --->
## Im Detail
Im folgenden Beispiel wird eine planare T-Spline-Oberfläche mit extrudierten, unterteilten und gezogenen Scheitelpunkten und Flächen mit dem Block `TSplineTopology.DecomposedEdges` überprüft, wodurch eine Liste der folgenden in der T-Spline-Oberfläche enthaltenen Kantentypen zurückgegeben wird:

- `all`: Liste aller Kanten
- `nonManifold`: Liste der nicht mannigfaltigen Kanten
- `border`: Liste der Randkanten
- `inner`: Liste der Innenkanten


Der Block `Edge.CurveGeometry` wird verwendet, um die verschiedenen Kantentypen der Oberfläche hervorzuheben.
___
## Beispieldatei

![TSplineTopology.DecomposedEdges](./7LMFKLQNCV53W7KLS5QWD3E27NGGA33QPHSXMUGH323WVXWJY3GQ_img.gif)
