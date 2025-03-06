<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction) --->
<!--- 32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA --->
## En detalle:
`Curve.ExtrudeAsSolid (curve, direction)` extruye una curva plana cerrada de entrada mediante un vector de entrada para determinar la dirección de la extrusión. La longitud del vector se utiliza para la distancia de extrusión. Este nodo tapa los extremos de la extrusión para crear un sólido.

En el ejemplo siguiente, creamos primero una NurbsCurve mediante un nodo `NurbsCurve.ByPoints` con un conjunto de puntos generados aleatoriamente como entrada. Se utiliza un bloque de código para especificar los componentes X, Y y Z del nodo `Vector.ByCoordinates`. Este vector se utiliza como entrada de dirección en el nodo `Curve.ExtrudeAsSolid`.
___
## Archivo de ejemplo

![Curve.ExtrudeAsSolid(curve, direction)](./32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA_img.jpg)
