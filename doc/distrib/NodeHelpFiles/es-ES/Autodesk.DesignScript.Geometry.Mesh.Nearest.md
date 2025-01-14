## En detalle
`Mesh.Nearest` devuelve un punto de la malla de entrada que está más cerca del punto especificado. El punto devuelto es una proyección del punto de entrada sobre la malla usando el vector normal a la malla que pasa por el punto, lo que da como resultado el punto más cercano posible.

En el ejemplo siguiente, se crea un caso de uso sencillo para mostrar cómo funciona el nodo. El punto de entrada se encuentra sobre una malla esférica, pero no directamente encima. El punto resultante es el más cercano que se encuentra a la malla. Esto se contrasta con la salida del nodo `Mesh.Project` (utilizando el mismo punto y malla como entradas junto con un vector en la dirección Z negativa) donde el punto resultante se proyecta sobre la malla directamente debajo del punto de entrada. El nodo `Line.ByStartAndEndPoint` se utiliza para mostrar la trayectoria del punto proyectado sobre la malla.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Nearest_img.jpg)
