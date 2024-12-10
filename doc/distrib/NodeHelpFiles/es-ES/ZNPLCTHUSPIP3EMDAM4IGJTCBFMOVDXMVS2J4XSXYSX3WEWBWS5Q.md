<!--- Autodesk.DesignScript.Geometry.Curve.CoordinateSystemAtSegmentLength --->
<!--- ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q --->
## En detalle:
CoordinateSystemAtSegmentLength devolverá un sistema de coordenadas alineado con la curva de entrada en la longitud de curva especificada, medida desde el punto inicial de la curva. El sistema de coordenadas resultante tendrá su eje X en la dirección de la normal de la curva y el eje Y en la dirección de la tangente de la curva en la longitud especificada. En el siguiente ejemplo, se crea primero una curva NURBS mediante un nodo ByControlPoints con un conjunto de puntos generados aleatoriamente como entrada. Se utiliza un control deslizante de número para controlar la entrada de longitud de segmento para un nodo CoordinateSystemAtParameter. Si la longitud especificada es mayor que la longitud de la curva, este nodo devolverá un sistema de coordenadas en el punto final de la curva.
___
## Archivo de ejemplo

![CoordinateSystemAtSegmentLength](./ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q_img.jpg)

