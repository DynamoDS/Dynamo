## En detalle:
Extend extenderá una curva de entrada según la distancia de entrada especificada. La entrada pickSide utiliza el punto inicial o el final de la curva como entrada y determina el extremo de la curva que se va a extender. En el siguiente ejemplo, se crea primero una curva NURBS mediante un nodo ByControlPoints con un conjunto de puntos generados aleatoriamente como entrada. Se usa el nodo de consulta Curve.EndPoint para encontrar el punto final de la curva a fin de utilizarlo como entrada pickSide. Un control deslizante de número permite ajustar la distancia de la extensión.
___
## Archivo de ejemplo

![Extend](./Autodesk.DesignScript.Geometry.Curve.Extend_img.jpg)

