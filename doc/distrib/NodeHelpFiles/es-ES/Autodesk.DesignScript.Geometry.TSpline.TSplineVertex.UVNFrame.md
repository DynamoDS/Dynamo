## In-Depth
Este nodo devuelve un objeto TSplineUVNFrame que puede ser útil para visualizar la posición y la orientación del vértice, así como para utilizar los vectores U, V o N a fin de manipular aún más la superficie de T-Spline.

En el ejemplo siguiente, se utiliza el nodo `TSplineVertex.UVNFrame` para obtener el marco UVN del vértice seleccionado. A continuación, el marco UVN se utiliza para devolver la normal del vértice. Por último, la dirección normal se utiliza para desplazar el vértice mediante el nodo `TSplineSurface.MoveVertices`.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.UVNFrame_img.jpg)
