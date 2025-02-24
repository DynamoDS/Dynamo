## In-Depth
`TSplineFace.Info` devuelve las siguientes propiedades de una cara de T-Spline:
- `uvnFrame`: punto en la pared, vector U, vector V y vector normal de la cara de T-Spline.
- `index`: el índice de la cara.
- `valence`: el número de vértices o aristas que forman una cara.
- `sides`: el número de aristas de cada cara de T-Spline.

En el ejemplo siguiente, se utilizan `TSplineSurface.ByBoxCorners` y `TSplineTopology.RegularFaces` respectivamente para crear una T-Spline y seleccionar sus caras. `List.GetItemAtIndex` se utiliza para seleccionar una cara específica de la T-Spline y `TSplineFace.Info` se utiliza para encontrar sus propiedades.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Info_img.jpg)
