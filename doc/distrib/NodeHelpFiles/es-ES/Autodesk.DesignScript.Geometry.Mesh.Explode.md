## En detalle
El nodo `Mesh.Explode` utiliza una única malla y devuelve una lista de caras de malla como mallas independientes.

En el ejemplo siguiente, se muestra una cúpula de malla que se descompone mediante `Mesh.Explode`, seguida de un desplazamiento de cada cara en la dirección de la normal de cara. Esto se consigue utilizando los nodos `Mesh.TriangleNormals` y `Mesh.Translate`. Aunque, en este ejemplo, las caras de la malla parecen cuadriláteros, en realidad son triángulos con normales idénticas.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Explode_img.jpg)
