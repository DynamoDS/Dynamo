## En detalle:
En el ejemplo siguiente, todos los vértices interiores de una superficie plana de T-Spline se recopilan mediante el nodo `TSplineTopology.InnerVertices`. Los vértices, junto con la superficie a la que pertenecen, se utilizan como entrada para el nodo `TSplineSurface.PullVertices`. La entrada `geometry` es una esfera situada sobre la superficie plana. La entrada `surfacePoints` se establece en el valor "False" (falso) y se utilizan puntos de control para realizar la operación de estiramiento.
___
## Archivo de ejemplo

![TSplineSurface.PullVertices](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.PullVertices_img.jpg)
