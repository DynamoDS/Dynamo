## En detalle
`Mesh.Project` devuelve un punto en la malla de entrada que es una proyección del punto de entrada en la malla en la dirección del vector especificado. Para que el nodo funcione correctamente, una línea dibujada desde el punto de entrada en la dirección del vector de entrada debe intersecarse con la malla proporcionada.

En el gráfico de ejemplo, se muestra un caso de uso sencillo de cómo funciona el nodo. El punto de entrada se encuentra sobre una malla esférica, pero no directamente encima. El punto se proyecta en la dirección del vector negativo del eje Z. El punto resultante se proyecta sobre la esfera y aparece justo debajo del punto de entrada. Esto contrasta con la salida del nodo `Mesh.Nearest` (utilizando el mismo punto y malla como entradas) donde el punto resultante se encuentra sobre la malla a lo largo del vector normal que pasa por el punto de entrada (el punto más cercano). El nodo `Line.ByStartAndEndPoint` se utiliza para mostrar la trayectoria del punto proyectado sobre la malla.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Project_img.jpg)
