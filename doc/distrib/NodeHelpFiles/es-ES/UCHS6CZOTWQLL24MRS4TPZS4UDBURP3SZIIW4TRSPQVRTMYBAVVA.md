<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseVertices --->
<!--- UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA --->
## In-Depth
En el ejemplo siguiente, el nodo `TSplineSurface.UncreaseVertices` se utiliza en los vértices de las esquinas de una primitiva plana. Por defecto, estos vértices se pliegan en el momento en que se crea la superficie. Los vértices se identifican con ayuda de los nodos `TSplineVertex.UVNFrame` y `TSplineUVNFrame.Poision`, con la opción `Mostrar etiquetas` activada. A continuación, los vértices de las esquinas se seleccionan con ayuda del nodo `TSplineTopology.VertexByIndex` y se quitan los pliegues. El efecto de esta acción puede previsualizarse si la forma se encuentra en el modo suavizado.

## Archivo de ejemplo

![Example](./UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA_img.jpg)
