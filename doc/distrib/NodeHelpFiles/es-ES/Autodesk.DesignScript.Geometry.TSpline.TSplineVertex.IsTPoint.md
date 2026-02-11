## In-Depth
`TSplineVertex.IsTPoint` devuelve si un vértice es un punto T. Los puntos T son vértices al final de filas parciales de puntos de control.

En el ejemplo siguiente, se utiliza `TSplineSurface.SubdivideFaces` en una primitiva de cuadro de T-Spline para ejemplificar una de las múltiples formas de añadir puntos T a una superficie. El nodo `TSplineVertex.IsTPoint` se utiliza para confirmar que un vértice en un índice es un punto T. Para visualizar mejor la posición de los puntos T, se emplean los nodos `TSplineVertex.UVNFrame` y `TSplineUVNFrame.Position`.



## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsTPoint_img.jpg)
