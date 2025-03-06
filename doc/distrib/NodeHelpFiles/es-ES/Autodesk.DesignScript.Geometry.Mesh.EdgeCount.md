## En detalle
Este nodo cuenta el número de aristas de una malla proporcionada. Si la malla está formada por triángulos, que es el caso de todas las mallas de `MeshToolkit`, el nodo `Mesh.EdgeCount` solo devuelve aristas únicas. Como resultado, cabe esperar que el número de aristas no sea el triple del número de triángulos de la malla. Esta suposición puede utilizarse para verificar que la malla no contiene caras no soldadas (lo que puede ocurrir en mallas importadas).

En el ejemplo siguiente, `Mesh.Cone` y `Number.Slider` se utilizan para crear un cono, que se usa a continuación como entrada para contar las aristas. Tanto `Mesh.Edges` como `Mesh.Triangles` se pueden utilizar para previsualizar la estructura y la rejilla de una malla en la vista preliminar; `Mesh.Edges` muestra un mejor rendimiento en mallas complejas y pesadas.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgeCount_img.jpg)
