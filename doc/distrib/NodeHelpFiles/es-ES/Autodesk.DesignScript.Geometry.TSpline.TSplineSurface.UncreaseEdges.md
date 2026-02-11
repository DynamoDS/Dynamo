## In-Depth
Al contrario que el nodo `TSplineSurface.CreaseEdges`, este nodo elimina el pliegue de la arista especificada en una superficie de T-Spline.
En el ejemplo siguiente, se genera una superficie de T-Spline a partir de un toroide de T-Spline. Todas las aristas se seleccionan mediante los nodos `TSplineTopology.EdgeByIndex` y `TSplineTopology.EdgesCount` y el pliegue se aplica a todas las aristas con la ayuda del nodo `TSplineSurface.CreaseEdges`. Se selecciona un subconjunto de las aristas con índices de 0 a 7 y se aplica la operación inversa, esta vez, mediante el nodo `TSplineSurface.UncreaseEdges`. La posición de los bordes seleccionados se previsualiza con la ayuda de los nodos `TSplineEdge.UVNFrame` y `TSplineUVNFrame.Poision`.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseEdges_img.jpg)
