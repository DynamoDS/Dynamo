## In-Depth
`TSplineVertex.Info` devuelve las siguientes propiedades de un vértice de T-Spline:
- `uvnFrame`: punto en la pared, vector U, vector V y vector normal del vértice de T-Spline.
- `index`: el índice del vértice seleccionado en la superficie de T-Spline.
- `isStarPoint`: determina si el vértice seleccionado es un punto de estrella.
- `isTpoint`: determina si el vértice seleccionado es un punto T.
- `isManifold`: determina si el vértice seleccionado es múltiple.
- `valence`: el número de aristas en el vértice de T-Spline seleccionado.
- `functionalValence`: la valencia funcional de un vértice. Consulte la documentación del nodo `TSplineVertex.FunctionalValence` para obtener más información.

En el ejemplo siguiente, se utilizan `TSplineSurface.ByBoxCorners` y `TSplineTopology.VertexByIndex` respectivamente para crear una superficie de T-Spline y seleccionar sus vértices. `TSplineVertex.Info` se utiliza para recopilar la información anterior sobre un vértice elegido.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Info_img.jpg)
