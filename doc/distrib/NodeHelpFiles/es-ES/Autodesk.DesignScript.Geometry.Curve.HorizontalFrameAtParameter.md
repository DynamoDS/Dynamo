## En detalle:
HorizontalFrameAtParameter devolverá un sistema de coordenadas alineado con la curva de entrada en el parámetro especificado. La parametrización de una curva se mide en el rango de 0 a 1; 0 representa el inicio de la curva y 1, el final. El sistema de coordenadas resultante tendrá su eje Z en la dirección Z universal y el eje Y en la dirección de la tangente de la curva en el parámetro especificado. En el siguiente ejemplo, se crea primero una curva NURBS mediante un nodo ByControlPoints, con un conjunto de puntos generados aleatoriamente como entrada. Se utiliza un control deslizante de número establecido en el rango de 0 a 1 para ajustar la entrada de parámetros para un nodo HorizontalFrameAtParameter.
___
## Archivo de ejemplo

![HorizontalFrameAtParameter](./Autodesk.DesignScript.Geometry.Curve.HorizontalFrameAtParameter_img.jpg)

