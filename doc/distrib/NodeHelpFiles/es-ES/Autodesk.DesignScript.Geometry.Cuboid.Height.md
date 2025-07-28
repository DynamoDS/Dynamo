## En detalle:
`Cuboid.Height` devuelve la altura del ortoedro de entrada. Tenga en cuenta que si el ortoedro se ha transformado en un sistema de coordenadas diferente con un factor de escala, devolverá las cotas originales del ortoedro, no las cotas del espacio universal. En otras palabras, si crea un ortoedro con una anchura (eje X) de 10 y lo transforma en un CoordinateSystem con una escala de 2 veces en X, la anchura seguirá siendo 10.

En el ejemplo siguiente, generamos un ortoedro por esquinas y, a continuación, utilizamos el nodo `Cuboid.Height` para buscar su altura.

___
## Archivo de ejemplo

![Height](./Autodesk.DesignScript.Geometry.Cuboid.Height_img.jpg)

