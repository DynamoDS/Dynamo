## En detalle
`Mesh.Remesh` crea una nueva malla en la que los triángulos de un objeto especificado se redistribuyen de un modo más uniforme, independientemente de los cambios en las normales de los triángulos. Esta operación puede ser útil para mallas con una densidad variable de triángulos con el fin de preparar la malla para el análisis de resistencia. Volver a crear una malla repetidamente genera mallas progresivamente más uniformes. Para mallas cuyos vértices ya sean equidistantes (por ejemplo, una malla de icosfera), el resultado del nodo `Mesh.Remesh` será la misma malla.
En el ejemplo siguiente, se utiliza `Mesh.Remesh` en una malla importada con una alta densidad de triángulos en áreas con detalles finos. El resultado del nodo `Mesh.Remesh` se traslada hacia un lado y se utiliza `Mesh.Edges` para visualizar el resultado.

`(El archivo de ejemplo utilizado tiene licencia de Creative Commons)`

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Remesh_img.jpg)
