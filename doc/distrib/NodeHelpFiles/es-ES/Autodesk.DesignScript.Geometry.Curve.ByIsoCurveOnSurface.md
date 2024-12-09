## En detalle:
Curve.ByIsoCurveOnSurface creará una curva que es la curva isométrica de una superficie especificando la dirección U o V, y el parámetro en la dirección opuesta en la que se creará la curva. La entrada direction determina la dirección de la isocurva que se creará. Un valor de uno se corresponde con la dirección U, mientras que un valor de cero se corresponde con la dirección V. En el siguiente ejemplo, se crea primero la rejilla de puntos y se traslada en la dirección Z por una cantidad aleatoria. Estos puntos se utilizan para crear superficies mediante un nodo NurbsSurface.ByPoints. Esta superficie se utiliza como la baseSurface de un nodo ByIsoCurveOnSurface. Se usa un control deslizante de número establecido en un rango de 0 a 1 y un paso de 1 para determinar si la isocurva se extrae en la dirección U o V. Un segundo control deslizante de número permite determinar el parámetro en el que se extrae la isocurva.
___
## Archivo de ejemplo

![ByIsoCurveOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByIsoCurveOnSurface_img.jpg)

