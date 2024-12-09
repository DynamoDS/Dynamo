## In-Depth
`TSplineFace.Index` devuelve el índice de la cara en la superficie de T-Spline. Tenga en cuenta que en una topología de superficie de T-Spline, los índices de cara, arista y vértice no coinciden necesariamente con el número de secuencia del elemento en la lista. Utilice el nodo `TSplineSurface.CompressIndices` para solucionar este problema.

En el ejemplo siguiente, se utiliza `TSplineFace.Index` para mostrar los índices de todas las caras normales de una superficie de T-Spline.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Index_img.jpg)
