## En detalle:
PlaneAtSegmentLength devolverá un plano alineado con una curva en un punto que es una distancia especificada a lo largo de la curva, medida desde el punto inicial. Si la longitud de entrada es mayor que la longitud total de la curva, este nodo utilizará el punto final de la curva. El vector normal del plano resultante se corresponderá con la tangente de la curva. En el siguiente ejemplo, se crea primero una curva NURBS mediante un nodo ByControlPoints, con un conjunto de puntos generados aleatoriamente como entrada. Se utiliza un control deslizante de número para ajustar la entrada del parámetro de un nodo PlaneAtSegmentLength.
___
## Archivo de ejemplo

![PlaneAtSegmentLength](./Autodesk.DesignScript.Geometry.Curve.PlaneAtSegmentLength_img.jpg)

