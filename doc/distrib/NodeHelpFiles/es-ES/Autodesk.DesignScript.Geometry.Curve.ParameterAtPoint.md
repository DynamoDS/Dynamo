## En detalle:
ParameterAtPoint devolverá el valor del parámetro de un punto especificado a lo largo de una curva. Si el punto de entrada no se encuentra en la curva, ParameterAtPoint devolverá el parámetro del punto en la curva que se cierre en el punto de entrada. En el siguiente ejemplo, se crea primero una curva NURBS mediante un nodo ByControlPoints con un conjunto de puntos generados aleatoriamente como entrada. Se crea un punto único adicional con un bloque de código para especificar las coordenadas X e Y. El nodo ParameterAtPoint devuelve el parámetro a lo largo de la curva en el punto más cercano al punto de entrada.
___
## Archivo de ejemplo

![ParameterAtPoint](./Autodesk.DesignScript.Geometry.Curve.ParameterAtPoint_img.jpg)

