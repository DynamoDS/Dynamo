## En detalle
`Mesh.CloseCracks` cierra las grietas de una malla mediante la eliminación de los contornos internos de un objeto de malla. Los contornos internos pueden surgir de forma natural como resultado de las operaciones de modelado de malla. Los triángulos pueden suprimirse en esta operación si se eliminan las aristas degeneradas. En el siguiente ejemplo, se utiliza `Mesh.CloseCracks` en una malla importada. Se usa `Mesh.VertexNormals` para visualizar los vértices superpuestos. Después de pasar la malla original por `Mesh.CloseCracks`, el número de aristas se reduce, lo que también es evidente al comparar el recuento de aristas mediante un nodo `Mesh.EdgeCount`.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.CloseCracks_img.jpg)
