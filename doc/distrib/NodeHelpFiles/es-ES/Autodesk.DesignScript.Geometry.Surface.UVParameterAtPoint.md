## En detalle:
UVParameterAtPoint busca la posición UV de la superficie en el punto de entrada de una superficie. Si el punto de entrada no se encuentra en la superficie, este nodo buscará el punto en la superficie más cercana al punto de entrada. En el siguiente ejemplo, se crea primero una superficie mediante BySweep2Rails. A continuación, se utiliza un bloque de código para especificar un punto en el que buscar el parámetro UV. El punto no se encuentra en la superficie, por lo que el nodo utiliza el punto más cercano en la superficie como la posición para buscar el parámetro UV.
___
## Archivo de ejemplo

![UVParameterAtPoint](./Autodesk.DesignScript.Geometry.Surface.UVParameterAtPoint_img.jpg)

