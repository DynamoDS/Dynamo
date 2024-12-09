## En detalle
El nodo `Mesh.GenerateSupport` se utiliza para añadir soportes a la geometría de la malla de entrada con el fin de prepararla para la impresión en 3D. Los soportes son necesarios para imprimir correctamente la geometría con salientes a fin de garantizar la correcta adhesión de las capas y evitar que el material se pandee durante el proceso de impresión. `Mesh.GenerateSupport` detecta los salientes y genera automáticamente soportes de tipo árbol que consumen menos material y pueden retirarse más fácilmente al tener menos contacto con la superficie impresa. En los casos en los que no se detectan salientes, el resultado del nodo `Mesh.GenerateSupport` es la misma malla, girada y con una orientación óptima para la impresión y trasladada al plano XY. La configuración de los soportes se controla mediante las siguientes entradas:
- `baseHeight`: define el grosor de la parte más baja del soporte, su base.
- `baseDiameter` controla el tamaño de la base del soporte.
- La entrada `postDiameter` controla el tamaño de cada soporte en su centro.
- `tipHeight`y `tipDiameter`controlan el tamaño de los soportes en su punta en contacto con la superficie impresa.
En el ejemplo siguiente, se utiliza el nodo `Mesh.GenerateSupport` para añadir soportes a una malla en forma de T.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.GenerateSupport_img.jpg)
