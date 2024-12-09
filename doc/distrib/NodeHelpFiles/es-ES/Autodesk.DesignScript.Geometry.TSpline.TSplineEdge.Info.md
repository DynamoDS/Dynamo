## In-Depth
`TSplineEdge.Info` devuelve las siguientes propiedades de una arista de superficie de T-Spline:
- `uvnFrame`: punto en la pared, vector U, vector V y vector normal de la arista de T-Spline.
- `index`: el índice del borde.
- `isBorder`: determina si la arista seleccionada es un borde de superficie de T-Spline.
- `isManifold`: determina si la arista seleccionada es múltiple.

En el ejemplo siguiente, se utiliza `TSplineTopology.DecomposedEdges` para obtener una lista de todas las aristas de una superficie de primitiva de cilindro de T-Spline y `TSplineEdge.Info` para investigar sus propiedades.


## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Info_img.jpg)
