## En detalle:
NormalAtPoint busca el vector normal de la superficie en el punto de entrada de una superficie. Si el punto de entrada no se encuentra en la superficie, este nodo buscará el punto en la superficie más cercana al punto de entrada. En el siguiente ejemplo, se crea primero una superficie mediante BySweep2Rails. A continuación, se utiliza un bloque de código para especificar un punto en el que buscar la normal. El punto no se encuentra en la superficie, por lo que el nodo utiliza el punto más cercano en la superficie como la posición en la que buscar la normal.
___
## Archivo de ejemplo

![NormalAtPoint](./Autodesk.DesignScript.Geometry.Surface.NormalAtPoint_img.jpg)

